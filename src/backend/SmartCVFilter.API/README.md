# Smart CV Filter API

A .NET 8 Web API for automated CV screening using AI (Gemini AI). This backend service provides endpoints for job post management, applicant tracking, CV upload, and AI-powered screening.

## Features

- **JWT-based Authentication** - Secure user authentication and authorization
- **Job Post Management** - CRUD operations for job postings
- **Applicant Management** - Track and manage job applicants
- **CV File Upload** - Upload and process CV files (PDF, DOC, DOCX, TXT)
- **AI Screening** - Integration with Gemini AI for automated CV analysis
- **Database Integration** - Entity Framework Core with PostgreSQL
- **Swagger Documentation** - API documentation and testing interface

## Prerequisites

- .NET 8 SDK
- PostgreSQL 12+ (local installation or cloud instance)
- Visual Studio 2022 or VS Code

> **📖 PostgreSQL Setup Guide**: For detailed PostgreSQL installation and configuration instructions, see [POSTGRESQL_SETUP.md](./POSTGRESQL_SETUP.md)

## Setup Instructions

### 1. Database Setup

1. Install PostgreSQL and create a database:

```bash
# Create database
createdb smart_cv_filter_db

# Or using psql
psql -U postgres
CREATE DATABASE "smart_cv_filter_db";
```

2. Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=smart_cv_filter_db;Username=postgres;Password=your_password_here"
  }
}
```

3. Run Entity Framework migrations:

```bash
cd src/backend/SmartCVFilter.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 2. Configuration

1. Update JWT settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "SmartCVFilter.API",
    "Audience": "SmartCVFilter.Client",
    "ExpirationInDays": 7
  }
}
```

2. Add Gemini AI API key:

```json
{
  "GeminiAI": {
    "ApiKey": "YourGeminiAIApiKeyHere",
    "ModelName": "gemini-pro",
    "MaxTokens": 2048
  }
}
```

### 3. Run the Application

```bash
cd src/backend/SmartCVFilter.API
dotnet run
```

The API will be available at:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/validate` - Validate JWT token

### Job Posts

- `GET /api/jobposts` - Get user's job posts
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
- `POST /api/jobposts/{jobPostId}/applicants/screen` - Start AI screening

### CV Upload

- `POST /api/applicants/{applicantId}/cvupload/upload` - Upload CV file
- `GET /api/applicants/{applicantId}/cvupload/{cvFileId}/download` - Download CV file
- `DELETE /api/applicants/{applicantId}/cvupload/{cvFileId}` - Delete CV file
- `POST /api/applicants/{applicantId}/cvupload/{cvFileId}/extract-text` - Extract text from CV

### Screening Results

- `GET /api/screening/results/{resultId}` - Get screening result
- `GET /api/screening/applicants/{applicantId}/results` - Get screening results for applicant

## Default Users

The application creates default users on startup:

- **Admin User**

  - Email: `admin@smartcvfilter.com`
  - Password: `Admin123!`
  - Role: Admin

- **Sample Recruiter**
  - Email: `recruiter@example.com`
  - Password: `Recruiter123!`
  - Role: Recruiter

## File Upload

The API supports CV file uploads with the following specifications:

- **Supported formats**: PDF, DOC, DOCX, TXT
- **Maximum file size**: 10MB
- **Upload path**: `wwwroot/uploads/cvs/`

## AI Integration

The API integrates with Google Gemini AI for CV analysis. To enable this feature:

1. Get a Gemini AI API key from Google AI Studio
2. Add the API key to `appsettings.json`
3. The AI will analyze CVs and provide scoring, strengths, weaknesses, and detailed analysis

## Database Schema

The application uses the following main entities:

- **ApplicationUser** - User accounts (recruiters, admins)
- **JobPost** - Job postings
- **Applicant** - Job applicants
- **CVFile** - Uploaded CV files
- **ScreeningResult** - AI analysis results

## Development Notes

- The application uses Entity Framework Core with PostgreSQL for data access
- JWT tokens are used for authentication
- File uploads are stored in the `wwwroot/uploads` directory
- AI screening is processed asynchronously in the background
- Comprehensive logging is implemented using Serilog
- PostgreSQL-specific optimizations are included for better performance

## Next Steps

1. **Real Gemini AI Integration**: Replace the mock AI service with actual Gemini AI API calls
2. **PDF/DOC Text Extraction**: Implement proper text extraction from PDF and Word documents
3. **Email Notifications**: Add email notifications for screening completion
4. **Real-time Updates**: Implement SignalR for real-time screening status updates
5. **Advanced Filtering**: Add more sophisticated CV filtering criteria
6. **Analytics Dashboard**: Add analytics and reporting features
