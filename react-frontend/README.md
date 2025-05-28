# HTML Renderer - React Frontend

A modern React TypeScript frontend for the HTML Renderer application. This frontend provides an intuitive interface for loading, rendering, and previewing HTML content from various sources.

## Features

- **Modern React UI**: Built with React 18 and TypeScript for type safety
- **Responsive Design**: Works seamlessly on desktop and mobile devices
- **Real-time Preview**: Live preview of rendered HTML content in an iframe
- **Multiple Input Methods**:
  - Direct HTML content input via textarea
  - File upload support for HTML files
  - Pre-loaded sample files
- **Status Feedback**: Real-time status updates for all operations
- **Beautiful UI**: Modern glassmorphism design with smooth animations

## Getting Started

### Prerequisites
- Node.js 16+ 
- npm or yarn
- The .NET backend running on port 54568

### Installation

1. Install dependencies:
   ```bash
   npm install
   ```

2. Start the development server:
   ```bash
   npm start
   ```

3. The application will be available at `http://localhost:52703`

### Environment Configuration

The application uses the following environment variables (configured in `.env`):

- `PORT=52703` - Port for the React development server
- `REACT_APP_API_URL=http://localhost:54568` - URL of the .NET backend API
- `GENERATE_SOURCEMAP=false` - Disable source maps for production

## Architecture

### Components Structure

- **App.tsx**: Main application component containing all functionality
- **App.css**: Comprehensive styling with modern design patterns

### Key Features

1. **HTML Content Input**: 
   - Large textarea for direct HTML input
   - Syntax highlighting-friendly monospace font
   - Real-time validation

2. **File Upload**:
   - Drag-and-drop file input
   - HTML file validation
   - Progress feedback

3. **Sample Files**:
   - Dynamic loading of available samples
   - One-click sample loading
   - Visual feedback for loaded samples

4. **Preview Panel**:
   - Secure iframe rendering
   - Refresh functionality
   - Responsive design

### API Integration

The frontend communicates with the .NET backend through these endpoints:

- `GET /samples` - Fetch available sample files
- `POST /render-content` - Render HTML from string input
- `POST /upload-content` - Upload and render HTML files
- `GET /samples-content/{fileName}` - Load specific sample files
- `GET /content/{id}` - Retrieve rendered content by ID
- `GET /welcome` - Welcome page for initial preview

## Available Scripts

- `npm start` - Start development server
- `npm build` - Build for production
- `npm test` - Run tests
- `npm eject` - Eject from Create React App

## Production Build

To create a production build:

```bash
npm run build
```

The build artifacts will be stored in the `build/` directory.