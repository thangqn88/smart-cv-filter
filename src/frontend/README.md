# Smart CV Filter - Frontend

A modern .NET 8 MVC web application for managing job postings, applicants, and AI-powered resume screening.

## Features

### 🎯 Core Functionality

- **Job Posting Management**: Create, edit, and manage job postings with detailed requirements
- **Applicant Management**: Track and manage job applicants with comprehensive profiles
- **AI Screening Integration**: Automated resume screening with detailed AI analysis
- **User Authentication**: Secure login and registration system
- **Role Management**: User and role management (placeholder for future implementation)

### 🎨 User Interface

- **Modern Design**: Clean, responsive interface using Bootstrap 5.3
- **Left-side Navigation**: Intuitive sidebar navigation with icons
- **Dashboard**: Overview with statistics and quick actions
- **Data Tables**: Sortable, searchable tables with filtering
- **Real-time Preview**: Live preview for forms
- **Responsive Design**: Mobile-friendly interface

### 🔧 Technical Features

- **.NET 8 MVC**: Latest ASP.NET Core MVC framework
- **API Integration**: RESTful API communication with backend services
- **Authentication**: JWT-based authentication with cookie storage
- **Form Validation**: Client-side and server-side validation
- **Error Handling**: Comprehensive error handling and user feedback
- **Print Support**: Print-friendly views for reports

## Project Structure

```
src/frontend/SmartCVFilter.Web/
├── Controllers/           # MVC Controllers
│   ├── AuthController.cs
│   ├── HomeController.cs
│   ├── JobPostsController.cs
│   ├── ApplicantsController.cs
│   ├── ScreeningController.cs
│   ├── UsersController.cs
│   └── RolesController.cs
├── Models/               # Data Models
│   ├── AuthModels.cs
│   ├── JobPostModels.cs
│   └── ApplicantModels.cs
├── Services/            # API Services
│   ├── IApiService.cs
│   ├── ApiService.cs
│   ├── JobPostService.cs
│   ├── ApplicantService.cs
│   └── ScreeningService.cs
├── Views/               # MVC Views
│   ├── Shared/          # Layout and shared views
│   ├── Home/            # Dashboard views
│   ├── Auth/            # Authentication views
│   ├── JobPosts/        # Job posting management
│   ├── Applicants/      # Applicant management
│   ├── Screening/       # AI screening results
│   ├── Users/           # User management
│   └── Roles/           # Role management
├── wwwroot/             # Static files
│   ├── css/             # Custom styles
│   ├── js/              # JavaScript files
│   └── lib/             # Third-party libraries
├── appsettings.json     # Configuration
└── Program.cs           # Application startup
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Backend API running (see backend README)

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd src/frontend
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Update configuration**

   - Update `appsettings.json` with your backend API URL
   - Configure JWT settings if needed

4. **Run the application**

   ```bash
   dotnet run
   ```

5. **Access the application**
   - Navigate to `https://localhost:5001` (or the configured port)
   - Register a new account or login with existing credentials

### Configuration

#### API Settings

Update the API base URL in `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001/api",
    "Timeout": 30
  }
}
```

#### JWT Settings

Configure JWT settings for authentication:

```json
{
  "JwtSettings": {
    "Issuer": "SmartCVFilter",
    "Audience": "SmartCVFilter",
    "SecretKey": "YourSecretKeyHereThatIsAtLeast32CharactersLong"
  }
}
```

## Features Overview

### Dashboard

- Overview statistics (total job posts, active posts, applicants)
- Recent job posts with quick actions
- Quick action buttons for common tasks

### Job Posting Management

- **Create Job Posts**: Comprehensive form with live preview
- **Edit Job Posts**: Update existing job postings
- **View Details**: Detailed job post information
- **Delete Job Posts**: Remove job postings
- **Search & Filter**: Find job posts by status, location, etc.

### Applicant Management

- **Add Applicants**: Manual applicant entry
- **Edit Applicants**: Update applicant information
- **View Details**: Comprehensive applicant profiles
- **AI Screening**: Start AI screening for selected applicants
- **Status Management**: Track applicant status

### AI Screening Results

- **View Results**: Detailed AI analysis results
- **Score Visualization**: Progress bars and score indicators
- **Strengths & Weaknesses**: Categorized analysis
- **Detailed Analysis**: Comprehensive AI insights
- **Export & Print**: Export results to CSV or print

### User Interface Features

- **Responsive Design**: Works on desktop, tablet, and mobile
- **Modern UI**: Clean, professional interface
- **Interactive Elements**: Tooltips, modals, and animations
- **Form Validation**: Real-time validation feedback
- **Search & Filter**: Advanced filtering capabilities

## API Integration

The frontend communicates with the backend API through service classes:

- **ApiService**: Handles authentication and token management
- **JobPostService**: Manages job posting operations
- **ApplicantService**: Handles applicant management
- **ScreeningService**: Manages AI screening results

All API calls include proper error handling and user feedback.

## Styling and Theming

The application uses:

- **Bootstrap 5.3**: Latest Bootstrap framework
- **Bootstrap Icons**: Comprehensive icon set
- **Custom CSS**: Additional styling in `wwwroot/css/site.css`
- **Responsive Design**: Mobile-first approach

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Development

### Adding New Features

1. Create models in the `Models/` directory
2. Add API services in the `Services/` directory
3. Create controllers in the `Controllers/` directory
4. Add views in the appropriate `Views/` subdirectory
5. Update navigation in `Views/Shared/_Layout.cshtml`

### Code Style

- Follow C# naming conventions
- Use async/await for API calls
- Implement proper error handling
- Add validation attributes to models
- Use dependency injection for services

## Deployment

### Production Configuration

1. Update `appsettings.Production.json` with production settings
2. Configure HTTPS certificates
3. Set up proper logging
4. Configure database connection strings
5. Set up reverse proxy (IIS, Nginx, etc.)

### Docker Support

The application can be containerized using Docker:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "SmartCVFilter.Web.dll"]
```

## Troubleshooting

### Common Issues

1. **API Connection Errors**: Check backend API URL and ensure it's running
2. **Authentication Issues**: Verify JWT settings and token storage
3. **CORS Errors**: Ensure backend CORS is properly configured
4. **Validation Errors**: Check model validation attributes

### Logs

Application logs are available in the console output and can be configured in `appsettings.json`.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:

- Create an issue in the repository
- Check the documentation
- Review the code comments

---

**Smart CV Filter Frontend** - AI-Powered Resume Screening Made Simple
