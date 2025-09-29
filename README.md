# Smart CV Filter

An intelligent CV screening application that uses AI (Google Gemini) to automatically analyze and filter job applications. Built with .NET 8 Web API backend and Vue.js frontend, containerized with Docker for easy deployment.

## 🚀 Quick Start with Docker Desktop (Windows)

### Prerequisites

- **Docker Desktop for Windows** - [Download here](https://www.docker.com/products/docker-desktop/)
- **Git** - [Download here](https://git-scm.com/download/win)
- **PowerShell** (included with Windows)

### Step 1: Clone the Repository

```powershell
git clone <repository-url>
cd smart-cv-filter
```

### Step 2: Set Up Environment Variables

Create a `.env` file in the root directory:

```powershell
# Copy the example file
copy env.example .env
```

Edit `.env` file with your configuration:

```env
# Database Configuration
POSTGRES_PASSWORD=postgres123
POSTGRES_DB=smart_cv_filter_db
POSTGRES_USER=postgres

# JWT Configuration
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=SmartCVFilter
JWT_AUDIENCE=SmartCVFilter
JWT_EXPIRATION_HOURS=24

# Google Gemini AI Configuration
GEMINI_API_KEY=your-gemini-api-key-here
GEMINI_BASE_URL=https://generativelanguage.googleapis.com/v1beta

# File Upload Configuration
FILE_UPLOAD_MAX_SIZE_MB=10
FILE_UPLOAD_ALLOWED_EXTENSIONS=.pdf,.doc,.docx
FILE_UPLOAD_PATH=/app/uploads

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80

# Frontend Configuration
VITE_API_BASE_URL=http://localhost:5000/api

# Redis Configuration (Optional)
REDIS_PASSWORD=
REDIS_DB=0

# pgAdmin Configuration
PGADMIN_DEFAULT_EMAIL=admin@smartcv.com
PGADMIN_DEFAULT_PASSWORD=admin123
```

### Step 3: Build and Run the Application

#### Option A: Using PowerShell Scripts (Recommended)

```powershell
# Build the application
.\scripts\docker-build.ps1

# Run the application
.\scripts\docker-run.ps1
```

#### Option B: Using Docker Compose Directly

```powershell
# Build images
docker-compose build

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Step 4: Access the Application

Once running, access the application at:

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger
- **pgAdmin**: http://localhost:5050
  - Email: `admin@smartcv.com`
  - Password: `admin123`
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

## 🛠️ Available Commands

### Build Commands

```powershell
# Basic build
.\scripts\docker-build.ps1

# Clean build (remove existing images)
.\scripts\docker-build.ps1 -Clean

# Build and push to registry
.\scripts\docker-build.ps1 -Push

# Show help
.\scripts\docker-build.ps1 -Help
```

### Run Commands

```powershell
# Start application (detached mode)
.\scripts\docker-run.ps1

# Start in foreground (see logs)
.\scripts\docker-run.ps1 -Foreground

# Clean start (remove containers and volumes)
.\scripts\docker-run.ps1 -Clean

# Start and follow logs
.\scripts\docker-run.ps1 -Logs

# Stop application
.\scripts\docker-run.ps1 -Stop

# Restart application
.\scripts\docker-run.ps1 -Restart

# Show help
.\scripts\docker-run.ps1 -Help
```

### Test Commands

```powershell
# Test Docker configuration
.\scripts\test-docker.ps1
```

## 🏗️ Architecture

The application consists of the following services:

| Service         | Container Name             | Port | Description                 |
| --------------- | -------------------------- | ---- | --------------------------- |
| **PostgreSQL**  | `smart-cv-filter-postgres` | 5432 | Primary database            |
| **pgAdmin**     | `smart-cv-filter-pgadmin`  | 5050 | Database management UI      |
| **Backend API** | `smart-cv-filter-backend`  | 5000 | .NET 8 Web API              |
| **Frontend**    | `smart-cv-filter-frontend` | 3000 | Vue.js + Nginx              |
| **Redis**       | `smart-cv-filter-redis`    | 6379 | Caching and session storage |

## 📊 Monitoring and Management

### View Logs

```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres
```

### Check Service Status

```powershell
# Check all services
docker-compose ps

# Check individual service health
docker inspect smart-cv-filter-backend --format='{{.State.Health.Status}}'
```

### Access Container Shells

```powershell
# Backend container
docker exec -it smart-cv-filter-backend /bin/bash

# Frontend container
docker exec -it smart-cv-filter-frontend /bin/sh

# PostgreSQL container
docker exec -it smart-cv-filter-postgres psql -U postgres -d smart_cv_filter_db
```

## 🔧 Troubleshooting

### Common Issues

#### 1. Port Already in Use

```powershell
# Check what's using the port
netstat -ano | findstr :3000

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

#### 2. Docker Desktop Not Running

- Ensure Docker Desktop is running
- Check if Docker Desktop is started in system tray
- Restart Docker Desktop if needed

#### 3. Build Failures

```powershell
# Clean build
.\scripts\docker-build.ps1 -Clean

# Or manually clean
docker-compose down --rmi all --volumes
docker system prune -f
```

#### 4. Database Connection Issues

```powershell
# Check PostgreSQL logs
docker-compose logs postgres

# Restart PostgreSQL
docker-compose restart postgres
```

### Debugging

```powershell
# Check resource usage
docker stats

# Check container details
docker inspect smart-cv-filter-backend

# View container logs
docker logs smart-cv-filter-backend
```

## 🔄 Updates and Maintenance

### Update Application

```powershell
# Pull latest changes
git pull

# Rebuild and restart
.\scripts\docker-build.ps1 -Clean
.\scripts\docker-run.ps1 -Clean
```

### Backup Database

```powershell
# Create backup
docker exec smart-cv-filter-postgres pg_dump -U postgres smart_cv_filter_db > backup.sql

# Restore backup
docker exec -i smart-cv-filter-postgres psql -U postgres smart_cv_filter_db < backup.sql
```

### Clean Up

```powershell
# Stop and remove containers
docker-compose down

# Remove volumes (WARNING: This will delete all data)
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Full cleanup
docker system prune -a --volumes
```

## 📝 Features

- **JWT Authentication** - Secure user authentication and authorization
- **Job Post Management** - CRUD operations for job postings
- **Applicant Management** - Track and manage job applicants
- **CV File Upload** - Upload and process CV files (PDF, DOC, DOCX)
- **AI Screening** - Integration with Google Gemini AI for automated CV analysis
- **Real-time Updates** - Live processing status updates
- **Database Management** - pgAdmin interface for database administration
- **Responsive UI** - Modern Vue.js frontend with Tailwind CSS

## 🆘 Support

If you encounter issues:

1. Check the logs: `docker-compose logs -f`
2. Verify all services are running: `docker-compose ps`
3. Check resource usage: `docker stats`
4. Try a clean build: `.\scripts\docker-build.ps1 -Clean`
5. Check the troubleshooting section above

For additional help, please refer to the [Docker README](DOCKER_README.md) or create an issue in the repository.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
