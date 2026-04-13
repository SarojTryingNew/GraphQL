# REST Direct vs REST+GraphQL - Bulk Operations Only Test Suite
param(
    [string]$BaseUrl = "http://localhost:5072"
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "   BULK OPERATIONS - Customizable Test Suite" -ForegroundColor Cyan
Write-Host "   REST Direct vs REST+GraphQL Backend Comparison" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if API is running (once at start)
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

# Function to get configuration for an operation
function Get-OperationConfig {
    param([string]$OperationName, [string]$Color)
    
    Write-Host ""
    Write-Host "CONFIGURE $OperationName OPERATION:" -ForegroundColor $Color
    Write-Host "  1. Quick        (50 iterations x 10 records  = 500 total)" -ForegroundColor Green
    Write-Host "  2. Standard     (100 iterations x 10 records = 1,000 total)" -ForegroundColor Cyan
    Write-Host "  3. Load Test    (100 iterations x 20 records = 2,000 total)" -ForegroundColor Yellow
    Write-Host "  4. Heavy Load   (200 iterations x 30 records = 6,000 total)" -ForegroundColor Magenta
    Write-Host "  5. Custom" -ForegroundColor White
    Write-Host ""
    
    $choice = Read-Host "  Select configuration for $OperationName (1-5)"
    
    switch ($choice) {
        "1" {
            Write-Host "  Selected: Quick (50 x 10)" -ForegroundColor Green
            return @{ Iterations = 50; OrdersPerBulk = 10 }
        }
        "2" {
            Write-Host "  Selected: Standard (100 x 10)" -ForegroundColor Cyan
            return @{ Iterations = 100; OrdersPerBulk = 10 }
        }
        "3" {
            Write-Host "  Selected: Load Test (100 x 20)" -ForegroundColor Yellow
            return @{ Iterations = 100; OrdersPerBulk = 20 }
        }
        "4" {
            Write-Host "  Selected: Heavy Load (200 x 30)" -ForegroundColor Magenta
            return @{ Iterations = 200; OrdersPerBulk = 30 }
        }
        "5" {
            $iters = [int](Read-Host "  Enter iterations")
            $records = [int](Read-Host "  Enter records per bulk")
            Write-Host "  Selected: Custom ($iters x $records)" -ForegroundColor White
            return @{ Iterations = $iters; OrdersPerBulk = $records }
        }
        default {
            Write-Host "  Invalid. Using Standard (100 x 10)" -ForegroundColor Yellow
            return @{ Iterations = 100; OrdersPerBulk = 10 }
        }
    }
}

# Main loop
while ($true) {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "SELECT OPERATIONS TO RUN:" -ForegroundColor Yellow
    Write-Host "  Enter operation numbers separated by commas (e.g., 1,2,4)" -ForegroundColor White
    Write-Host ""
    Write-Host "  1. CREATE   - Bulk create orders" -ForegroundColor Green
    Write-Host "  2. UPDATE   - Bulk update orders" -ForegroundColor Blue
    Write-Host "  3. DELETE   - Bulk delete orders" -ForegroundColor Red
    Write-Host "  4. GET      - Bulk get orders" -ForegroundColor Cyan
    Write-Host "  5. ALL      - Run all operations" -ForegroundColor Magenta
    Write-Host "  0. EXIT     - Quit the test suite" -ForegroundColor Gray
    Write-Host ""

    $selection = Read-Host "Select operations (0-5 or multiple like 1,3)"
    
    # Check for exit
    if ($selection -match "^0$|^exit$|^quit$") {
        Write-Host ""
        Write-Host "Exiting test suite. Goodbye!" -ForegroundColor Yellow
        break
    }
    
    $selectedOps = @()

    if ($selection -match "5|all") {
        $selectedOps = @("CREATE", "UPDATE", "DELETE", "GET")
        Write-Host "Selected: ALL operations" -ForegroundColor Magenta
    } else {
        $numbers = $selection -split "," | ForEach-Object { $_.Trim() }
        foreach ($num in $numbers) {
            switch ($num) {
                "1" { $selectedOps += "CREATE"; Write-Host "Selected: CREATE" -ForegroundColor Green }
                "2" { $selectedOps += "UPDATE"; Write-Host "Selected: UPDATE" -ForegroundColor Blue }
                "3" { $selectedOps += "DELETE"; Write-Host "Selected: DELETE" -ForegroundColor Red }
                "4" { $selectedOps += "GET"; Write-Host "Selected: GET" -ForegroundColor Cyan }
            }
        }
    }

    if ($selectedOps.Count -eq 0) {
        Write-Host "No valid operations selected. Please try again." -ForegroundColor Red
        continue
    }

    # Get configuration for each selected operation
    $configs = @{}
    if ($selectedOps -contains "CREATE") {
        $configs["CREATE"] = Get-OperationConfig -OperationName "CREATE" -Color "Green"
    }
    if ($selectedOps -contains "UPDATE") {
        $configs["UPDATE"] = Get-OperationConfig -OperationName "UPDATE" -Color "Blue"
    }
    if ($selectedOps -contains "DELETE") {
        $configs["DELETE"] = Get-OperationConfig -OperationName "DELETE" -Color "Red"
    }
    # GET operation doesn't need configuration - it just gets all data
    if ($selectedOps -contains "GET") {
        $configs["GET"] = @{ Iterations = 1; OrdersPerBulk = "All" }
    }

    Write-Host ""
    Write-Host "Test Configuration Summary:" -ForegroundColor Yellow
    Write-Host "  Base URL: $BaseUrl"
    $totalRecords = 0
    foreach ($op in $selectedOps) {
        $cfg = $configs[$op]
        $color = switch ($op) {
            "CREATE" { "Green" }
            "UPDATE" { "Blue" }
            "DELETE" { "Red" }
            "GET"    { "Cyan" }
        }
        if ($op -eq "GET") {
            Write-Host "  ${op}:".PadRight(10) "Get all available orders (no iterations)" -ForegroundColor $color
        } else {
            $total = $cfg.Iterations * $cfg.OrdersPerBulk
            $totalRecords += $total
            Write-Host "  ${op}:".PadRight(10) "$($cfg.Iterations) iterations x $($cfg.OrdersPerBulk) records = $total total" -ForegroundColor $color
        }
    }
    if ($totalRecords -gt 0) {
        Write-Host "  TOTAL:    $totalRecords records across all operations (excluding GET)" -ForegroundColor Cyan
    }
    Write-Host ""

    # Reset metrics
    Write-Host "Resetting metrics..." -ForegroundColor Cyan
    try {
        $scenarioParts = @()
        foreach ($op in $selectedOps) {
            if ($op -eq "GET") {
                $scenarioParts += "GET(All)"
            } else {
                $cfg = $configs[$op]
                $scenarioParts += "$op($($cfg.Iterations)x$($cfg.OrdersPerBulk))"
            }
        }
        $scenarioName = "Bulk Ops - " + ($scenarioParts -join " ")
        $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
        Invoke-RestMethod -Uri "$BaseUrl/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
        Invoke-RestMethod -Uri "$BaseUrl/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
        Write-Host "Metrics reset complete" -ForegroundColor Green
    } catch {
        Write-Host "Warning: Could not reset metrics" -ForegroundColor Yellow
    }

    Write-Host ""

    function Invoke-BulkTest {
    param([string]$Name, [scriptblock]$Request, [int]$Count, [int]$BulkSize, [string]$Color = "Cyan")
    
    Write-Host "Testing: $Name" -ForegroundColor $Color
    $successCount = 0
    $errorCount = 0
    
    for ($i = 1; $i -le $Count; $i++) {
        try {
            $null = & $Request -url $BaseUrl -ordersCount $BulkSize
            $successCount++
        }
        catch {
            $errorCount++
            if ($i -le 3) { Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red }
        }
        if ($i % 5 -eq 0) { Write-Progress -Activity $Name -Status "$i of $Count" -PercentComplete (($i/$Count)*100) }
    }
    
    Write-Progress -Activity $Name -Completed
    Write-Host "  Result: $successCount/$Count successful ($($successCount * $BulkSize) records)" -ForegroundColor $(if($successCount -eq $Count){"Green"}else{"Yellow"})
    Write-Host ""
}

    # TEST 1: BULK CREATE
    if ($selectedOps -contains "CREATE") {
        $cfg = $configs["CREATE"]
        Write-Host "================================================================" -ForegroundColor Green
        Write-Host "TEST: BULK CREATE ($($cfg.Iterations) x $($cfg.OrdersPerBulk))" -ForegroundColor Green
        Write-Host "================================================================" -ForegroundColor Green

        Invoke-BulkTest -Name "REST Direct - CREATE" -Count $cfg.Iterations -BulkSize $cfg.OrdersPerBulk -Color "Green" -Request {
            param($url, $ordersCount)
        $orders = @()
        for ($i = 1; $i -le $ordersCount; $i++) {
            $orders += @{
                customerId = (($i % 5) + 1)
                status = "Pending"
                items = @(@{
                    productId = (($i % 8) + 1)
                    quantity = [int](Get-Random -Minimum 1 -Maximum 5)
                    discount = [int](Get-Random -Minimum 0 -Maximum 20)
                    notes = @("Bulk test")
                })
            }
        }
        Invoke-RestMethod -Uri "$url/api/orders/bulk" -Method Post -Body (@{orders=$orders}|ConvertTo-Json -Depth 10) -ContentType "application/json"
    }

    Invoke-BulkTest -Name "REST+GraphQL - CREATE" -Count $cfg.Iterations -BulkSize $cfg.OrdersPerBulk -Color "Magenta" -Request {
        param($url, $ordersCount)
        $orders = @()
        for ($i = 1; $i -le $ordersCount; $i++) {
            $orders += @{
                customerId = (($i % 5) + 1)
                status = "Pending"
                items = @(@{
                    productId = (($i % 8) + 1)
                    quantity = [int](Get-Random -Minimum 1 -Maximum 5)
                    discount = [int](Get-Random -Minimum 0 -Maximum 20)
                    notes = @("Bulk test")
                })
            }
        }
        Invoke-RestMethod -Uri "$url/api/graphql-backend/orders/bulk" -Method Post -Body (@{orders=$orders}|ConvertTo-Json -Depth 10) -ContentType "application/json"
    }
}

    # TEST 2: BULK UPDATE
    if ($selectedOps -contains "UPDATE") {
        $cfg = $configs["UPDATE"]
        Write-Host "================================================================" -ForegroundColor Blue
        Write-Host "TEST: BULK UPDATE ($($cfg.Iterations) x $($cfg.OrdersPerBulk))" -ForegroundColor Blue
        Write-Host "================================================================" -ForegroundColor Blue

        Invoke-BulkTest -Name "REST Direct - UPDATE" -Count $cfg.Iterations -BulkSize $cfg.OrdersPerBulk -Color "Blue" -Request {
        param($url, $ordersCount)
        $allOrders = Invoke-RestMethod -Uri "$url/api/orders" -Method Get
        $ordersToUpdate = $allOrders | Select-Object -First $ordersCount
        $updates = @()
        foreach ($order in $ordersToUpdate) {
            $updates += @{
                id = $order.id
                customerId = $order.customerId
                status = "Processing"
                items = $order.items
            }
        }
        if ($updates.Count -eq 0) { throw "No orders to update" }
        Invoke-RestMethod -Uri "$url/api/orders/bulk" -Method Put -Body (@{orders=$updates}|ConvertTo-Json -Depth 10) -ContentType "application/json"
    }

        Invoke-BulkTest -Name "REST+GraphQL - UPDATE" -Count $cfg.Iterations -BulkSize $cfg.OrdersPerBulk -Color "Magenta" -Request {
        param($url, $ordersCount)
        $allOrders = Invoke-RestMethod -Uri "$url/api/graphql-backend/orders" -Method Get
        $ordersToUpdate = $allOrders | Select-Object -First $ordersCount
        $updates = @()
        foreach ($order in $ordersToUpdate) {
            $updates += @{
                id = $order.id
                customerId = $order.customerId
                status = "Processing"
                items = $order.items
            }
        }
        if ($updates.Count -eq 0) { throw "No orders to update" }
        Invoke-RestMethod -Uri "$url/api/graphql-backend/orders/bulk" -Method Put -Body (@{orders=$updates}|ConvertTo-Json -Depth 10) -ContentType "application/json"
    }
    }

    # TEST 3: BULK DELETE
    if ($selectedOps -contains "DELETE") {
        $cfg = $configs["DELETE"]
            Write-Host "================================================================" -ForegroundColor Red
        Write-Host "TEST: BULK DELETE ($($cfg.Iterations) x $($cfg.OrdersPerBulk))" -ForegroundColor Red
        Write-Host "================================================================" -ForegroundColor Red

        Invoke-BulkTest -Name "REST Direct - DELETE" -Count $cfg.Iterations -BulkSize $cfg.OrdersPerBulk -Color "Red" -Request {
        param($url, $ordersCount)
        $allOrders = Invoke-RestMethod -Uri "$url/api/orders" -Method Get
        $orderIds = $allOrders | Select-Object -First $ordersCount | ForEach-Object { $_.id }
        if ($orderIds.Count -eq 0) { throw "No orders to delete" }
        Invoke-RestMethod -Uri "$url/api/orders/bulk" -Method Delete -Body (@{orderIds=@($orderIds)}|ConvertTo-Json) -ContentType "application/json"
    }

        Invoke-BulkTest -Name "REST+GraphQL - DELETE" -Count $cfg.Iterations -BulkSize $cfg.OrdersPerBulk -Color "Magenta" -Request {
        param($url, $ordersCount)
        $allOrders = Invoke-RestMethod -Uri "$url/api/graphql-backend/orders" -Method Get
        $orderIds = $allOrders | Select-Object -First $ordersCount | ForEach-Object { $_.id }
        if ($orderIds.Count -eq 0) { throw "No orders to delete" }
        Invoke-RestMethod -Uri "$url/api/graphql-backend/orders/bulk" -Method Delete -Body (@{orderIds=@($orderIds)}|ConvertTo-Json) -ContentType "application/json"
    }
    }

    # TEST 4: GET ALL ORDERS
    if ($selectedOps -contains "GET") {
        Write-Host "================================================================" -ForegroundColor Cyan
        Write-Host "TEST: GET ALL ORDERS" -ForegroundColor Cyan
        Write-Host "================================================================" -ForegroundColor Cyan

        # REST Direct - Get All Orders
        Write-Host "[REST Direct - GET] ".PadRight(35) -NoNewline -ForegroundColor Cyan
        try {
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            $restDirectOrders = Invoke-RestMethod -Uri "$BaseUrl/api/orders" -Method Get
            $sw.Stop()
            $count = $restDirectOrders.Count
            Write-Host "SUCCESS - Retrieved $count orders in $($sw.ElapsedMilliseconds)ms" -ForegroundColor Green
        } catch {
            Write-Host "FAILED - $($_.Exception.Message)" -ForegroundColor Red
        }

        # REST+GraphQL - Get All Orders
        Write-Host "[REST+GraphQL - GET] ".PadRight(35) -NoNewline -ForegroundColor Magenta
        try {
            $sw = [System.Diagnostics.Stopwatch]::StartNew()
            $restGraphQLOrders = Invoke-RestMethod -Uri "$BaseUrl/api/graphql-backend/orders" -Method Get
            $sw.Stop()
            $count = $restGraphQLOrders.Count
            Write-Host "SUCCESS - Retrieved $count orders in $($sw.ElapsedMilliseconds)ms" -ForegroundColor Green
        } catch {
            Write-Host "FAILED - $($_.Exception.Message)" -ForegroundColor Red
        }

        Write-Host ""
    }

    # RESULTS
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "Collecting metrics..." -ForegroundColor Yellow
    Write-Host "================================================================" -ForegroundColor Cyan
    Start-Sleep -Seconds 2

    try {
    $comparison = Invoke-RestMethod -Uri "$BaseUrl/api/metrics/comparison" -Method Get

    Write-Host ""
    Write-Host "PERFORMANCE RESULTS" -ForegroundColor Green
        Write-Host "===================" -ForegroundColor Green
        Write-Host ""
        Write-Host "WINNERS:" -ForegroundColor Yellow
        Write-Host "  Response Time: $($comparison.responseTimeWinner)" -ForegroundColor Green
        Write-Host "  Payload Size:  $($comparison.payloadSizeWinner)" -ForegroundColor Green
        Write-Host "  Throughput:    $($comparison.throughputWinner)" -ForegroundColor Green
        
        $rtImp = [Math]::Abs($comparison.responseTimeImprovement)
        $psImp = [Math]::Abs($comparison.payloadSizeImprovement)
        
        Write-Host ""
        Write-Host "IMPROVEMENTS:" -ForegroundColor Yellow
        Write-Host "  Response Time: $($rtImp.ToString('F2'))% $(if($comparison.responseTimeImprovement -gt 0){'faster'}else{'slower'})"
        Write-Host "  Payload Size:  $($psImp.ToString('F2'))% $(if($comparison.payloadSizeImprovement -gt 0){'smaller'}else{'larger'})"
        
        Write-Host ""
        Write-Host "REST Direct:" -ForegroundColor Red
        Write-Host "  Requests:    $($comparison.restDirectMetrics.totalRequests)"
        Write-Host "  Success:     $($comparison.restDirectMetrics.successRate.ToString('F1'))%"
        Write-Host "  Avg Time:    $($comparison.restDirectMetrics.averageResponseTimeMs.ToString('F2')) ms"
        Write-Host "  P95 Time:    $($comparison.restDirectMetrics.p95ResponseTimeMs.ToString('F2')) ms"
        Write-Host "  Avg Payload: $([int]$comparison.restDirectMetrics.averageResponseSizeBytes) bytes"
        Write-Host "  Throughput:  $($comparison.restDirectMetrics.requestsPerSecond.ToString('F2')) req/s"
        
        Write-Host ""
        Write-Host "REST+GraphQL:" -ForegroundColor Magenta
        Write-Host "  Requests:    $($comparison.restWithGraphQLMetrics.totalRequests)"
        Write-Host "  Success:     $($comparison.restWithGraphQLMetrics.successRate.ToString('F1'))%"
        Write-Host "  Avg Time:    $($comparison.restWithGraphQLMetrics.averageResponseTimeMs.ToString('F2')) ms"
        Write-Host "  P95 Time:    $($comparison.restWithGraphQLMetrics.p95ResponseTimeMs.ToString('F2')) ms"
        Write-Host "  Avg Payload: $([int]$comparison.restWithGraphQLMetrics.averageResponseSizeBytes) bytes"
        Write-Host "  Throughput:  $($comparison.restWithGraphQLMetrics.requestsPerSecond.ToString('F2')) req/s"
        
        Write-Host ""
        $reportUrl = "$BaseUrl/api/metrics/report"
        Write-Host "Full report: $reportUrl" -ForegroundColor Cyan
        Write-Host "Opening report in browser..." -ForegroundColor Yellow
        Start-Process $reportUrl
    }
    catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "COMPLETED!" -ForegroundColor Green
    $summaryParts = @()
    foreach ($op in $selectedOps) {
        $cfg = $configs[$op]
        $summaryParts += "$op($($cfg.Iterations)x$($cfg.OrdersPerBulk))"
    }
    Write-Host "Operations executed: $($summaryParts -join ', ')" -ForegroundColor Cyan
    Write-Host "Total records: $totalRecords" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Press any key to return to menu..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

Write-Host ""
Write-Host "Test suite ended." -ForegroundColor Cyan
