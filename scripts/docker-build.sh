#!/bin/bash

# Docker build script for Smart CV Filter application

set -e

echo "🐳 Building Smart CV Filter Docker containers..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    print_error "docker-compose is not installed. Please install docker-compose and try again."
    exit 1
fi

# Parse command line arguments
CLEAN_BUILD=false
PUSH_IMAGES=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --clean)
            CLEAN_BUILD=true
            shift
            ;;
        --push)
            PUSH_IMAGES=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --clean    Clean build (remove existing images)"
            echo "  --push     Push images to registry after build"
            echo "  -h, --help Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Clean build if requested
if [ "$CLEAN_BUILD" = true ]; then
    print_status "Cleaning existing images..."
    docker-compose down --rmi all --volumes --remove-orphans 2>/dev/null || true
    docker system prune -f
fi

# Set compose file
COMPOSE_FILE="docker-compose.yml"
print_status "Building Smart CV Filter application..."

# Build images
print_status "Building Docker images..."
docker-compose -f $COMPOSE_FILE build --no-cache

if [ $? -eq 0 ]; then
    print_success "Docker images built successfully!"
else
    print_error "Failed to build Docker images"
    exit 1
fi

# Push images if requested
if [ "$PUSH_IMAGES" = true ]; then
    print_status "Pushing images to registry..."
    docker-compose -f $COMPOSE_FILE push
    
    if [ $? -eq 0 ]; then
        print_success "Images pushed successfully!"
    else
        print_error "Failed to push images"
        exit 1
    fi
fi

print_success "Build completed successfully!"
print_status "To start the application, run: ./scripts/docker-run.sh"
