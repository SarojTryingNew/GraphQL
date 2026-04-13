# REST vs GraphQL - Bulk CREATE Operations Load Test
param(
    [string]$BaseUrl = "http://localhost:5072",
    [int]$Iterations = 50,
    [int]$OrdersPerBulk = 10
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "   Bulk CREATE Operations - REST Direct vs REST+GraphQL" -ForegroundColor Cyan
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

Write-Host ""
Write-Host "Test Configuration:" -ForegroundColor Yellow
Write-Host "  Base URL: $BaseUrl"
Write-Host "  Iterations: $Iterations"
Write-Host "  Orders per bulk request: $OrdersPerBulk"
Write-Host "  Total orders to create: $($Iterations * $OrdersPerBulk)"
Write-Host ""

function Invoke-BulkTest {
    param(
        [string]$Name,
        [scriptblock]$Request,
        [int]$Count
    )
    
    Write-Host "Testing: $Name" -ForegroundColor Cyan
    $successCount = 0
    $errorCount = 0
    $totalOrdersCreated = 0
    
    for ($i = 1; $i -le $Count; $i++) {
        try {
            $result = & $Request -url $BaseUrl -ordersCount $OrdersPerBulk
            $successCount++
            $totalOrdersCreated += $OrdersPerBulk
        }
        catch {
            $errorCount++
            Write-Host "  Error in iteration $i : $($_.Exception.Message)" -ForegroundColor Red
        }
        
        if ($i % 5 -eq 0) {
            $progress = ($i / $Count) * 100
            Write-Progress -Activity "Bulk Testing: $Name" -Status "$i of $Count bulk requests" -PercentComplete $progress
        }
    }
    
    Write-Progress -Activity "Bulk Testing: $Name" -Completed
    Write-Host "  Completed: $successCount successful bulk requests" -ForegroundColor Green
    Write-Host "  Orders created: $totalOrdersCreated" -ForegroundColor Green
    Write-Host "  Errors: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
    Write-Host ""
}

# Test 1: REST Direct (calling DataStore directly)
Write-Host "`n=== Test 1: REST Direct - Bulk Create Orders ===" -ForegroundColor White

Invoke-BulkTest -Name "REST Direct - Bulk Create ($OrdersPerBulk orders per request)" -Count $Iterations -Request {
    param($url, $ordersCount)

    $orders = @()
    for ($i = 1; $i -le $ordersCount; $i++) {
        $customerId = (($i % 5) + 1)
        $orders += @{
            customerId = $customerId
            status = "Pending"
            items = @(
                @{
                    productId = (($i % 8) + 1)
                    quantity = [int](Get-Random -Minimum 1 -Maximum 5)
                    discount = [int](Get-Random -Minimum 0 -Maximum 20)
                    notes = @("Bulk create test $i", "Load test iteration")
                }
            )
        }
    }

    $bulkData = @{ orders = $orders } | ConvertTo-Json -Depth 10
    Invoke-RestMethod -Uri "$url/api/orders/bulk" -Method Post -Body $bulkData -ContentType "application/json" -ErrorAction Stop
}

# Test 2: REST with GraphQL Backend (REST API -> GraphQL -> DataStore)
Write-Host "`n=== Test 2: REST with GraphQL Backend - Bulk Create Orders ===" -ForegroundColor White

Invoke-BulkTest -Name "REST with GraphQL - Bulk Create ($OrdersPerBulk orders per request)" -Count $Iterations -Request {
    param($url, $ordersCount)

    $orders = @()
    for ($i = 1; $i -le $ordersCount; $i++) {
        $customerId = (($i % 5) + 1)
        $orders += @{
            customerId = $customerId
            status = "Pending"
            items = @(
                @{
                    productId = (($i % 8) + 1)
                    quantity = [int](Get-Random -Minimum 1 -Maximum 5)
                    discount = [int](Get-Random -Minimum 0 -Maximum 20)
                    notes = @("Bulk create test $i", "Load test iteration")
                }
            )
        }
    }

    $bulkData = @{ orders = $orders } | ConvertTo-Json -Depth 10
    Invoke-RestMethod -Uri "$url/api/graphql-backend/orders/bulk" -Method Post -Body $bulkData -ContentType "application/json" -ErrorAction Stop
}

# Collect and Display Results
Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Collecting Bulk CREATE Metrics..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $comparison = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/comparison" -Method Get -ErrorAction Stop

    Write-Host "`n================================================================" -ForegroundColor Green
    Write-Host "     BULK CREATE - KPI COMPARISON RESULTS" -ForegroundColor Green
    Write-Host "     REST Direct vs REST with GraphQL Backend" -ForegroundColor Green
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

    Write-Host "`nREST Direct API (DataStore) - BULK CREATE:" -ForegroundColor Red
    Write-Host "  Total Requests:     $($comparison.restDirectMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.restDirectMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.restDirectMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.restDirectMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.restDirectMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.restDirectMetrics.requestsPerSecond.ToString('F2')) req/s"

    Write-Host "`nREST with GraphQL Backend - BULK CREATE:" -ForegroundColor Magenta
    Write-Host "  Total Requests:     $($comparison.restWithGraphQLMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.restWithGraphQLMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.restWithGraphQLMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.restWithGraphQLMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.restWithGraphQLMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.restWithGraphQLMetrics.requestsPerSecond.ToString('F2')) req/s"

    # Calculate bulk operation efficiency
    $totalOrders = $Iterations * $OrdersPerBulk
    Write-Host "`nBULK CREATE EFFICIENCY:" -ForegroundColor Cyan
    Write-Host "  Total Orders Created: ~$totalOrders"
    if ($comparison.restDirectMetrics.totalRequests -gt 0) {
        Write-Host "  REST Direct Efficiency:      $(($totalOrders / $comparison.restDirectMetrics.totalRequests).ToString('F2')) orders/request"
    }
    if ($comparison.restWithGraphQLMetrics.totalRequests -gt 0) {
        Write-Host "  REST+GraphQL Efficiency:     $(($totalOrders / $comparison.restWithGraphQLMetrics.totalRequests).ToString('F2')) orders/request"
    }

    Write-Host "`nFull HTML Report: $BaseUrl/api/metrics/report" -ForegroundColor Cyan
}
catch {
    Write-Host "`nError retrieving metrics: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "View metrics at: $BaseUrl/api/metrics/report" -ForegroundColor Yellow
}

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Bulk CREATE operations testing completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  - Tested bulk CREATE operations ($Iterations iterations)" -ForegroundColor White
Write-Host "  - Each bulk request processed $OrdersPerBulk orders" -ForegroundColor White
Write-Host "  - Total orders created: ~$($Iterations * $OrdersPerBulk)" -ForegroundColor White
Write-Host ""
