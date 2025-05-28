using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddAntiforgery();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:52703", "http://localhost:56781")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// In-memory storage for HTML content with unique IDs
var htmlStorage = new ConcurrentDictionary<string, string>();

// Configure the HTTP request pipeline
app.UseCors();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

// Configure URLs to listen on all interfaces
app.Urls.Add("http://0.0.0.0:54568");

// Sample HTML content for demonstration
var sampleHtmlContent = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Sample HTML Content</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background-color: #f5f5f5; }
        .container { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #333; border-bottom: 2px solid #007acc; padding-bottom: 10px; }
        .highlight { background-color: #fff3cd; padding: 10px; border-left: 4px solid #ffc107; margin: 20px 0; }
        .button { background-color: #007acc; color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; }
        .button:hover { background-color: #005a9e; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>HTML Renderer Demo</h1>
        <p>This is a sample HTML content loaded and rendered by the .NET application.</p>
        <div class='highlight'>
            <strong>Note:</strong> This HTML content was loaded from a string variable and is being rendered in the browser.
        </div>
        <p>You can include:</p>
        <ul>
            <li>Styled content with CSS</li>
            <li>Interactive elements</li>
            <li>Images and media</li>
            <li>JavaScript functionality</li>
        </ul>
        <button class='button' onclick='alert(""Hello from rendered HTML!"")'>Click Me!</button>
        <p><em>Current time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</em></p>
    </div>
</body>
</html>";

// Main route - serves the HTML content
app.MapGet("/", () => Results.Content(sampleHtmlContent, "text/html"));

// API endpoint to load HTML from different sources
app.MapPost("/render", async ([FromBody] HtmlRequest request) =>
{
    try
    {
        string htmlContent;
        
        switch (request.Source?.ToLower())
        {
            case "file":
                if (string.IsNullOrEmpty(request.FilePath))
                    return Results.BadRequest("FilePath is required when source is 'file'");
                
                if (!System.IO.File.Exists(request.FilePath))
                    return Results.NotFound($"File not found: {request.FilePath}");
                
                htmlContent = await System.IO.File.ReadAllTextAsync(request.FilePath);
                break;
                
            case "string":
            default:
                htmlContent = request.Content ?? sampleHtmlContent;
                break;
        }
        
        // Generate unique ID and store content
        var contentId = Guid.NewGuid().ToString();
        htmlStorage[contentId] = htmlContent;
        
        // Return page with iframe
        var iframePage = $@"
<!DOCTYPE html>
<html>
<head>
    <title>HTML Renderer</title>
    <style>
        body {{ margin: 0; padding: 0; font-family: Arial, sans-serif; }}
        .header {{ background: #007acc; color: white; padding: 10px; text-align: center; }}
        .iframe-container {{ width: 100%; height: calc(100vh - 60px); }}
        iframe {{ width: 100%; height: 100%; border: none; }}
        .controls {{ position: fixed; top: 10px; right: 10px; z-index: 1000; }}
        .controls button {{ background: rgba(0,0,0,0.7); color: white; border: none; padding: 5px 10px; margin: 2px; border-radius: 3px; cursor: pointer; }}
        .controls button:hover {{ background: rgba(0,0,0,0.9); }}
    </style>
</head>
<body>
    <div class=""header"">
        <h2>HTML Renderer - Content ID: {contentId}</h2>
    </div>
    <div class=""controls"">
        <button onclick=""window.location.href='/control'"">← Back to Control Panel</button>
        <button onclick=""document.querySelector('iframe').src = document.querySelector('iframe').src"">🔄 Refresh</button>
    </div>
    <div class=""iframe-container"">
        <iframe src=""/content/{contentId}"" title=""Rendered HTML Content""></iframe>
    </div>
</body>
</html>";
        
        return Results.Content(iframePage, "text/html");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error rendering HTML: {ex.Message}");
    }
});

// API endpoint to upload and render HTML file
app.MapPost("/upload", async (IFormFile file) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("No file uploaded");
    
    if (!file.FileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest("Only HTML files are allowed");
    
    try
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var htmlContent = await reader.ReadToEndAsync();
        
        // Generate unique ID and store content
        var contentId = Guid.NewGuid().ToString();
        htmlStorage[contentId] = htmlContent;
        
        // Return page with iframe
        var iframePage = $@"
<!DOCTYPE html>
<html>
<head>
    <title>HTML Renderer - Uploaded File</title>
    <style>
        body {{ margin: 0; padding: 0; font-family: Arial, sans-serif; }}
        .header {{ background: #007acc; color: white; padding: 10px; text-align: center; }}
        .iframe-container {{ width: 100%; height: calc(100vh - 60px); }}
        iframe {{ width: 100%; height: 100%; border: none; }}
        .controls {{ position: fixed; top: 10px; right: 10px; z-index: 1000; }}
        .controls button {{ background: rgba(0,0,0,0.7); color: white; border: none; padding: 5px 10px; margin: 2px; border-radius: 3px; cursor: pointer; }}
        .controls button:hover {{ background: rgba(0,0,0,0.9); }}
    </style>
</head>
<body>
    <div class=""header"">
        <h2>HTML Renderer - Uploaded: {file.FileName}</h2>
    </div>
    <div class=""controls"">
        <button onclick=""window.location.href='/control'"">← Back to Control Panel</button>
        <button onclick=""document.querySelector('iframe').src = document.querySelector('iframe').src"">🔄 Refresh</button>
    </div>
    <div class=""iframe-container"">
        <iframe src=""/content/{contentId}"" title=""Uploaded HTML Content""></iframe>
    </div>
</body>
</html>";
        
        return Results.Content(iframePage, "text/html");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error processing uploaded file: {ex.Message}");
    }
}).DisableAntiforgery();

// API endpoint to get available sample files
app.MapGet("/samples", () =>
{
    var samplesDir = Path.Combine(Directory.GetCurrentDirectory(), "samples");
    if (!Directory.Exists(samplesDir))
        return Results.Ok(new { files = new string[0] });
    
    var htmlFiles = Directory.GetFiles(samplesDir, "*.html")
                             .Select(Path.GetFileName)
                             .ToArray();
    
    return Results.Ok(new { files = htmlFiles });
});

// API endpoint to render a sample file
app.MapGet("/samples/{fileName}", async (string fileName) =>
{
    var samplesDir = Path.Combine(Directory.GetCurrentDirectory(), "samples");
    var filePath = Path.Combine(samplesDir, fileName);
    
    if (!System.IO.File.Exists(filePath) || !fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        return Results.NotFound("Sample file not found");
    
    try
    {
        var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);
        
        // Generate unique ID and store content
        var contentId = Guid.NewGuid().ToString();
        htmlStorage[contentId] = htmlContent;
        
        // Return page with iframe
        var iframePage = $@"
<!DOCTYPE html>
<html>
<head>
    <title>HTML Renderer - Sample File</title>
    <style>
        body {{ margin: 0; padding: 0; font-family: Arial, sans-serif; }}
        .header {{ background: #007acc; color: white; padding: 10px; text-align: center; }}
        .iframe-container {{ width: 100%; height: calc(100vh - 60px); }}
        iframe {{ width: 100%; height: 100%; border: none; }}
        .controls {{ position: fixed; top: 10px; right: 10px; z-index: 1000; }}
        .controls button {{ background: rgba(0,0,0,0.7); color: white; border: none; padding: 5px 10px; margin: 2px; border-radius: 3px; cursor: pointer; }}
        .controls button:hover {{ background: rgba(0,0,0,0.9); }}
    </style>
</head>
<body>
    <div class=""header"">
        <h2>HTML Renderer - Sample: {fileName}</h2>
    </div>
    <div class=""controls"">
        <button onclick=""window.location.href='/control'"">← Back to Control Panel</button>
        <button onclick=""document.querySelector('iframe').src = document.querySelector('iframe').src"">🔄 Refresh</button>
    </div>
    <div class=""iframe-container"">
        <iframe src=""/content/{contentId}"" title=""Sample HTML Content""></iframe>
    </div>
</body>
</html>";
        
        return Results.Content(iframePage, "text/html");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error loading sample file: {ex.Message}");
    }
});

// Endpoint to serve HTML content by ID (for iframe)
app.MapGet("/content/{id}", (string id) =>
{
    if (htmlStorage.TryGetValue(id, out var content))
    {
        return Results.Content(content, "text/html");
    }
    return Results.NotFound("Content not found");
});

// Control panel for testing
// Welcome page for the preview iframe
app.MapGet("/welcome", () => Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>Welcome</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            margin: 0; 
            padding: 40px; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-align: center;
            min-height: calc(100vh - 80px);
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
        }
        .welcome-container {
            background: rgba(255,255,255,0.1);
            padding: 40px;
            border-radius: 15px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255,255,255,0.2);
            max-width: 600px;
        }
        h1 { 
            font-size: 2.5em; 
            margin-bottom: 20px; 
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }
        p { 
            font-size: 1.2em; 
            line-height: 1.6; 
            margin-bottom: 15px;
            opacity: 0.9;
        }
        .features {
            text-align: left;
            margin: 30px 0;
        }
        .feature {
            margin: 10px 0;
            padding: 10px;
            background: rgba(255,255,255,0.1);
            border-radius: 8px;
            border-left: 4px solid #fff;
        }
        .icon {
            font-size: 1.5em;
            margin-right: 10px;
        }
    </style>
</head>
<body>
    <div class='welcome-container'>
        <h1>🚀 HTML Renderer</h1>
        <p>Welcome to the HTML Renderer application!</p>
        <p>Use the control panel on the left to load and render HTML content.</p>
        
        <div class='features'>
            <div class='feature'>
                <span class='icon'>📝</span>
                <strong>String Input:</strong> Enter HTML directly in the textarea
            </div>
            <div class='feature'>
                <span class='icon'>📁</span>
                <strong>File Upload:</strong> Upload HTML files from your computer
            </div>
            <div class='feature'>
                <span class='icon'>📋</span>
                <strong>Sample Files:</strong> Try pre-loaded sample HTML files
            </div>
        </div>
        
        <p>All content will be rendered securely in this preview panel with full JavaScript support!</p>
    </div>
</body>
</html>", "text/html"));

// API endpoint to render content and return JSON with content ID
app.MapPost("/render-content", async ([FromBody] HtmlRequest request) =>
{
    try
    {
        string htmlContent;
        
        switch (request.Source?.ToLower())
        {
            case "file":
                if (string.IsNullOrEmpty(request.FilePath))
                    return Results.Json(new { success = false, error = "FilePath is required when source is 'file'" });
                
                if (!System.IO.File.Exists(request.FilePath))
                    return Results.Json(new { success = false, error = $"File not found: {request.FilePath}" });
                
                htmlContent = await System.IO.File.ReadAllTextAsync(request.FilePath);
                break;
                
            case "string":
            default:
                htmlContent = request.Content ?? sampleHtmlContent;
                break;
        }
        
        // Generate unique ID and store content
        var contentId = Guid.NewGuid().ToString();
        htmlStorage[contentId] = htmlContent;
        
        return Results.Json(new { success = true, contentId = contentId });
    }
    catch (Exception ex)
    {
        return Results.Json(new { success = false, error = ex.Message });
    }
});

// API endpoint to upload file and return JSON with content ID
app.MapPost("/upload-content", async (IFormFile file) =>
{
    if (file == null || file.Length == 0)
        return Results.Json(new { success = false, error = "No file uploaded" });
    
    if (!file.FileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        return Results.Json(new { success = false, error = "Only HTML files are allowed" });
    
    try
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var htmlContent = await reader.ReadToEndAsync();
        
        // Generate unique ID and store content
        var contentId = Guid.NewGuid().ToString();
        htmlStorage[contentId] = htmlContent;
        
        return Results.Json(new { success = true, contentId = contentId, fileName = file.FileName });
    }
    catch (Exception ex)
    {
        return Results.Json(new { success = false, error = ex.Message });
    }
}).DisableAntiforgery();

// API endpoint to load sample and return JSON with content ID
app.MapGet("/samples-content/{fileName}", async (string fileName) =>
{
    try
    {
        var samplesPath = Path.Combine(Directory.GetCurrentDirectory(), "samples");
        var filePath = Path.Combine(samplesPath, fileName);
        
        if (!System.IO.File.Exists(filePath))
            return Results.Json(new { success = false, error = "Sample file not found" });
        
        var htmlContent = await System.IO.File.ReadAllTextAsync(filePath);
        
        // Generate unique ID and store content
        var contentId = Guid.NewGuid().ToString();
        htmlStorage[contentId] = htmlContent;
        
        return Results.Json(new { success = true, contentId = contentId, fileName = fileName });
    }
    catch (Exception ex)
    {
        return Results.Json(new { success = false, error = ex.Message });
    }
});

app.MapGet("/control", () => Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>HTML Renderer Control Panel</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            margin: 0; 
            padding: 0; 
            background-color: #f0f0f0; 
            height: 100vh;
            overflow: hidden;
        }
        .container {
            display: flex;
            height: 100vh;
        }
        .control-panel {
            width: 40%;
            padding: 20px;
            background-color: #f0f0f0;
            overflow-y: auto;
            border-right: 2px solid #ddd;
        }
        .preview-panel {
            width: 60%;
            background-color: white;
            position: relative;
        }
        .panel { 
            background: white; 
            padding: 20px; 
            border-radius: 8px; 
            margin-bottom: 20px; 
            box-shadow: 0 2px 5px rgba(0,0,0,0.1); 
        }
        h1 { 
            color: #333; 
            margin-top: 0; 
            margin-bottom: 20px;
            text-align: center;
            background: #007acc;
            color: white;
            padding: 15px;
            border-radius: 8px;
        }
        h2 { 
            color: #333; 
            margin-top: 0; 
            font-size: 18px;
        }
        textarea { 
            width: 100%; 
            height: 150px; 
            margin: 10px 0; 
            padding: 10px; 
            border: 1px solid #ddd; 
            border-radius: 4px; 
            resize: vertical;
            box-sizing: border-box;
        }
        input[type='file'] { 
            width: 100%; 
            padding: 10px; 
            margin: 10px 0; 
            border: 1px solid #ddd; 
            border-radius: 4px; 
            box-sizing: border-box;
        }
        button { 
            background-color: #007acc; 
            color: white; 
            padding: 10px 20px; 
            border: none; 
            border-radius: 4px; 
            cursor: pointer; 
            margin: 5px 5px 5px 0; 
        }
        button:hover { 
            background-color: #005a9e; 
        }
        .sample-button {
            background-color: #28a745;
            margin: 3px;
            padding: 8px 15px;
            font-size: 14px;
        }
        .sample-button:hover {
            background-color: #218838;
        }
        .preview-iframe {
            width: 100%;
            height: 100%;
            border: none;
        }
        .preview-header {
            background: #007acc;
            color: white;
            padding: 10px 20px;
            font-weight: bold;
            border-bottom: 1px solid #ddd;
        }
        .status-message {
            background: #d4edda;
            color: #155724;
            padding: 10px;
            border-radius: 4px;
            margin: 10px 0;
            border: 1px solid #c3e6cb;
        }
        .error-message {
            background: #f8d7da;
            color: #721c24;
            padding: 10px;
            border-radius: 4px;
            margin: 10px 0;
            border: 1px solid #f5c6cb;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='control-panel'>
            <h1>HTML Renderer</h1>
            
            <div class='panel'>
                <h2>📝 Render HTML from String</h2>
                <textarea id='htmlContent' placeholder='Enter your HTML content here...'></textarea>
                <button onclick='renderFromString()'>Render HTML</button>
            </div>
            
            <div class='panel'>
                <h2>📁 Upload HTML File</h2>
                <input type='file' id='fileInput' accept='.html' />
                <button onclick='uploadFile()'>Upload and Render</button>
            </div>
            
            <div class='panel'>
                <h2>📋 Load Sample Files</h2>
                <button onclick='loadSamples()'>Load Available Samples</button>
                <div id='samplesList'></div>
            </div>
            
            <div id='statusMessage' style='display:none;'></div>
        </div>
        
        <div class='preview-panel'>
            <div class='preview-header'>
                <span id='previewTitle'>Preview Panel - Select content to render</span>
                <button onclick='refreshPreview()' style='float: right; background: rgba(255,255,255,0.2); border: 1px solid rgba(255,255,255,0.3); padding: 5px 10px; margin: -5px 0;'>🔄 Refresh</button>
            </div>
            <iframe id='previewFrame' class='preview-iframe' src='/welcome'></iframe>
        </div>
    </div>
    
    <script>
        function showStatus(message, isError = false) {
            const statusDiv = document.getElementById('statusMessage');
            statusDiv.className = isError ? 'error-message' : 'status-message';
            statusDiv.textContent = message;
            statusDiv.style.display = 'block';
            setTimeout(() => {
                statusDiv.style.display = 'none';
            }, 3000);
        }
        
        function updatePreviewTitle(title) {
            document.getElementById('previewTitle').textContent = title;
        }
        
        function loadContentInPreview(contentId, title) {
            const iframe = document.getElementById('previewFrame');
            iframe.src = '/content/' + contentId;
            updatePreviewTitle(title);
        }
        
        function refreshPreview() {
            const iframe = document.getElementById('previewFrame');
            iframe.src = iframe.src;
        }
        
        function renderFromString() {
            const content = document.getElementById('htmlContent').value;
            if (!content.trim()) {
                showStatus('Please enter some HTML content', true);
                return;
            }
            
            showStatus('Rendering HTML content...');
            
            fetch('/render-content', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ source: 'string', content: content })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    loadContentInPreview(data.contentId, 'String Input - ' + data.contentId.substring(0, 8));
                    showStatus('HTML content rendered successfully!');
                } else {
                    showStatus('Error: ' + data.error, true);
                }
            })
            .catch(error => {
                showStatus('Error: ' + error.message, true);
            });
        }
        
        function uploadFile() {
            const fileInput = document.getElementById('fileInput');
            const file = fileInput.files[0];
            
            if (!file) {
                showStatus('Please select a file', true);
                return;
            }
            
            showStatus('Uploading and rendering file...');
            
            const formData = new FormData();
            formData.append('file', file);
            
            fetch('/upload-content', {
                method: 'POST',
                body: formData
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    loadContentInPreview(data.contentId, 'Uploaded File - ' + file.name);
                    showStatus('File uploaded and rendered successfully!');
                    fileInput.value = ''; // Clear the file input
                } else {
                    showStatus('Error: ' + data.error, true);
                }
            })
            .catch(error => {
                showStatus('Error: ' + error.message, true);
            });
        }
        
        function loadSamples() {
            showStatus('Loading sample files...');
            
            fetch('/samples')
            .then(response => response.json())
            .then(data => {
                const samplesList = document.getElementById('samplesList');
                if (data.files.length === 0) {
                    samplesList.innerHTML = '<p style=""color: #666; font-style: italic;"">No sample files found.</p>';
                    showStatus('No sample files found', true);
                } else {
                    samplesList.innerHTML = '<h3 style=""margin: 15px 0 10px 0; color: #333;"">Available Samples:</h3>' + 
                        data.files.map(file => 
                            `<button class=""sample-button"" onclick='loadSample(""${file}"")'>${file}</button>`
                        ).join('');
                    showStatus('Sample files loaded successfully!');
                }
            })
            .catch(error => {
                showStatus('Error loading samples: ' + error.message, true);
            });
        }
        
        function loadSample(fileName) {
            showStatus('Loading sample: ' + fileName);
            
            fetch('/samples-content/' + fileName)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    loadContentInPreview(data.contentId, 'Sample File - ' + fileName);
                    showStatus('Sample file loaded successfully!');
                } else {
                    showStatus('Error: ' + data.error, true);
                }
            })
            .catch(error => {
                showStatus('Error: ' + error.message, true);
            });
        }
        
        // Load samples on page load
        window.onload = function() {
            loadSamples();
        };
    </script>
</body>
</html>", "text/html"));

app.Run();

// Request model for the render endpoint
public record HtmlRequest(string? Source, string? Content, string? FilePath);
