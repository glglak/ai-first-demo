import React from 'react'
import { Link } from 'react-router-dom'
import { useSession } from '../shared/contexts/SessionContext'
import { useSignalR } from '../shared/contexts/SignalRContext'

const Home: React.FC = () => {
  const { session } = useSession()
  const { latestScoreUpdate } = useSignalR()

  const features = [
    {
      title: 'Intelligence Test',
      description: 'Prove your loyalty to the family with AI development knowledge',
      icon: '🧠',
      link: '/demo/quiz',
      status: session?.hasCompletedQuiz ? 'completed' : 'available',
      color: 'bg-purple-500',
      quote: '"In this business, you gotta think fast"'
    },
    {
      title: 'Territory Wars',
      description: 'Conquer space and defend your turf in this epic battle',
      icon: '🚀',
      link: '/demo/game',
      status: 'available',
      color: 'bg-red-500',
      quote: '"The world is yours, but space is harder"'
    },
    {
      title: 'Family Secrets',
      description: 'Learn the tricks of the trade from the wisest in the family',
      icon: '💡',
      link: '/demo/tips',
      status: 'available',
      color: 'bg-green-500',
      quote: '"Knowledge is power, power is everything"'
    },
    {
      title: 'Family Business',
      description: 'Keep track of the operation with real-time intelligence',
      icon: '📊',
      link: '/demo/analytics',
      status: 'available',
      color: 'bg-blue-500',
      quote: '"Never let anyone outside the family know what you\'re thinking"'
    },
  ]

  return (
    <div className="max-w-7xl mx-auto">
      {/* Welcome Section */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-4">
          Welcome to the Family! 🤝
        </h1>
        <p className="text-lg text-gray-600 mb-6">
          "I always tell the truth, even when I lie." This empire showcases the power of AI-first development 
          using modern tools like Cursor and Windsurf. Built with respect - .NET 8, React, Redis, and Azure OpenAI.
        </p>
        
        {session && (
          <div className="bg-gradient-to-r from-purple-50 to-blue-50 border border-purple-200 rounded-lg p-4">
            <p className="text-purple-800">
              <span className="font-bold">Welcome to the family, {session.name}!</span> 
              {" "}Your loyalty has been noted and your access is confirmed. Time to prove yourself.
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
            className="block group hover:shadow-xl transition-all duration-300 hover:scale-105"
          >
            <div className="card h-full border-2 border-transparent hover:border-gray-300">
              <div className="flex items-start space-x-4">
                <div className={`${feature.color} w-12 h-12 rounded-lg flex items-center justify-center text-white text-xl flex-shrink-0 shadow-lg`}>
                  {feature.icon}
                </div>
                <div className="flex-1">
                  <div className="flex items-center justify-between mb-2">
                    <h3 className="text-lg font-bold text-gray-900 group-hover:text-purple-600 transition-colors">
                      {feature.title}
                    </h3>
                    {feature.status === 'completed' && (
                      <span className="bg-green-100 text-green-800 text-xs px-2 py-1 rounded-full border border-green-300">
                        ✓ Respect Earned
                      </span>
                    )}
                  </div>
                  <p className="text-gray-600 text-sm mb-2">{feature.description}</p>
                  <p className="text-xs text-gray-500 italic">{feature.quote}</p>
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
          <h3 className="text-lg font-semibold text-gray-900 mb-4">🔴 Family Activity</h3>
          <div className="space-y-3">
            {latestScoreUpdate && (
              <div className="bg-green-50 border border-green-200 rounded-md p-3">
                <p className="text-sm text-green-800">
                  <span className="font-bold">{latestScoreUpdate.playerName}</span> just earned{' '}
                  <span className="font-bold">{latestScoreUpdate.score}</span> respect points in Territory Wars!
                </p>
                <p className="text-xs text-green-600 mt-1">
                  "That's how we do business" - {new Date(latestScoreUpdate.timestamp).toLocaleTimeString()}
                </p>
              </div>
            )}
            
            {!latestScoreUpdate && (
              <div className="text-gray-500 text-sm italic">
                "Nobody's made their mark yet. Don't let anyone outshine you."
              </div>
            )}
          </div>
        </div>

        {/* Empire Architecture */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">🏗️ Empire Foundation</h3>
          <div className="space-y-2">
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-purple-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Vertical Slice Structure (Family Hierarchy)</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-red-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Redis Memory Bank (Never Forgets)</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Azure AI Consigliere (Smart Decisions)</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-green-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Real-time Communication Network</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-yellow-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Clean Code Discipline</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="w-2 h-2 bg-pink-500 rounded-full"></div>
              <span className="text-sm text-gray-700">Modern React Operations</span>
            </div>
          </div>
        </div>
      </div>

      {/* Empire Initiation Instructions */}
      <div className="card bg-gradient-to-r from-purple-50 to-blue-50 border-purple-200">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">🎯 Initiation Protocol</h3>
        <p className="text-sm text-gray-600 mb-4 italic">
          "First you get the knowledge, then you get the power, then you get the respect."
        </p>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="text-center">
            <div className="bg-purple-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-purple-600 font-bold text-lg">🧠</span>
            </div>
            <h4 className="font-bold text-gray-900 mb-1">Prove Intelligence</h4>
            <p className="text-xs text-gray-600">Pass the family intelligence test</p>
          </div>
          <div className="text-center">
            <div className="bg-red-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-red-600 font-bold text-lg">🚀</span>
            </div>
            <h4 className="font-bold text-gray-900 mb-1">Claim Territory</h4>
            <p className="text-xs text-gray-600">Dominate the space wars</p>
          </div>
          <div className="text-center">
            <div className="bg-green-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-green-600 font-bold text-lg">💡</span>
            </div>
            <h4 className="font-bold text-gray-900 mb-1">Learn Secrets</h4>
            <p className="text-xs text-gray-600">Discover family wisdom</p>
          </div>
          <div className="text-center">
            <div className="bg-blue-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-2">
              <span className="text-blue-600 font-bold text-lg">📊</span>
            </div>
            <h4 className="font-bold text-gray-900 mb-1">Watch Business</h4>
            <p className="text-xs text-gray-600">Monitor empire operations</p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Home