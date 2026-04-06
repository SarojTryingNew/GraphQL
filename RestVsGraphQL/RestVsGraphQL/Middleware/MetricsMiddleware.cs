using Microsoft.AspNetCore.Http;
using RestVsGraphQL.Metrics;
using System.Diagnostics;
using System.Text;

namespace RestVsGraphQL.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MetricsCollector _metricsCollector;

    public MetricsMiddleware(RequestDelegate next, MetricsCollector metricsCollector)
    {
        _next = next;
        _metricsCollector = metricsCollector;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip metrics collection for non-API endpoints
        if (!context.Request.Path.StartsWithSegments("/api") && 
            !context.Request.Path.StartsWithSegments("/graphql"))
        {
            await _next(context);
            return;
        }

        // Skip metrics collection for all /api/metrics endpoints
        if (context.Request.Path.StartsWithSegments("/api/metrics"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        var memoryBefore = GC.GetTotalMemory(false);

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var requestSize = context.Request.ContentLength ?? 0;
        var success = true;
        string? errorMessage = null;

        try
        {
            await _next(context);
            success = context.Response.StatusCode >= 200 && context.Response.StatusCode < 400;
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var memoryAfter = GC.GetTotalMemory(false);
            var memoryUsed = Math.Max(0, memoryAfter - memoryBefore);

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            var apiType = context.Request.Path.StartsWithSegments("/api") 
                ? ApiType.REST 
                : ApiType.GraphQL;

            var metric = new ApiMetric
            {
                ApiType = apiType,
                Endpoint = context.Request.Path,
                Method = context.Request.Method,
                Timestamp = DateTime.UtcNow,
                ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds,
                ResponseSizeBytes = responseBody.Length,
                RequestSizeBytes = requestSize,
                StatusCode = context.Response.StatusCode,
                Success = success,
                ErrorMessage = errorMessage,
                MemoryUsedBytes = memoryUsed
            };

            _metricsCollector.RecordRequest(metric);
        }
    }
}
