import React from 'react'
import { Link } from 'react-router-dom'
import { useSignalR } from '../shared/contexts/SignalRContext'

const HomePage: React.FC = () => {
  const { latestScoreUpdate } = useSignalR()

  return (
    <div className="space-y-8">
      {/* Hero Section */}
      <div className="text-center py-12 bg-gradient-to-br from-purple-50 to-blue-50 rounded-3xl">
        <h1 className="text-5xl font-bold text-gray-800 mb-4">
          🚀 AI First Demo
        </h1>
        <p className="text-xl text-gray-600 mb-8 max-w-2xl mx-auto">
          Experience the power of AI-First development with Cursor and Windsurf. 
          Explore interactive features, real-time analytics, and modern web technologies.
        </p>
        <div className="flex flex-wrap justify-center gap-4">
          <Link to="/quiz" className="btn btn-primary text-lg px-8 py-4">
            🧠 Start AI Quiz
          </Link>
          <Link to="/game" className="btn btn-secondary text-lg px-8 py-4">
            🎮 Play Game
          </Link>
        </div>
      </div>

      {/* Features Grid */}
      <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Link to="/quiz" className="feature-card group">
          <div className="text-4xl mb-4 group-hover:scale-110 transition-transform">🧠</div>
          <h3 className="text-xl font-bold text-gray-800 mb-2">AI Quiz</h3>
          <p className="text-gray-600 text-sm">
            Test your knowledge with AI-powered questions and get personalized feedback.
          </p>
        </Link>

        <Link to="/game" className="feature-card group">
          <div className="text-4xl mb-4 group-hover:scale-110 transition-transform">🎮</div>
          <h3 className="text-xl font-bold text-gray-800 mb-2">Spaceship Game</h3>
          <p className="text-gray-600 text-sm">
            Navigate through space, destroy asteroids, and compete on the leaderboard.
          </p>
        </Link>

        <Link to="/tips" className="feature-card group">
          <div className="text-4xl mb-4 group-hover:scale-110 transition-transform">💡</div>
          <h3 className="text-xl font-bold text-gray-800 mb-2">Tips & Tricks</h3>
          <p className="text-gray-600 text-sm">
            Discover expert tips for Cursor, Windsurf, and AI-First development.
          </p>
        </Link>

        <Link to="/analytics" className="feature-card group">
          <div className="text-4xl mb-4 group-hover:scale-110 transition-transform">📊</div>
          <h3 className="text-xl font-bold text-gray-800 mb-2">Analytics</h3>
          <p className="text-gray-600 text-sm">
            View real-time analytics and insights from all platform activities.
          </p>
        </Link>
      </div>

      {/* Live Updates */}
      {latestScoreUpdate && (
        <div className="bg-green-50 border border-green-200 rounded-xl p-4">
          <h3 className="text-lg font-semibold text-green-800 mb-2">🎉 Latest Activity</h3>
          <p className="text-green-700">
            {latestScoreUpdate.playerName} just scored {latestScoreUpdate.score} points in the game!
          </p>
        </div>
      )}

      {/* Technology Stack */}
      <div className="bg-white rounded-2xl shadow-lg p-8">
        <h2 className="text-3xl font-bold text-gray-800 mb-6 text-center">⚡ Powered By</h2>
        <div className="grid md:grid-cols-3 gap-6">
          <div className="text-center">
            <div className="text-3xl mb-3">🔷</div>
            <h3 className="font-bold text-gray-800">.NET 8</h3>
            <p className="text-gray-600 text-sm">Modern backend with vertical slice architecture</p>
          </div>
          <div className="text-center">
            <div className="text-3xl mb-3">⚛️</div>
            <h3 className="font-bold text-gray-800">React + TypeScript</h3>
            <p className="text-gray-600 text-sm">Type-safe frontend with modern React patterns</p>
          </div>
          <div className="text-center">
            <div className="text-3xl mb-3">🧠</div>
            <h3 className="font-bold text-gray-800">Azure OpenAI</h3>
            <p className="text-gray-600 text-sm">AI-powered features and intelligent insights</p>
          </div>
        </div>
      </div>
    </div>
  )
}

export default HomePage 