#!/bin/bash

# Bash script to run all tests
echo "Running Smart CV Filter API Tests..."

# Navigate to test project directory
cd SmartCVFilter.API.Tests

# Run tests with coverage
echo "Running unit tests with coverage..."
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"

# Run tests with specific logger
echo "Running tests with detailed output..."
dotnet test --logger "console;verbosity=normal"

echo "Test run completed!"
