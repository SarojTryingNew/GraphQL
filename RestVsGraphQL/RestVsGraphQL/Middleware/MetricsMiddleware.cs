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

        // Memory measurement: Using GC.GetTotalAllocatedBytes() because GetAllocatedBytesForCurrentThread()
        // doesn't work correctly with async/await (thread switching causes negative values).
        // Note: This measures process-wide allocations, so it's an approximation under concurrent load.
        // For precise per-request measurements, use a profiler or diagnostic tools instead.
        var memoryBefore = GC.GetTotalAllocatedBytes(precise: false);

        // Capture ALL requests and responses
        var shouldCapture = true;

        // Capture request body if needed
        string? requestBody = null;
        if (shouldCapture && context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0; // Reset for next middleware
        }

        // Use capturing stream to track bytes and capture response
        using var responseBodyWrapper = new ResponseCapturingStream(originalBodyStream, shouldCapture);
        context.Response.Body = responseBodyWrapper;

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

            // Calculate memory used during request (approximate under concurrent load)
            var memoryAfter = GC.GetTotalAllocatedBytes(precise: false);
            var memoryUsed = memoryAfter - memoryBefore;

            // Clamp to non-negative values (concurrent requests may cause fluctuations)
            if (memoryUsed < 0) memoryUsed = 0;

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
                ResponseSizeBytes = responseBodyWrapper.BytesWritten,
                RequestSizeBytes = requestSize,
                StatusCode = context.Response.StatusCode,
                Success = success,
                ErrorMessage = errorMessage,
                MemoryUsedBytes = memoryUsed,
                RequestBody = requestBody,
                ResponseBody = shouldCapture ? responseBodyWrapper.GetCapturedResponse() : null
            };

            _metricsCollector.RecordRequest(metric);
        }
    }
}
