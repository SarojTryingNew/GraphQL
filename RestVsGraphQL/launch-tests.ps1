# REST vs GraphQL - Performance Test Launcher
param([string]$TestType = "menu")
$performanceTestsPath = Join-Path $PSScriptRoot "PerformanceTests"

function Show-Menu {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "     REST vs GraphQL - Performance Test Launcher" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  COMPREHENSIVE TESTS:" -ForegroundColor Yellow
    Write-Host "  1. Quick Test (10 iterations + 100 bulk orders)" -ForegroundColor White
    Write-Host "  2. Standard Load Test (100 iterations + 500 bulk orders)" -ForegroundColor White
    Write-Host ""
    Write-Host "  INDIVIDUAL SCENARIO TESTS:" -ForegroundColor Yellow
    Write-Host "  3. Bulk Create Operations Test" -ForegroundColor Green
    Write-Host "  4. Bulk Update Operations Test" -ForegroundColor Yellow
    Write-Host "  5. Bulk Delete Operations Test" -ForegroundColor Red
    Write-Host "  6. Get All Orders Test" -ForegroundColor Cyan
    Write-Host "  7. Nested Object Graph Test" -ForegroundColor White
    Write-Host "  8. Dashboard Aggregation Test" -ForegroundColor White
    Write-Host "  9. Multiple Dependent Calls Test" -ForegroundColor White
    Write-Host ""
    Write-Host "  0. Exit" -ForegroundColor White
    Write-Host ""
}

