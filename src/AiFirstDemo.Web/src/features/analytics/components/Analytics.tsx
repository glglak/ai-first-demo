import React, { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useSignalR } from '../../../shared/contexts/SignalRContext'
import { analyticsApi } from '../../../shared/services/api'
import { DataTable } from '../../../shared/components/DataTable'
import { 
  quizColumns, 
  gameColumns, 
  tipsColumns,
  QuizParticipant,
  GameParticipant,
  TipsContributor 
} from './analyticsColumns'

const Analytics: React.FC = React.memo(() => {
  const { latestAnalyticsUpdate } = useSignalR()

  // Memoize query functions with faster refresh for demo
  const queryOptions = useMemo(() => ({
    quiz: {
      queryKey: ['analytics-quiz-participants'],
      queryFn: analyticsApi.getQuizParticipants,
      refetchInterval: 5000, // Refetch every 5 seconds for demo
      retry: 2,
      staleTime: 0, // Always consider data stale for real-time updates
    },
    game: {
      queryKey: ['analytics-game-participants'],
      queryFn: analyticsApi.getGameParticipants,
      refetchInterval: 3000, // Refetch every 3 seconds for demo
      retry: 2,
      staleTime: 0, // Always consider data stale for real-time updates
    },
    tips: {
      queryKey: ['analytics-tips-contributors'],
      queryFn: analyticsApi.getTipsContributors,
      refetchInterval: 10000, // Refetch every 10 seconds for demo
      retry: 2,
      staleTime: 0, // Always consider data stale for real-time updates
    }
  }), [])

  // Only fetch the three working leaderboard endpoints
  const { data: quizParticipants, isLoading: quizInitialLoading, isFetching: quizFetching, error: quizError } = useQuery(queryOptions.quiz)
  const { data: gameParticipants, isLoading: gameInitialLoading, isFetching: gameFetching, error: gameError } = useQuery(queryOptions.game)
  const { data: tipsContributors, isLoading: tipsInitialLoading, isFetching: tipsFetching, error: tipsError } = useQuery(queryOptions.tips)

  // Memoize computed values
  const participantCounts = useMemo(() => ({
    quiz: (quizParticipants || []).length,
    game: (gameParticipants || []).length,
    tips: (tipsContributors || []).length
  }), [quizParticipants, gameParticipants, tipsContributors])

  const isAnyFetching = useMemo(() => 
    (quizFetching && !quizInitialLoading) || 
    (gameFetching && !gameInitialLoading) || 
    (tipsFetching && !tipsInitialLoading)
  , [quizFetching, quizInitialLoading, gameFetching, gameInitialLoading, tipsFetching, tipsInitialLoading])

  const lastUpdateTime = useMemo(() => 
    latestAnalyticsUpdate ? new Date(latestAnalyticsUpdate.timestamp).toLocaleTimeString() : null
  , [latestAnalyticsUpdate])

  return (
    <div className="max-w-7xl mx-auto p-6">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          📊 Live Analytics Dashboard
        </h1>
        <p className="text-gray-600">
          Real-time insights into demo usage and performance. Data updates every few seconds for live demo experience.
        </p>
        {lastUpdateTime && (
          <div className="mt-2 text-sm text-green-600">
            🔴 Live • Last update: {lastUpdateTime}
          </div>
        )}
      </div>

      {/* Analytics Tables Section */}
      <div className="space-y-8">
        {/* Quiz Analytics Table */}
        <DataTable<QuizParticipant>
          data={quizParticipants || []}
          columns={quizColumns}
          title="🧠 Quiz Analytics"
          subtitle={`${participantCounts.quiz} participants tracked • Updates every 5s`}
          isLoading={quizInitialLoading}
          error={quizError?.message || null}
          searchPlaceholder="Search participants..."
          emptyStateIcon="🤔"
          emptyStateMessage="No quiz data yet"
          emptyStateSubMessage="Complete a quiz to see analytics"
          colorTheme="purple"
        />

        {/* Game Analytics Table */}
        <DataTable<GameParticipant>
          data={gameParticipants || []}
          columns={gameColumns}
          title="🚀 Game Leaderboard"
          subtitle={`${participantCounts.game} high scores tracked • Updates every 3s`}
          isLoading={gameInitialLoading}
          error={gameError?.message || null}
          searchPlaceholder="Search players..."
          emptyStateIcon="🎮"
          emptyStateMessage="No game scores yet"
          emptyStateSubMessage="Play the spaceship game to see scores"
          colorTheme="blue"
        />

        {/* Tips Analytics Table */}
        <DataTable<TipsContributor>
          data={tipsContributors || []}
          columns={tipsColumns}
          title="💡 Tips Contributors"
          subtitle={`${participantCounts.tips} contributors tracked • Updates every 10s`}
          isLoading={tipsInitialLoading}
          error={tipsError?.message || null}
          searchPlaceholder="Search contributors..."
          emptyStateIcon="💭"
          emptyStateMessage="No tips data yet"
          emptyStateSubMessage="Create tips to see analytics"
          colorTheme="green"
        />
      </div>

      {/* Footer with Real-time Info */}
      <div className="mt-8 pt-6 border-t border-gray-200">
        <div className="flex items-center justify-between text-sm text-gray-500">
          <div className="flex items-center space-x-4">
            <span>📡 Real-time via SignalR</span>
            <span>🔄 Fast auto-refresh for demo</span>
            <span>⚡ Sortable & Searchable</span>
          </div>
          {isAnyFetching ? (
            <div className="text-blue-600 flex items-center">
              <span className="animate-spin mr-2">⟳</span>
              Updating data...
            </div>
          ) : (
            <div className="text-green-600">
              ✓ Data current
            </div>
          )}
        </div>
      </div>
    </div>
  )
})

Analytics.displayName = 'Analytics'

export default Analytics