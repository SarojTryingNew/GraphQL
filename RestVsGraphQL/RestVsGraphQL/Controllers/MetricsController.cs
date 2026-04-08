using Microsoft.AspNetCore.Mvc;
using RestVsGraphQL.Metrics;
using System.Text;
using System.Web;

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

    [HttpGet("examples")]
    public ActionResult GetRequestResponseExamples()
    {
        var examples = _metricsCollector.GetCapturedExamples();
        var html = GenerateExamplesHtmlPage(examples);
        return Content(html, "text/html");
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
            .metric-card { background: #ecf0f1; padding: 20px; border-radius: 8px; border-left: 4px solid #3498db; position: relative; }
            .metric-card.rest { border-left-color: #e74c3c; }
            .metric-card.graphql { border-left-color: #e91e63; }
            .metric-card.winner { background: #d5f4e6; border-left-color: #27ae60; }
            .metric-title { font-size: 14px; color: #7f8c8d; text-transform: uppercase; margin-bottom: 5px; }
            .metric-value { font-size: 32px; font-weight: bold; color: #2c3e50; }
            .metric-unit { font-size: 16px; color: #95a5a6; margin-left: 5px; }
            .metric-info { font-size: 12px; color: #7f8c8d; margin-top: 8px; font-style: italic; line-height: 1.4; }
            .info-icon { display: inline-block; width: 16px; height: 16px; background: #3498db; color: white; border-radius: 50%; text-align: center; line-height: 16px; font-size: 12px; cursor: help; margin-left: 5px; }
            .section-info { background: #e8f4f8; padding: 12px; border-radius: 6px; margin: 10px 0; border-left: 3px solid #3498db; font-size: 14px; color: #34495e; }
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
            .examples-btn { display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; margin: 20px 0; box-shadow: 0 4px 6px rgba(0,0,0,0.1); transition: transform 0.2s, box-shadow 0.2s; }
            .examples-btn:hover { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(0,0,0,0.15); }
            .examples-btn::before { content: ''; }
            .clear-btn { padding: 12px 24px; background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); color: white; border: none; border-radius: 8px; font-size: 14px; font-weight: bold; cursor: pointer; box-shadow: 0 4px 6px rgba(0,0,0,0.1); transition: transform 0.2s, box-shadow 0.2s; }
            .clear-btn:hover { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(0,0,0,0.15); background: linear-gradient(135deg, #c0392b 0%, #a93226 100%); }
            .header-controls { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
        ");
        sb.AppendLine("</style>");
        sb.AppendLine("<script>");
        sb.AppendLine(@"
            async function clearMetrics() {
                if (!confirm('Are you sure you want to clear all metrics and reset the data? This action cannot be undone.')) {
                    return;
                }

                try {
                    const response = await fetch('/api/metrics/reset', { method: 'POST' });

                    if (response.ok) {
                        alert('✅ Metrics cleared successfully! The page will now reload.');
                        location.reload();
                    } else {
                        const error = await response.text();
                        alert('❌ Error clearing metrics: ' + error);
                    }
                } catch (error) {
                    alert('❌ Network error clearing metrics: ' + error.message);
                }
            }
        ");
        sb.AppendLine("</script>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<div class='container'>");

        sb.AppendLine("<div class='header-controls'>");
        sb.AppendLine("<div>");
        sb.AppendLine($"<h1 style='margin: 0;'>REST vs GraphQL - KPI & NFR Comparison Report</h1>");
        sb.AppendLine($"<p style='margin: 5px 0;'><strong>Test Scenario:</strong> {report.TestScenario}</p>");
        sb.AppendLine($"<p style='margin: 5px 0;'><strong>Test Started:</strong> {report.TestStartTime:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine($"<p style='margin: 5px 0;'><strong>Report Generated:</strong> {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div>");
        sb.AppendLine("<button class='clear-btn' onclick='clearMetrics()'>🗑️ Clear Metrics & Reset</button>");
        sb.AppendLine("<p style='color: #7f8c8d; font-size: 12px; margin-top: 8px; text-align: right;'>Reset all metrics and start fresh testing</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");

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
        sb.AppendLine("<div class='section-info'><strong>What this means:</strong> KPIs provide a high-level overview of API performance. Total requests show workload volume, success rate indicates reliability, response time measures speed, and throughput shows capacity under load.</div>");
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
        sb.AppendLine("<div class='section-info'><strong>What this means:</strong> Response time metrics show how quickly your API responds. Lower values are better. P50/P95/P99 percentiles reveal user experience - P95 means 95% of users get responses faster than this value. High P99 values may indicate occasional slowdowns that affect user experience.</div>");
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
        sb.AppendLine("<div class='section-info'><strong>What this means:</strong> Payload size affects network bandwidth costs and mobile data usage. Smaller payloads mean faster downloads and lower costs. GraphQL typically has smaller payloads due to field selection, while REST may over-fetch data. Monitor this if bandwidth costs or mobile performance are concerns.</div>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>Metric</th><th>REST</th><th>GraphQL</th><th>Winner</th></tr>");
        AddTableRow(sb, "Avg Payload Size", $"{FormatBytes(report.RestMetrics.AverageResponseSizeBytes)}", $"{FormatBytes(report.GraphQLMetrics.AverageResponseSizeBytes)}", report.PayloadSizeWinner.ToString());
        AddTableRow(sb, "Min Payload", $"{FormatBytes(report.RestMetrics.MinResponseSizeBytes)}", $"{FormatBytes(report.GraphQLMetrics.MinResponseSizeBytes)}", report.RestMetrics.MinResponseSizeBytes < report.GraphQLMetrics.MinResponseSizeBytes ? "REST" : "GraphQL");
        AddTableRow(sb, "Max Payload", $"{FormatBytes(report.RestMetrics.MaxResponseSizeBytes)}", $"{FormatBytes(report.GraphQLMetrics.MaxResponseSizeBytes)}", report.RestMetrics.MaxResponseSizeBytes < report.GraphQLMetrics.MaxResponseSizeBytes ? "REST" : "GraphQL");
        AddTableRow(sb, "Total Bandwidth", $"{FormatBytes(report.RestMetrics.TotalBandwidthBytes)}", $"{FormatBytes(report.GraphQLMetrics.TotalBandwidthBytes)}", report.RestMetrics.TotalBandwidthBytes < report.GraphQLMetrics.TotalBandwidthBytes ? "REST" : "GraphQL");
        sb.AppendLine("</table>");

        // Memory Usage
        sb.AppendLine("<h2>Memory Usage (NFR: Resource Efficiency)</h2>");
        sb.AppendLine("<div class='section-info'><strong>What this means:</strong> Memory usage impacts server costs and scalability. Lower memory consumption allows more concurrent requests per server. High memory usage can cause garbage collection pressure and increased hosting costs. This is critical for cloud deployments where you pay per resource.</div>");
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
        sb.AppendLine("<div class='section-info'><strong>What this means:</strong> This summary compares REST vs GraphQL across key non-functional requirements. <strong>Performance</strong> affects user experience, <strong>Reliability</strong> ensures consistent service, and <strong>Efficiency</strong> impacts operational costs. Use this to make informed architectural decisions based on your priorities.</div>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr><th>NFR Category</th><th>Metric</th><th>REST</th><th>GraphQL</th><th>Winner</th></tr>");
        sb.AppendLine("<tr><td rowspan='2'><strong>Performance</strong></td><td>Latency (Avg)</td><td>" + $"{report.RestMetrics.AverageResponseTimeMs:F2} ms" + "</td><td>" + $"{report.GraphQLMetrics.AverageResponseTimeMs:F2} ms" + "</td><td><span class='badge winner'>" + report.ResponseTimeWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td>Throughput</td><td>" + $"{report.RestMetrics.RequestsPerSecond:F2} req/s" + "</td><td>" + $"{report.GraphQLMetrics.RequestsPerSecond:F2} req/s" + "</td><td><span class='badge winner'>" + report.ThroughputWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td><strong>Reliability</strong></td><td>Success Rate</td><td>" + $"{report.RestMetrics.SuccessRate:F2}%" + "</td><td>" + $"{report.GraphQLMetrics.SuccessRate:F2}%" + "</td><td><span class='badge winner'>" + report.ReliabilityWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td rowspan='2'><strong>Efficiency</strong></td><td>Bandwidth Usage</td><td>" + FormatBytes(report.RestMetrics.AverageResponseSizeBytes) + "</td><td>" + FormatBytes(report.GraphQLMetrics.AverageResponseSizeBytes) + "</td><td><span class='badge winner'>" + report.PayloadSizeWinner + "</span></td></tr>");
        sb.AppendLine("<tr><td>Memory Usage</td><td>" + FormatBytes(report.RestMetrics.AverageMemoryUsedBytes) + "</td><td>" + FormatBytes(report.GraphQLMetrics.AverageMemoryUsedBytes) + "</td><td><span class='badge winner'>" + report.MemoryEfficiencyWinner + "</span></td></tr>");
        sb.AppendLine("</table>");

        // Add examples button at bottom
        sb.AppendLine("<div style='text-align: center; margin: 40px 0;'>");
        sb.AppendLine("<a href='/api/metrics/examples' class='examples-btn'>View Request/Response Examples</a>");
        sb.AppendLine("<p style='color: #7f8c8d; font-size: 14px; margin-top: 10px;'>See actual REST vs GraphQL requests and responses captured during this test</p>");
        sb.AppendLine("</div>");

        sb.AppendLine("</div></body></html>");
        return sb.ToString();
    }

    private void AddMetricCard(StringBuilder sb, string title, string value, string label, string cssClass, string unit = "")
    {
        var info = GetMetricInfo(title);
        sb.AppendLine($"<div class='metric-card {cssClass}'>");
        sb.AppendLine($"<div class='metric-title'>{title} - {label}</div>");
        sb.AppendLine($"<div class='metric-value'>{value}<span class='metric-unit'>{unit}</span></div>");
        if (!string.IsNullOrEmpty(info))
        {
            sb.AppendLine($"<div class='metric-info'>{info}</div>");
        }
        sb.AppendLine("</div>");
    }

    private string GetMetricInfo(string metricTitle)
    {
        return metricTitle switch
        {
            "Total Requests" => "Total number of API calls processed. Higher values indicate heavier workload.",
            "Success Rate" => "Percentage of successful responses (2xx status). Target: >99.9% for production systems.",
            "Avg Response Time" => "Average time to complete a request. Lower is better. Target: <100ms for excellent UX.",
            "Throughput" => "Requests processed per second. Higher values indicate better capacity and scalability.",
            _ => string.Empty
        };
    }

    private void AddTableRow(StringBuilder sb, string metric, string restValue, string graphqlValue, string winner)
    {
        sb.AppendLine($"<tr><td>{metric}</td><td>{restValue}</td><td>{graphqlValue}</td><td><span class='badge winner'>{winner}</span></td></tr>");
    }

    private string GenerateExamplesHtmlPage(List<ApiMetric> examples)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html><head>");
        sb.AppendLine("<title>REST vs GraphQL - Request/Response Examples</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background: #f5f5f5; }
            .container { max-width: 1800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
            h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
            h2 { color: #34495e; margin-top: 30px; border-left: 4px solid #3498db; padding-left: 15px; }
            .info-box { background: #e8f4f8; padding: 15px; border-radius: 6px; margin: 20px 0; border-left: 3px solid #3498db; }
            .comparison-pair { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin: 20px 0; }
            .api-column { background: #f8f9fa; padding: 20px; border-radius: 8px; }
            .api-column.rest { border-top: 4px solid #e74c3c; }
            .api-column.graphql { border-top: 4px solid #e91e63; }
            .api-label { font-size: 18px; font-weight: bold; margin-bottom: 15px; }
            .api-label.rest { color: #e74c3c; }
            .api-label.graphql { color: #e91e63; }
            .endpoint-title { font-size: 14px; font-weight: bold; color: #2c3e50; margin-bottom: 10px; }
            .req-resp-section { margin-bottom: 15px; }
            .section-header { font-size: 13px; font-weight: bold; color: #7f8c8d; margin-bottom: 5px; text-transform: uppercase; cursor: pointer; user-select: none; padding: 8px; background: #ecf0f1; border-radius: 4px; transition: background 0.2s; }
            .section-header:hover { background: #d5dbdb; }
            .section-header::before { content: '▼ '; font-size: 10px; margin-right: 5px; display: inline-block; transition: transform 0.2s; }
            .section-header.collapsed::before { transform: rotate(-90deg); }
            .collapsible-content { max-height: 500px; overflow: hidden; transition: max-height 0.3s ease-out; }
            .collapsible-content.collapsed { max-height: 0; }
            .code-block { background: #2c3e50; color: #ecf0f1; padding: 15px; border-radius: 5px; overflow-x: auto; font-family: 'Courier New', monospace; font-size: 12px; line-height: 1.5; max-height: 400px; overflow-y: auto; }
            .metadata { font-size: 12px; color: #7f8c8d; margin-top: 5px; }
            .no-data { color: #95a5a6; font-style: italic; padding: 10px; }
            .back-link { display: inline-block; margin-top: 20px; padding: 10px 20px; background: #3498db; color: white; text-decoration: none; border-radius: 5px; }
            .back-link:hover { background: #2980b9; }
            .status-badge { display: inline-block; padding: 3px 8px; border-radius: 3px; font-size: 11px; font-weight: bold; margin-left: 5px; }
            .status-200 { background: #27ae60; color: white; }
            .status-400 { background: #e74c3c; color: white; }
            .pair-number { color: #7f8c8d; font-size: 14px; font-weight: bold; margin: 20px 0 10px 0; padding: 8px; background: #ecf0f1; border-radius: 4px; }
        ");
        sb.AppendLine("</style>");
        sb.AppendLine("<script>");
        sb.AppendLine(@"
            function toggleCollapse(element) {
                element.classList.toggle('collapsed');
                const content = element.nextElementSibling;
                content.classList.toggle('collapsed');
            }
        ");
        sb.AppendLine("</script>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<div class='container'>");

        sb.AppendLine("<h1>Request/Response Examples</h1>");
        sb.AppendLine("<div class='info-box'>");
        sb.AppendLine("<strong>About These Examples:</strong><br>");
        sb.AppendLine("This page shows <strong>ALL</strong> HTTP requests and responses captured during your performance tests. ");
        sb.AppendLine("Every REST and GraphQL request is captured with its full request body and response body. ");
        sb.AppendLine("This gives you complete visibility into what data is being sent and received. ");
        sb.AppendLine("<strong>Note:</strong> For large tests (100+ iterations), this page may contain many examples.");
        sb.AppendLine("</div>");

        if (!examples.Any())
        {
            sb.AppendLine("<div class='no-data'>");
            sb.AppendLine("<h2>No Examples Captured Yet</h2>");
            sb.AppendLine("<p>Run some performance tests using <code>launch-tests.ps1</code> and examples will appear here.</p>");
            sb.AppendLine("<p>The system automatically captures <strong>all requests</strong> during testing.</p>");
            sb.AppendLine("</div>");
        }
        else
        {
            // Show summary at top
            var restCount = examples.Count(e => e.ApiType == ApiType.REST);
            var graphqlCount = examples.Count(e => e.ApiType == ApiType.GraphQL);

            // Get ALL distinct examples (across all groups)
            var allRestExamples = examples.Where(e => e.ApiType == ApiType.REST)
                .GroupBy(e => new { e.Endpoint, RequestBody = e.RequestBody ?? "" })
                .Select(g => g.First())
                .ToList();

            var allGraphQLExamples = examples.Where(e => e.ApiType == ApiType.GraphQL)
                .GroupBy(e => new { e.Endpoint, RequestBody = e.RequestBody ?? "" })
                .Select(g => g.First())
                .ToList();

            var totalDistinct = allRestExamples.Count + allGraphQLExamples.Count;

            sb.AppendLine("<div class='info-box'>");
            sb.AppendLine($"<strong>Captured:</strong> {restCount} REST requests ({allRestExamples.Count} distinct), {graphqlCount} GraphQL requests ({allGraphQLExamples.Count} distinct) - Total: {examples.Count} requests ({totalDistinct} distinct)");
            sb.AppendLine("</div>");

            // Create single two-column layout for entire page
            sb.AppendLine("<div class='comparison-pair'>");

            // LEFT COLUMN - ALL REST EXAMPLES
            sb.AppendLine("<div class='api-column rest'>");
            sb.AppendLine("<div class='api-label rest'>REST API</div>");
            sb.AppendLine($"<p style='color: #7f8c8d; font-size: 14px; margin-bottom: 20px;'>Showing {allRestExamples.Count} distinct REST requests</p>");

            if (allRestExamples.Any())
            {
                foreach (var example in allRestExamples)
                {
                    sb.AppendLine("<div style='margin-bottom: 30px; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>");
                    sb.AppendLine($"<div class='endpoint-title'>{HttpUtility.HtmlEncode(example.Method)} {HttpUtility.HtmlEncode(example.Endpoint)} <span class='status-badge status-{example.StatusCode}'>{example.StatusCode}</span></div>");

                    // Request
                    sb.AppendLine("<div class='req-resp-section'>");
                    sb.AppendLine("<div class='section-header' onclick='toggleCollapse(this)'>Request</div>");
                    sb.AppendLine("<div class='collapsible-content'>");
                    if (!string.IsNullOrEmpty(example.RequestBody))
                    {
                        sb.AppendLine($"<pre class='code-block'>{HttpUtility.HtmlEncode(FormatJson(example.RequestBody))}</pre>");
                    }
                    else
                    {
                        sb.AppendLine("<div class='no-data'>No request body (GET request)</div>");
                    }
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");

                    // Response
                    sb.AppendLine("<div class='req-resp-section'>");
                    sb.AppendLine("<div class='section-header' onclick='toggleCollapse(this)'>Response</div>");
                    sb.AppendLine("<div class='collapsible-content'>");
                    if (!string.IsNullOrEmpty(example.ResponseBody))
                    {
                        sb.AppendLine($"<pre class='code-block'>{HttpUtility.HtmlEncode(FormatJson(example.ResponseBody))}</pre>");
                        sb.AppendLine($"<div class='metadata'>Size: {FormatBytes(example.ResponseSizeBytes)} | Time: {example.ResponseTimeMs:F2}ms</div>");
                    }
                    else
                    {
                        sb.AppendLine("<div class='no-data'>Response body not captured</div>");
                    }
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");
                }
            }
            else
            {
                sb.AppendLine("<div class='no-data'>No REST examples available</div>");
            }

            sb.AppendLine("</div>"); // End REST column

            // RIGHT COLUMN - ALL GRAPHQL EXAMPLES
            sb.AppendLine("<div class='api-column graphql'>");
            sb.AppendLine("<div class='api-label graphql'>GraphQL API</div>");
            sb.AppendLine($"<p style='color: #7f8c8d; font-size: 14px; margin-bottom: 20px;'>Showing {allGraphQLExamples.Count} distinct GraphQL requests</p>");

            if (allGraphQLExamples.Any())
            {
                foreach (var example in allGraphQLExamples)
                {
                    sb.AppendLine("<div style='margin-bottom: 30px; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);'>");
                    sb.AppendLine($"<div class='endpoint-title'>{HttpUtility.HtmlEncode(example.Method)} {HttpUtility.HtmlEncode(example.Endpoint)} <span class='status-badge status-{example.StatusCode}'>{example.StatusCode}</span></div>");

                    // Request
                    sb.AppendLine("<div class='req-resp-section'>");
                    sb.AppendLine("<div class='section-header' onclick='toggleCollapse(this)'>Request (GraphQL Query)</div>");
                    sb.AppendLine("<div class='collapsible-content'>");
                    if (!string.IsNullOrEmpty(example.RequestBody))
                    {
                        sb.AppendLine($"<pre class='code-block'>{HttpUtility.HtmlEncode(FormatJson(example.RequestBody))}</pre>");
                    }
                    else
                    {
                        sb.AppendLine("<div class='no-data'>No request body captured</div>");
                    }
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");

                    // Response
                    sb.AppendLine("<div class='req-resp-section'>");
                    sb.AppendLine("<div class='section-header' onclick='toggleCollapse(this)'>Response</div>");
                    sb.AppendLine("<div class='collapsible-content'>");
                    if (!string.IsNullOrEmpty(example.ResponseBody))
                    {
                        sb.AppendLine($"<pre class='code-block'>{HttpUtility.HtmlEncode(FormatJson(example.ResponseBody))}</pre>");
                        sb.AppendLine($"<div class='metadata'>Size: {FormatBytes(example.ResponseSizeBytes)} | Time: {example.ResponseTimeMs:F2}ms</div>");
                    }
                    else
                    {
                        sb.AppendLine("<div class='no-data'>Response body not captured</div>");
                    }
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");
                    sb.AppendLine("</div>");
                }
            }
            else
            {
                sb.AppendLine("<div class='no-data'>No GraphQL examples available</div>");
            }

            sb.AppendLine("</div>"); // End GraphQL column
            sb.AppendLine("</div>"); // End comparison-pair
        }

        sb.AppendLine("<a href='/api/metrics/report' class='back-link'>← Back to Performance Report</a>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private string NormalizeEndpoint(string endpoint)
    {
        // Group similar endpoints together (e.g., /api/customers/123 -> Customers)
        if (endpoint.Contains("/customers")) return "Customers";
        if (endpoint.Contains("/orders")) return "Orders";
        if (endpoint.Contains("/products")) return "Products";
        if (endpoint.Contains("/dashboard")) return "Dashboard";
        if (endpoint.Contains("/graphql")) return "GraphQL";
        return endpoint;
    }

    private string FormatJson(string json)
    {
        try
        {
            // Simple JSON formatting (indent with 2 spaces)
            var obj = System.Text.Json.JsonSerializer.Deserialize<object>(json);
            return System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        catch
        {
            return json; // Return as-is if not valid JSON
        }
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
