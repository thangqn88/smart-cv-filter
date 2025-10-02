@echo off
echo Resetting Smart CV Filter Database...

cd /d "E:\Repos\smart-cv-filter\src\backend\SmartCVFilter.API"

echo Dropping existing database...
dotnet ef database drop --force

echo Creating new database...
dotnet ef database update

echo Starting application to seed data...
echo Press Ctrl+C to stop the application after seeding is complete
dotnet run

echo Database reset complete!
pause
