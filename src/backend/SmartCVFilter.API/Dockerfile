# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SmartCVFilter.API.csproj", "./"]
RUN dotnet restore "SmartCVFilter.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "SmartCVFilter.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SmartCVFilter.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create directories for uploads and logs
RUN mkdir -p /app/uploads/cvs /app/logs

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "SmartCVFilter.API.dll"]

