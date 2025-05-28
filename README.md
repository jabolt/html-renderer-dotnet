# HTML Renderer - .NET Application

A .NET 8 ASP.NET Core web application that demonstrates loading and rendering HTML content from various sources.

## Features

- **String-based HTML rendering**: Load HTML content from string variables
- **File-based HTML rendering**: Load HTML content from files
- **File upload support**: Upload and render HTML files through a web interface
- **Sample HTML files**: Pre-built examples demonstrating different capabilities
- **Interactive control panel**: Web-based interface for testing different rendering methods
- **CORS enabled**: Allows cross-origin requests
- **Real-time rendering**: Immediate display of HTML content in the browser

## Getting Started

### Prerequisites
- .NET 8 SDK

### Running the Application

1. Clone or download the project
2. Navigate to the project directory
3. Build the application:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. Open your browser and navigate to `http://localhost:54568`

## Available Endpoints

### Main Routes
- **`/`** - Displays sample HTML content loaded from a string
- **`/control`** - Interactive control panel for testing different rendering methods

### API Endpoints
- **`POST /render`** - Render HTML from string or file
  ```json
  {
    "source": "string|file",
    "content": "HTML content (for string source)",
    "filePath": "path/to/file.html (for file source)"
  }
  ```
- **`POST /upload`** - Upload and render HTML file
- **`GET /samples`** - Get list of available sample files
- **`GET /samples/{fileName}`** - Render a specific sample file

## Sample Files

The application includes three sample HTML files in the `samples/` directory:

1. **`simple.html`** - Basic HTML with modern CSS styling
2. **`interactive.html`** - Interactive elements with JavaScript functionality
3. **`dashboard.html`** - Complex dashboard with real-time updates and animations

## Usage Examples

### 1. Basic HTML Rendering
Visit `http://localhost:54568` to see HTML content loaded from a string variable.

### 2. Interactive Control Panel
Visit `http://localhost:54568/control` to:
- Enter custom HTML content and render it
- Upload HTML files for rendering
- Browse and load sample files

### 3. API Usage
```bash
# Render HTML from string
curl -X POST http://localhost:54568/render \
  -H "Content-Type: application/json" \
  -d '{"source": "string", "content": "<h1>Hello World!</h1>"}'

# Render HTML from file
curl -X POST http://localhost:54568/render \
  -H "Content-Type: application/json" \
  -d '{"source": "file", "filePath": "/path/to/your/file.html"}'
```

## Key Implementation Details

- **Minimal API**: Uses .NET 8's minimal API approach for clean, concise code
- **Content-Type handling**: Properly sets `text/html` content type for browser rendering
- **Error handling**: Comprehensive error handling for file operations and invalid requests
- **Security**: File upload validation to ensure only HTML files are processed
- **Performance**: Async/await pattern for file I/O operations

## Architecture

The application demonstrates several important concepts:

1. **String-based HTML rendering**: Shows how to serve HTML content stored as strings
2. **File-based rendering**: Demonstrates reading HTML files and serving their content
3. **Dynamic content**: Includes server-side data (like current timestamp) in rendered HTML
4. **Interactive features**: JavaScript functionality works seamlessly in rendered content
5. **RESTful API design**: Clean API endpoints for different rendering scenarios

## Extending the Application

You can easily extend this application by:

- Adding more sample HTML files to the `samples/` directory
- Implementing HTML template engines (Razor, Handlebars, etc.)
- Adding authentication and authorization
- Implementing HTML content validation and sanitization
- Adding database storage for HTML templates
- Creating a rich text editor for HTML content creation

## Technical Notes

- The application listens on `http://0.0.0.0:54568` to allow external access
- CORS is enabled for cross-origin requests
- Static file serving is configured (though no static files are currently used)
- The application uses the development environment by default

This application serves as a solid foundation for any system that needs to dynamically load and render HTML content in a .NET environment.