# REST vs GraphQL - Load Testing & KPI/NFR Measurement
param(
    [string]$BaseUrl = "http://localhost:5072",
    [int]$Iterations = 100
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "     REST vs GraphQL - KPI & NFR Load Testing" -ForegroundColor Cyan
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
    $testType = if ($Iterations -eq 10) { "Quick Test" } elseif ($Iterations -eq 100) { "Standard Load Test" } else { "Custom Test" }
    $scenarioBody = @{ scenarioName = "$testType ($Iterations iterations)" } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction Stop
    Write-Host "Test scenario set" -ForegroundColor Green
}
catch {
    Write-Host "Warning: Could not set test scenario" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Test Configuration:" -ForegroundColor Yellow
Write-Host "  Base URL: $BaseUrl"
Write-Host "  Iterations: $Iterations per test"
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

# Test 1: Simple GET Requests
Write-Host "`n=== Test 1: Simple GET Requests ===" -ForegroundColor White

Invoke-LoadTest -Name "REST - Get All Customers" -Count $Iterations -Request {
    param($url)
    Invoke-RestMethod -Uri "$url/api/customers" -Method Get -ErrorAction Stop
}

Invoke-LoadTest -Name "GraphQL - Get All Customers" -Count $Iterations -Request {
    param($url)
    $query = @{ query = "{ customers { id name email } }" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $query -ContentType "application/json" -ErrorAction Stop
}

# Test 2: Nested Data Queries
Write-Host "`n=== Test 2: Nested Data (3 levels deep) ===" -ForegroundColor White

Invoke-LoadTest -Name "REST - Order with Nested Data" -Count $Iterations -Request {
    param($url)
    Invoke-RestMethod -Uri "$url/api/orders/1/nested" -Method Get -ErrorAction Stop
}

Invoke-LoadTest -Name "GraphQL - Order with Nested Data" -Count $Iterations -Request {
    param($url)
    $query = @{ query = "{ order(id: 1) { id orderDate customer { name } items { quantity product { name category { name } } notes { content } } } }" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $query -ContentType "application/json" -ErrorAction Stop
}

# Test 3: Dashboard Aggregations
Write-Host "`n=== Test 3: Dashboard Aggregations ===" -ForegroundColor White

Invoke-LoadTest -Name "REST - Dashboard" -Count $Iterations -Request {
    param($url)
    Invoke-RestMethod -Uri "$url/api/dashboard" -Method Get -ErrorAction Stop
}

Invoke-LoadTest -Name "GraphQL - Dashboard" -Count $Iterations -Request {
    param($url)
    $query = @{ query = "{ dashboard { totalOrders totalRevenue topProducts { productName revenue } topCustomers { customerName totalSpent } } }" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $query -ContentType "application/json" -ErrorAction Stop
}

# Test 4: Multiple Resources
Write-Host "`n=== Test 4: Multiple Resources ===" -ForegroundColor White
Write-Host "  This test simulates $Iterations users requesting customer data + orders + products" -ForegroundColor Gray
Write-Host "  REST: Makes 3 HTTP calls per user ($($Iterations * 3) total requests)" -ForegroundColor Gray
Write-Host "  GraphQL: Makes 1 HTTP call per user ($Iterations total requests)" -ForegroundColor Gray
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
    Write-Host "                 KPI COMPARISON RESULTS" -ForegroundColor Green
    Write-Host "================================================================" -ForegroundColor Green
    
    Write-Host "`nWINNERS:" -ForegroundColor Yellow
    Write-Host "  Response Time:  $($comparison.responseTimeWinner)" -ForegroundColor Green
    Write-Host "  Payload Size:   $($comparison.payloadSizeWinner)" -ForegroundColor Green
    Write-Host "  Throughput:     $($comparison.throughputWinner)" -ForegroundColor Green
    
    Write-Host "`nREST API METRICS:" -ForegroundColor Red
    Write-Host "  Total Requests:     $($comparison.restMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.restMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.restMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.restMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    
    Write-Host "`nGraphQL API METRICS:" -ForegroundColor Magenta
    Write-Host "  Total Requests:     $($comparison.graphQLMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.graphQLMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.graphQLMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.graphQLMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    
    Write-Host "`nFull HTML Report: $BaseUrl/api/metrics/report" -ForegroundColor Cyan
}
catch {
    Write-Host "`nError retrieving metrics: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "View metrics at: $BaseUrl/api/metrics/report" -ForegroundColor Yellow
}

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Load testing completed!" -ForegroundColor Green
Write-Host ""

