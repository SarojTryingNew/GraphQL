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

        // Fix: Calculate throughput based on actual request time span, not uptime
        var timeSpanSeconds = filteredMetrics.Count > 1
            ? (filteredMetrics.Max(m => m.Timestamp) - filteredMetrics.Min(m => m.Timestamp)).TotalSeconds
            : _uptime.Elapsed.TotalSeconds;

        // Ensure we don't divide by zero
        if (timeSpanSeconds < 0.001) timeSpanSeconds = 0.001;

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

            // Throughput - Fixed to use actual request time span
            UptimeSeconds = timeSpanSeconds,
            RequestsPerSecond = filteredMetrics.Count / timeSpanSeconds,

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
        // Get metrics for legacy REST and GraphQL (for backward compatibility)
        var restMetrics = GetSummary(ApiType.REST);
        var graphqlMetrics = GetSummary(ApiType.GraphQL);

        // Get metrics for new comparison: Direct REST vs REST with GraphQL
        var restDirectMetrics = GetSummary(ApiType.RESTDirect);
        var restWithGraphQLMetrics = GetSummary(ApiType.RESTWithGraphQL);

        // If new metrics exist, use them; otherwise fall back to legacy
        var restToCompare = restDirectMetrics.TotalRequests > 0 ? restDirectMetrics : restMetrics;
        var graphqlToCompare = restWithGraphQLMetrics.TotalRequests > 0 ? restWithGraphQLMetrics : graphqlMetrics;

        return new ComparisonReport
        {
            RestMetrics = restToCompare,
            GraphQLMetrics = graphqlToCompare,
            RESTDirectMetrics = restDirectMetrics,
            RESTWithGraphQLMetrics = restWithGraphQLMetrics,
            GeneratedAt = DateTime.UtcNow,
            TestScenario = _currentTestScenario,
            TestStartTime = _testStartTime,

            // Comparative Analysis
            ResponseTimeWinner = restToCompare.AverageResponseTimeMs < graphqlToCompare.AverageResponseTimeMs 
                ? ApiType.RESTDirect : ApiType.RESTWithGraphQL,
            PayloadSizeWinner = restToCompare.AverageResponseSizeBytes < graphqlToCompare.AverageResponseSizeBytes 
                ? ApiType.RESTDirect : ApiType.RESTWithGraphQL,
            ThroughputWinner = restToCompare.RequestsPerSecond > graphqlToCompare.RequestsPerSecond 
                ? ApiType.RESTDirect : ApiType.RESTWithGraphQL,
            ReliabilityWinner = restToCompare.SuccessRate > graphqlToCompare.SuccessRate 
                ? ApiType.RESTDirect : ApiType.RESTWithGraphQL,
            MemoryEfficiencyWinner = restToCompare.AverageMemoryUsedBytes < graphqlToCompare.AverageMemoryUsedBytes 
                ? ApiType.RESTDirect : ApiType.RESTWithGraphQL,

            // Performance Improvements
            ResponseTimeImprovement = CalculateImprovement(
                restToCompare.AverageResponseTimeMs, 
                graphqlToCompare.AverageResponseTimeMs),
            PayloadSizeImprovement = CalculateImprovement(
                restToCompare.AverageResponseSizeBytes, 
                graphqlToCompare.AverageResponseSizeBytes),
            ThroughputImprovement = CalculateImprovement(
                graphqlToCompare.RequestsPerSecond, 
                restToCompare.RequestsPerSecond),
            MemoryEfficiencyImprovement = CalculateImprovement(
                restToCompare.AverageMemoryUsedBytes, 
                graphqlToCompare.AverageMemoryUsedBytes)
        };
    }

    private double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (!sortedValues.Any()) return 0;

        // Calculate percentile index using nearest-rank method
        var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;

        // Clamp index to valid range
        index = Math.Clamp(index, 0, sortedValues.Count - 1);

        return sortedValues[index];
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

    public List<ApiMetric> GetCapturedExamples()
    {
        // Return ALL metrics (requests/responses), excluding only the metrics endpoints
        // GET requests typically have no request body, but we still want to show them
        return _metrics
            .Where(m => !m.Endpoint.StartsWith("/api/metrics", StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.ApiType)
            .ThenBy(m => m.Endpoint)
            .ThenBy(m => m.Timestamp)
            .ToList();
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

    // Request/Response capture (only stored for sample requests)
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
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
    public MetricsSummary RESTDirectMetrics { get; set; } = new();
    public MetricsSummary RESTWithGraphQLMetrics { get; set; } = new();

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
    REST,           // Legacy - kept for backward compatibility
    GraphQL,
    RESTDirect,     // REST API calling DataStore directly
    RESTWithGraphQL // REST API using GraphQL as internal layer
}
