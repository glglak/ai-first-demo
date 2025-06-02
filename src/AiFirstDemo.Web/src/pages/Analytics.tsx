import React from 'react'
import { useQuery } from '@tanstack/react-query'
import { useSignalR } from '../contexts/SignalRContext'
import { analyticsApi } from '../services/api'

const Analytics: React.FC = () => {
  const { latestAnalyticsUpdate } = useSignalR()

  // Fetch dashboard data
  const { data: dashboard, isLoading } = useQuery({
    queryKey: ['analytics-dashboard'],
    queryFn: analyticsApi.getDashboard,
    refetchInterval: 30000, // Refetch every 30 seconds
  })

  // Fetch active users
  const { data: activeUsers } = useQuery({
    queryKey: ['analytics-active-users'],
    queryFn: analyticsApi.getActiveUsers,
    refetchInterval: 10000, // Refetch every 10 seconds
  })

  if (isLoading) {
    return (
      <div className="text-center py-12">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-4" />
        <p className="text-gray-600">Loading analytics...</p>
      </div>
    )
  }

  if (!dashboard) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-600">No analytics data available.</p>
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          📊 Live Analytics Dashboard
        </h1>
        <p className="text-gray-600">
          Real-time insights into demo usage and performance. Updates automatically via SignalR.
        </p>
        {latestAnalyticsUpdate && (
          <div className="mt-2 text-sm text-green-600">
            🔴 Live • Last update: {new Date(latestAnalyticsUpdate.timestamp).toLocaleTimeString()}
          </div>
        )}
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <MetricCard
          title="Total Sessions"
          value={dashboard.totalSessions}
          icon="👥"
          color="bg-blue-500"
        />
        <MetricCard
          title="Active Now"
          value={dashboard.activeSessionsNow}
          icon="🟢"
          color="bg-green-500"
        />
        <MetricCard
          title="Quizzes Completed"
          value={dashboard.quizzesCompleted}
          icon="🧠"
          color="bg-purple-500"
        />
        <MetricCard
          title="Games Played"
          value={dashboard.gamesPlayed}
          icon="🚀"
          color="bg-orange-500"
        />
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        {/* Hourly Activity */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            📈 Hourly Activity (Last 24h)
          </h3>
          <div className="h-64">
            <SimpleBarChart data={dashboard.hourlyActivity} />
          </div>
        </div>

        {/* Quiz Performance */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            🎯 Quiz Performance by Category
          </h3>
          <div className="space-y-3">
            {dashboard.quizPerformance.map((perf, index) => (
              <div key={index} className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-700">
                  {perf.category}
                </span>
                <div className="flex items-center space-x-2">
                  <div className="w-24 bg-gray-200 rounded-full h-2">
                    <div
                      className="bg-blue-600 h-2 rounded-full"
                      style={{ width: `${perf.averageScore}%` }}
                    />
                  </div>
                  <span className="text-sm text-gray-600">
                    {perf.averageScore.toFixed(1)}%
                  </span>
                  <span className="text-xs text-gray-500">
                    ({perf.totalAttempts})
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Additional Stats */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Popular Tips */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            💡 Popular Tips
          </h3>
          <div className="space-y-3">
            {dashboard.popularTips.slice(0, 5).map((tip, index) => (
              <div key={index} className="flex items-center justify-between">
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 truncate">
                    {tip.title}
                  </p>
                  <p className="text-xs text-gray-500">{tip.category}</p>
                </div>
                <div className="flex items-center space-x-1 text-sm text-gray-600">
                  <span>❤️</span>
                  <span>{tip.likes}</span>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* System Health */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            ⚡ System Health
          </h3>
          <div className="space-y-3">
            <HealthIndicator label="API Response" status="healthy" value="<100ms" />
            <HealthIndicator label="Redis Connection" status="healthy" value="Connected" />
            <HealthIndicator label="Azure OpenAI" status="healthy" value="Active" />
            <HealthIndicator label="SignalR Hubs" status="healthy" value="2 Connected" />
          </div>
        </div>

        {/* Active Users */}
        <div className="card">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">
            👤 Recent Activity
          </h3>
          <div className="space-y-3">
            {activeUsers?.slice(0, 5).map((user, index) => (
              <div key={index} className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-900">
                    {user.userName}
                  </p>
                  <p className="text-xs text-gray-500">
                    {new Date(user.lastActivity).toLocaleTimeString()}
                  </p>
                </div>
                <div className="flex items-center space-x-2">
                  {user.completedQuiz && (
                    <span className="text-green-600 text-xs">✓ Quiz</span>
                  )}
                  {user.gameAttempts > 0 && (
                    <span className="text-blue-600 text-xs">{user.gameAttempts} Games</span>
                  )}
                </div>
              </div>
            )) || (
              <div className="text-sm text-gray-500 italic">
                No recent activity
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

// Metric Card Component
interface MetricCardProps {
  title: string
  value: number
  icon: string
  color: string
}

const MetricCard: React.FC<MetricCardProps> = ({ title, value, icon, color }) => {
  return (
    <div className="card">
      <div className="flex items-center">
        <div className={`${color} w-12 h-12 rounded-lg flex items-center justify-center text-white text-xl mr-4`}>
          {icon}
        </div>
        <div>
          <p className="text-sm font-medium text-gray-600">{title}</p>
          <p className="text-2xl font-bold text-gray-900">{value.toLocaleString()}</p>
        </div>
      </div>
    </div>
  )
}

// Health Indicator Component
interface HealthIndicatorProps {
  label: string
  status: 'healthy' | 'warning' | 'error'
  value: string
}

const HealthIndicator: React.FC<HealthIndicatorProps> = ({ label, status, value }) => {
  const getStatusColor = () => {
    switch (status) {
      case 'healthy': return 'text-green-600'
      case 'warning': return 'text-yellow-600'
      case 'error': return 'text-red-600'
      default: return 'text-gray-600'
    }
  }

  const getStatusIcon = () => {
    switch (status) {
      case 'healthy': return '✅'
      case 'warning': return '⚠️'
      case 'error': return '❌'
      default: return '❓'
    }
  }

  return (
    <div className="flex items-center justify-between">
      <div className="flex items-center space-x-2">
        <span>{getStatusIcon()}</span>
        <span className="text-sm text-gray-700">{label}</span>
      </div>
      <span className={`text-sm font-medium ${getStatusColor()}`}>
        {value}
      </span>
    </div>
  )
}

// Simple Bar Chart Component
interface SimpleBarChartProps {
  data: Array<{
    hour: string
    sessions: number
    quizAttempts: number
    gamePlays: number
  }>
}

const SimpleBarChart: React.FC<SimpleBarChartProps> = ({ data }) => {
  const maxValue = Math.max(...data.map(d => d.sessions + d.quizAttempts + d.gamePlays))
  
  return (
    <div className="h-full flex items-end justify-between space-x-1">
      {data.slice(-12).map((item, index) => {
        const totalHeight = ((item.sessions + item.quizAttempts + item.gamePlays) / Math.max(maxValue, 1)) * 200
        const sessionsHeight = (item.sessions / Math.max(maxValue, 1)) * 200
        const quizHeight = (item.quizAttempts / Math.max(maxValue, 1)) * 200
        const gameHeight = (item.gamePlays / Math.max(maxValue, 1)) * 200
        
        return (
          <div key={index} className="flex flex-col items-center flex-1">
            <div className="w-full relative" style={{ height: '200px' }}>
              <div 
                className="absolute bottom-0 w-full bg-blue-500 rounded-t"
                style={{ height: `${sessionsHeight}px` }}
              />
              <div 
                className="absolute bottom-0 w-full bg-green-500 rounded-t opacity-75"
                style={{ height: `${quizHeight}px`, transform: `translateY(-${sessionsHeight}px)` }}
              />
              <div 
                className="absolute bottom-0 w-full bg-orange-500 rounded-t opacity-75"
                style={{ height: `${gameHeight}px`, transform: `translateY(-${sessionsHeight + quizHeight}px)` }}
              />
            </div>
            <div className="text-xs text-gray-500 mt-2">
              {new Date(item.hour).getHours()}h
            </div>
          </div>
        )
      })}
    </div>
  )
}

export default Analytics