# Script to reproduce production analytics issue locally
# This creates quiz submissions and checks if we get the same issue: total count but no data

Write-Host "🧪 REPRODUCING PRODUCTION ISSUE LOCALLY" -ForegroundColor Cyan
Write-Host "Expected: Total count > 0, but data array empty []" -ForegroundColor Yellow
Write-Host ""

$baseUrl = "http://localhost:5003"
$sessions = @()

# Create multiple sessions and quiz submissions
Write-Host "1️⃣ Creating test sessions and quiz submissions..." -ForegroundColor Green

for ($i = 1; $i -le 5; $i++) {
    try {
        # Create session
        $sessionBody = @{
            name = "Test User $i"
            email = "test$i@example.com"
        } | ConvertTo-Json
        
        Write-Host "Creating session $i..." -NoNewline
        $sessionResponse = Invoke-WebRequest -Uri "$baseUrl/api/sessions" -Method POST -Body $sessionBody -ContentType "application/json"
        $session = $sessionResponse.Content | ConvertFrom-Json
        $sessions += $session
        Write-Host " ✅ Created: $($session.sessionId)" -ForegroundColor Green
        
        # Submit quiz for this session
        $answers = @()
        $answers += @{ questionId = "1"; selectedAnswer = "Ctrl+L" }
        $answers += @{ questionId = "2"; selectedAnswer = "It depends on the task" }
        $answers += @{ questionId = "3"; selectedAnswer = "References files/symbols" }
        
        $quizBody = @{
            sessionId = $session.sessionId
            answers = $answers
        } | ConvertTo-Json -Depth 3
        
        Write-Host "Submitting quiz $i..." -NoNewline
        $quizResponse = Invoke-WebRequest -Uri "$baseUrl/api/quiz/submit" -Method POST -Body $quizBody -ContentType "application/json"
        Write-Host " ✅ Quiz submitted" -ForegroundColor Green
        
        # Small delay to avoid overwhelming the API
        Start-Sleep -Milliseconds 500
        
    } catch {
        Write-Host " ❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "2️⃣ Checking Redis debug info..." -ForegroundColor Green

try {
    $debugResponse = Invoke-WebRequest -Uri "$baseUrl/api/analytics/debug/redis-keys" -Method GET
    $debugData = $debugResponse.Content | ConvertFrom-Json
    
    Write-Host "📊 Redis Debug Results:" -ForegroundColor Cyan
    Write-Host "   Quiz IP attempts: $($debugData.quiz_ip_attempt_keys_count)" -ForegroundColor White
    Write-Host "   Quiz regular attempts: $($debugData.quiz_regular_attempt_keys_count)" -ForegroundColor White
    Write-Host "   Session keys: $($debugData.session_keys_count)" -ForegroundColor White
    Write-Host "   Sample session keys: $($debugData.session_sample_keys -join ', ')" -ForegroundColor Gray
    Write-Host "   Sample quiz attempt keys: $($debugData.quiz_ip_attempt_sample_keys -join ', ')" -ForegroundColor Gray
} catch {
    Write-Host "❌ Failed to get debug info: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "3️⃣ Testing Analytics - Checking for the production issue..." -ForegroundColor Green

try {
    $analyticsResponse = Invoke-WebRequest -Uri "$baseUrl/api/analytics/leaderboard/quiz?limit=10&offset=0" -Method GET
    $analyticsData = $analyticsResponse.Content | ConvertFrom-Json
    
    Write-Host "📈 Analytics Results:" -ForegroundColor Cyan
    Write-Host "   Total: $($analyticsData.total)" -ForegroundColor White
    Write-Host "   Data count: $($analyticsData.data.Count)" -ForegroundColor White
    Write-Host "   Has Next: $($analyticsData.hasNext)" -ForegroundColor White
    
    if ($analyticsData.total -gt 0 -and $analyticsData.data.Count -eq 0) {
        Write-Host ""
        Write-Host "🎯 ISSUE REPRODUCED! Same as production:" -ForegroundColor Red
        Write-Host "   ✅ Total: $($analyticsData.total) (quiz attempts found)" -ForegroundColor Green
        Write-Host "   ❌ Data: [] (participant data empty)" -ForegroundColor Red
        Write-Host ""
        Write-Host "This confirms the issue: Quiz attempts are stored but analytics can't retrieve participant data" -ForegroundColor Yellow
    } elseif ($analyticsData.total -gt 0 -and $analyticsData.data.Count -gt 0) {
        Write-Host ""
        Write-Host "✅ ISSUE FIXED! Analytics working correctly:" -ForegroundColor Green
        Write-Host "   Total: $($analyticsData.total)" -ForegroundColor Green
        Write-Host "   Data: $($analyticsData.data.Count) participants shown" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "⚠️  No quiz data found (total: $($analyticsData.total))" -ForegroundColor Yellow
        Write-Host "This might mean the quiz submissions didn't work properly" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Failed to get analytics: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "4️⃣ Getting total count for comparison..." -ForegroundColor Green

try {
    $totalResponse = Invoke-WebRequest -Uri "$baseUrl/api/analytics/leaderboard/quiz/total" -Method GET
    $totalData = $totalResponse.Content | ConvertFrom-Json
    Write-Host "📊 Total Count Endpoint: $($totalData.total)" -ForegroundColor White
} catch {
    Write-Host "❌ Total count endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔍 DEBUGGING SUMMARY:" -ForegroundColor Cyan
Write-Host "If you see 'ISSUE REPRODUCED' above, we have the same problem as production." -ForegroundColor White
Write-Host "Next steps would be to use the debug info to see why session data isn't being found." -ForegroundColor White
Write-Host "If you see 'ISSUE FIXED', then the recent changes resolved the problem!" -ForegroundColor White 