# Docker Deployment Guide

This guide explains how to build and run the HTML Renderer application using Docker containers.

## 🐳 Prerequisites

- Docker installed on your system
- Docker Compose (optional, for easier management)

### Installing Docker

**Windows/Mac:**
- Download Docker Desktop from [https://docs.docker.com/get-docker/](https://docs.docker.com/get-docker/)

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get update
sudo apt-get install docker.io docker-compose
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker $USER
```

## 🚀 Quick Start

### Option 1: Using the Build Script (Recommended)

```bash
# Make the script executable
chmod +x docker-run.sh

# Build and run the container
./docker-run.sh
```

### Option 2: Using Docker Commands

```bash
# Build the image
docker build -t html-renderer:latest .

# Run the container
docker run -d \
    --name html-renderer-app \
    -p 8080:8080 \
    --restart unless-stopped \
    html-renderer:latest
```

### Option 3: Using Docker Compose

```bash
# Build and start the service
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the service
docker-compose down
```

## 🌐 Accessing the Application

Once the container is running, you can access:

- **Welcome Page**: [http://localhost:8080/welcome](http://localhost:8080/welcome)
- **Control Panel**: [http://localhost:8080/control](http://localhost:8080/control)
- **API Endpoints**: [http://localhost:8080/](http://localhost:8080/)

## 📋 Container Management

### View Container Status
```bash
docker ps | grep html-renderer
```

### View Application Logs
```bash
docker logs html-renderer-app
docker logs -f html-renderer-app  # Follow logs in real-time
```

### Stop the Container
```bash
docker stop html-renderer-app
```

### Start the Container
```bash
docker start html-renderer-app
```

### Remove the Container
```bash
docker stop html-renderer-app
docker rm html-renderer-app
```

### Remove the Image
```bash
docker rmi html-renderer:latest
```

## 🔧 Configuration

### Environment Variables

The container supports the following environment variables:

- `ASPNETCORE_ENVIRONMENT`: Set to `Production` (default) or `Development`
- `ASPNETCORE_URLS`: Set to `http://+:8080` (default)

### Custom Port Mapping

To run on a different port:

```bash
docker run -d \
    --name html-renderer-app \
    -p 3000:8080 \
    html-renderer:latest
```

Then access at [http://localhost:3000](http://localhost:3000)

### Volume Mounting

To persist or customize sample files:

```bash
docker run -d \
    --name html-renderer-app \
    -p 8080:8080 \
    -v $(pwd)/custom-samples:/app/samples:ro \
    html-renderer:latest
```

## 🏗️ Building from Source

### Dockerfile Explanation

The Dockerfile uses a multi-stage build:

1. **Build Stage**: Uses .NET 8 SDK to compile the application
2. **Runtime Stage**: Uses .NET 8 runtime for a smaller final image
3. **Security**: Runs as non-root user `appuser`
4. **Optimization**: Excludes unnecessary files using `.dockerignore`

### Build Arguments

```bash
# Build with specific configuration
docker build \
    --build-arg BUILD_CONFIGURATION=Release \
    -t html-renderer:latest .
```

## 🔍 Troubleshooting

### Container Won't Start

1. Check if port 8080 is already in use:
   ```bash
   netstat -tulpn | grep 8080
   ```

2. View container logs:
   ```bash
   docker logs html-renderer-app
   ```

3. Check Docker daemon status:
   ```bash
   docker info
   ```

### Application Not Accessible

1. Verify container is running:
   ```bash
   docker ps
   ```

2. Check port mapping:
   ```bash
   docker port html-renderer-app
   ```

3. Test from inside container:
   ```bash
   docker exec -it html-renderer-app curl http://localhost:8080/welcome
   ```

### Performance Issues

1. Check container resource usage:
   ```bash
   docker stats html-renderer-app
   ```

2. Increase memory limit:
   ```bash
   docker run -d \
       --name html-renderer-app \
       -p 8080:8080 \
       --memory=512m \
       html-renderer:latest
   ```

## 🔒 Security Considerations

- The application runs as a non-root user (`appuser`)
- Only necessary ports are exposed
- Sample files are mounted read-only when using volumes
- No sensitive data is included in the image
- CORS is configured for development convenience

## 📊 Health Checks

The Docker Compose configuration includes health checks:

```bash
# Check health status
docker-compose ps

# View health check logs
docker inspect html-renderer-app | grep -A 10 Health
```

## 🚀 Production Deployment

For production deployment, consider:

1. **Reverse Proxy**: Use nginx or Apache as a reverse proxy
2. **SSL/TLS**: Terminate SSL at the proxy level
3. **Monitoring**: Add logging and monitoring solutions
4. **Scaling**: Use Docker Swarm or Kubernetes for scaling
5. **Secrets**: Use Docker secrets for sensitive configuration

### Example with nginx

```yaml
version: '3.8'
services:
  html-renderer:
    build: .
    expose:
      - "8080"
    
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - html-renderer
```

## 📝 Notes

- The container automatically starts the application on port 8080
- All application features are available in the containerized version
- Sample HTML files are included in the container
- The application supports file uploads and string input as in the standalone version