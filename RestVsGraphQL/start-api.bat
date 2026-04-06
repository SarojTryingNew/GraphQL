@echo off
echo Starting REST vs GraphQL API...
echo ================================

cd /d "%~dp0"

echo.
echo Building the project...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Starting the API on http://localhost:5000...
echo.
echo The API provides:
echo   - REST endpoints at: http://localhost:5000/api/*
echo   - GraphQL endpoint at: http://localhost:5000/graphql
echo   - GraphQL IDE (Banana Cake Pop): Open http://localhost:5000/graphql in your browser
echo.
echo Press Ctrl+C to stop the server
echo.

dotnet run

pause
