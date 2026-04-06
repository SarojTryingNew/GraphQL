using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Metrics;
using System.Text;

namespace RestVsGraphQL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly MetricsCollector _metricsCollector;

    public MetricsController(MetricsCollector metricsCollector)
    {
        _metricsCollector = metricsCollector;
    }

    [HttpGet("summary")]
    public ActionResult<object> GetSummary()
    {
        var restMetrics = _metricsCollector.GetSummary(ApiType.REST);
        var graphqlMetrics = _metricsCollector.GetSummary(ApiType.GraphQL);

        return Ok(new
        {
            REST = restMetrics,
            GraphQL = graphqlMetrics
        });
    }

    [HttpGet("comparison")]
    public ActionResult<ComparisonReport> GetComparison()
    {
        return Ok(_metricsCollector.GetComparisonReport());
    }

    [HttpGet("rest")]
    public ActionResult<MetricsSummary> GetRestMetrics()
    {
        return Ok(_metricsCollector.GetSummary(ApiType.REST));
    }

    [HttpGet("graphql")]
    public ActionResult<MetricsSummary> GetGraphQLMetrics()
    {
        return Ok(_metricsCollector.GetSummary(ApiType.GraphQL));
    }

    [HttpGet("report")]
    public ActionResult GetDetailedReport()
    {
        var report = _metricsCollector.GetComparisonReport();
        var html = GenerateHtmlReport(report);
        return Content(html, "text/html");
    }

    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _metricsCollector.Reset();
        return Ok(new { message = "Metrics reset successfully" });
    }

    [HttpPost("scenario")]
    public IActionResult SetTestScenario([FromBody] TestScenarioRequest request)
    {
        _metricsCollector.SetTestScenario(request.ScenarioName);
        return Ok(new { message = $"Test scenario set to: {request.ScenarioName}" });
    }

    [HttpGet("scenario")]
    public IActionResult GetCurrentScenario()
    {
        return Ok(new { scenario = _metricsCollector.GetCurrentTestScenario() });
    }

    private string GenerateHtmlReport(ComparisonReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<title>REST vs GraphQL - KPI & NFR Comparison Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background: #f5f5f5; }
            .container { max-width: 1400px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
            h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
            h2 { color: #34495e; margin-top: 30px; border-left: 4px solid #3498db; padding-left: 15px; }
            .metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; margin: 20px 0; }
            .metric-card { background: #ecf0f1; padding: 20px; border-radius: 8px; border-left: 4px solid #3498db; }
            .metric-card.rest { border-left-color: #e74c3c; }
            .metric-card.graphql { border-left-color: #e91e63; }
            .metric-card.winner { background: #d5f4e6; border-left-color: #27ae60; }
            .metric-title { font-size: 14px; color: #7f8c8d; text-transform: uppercase; margin-bottom: 5px; }
            .metric-value { font-size: 32px; font-weight: bold; color: #2c3e50; }
            .metric-unit { font-size: 16px; color: #95a5a6; margin-left: 5px; }
            table { width: 100%; border-collapse: collapse; margin: 20px 0; }
            th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ecf0f1; }
            th { background: #34495e; color: white; font-weight: 600; }
            tr:hover { background: #f8f9fa; }
            .badge { display: inline-block; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: bold; }
            .badge.rest { background: #e74c3c; color: white; }
            .badge.graphql { background: #e91e63; color: white; }
            .badge.winner { background: #27ae60; color: white; }
            .improvement { color: #27ae60; font-weight: bold; }
            .degradation { color: #e74c3c; font-weight: bold; }
            .summary-box { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 10px; margin: 20px 0; }
            .summary-box h3 { margin-top: 0; }
        ");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<div class='container'>");
        
        sb.AppendLine($"<h1>REST vs GraphQL - KPI & NFR Comparison Report</h1>");
        sb.AppendLine($"<p><strong>Test Scenario:</strong> {report.TestScenario}</p>");
        sb.AppendLine($"<p><strong>Test Started:</strong> {report.TestStartTime:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine($"<p><strong>Report Generated:</strong> {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");

        // Executive Summary
        sb.AppendLine("<div class='summary-box'>");
        sb.AppendLine("<h3>Executive Summary</h3>");

        // Response Time Winner
        var responseTimeWinner = Math.Abs(report.ResponseTimeImprovement) < 0.01 
            ? "REST & GraphQL (Tie)" 
            : $"{report.ResponseTimeWinner} ({Math.Abs(report.ResponseTimeImprovement):F2}% {(report.ResponseTimeImprovement > 0 ? "faster" : "slower")})";
        sb.AppendLine($"<p><strong>Response Time Winner:</strong> {responseTimeWinner}</p>");

        // Payload Size Winner
        var payloadSizeWinner = Math.Abs(report.PayloadSizeImprovement) < 0.01 
            ? "REST & GraphQL (Tie)" 
            : $"{report.PayloadSizeWinner} ({Math.Abs(report.PayloadSizeImprovement):F2}% {(report.PayloadSizeImprovement > 0 ? "smaller" : "larger")})";
        sb.AppendLine($"<p><strong>Payload Size Winner:</strong> {payloadSizeWinner}</p>");

        // Memory Efficiency Winner
        var memoryWinner = Math.Abs(report.MemoryEfficiencyImprovement) < 0.01 
            ? "REST & GraphQL (Tie)" 
            : $"{report.MemoryEfficiencyWinner} ({Math.Abs(report.MemoryEfficiencyImprovement):F2}% {(report.MemoryEfficiencyImprovement > 0 ? "less memory" : "more memory")})";
        sb.AppendLine($"<p><strong>Memory Efficiency Winner:</strong> {memoryWinner}</p>");

        // Throughput Winner
        var throughputWinner = Math.Abs(report.ThroughputImprovement) < 0.01 
            ? "REST & GraphQL (Tie)" 
            : $"{report.ThroughputWinner} ({Math.Abs(report.ThroughputImprovement):F2}% {(report.ThroughputImprovement > 0 ? "higher" : "lower")})";
        sb.AppendLine($"<p><strong>Throughput Winner:</strong> {throughputWinner}</p>");

        // Reliability Winner
        var reliabilityWinner = report.RestMetrics.SuccessRate == report.GraphQLMetrics.SuccessRate 
            ? "REST & GraphQL (Tie)" 
            : report.ReliabilityWinner.ToString();
        sb.AppendLine($"<p><strong>Reliability Winner:</strong> {reliabilityWinner}</p>");

        sb.AppendLine("</div>");

        // KPI Comparison
        sb.AppendLine("<h2>Key Performance Indicators (KPIs)</h2>");
        sb.AppendLine("<div class='metrics-grid'>");
        
        AddMetricCard(sb, "Total Requests", report.RestMetrics.TotalRequests.ToString(), "REST", "rest");
        AddMetricCard(sb, "Total Requests", report.GraphQLMetrics.TotalRequests.ToString(), "GraphQL", "graphql");
        
        AddMetricCard(sb, "Success Rate", $"{report.RestMetrics.SuccessRate:F2}", "REST", report.ReliabilityWinner == ApiType.REST ? "winner" : "rest", "%");
        AddMetricCard(sb, "Success Rate", $"{report.GraphQLMetrics.SuccessRate:F2}", "GraphQL", report.ReliabilityWinner == ApiType.GraphQL ? "winner" : "graphql", "%");
        
        AddMetricCard(sb, "Avg Response Time", $"{report.RestMetrics.AverageResponseTimeMs:F2}", "REST", report.ResponseTimeWinner == ApiType.REST ? "winner" : "rest", "ms");
        AddMetricCard(sb, "Avg Response Time", $"{report.GraphQLMetrics.AverageResponseTimeMs:F2}", "GraphQL", report.ResponseTimeWinner == ApiType.GraphQL ? "winner" : "graphql", "ms");
        
        AddMetricCard(sb, "Throughput", $"{report.RestMetrics.RequestsPerSecond:F2}", "REST", report.ThroughputWinner == ApiType.REST ? "winner" : "rest", "req/s");
        AddMetricCard(sb, "Throughput", $"{report.GraphQLMetrics.RequestsPerSecond:F2}", "GraphQL", report.ThroughputWinner == ApiType.GraphQL ? "winner" : "graphql", "req/s");
        
        sb.AppendLine("</div>");

        // Response Time Details
        sb.AppendLine("<h2>Response Time Analysis (NFR: Performance)</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Metric</th><th>REST</th><th>GraphQL</th><th>Winner</th></tr>");
        AddTableRow(sb, "Average", $"{report.RestMetrics.AverageResponseTimeMs:F2} ms", $"{report.GraphQLMetrics.AverageResponseTimeMs:F2} ms", report.ResponseTimeWinner.ToString());
        AddTableRow(sb, "P50 (Median)", $"{report.RestMetrics.P50ResponseTimeMs:F2} ms", $"{report.GraphQLMetrics.P50ResponseTimeMs:F2} ms", report.RestMetrics.P50ResponseTimeMs < report.GraphQLMetrics.P50ResponseTimeMs ? "REST" : "GraphQL");
        AddTableRow(sb, "P95", $"{report.RestMetrics.P95ResponseTimeMs:F2} ms", $"{report.GraphQLMetrics.P95ResponseTimeMs:F2} ms", report.RestMetrics.P95ResponseTimeMs < report.GraphQLMetrics.P95ResponseTimeMs ? "REST" : "GraphQL");
        AddTableRow(sb, "P99", $"{report.RestMetrics.P99ResponseTimeMs:F2} ms", $"{report.GraphQLMetrics.P99ResponseTimeMs:F2} ms", report.RestMetrics.P99ResponseTimeMs < report.GraphQLMetrics.P99ResponseTimeMs ? "REST" : "GraphQL");
        AddTableRow(sb, "Min", $"{report.RestMetrics.MinResponseTimeMs:F2} ms", $"{report.GraphQLMetrics.MinResponseTimeMs:F2} ms", report.RestMetrics.MinResponseTimeMs < report.GraphQLMetrics.MinResponseTimeMs ? "REST" : "GraphQL");
        AddTableRow(sb, "Max", $"{report.RestMetrics.MaxResponseTimeMs:F2} ms", $"{report.GraphQLMetrics.MaxResponseTimeMs:F2} ms", report.RestMetrics.MaxResponseTimeMs < report.GraphQLMetrics.MaxResponseTimeMs ? "REST" : "GraphQL");
        sb.AppendLine("</table>");

        // Bandwidth Efficiency
        sb.AppendLine("<h2>Bandwidth Efficiency (NFR: Efficiency)</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Metric</th><th>REST</th><th>GraphQL</th><th>Winner</th></tr>");
        AddTableRow(sb, "Avg Payload Size", $"{FormatBytes(report.RestMetrics.AverageResponseSizeBytes)}", $"{FormatBytes(report.GraphQLMetrics.AverageResponseSizeBytes)}", report.PayloadSizeWinner.ToString());
        AddTableRow(sb, "Min Payload", $"{FormatBytes(report.RestMetrics.MinResponseSizeBytes)}", $"{FormatBytes(report.GraphQLMetrics.MinResponseSizeBytes)}", report.RestMetrics.MinResponseSizeBytes < report.GraphQLMetrics.MinResponseSizeBytes ? "REST" : "GraphQL");
        AddTableRow(sb, "Max Payload", $"{FormatBytes(report.RestMetrics.MaxResponseSizeBytes)}", $"{FormatBytes(report.GraphQLMetrics.MaxResponseSizeBytes)}", report.RestMetrics.MaxResponseSizeBytes < report.GraphQLMetrics.MaxResponseSizeBytes ? "REST" : "GraphQL");
        AddTableRow(sb, "Total Bandwidth", $"{FormatBytes(report.RestMetrics.TotalBandwidthBytes)}", $"{FormatBytes(report.GraphQLMetrics.TotalBandwidthBytes)}", report.RestMetrics.TotalBandwidthBytes < report.GraphQLMetrics.TotalBandwidthBytes ? "REST" : "GraphQL");
        sb.AppendLine("</table>");

        // Memory Usage
        sb.AppendLine("<h2>Memory Usage (NFR: Resource Efficiency)</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Metric</th><th>REST</th><th>GraphQL</th><th>Winner</th></tr>");
        AddTableRow(sb, "Avg Memory per Request", $"{FormatBytes(report.RestMetrics.AverageMemoryUsedBytes)}", $"{FormatBytes(report.GraphQLMetrics.AverageMemoryUsedBytes)}", report.MemoryEfficiencyWinner.ToString());
        AddTableRow(sb, "Min Memory", $"{FormatBytes(report.RestMetrics.MinMemoryUsedBytes)}", $"{FormatBytes(report.GraphQLMetrics.MinMemoryUsedBytes)}", report.RestMetrics.MinMemoryUsedBytes < report.GraphQLMetrics.MinMemoryUsedBytes ? "REST" : "GraphQL");
        AddTableRow(sb, "Max Memory", $"{FormatBytes(report.RestMetrics.MaxMemoryUsedBytes)}", $"{FormatBytes(report.GraphQLMetrics.MaxMemoryUsedBytes)}", report.RestMetrics.MaxMemoryUsedBytes < report.GraphQLMetrics.MaxMemoryUsedBytes ? "REST" : "GraphQL");
        AddTableRow(sb, "Total Memory Used", $"{FormatBytes(report.RestMetrics.TotalMemoryUsedBytes)}", $"{FormatBytes(report.GraphQLMetrics.TotalMemoryUsedBytes)}", report.RestMetrics.TotalMemoryUsedBytes < report.GraphQLMetrics.TotalMemoryUsedBytes ? "REST" : "GraphQL");
        sb.AppendLine("</table>");

        // Endpoint Analysis
        if (report.RestMetrics.EndpointMetrics.Any())
        {
            sb.AppendLine("<h2>REST Endpoint Analysis</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Endpoint</th><th>Calls</th><th>Avg Response Time</th><th>Avg Payload</th><th>Success Rate</th></tr>");
            foreach (var endpoint in report.RestMetrics.EndpointMetrics)
            {
                sb.AppendLine($"<tr><td>{endpoint.Endpoint}</td><td>{endpoint.TotalCalls}</td><td>{endpoint.AverageResponseTime:F2} ms</td><td>{FormatBytes(endpoint.AveragePayloadSize)}</td><td>{endpoint.SuccessRate:F2}%</td></tr>");
            }
            sb.AppendLine("</table>");
        }

        if (report.GraphQLMetrics.EndpointMetrics.Any())
        {
            sb.AppendLine("<h2>GraphQL Query Analysis</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Endpoint</th><th>Calls</th><th>Avg Response Time</th><th>Avg Payload</th><th>Success Rate</th></tr>");
            foreach (var endpoint in report.GraphQLMetrics.EndpointMetrics)
            {
                sb.AppendLine($"<tr><td>{endpoint.Endpoint}</td><td>{endpoint.TotalCalls}</td><td>{endpoint.AverageResponseTime:F2} ms</td><td>{FormatBytes(endpoint.AveragePayloadSize)}</td><td>{endpoint.SuccessRate:F2}%</td></tr>");
            }
            sb.AppendLine("</table>");
        }

        // NFR Assessment
        sb.AppendLine("<h2>Non-Functional Requirements Assessment</h2>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>NFR Category</th><th>Metric</th><th>REST</th><th>GraphQL</th><th>Winner</th></tr>");
        sb.AppendLine("<tr><td rowspan='2'><strong>Performance</strong></td><td>Latency (Avg)</td><td>" + $"{report.RestMetrics.AverageResponseTimeMs:F2} ms" + "</td><td>" + $"{report.GraphQLMetrics.AverageResponseTimeMs:F2} ms" + "</td><td><span class='badge winner'>" + report.ResponseTimeWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td>Throughput</td><td>" + $"{report.RestMetrics.RequestsPerSecond:F2} req/s" + "</td><td>" + $"{report.GraphQLMetrics.RequestsPerSecond:F2} req/s" + "</td><td><span class='badge winner'>" + report.ThroughputWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td><strong>Reliability</strong></td><td>Success Rate</td><td>" + $"{report.RestMetrics.SuccessRate:F2}%" + "</td><td>" + $"{report.GraphQLMetrics.SuccessRate:F2}%" + "</td><td><span class='badge winner'>" + report.ReliabilityWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td rowspan='2'><strong>Efficiency</strong></td><td>Bandwidth Usage</td><td>" + FormatBytes(report.RestMetrics.AverageResponseSizeBytes) + "</td><td>" + FormatBytes(report.GraphQLMetrics.AverageResponseSizeBytes) + "</td><td><span class='badge winner'>" + report.PayloadSizeWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td>Memory Usage</td><td>" + FormatBytes(report.RestMetrics.AverageMemoryUsedBytes) + "</td><td>" + FormatBytes(report.GraphQLMetrics.AverageMemoryUsedBytes) + "</td><td><span class='badge winner'>" + report.MemoryEfficiencyWinner + "</span></td></tr>");
        sb.AppendLine("</table>");

        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }

    private void AddMetricCard(StringBuilder sb, string title, string value, string label, string cssClass, string unit = "")
    {
        sb.AppendLine($"<div class='metric-card {cssClass}'>");
        sb.AppendLine($"<div class='metric-title'>{title} - {label}</div>");
        sb.AppendLine($"<div class='metric-value'>{value}<span class='metric-unit'>{unit}</span></div>");
        sb.AppendLine("</div>");
    }

    private void AddTableRow(StringBuilder sb, string metric, string restValue, string graphqlValue, string winner)
    {
        sb.AppendLine($"<tr><td>{metric}</td><td>{restValue}</td><td>{graphqlValue}</td><td><span class='badge winner'>{winner}</span></td></tr>");
    }

    private string FormatBytes(double bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes /= 1024;
        }
        return $"{bytes:F2} {sizes[order]}";
    }
}

public class TestScenarioRequest
{
    public string ScenarioName { get; set; } = string.Empty;
}
