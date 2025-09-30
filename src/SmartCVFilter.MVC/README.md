# Smart CV Filter - Merged MVC Application

This is a merged .NET 8 MVC application that combines the frontend and backend functionality into a single project for easier development and debugging.

## Features

- **Job Post Management**: Create, read, update, and delete job postings
- **Applicant Management**: Manage job applicants and their applications
- **AI-Powered CV Screening**: Automated resume analysis using Gemini AI
- **User Authentication**: Secure login and registration system
- **Responsive UI**: Modern Bootstrap-based interface

## Project Structure

```
src/SmartCVFilter.MVC/
├── Controllers/           # MVC Controllers
│   ├── HomeController.cs
│   ├── AuthMvcController.cs
│   ├── JobPostsMvcController.cs
│   ├── ApplicantsMvcController.cs
│   ├── ScreeningMvcController.cs
│   └── [API Controllers]  # Original API controllers for external access
├── Views/                 # Razor Views
│   ├── Home/
│   ├── Auth/
│   ├── JobPosts/
│   ├── Applicants/
│   └── Screening/
├── Models/               # Entity Models
├── DTOs/                # Data Transfer Objects
├── Services/            # Business Logic Services
├── Data/               # Database Context and Migrations
└── wwwroot/            # Static Files (CSS, JS, Images)
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL Database
- Gemini AI API Key (optional, for AI screening features)

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd smart-cv-filter/src/SmartCVFilter.MVC
   ```

2. **Configure the database**

   - Update the connection string in `appsettings.json`
   - Run the database migrations:
     ```bash
     dotnet ef database update
     ```

3. **Configure Gemini AI (Optional)**

   - Add your Gemini AI API key to `appsettings.json`:
     ```json
     "GeminiAI": {
       "ApiKey": "your-api-key-here",
       "ModelName": "gemini-pro",
       "MaxTokens": 2048
     }
     ```

4. **Run the application**

   ```bash
   dotnet run
   ```

5. **Access the application**
   - Web Interface: http://localhost:5000
   - API Documentation: http://localhost:5000/swagger

## Configuration

### Database Connection

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=smart_cv_filter_db;Username=postgres;Password=your-password"
}
```

### JWT Settings

Configure JWT authentication settings:

```json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "SmartCVFilter.API",
  "Audience": "SmartCVFilter.Client",
  "ExpirationInDays": 7
}
```

### File Upload Settings

Configure file upload limits and allowed extensions:

```json
"FileUpload": {
  "MaxFileSize": 10485760,
  "AllowedExtensions": [".pdf", ".doc", ".docx", ".txt"],
  "UploadPath": "uploads/cvs"
}
```

## API Endpoints

The application provides both MVC views and API endpoints:

### MVC Routes

- `/` - Dashboard
- `/Auth/Login` - User login
- `/Auth/Register` - User registration
- `/JobPosts` - Job post management
- `/Applicants` - Applicant management
- `/Screening` - AI screening results

### API Endpoints

- `/api/Auth/*` - Authentication endpoints
- `/api/JobPosts/*` - Job post CRUD operations
- `/api/Applicants/*` - Applicant management
- `/api/Screening/*` - Screening operations
- `/api/CVUpload/*` - File upload operations

## Development

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Building for Production

```bash
dotnet build --configuration Release
dotnet publish --configuration Release
```

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Features Overview

### Job Post Management

- Create and manage job postings
- Set requirements, skills, and responsibilities
- Track application status and closing dates

### Applicant Management

- Add and manage job applicants
- Track application status
- Upload and manage CV files

### AI-Powered Screening

- Automated CV analysis using Gemini AI
- Score applicants based on job requirements
- Generate detailed screening reports

### User Authentication

- Secure user registration and login
- JWT-based authentication for API access
- Cookie-based authentication for web interface

## Troubleshooting

### Common Issues

1. **Database Connection Issues**

   - Verify PostgreSQL is running
   - Check connection string in appsettings.json
   - Ensure database exists

2. **File Upload Issues**

   - Check file size limits
   - Verify upload directory permissions
   - Ensure allowed file extensions

3. **AI Screening Not Working**
   - Verify Gemini AI API key is configured
   - Check API quota and limits
   - Review error logs for specific issues

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
