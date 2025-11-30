# Smart CV Filter

An intelligent CV screening application that uses AI (Google Gemini) to automatically analyze and filter job applications. Built with .NET 8 Web API backend and .NET 8 MVC frontend.

## 🏗️ Architecture

The application consists of two main components:

- **Backend API** (`src/backend/SmartCVFilter.API`) - .NET 8 Web API providing RESTful endpoints
- **Frontend Web** (`src/frontend/SmartCVFilter.Web`) - .NET 8 MVC application with Razor views

## 📋 Prerequisites

### Option 1: Docker (Recommended)

- **Docker Desktop** - [Download here](https://www.docker.com/products/docker-desktop/)
- **Docker Compose** - Included with Docker Desktop
- **Git** - [Download here](https://git-scm.com/download/win)

### Option 2: Local Development

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 12+** - [Download here](https://www.postgresql.org/download/) or use Docker
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** - [Download here](https://git-scm.com/download/win)

> **📖 PostgreSQL Setup Guide**: For detailed PostgreSQL installation and configuration instructions, see [src/backend/POSTGRESQL_SETUP.md](./src/backend/POSTGRESQL_SETUP.md)

## 🚀 Quick Start

### 🐳 Docker Setup (Recommended)

The easiest way to run the application is using Docker Compose.

#### Step 1: Clone the Repository

```powershell
git clone <repository-url>
cd smart-cv-filter
```

#### Step 2: Configure Environment Variables

Create a `.env` file in the root directory (or copy from `.env.example`):

```powershell
# Copy the example file
Copy-Item .env.example .env
```

Edit `.env` file with your configuration:

```env
# Database Configuration
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres123
POSTGRES_DB=smart_cv_filter_db
POSTGRES_PORT=5432

# Google Gemini AI Configuration
GEMINI_API_KEY=your-gemini-api-key-here
GEMINI_MODEL_NAME=gemini-2.5-flash
GEMINI_MAX_TOKENS=2048

# File Upload Configuration
FILE_UPLOAD_MAX_SIZE=10485760
FILE_UPLOAD_ALLOWED_EXTENSIONS=.pdf,.doc,.docx,.txt
FILE_UPLOAD_PATH=uploads/cvs

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
BACKEND_PORT=5000
FRONTEND_PORT=3000

# Frontend Configuration
FRONTEND_API_BASE_URL=http://localhost:5000/api

# pgAdmin Configuration
PGADMIN_DEFAULT_EMAIL=admin@smartcv.com
PGADMIN_DEFAULT_PASSWORD=admin123
PGADMIN_PORT=5050
```

#### Step 3: Build and Run

**Using PowerShell Scripts (Recommended):**

```powershell
# Test Docker configuration
.\scripts\test-docker.ps1

# Build Docker images
.\scripts\docker-build.ps1

# Run the application
.\scripts\docker-run.ps1
```

**Using Docker Compose Directly:**

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

#### Step 4: Access the Application

Once running, access the application at:

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger
- **pgAdmin**: http://localhost:5050
  - Email: `admin@smartcv.com`
  - Password: `admin123`
- **PostgreSQL**: localhost:5432

#### Docker Commands

```powershell
# Build images
.\scripts\docker-build.ps1

# Build with clean (remove existing images)
.\scripts\docker-build.ps1 -Clean

# Start application (detached)
.\scripts\docker-run.ps1

# Start in foreground (see logs)
.\scripts\docker-run.ps1 -Foreground

# Start and follow logs
.\scripts\docker-run.ps1 -Logs

# Stop application
.\scripts\docker-run.ps1 -Stop

# Restart application
.\scripts\docker-run.ps1 -Restart

# Clean start (remove containers and volumes)
.\scripts\docker-run.ps1 -Clean

# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres

# Check service status
docker-compose ps

# Access container shell
docker exec -it smart-cv-filter-backend /bin/bash
docker exec -it smart-cv-filter-frontend /bin/bash
docker exec -it smart-cv-filter-postgres psql -U postgres -d smart_cv_filter_db
```

### 💻 Local Development Setup

### Step 1: Clone the Repository

```powershell
git clone <repository-url>
cd smart-cv-filter
```

### Step 2: Set Up Database

1. **Install PostgreSQL** (if not already installed)

2. **Create the database**:

```powershell
# Using psql
psql -U postgres
CREATE DATABASE smart_cv_filter_db;
```

3. **Update connection string** in `src/backend/SmartCVFilter.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=smart_cv_filter_db;Username=postgres;Password=YourPasswordHere"
  }
}
```

4. **Run migrations**:

```powershell
cd src/backend/SmartCVFilter.API
dotnet ef database update
```

### Step 3: Configure Backend API

Update `src/backend/SmartCVFilter.API/appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "SmartCVFilter.API",
    "Audience": "SmartCVFilter.Client",
    "ExpirationInDays": 7
  },
  "GeminiAI": {
    "ApiKey": "YourGeminiAIApiKeyHere",
    "ModelName": "gemini-2.5-flash",
    "MaxTokens": 2048
  },
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".txt"],
    "UploadPath": "uploads/cvs"
  }
}
```

### Step 4: Configure Frontend

Update `src/frontend/SmartCVFilter.Web/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:4000/api",
    "Timeout": 30
  },
  "JwtSettings": {
    "Issuer": "SmartCVFilter",
    "Audience": "SmartCVFilter",
    "SecretKey": "YourSecretKeyHereThatIsAtLeast32CharactersLong"
  }
}
```

> **Note**: The JWT SecretKey must match between backend and frontend configurations.

### Step 5: Run the Application

#### Start Backend API

```powershell
cd src/backend/SmartCVFilter.API
dotnet run
```

The API will be available at:

- **HTTP**: `http://localhost:4000`
- **Swagger UI**: `http://localhost:4000/swagger`

#### Start Frontend Web (in a new terminal)

```powershell
cd src/frontend/SmartCVFilter.Web
dotnet run
```

The web application will be available at:

- **HTTP**: `http://localhost:5002`

### Step 6: Access the Application

1. Open your browser and navigate to `http://localhost:5002`
2. Register a new account or use the default admin credentials:
   - **Email**: `admin@smartcvfilter.com`
   - **Password**: `Admin123!`
   - **Role**: Admin

## 📁 Project Structure

```
smart-cv-filter/
├── src/
│   ├── backend/
│   │   └── SmartCVFilter.API/          # Backend Web API
│   │       ├── Controllers/             # API Controllers
│   │       │   ├── AuthController.cs
│   │       │   ├── JobPostsController.cs
│   │       │   ├── ApplicantsController.cs
│   │       │   ├── CVUploadController.cs
│   │       │   ├── ScreeningController.cs
│   │       │   ├── UsersController.cs
│   │       │   └── RolesController.cs
│   │       ├── Services/                # Business Logic Services
│   │       │   ├── GeminiAIService.cs   # AI Integration
│   │       │   ├── AuthService.cs
│   │       │   ├── JobPostService.cs
│   │       │   └── ...
│   │       ├── Models/                  # Data Models
│   │       ├── DTOs/                    # Data Transfer Objects
│   │       ├── Data/                    # Database Context
│   │       └── Migrations/               # EF Core Migrations
│   │
│   └── frontend/
│       └── SmartCVFilter.Web/           # Frontend MVC Application
│           ├── Controllers/              # MVC Controllers
│           ├── Views/                    # Razor Views
│           ├── Models/                   # View Models
│           ├── Services/                # API Client Services
│           └── wwwroot/                  # Static Files
│
├── tests/                                # Unit Tests
└── charts/                               # Architecture Diagrams
```

## 🔌 API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login (returns JWT token)
- `POST /api/auth/validate` - Validate JWT token

### Job Posts

- `GET /api/jobposts` - Get user's job posts (requires authentication)
- `GET /api/jobposts/all` - Get all public job posts
- `GET /api/jobposts/{id}` - Get job post by ID
- `POST /api/jobposts` - Create new job post
- `PUT /api/jobposts/{id}` - Update job post
- `DELETE /api/jobposts/{id}` - Delete job post

### Applicants

- `GET /api/jobposts/{jobPostId}/applicants` - Get applicants for job post
- `GET /api/jobposts/{jobPostId}/applicants/{id}` - Get applicant by ID
- `POST /api/jobposts/{jobPostId}/applicants` - Create new applicant
- `PUT /api/jobposts/{jobPostId}/applicants/{id}` - Update applicant
- `DELETE /api/jobposts/{jobPostId}/applicants/{id}` - Delete applicant

### CV Upload

- `POST /api/applicants/{applicantId}/cvupload/upload` - Upload CV file
- `GET /api/applicants/{applicantId}/cvupload/{cvFileId}/download` - Download CV file
- `DELETE /api/applicants/{applicantId}/cvupload/{cvFileId}` - Delete CV file
- `POST /api/applicants/{applicantId}/cvupload/{cvFileId}/extract-text` - Extract text from CV

### AI Screening

- `POST /api/jobposts/{jobPostId}/applicants/screen` - Start AI screening for applicants
- `GET /api/screening/results/{resultId}` - Get screening result
- `GET /api/screening/applicants/{applicantId}/results` - Get screening results for applicant
- `GET /api/aiprocessing/applicant/{applicantId}/status` - Get AI processing status

### Users & Roles

- `GET /api/users` - Get all users (with pagination)
- `GET /api/users/{id}` - Get user by ID
- `GET /api/roles` - Get all roles
- `GET /api/roles/{id}` - Get role by ID

> **Full API Documentation**: Visit `http://localhost:4000/swagger` when the backend is running.

## 🎯 Features

### Core Functionality

- **JWT Authentication** - Secure user authentication and authorization
- **Job Post Management** - Create, edit, and manage job postings with detailed requirements
- **Applicant Management** - Track and manage job applicants with comprehensive profiles
- **CV File Upload** - Upload and process CV files (PDF, DOC, DOCX, TXT)
- **AI Screening** - Integration with Google Gemini AI for automated CV analysis
- **User & Role Management** - Manage users and roles with proper authorization

### AI Screening Features

- **Automated Analysis** - AI-powered CV analysis against job requirements
- **Scoring System** - Overall match score and category-specific scores
- **Strengths & Weaknesses** - Detailed breakdown of candidate strengths and areas for improvement
- **Detailed Analysis** - Comprehensive AI-generated insights
- **Processing Status** - Real-time status tracking for AI processing

### Frontend Features

- **Modern UI** - Clean, responsive interface using Bootstrap 5.3
- **Dashboard** - Overview with statistics and quick actions
- **Data Tables** - Sortable, searchable tables with pagination
- **Form Validation** - Client-side and server-side validation
- **Notification System** - User-friendly notifications for actions
- **Print Support** - Print-friendly views for reports

## 🛠️ Development

### Running in Development Mode

Both projects support hot reload:

```powershell
# Backend (with hot reload)
cd src/backend/SmartCVFilter.API
dotnet watch run

# Frontend (with hot reload)
cd src/frontend/SmartCVFilter.Web
dotnet watch run
```

### Database Migrations

```powershell
# Create a new migration
cd src/backend/SmartCVFilter.API
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback last migration
dotnet ef database update PreviousMigrationName
```

### Database Reset Scripts

The backend includes database reset scripts:

```powershell
# Windows PowerShell
cd src/backend/SmartCVFilter.API
.\scripts\reset-database.ps1

# Windows Batch
.\scripts\reset-database.bat

# Linux/Mac
.\scripts\reset-database.sh
```

### Testing

```powershell
# Run backend tests
cd tests/SmartCVFilter.API.Tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## ⚙️ Configuration

### Backend Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=smart_cv_filter_db;Username=postgres;Password=YourPassword"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "SmartCVFilter.API",
    "Audience": "SmartCVFilter.Client",
    "ExpirationInDays": 7
  },
  "GeminiAI": {
    "ApiKey": "YourGeminiAIApiKeyHere",
    "ModelName": "gemini-2.5-flash",
    "MaxTokens": 2048
  },
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".pdf", ".doc", ".docx", ".txt"],
    "UploadPath": "uploads/cvs"
  },
  "Pagination": {
    "DefaultPageSize": 10,
    "MaxPageSize": 100,
    "MinPageSize": 1
  }
}
```

### Frontend Configuration (`appsettings.json`)

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:4000/api",
    "Timeout": 30
  },
  "JwtSettings": {
    "Issuer": "SmartCVFilter",
    "Audience": "SmartCVFilter",
    "SecretKey": "YourSecretKeyHereThatIsAtLeast32CharactersLong"
  },
  "Pagination": {
    "DefaultPageSize": 10,
    "MaxPageSize": 100,
    "MinPageSize": 1
  }
}
```

## 🔐 Default Users

The application creates default users on startup (via `SeedData.cs`):

- **Admin User**

  - Email: `admin@smartcvfilter.com`
  - Password: `Admin123!`
  - Role: Admin

- **Sample Recruiter**
  - Email: `recruiter@example.com`
  - Password: `Recruiter123!`
  - Role: Recruiter

## 📚 Additional Documentation

- [Backend API README](./src/backend/SmartCVFilter.API/README.md) - Detailed backend documentation
- [Frontend README](./src/frontend/README.md) - Frontend-specific documentation
- [PostgreSQL Setup Guide](./src/backend/POSTGRESQL_SETUP.md) - Database setup instructions
- [Gemini AI Integration](./src/backend/SmartCVFilter.API/GEMINI_API_UPDATE.md) - AI integration details

## 🔧 Troubleshooting

### Common Issues

#### 1. Database Connection Errors

```powershell
# Verify PostgreSQL is running
# Windows
Get-Service postgresql*

# Check connection string in appsettings.json
# Ensure database exists
psql -U postgres -l
```

#### 2. Port Already in Use

```powershell
# Check what's using the port
netstat -ano | findstr :4000
netstat -ano | findstr :5002

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

#### 3. Migration Errors

```powershell
# Drop and recreate database (WARNING: Deletes all data)
cd src/backend/SmartCVFilter.API
dotnet ef database drop
dotnet ef database update
```

#### 4. CORS Errors

Ensure the backend CORS policy includes the frontend URL. Check `Program.cs`:

```csharp
policy.WithOrigins("http://localhost:5002", "http://localhost:4173", "http://localhost:3000")
```

#### 5. JWT Token Issues

- Ensure JWT SecretKey matches between backend and frontend
- Check token expiration settings
- Verify issuer and audience match

#### 6. AI Processing Not Working

- Verify Gemini API key is configured correctly
- Check API key has proper permissions
- Review logs in `src/backend/SmartCVFilter.API/logs/`

## 🚀 Deployment

### Production Configuration

1. **Update `appsettings.Production.json`** with production settings
2. **Set environment variables** for sensitive data
3. **Configure HTTPS** certificates
4. **Set up reverse proxy** (IIS, Nginx, etc.)
5. **Configure logging** for production monitoring

### Environment Variables

For production, use environment variables instead of appsettings:

```powershell
# Backend
$env:ConnectionStrings__DefaultConnection="Host=prod-db;Database=smart_cv_filter_db;..."
$env:JwtSettings__SecretKey="YourProductionSecretKey"
$env:GeminiAI__ApiKey="YourProductionApiKey"

# Frontend
$env:ApiSettings__BaseUrl="https://api.yourdomain.com/api"
```

## 📝 Technology Stack

### Backend

- **.NET 8** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL** - Relational database
- **JWT Bearer Authentication** - Token-based authentication
- **Serilog** - Structured logging
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation
- **Google Gemini AI** - AI-powered CV analysis

### Frontend

- **.NET 8 MVC** - Model-View-Controller framework
- **Razor Views** - Server-side rendering
- **Bootstrap 5.3** - CSS framework
- **Bootstrap Icons** - Icon library
- **JavaScript** - Client-side interactivity
- **Cookie Authentication** - Session management

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

If you encounter issues:

1. Check the [Troubleshooting](#-troubleshooting) section above
2. Review the logs in `src/backend/SmartCVFilter.API/logs/`
3. Check the [Backend README](./src/backend/SmartCVFilter.API/README.md) for detailed API documentation
4. Check the [Frontend README](./src/frontend/README.md) for frontend-specific help
5. Create an issue in the repository

---

**Smart CV Filter** - AI-Powered Resume Screening Made Simple
