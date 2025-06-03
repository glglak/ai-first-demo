import React, { useState, useEffect, useRef } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useSession } from '../../../shared/contexts/SessionContext'
import { gameApi } from '../../../shared/services/api'
import { SubmitScoreRequest } from '../../../shared/types'

// Constants for game
const CANVAS_WIDTH = 800
const CANVAS_HEIGHT = 600
const SHIP_SIZE = 20
const BULLET_SIZE = 5
const ASTEROID_MIN_SIZE = 20
const ASTEROID_MAX_SIZE = 50

interface GameObject {
  x: number
  y: number
  vx: number
  vy: number
  size: number
}

interface Ship extends GameObject {
  angle: number
}

interface Bullet extends GameObject {
  life: number
}

interface Asteroid extends GameObject {}

const SpaceshipGame: React.FC = () => {
  const { session } = useSession()
  const queryClient = useQueryClient()
  const gameCanvasRef = useRef<HTMLCanvasElement>(null)

  const { data: leaderboard, refetch: _refetchLeaderboard } = useQuery({
    queryKey: ['leaderboard'],
    queryFn: gameApi.getLeaderboard,
    staleTime: 30000,
  })

  const submitScoreMutation = useMutation({
    mutationFn: (request: SubmitScoreRequest) => gameApi.submitScore(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leaderboard'] })
    }
  })

  const [gameState, setGameState] = useState<'menu' | 'playing' | 'gameOver'>('menu')
  const [score, setScore] = useState(0)
  const [lives, setLives] = useState(3)
  const [ship, setShip] = useState<Ship>({
    x: CANVAS_WIDTH / 2,
    y: CANVAS_HEIGHT / 2,
    vx: 0,
    vy: 0,
    size: SHIP_SIZE,
    angle: 0
  })
  const [bullets, setBullets] = useState<Bullet[]>([])
  const [asteroids, setAsteroids] = useState<Asteroid[]>([])
  const [keys, setKeys] = useState<Set<string>>(new Set())

  // Initialize asteroids
  const createAsteroid = (): Asteroid => {
    const size = Math.random() * (ASTEROID_MAX_SIZE - ASTEROID_MIN_SIZE) + ASTEROID_MIN_SIZE
    const x = Math.random() < 0.5 ? -size : CANVAS_WIDTH + size
    const y = Math.random() * CANVAS_HEIGHT
    const vx = (Math.random() - 0.5) * 4
    const vy = (Math.random() - 0.5) * 4
    
    return { x, y, vx, vy, size }
  }

  // Game logic
  const startGame = () => {
    setGameState('playing')
    setScore(0)
    setLives(3)
    setShip({
      x: CANVAS_WIDTH / 2,
      y: CANVAS_HEIGHT / 2,
      vx: 0,
      vy: 0,
      size: SHIP_SIZE,
      angle: 0
    })
    setBullets([])
    setAsteroids(Array.from({ length: 5 }, createAsteroid))
  }

  const shoot = () => {
    if (gameState !== 'playing') return
    
    const bulletVx = Math.cos(ship.angle) * 8
    const bulletVy = Math.sin(ship.angle) * 8
    
    const newBullet: Bullet = {
      x: ship.x + Math.cos(ship.angle) * ship.size,
      y: ship.y + Math.sin(ship.angle) * ship.size,
      vx: bulletVx,
      vy: bulletVy,
      size: BULLET_SIZE,
      life: 60
    }
    
    setBullets(prev => [...prev, newBullet])
  }

  // Input handling
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      setKeys(prev => new Set(prev).add(e.key.toLowerCase()))
      if (e.key === ' ') {
        e.preventDefault()
        shoot()
      }
    }

    const handleKeyUp = (e: KeyboardEvent) => {
      setKeys(prev => {
        const newKeys = new Set(prev)
        newKeys.delete(e.key.toLowerCase())
        return newKeys
      })
    }

    window.addEventListener('keydown', handleKeyDown)
    window.addEventListener('keyup', handleKeyUp)

    return () => {
      window.removeEventListener('keydown', handleKeyDown)
      window.removeEventListener('keyup', handleKeyUp)
    }
  }, [ship])

  // Game loop
  useEffect(() => {
    if (gameState !== 'playing') return

    const gameLoop = setInterval(() => {
      // Update ship
      setShip(prev => {
        let newShip = { ...prev }

        // Rotation
        if (keys.has('a') || keys.has('arrowleft')) {
          newShip.angle -= 0.1
        }
        if (keys.has('d') || keys.has('arrowright')) {
          newShip.angle += 0.1
        }

        // Thrust
        if (keys.has('w') || keys.has('arrowup')) {
          newShip.vx += Math.cos(newShip.angle) * 0.3
          newShip.vy += Math.sin(newShip.angle) * 0.3
        }

        // Apply friction
        newShip.vx *= 0.98
        newShip.vy *= 0.98

        // Update position
        newShip.x += newShip.vx
        newShip.y += newShip.vy

        // Wrap around screen
        if (newShip.x < 0) newShip.x = CANVAS_WIDTH
        if (newShip.x > CANVAS_WIDTH) newShip.x = 0
        if (newShip.y < 0) newShip.y = CANVAS_HEIGHT
        if (newShip.y > CANVAS_HEIGHT) newShip.y = 0

        return newShip
      })

      // Update bullets
      setBullets(prev => prev
        .map(bullet => ({
          ...bullet,
          x: bullet.x + bullet.vx,
          y: bullet.y + bullet.vy,
          life: bullet.life - 1
        }))
        .filter(bullet => bullet.life > 0 && 
          bullet.x > 0 && bullet.x < CANVAS_WIDTH && 
          bullet.y > 0 && bullet.y < CANVAS_HEIGHT)
      )

      // Update asteroids
      setAsteroids(prev => prev.map(asteroid => ({
        ...asteroid,
        x: asteroid.x + asteroid.vx,
        y: asteroid.y + asteroid.vy
      })).filter(asteroid => 
        asteroid.x > -asteroid.size - 100 && 
        asteroid.x < CANVAS_WIDTH + asteroid.size + 100 &&
        asteroid.y > -asteroid.size - 100 && 
        asteroid.y < CANVAS_HEIGHT + asteroid.size + 100
      ))

      // Add new asteroids occasionally
      if (Math.random() < 0.02) {
        setAsteroids(prev => [...prev, createAsteroid()])
      }

      // Collision detection
      setBullets(prevBullets => {
        setAsteroids(prevAsteroids => {
          let newBullets = [...prevBullets]
          let newAsteroids = [...prevAsteroids]
          let scoreIncrease = 0

          for (let i = newBullets.length - 1; i >= 0; i--) {
            for (let j = newAsteroids.length - 1; j >= 0; j--) {
              const bullet = newBullets[i]
              const asteroid = newAsteroids[j]
              const distance = Math.sqrt(
                Math.pow(bullet.x - asteroid.x, 2) + Math.pow(bullet.y - asteroid.y, 2)
              )

              if (distance < bullet.size + asteroid.size) {
                newBullets.splice(i, 1)
                newAsteroids.splice(j, 1)
                scoreIncrease += Math.floor(asteroid.size)
                break
              }
            }
          }

          if (scoreIncrease > 0) {
            setScore(prev => prev + scoreIncrease)
          }

          return newAsteroids
        })
        return newBullets
      })

      // Check ship-asteroid collision
      setAsteroids(prevAsteroids => {
        setShip(prevShip => {
          for (const asteroid of prevAsteroids) {
            const distance = Math.sqrt(
              Math.pow(prevShip.x - asteroid.x, 2) + Math.pow(prevShip.y - asteroid.y, 2)
            )

            if (distance < prevShip.size + asteroid.size) {
              setLives(prev => {
                const newLives = prev - 1
                if (newLives <= 0) {
                  setGameState('gameOver')
                  if (session) {
                    submitScoreMutation.mutate({
                      sessionId: session.sessionId,
                      score,
                      gameMode: 'asteroids'
                    })
                  }
                }
                return newLives
              })
              
              // Reset ship position
              return {
                ...prevShip,
                x: CANVAS_WIDTH / 2,
                y: CANVAS_HEIGHT / 2,
                vx: 0,
                vy: 0
              }
            }
          }
          return prevShip
        })
        return prevAsteroids
      })

    }, 1000 / 60) // 60 FPS

    return () => clearInterval(gameLoop)
  }, [gameState, keys, score, session])

  // Rendering
  useEffect(() => {
    const canvas = gameCanvasRef.current
    if (!canvas) return

    const ctx = canvas.getContext('2d')
    if (!ctx) return

    // Clear canvas
    ctx.fillStyle = '#000011'
    ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT)

    if (gameState === 'playing') {
      // Draw ship
      ctx.save()
      ctx.translate(ship.x, ship.y)
      ctx.rotate(ship.angle)
      ctx.strokeStyle = '#00ff00'
      ctx.lineWidth = 2
      ctx.beginPath()
      ctx.moveTo(ship.size, 0)
      ctx.lineTo(-ship.size, -ship.size / 2)
      ctx.lineTo(-ship.size / 2, 0)
      ctx.lineTo(-ship.size, ship.size / 2)
      ctx.closePath()
      ctx.stroke()
      ctx.restore()

      // Draw bullets
      ctx.fillStyle = '#ffff00'
      bullets.forEach(bullet => {
        ctx.beginPath()
        ctx.arc(bullet.x, bullet.y, bullet.size, 0, Math.PI * 2)
        ctx.fill()
      })

      // Draw asteroids
      ctx.strokeStyle = '#888888'
      ctx.lineWidth = 2
      asteroids.forEach(asteroid => {
        ctx.beginPath()
        ctx.arc(asteroid.x, asteroid.y, asteroid.size, 0, Math.PI * 2)
        ctx.stroke()
      })

      // Draw UI
      ctx.fillStyle = '#ffffff'
      ctx.font = '20px Arial'
      ctx.fillText(`Score: ${score}`, 10, 30)
      ctx.fillText(`Lives: ${lives}`, 10, 60)
    }
  }, [ship, bullets, asteroids, score, lives, gameState])

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">🚀 Spaceship Game</h1>
        <p className="text-gray-700">
          Navigate through space, destroy asteroids, and set a high score!
        </p>
      </div>

      {/* Game Canvas */}
      <div className="flex justify-center">
        <div className="relative">
          <canvas
            ref={gameCanvasRef}
            width={CANVAS_WIDTH}
            height={CANVAS_HEIGHT}
            className="border-2 border-gray-300 rounded-lg bg-black"
          />
          
          {/* Game state overlays */}
          {gameState === 'menu' && (
            <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-75 rounded-lg">
              <div className="text-center text-white">
                <h2 className="text-2xl font-bold mb-4">🚀 Asteroids</h2>
                <p className="mb-4">Use WASD or Arrow Keys to move</p>
                <p className="mb-6">Space to shoot</p>
                <button
                  onClick={startGame}
                  className="bg-green-500 hover:bg-green-600 text-white font-bold py-3 px-6 rounded-lg"
                >
                  Start Game
                </button>
              </div>
            </div>
          )}

          {gameState === 'gameOver' && (
            <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-75 rounded-lg">
              <div className="text-center text-white">
                <h2 className="text-2xl font-bold mb-4">💥 Game Over!</h2>
                <p className="text-xl mb-4">Final Score: {score}</p>
                <button
                  onClick={startGame}
                  className="bg-blue-500 hover:bg-blue-600 text-white font-bold py-3 px-6 rounded-lg"
                >
                  Play Again
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Instructions */}
      {gameState === 'playing' && (
        <div className="text-center text-sm text-gray-600">
          <p>🎮 WASD or Arrow Keys to move • Spacebar to shoot</p>
        </div>
      )}

      {/* Leaderboard */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-4">🏆 Leaderboard</h2>
        {leaderboard && leaderboard.length > 0 ? (
          <div className="space-y-2">
            {leaderboard.slice(0, 10).map((entry, index) => (
              <div
                key={index}
                className="flex justify-between items-center p-2 rounded-lg bg-gray-50"
              >
                <div className="flex items-center space-x-3">
                  <span className="text-lg">
                    {index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : '🎮'}
                  </span>
                  <span className="font-medium">{entry.playerName}</span>
                </div>
                <span className="font-bold text-purple-600">{entry.score}</span>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500 text-center">No scores yet. Be the first!</p>
        )}
      </div>
    </div>
  )
}

export default SpaceshipGame 