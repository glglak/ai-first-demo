# AI First Demo - Development Startup Script
Write-Host "🚀 Starting AI First Demo..." -ForegroundColor Green

# Check if Docker is running
Write-Host "📦 Starting Redis..." -ForegroundColor Yellow
try {
    docker-compose up -d
    Write-Host "✅ Redis started successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to start Redis. Make sure Docker Desktop is running." -ForegroundColor Red
    exit 1
}

# Start the .NET API in background
Write-Host "🔧 Starting .NET API..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/AiFirstDemo.Api; dotnet run"

# Wait a moment for API to start
Start-Sleep -Seconds 3

# Start the React frontend
Write-Host "⚛️ Starting React Frontend..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/AiFirstDemo.Web; npm run dev"

Write-Host ""
Write-Host "🎉 AI First Demo is starting up!" -ForegroundColor Green
Write-Host "📍 API: https://localhost:5003" -ForegroundColor Cyan
Write-Host "📍 Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "📍 API Docs: https://localhost:5003/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 