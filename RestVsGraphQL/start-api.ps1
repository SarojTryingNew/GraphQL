Write-Host "Starting REST vs GraphQL API..." -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to script directory
Set-Location $PSScriptRoot

Write-Host "Building the project..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Starting the API on http://localhost:5000..." -ForegroundColor Green
Write-Host ""
Write-Host "The API provides:" -ForegroundColor White
Write-Host "  - REST endpoints at: http://localhost:5000/api/*" -ForegroundColor Gray
Write-Host "  - GraphQL endpoint at: http://localhost:5000/graphql" -ForegroundColor Gray
Write-Host "  - GraphQL IDE (Banana Cake Pop): Open http://localhost:5000/graphql in your browser" -ForegroundColor Gray
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Start the API
dotnet run

Read-Host "Press Enter to exit"