function Open-MetricsReport {
    Write-Host ""
    Write-Host "Opening metrics report in browser..." -ForegroundColor Green
    Start-Process "http://localhost:5072/api/metrics/report"
    Start-Sleep -Seconds 2
    Write-Host ""
    Write-Host "Press any key to return to menu..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

$keepRunning = $true
while ($keepRunning) {
    Show-Menu
    $choice = Read-Host "Enter your choice"

    switch ($choice) {
        "1" { 
            Write-Host ""
            # Set scenario before running test
            Write-Host "Preparing test..." -ForegroundColor Cyan
            try {
                $scenarioName = "Quick Test (10 iterations + 100 bulk orders)"
                $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
            } catch {
                Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
            }
            Write-Host ""

            & "$performanceTestsPath\quick-test.ps1"
            Open-MetricsReport
        }
        "2" {
            Write-Host ""
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host "     Standard Load Test - Comprehensive Testing" -ForegroundColor Cyan
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Test Plan:" -ForegroundColor Yellow
            Write-Host "  - Standard Tests: 100 iterations per scenario" -ForegroundColor White
            Write-Host "  - Bulk Operations: 10 iterations × 50 orders = 500 orders" -ForegroundColor White
            Write-Host ""

            # Set scenario before running tests
            Write-Host "Preparing test..." -ForegroundColor Cyan
            try {
                $scenarioName = "Standard Load Test (100 iterations + 500 bulk orders)"
                $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
            } catch {
                Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
            }
            Write-Host ""

            # Run standard load tests
            Write-Host "PART 1: Standard Load Tests" -ForegroundColor Magenta
            Write-Host "=" * 60 -ForegroundColor Magenta
            & "$performanceTestsPath\load-test.ps1" -Iterations 100

            # Run bulk operations
            Write-Host ""
            Write-Host "PART 2: Bulk Operations Tests" -ForegroundColor Magenta
            Write-Host "=" * 60 -ForegroundColor Magenta
            & "$performanceTestsPath\load-test-bulk.ps1" -Iterations 10 -OrdersPerBulk 50

            Write-Host ""
            Write-Host "================================================================" -ForegroundColor Green
            Write-Host "     Standard Load Test Completed!" -ForegroundColor Green
            Write-Host "================================================================" -ForegroundColor Green
            Write-Host ""
            Write-Host "Summary:" -ForegroundColor Yellow
            Write-Host "  [OK] Standard tests: 100 iterations (Test 4: REST 300 calls vs GraphQL 100 calls)" -ForegroundColor White
            Write-Host "  [OK] Bulk operations: 500 orders created/updated" -ForegroundColor White
            Write-Host ""

            Open-MetricsReport
        }
        "3" {
            Write-Host ""
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host "     Bulk CREATE Operations Test - Configuration" -ForegroundColor Cyan
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  1. Quick Test       (10 iterations × 10 orders = 100 orders)" -ForegroundColor Green
            Write-Host "  2. Standard Test    (50 iterations × 10 orders = 500 orders)" -ForegroundColor Yellow
            Write-Host "  3. Heavy Load Test  (50 iterations × 20 orders = 1,000 orders)" -ForegroundColor Magenta
            Write-Host "  4. Stress Test      (200 iterations × 50 orders = 10,000 orders)" -ForegroundColor Red
            Write-Host "  5. Custom Configuration" -ForegroundColor Cyan
            Write-Host "  0. Back to Main Menu" -ForegroundColor White
            Write-Host ""

            $bulkChoice = Read-Host "Select test type"

            $iterations = 0
            $ordersPerBulk = 0

            switch ($bulkChoice) {
                "1" { 
                    $iterations = 10
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Quick Bulk CREATE Test" -ForegroundColor Green
                }
                "2" { 
                    $iterations = 50
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Standard Bulk CREATE Test" -ForegroundColor Yellow
                }
                "3" { 
                    $iterations = 50
                    $ordersPerBulk = 20
                    Write-Host "`nSelected: Heavy Load Bulk CREATE Test" -ForegroundColor Magenta
                }
                "4" { 
                    $iterations = 200
                    $ordersPerBulk = 50
                    Write-Host "`nSelected: Stress Test" -ForegroundColor Red
                    Write-Host "Warning: This will create 10,000 orders and may take several minutes!" -ForegroundColor Yellow
                    $confirm = Read-Host "Continue? (Y/N)"
                    if ($confirm -ne "Y" -and $confirm -ne "y") {
                        Write-Host "Stress test cancelled." -ForegroundColor Yellow
                        Start-Sleep -Seconds 1
                        continue
                    }
                }
                "5" { 
                    Write-Host "`nCustom Configuration" -ForegroundColor Cyan
                    Write-Host ""
                    $iterations = Read-Host "Number of bulk iterations"
                    $iterations = [int]$iterations
                    $ordersPerBulk = Read-Host "Orders per bulk request"
                    $ordersPerBulk = [int]$ordersPerBulk
                }
                "0" { 
                    continue
                }
                default { 
                    Write-Host ""
                    Write-Host "Invalid choice. Returning to main menu." -ForegroundColor Red
                    Start-Sleep -Seconds 1
                    continue
                }
            }

            if ($iterations -gt 0 -and $ordersPerBulk -gt 0) {
                Write-Host ""
                Write-Host "Configuration:" -ForegroundColor Yellow
                Write-Host "  Iterations:      $iterations" -ForegroundColor White
                Write-Host "  Orders per bulk: $ordersPerBulk" -ForegroundColor White
                Write-Host "  Total orders:    $($iterations * $ordersPerBulk)" -ForegroundColor White
                Write-Host ""

                # Set scenario before running test
                Write-Host "Preparing test..." -ForegroundColor Cyan
                try {
                    $scenarioName = "Bulk CREATE Operations ($iterations iterations × $ordersPerBulk orders)"
                    $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                    Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                    Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                    Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
                } catch {
                    Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
                }
                Write-Host ""

                & "$performanceTestsPath\load-test-bulk-create.ps1" -Iterations $iterations -OrdersPerBulk $ordersPerBulk
                Open-MetricsReport
            }
        }
        "4" {
            Write-Host ""
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host "     Bulk UPDATE Operations Test - Configuration" -ForegroundColor Cyan
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  1. Quick Test       (10 iterations × 10 orders = 100 orders)" -ForegroundColor Green
            Write-Host "  2. Standard Test    (25 iterations × 10 orders = 250 orders)" -ForegroundColor Yellow
            Write-Host "  3. Heavy Load Test  (50 iterations × 20 orders = 1,000 orders)" -ForegroundColor Magenta
            Write-Host "  4. Custom Configuration" -ForegroundColor Cyan
            Write-Host "  0. Back to Main Menu" -ForegroundColor White
            Write-Host ""

            $bulkChoice = Read-Host "Select test type"

            $iterations = 0
            $ordersPerBulk = 0

            switch ($bulkChoice) {
                "1" { 
                    $iterations = 10
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Quick Bulk UPDATE Test" -ForegroundColor Green
                }
                "2" { 
                    $iterations = 25
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Standard Bulk UPDATE Test" -ForegroundColor Yellow
                }
                "3" { 
                    $iterations = 50
                    $ordersPerBulk = 20
                    Write-Host "`nSelected: Heavy Load Bulk UPDATE Test" -ForegroundColor Magenta
                }
                "4" { 
                    Write-Host "`nCustom Configuration" -ForegroundColor Cyan
                    Write-Host ""
                    $iterations = Read-Host "Number of bulk iterations"
                    $iterations = [int]$iterations
                    $ordersPerBulk = Read-Host "Orders per bulk request"
                    $ordersPerBulk = [int]$ordersPerBulk
                }
                "0" { 
                    continue
                }
                default { 
                    Write-Host ""
                    Write-Host "Invalid choice. Returning to main menu." -ForegroundColor Red
                    Start-Sleep -Seconds 1
                    continue
                }
            }

            if ($iterations -gt 0 -and $ordersPerBulk -gt 0) {
                Write-Host ""
                Write-Host "Configuration:" -ForegroundColor Yellow
                Write-Host "  Iterations:      $iterations" -ForegroundColor White
                Write-Host "  Orders per bulk: $ordersPerBulk" -ForegroundColor White
                Write-Host "  Total orders:    $($iterations * $ordersPerBulk)" -ForegroundColor White
                Write-Host ""

                # Set scenario before running test
                Write-Host "Preparing test..." -ForegroundColor Cyan
                try {
                    $scenarioName = "Bulk UPDATE Operations ($iterations iterations × $ordersPerBulk orders)"
                    $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                    Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                    Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                    Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
                } catch {
                    Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
                }
                Write-Host ""

                & "$performanceTestsPath\load-test-bulk-update.ps1" -Iterations $iterations -OrdersPerBulk $ordersPerBulk
                Open-MetricsReport
            }
        }
        "5" {
            Write-Host ""
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host "     Bulk DELETE Operations Test - Configuration" -ForegroundColor Cyan
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  1. Quick Test       (10 iterations × 10 orders = 100 orders)" -ForegroundColor Green
            Write-Host "  2. Standard Test    (16 iterations × 10 orders = 160 orders)" -ForegroundColor Yellow
            Write-Host "  3. Heavy Load Test  (25 iterations × 20 orders = 500 orders)" -ForegroundColor Magenta
            Write-Host "  4. Custom Configuration" -ForegroundColor Cyan
            Write-Host "  0. Back to Main Menu" -ForegroundColor White
            Write-Host ""

            $bulkChoice = Read-Host "Select test type"

            $iterations = 0
            $ordersPerBulk = 0

            switch ($bulkChoice) {
                "1" { 
                    $iterations = 10
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Quick Bulk DELETE Test" -ForegroundColor Green
                }
                "2" { 
                    $iterations = 16
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Standard Bulk DELETE Test" -ForegroundColor Yellow
                }
                "3" { 
                    $iterations = 25
                    $ordersPerBulk = 20
                    Write-Host "`nSelected: Heavy Load Bulk DELETE Test" -ForegroundColor Magenta
                }
                "4" { 
                    Write-Host "`nCustom Configuration" -ForegroundColor Cyan
                    Write-Host ""
                    $iterations = Read-Host "Number of bulk iterations"
                    $iterations = [int]$iterations
                    $ordersPerBulk = Read-Host "Orders per bulk request"
                    $ordersPerBulk = [int]$ordersPerBulk
                }
                "0" { 
                    continue
                }
                default { 
                    Write-Host ""
                    Write-Host "Invalid choice. Returning to main menu." -ForegroundColor Red
                    Start-Sleep -Seconds 1
                    continue
                }
            }

            if ($iterations -gt 0 -and $ordersPerBulk -gt 0) {
                Write-Host ""
                Write-Host "Configuration:" -ForegroundColor Yellow
                Write-Host "  Iterations:      $iterations" -ForegroundColor White
                Write-Host "  Orders per bulk: $ordersPerBulk" -ForegroundColor White
                Write-Host "  Total orders:    $($iterations * $ordersPerBulk)" -ForegroundColor White
                Write-Host ""

                # Set scenario before running test
                Write-Host "Preparing test..." -ForegroundColor Cyan
                try {
                    $scenarioName = "Bulk DELETE Operations ($iterations iterations × $ordersPerBulk orders)"
                    $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                    Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                    Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                    Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
                } catch {
                    Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
                }
                Write-Host ""

                & "$performanceTestsPath\load-test-bulk-delete.ps1" -Iterations $iterations -OrdersPerBulk $ordersPerBulk
                Open-MetricsReport
            }
        }
        "6" {
            Write-Host ""
            Write-Host "Enter number of iterations for Get All Orders Test (default: 10):" -ForegroundColor Cyan
            $iterations = Read-Host "Iterations"
            if ([string]::IsNullOrWhiteSpace($iterations)) { $iterations = 10 }
            else { $iterations = [int]$iterations }

            # Set scenario before running test
            Write-Host ""
            Write-Host "Preparing test..." -ForegroundColor Cyan
            try {
                $scenarioName = "Get All Orders Test ($iterations iterations)"
                $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
            } catch {
                Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
            }
            Write-Host ""

            & "$performanceTestsPath\load-test-bulk-get.ps1" -Iterations $iterations
            Open-MetricsReport
        }
        "7" {
            Write-Host ""
            Write-Host "Enter number of iterations for Nested Object Graph Test (default: 100):" -ForegroundColor Cyan
            $iterations = Read-Host "Iterations"
            if ([string]::IsNullOrWhiteSpace($iterations)) { $iterations = 100 }
            else { $iterations = [int]$iterations }

            # Set scenario before running test
            Write-Host ""
            Write-Host "Preparing test..." -ForegroundColor Cyan
            try {
                $scenarioName = "Nested Object Graph Test ($iterations iterations)"
                $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
            } catch {
                Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
            }

            Write-Host ""
            & "$performanceTestsPath\load-test-nested.ps1" -Iterations $iterations
            Open-MetricsReport
        }
        "8" {
            Write-Host ""
            Write-Host "Enter number of iterations for Dashboard Aggregation Test (default: 100):" -ForegroundColor Cyan
            $iterations = Read-Host "Iterations"
            if ([string]::IsNullOrWhiteSpace($iterations)) { $iterations = 100 }
            else { $iterations = [int]$iterations }

            # Set scenario before running test
            Write-Host ""
            Write-Host "Preparing test..." -ForegroundColor Cyan
            try {
                $scenarioName = "Dashboard Aggregation Test ($iterations iterations)"
                $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
            } catch {
                Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
            }

            Write-Host ""
            & "$performanceTestsPath\load-test-dashboard.ps1" -Iterations $iterations
            Open-MetricsReport
        }
        "9" {
            Write-Host ""
            Write-Host "Enter number of user iterations for Multiple Calls Test (default: 100):" -ForegroundColor Cyan
            Write-Host "  REST will make 3× HTTP calls, GraphQL will make 1× HTTP calls" -ForegroundColor Gray
            $iterations = Read-Host "Iterations"
            if ([string]::IsNullOrWhiteSpace($iterations)) { $iterations = 100 }
            else { $iterations = [int]$iterations }

            # Set scenario before running test
            Write-Host ""
            Write-Host "Preparing test..." -ForegroundColor Cyan
            try {
                $scenarioName = "Multiple Dependent Calls Test ($iterations iterations)"
                $scenarioBody = @{ scenarioName = $scenarioName } | ConvertTo-Json
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
                Invoke-RestMethod -Uri "http://localhost:5072/api/metrics/scenario" -Method Post -Body $scenarioBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null
                Write-Host "Test scenario set: $scenarioName" -ForegroundColor Green
            } catch {
                Write-Host "Warning: Could not set scenario" -ForegroundColor Yellow
            }

            Write-Host ""
            & "$performanceTestsPath\load-test-multiple.ps1" -Iterations $iterations
            Open-MetricsReport
        }
        "0" {
            Write-Host ""
            Write-Host "Goodbye!" -ForegroundColor Green
            $keepRunning = $false
        }
        default { 
            Write-Host ""
            Write-Host "Invalid choice. Please try again." -ForegroundColor Red
            Start-Sleep -Seconds 1
        }
    }
}

Write-Host "Thank you for using the Performance Test Launcher!" -ForegroundColor Cyan