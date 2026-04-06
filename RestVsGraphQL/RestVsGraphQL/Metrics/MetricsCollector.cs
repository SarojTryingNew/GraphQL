using System.Collections.Concurrent;
using System.Diagnostics;

namespace RestVsGraphQL.Metrics;

public class MetricsCollector
{
    private readonly ConcurrentBag<ApiMetric> _metrics = new();
    private readonly ConcurrentDictionary<string, int> _requestCounts = new();
    private readonly ConcurrentDictionary<string, int> _errorCounts = new();
    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private string _currentTestScenario = "General";
    private DateTime _testStartTime = DateTime.UtcNow;

    public void SetTestScenario(string scenarioName)
    {
        _currentTestScenario = scenarioName;
        _testStartTime = DateTime.UtcNow;
    }

    public string GetCurrentTestScenario() => _currentTestScenario;

    public void RecordRequest(ApiMetric metric)
    {
        _metrics.Add(metric);
        _requestCounts.AddOrUpdate(metric.Endpoint, 1, (_, count) => count + 1);
        
        if (!metric.Success)
        {
            _errorCounts.AddOrUpdate(metric.Endpoint, 1, (_, count) => count + 1);
        }
    }

    public MetricsSummary GetSummary(ApiType? filterByType = null)
    {
        // Filter out /api/metrics endpoints and apply API type filter
        var filteredMetrics = _metrics
            .Where(m => !m.Endpoint.StartsWith("/api/metrics", StringComparison.OrdinalIgnoreCase))
            .Where(m => !filterByType.HasValue || m.ApiType == filterByType.Value)
            .ToList();

        if (!filteredMetrics.Any())
            return new MetricsSummary();

        var responseTimes = filteredMetrics.Select(m => m.ResponseTimeMs).OrderBy(x => x).ToList();
        var payloadSizes = filteredMetrics.Select(m => m.ResponseSizeBytes).ToList();
        var memoryUsages = filteredMetrics.Select(m => m.MemoryUsedBytes).ToList();

        return new MetricsSummary
        {
            TotalRequests = filteredMetrics.Count,
            SuccessfulRequests = filteredMetrics.Count(m => m.Success),
            FailedRequests = filteredMetrics.Count(m => !m.Success),
            SuccessRate = filteredMetrics.Count > 0 
                ? (double)filteredMetrics.Count(m => m.Success) / filteredMetrics.Count * 100 
                : 0,

            // Response Time Statistics
            AverageResponseTimeMs = responseTimes.Average(),
            MinResponseTimeMs = responseTimes.Min(),
            MaxResponseTimeMs = responseTimes.Max(),
            P50ResponseTimeMs = GetPercentile(responseTimes, 50),
            P95ResponseTimeMs = GetPercentile(responseTimes, 95),
            P99ResponseTimeMs = GetPercentile(responseTimes, 99),

            // Payload Statistics
            AverageResponseSizeBytes = payloadSizes.Average(),
            MinResponseSizeBytes = payloadSizes.Min(),
            MaxResponseSizeBytes = payloadSizes.Max(),
            TotalBandwidthBytes = payloadSizes.Sum(),

            // Memory Statistics
            AverageMemoryUsedBytes = memoryUsages.Average(),
            MinMemoryUsedBytes = memoryUsages.Min(),
            MaxMemoryUsedBytes = memoryUsages.Max(),
            TotalMemoryUsedBytes = memoryUsages.Sum(),

            // Throughput
            UptimeSeconds = _uptime.Elapsed.TotalSeconds,
            RequestsPerSecond = filteredMetrics.Count / _uptime.Elapsed.TotalSeconds,
            
            // Endpoint Breakdown
            EndpointMetrics = filteredMetrics
                .GroupBy(m => m.Endpoint)
                .Select(g => new EndpointMetric
                {
                    Endpoint = g.Key,
                    TotalCalls = g.Count(),
                    AverageResponseTime = g.Average(m => m.ResponseTimeMs),
                    AveragePayloadSize = g.Average(m => m.ResponseSizeBytes),
                    SuccessRate = (double)g.Count(m => m.Success) / g.Count() * 100
                })
                .OrderByDescending(e => e.TotalCalls)
                .ToList()
        };
    }

