# PostgreSQL Setup Guide for Smart CV Filter

This guide will help you set up PostgreSQL for the Smart CV Filter backend API.

## Option 1: Using Docker (Recommended)

### Prerequisites

- Docker and Docker Compose installed

### Setup Steps

1. **Start PostgreSQL using Docker Compose:**

   ```bash
   cd src/backend
   docker-compose up -d
   ```

2. **Verify PostgreSQL is running:**

   ```bash
   docker ps
   ```

3. **Access pgAdmin (Optional):**

   - Open http://localhost:5050
   - Email: admin@smartcvfilter.com
   - Password: admin123

4. **Update connection string in appsettings.json:**

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=SmartCVFilterDB;Username=postgres;Password=postgres123;Port=5432"
     }
   }
   ```

5. **Run migrations:**
   ```bash
   cd src/backend/SmartCVFilter.API
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## Option 2: Local PostgreSQL Installation

### Windows

1. **Download and Install PostgreSQL:**

   - Download from: https://www.postgresql.org/download/windows/
   - Run the installer and follow the setup wizard
   - Remember the password you set for the 'postgres' user

2. **Create Database:**
   ```cmd
   # Open Command Prompt as Administrator
   psql -U postgres
   CREATE DATABASE "SmartCVFilterDB";
   \q
   ```

### macOS

1. **Install using Homebrew:**

   ```bash
   brew install postgresql
   brew services start postgresql
   ```

2. **Create Database:**
   ```bash
   createdb SmartCVFilterDB
   ```

### Linux (Ubuntu/Debian)

1. **Install PostgreSQL:**

   ```bash
   sudo apt update
   sudo apt install postgresql postgresql-contrib
   sudo systemctl start postgresql
   sudo systemctl enable postgresql
   ```

2. **Create Database:**
   ```bash
   sudo -u postgres createdb SmartCVFilterDB
   sudo -u postgres createdb SmartCVFilterDB_Dev
   ```

## Connection String Configuration

Update your `appsettings.json` with the appropriate connection string:

### Local Installation

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SmartCVFilterDB;Username=postgres;Password=your_password_here;Port=5432"
  }
}
```

### Docker Installation

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SmartCVFilterDB;Username=postgres;Password=postgres123;Port=5432"
  }
}
```

### Cloud/Remote PostgreSQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host;Database=SmartCVFilterDB;Username=your-username;Password=your-password;Port=5432;SSL Mode=Require;"
  }
}
```

## Running Migrations

1. **Navigate to the API project:**

   ```bash
   cd src/backend/SmartCVFilter.API
   ```

2. **Install EF Core tools (if not already installed):**

   ```bash
   dotnet tool install --global dotnet-ef
   ```

3. **Create initial migration:**

   ```bash
   dotnet ef migrations add InitialCreate
   ```

4. **Apply migrations to database:**
   ```bash
   dotnet ef database update
   ```

## Verification

1. **Check if tables were created:**

   ```bash
   # Using psql
   psql -U postgres -d SmartCVFilterDB
   \dt
   \q
   ```

2. **Run the application:**

   ```bash
   dotnet run
   ```

3. **Test the API:**
   - Open https://localhost:5001/swagger
   - Try the authentication endpoints

## Troubleshooting

### Common Issues

1. **Connection refused:**

   - Ensure PostgreSQL is running
   - Check if the port 5432 is available
   - Verify the connection string

2. **Authentication failed:**

   - Check username and password
   - Ensure the user has proper permissions

3. **Database does not exist:**

   - Create the database manually
   - Check the database name in connection string

4. **Migration errors:**
   - Delete existing migrations folder
   - Recreate migration: `dotnet ef migrations add InitialCreate`
   - Apply migration: `dotnet ef database update`

### Useful Commands

```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Start PostgreSQL
sudo systemctl start postgresql

# Stop PostgreSQL
sudo systemctl stop postgresql

# Connect to PostgreSQL
psql -U postgres

# List databases
\l

# Connect to specific database
\c SmartCVFilterDB

# List tables
\dt

# Exit psql
\q
```

## Performance Considerations

1. **Indexing:** The application creates appropriate indexes for better performance
2. **Connection Pooling:** Entity Framework handles connection pooling automatically
3. **SSL:** Enable SSL for production environments
4. **Backup:** Set up regular database backups for production

## Security Notes

1. **Change default passwords** in production
2. **Use environment variables** for sensitive configuration
3. **Enable SSL** for remote connections
4. **Restrict database access** to application servers only
5. **Regular security updates** for PostgreSQL
