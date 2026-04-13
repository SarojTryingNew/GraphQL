# Diagnostic script to check Examples page layout
param(
    [string]$BaseUrl = "http://localhost:5072"
)

Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "   EXAMPLES PAGE LAYOUT DIAGNOSTIC" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

try {
    Write-Host "Fetching Examples page..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/metrics/examples" -Method Get
    $html = $response.Content
    
    Write-Host ""
    Write-Host "DETECTED COLUMNS:" -ForegroundColor Cyan
    Write-Host "=================" -ForegroundColor Cyan
    
    # Extract all api-label elements
    if ($html -match '<div class=[''"]api-label rest[''"]>(.*?)</div>') {
        Write-Host "LEFT COLUMN LABEL:  " -NoNewline -ForegroundColor Green
        Write-Host $matches[1] -ForegroundColor White
    }
    
    if ($html -match '<div class=[''"]api-label graphql[''"]>(.*?)</div>') {
        Write-Host "RIGHT COLUMN LABEL: " -NoNewline -ForegroundColor Magenta
        Write-Host $matches[1] -ForegroundColor White
    }
    
    # Count columns
    $columnCount = ([regex]::Matches($html, '<div class=[''"]api-column')).Count
    Write-Host ""
    Write-Host "Total Columns Found: $columnCount" -ForegroundColor Cyan
    
    # Check for REST Direct requests
    Write-Host ""
    Write-Host "CONTENT ANALYSIS:" -ForegroundColor Cyan
    Write-Host "=================" -ForegroundColor Cyan
    
    $restDirectMatches = ([regex]::Matches($html, '\[Direct\]')).Count
    Write-Host "REST Direct badges ([Direct]): $restDirectMatches" -ForegroundColor $(if($restDirectMatches -gt 0){"Green"}else{"Red"})
    
    $restGraphQLMatches = ([regex]::Matches($html, '\[\+GraphQL\]')).Count
    Write-Host "REST+GraphQL badges ([+GraphQL]): $restGraphQLMatches" -ForegroundColor $(if($restGraphQLMatches -gt 0){"Green"}else{"Red"})
    
    # Extract all endpoints
    Write-Host ""
    Write-Host "REQUESTS FOUND:" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    
    $endpoints = [regex]::Matches($html, '(GET|POST|PUT|DELETE)\s+(/api/[^\s<]+)')
    if ($endpoints.Count -eq 0) {
        Write-Host "No requests found in Examples page!" -ForegroundColor Red
    } else {
        $endpoints | ForEach-Object {
            $method = $_.Groups[1].Value
            $path = $_.Groups[2].Value
            
            $color = "White"
            if ($path -like "*/graphql-backend/*") {
                $color = "Magenta"
                Write-Host "  $method $path [REST+GraphQL]" -ForegroundColor $color
            } elseif ($path -like "/api/*") {
                $color = "Cyan"
                Write-Host "  $method $path [REST Direct]" -ForegroundColor $color
            } else {
                Write-Host "  $method $path [Unknown]" -ForegroundColor Yellow
            }
        }
    }
    
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "CURRENT LAYOUT:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  ┌─────────────────────────────────────────┬──────────────────────────┐" -ForegroundColor DarkGray
    Write-Host "  │ REST APIs (Direct + GraphQL Backend)    │ GraphQL API              │" -ForegroundColor DarkGray
    Write-Host "  │                                         │                          │" -ForegroundColor DarkGray
    Write-Host "  │ • GET /api/orders [Direct]              │ • POST /graphql          │" -ForegroundColor DarkGray
    Write-Host "  │ • GET /api/graphql-backend/orders       │   (if direct GraphQL     │" -ForegroundColor DarkGray
    Write-Host "  │   [+GraphQL]                            │    calls were made)      │" -ForegroundColor DarkGray
    Write-Host "  │ • POST /api/orders/bulk [Direct]        │                          │" -ForegroundColor DarkGray
    Write-Host "  │ • POST /api/graphql-backend/orders/bulk │                          │" -ForegroundColor DarkGray
    Write-Host "  │   [+GraphQL]                            │                          │" -ForegroundColor DarkGray
    Write-Host "  └─────────────────────────────────────────┴──────────────────────────┘" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Do you want a DIFFERENT layout? (3 columns)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  ┌─────────────────────┬─────────────────────┬──────────────────────┐" -ForegroundColor DarkGray
    Write-Host "  │ REST Direct         │ REST+GraphQL        │ GraphQL API          │" -ForegroundColor DarkGray
    Write-Host "  │                     │                     │                      │" -ForegroundColor DarkGray
    Write-Host "  │ • GET /api/orders   │ • GET /api/graphql- │ • POST /graphql      │" -ForegroundColor DarkGray
    Write-Host "  │ • POST /api/orders  │   backend/orders    │                      │" -ForegroundColor DarkGray
    Write-Host "  │                     │ • POST /api/graphql-│                      │" -ForegroundColor DarkGray
    Write-Host "  │                     │   backend/orders    │                      │" -ForegroundColor DarkGray
    Write-Host "  └─────────────────────┴─────────────────────┴──────────────────────┘" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "If you want the 3-column layout, please confirm!" -ForegroundColor Yellow
    Write-Host "================================================================" -ForegroundColor Cyan
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
