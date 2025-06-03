import React from 'react'
import { Link } from 'react-router-dom'
import { useSession } from '../shared/contexts/SessionContext'
import { useSignalR } from '../shared/contexts/SignalRContext'

const Home: React.FC = () => {
  const { session } = useSession()
  const { latestScoreUpdate } = useSignalR()

  const features = [
    {
      title: 'AI Development Quiz',
      description: 'Test your knowledge of Cursor and AI-first development practices',
      icon: '🧠',
      link: '/demo/quiz',
      status: session?.hasCompletedQuiz ? 'completed' : 'available',
      color: 'bg-blue-500',
    },
    {
      title: 'Spaceship Game',
      description: 'Play our HTML5 canvas game with real-time leaderboards',
      icon: '🚀',
      link: '/demo/game',
      status: 'available',
      color: 'bg-green-500',
    },
    {
      title: 'Tips & Tricks',
      description: 'AI-curated tips for better development with Cursor',
      icon: '💡',
      link: '/demo/tips',
      status: 'available',
      color: 'bg-yellow-500',
    },
    {
      title: 'Live Analytics',
      description: 'Real-time dashboard showing demo usage and performance',
      icon: '📊',
      link: '/demo/analytics',
      status: 'available',
      color: 'bg-purple-500',
    },
  ]

  return (
    <div className="max-w-7xl mx-auto">
      {/* Welcome Section */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-4">
          Welcome to AI First Demo! 👋
        </h1>
        <p className="text-lg text-gray-600 mb-6">
          This demo showcases modern development practices using AI-first tools like Cursor and Windsurf,
          built with a clean architecture using .NET 8, React, Redis, and Azure OpenAI.
        </p>
        
        {session && (
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <p className="text-blue-800">
              <span className="font-medium">Hello, {session.name}!</span> 
              Your session is active and ready to explore.
            </p>
          </div>
        )}
      </div>

      {/* Features Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
        {features.map((feature) => (
          <Link
            key={feature.title}
            to={feature.link}
            className="block group hover:shadow-lg transition-shadow duration-200"
          >
            <div className="card h-full">
              <div className="flex items-start space-x-4">
                <div className={`${feature.color} w-12 h-12 rounded-lg flex items-center justify-center text-white text-xl flex-shrink-0`}>
                  {feature.icon}
                </div>
                <div className="flex-1">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="text-lg font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">
                      {feature.title}
                    </h3>
                    {feature.status === 'completed' && (
                      <span className="bg-green-100 text-green-800 text-xs px-2 py-1 rounded-full">
                        ✓ Complete
                      </span>
                    )}
                  </div>
                  <p className="text-gray-600 text-sm">{feature.description}</p>
                </div>
              </div>
            </div>
          </Link>
        ))}
      </div>

      {/* Live Updates Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        {/* Recent Activity */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">🔴 Live Activity</h3>
          <div className="space-y-3">
            {latestScoreUpdate && (
              <div className="bg-green-50 border border-green-200 rounded-md p-3">
                <p className="text-sm text-green-800">
                  <span className="font-medium">{latestScoreUpdate.playerName}</span> scored{' '}
                  <span className="font-bold">{latestScoreUpdate.score}</span> points in the spaceship game!
                </p>
                <p className="text-xs text-green-600 mt-1">
                  {new Date(latestScoreUpdate.timestamp).toLocaleTimeString()}
                </p>
              </div>
            )}
            
            {!latestScoreUpdate && (
              <div className="text-gray-500 text-sm italic">
                No recent activity. Be the first to play!
              </div>
            )}
          </div>
        </div>

        {/* Architecture Highlights */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">🏗️ Architecture Highlights</h3>
          <div className="space-y-2">
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Vertical Slice Architecture</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Redis as Primary Database</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Azure OpenAI Integration</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Real-time SignalR Updates</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Clean Code Separation</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Modern React Patterns</span>
            </div>
          </div>
        </div>
      </div>

      {/* Demo Instructions */}
      <div className="card bg-gradient-to-r from-blue-50 to-indigo-50 border-blue-200">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">🎯 Demo Flow Suggestion</h3>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="text-center">
            <div className="bg-blue-100 w-10 h-10 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-blue-600 font-bold">1</span>
            </div>
            <h4 className="font-medium text-gray-900 mb-1">Take the Quiz</h4>
            <p className="text-xs text-gray-600">Test your AI development knowledge</p>
          </div>
          <div className="text-center">
            <div className="bg-green-100 w-10 h-10 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-green-600 font-bold">2</span>
            </div>
            <h4 className="font-medium text-gray-900 mb-1">Play the Game</h4>
            <p className="text-xs text-gray-600">Compete on the leaderboard</p>
          </div>
          <div className="text-center">
            <div className="bg-yellow-100 w-10 h-10 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-yellow-600 font-bold">3</span>
            </div>
            <h4 className="font-medium text-gray-900 mb-1">Explore Tips</h4>
            <p className="text-xs text-gray-600">Get AI-generated insights</p>
          </div>
          <div className="text-center">
            <div className="bg-purple-100 w-10 h-10 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-purple-600 font-bold">4</span>
            </div>
            <h4 className="font-medium text-gray-900 mb-1">View Analytics</h4>
            <p className="text-xs text-gray-600">See real-time insights</p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Home