# PowerShell script to run all tests
Write-Host "Running Smart CV Filter API Tests..." -ForegroundColor Green

# Navigate to test project directory
Set-Location "SmartCVFilter.API.Tests"

# Run tests with coverage
Write-Host "Running unit tests with coverage..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"

# Run tests with specific logger
Write-Host "Running tests with detailed output..." -ForegroundColor Yellow
dotnet test --logger "console;verbosity=normal"

Write-Host "Test run completed!" -ForegroundColor Green
