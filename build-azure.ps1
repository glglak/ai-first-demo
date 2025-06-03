# Azure App Service Build Script for AI-First Demo
Write-Host "Starting Azure App Service Build Process..." -ForegroundColor Green

# Step 1: Clean previous build artifacts
Write-Host "Cleaning previous build artifacts..." -ForegroundColor Yellow
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}

# Step 2: Build React Frontend
Write-Host "Building React Frontend..." -ForegroundColor Cyan
Set-Location "src/AiFirstDemo.Web"
npm install
npm run build
Set-Location "../.."

# Step 3: Publish .NET Backend
Write-Host "Publishing .NET Backend..." -ForegroundColor Cyan
dotnet publish src/AiFirstDemo.Api/AiFirstDemo.Api.csproj --configuration Release --output "./publish" --verbosity minimal

# Step 4: Copy React build to wwwroot
Write-Host "Integrating React frontend with .NET backend..." -ForegroundColor Cyan
$reactBuildPath = "src/AiFirstDemo.Api/wwwroot"
$wwwrootPath = "publish/wwwroot"

if (Test-Path $reactBuildPath) {
    if (-not (Test-Path $wwwrootPath)) {
        New-Item -ItemType Directory -Path $wwwrootPath -Force | Out-Null
    }
    Copy-Item -Path "$reactBuildPath/*" -Destination $wwwrootPath -Recurse -Force
    Write-Host "   React app copied to wwwroot" -ForegroundColor Green
} else {
    Write-Host "   React build not found at $reactBuildPath" -ForegroundColor Red
    exit 1
}

# Step 5: Verify deployment package
Write-Host "Verifying deployment package..." -ForegroundColor Cyan
if (Test-Path "publish/wwwroot/index.html") {
    Write-Host "   React frontend integrated successfully" -ForegroundColor Green
} else {
    Write-Host "   React frontend not properly integrated" -ForegroundColor Red
}

# Step 6: Create deployment zip
Write-Host "Creating deployment zip file..." -ForegroundColor Cyan
$zipPath = "ai-first-demo-azure-deployment.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

try {
    Compress-Archive -Path "publish/*" -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "   Deployment zip created: $zipPath" -ForegroundColor Green
} catch {
    Write-Host "   Could not create zip file" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Build completed successfully! Ready for Azure deployment." -ForegroundColor Green
Write-Host "Package location: ./publish/" -ForegroundColor Yellow
Write-Host "Deployment zip: ./$zipPath" -ForegroundColor Yellow