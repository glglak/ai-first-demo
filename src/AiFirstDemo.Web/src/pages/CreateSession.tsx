import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { useSession } from '../contexts/SessionContext'
import { sessionApi } from '../services/api'
import { CreateSessionRequest } from '../types'

const CreateSession: React.FC = () => {
  const [name, setName] = useState('')
  const [error, setError] = useState('')
  const { setSession } = useSession()
  const navigate = useNavigate()

  const createSessionMutation = useMutation({
    mutationFn: (request: CreateSessionRequest) => sessionApi.create(request),
    onSuccess: (session) => {
      setSession(session)
      navigate('/demo')
    },
    onError: (error: any) => {
      if (error.response?.status === 409) {
        setError('A session already exists for your IP address. Please wait or try from a different network.')
      } else {
        setError('Failed to create session. Please try again.')
      }
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!name.trim()) {
      setError('Please enter your name')
      return
    }
    
    setError('')
    createSessionMutation.mutate({
      name: name.trim(),
      ipAddress: '', // Will be determined by server
    })
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="max-w-md w-full">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="mx-auto w-16 h-16 bg-blue-600 rounded-full flex items-center justify-center mb-4">
            <span className="text-2xl text-white">🚀</span>
          </div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            AI First Demo
          </h1>
          <p className="text-gray-600">
            Showcasing Cursor & Windsurf with .NET 8, React, Redis, and Azure OpenAI
          </p>
        </div>

        {/* Session Creation Form */}
        <div className="bg-white rounded-xl shadow-lg p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">
            Join the Demo
          </h2>
          
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
                Your Name
              </label>
              <input
                type="text"
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter your name..."
                className="input"
                disabled={createSessionMutation.isPending}
                maxLength={50}
              />
            </div>

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-md p-3">
                <p className="text-sm text-red-600">{error}</p>
              </div>
            )}

            <button
              type="submit"
              disabled={createSessionMutation.isPending || !name.trim()}
              className="w-full btn btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {createSessionMutation.isPending ? (
                <span className="flex items-center justify-center space-x-2">
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                  <span>Creating Session...</span>
                </span>
              ) : (
                'Start Demo'
              )}
            </button>
          </form>

          {/* Demo Info */}
          <div className="mt-6 pt-6 border-t border-gray-200">
            <h3 className="text-sm font-medium text-gray-900 mb-2">
              What you'll experience:
            </h3>
            <ul className="text-sm text-gray-600 space-y-1">
              <li>• 📝 Interactive AI Development Quiz</li>
              <li>• 🎮 Real-time Spaceship Game with Leaderboard</li>
              <li>• 💡 AI-Generated Tips & Tricks</li>
              <li>• 📊 Live Analytics Dashboard</li>
              <li>• 🏗️ Clean Architecture Demo</li>
            </ul>
          </div>

          <div className="mt-4 text-xs text-gray-500 text-center">
            One session per IP address • Session expires in 24 hours
          </div>
        </div>

        {/* Technical Stack */}
        <div className="mt-6 text-center">
          <p className="text-sm text-gray-600 mb-2">Built with:</p>
          <div className="flex justify-center space-x-4 text-xs text-gray-500">
            <span>.NET 8</span>
            <span>•</span>
            <span>React</span>
            <span>•</span>
            <span>Redis</span>
            <span>•</span>
            <span>Azure OpenAI</span>
          </div>
        </div>
      </div>
    </div>
  )
}

export default CreateSession