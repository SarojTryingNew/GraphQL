# REST vs GraphQL - UI Aggregation / Dashboard Test
# Scenario (c): Dashboard / Composite View with Aggregations
param(
    [string]$BaseUrl = "http://localhost:5072",
    [int]$Iterations = 100
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "     UI Aggregation / Dashboard - REST vs GraphQL" -ForegroundColor Cyan
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

# Reset metrics
Write-Host "`nResetting metrics..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/reset" -Method Post -ErrorAction Stop
    Write-Host "Metrics reset successfully" -ForegroundColor Green
}
catch {
    Write-Host "Warning: Could not reset metrics" -ForegroundColor Yellow
}

# Set test scenario
Write-Host "Setting test scenario..." -ForegroundColor Yellow
try {
    $scenarioBody = @{ scenarioName = "Dashboard Aggregation Test ($Iterations iterations)" } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction Stop
    Write-Host "Test scenario set" -ForegroundColor Green
}
catch {
    Write-Host "Warning: Could not set test scenario" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Test Configuration:" -ForegroundColor Yellow
Write-Host "  Base URL: $BaseUrl"
Write-Host "  Iterations: $Iterations"
Write-Host "  Aggregations: Total Orders, Total Revenue, Top Products, Top Customers"
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

# Test: Dashboard Aggregations
Write-Host "`n=== Dashboard / UI Aggregation Operations ===" -ForegroundColor White
Write-Host "  Metrics: Total Orders, Total Revenue" -ForegroundColor Gray
Write-Host "  Aggregations: Top Products (with revenue), Top Customers (with spending)" -ForegroundColor Gray
Write-Host ""

Invoke-LoadTest -Name "REST - Dashboard Aggregations" -Count $Iterations -Request {
    param($url)
    Invoke-RestMethod -Uri "$url/api/dashboard" -Method Get -ErrorAction Stop
}

Invoke-LoadTest -Name "GraphQL - Dashboard Aggregations" -Count $Iterations -Request {
    param($url)
    $query = @{ query = "{ dashboard { totalOrders totalRevenue topProducts { productName revenue } topCustomers { customerName totalSpent } } }" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $query -ContentType "application/json" -ErrorAction Stop
}

# Collect and Display Results
Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Collecting Metrics..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $comparison = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/comparison" -Method Get -ErrorAction Stop
    
    Write-Host "`n================================================================" -ForegroundColor Green
    Write-Host "           DASHBOARD - KPI COMPARISON RESULTS" -ForegroundColor Green
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
    
    Write-Host "`nREST API - DASHBOARD:" -ForegroundColor Red
    Write-Host "  Total Requests:     $($comparison.restMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.restMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.restMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.restMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.restMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.restMetrics.requestsPerSecond.ToString('F2')) req/s"
    
    Write-Host "`nGraphQL API - DASHBOARD:" -ForegroundColor Magenta
    Write-Host "  Total Requests:     $($comparison.graphQLMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.graphQLMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.graphQLMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.graphQLMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.graphQLMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.graphQLMetrics.requestsPerSecond.ToString('F2')) req/s"
    
    Write-Host "`nFull HTML Report: $BaseUrl/api/metrics/report" -ForegroundColor Cyan
}
catch {
    Write-Host "`nError retrieving metrics: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "View metrics at: $BaseUrl/api/metrics/report" -ForegroundColor Yellow
}

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Dashboard aggregation testing completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  - Tested dashboard with complex aggregations" -ForegroundColor White
Write-Host "  - Metrics: Total orders, revenue, top products, top customers" -ForegroundColor White
Write-Host "  - Total requests: $Iterations per API" -ForegroundColor White
Write-Host ""
