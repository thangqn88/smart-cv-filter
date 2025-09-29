# Smart CV Filter - Docker Setup

This guide explains how to run the Smart CV Filter application using Docker containers.

## 🐳 Quick Start

### Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2.0+
- Git

### 1. Clone the Repository

```bash
git clone <repository-url>
cd smart-cv-filter
```

### 2. Set Environment Variables

Create a `.env` file in the root directory:

```bash
# .env
GEMINI_API_KEY=your-gemini-api-key-here
POSTGRES_PASSWORD=postgres123
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
```

### 3. Build and Run

#### Option A: Using Scripts (Recommended)

**Linux/Mac:**

```bash
# Make scripts executable
chmod +x scripts/*.sh

# Build and run production
./scripts/docker-build.sh
./scripts/docker-run.sh

# Or build and run development
./scripts/docker-build.sh --dev
./scripts/docker-run.sh --dev
```

**Windows PowerShell:**

```powershell
# Build and run production
.\scripts\docker-build.ps1
.\scripts\docker-run.ps1

# Or build and run development
.\scripts\docker-build.ps1 -Dev
.\scripts\docker-run.ps1 -Dev
```

#### Option B: Using Docker Compose Directly

**Production:**

```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

**Development:**

```bash
# Build and start development environment
docker-compose -f docker-compose.dev.yml up --build

# Stop development environment
docker-compose -f docker-compose.dev.yml down
```

## 🏗️ Architecture

The application consists of the following services:

### Core Services

| Service         | Port | Description                   |
| --------------- | ---- | ----------------------------- |
| **Frontend**    | 3000 | Vue.js application with Nginx |
| **Backend API** | 5000 | .NET 8 Web API                |
| **PostgreSQL**  | 5432 | Primary database              |
| **Redis**       | 6379 | Caching and session storage   |

### Management Services

| Service     | Port | Description            |
| ----------- | ---- | ---------------------- |
| **pgAdmin** | 5050 | Database management UI |

## 📁 Project Structure

```
smart-cv-filter/
├── docker-compose.yml          # Production configuration
├── docker-compose.dev.yml      # Development configuration
├── scripts/                    # Build and run scripts
│   ├── docker-build.sh         # Linux/Mac build script
│   ├── docker-run.sh           # Linux/Mac run script
│   ├── docker-build.ps1        # Windows build script
│   └── docker-run.ps1          # Windows run script
├── src/
│   ├── backend/
│   │   └── SmartCVFilter.API/
│   │       ├── Dockerfile      # Backend Dockerfile
│   │       └── .dockerignore   # Backend ignore file
│   └── frontend/
│       └── smart-cv-filter/
│           ├── Dockerfile      # Frontend Dockerfile
│           ├── nginx.conf      # Nginx configuration
│           └── .dockerignore   # Frontend ignore file
└── DOCKER_README.md           # This file
```

## 🔧 Configuration

### Environment Variables

| Variable            | Description              | Default                                            |
| ------------------- | ------------------------ | -------------------------------------------------- |
| `GEMINI_API_KEY`    | Google Gemini AI API key | `your-gemini-api-key-here`                         |
| `POSTGRES_PASSWORD` | PostgreSQL password      | `postgres123`                                      |
| `JWT_SECRET_KEY`    | JWT signing key          | `YourSuperSecretKeyThatIsAtLeast32CharactersLong!` |

### Database Configuration

The application uses PostgreSQL with the following default settings:

- **Database**: `smart_cv_filter_db` (follows proper database naming convention)
- **Username**: `postgres`
- **Password**: `postgres123`
- **Host**: `postgres` (internal Docker network)
- **Port**: `5432`

### File Upload Configuration

- **Max File Size**: 10MB
- **Allowed Extensions**: `.pdf`, `.doc`, `.docx`
- **Upload Path**: `/app/uploads` (inside container)

## 🚀 Single Environment

The application uses a single environment configuration that combines the best of both development and production:

- **Development Features**: Detailed logging, hot reload capabilities
- **Production Features**: Optimized builds, Nginx serving static files, health checks
- **Single Database**: Uses `smart_cv_filter_db` for all operations
- **Consistent Setup**: Same configuration across all environments

**Start Application:**

```bash
# Windows PowerShell
.\scripts\docker-run.ps1

# Linux/Mac
./scripts/docker-run.sh
```

## 📊 Monitoring and Logs

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres
```

### Health Checks

```bash
# Check service health
docker-compose ps

# Check individual service health
docker inspect smart-cv-filter-backend --format='{{.State.Health.Status}}'
```

### Access Services

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger
- **pgAdmin**: http://localhost:5050
  - Email: `admin@smartcv.com`
  - Password: `admin123`

## 🛠️ Troubleshooting

### Common Issues

#### 1. Port Already in Use

```bash
# Check what's using the port
netstat -tulpn | grep :3000

# Kill the process
sudo kill -9 <PID>
```

#### 2. Database Connection Issues

```bash
# Check PostgreSQL logs
docker-compose logs postgres

# Restart PostgreSQL
docker-compose restart postgres
```

#### 3. Build Failures

```bash
# Clean build
./scripts/docker-build.sh --clean

# Or manually clean
docker-compose down --rmi all --volumes
docker system prune -f
```

#### 4. Permission Issues (Linux/Mac)

```bash
# Fix script permissions
chmod +x scripts/*.sh

# Fix Docker permissions
sudo usermod -aG docker $USER
# Log out and back in
```

### Debugging

#### Enter Container

```bash
# Backend container
docker exec -it smart-cv-filter-backend /bin/bash

# Frontend container
docker exec -it smart-cv-filter-frontend /bin/sh

# PostgreSQL container
docker exec -it smart-cv-filter-postgres psql -U postgres -d smart_cv_filter_db
```

#### Check Container Resources

```bash
# Resource usage
docker stats

# Container details
docker inspect smartcv-backend
```

## 🔄 Updates and Maintenance

### Update Application

```bash
# Pull latest changes
git pull

# Rebuild and restart
./scripts/docker-build.sh --clean
./scripts/docker-run.sh --clean
```

### Backup Database

```bash
# Create backup
docker exec smart-cv-filter-postgres pg_dump -U postgres smart_cv_filter_db > backup.sql

# Restore backup
docker exec -i smart-cv-filter-postgres psql -U postgres smart_cv_filter_db < backup.sql
```

### Clean Up

```bash
# Stop and remove containers
docker-compose down

# Remove volumes (WARNING: This will delete all data)
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Full cleanup
docker system prune -a --volumes
```

## 📝 Script Options

### Build Script Options

```bash
./scripts/docker-build.sh [OPTIONS]

Options:
  --dev      Build development images
  --clean    Clean build (remove existing images)
  --push     Push images to registry after build
  -h, --help Show help message
```

### Run Script Options

```bash
./scripts/docker-run.sh [OPTIONS]

Options:
  --dev         Run in development mode
  --foreground  Run in foreground (not detached)
  --clean       Clean start (remove existing containers and volumes)
  --logs        Follow logs after starting
  --stop        Stop the application
  --restart     Restart the application
  -h, --help    Show help message
```

## 🆘 Support

If you encounter issues:

1. Check the logs: `docker-compose logs -f`
2. Verify all services are running: `docker-compose ps`
3. Check resource usage: `docker stats`
4. Try a clean build: `./scripts/docker-build.sh --clean`
5. Check the troubleshooting section above

For additional help, please refer to the main project README or create an issue in the repository.
