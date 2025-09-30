# PowerShell script to set up the PostgreSQL database
Write-Host "Setting up Smart CV Filter PostgreSQL Database..." -ForegroundColor Green

# Check if PostgreSQL is running
Write-Host "Checking PostgreSQL connection..." -ForegroundColor Yellow
try {
    psql --version
    Write-Host "PostgreSQL is available" -ForegroundColor Green
} catch {
    Write-Host "PostgreSQL is not installed or not in PATH. Please install PostgreSQL first." -ForegroundColor Red
    Write-Host "Download from: https://www.postgresql.org/download/" -ForegroundColor Cyan
    exit 1
}

# Navigate to the API project directory
Set-Location "src/backend/SmartCVFilter.API"

# Add Entity Framework tools if not already installed
Write-Host "Installing Entity Framework tools..." -ForegroundColor Yellow
dotnet tool install --global dotnet-ef

# Create initial migration
Write-Host "Creating initial migration..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate

# Update database
Write-Host "Updating database..." -ForegroundColor Yellow
dotnet ef database update

Write-Host "PostgreSQL database setup completed!" -ForegroundColor Green
Write-Host "You can now run the application with: dotnet run" -ForegroundColor Cyan
Write-Host "Make sure to update the connection string in appsettings.json with your PostgreSQL credentials" -ForegroundColor Yellow
