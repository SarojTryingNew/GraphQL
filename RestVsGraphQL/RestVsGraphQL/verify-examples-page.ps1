# Quick verification script for Examples page
param(
    [string]$BaseUrl = "http://localhost:5072"
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "   VERIFY EXAMPLES PAGE - Quick Test" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if API is running
Write-Host "1. Checking if API is running at $BaseUrl..." -ForegroundColor Yellow
try {
    $null = Invoke-WebRequest -Uri "$BaseUrl/api/customers" -Method Get -TimeoutSec 5 -ErrorAction Stop
    Write-Host "   ✓ API is running!" -ForegroundColor Green
}
catch {
    Write-Host "   ✗ Cannot connect to API" -ForegroundColor Red
    Write-Host "   Please start the API with: dotnet run" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "2. Resetting metrics..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/metrics/reset" -Method Post -ErrorAction SilentlyContinue | Out-Null
    Write-Host "   ✓ Metrics reset" -ForegroundColor Green
}
catch {
    Write-Host "   ⚠ Could not reset metrics (continuing anyway)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "3. Making test requests..." -ForegroundColor Yellow

# Make a REST Direct GET request
try {
    $restDirect = Invoke-RestMethod -Uri "$BaseUrl/api/orders" -Method Get
    Write-Host "   ✓ REST Direct GET: Retrieved $($restDirect.Count) orders" -ForegroundColor Green
}
catch {
    Write-Host "   ✗ REST Direct GET failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Make a REST+GraphQL GET request
try {
    $restGraphQL = Invoke-RestMethod -Uri "$BaseUrl/api/graphql-backend/orders" -Method Get
    Write-Host "   ✓ REST+GraphQL GET: Retrieved $($restGraphQL.Count) orders" -ForegroundColor Green
}
catch {
    Write-Host "   ✗ REST+GraphQL GET failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Make a REST Direct POST request
try {
    $createBody = @{
        orders = @(
            @{
                customerId = 1
                status = "Pending"
                items = @(
                    @{
                        productId = 1
                        quantity = 2
                        discount = 10
                        notes = @("Test order")
                    }
                )
            }
        )
    } | ConvertTo-Json -Depth 10
    
    $createResult = Invoke-RestMethod -Uri "$BaseUrl/api/orders/bulk" -Method Post -Body $createBody -ContentType "application/json"
    Write-Host "   ✓ REST Direct POST: Created $($createResult.successCount) order(s)" -ForegroundColor Green
}
catch {
    Write-Host "   ✗ REST Direct POST failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Make a REST+GraphQL POST request
try {
    $createBody = @{
        orders = @(
            @{
                customerId = 2
                status = "Pending"
                items = @(
                    @{
                        productId = 2
                        quantity = 1
                        discount = 5
                        notes = @("Test order via GraphQL backend")
                    }
                )
            }
        )
    } | ConvertTo-Json -Depth 10
    
    $createResult = Invoke-RestMethod -Uri "$BaseUrl/api/graphql-backend/orders/bulk" -Method Post -Body $createBody -ContentType "application/json"
    Write-Host "   ✓ REST+GraphQL POST: Created $($createResult.successCount) order(s)" -ForegroundColor Green
}
catch {
    Write-Host "   ✗ REST+GraphQL POST failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "4. Checking examples endpoint..." -ForegroundColor Yellow
Start-Sleep -Seconds 1

try {
    $examplesUrl = "$BaseUrl/api/metrics/examples"
    $response = Invoke-WebRequest -Uri $examplesUrl -Method Get
    
    $html = $response.Content
    
    # Check for key elements in the HTML
    $hasRestDirectColumn = $html -match "REST Direct"
    $hasRestGraphQLColumn = $html -match "REST\+GraphQL"
    $hasOldGraphQLColumn = $html -match "GraphQL API"
    $hasRestDirectEndpoints = $html -match "GET /api/orders"
    $hasRestGraphQLEndpoints = $html -match "GET /api/graphql-backend/orders"

    Write-Host ""
    Write-Host "   HTML Content Analysis:" -ForegroundColor Cyan
    Write-Host "   ✓ Has 'REST Direct' column: $hasRestDirectColumn" -ForegroundColor $(if($hasRestDirectColumn){"Green"}else{"Red"})
    Write-Host "   ✓ Has 'REST+GraphQL' column: $hasRestGraphQLColumn" -ForegroundColor $(if($hasRestGraphQLColumn){"Green"}else{"Red"})
    Write-Host "   ✓ Has 'GraphQL API' column (should be False): $hasOldGraphQLColumn" -ForegroundColor $(if(-not $hasOldGraphQLColumn){"Green"}else{"Red"})
    Write-Host "   ✓ Shows REST Direct endpoints: $hasRestDirectEndpoints" -ForegroundColor $(if($hasRestDirectEndpoints){"Green"}else{"Red"})
    Write-Host "   ✓ Shows REST+GraphQL endpoints: $hasRestGraphQLEndpoints" -ForegroundColor $(if($hasRestGraphQLEndpoints){"Green"}else{"Red"})

    Write-Host ""
    if ($hasRestDirectColumn -and $hasRestGraphQLColumn -and -not $hasOldGraphQLColumn) {
        Write-Host "   ✓ VERIFICATION SUCCESSFUL!" -ForegroundColor Green
        Write-Host "   The Examples page is using the new 2-column layout (REST Direct vs REST+GraphQL)." -ForegroundColor Green
    }
    else {
        Write-Host "   ⚠ VERIFICATION INCOMPLETE" -ForegroundColor Yellow
        if ($hasOldGraphQLColumn) {
            Write-Host "   Still seeing 'GraphQL API' column. Please restart the app and clear browser cache." -ForegroundColor Yellow
        }
        Write-Host "   Some elements may be missing. Check the page manually." -ForegroundColor Yellow
    }
}
catch {
    Write-Host "   ✗ Failed to fetch examples page: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "5. Opening Examples page in browser..." -ForegroundColor Yellow
$examplesUrl = "$BaseUrl/api/metrics/examples"
Write-Host "   URL: $examplesUrl" -ForegroundColor Cyan
Start-Process $examplesUrl

Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "Verification complete!" -ForegroundColor Green
Write-Host ""
Write-Host "EXPECTED LAYOUT (NEW):" -ForegroundColor Yellow
Write-Host "  LEFT COLUMN:  'REST Direct' (Blue border)" -ForegroundColor Cyan
Write-Host "                - GET /api/orders" -ForegroundColor White
Write-Host "                - POST /api/orders/bulk" -ForegroundColor White
Write-Host "                - PUT /api/orders/bulk" -ForegroundColor White
Write-Host "                - DELETE /api/orders/bulk" -ForegroundColor White
Write-Host ""
Write-Host "  RIGHT COLUMN: 'REST+GraphQL' (Purple border)" -ForegroundColor Magenta
Write-Host "                - GET /api/graphql-backend/orders" -ForegroundColor White
Write-Host "                - POST /api/graphql-backend/orders/bulk" -ForegroundColor White
Write-Host "                - PUT /api/graphql-backend/orders/bulk" -ForegroundColor White
Write-Host "                - DELETE /api/graphql-backend/orders/bulk" -ForegroundColor White
Write-Host ""
Write-Host "NOTE: The 'GraphQL API' column has been REMOVED." -ForegroundColor Yellow
Write-Host ""
Write-Host "If you see a different layout, make sure to:" -ForegroundColor Yellow
Write-Host "  1. Stop the application (Ctrl+C)" -ForegroundColor White
Write-Host "  2. Rebuild: dotnet build" -ForegroundColor White
Write-Host "  3. Restart: dotnet run" -ForegroundColor White
Write-Host "  4. Clear browser cache (Ctrl+Shift+Delete)" -ForegroundColor White
Write-Host "  5. Hard refresh the page (Ctrl+F5)" -ForegroundColor White
Write-Host "  6. Run this script again" -ForegroundColor White
Write-Host "================================================================" -ForegroundColor Cyan
