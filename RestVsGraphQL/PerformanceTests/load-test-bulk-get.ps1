# REST vs GraphQL - Get All Orders Load Test
param(
    [string]$BaseUrl = "http://localhost:5072",
    [int]$Iterations = 10
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "     Get All Orders Load Test - REST vs GraphQL" -ForegroundColor Cyan
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
Write-Host ""

function Invoke-GetAllTest {
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
            $result = & $Request -url $BaseUrl
            $successCount++
        }
        catch {
            $errorCount++
            Write-Host "  Error in iteration $i : $($_.Exception.Message)" -ForegroundColor Red
        }

        if ($i % 5 -eq 0) {
            $progress = ($i / $Count) * 100
            Write-Progress -Activity "Get All Testing: $Name" -Status "$i of $Count requests" -PercentComplete $progress
        }
    }

    Write-Progress -Activity "Get All Testing: $Name" -Completed
    Write-Host "  Completed: $successCount successful requests" -ForegroundColor Green
    Write-Host "  Errors: $errorCount" -ForegroundColor $(if ($errorCount -gt 0) { "Red" } else { "Green" })
    Write-Host ""
}

# Test 1: REST Get All Orders
Write-Host "`n=== Test 1: REST Get All Orders ===" -ForegroundColor White

Invoke-GetAllTest -Name "REST - Get All Orders" -Count $Iterations -Request {
    param($url)
    Invoke-RestMethod -Uri "$url/api/orders" -Method Get -ErrorAction Stop
}

# Test 2: GraphQL Get All Orders
Write-Host "`n=== Test 2: GraphQL Get All Orders ===" -ForegroundColor White

Invoke-GetAllTest -Name "GraphQL - Get All Orders" -Count $Iterations -Request {
    param($url)

    $query = @"
query GetAllOrders {
    orders {
        id
        customerId
        orderDate
        status
        totalAmount
        customer {
            id
            name
            email
        }
        items {
            id
            quantity
            unitPrice
            discount
            product {
                id
                name
                price
            }
        }
    }
}
"@

    $graphqlRequest = @{ query = $query } | ConvertTo-Json
    Invoke-RestMethod -Uri "$url/graphql" -Method Post -Body $graphqlRequest -ContentType "application/json" -ErrorAction Stop
}

# Collect and Display Results
Write-Host "`n================================================================" -ForegroundColor Cyan
Write-Host "Collecting Get All Orders Metrics..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $comparison = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/comparison" -Method Get -ErrorAction Stop

    Write-Host "`n================================================================" -ForegroundColor Green
    Write-Host "           GET ALL ORDERS - KPI COMPARISON RESULTS" -ForegroundColor Green
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

    Write-Host "`nREST API - GET ALL ORDERS:" -ForegroundColor Red
    Write-Host "  Total Requests:     $($comparison.restMetrics.totalRequests)"
    Write-Host "  Success Rate:       $($comparison.restMetrics.successRate.ToString('F2'))%"
    Write-Host "  Avg Response Time:  $($comparison.restMetrics.averageResponseTimeMs.ToString('F2')) ms"
    Write-Host "  P95 Response Time:  $($comparison.restMetrics.p95ResponseTimeMs.ToString('F2')) ms"
    Write-Host "  Avg Payload:        $($comparison.restMetrics.averageResponseSizeBytes.ToString('F0')) bytes"
    Write-Host "  Throughput:         $($comparison.restMetrics.requestsPerSecond.ToString('F2')) req/s"

    Write-Host "`nGraphQL API - GET ALL ORDERS:" -ForegroundColor Magenta
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
Write-Host "Get All Orders load testing completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  - Tested GET ALL orders ($Iterations iterations per API)" -ForegroundColor White
Write-Host "  - Total requests: $($Iterations * 2)" -ForegroundColor White
Write-Host ""
