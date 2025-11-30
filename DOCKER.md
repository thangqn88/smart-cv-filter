# Docker Setup Guide

This guide provides detailed instructions for running Smart CV Filter using Docker.

## Prerequisites

- Docker Desktop for Windows/Mac or Docker Engine for Linux
- Docker Compose (included with Docker Desktop)
- Git

## Quick Start

1. **Clone the repository**
   ```powershell
   git clone <repository-url>
   cd smart-cv-filter
   ```

2. **Create `.env` file**
   ```powershell
   Copy-Item .env.example .env
   # Edit .env with your configuration
   ```

3. **Test Docker configuration**
   ```powershell
   .\scripts\test-docker.ps1
   ```

4. **Build and run**
   ```powershell
   .\scripts\docker-build.ps1
   .\scripts\docker-run.ps1
   ```

5. **Access the application**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger
   - pgAdmin: http://localhost:5050

## Docker Services

### PostgreSQL Database
- **Container**: `smart-cv-filter-postgres`
- **Port**: 5432
- **Database**: `smart_cv_filter_db`
- **User**: `postgres`
- **Password**: Set in `.env` file

### pgAdmin
- **Container**: `smart-cv-filter-pgadmin`
- **Port**: 5050
- **Email**: Set in `.env` (default: `admin@smartcv.com`)
- **Password**: Set in `.env` (default: `admin123`)

### Backend API
- **Container**: `smart-cv-filter-backend`
- **Port**: 5000
- **Internal Port**: 80
- **Health Check**: Enabled

### Frontend Web
- **Container**: `smart-cv-filter-frontend`
- **Port**: 3000
- **Internal Port**: 80

## Environment Variables

Key environment variables (set in `.env` file):

```env
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres123
POSTGRES_DB=smart_cv_filter_db

# JWT
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!

# Gemini AI
GEMINI_API_KEY=your-api-key-here

# Ports
BACKEND_PORT=5000
FRONTEND_PORT=3000
PGADMIN_PORT=5050
```

## Common Commands

### Build
```powershell
# Build all images
.\scripts\docker-build.ps1

# Clean build (remove existing images)
.\scripts\docker-build.ps1 -Clean
```

### Run
```powershell
# Start (detached)
.\scripts\docker-run.ps1

# Start in foreground
.\scripts\docker-run.ps1 -Foreground

# Start and follow logs
.\scripts\docker-run.ps1 -Logs

# Stop
.\scripts\docker-run.ps1 -Stop

# Restart
.\scripts\docker-run.ps1 -Restart

# Clean start (removes volumes)
.\scripts\docker-run.ps1 -Clean
```

### Docker Compose Commands
```powershell
# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres

# Check status
docker-compose ps

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Rebuild specific service
docker-compose build backend
docker-compose up -d backend
```

### Container Management
```powershell
# Access backend container shell
docker exec -it smart-cv-filter-backend /bin/bash

# Access frontend container shell
docker exec -it smart-cv-filter-frontend /bin/bash

# Access PostgreSQL
docker exec -it smart-cv-filter-postgres psql -U postgres -d smart_cv_filter_db

# View container resource usage
docker stats
```

## Troubleshooting

### Port Already in Use
```powershell
# Find process using port
netstat -ano | findstr :3000
netstat -ano | findstr :5000

# Kill process
taskkill /PID <PID> /F
```

### Database Connection Issues
```powershell
# Check PostgreSQL logs
docker-compose logs postgres

# Verify database is running
docker exec -it smart-cv-filter-postgres pg_isready -U postgres

# Restart PostgreSQL
docker-compose restart postgres
```

### Container Won't Start
```powershell
# Check logs
docker-compose logs backend
docker-compose logs frontend

# Check container status
docker-compose ps

# Rebuild containers
.\scripts\docker-build.ps1 -Clean
.\scripts\docker-run.ps1 -Clean
```

### Volume Issues
```powershell
# List volumes
docker volume ls

# Remove volumes
docker-compose down -v

# Inspect volume
docker volume inspect smart-cv-filter_postgres_data
```

### Clean Everything
```powershell
# Stop and remove everything
docker-compose down -v --rmi all

# Clean Docker system
docker system prune -a --volumes
```

## Database Migrations

Migrations run automatically on backend startup. To run manually:

```powershell
# Access backend container
docker exec -it smart-cv-filter-backend /bin/bash

# Run migrations
dotnet ef database update
```

## Backup and Restore

### Backup Database
```powershell
docker exec smart-cv-filter-postgres pg_dump -U postgres smart_cv_filter_db > backup.sql
```

### Restore Database
```powershell
docker exec -i smart-cv-filter-postgres psql -U postgres smart_cv_filter_db < backup.sql
```

## Production Deployment

For production:

1. Update `.env` with production values
2. Set `ASPNETCORE_ENVIRONMENT=Production`
3. Use strong passwords and secrets
4. Configure proper firewall rules
5. Set up SSL/TLS certificates
6. Configure backup strategy
7. Set up monitoring and logging

## Network Architecture

All services run in a Docker bridge network (`smart-cv-filter-network`):

- Services communicate using container names
- Frontend connects to backend via `http://backend:80/api`
- Backend connects to PostgreSQL via `postgres:5432`
- External access via mapped ports

## Volumes

Persistent data is stored in Docker volumes:

- `postgres_data` - Database files
- `pgadmin_data` - pgAdmin configuration
- `backend_uploads` - Uploaded CV files
- `backend_logs` - Application logs

## Health Checks

- **Backend**: Checks `/api/auth/validate` endpoint
- **PostgreSQL**: Uses `pg_isready` command

Health checks ensure services are ready before dependent services start.

