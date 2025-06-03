import React, { useRef, useEffect, useState, useCallback } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useSession } from '../../../shared/contexts/SessionContext'
import { gameApi } from '../../../shared/services/api'
import { SubmitScoreRequest } from '../../../shared/types'

interface GameState {
  player: {
    x: number
    y: number
    angle: number
    velocity: { x: number; y: number }
    radius: number
  }
  bullets: Array<{
    x: number
    y: number
    velocity: { x: number; y: number }
    life: number
  }>
  asteroids: Array<{
    x: number
    y: number
    velocity: { x: number; y: number }
    radius: number
    angle: number
    rotationSpeed: number
  }>
  score: number
  level: number
  lives: number
  gameStartTime: number
  asteroidsDestroyed: number
  isGameOver: boolean
  isPaused: boolean
}

const SpaceshipGame: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const gameLoopRef = useRef<number>()
  const keysRef = useRef<Set<string>>(new Set())
  const [gameState, setGameState] = useState<GameState | null>(null)
  const [showLeaderboard, setShowLeaderboard] = useState(false)
  const [playerName, setPlayerName] = useState('')
  const [showNameInput, setShowNameInput] = useState(false)
  const { session } = useSession()
  const queryClient = useQueryClient()

  // Game constants
  const CANVAS_WIDTH = 800
  const CANVAS_HEIGHT = 600
  const PLAYER_SIZE = 15
  const BULLET_SPEED = 8
  const ASTEROID_MIN_SIZE = 20
  const ASTEROID_MAX_SIZE = 50
  const PLAYER_ACCELERATION = 0.3
  const PLAYER_MAX_SPEED = 8
  const FRICTION = 0.98

  // API queries
  const { data: leaderboard } = useQuery({
    queryKey: ['leaderboard', session?.sessionId],
    queryFn: () => gameApi.getLeaderboard(session?.sessionId),
    enabled: showLeaderboard
  })

  const submitScoreMutation = useMutation({
    mutationFn: (request: SubmitScoreRequest) => gameApi.submitScore(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leaderboard'] })
      setShowNameInput(false)
      setShowLeaderboard(true)
    }
  })

  // Pre-fill player name if user has a quiz session
  useEffect(() => {
    if (session?.name && !playerName) {
      setPlayerName(session.name)
    }
  }, [session, playerName])

  // Initialize game
  const initializeGame = useCallback(() => {
    const newGameState: GameState = {
      player: {
        x: CANVAS_WIDTH / 2,
        y: CANVAS_HEIGHT / 2,
        angle: 0,
        velocity: { x: 0, y: 0 },
        radius: PLAYER_SIZE
      },
      bullets: [],
      asteroids: [],
      score: 0,
      level: 1,
      lives: 3,
      gameStartTime: Date.now(),
      asteroidsDestroyed: 0,
      isGameOver: false,
      isPaused: false
    }

    // Create initial asteroids
    for (let i = 0; i < 5; i++) {
      newGameState.asteroids.push(createAsteroid())
    }

    setGameState(newGameState)
  }, [])

  // Create asteroid
  const createAsteroid = useCallback(() => {
    const radius = ASTEROID_MIN_SIZE + Math.random() * (ASTEROID_MAX_SIZE - ASTEROID_MIN_SIZE)
    let x, y
    
    // Spawn asteroids away from player
    do {
      x = Math.random() * CANVAS_WIDTH
      y = Math.random() * CANVAS_HEIGHT
    } while (
      Math.sqrt((x - CANVAS_WIDTH/2) ** 2 + (y - CANVAS_HEIGHT/2) ** 2) < 100
    )

    return {
      x,
      y,
      velocity: {
        x: (Math.random() - 0.5) * 4,
        y: (Math.random() - 0.5) * 4
      },
      radius,
      angle: Math.random() * Math.PI * 2,
      rotationSpeed: (Math.random() - 0.5) * 0.1
    }
  }, [])

  // Handle keyboard input
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      keysRef.current.add(e.code)
      
      if (e.code === 'Space') {
        e.preventDefault()
        if (gameState && !gameState.isGameOver && !gameState.isPaused) {
          shoot()
        }
      }
      
      if (e.code === 'KeyP') {
        togglePause()
      }
    }

    const handleKeyUp = (e: KeyboardEvent) => {
      keysRef.current.delete(e.code)
    }

    window.addEventListener('keydown', handleKeyDown)
    window.addEventListener('keyup', handleKeyUp)

    return () => {
      window.removeEventListener('keydown', handleKeyDown)
      window.removeEventListener('keyup', handleKeyUp)
    }
  }, [gameState])

  // Shoot bullet
  const shoot = useCallback(() => {
    if (!gameState) return

    const bullet = {
      x: gameState.player.x,
      y: gameState.player.y,
      velocity: {
        x: Math.cos(gameState.player.angle) * BULLET_SPEED,
        y: Math.sin(gameState.player.angle) * BULLET_SPEED
      },
      life: 60 // bullets live for 60 frames
    }

    setGameState(prev => prev ? {
      ...prev,
      bullets: [...prev.bullets, bullet]
    } : null)
  }, [gameState])

  // Toggle pause
  const togglePause = useCallback(() => {
    setGameState(prev => prev ? {
      ...prev,
      isPaused: !prev.isPaused
    } : null)
  }, [])

  // Game loop
  const gameLoop = useCallback(() => {
    if (!gameState || gameState.isGameOver || gameState.isPaused) return

    const newState = { ...gameState }

    // Handle player input
    if (keysRef.current.has('ArrowLeft') || keysRef.current.has('KeyA')) {
      newState.player.angle -= 0.1
    }
    if (keysRef.current.has('ArrowRight') || keysRef.current.has('KeyD')) {
      newState.player.angle += 0.1
    }
    if (keysRef.current.has('ArrowUp') || keysRef.current.has('KeyW')) {
      newState.player.velocity.x += Math.cos(newState.player.angle) * PLAYER_ACCELERATION
      newState.player.velocity.y += Math.sin(newState.player.angle) * PLAYER_ACCELERATION
    }

    // Apply friction and speed limit
    newState.player.velocity.x *= FRICTION
    newState.player.velocity.y *= FRICTION
    
    const speed = Math.sqrt(newState.player.velocity.x ** 2 + newState.player.velocity.y ** 2)
    if (speed > PLAYER_MAX_SPEED) {
      newState.player.velocity.x = (newState.player.velocity.x / speed) * PLAYER_MAX_SPEED
      newState.player.velocity.y = (newState.player.velocity.y / speed) * PLAYER_MAX_SPEED
    }

    // Update player position
    newState.player.x += newState.player.velocity.x
    newState.player.y += newState.player.velocity.y

    // Wrap player around screen
    if (newState.player.x < 0) newState.player.x = CANVAS_WIDTH
    if (newState.player.x > CANVAS_WIDTH) newState.player.x = 0
    if (newState.player.y < 0) newState.player.y = CANVAS_HEIGHT
    if (newState.player.y > CANVAS_HEIGHT) newState.player.y = 0

    // Update bullets
    newState.bullets = newState.bullets
      .map(bullet => ({
        ...bullet,
        x: bullet.x + bullet.velocity.x,
        y: bullet.y + bullet.velocity.y,
        life: bullet.life - 1
      }))
      .filter(bullet => bullet.life > 0 && 
        bullet.x >= 0 && bullet.x <= CANVAS_WIDTH &&
        bullet.y >= 0 && bullet.y <= CANVAS_HEIGHT)

    // Update asteroids
    newState.asteroids = newState.asteroids.map(asteroid => ({
      ...asteroid,
      x: asteroid.x + asteroid.velocity.x,
      y: asteroid.y + asteroid.velocity.y,
      angle: asteroid.angle + asteroid.rotationSpeed
    }))

    // Wrap asteroids around screen
    newState.asteroids.forEach(asteroid => {
      if (asteroid.x < -asteroid.radius) asteroid.x = CANVAS_WIDTH + asteroid.radius
      if (asteroid.x > CANVAS_WIDTH + asteroid.radius) asteroid.x = -asteroid.radius
      if (asteroid.y < -asteroid.radius) asteroid.y = CANVAS_HEIGHT + asteroid.radius
      if (asteroid.y > CANVAS_HEIGHT + asteroid.radius) asteroid.y = -asteroid.radius
    })

    // Check bullet-asteroid collisions
    for (let i = newState.bullets.length - 1; i >= 0; i--) {
      const bullet = newState.bullets[i]
      for (let j = newState.asteroids.length - 1; j >= 0; j--) {
        const asteroid = newState.asteroids[j]
        const distance = Math.sqrt(
          (bullet.x - asteroid.x) ** 2 + (bullet.y - asteroid.y) ** 2
        )
        
        if (distance < asteroid.radius) {
          // Remove bullet and asteroid
          newState.bullets.splice(i, 1)
          newState.asteroids.splice(j, 1)
          
          // Add score
          newState.score += Math.floor(asteroid.radius)
          newState.asteroidsDestroyed++
          
          // Split large asteroids
          if (asteroid.radius > 30) {
            for (let k = 0; k < 2; k++) {
              newState.asteroids.push({
                x: asteroid.x,
                y: asteroid.y,
                velocity: {
                  x: (Math.random() - 0.5) * 6,
                  y: (Math.random() - 0.5) * 6
                },
                radius: asteroid.radius / 2,
                angle: Math.random() * Math.PI * 2,
                rotationSpeed: (Math.random() - 0.5) * 0.2
              })
            }
          }
          break
        }
      }
    }

    // Check player-asteroid collisions
    for (const asteroid of newState.asteroids) {
      const distance = Math.sqrt(
        (newState.player.x - asteroid.x) ** 2 + (newState.player.y - asteroid.y) ** 2
      )
      
      if (distance < newState.player.radius + asteroid.radius) {
        newState.lives--
        
        if (newState.lives <= 0) {
          newState.isGameOver = true
          setShowNameInput(true)
        } else {
          // Reset player position
          newState.player.x = CANVAS_WIDTH / 2
          newState.player.y = CANVAS_HEIGHT / 2
          newState.player.velocity = { x: 0, y: 0 }
        }
        break
      }
    }

    // Level progression
    if (newState.asteroids.length === 0 && !newState.isGameOver) {
      newState.level++
      const asteroidCount = Math.min(5 + newState.level, 12)
      for (let i = 0; i < asteroidCount; i++) {
        newState.asteroids.push(createAsteroid())
      }
    }

    setGameState(newState)
  }, [gameState, createAsteroid])

  // Start game loop
  useEffect(() => {
    if (gameState && !gameState.isGameOver && !gameState.isPaused) {
      gameLoopRef.current = requestAnimationFrame(() => {
        gameLoop()
        gameLoopRef.current = requestAnimationFrame(gameLoop)
      })
    }

    return () => {
      if (gameLoopRef.current) {
        cancelAnimationFrame(gameLoopRef.current)
      }
    }
  }, [gameLoop, gameState])

  // Render game
  useEffect(() => {
    const canvas = canvasRef.current
    if (!canvas || !gameState) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    // Clear canvas
    ctx.fillStyle = '#000011'
    ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT)

    // Draw stars
    ctx.fillStyle = '#ffffff'
    for (let i = 0; i < 100; i++) {
      const x = (i * 37) % CANVAS_WIDTH
      const y = (i * 73) % CANVAS_HEIGHT
      ctx.fillRect(x, y, 1, 1)
    }

    // Draw player
    ctx.save()
    ctx.translate(gameState.player.x, gameState.player.y)
    ctx.rotate(gameState.player.angle)
    ctx.strokeStyle = '#00ff00'
    ctx.lineWidth = 2
    ctx.beginPath()
    ctx.moveTo(15, 0)
    ctx.lineTo(-10, -8)
    ctx.lineTo(-5, 0)
    ctx.lineTo(-10, 8)
    ctx.closePath()
    ctx.stroke()
    ctx.restore()

    // Draw bullets
    ctx.fillStyle = '#ffff00'
    gameState.bullets.forEach(bullet => {
      ctx.beginPath()
      ctx.arc(bullet.x, bullet.y, 2, 0, Math.PI * 2)
      ctx.fill()
    })

    // Draw asteroids
    ctx.strokeStyle = '#888888'
    ctx.lineWidth = 2
    gameState.asteroids.forEach(asteroid => {
      ctx.save()
      ctx.translate(asteroid.x, asteroid.y)
      ctx.rotate(asteroid.angle)
      ctx.beginPath()
      const points = 8
      for (let i = 0; i < points; i++) {
        const angle = (i / points) * Math.PI * 2
        const radius = asteroid.radius * (0.8 + Math.sin(i * 3) * 0.2)
        const x = Math.cos(angle) * radius
        const y = Math.sin(angle) * radius
        if (i === 0) ctx.moveTo(x, y)
        else ctx.lineTo(x, y)
      }
      ctx.closePath()
      ctx.stroke()
      ctx.restore()
    })

    // Draw UI
    ctx.fillStyle = '#ffffff'
    ctx.font = '20px Arial'
    ctx.fillText(`Score: ${gameState.score}`, 10, 30)
    ctx.fillText(`Level: ${gameState.level}`, 10, 60)
    ctx.fillText(`Lives: ${gameState.lives}`, 10, 90)

    if (gameState.isPaused) {
      ctx.fillStyle = 'rgba(0, 0, 0, 0.7)'
      ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT)
      ctx.fillStyle = '#ffffff'
      ctx.font = '48px Arial'
      ctx.textAlign = 'center'
      ctx.fillText('PAUSED', CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2)
      ctx.font = '24px Arial'
      ctx.fillText('Press P to resume', CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2 + 50)
      ctx.textAlign = 'left'
    }

    if (gameState.isGameOver) {
      ctx.fillStyle = 'rgba(0, 0, 0, 0.8)'
      ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT)
      ctx.fillStyle = '#ff0000'
      ctx.font = '48px Arial'
      ctx.textAlign = 'center'
      ctx.fillText('GAME OVER', CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2 - 50)
      ctx.fillStyle = '#ffffff'
      ctx.font = '24px Arial'
      ctx.fillText(`Final Score: ${gameState.score}`, CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2)
      ctx.fillText(`Level Reached: ${gameState.level}`, CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2 + 30)
      ctx.textAlign = 'left'
    }
  }, [gameState])

  // Submit score
  const handleSubmitScore = () => {
    if (!gameState || !playerName.trim()) return

    const gameDurationMs = Date.now() - gameState.gameStartTime
    const gameDurationSeconds = Math.floor(gameDurationMs / 1000)
    const hours = Math.floor(gameDurationSeconds / 3600)
    const minutes = Math.floor((gameDurationSeconds % 3600) / 60)
    const seconds = gameDurationSeconds % 60
    const gameDurationTimeSpan = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
    
    submitScoreMutation.mutate({
      sessionId: session?.sessionId || 'game-only-session', // Use quiz session if available, otherwise use a default
      playerName: playerName.trim(),
      score: gameState.score,
      gameDuration: gameDurationTimeSpan,
      level: gameState.level,
      asteroidsDestroyed: gameState.asteroidsDestroyed
    })
  }

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h2 className="text-3xl font-bold text-gray-900 mb-2">🚀 Spaceship Game</h2>
        <p className="text-gray-700">
          Use WASD or Arrow Keys to move, Space to shoot, P to pause
        </p>
      </div>

      <div className="flex justify-center space-x-4 mb-4">
        <button
          onClick={initializeGame}
          className="btn btn-primary"
        >
          {gameState ? 'New Game' : 'Start Game'}
        </button>
        
        <button
          onClick={() => setShowLeaderboard(!showLeaderboard)}
          className="btn btn-secondary"
        >
          {showLeaderboard ? 'Hide Leaderboard' : 'Show Leaderboard'}
        </button>
      </div>

      <div className="flex justify-center">
        <div className="relative">
          <canvas
            ref={canvasRef}
            width={CANVAS_WIDTH}
            height={CANVAS_HEIGHT}
            className="border-2 border-purple-500 rounded-lg bg-black"
            tabIndex={0}
          />
          
          {!gameState && (
            <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-75 rounded-lg">
              <div className="text-center text-white">
                <h3 className="text-2xl font-bold mb-4">Ready to Play?</h3>
                <p className="mb-4">Destroy asteroids to score points!</p>
                <button onClick={initializeGame} className="btn btn-primary">
                  Start Game
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Name Input Modal */}
      {showNameInput && gameState?.isGameOver && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-xl font-bold mb-4">Submit Your Score!</h3>
            <p className="text-gray-600 mb-4">
              Score: {gameState.score} | Level: {gameState.level}
            </p>
            <input
              type="text"
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              placeholder="Enter your name..."
              className="w-full p-3 border border-gray-300 rounded-lg mb-4"
              maxLength={20}
              onKeyPress={(e) => e.key === 'Enter' && handleSubmitScore()}
            />
            <div className="flex space-x-3">
              <button
                onClick={handleSubmitScore}
                disabled={!playerName.trim() || submitScoreMutation.isPending}
                className="flex-1 btn btn-primary disabled:opacity-50"
              >
                {submitScoreMutation.isPending ? 'Submitting...' : 'Submit Score'}
              </button>
              <button
                onClick={() => setShowNameInput(false)}
                className="flex-1 btn btn-secondary"
              >
                Skip
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Leaderboard */}
      {showLeaderboard && leaderboard && (
        <div className="card">
          <h3 className="text-2xl font-bold text-gray-900 mb-6 text-center">🏆 Leaderboard</h3>
          
          <div className="grid md:grid-cols-3 gap-6">
            {/* Daily Leaderboard */}
            <div>
              <h4 className="text-lg font-semibold text-purple-600 mb-3">Today</h4>
              <div className="space-y-2">
                {leaderboard.dailyLeaderboard.slice(0, 5).map((entry, index) => (
                  <div key={index} className="flex justify-between items-center bg-gray-50 rounded-lg p-2">
                    <span className="text-gray-900">
                      #{entry.rank} {entry.playerName}
                    </span>
                    <span className="text-purple-600 font-bold">{entry.score}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Weekly Leaderboard */}
            <div>
              <h4 className="text-lg font-semibold text-blue-600 mb-3">This Week</h4>
              <div className="space-y-2">
                {leaderboard.weeklyLeaderboard.slice(0, 5).map((entry, index) => (
                  <div key={index} className="flex justify-between items-center bg-gray-50 rounded-lg p-2">
                    <span className="text-gray-900">
                      #{entry.rank} {entry.playerName}
                    </span>
                    <span className="text-blue-600 font-bold">{entry.score}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* All Time Leaderboard */}
            <div>
              <h4 className="text-lg font-semibold text-yellow-600 mb-3">All Time</h4>
              <div className="space-y-2">
                {leaderboard.allTimeLeaderboard.slice(0, 5).map((entry, index) => (
                  <div key={index} className="flex justify-between items-center bg-gray-50 rounded-lg p-2">
                    <span className="text-gray-900">
                      #{entry.rank} {entry.playerName}
                    </span>
                    <span className="text-yellow-600 font-bold">{entry.score}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {leaderboard.playerBest && (
            <div className="mt-6 p-4 bg-gradient-to-r from-purple-500 to-blue-500 rounded-lg">
              <h4 className="text-white font-semibold mb-2">Your Best Score</h4>
              <div className="text-white">
                <span className="font-bold">{leaderboard.playerBest.score}</span> points
                <span className="ml-4 text-purple-200">Level {leaderboard.playerBest.level}</span>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Game Instructions */}
      <div className="card">
        <h3 className="text-xl font-bold text-gray-900 mb-4">How to Play</h3>
        <div className="grid md:grid-cols-2 gap-4 text-gray-700">
          <div>
            <h4 className="font-semibold text-purple-600 mb-2">Controls</h4>
            <ul className="space-y-1 text-sm">
              <li>• WASD or Arrow Keys: Move ship</li>
              <li>• Space: Shoot</li>
              <li>• P: Pause/Resume</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold text-blue-600 mb-2">Gameplay</h4>
            <ul className="space-y-1 text-sm">
              <li>• Destroy asteroids to score points</li>
              <li>• Large asteroids split into smaller ones</li>
              <li>• Avoid collisions - you have 3 lives</li>
              <li>• Each level adds more asteroids</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  )
}

export default SpaceshipGame 