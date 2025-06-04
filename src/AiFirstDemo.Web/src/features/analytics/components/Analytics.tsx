import React, { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useSignalR } from '../../../shared/contexts/SignalRContext'
import { analyticsApi } from '../../../shared/services/api'
import { DataTable } from '../../../shared/components/DataTable'
import { UnifiedParticipant } from '../../../shared/types'
import { getRandomGodfatherQuote, getRandomAIPacinoQuote } from '../../../shared/utils'
import { 
  createQuizColumns, 
  createGameColumns, 
  createTipsColumns
} from './analyticsColumns'

const Analytics: React.FC = React.memo(() => {
  const { latestAnalyticsUpdate } = useSignalR()
  
  // Pagination state
  const [quizPagination, setQuizPagination] = useState({ page: 0, pageSize: 10 })
  const [gamePagination, setGamePagination] = useState({ page: 0, pageSize: 10 })
  const [tipsPagination, setTipsPagination] = useState({ page: 0, pageSize: 10 })

  // Memoize query functions with pagination
  const queryOptions = useMemo(() => ({
    quiz: {
      queryKey: ['analytics-quiz-participants', quizPagination.page, quizPagination.pageSize],
      queryFn: () => analyticsApi.getQuizParticipants(quizPagination.pageSize, quizPagination.page * quizPagination.pageSize),
      refetchInterval: 15000, // Reduced frequency due to pagination
      retry: 2,
      staleTime: 10000, // 10 seconds stale time
    },
    game: {
      queryKey: ['analytics-game-participants', gamePagination.page, gamePagination.pageSize],
      queryFn: () => analyticsApi.getGameParticipants(gamePagination.pageSize, gamePagination.page * gamePagination.pageSize),
      refetchInterval: 15000, // Reduced frequency due to pagination
      retry: 2,
      staleTime: 10000, // 10 seconds stale time
    },
    tips: {
      queryKey: ['analytics-tips-contributors', tipsPagination.page, tipsPagination.pageSize],
      queryFn: () => analyticsApi.getTipsContributors(tipsPagination.pageSize, tipsPagination.page * tipsPagination.pageSize),
      refetchInterval: 30000, // Tips change less frequently
      retry: 2,
      staleTime: 20000, // 20 seconds stale time
    }
  }), [quizPagination, gamePagination, tipsPagination])

  // Fetch paginated data
  const { data: quizResponse, isLoading: quizInitialLoading, isFetching: quizFetching, error: quizError } = useQuery(queryOptions.quiz)
  const { data: gameResponse, isLoading: gameInitialLoading, isFetching: gameFetching, error: gameError } = useQuery(queryOptions.game)
  const { data: tipsResponse, isLoading: tipsInitialLoading, isFetching: tipsFetching, error: tipsError } = useQuery(queryOptions.tips)

  // Extract data and pagination info
  const quizData = useMemo(() => quizResponse?.data || [], [quizResponse])
  const gameData = useMemo(() => gameResponse?.data || [], [gameResponse])
  const tipsData = useMemo(() => tipsResponse?.data || [], [tipsResponse])

  // Memoize computed values
  const participantCounts = useMemo(() => ({
    quiz: quizResponse?.total || 0,
    game: gameResponse?.total || 0,
    tips: tipsResponse?.total || 0
  }), [quizResponse, gameResponse, tipsResponse])

  // Create memoized columns with correct pagination offset
  const columns = useMemo(() => ({
    quiz: createQuizColumns(quizPagination.page * quizPagination.pageSize),
    game: createGameColumns(gamePagination.page * gamePagination.pageSize),
    tips: createTipsColumns(tipsPagination.page * tipsPagination.pageSize)
  }), [quizPagination, gamePagination, tipsPagination])

  const isAnyFetching = useMemo(() => 
    (quizFetching && !quizInitialLoading) || 
    (gameFetching && !gameInitialLoading) || 
    (tipsFetching && !tipsInitialLoading)
  , [quizFetching, quizInitialLoading, gameFetching, gameInitialLoading, tipsFetching, tipsInitialLoading])

  const lastUpdateTime = useMemo(() => 
    latestAnalyticsUpdate ? new Date(latestAnalyticsUpdate.timestamp).toLocaleTimeString() : null
  , [latestAnalyticsUpdate])

  // Memoize random quotes for loading states
  const loadingQuotes = useMemo(() => ({
    quiz: getRandomAIPacinoQuote(),
    game: getRandomGodfatherQuote(),
    tips: getRandomAIPacinoQuote()
  }), []) // Only generate once per component mount

  return (
    <div className="max-w-7xl mx-auto p-6">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-2">
          🎬 AI Pacino Analytics Dashboard
        </h1>
        <p className="text-gray-600 text-lg">
          "Say hello to my little algorithm!" - Track performance, competition, and wisdom in the last 30 days.
        </p>
        {lastUpdateTime && (
          <div className="mt-2 text-sm text-green-600">
            🔴 Live Feed • Last update: {lastUpdateTime}
          </div>
        )}
      </div>

      {/* Analytics Tables Section */}
      <div className="space-y-8">
        {/* Quiz Analytics Table */}
        <DataTable<UnifiedParticipant>
          data={quizData}
          columns={columns.quiz}
          title="🧠 The Intelligence Family"
          subtitle={`${participantCounts.quiz} brain warriors • "In this business, you gotta think fast"`}
          isLoading={quizInitialLoading}
          error={quizError?.message || null}
          searchPlaceholder="Search the family..."
          emptyStateIcon="🤔"
          emptyStateMessage="Nobody's smart enough yet"
          emptyStateSubMessage="Take the quiz to join the intelligence family"
          colorTheme="purple"
          loadingQuote={loadingQuotes.quiz}
          pagination={{
            pageIndex: quizPagination.page,
            pageSize: quizPagination.pageSize,
            pageCount: Math.ceil(participantCounts.quiz / quizPagination.pageSize),
            total: participantCounts.quiz,
            hasNext: quizResponse?.hasNext || false,
            hasPrevious: quizResponse?.hasPrevious || false,
            onPageChange: (pageIndex: number) => setQuizPagination(prev => ({ ...prev, page: pageIndex })),
            onPageSizeChange: (pageSize: number) => setQuizPagination({ page: 0, pageSize })
          }}
        />

        {/* Game Analytics Table */}
        <DataTable<UnifiedParticipant>
          data={gameData}
          columns={columns.game}
          title="🚀 Scarface's Space Warriors"
          subtitle={`${participantCounts.game} power players • "The world is yours, but space is harder"`}
          isLoading={gameInitialLoading}
          error={gameError?.message || null}
          searchPlaceholder="Search the warriors..."
          emptyStateIcon="🎮"
          emptyStateMessage="No one's conquered space yet"
          emptyStateSubMessage="Play the game to claim your territory"
          colorTheme="blue"
          loadingQuote={loadingQuotes.game}
          pagination={{
            pageIndex: gamePagination.page,
            pageSize: gamePagination.pageSize,
            pageCount: Math.ceil(participantCounts.game / gamePagination.pageSize),
            total: participantCounts.game,
            hasNext: gameResponse?.hasNext || false,
            hasPrevious: gameResponse?.hasPrevious || false,
            onPageChange: (pageIndex: number) => setGamePagination(prev => ({ ...prev, page: pageIndex })),
            onPageSizeChange: (pageSize: number) => setGamePagination({ page: 0, pageSize })
          }}
        />

        {/* Tips Analytics Table */}
        <DataTable<UnifiedParticipant>
          data={tipsData}
          columns={columns.tips}
          title="💡 The Wise Guys"
          subtitle={`${participantCounts.tips} wisdom dealers • "Knowledge is power, power is everything"`}
          isLoading={tipsInitialLoading}
          error={tipsError?.message || null}
          searchPlaceholder="Search the wise guys..."
          emptyStateIcon="💭"
          emptyStateMessage="No wisdom dealers yet"
          emptyStateSubMessage="Share your knowledge to become a wise guy"
          colorTheme="green"
          loadingQuote={loadingQuotes.tips}
          pagination={{
            pageIndex: tipsPagination.page,
            pageSize: tipsPagination.pageSize,
            pageCount: Math.ceil(participantCounts.tips / tipsPagination.pageSize),
            total: participantCounts.tips,
            hasNext: tipsResponse?.hasNext || false,
            hasPrevious: tipsResponse?.hasPrevious || false,
            onPageChange: (pageIndex: number) => setTipsPagination(prev => ({ ...prev, page: pageIndex })),
            onPageSizeChange: (pageSize: number) => setTipsPagination({ page: 0, pageSize })
          }}
        />
      </div>

      {/* Footer with Real-time Info */}
      <div className="mt-8 pt-6 border-t border-gray-200">
        <div className="flex items-center justify-between text-sm text-gray-500">
          <div className="flex items-center space-x-4">
            <span>📡 Real-time intelligence network</span>
            <span>🔄 Smart pagination system</span>
            <span>⚡ Lightning-fast search</span>
            <span>🎭 "AI Pacino approved"</span>
          </div>
          {isAnyFetching ? (
            <div className="text-blue-600 flex items-center">
              <span className="animate-spin mr-2">⟳</span>
              Updating the family records...
            </div>
          ) : (
            <div className="text-green-600">
              ✓ "Everything's running smooth, boss"
            </div>
          )}
        </div>
      </div>
    </div>
  )
})

Analytics.displayName = 'Analytics'

export default Analytics