import React, { createContext, useContext, useState, useEffect } from 'react'
import { UserSession } from '../types'

interface SessionContextType {
  session: UserSession | null
  setSession: (session: UserSession | null) => void
  isLoading: boolean
}

const SessionContext = createContext<SessionContextType | undefined>(undefined)

export const useSession = () => {
  const context = useContext(SessionContext)
  if (context === undefined) {
    throw new Error('useSession must be used within a SessionProvider')
  }
  return context
}

interface SessionProviderProps {
  children: React.ReactNode
}

export const SessionProvider: React.FC<SessionProviderProps> = ({ children }) => {
  const [session, setSession] = useState<UserSession | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    // Try to restore session from localStorage
    const savedSession = localStorage.getItem('ai-demo-session')
    if (savedSession) {
      try {
        const parsedSession = JSON.parse(savedSession)
        setSession(parsedSession)
      } catch (error) {
        console.error('Failed to parse saved session:', error)
        localStorage.removeItem('ai-demo-session')
      }
    }
    setIsLoading(false)
  }, [])

  const handleSetSession = (newSession: UserSession | null) => {
    setSession(newSession)
    if (newSession) {
      localStorage.setItem('ai-demo-session', JSON.stringify(newSession))
    } else {
      localStorage.removeItem('ai-demo-session')
    }
  }

  return (
    <SessionContext.Provider value={{ session, setSession: handleSetSession, isLoading }}>
      {children}
    </SessionContext.Provider>
  )
}