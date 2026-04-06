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
    Write-Host "  3. Bulk Operations Test" -ForegroundColor White
    Write-Host "  4. Nested Object Graph Test" -ForegroundColor White
    Write-Host "  5. Dashboard Aggregation Test" -ForegroundColor White
    Write-Host "  6. Multiple Dependent Calls Test" -ForegroundColor White
    Write-Host ""
    Write-Host "  0. Exit" -ForegroundColor White
    Write-Host ""
}

function Open-MetricsReport {
    Write-Host ""
    Write-Host "Opening metrics report in browser..." -ForegroundColor Green
    Start-Process "http://localhost:5072/api/metrics/report"
    Start-Sleep -Seconds 1
}

function Ask-Continue {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Yellow
    Write-Host "  R - Run another test" -ForegroundColor Green
    Write-Host "  E - Exit" -ForegroundColor Red
    Write-Host "================================================================" -ForegroundColor Yellow
    $continue = Read-Host "Your choice (R/E)"
    return $continue.ToUpper() -eq "R"
}

$keepRunning = $true
while ($keepRunning) {
    Show-Menu
    $choice = Read-Host "Enter your choice"

    switch ($choice) {
        "1" { 
            Write-Host ""
            & "$performanceTestsPath\quick-test.ps1"
            Open-MetricsReport
            $keepRunning = Ask-Continue
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
            $keepRunning = Ask-Continue
        }
        "3" { 
            Write-Host ""
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host "     Bulk Operations Test - Configuration" -ForegroundColor Cyan
            Write-Host "================================================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  1. Quick Bulk Test       (10 iterations × 10 orders = 100 orders)" -ForegroundColor Green
            Write-Host "  2. Standard Bulk Test    (50 iterations × 10 orders = 500 orders)" -ForegroundColor Yellow
            Write-Host "  3. Heavy Load Bulk Test  (50 iterations × 20 orders = 1,000 orders)" -ForegroundColor Magenta
            Write-Host "  4. Stress Test           (200 iterations × 50 orders = 10,000 orders)" -ForegroundColor Red
            Write-Host "  5. Custom Configuration  (specify your own values)" -ForegroundColor Cyan
            Write-Host "  0. Back to Main Menu" -ForegroundColor White
            Write-Host ""

            $bulkChoice = Read-Host "Select test type"

            $iterations = 0
            $ordersPerBulk = 0

            switch ($bulkChoice) {
                "1" { 
                    $iterations = 10
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Quick Bulk Test" -ForegroundColor Green
                }
                "2" { 
                    $iterations = 50
                    $ordersPerBulk = 10
                    Write-Host "`nSelected: Standard Bulk Test" -ForegroundColor Yellow
                }
                "3" { 
                    $iterations = 50
                    $ordersPerBulk = 20
                    Write-Host "`nSelected: Heavy Load Bulk Test" -ForegroundColor Magenta
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

                & "$performanceTestsPath\load-test-bulk.ps1" -Iterations $iterations -OrdersPerBulk $ordersPerBulk
                Open-MetricsReport
            }

            $keepRunning = Ask-Continue
        }
        "4" {
            Write-Host ""
            Write-Host "Enter number of iterations for Nested Object Graph Test (default: 100):" -ForegroundColor Cyan
            $iterations = Read-Host "Iterations"
            if ([string]::IsNullOrWhiteSpace($iterations)) { $iterations = 100 }
            else { $iterations = [int]$iterations }

            Write-Host ""
            & "$performanceTestsPath\load-test-nested.ps1" -Iterations $iterations
            Open-MetricsReport
            $keepRunning = Ask-Continue
        }
        "5" {
            Write-Host ""
            Write-Host "Enter number of iterations for Dashboard Aggregation Test (default: 100):" -ForegroundColor Cyan
            $iterations = Read-Host "Iterations"
            if ([string]::IsNullOrWhiteSpace($iterations)) { $iterations = 100 }
            else { $iterations = [int]$iterations }

            Write-Host ""
            & "$performanceTestsPath\load-test-dashboard.ps1" -Iterations $iterations
            Open-MetricsReport
            $keepRunning = Ask-Continue
        }
        "6" {
            Write-Host ""
            Write-Host "Enter number of user iterations for Multiple Calls Test (default: 100):" -ForegroundColor Cyan
            Write-Host "  REST will make 3× HTTP calls, GraphQL will make 1× HTTP calls" -ForegroundColor Gray
            $iterations = Read-Host "Iterations"
            if ([string]::IsNullOrWhiteSpace($iterations)) { $iterations = 100 }
            else { $iterations = [int]$iterations }

            Write-Host ""
            & "$performanceTestsPath\load-test-multiple.ps1" -Iterations $iterations
            Open-MetricsReport
            $keepRunning = Ask-Continue
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