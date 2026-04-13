# REST vs GraphQL - Bulk DELETE Operations Load Test
param(
    [string]$BaseUrl = "http://localhost:5072",
    [int]$Iterations = 16,
    [int]$OrdersPerBulk = 10
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "     Bulk DELETE Operations - REST vs GraphQL" -ForegroundColor Cyan
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
Write-Host "  Total orders to delete: $($Iterations * $OrdersPerBulk)"
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
    $totalOrdersDeleted = 0
    
    for ($i = 1; $i -le $Count; $i++) {
        try {
            $result = & $Request -url $BaseUrl -ordersCount $OrdersPerBulk
            $successCount++
            $totalOrdersDeleted += $OrdersPerBulk
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
    Write-Host "  Orders deleted: $totalOrdersDeleted" -ForegroundColor Green
    Write-Host "  Errors: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
    Write-Host ""
}

# Test 1: REST Bulk Delete
Write-Host "`n=== Test 1: REST Bulk Delete Orders ===" -ForegroundColor White

Invoke-BulkTest -Name "REST - Bulk Delete ($OrdersPerBulk orders per request)" -Count $Iterations -Request {
    param($url, $ordersCount)
    
    # Get oldest orders to delete
    $allOrders = Invoke-RestMethod -Uri "$url/api/orders" -Method Get -ErrorAction Stop
    $oldOrders = $allOrders | Select-Object -Last $ordersCount
    
    $orderIds = @()
    foreach ($order in $oldOrders) {
        $orderIds += $order.id
    }
    
    $bulkDeleteData = @{ orderIds = $orderIds } | ConvertTo-Json -Depth 10
    Invoke-RestMethod -Uri "$url/api/orders/bulk" -Method Delete -Body $bulkDeleteData -ContentType "application/json" -ErrorAction Stop
}

# Test 2: GraphQL Bulk Delete
Write-Host "`n=== Test 2: GraphQL Bulk Delete Orders ===" -ForegroundColor White

Invoke-BulkTest -Name "GraphQL - Bulk Delete ($OrdersPerBulk orders per request)" -Count $Iterations -Request {
    param($url, $ordersCount)
    
    # Get oldest orders via GraphQL
    $query = "{ orders { id } }"
    $graphqlQuery = @{ query = $query } | ConvertTo-Json
    $ordersResponse = Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $graphqlQuery -ContentType "application/json" -ErrorAction Stop
    $oldOrders = $ordersResponse.data.orders | Select-Object -Last $ordersCount
    
    $orderIds = @()
    foreach ($order in $oldOrders) {
        $orderIds += $order.id
    }
    
    $mutation = "mutation(`$request: BulkOrderDeleteRequestInput!) { bulkDeleteOrders(request: `$request) { successCount failureCount errors deletedIds } }"
    $variables = @{ request = @{ orderIds = $orderIds } }
    $graphqlRequest = @{ query = $mutation; variables = $variables } | ConvertTo-Json -Depth 10
    
    Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $graphqlRequest -ContentType "application/json" -ErrorAction Stop
}

# Collect and Display Results
Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Collecting Bulk DELETE Metrics..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $comparison = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/comparison" -Method Get -ErrorAction Stop
    
    Write-Host "`n================================================================" -ForegroundColor Green
    Write-Host "           BULK DELETE - KPI COMPARISON RESULTS" -ForegroundColor Green
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
    
    Write-Host "`nREST API - BULK DELETE:" -ForegroundColor Red
    Write-Host "  Total Requests:     $($comparison.restMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.restMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.restMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.restMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.restMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.restMetrics.requestsPerSecond.ToString('F2')) req/s"
    
    Write-Host "`nGraphQL API - BULK DELETE:" -ForegroundColor Magenta
    Write-Host "  Total Requests:     $($comparison.graphQLMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.graphQLMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.graphQLMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.graphQLMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.graphQLMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.graphQLMetrics.requestsPerSecond.ToString('F2')) req/s"
    
    # Calculate bulk operation efficiency
    $totalOrders = $Iterations * $OrdersPerBulk
    Write-Host "`nBULK DELETE EFFICIENCY:" -ForegroundColor Cyan
    Write-Host "  Total Orders Deleted: ~$totalOrders"
    Write-Host "  REST Efficiency:      $(($totalOrders / $comparison.restMetrics.totalRequests).ToString('F2')) orders/request"
    Write-Host "  GraphQL Efficiency:   $(($totalOrders / $comparison.graphQLMetrics.totalRequests).ToString('F2')) orders/request"
    
    Write-Host "`nFull HTML Report: $BaseUrl/api/metrics/report" -ForegroundColor Cyan
}
catch {
    Write-Host "`nError retrieving metrics: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "View metrics at: $BaseUrl/api/metrics/report" -ForegroundColor Yellow
}

Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Bulk DELETE operations testing completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  - Tested bulk DELETE operations ($Iterations iterations)" -ForegroundColor White
Write-Host "  - Each bulk request processed $OrdersPerBulk orders" -ForegroundColor White
Write-Host "  - Total orders deleted: ~$($Iterations * $OrdersPerBulk)" -ForegroundColor White
Write-Host "  - Includes cascade deletion of items and notes" -ForegroundColor White
Write-Host ""