    public ComparisonReport GetComparisonReport()
    {
        var restMetrics = GetSummary(ApiType.REST);
        var graphqlMetrics = GetSummary(ApiType.GraphQL);

        return new ComparisonReport
        {
            RestMetrics = restMetrics,
            GraphQLMetrics = graphqlMetrics,
            GeneratedAt = DateTime.UtcNow,
            TestScenario = _currentTestScenario,
            TestStartTime = _testStartTime,
            
            // Comparative Analysis
            ResponseTimeWinner = restMetrics.AverageResponseTimeMs < graphqlMetrics.AverageResponseTimeMs 
                ? ApiType.REST : ApiType.GraphQL,
            PayloadSizeWinner = restMetrics.AverageResponseSizeBytes < graphqlMetrics.AverageResponseSizeBytes 
                ? ApiType.REST : ApiType.GraphQL,
            ThroughputWinner = restMetrics.RequestsPerSecond > graphqlMetrics.RequestsPerSecond 
                ? ApiType.REST : ApiType.GraphQL,
            ReliabilityWinner = restMetrics.SuccessRate > graphqlMetrics.SuccessRate 
                ? ApiType.REST : ApiType.GraphQL,
            
            // Performance Improvements
            ResponseTimeImprovement = CalculateImprovement(
                restMetrics.AverageResponseTimeMs, 
                graphqlMetrics.AverageResponseTimeMs),
            PayloadSizeImprovement = CalculateImprovement(
                restMetrics.AverageResponseSizeBytes, 
                graphqlMetrics.AverageResponseSizeBytes),
            ThroughputImprovement = CalculateImprovement(
                graphqlMetrics.RequestsPerSecond, 
                restMetrics.RequestsPerSecond)
        };
    }

    private double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (!sortedValues.Any()) return 0;
        
        var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }

    private double CalculateImprovement(double baseline, double comparison)
    {
        if (baseline == 0) return 0;
        return ((baseline - comparison) / baseline) * 100;
    }

    public void Reset()
    {
        _metrics.Clear();
        _requestCounts.Clear();
        _errorCounts.Clear();
    }
}

public class ApiMetric
{
    public ApiType ApiType { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double ResponseTimeMs { get; set; }
    public long ResponseSizeBytes { get; set; }
    public long RequestSizeBytes { get; set; }
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long MemoryUsedBytes { get; set; }
}

public class MetricsSummary
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRate { get; set; }
    
    public double AverageResponseTimeMs { get; set; }
    public double MinResponseTimeMs { get; set; }
    public double MaxResponseTimeMs { get; set; }
    public double P50ResponseTimeMs { get; set; }
    public double P95ResponseTimeMs { get; set; }
    public double P99ResponseTimeMs { get; set; }
    
    public double AverageResponseSizeBytes { get; set; }
    public long MinResponseSizeBytes { get; set; }
    public long MaxResponseSizeBytes { get; set; }
    public long TotalBandwidthBytes { get; set; }

    public double AverageMemoryUsedBytes { get; set; }
    public long MinMemoryUsedBytes { get; set; }
    public long MaxMemoryUsedBytes { get; set; }
    public long TotalMemoryUsedBytes { get; set; }

    public double UptimeSeconds { get; set; }
    public double RequestsPerSecond { get; set; }
    
    public List<EndpointMetric> EndpointMetrics { get; set; } = new();
}

public class EndpointMetric
{
    public string Endpoint { get; set; } = string.Empty;
    public int TotalCalls { get; set; }
    public double AverageResponseTime { get; set; }
    public double AveragePayloadSize { get; set; }
    public double SuccessRate { get; set; }
}

public class ComparisonReport
{
    public DateTime GeneratedAt { get; set; }
    public string TestScenario { get; set; } = "General";
    public DateTime TestStartTime { get; set; }
    public MetricsSummary RestMetrics { get; set; } = new();
    public MetricsSummary GraphQLMetrics { get; set; } = new();
    
    public ApiType ResponseTimeWinner { get; set; }
    public ApiType PayloadSizeWinner { get; set; }
    public ApiType ThroughputWinner { get; set; }
    public ApiType ReliabilityWinner { get; set; }
    public ApiType MemoryEfficiencyWinner { get; set; }

    public double ResponseTimeImprovement { get; set; }
    public double PayloadSizeImprovement { get; set; }
    public double ThroughputImprovement { get; set; }
    public double MemoryEfficiencyImprovement { get; set; }
}

public enum ApiType
{
    REST,
    GraphQL
}
