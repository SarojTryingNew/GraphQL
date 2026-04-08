# REST vs GraphQL - Multiple Dependent REST Calls Test
# Scenario (d): Multiple resources requiring multiple REST calls vs single GraphQL query
param(
    [string]$BaseUrl = "http://localhost:5072",
    [int]$Iterations = 100
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "     Multiple Dependent REST Calls - REST vs GraphQL" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if API is running
Write-Host "Checking if API is running at $BaseUrl..." -ForegroundColor Yellow
try {
    $null = Invoke-WebRequest -Uri "$BaseUrl/api/customers" -Method Get -TimeoutSec 5 -ErrorAction Stop
    Write-Host "API is running!" -ForegroundColor Green
}
catch {
    Write-Host "Cannot connect to API at $BaseUrl" -ForegroundColor Red
    Write-Host "Please make sure the API is running (dotnet run)" -ForegroundColor Yellow
    exit 1
}

# Note: Metrics reset and scenario setting handled by launch-tests.ps1

Write-Host ""
Write-Host "Test Configuration:" -ForegroundColor Yellow
Write-Host "  Base URL: $BaseUrl"
Write-Host "  Iterations: $Iterations users"
Write-Host "  REST: 3 HTTP calls per user ($($Iterations * 3) total calls)"
Write-Host "  GraphQL: 1 HTTP call per user ($Iterations total calls)"
Write-Host ""

function Invoke-LoadTest {
    param(
        [string]$Name,
        [scriptblock]$Request,
        [int]$Count
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Cyan
    $successCount = 0
    $errorCount = 0
    
    for ($i = 1; $i -le $Count; $i++) {
        try {
            $null = & $Request -url $BaseUrl
            $successCount++
        }
        catch {
            $errorCount++
        }
        
        if ($i % 10 -eq 0) {
            $progress = ($i / $Count) * 100
            Write-Progress -Activity "Load Testing: $Name" -Status "$i of $Count requests" -PercentComplete $progress
        }
    }
    
    Write-Progress -Activity "Load Testing: $Name" -Completed
    Write-Host "  Completed: $successCount successful, $errorCount errors" -ForegroundColor Green
    Write-Host ""
}

# Test: Multiple Resources
Write-Host "`n=== Multiple Dependent REST Calls ===" -ForegroundColor White
Write-Host "  Scenario: Fetch customer details + orders + product catalog" -ForegroundColor Gray
Write-Host "  REST: 3 sequential HTTP calls" -ForegroundColor Gray
Write-Host "    1. GET /api/customers/1" -ForegroundColor DarkGray
Write-Host "    2. GET /api/customers/1/orders" -ForegroundColor DarkGray
Write-Host "    3. GET /api/products" -ForegroundColor DarkGray
Write-Host "  GraphQL: 1 combined query" -ForegroundColor Gray
Write-Host ""

Invoke-LoadTest -Name "REST - Multiple Calls (3 HTTP requests per user)" -Count $Iterations -Request {
    param($url)
    Invoke-RestMethod -Uri "$url/api/customers/1" -Method Get -ErrorAction Stop
    Invoke-RestMethod -Uri "$url/api/customers/1/orders" -Method Get -ErrorAction Stop
    Invoke-RestMethod -Uri "$url/api/products" -Method Get -ErrorAction Stop
}

Invoke-LoadTest -Name "GraphQL - Single Call (1 HTTP request per user)" -Count $Iterations -Request {
    param($url)
    $query = @{ query = "{ customer(id: 1) { name orders { id totalAmount } } products { id name price } }" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $query -ContentType "application/json" -ErrorAction Stop
}

# Collect and Display Results
Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Collecting Metrics..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $comparison = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/comparison" -Method Get -ErrorAction Stop
    
    Write-Host "`n================================================================" -ForegroundColor Green
    Write-Host "           MULTIPLE CALLS - KPI COMPARISON RESULTS" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Green
    
    Write-Host "`nWINNERS:" -ForegroundColor Yellow
    Write-Host "  Response Time:  $($comparison.responseTimeWinner)" -ForegroundColor Green
    Write-Host "  Payload Size:   $($comparison.payloadSizeWinner)" -ForegroundColor Green
    Write-Host "  Throughput:     $($comparison.throughputWinner)" -ForegroundColor Green
    
    # Calculate improvements
    $rtImprovement = [Math]::Abs($comparison.responseTimeImprovement)
    $psImprovement = [Math]::Abs($comparison.payloadSizeImprovement)
    
    Write-Host "`nIMPROVEMENTS:" -ForegroundColor Yellow
    Write-Host "  Response Time:  $($rtImprovement.ToString('F2'))% " -NoNewline
    if ($comparison.responseTimeImprovement -gt 0) {
        Write-Host "faster" -ForegroundColor Green
    } else {
        Write-Host "slower" -ForegroundColor Red
    }
    
    Write-Host "  Payload Size:   $($psImprovement.ToString('F2'))% " -NoNewline
    if ($comparison.payloadSizeImprovement -gt 0) {
        Write-Host "smaller" -ForegroundColor Green
    } else {
        Write-Host "larger" -ForegroundColor Red
    }
    
    Write-Host "`nREST API - MULTIPLE CALLS:" -ForegroundColor Red
    Write-Host "  Total Requests:     $($comparison.restMetrics.totalRequests) (should be ~$($Iterations * 3))"
    Write-Host "  Success Rate:       $($comparison.restMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.restMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.restMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.restMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.restMetrics.requestsPerSecond.ToString('F2')) req/s"
    
    Write-Host "`nGraphQL API - SINGLE CALL:" -ForegroundColor Magenta
    Write-Host "  Total Requests:     $($comparison.graphQLMetrics.totalRequests) (should be ~$Iterations)"
    Write-Host "  Success Rate:       $($comparison.graphQLMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.graphQLMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.graphQLMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.graphQLMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.graphQLMetrics.requestsPerSecond.ToString('F2')) req/s"
    
    # Calculate call efficiency
    $restCalls = $comparison.restMetrics.totalRequests
    $graphqlCalls = $comparison.graphQLMetrics.totalRequests
    $callReduction = 0
    if ($restCalls -gt 0) {
        $callReduction = (($restCalls - $graphqlCalls) / $restCalls) * 100
    }
    
    Write-Host "`nHTTP CALL EFFICIENCY:" -ForegroundColor Cyan
    Write-Host "  REST Total Calls:     $restCalls"
    Write-Host "  GraphQL Total Calls:  $graphqlCalls"
    Write-Host "  Reduction:            $($callReduction.ToString('F2'))% fewer HTTP calls with GraphQL" -ForegroundColor Green
    
    Write-Host "`nFull HTML Report: $BaseUrl/api/metrics/report" -ForegroundColor Cyan
}
catch {
    Write-Host "`nError retrieving metrics: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "View metrics at: $BaseUrl/api/metrics/report" -ForegroundColor Yellow
}

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Multiple dependent REST calls testing completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  - Simulated $Iterations users requesting multi-resource data" -ForegroundColor White
Write-Host "  - REST made ~$($Iterations * 3) HTTP calls (3 per user)" -ForegroundColor White
Write-Host "  - GraphQL made ~$Iterations HTTP calls (1 per user)" -ForegroundColor White
Write-Host "  - GraphQL achieved ~67% reduction in HTTP requests" -ForegroundColor White
Write-Host ""
