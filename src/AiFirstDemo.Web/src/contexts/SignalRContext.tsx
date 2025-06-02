import React, { createContext, useContext, useEffect, useState } from 'react'
import * as signalR from '@microsoft/signalr'
import { ScoreUpdate, GameEvent, AnalyticsUpdate } from '../types'

interface SignalRContextType {
  gameConnection: signalR.HubConnection | null
  analyticsConnection: signalR.HubConnection | null
  isGameConnected: boolean
  isAnalyticsConnected: boolean
  latestScoreUpdate: ScoreUpdate | null
  latestGameEvent: GameEvent | null
  latestAnalyticsUpdate: AnalyticsUpdate | null
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined)

export const useSignalR = () => {
  const context = useContext(SignalRContext)
  if (context === undefined) {
    throw new Error('useSignalR must be used within a SignalRProvider')
  }
  return context
}

interface SignalRProviderProps {
  children: React.ReactNode
}

export const SignalRProvider: React.FC<SignalRProviderProps> = ({ children }) => {
  const [gameConnection, setGameConnection] = useState<signalR.HubConnection | null>(null)
  const [analyticsConnection, setAnalyticsConnection] = useState<signalR.HubConnection | null>(null)
  const [isGameConnected, setIsGameConnected] = useState(false)
  const [isAnalyticsConnected, setIsAnalyticsConnected] = useState(false)
  const [latestScoreUpdate, setLatestScoreUpdate] = useState<ScoreUpdate | null>(null)
  const [latestGameEvent, setLatestGameEvent] = useState<GameEvent | null>(null)
  const [latestAnalyticsUpdate, setLatestAnalyticsUpdate] = useState<AnalyticsUpdate | null>(null)

  useEffect(() => {
    // Setup Game Hub connection
    const gameHub = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/game')
      .withAutomaticReconnect()
      .build()

    // Setup Analytics Hub connection
    const analyticsHub = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/analytics')
      .withAutomaticReconnect()
      .build()

    // Game Hub event handlers
    gameHub.on('ScoreUpdate', (update: ScoreUpdate) => {
      setLatestScoreUpdate(update)
    })

    gameHub.on('GameEvent', (event: GameEvent) => {
      setLatestGameEvent(event)
    })

    // Analytics Hub event handlers
    analyticsHub.on('AnalyticsUpdate', (update: AnalyticsUpdate) => {
      setLatestAnalyticsUpdate(update)
    })

    // Start connections
    const startConnections = async () => {
      try {
        await gameHub.start()
        setIsGameConnected(true)
        setGameConnection(gameHub)
        console.log('Game hub connected')
      } catch (error) {
        console.error('Error connecting to game hub:', error)
      }

      try {
        await analyticsHub.start()
        setIsAnalyticsConnected(true)
        setAnalyticsConnection(analyticsHub)
        console.log('Analytics hub connected')
      } catch (error) {
        console.error('Error connecting to analytics hub:', error)
      }
    }

    startConnections()

    // Cleanup
    return () => {
      gameHub.stop()
      analyticsHub.stop()
    }
  }, [])

  return (
    <SignalRContext.Provider
      value={{
        gameConnection,
        analyticsConnection,
        isGameConnected,
        isAnalyticsConnected,
        latestScoreUpdate,
        latestGameEvent,
        latestAnalyticsUpdate,
      }}
    >
      {children}
    </SignalRContext.Provider>
  )
}