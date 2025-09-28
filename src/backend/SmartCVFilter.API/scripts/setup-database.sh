#!/bin/bash

# Bash script to set up the PostgreSQL database
echo "Setting up Smart CV Filter PostgreSQL Database..."

# Check if PostgreSQL is running
echo "Checking PostgreSQL connection..."
if ! command -v psql &> /dev/null; then
    echo "PostgreSQL is not installed or not in PATH. Please install PostgreSQL first."
    echo "On Ubuntu/Debian: sudo apt-get install postgresql postgresql-contrib"
    echo "On macOS: brew install postgresql"
    echo "On Windows: Download from https://www.postgresql.org/download/"
    exit 1
fi

# Navigate to the API project directory
cd src/backend/SmartCVFilter.API

# Add Entity Framework tools if not already installed
echo "Installing Entity Framework tools..."
dotnet tool install --global dotnet-ef

# Create initial migration
echo "Creating initial migration..."
dotnet ef migrations add InitialCreate

# Update database
echo "Updating database..."
dotnet ef database update

echo "PostgreSQL database setup completed!"
echo "You can now run the application with: dotnet run"
echo "Make sure to update the connection string in appsettings.json with your PostgreSQL credentials"
