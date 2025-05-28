#!/bin/bash

# HTML Renderer Docker Build and Run Script
# This script builds and runs the HTML Renderer application in a Docker container

set -e

echo "🐳 HTML Renderer Docker Setup"
echo "=============================="

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    echo "Visit: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

echo "✅ Docker is available and running"

# Build the Docker image
echo "🔨 Building Docker image..."
docker build -t html-renderer:latest .

if [ $? -eq 0 ]; then
    echo "✅ Docker image built successfully"
else
    echo "❌ Failed to build Docker image"
    exit 1
fi

# Stop and remove existing container if it exists
echo "🧹 Cleaning up existing container..."
docker stop html-renderer-app 2>/dev/null || true
docker rm html-renderer-app 2>/dev/null || true

# Run the container
echo "🚀 Starting HTML Renderer container..."
docker run -d \
    --name html-renderer-app \
    -p 8080:8080 \
    --restart unless-stopped \
    html-renderer:latest

if [ $? -eq 0 ]; then
    echo "✅ Container started successfully"
    echo ""
    echo "🌐 Application is now running at:"
    echo "   http://localhost:8080"
    echo "   http://localhost:8080/welcome (Welcome page)"
    echo "   http://localhost:8080/control (Control panel)"
    echo ""
    echo "📋 Container management commands:"
    echo "   View logs:    docker logs html-renderer-app"
    echo "   Stop:         docker stop html-renderer-app"
    echo "   Start:        docker start html-renderer-app"
    echo "   Remove:       docker rm html-renderer-app"
    echo ""
    echo "🔍 Container status:"
    docker ps | grep html-renderer-app
else
    echo "❌ Failed to start container"
    exit 1
fi