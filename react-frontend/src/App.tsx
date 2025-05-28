import React, { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import './App.css';

interface SampleFile {
  files: string[];
}

interface ApiResponse {
  success: boolean;
  contentId?: string;
  fileName?: string;
  error?: string;
}

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:54568';

function App() {
  const [htmlContent, setHtmlContent] = useState('');
  const [previewUrl, setPreviewUrl] = useState(`${API_BASE_URL}/welcome`);
  const [status, setStatus] = useState('Ready');
  const [isError, setIsError] = useState(false);
  const [sampleFiles, setSampleFiles] = useState<string[]>([]);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const showStatus = (message: string, error: boolean = false) => {
    setStatus(message);
    setIsError(error);
    setTimeout(() => {
      setStatus('Ready');
      setIsError(false);
    }, 3000);
  };

  const loadSamples = useCallback(async () => {
    try {
      const response = await axios.get<SampleFile>(`${API_BASE_URL}/samples`);
      setSampleFiles(response.data.files);
    } catch (error) {
      showStatus('Error loading samples', true);
    }
  }, []);

  useEffect(() => {
    loadSamples();
  }, [loadSamples]);

  const renderHtmlContent = async () => {
    if (!htmlContent.trim()) {
      showStatus('Please enter some HTML content', true);
      return;
    }

    try {
      showStatus('Rendering HTML content...');
      const response = await axios.post<ApiResponse>(`${API_BASE_URL}/render-content`, {
        source: 'string',
        content: htmlContent
      });

      if (response.data.success && response.data.contentId) {
        setPreviewUrl(`${API_BASE_URL}/content/${response.data.contentId}`);
        showStatus('HTML content rendered successfully!');
      } else {
        showStatus(response.data.error || 'Error rendering content', true);
      }
    } catch (error) {
      showStatus('Error rendering content', true);
    }
  };

  const handleFileUpload = async () => {
    if (!selectedFile) {
      showStatus('Please select a file', true);
      return;
    }

    const formData = new FormData();
    formData.append('file', selectedFile);

    try {
      showStatus('Uploading and rendering file...');
      const response = await axios.post<ApiResponse>(`${API_BASE_URL}/upload-content`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });

      if (response.data.success && response.data.contentId) {
        setPreviewUrl(`${API_BASE_URL}/content/${response.data.contentId}`);
        showStatus(`File "${response.data.fileName}" uploaded and rendered successfully!`);
        setSelectedFile(null);
      } else {
        showStatus(response.data.error || 'Error uploading file', true);
      }
    } catch (error) {
      showStatus('Error uploading file', true);
    }
  };

  const loadSample = async (fileName: string) => {
    try {
      showStatus(`Loading sample: ${fileName}`);
      const response = await axios.get<ApiResponse>(`${API_BASE_URL}/samples-content/${fileName}`);

      if (response.data.success && response.data.contentId) {
        setPreviewUrl(`${API_BASE_URL}/content/${response.data.contentId}`);
        showStatus('Sample file loaded successfully!');
      } else {
        showStatus(response.data.error || 'Error loading sample', true);
      }
    } catch (error) {
      showStatus('Error loading sample', true);
    }
  };

  return (
    <div className="App">
      <div className="container">
        <div className="control-panel">
          <div className="header">
            <h1>🚀 HTML Renderer</h1>
            <p>Load and render HTML content from various sources</p>
          </div>

          <div className="status-bar" style={{ color: isError ? '#e74c3c' : '#27ae60' }}>
            {status}
          </div>

          <div className="section">
            <h3>📝 HTML Content Input</h3>
            <textarea
              value={htmlContent}
              onChange={(e) => setHtmlContent(e.target.value)}
              placeholder="Enter your HTML content here..."
              rows={8}
            />
            <button onClick={renderHtmlContent} className="primary-button">
              Render HTML
            </button>
          </div>

          <div className="section">
            <h3>📁 File Upload</h3>
            <input
              type="file"
              accept=".html"
              onChange={(e) => setSelectedFile(e.target.files?.[0] || null)}
            />
            <button onClick={handleFileUpload} className="primary-button" disabled={!selectedFile}>
              Upload & Render
            </button>
          </div>

          <div className="section">
            <h3>📋 Sample Files</h3>
            <div className="samples-list">
              {sampleFiles.map((file) => (
                <button
                  key={file}
                  onClick={() => loadSample(file)}
                  className="sample-button"
                >
                  {file}
                </button>
              ))}
            </div>
          </div>
        </div>

        <div className="preview-panel">
          <div className="preview-header">
            <h2>Preview</h2>
            <button
              onClick={() => setPreviewUrl(previewUrl + '?refresh=' + Date.now())}
              className="refresh-button"
            >
              🔄 Refresh
            </button>
          </div>
          <iframe
            src={previewUrl}
            title="HTML Preview"
            className="preview-iframe"
          />
        </div>
      </div>
    </div>
  );
}

export default App;
