# Reset Database Script
# This script will drop and recreate the database with clean seed data

Write-Host "Resetting Smart CV Filter Database..." -ForegroundColor Green

# Navigate to the API project directory
Set-Location "E:\Repos\smart-cv-filter\src\backend\SmartCVFilter.API"

# Drop the database
Write-Host "Dropping existing database..." -ForegroundColor Yellow
dotnet ef database drop --force

# Create the database
Write-Host "Creating new database..." -ForegroundColor Yellow
dotnet ef database update

# Run the application to trigger seeding
Write-Host "Starting application to seed data..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the application after seeding is complete" -ForegroundColor Cyan

# Start the application (this will trigger the seeding)
dotnet run

Write-Host "Database reset complete!" -ForegroundColor Green
