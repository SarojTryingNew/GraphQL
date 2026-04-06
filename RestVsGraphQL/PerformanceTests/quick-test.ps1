# Quick test of the load test script with fewer iterations
param(
    [string]$BaseUrl = "http://localhost:5072"
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "     Quick Test - Comprehensive REST vs GraphQL Testing" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test Plan:" -ForegroundColor Yellow
Write-Host "  - Standard Tests: 10 iterations per scenario" -ForegroundColor White
Write-Host "  - Bulk Operations: 10 iterations × 10 orders = 100 orders" -ForegroundColor White
Write-Host ""

# Get the script directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

# Run standard load tests
Write-Host "PART 1: Standard Load Tests" -ForegroundColor Magenta
Write-Host "=" * 60 -ForegroundColor Magenta
& "$scriptPath\load-test.ps1" -BaseUrl $BaseUrl -Iterations 10

# Run bulk operations
Write-Host ""
Write-Host "PART 2: Bulk Operations Tests" -ForegroundColor Magenta
Write-Host "=" * 60 -ForegroundColor Magenta
& "$scriptPath\load-test-bulk.ps1" -BaseUrl $BaseUrl -Iterations 10 -OrdersPerBulk 10

Write-Host ""
Write-Host "================================================================" -ForegroundColor Green
Write-Host "     Quick Test Completed!" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  [OK] Standard tests: 10 iterations (Test 4: REST 30 calls vs GraphQL 10 calls)" -ForegroundColor White
Write-Host "  [OK] Bulk operations: 100 orders created/updated" -ForegroundColor White
Write-Host ""
Write-Host "For more comprehensive results, run the standard load test:" -ForegroundColor Cyan
Write-Host "  Option 2 from main menu (100 iterations + 500 bulk orders)" -ForegroundColor White
