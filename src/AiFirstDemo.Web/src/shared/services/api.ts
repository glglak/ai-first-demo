import axios from 'axios'
import {
  UserSession,
  QuizQuestion,
  QuizSubmission,
  QuizResult,
  GameScore,
  LeaderboardResponse,
  SubmitScoreRequest,
  GameStats,
  Tip,
  TipsCollectionResponse,
  CreateTipRequest,
  GenerateTipsRequest,
  DashboardData,
  UserActivity,
  HourlyActivity,
  UnifiedLeaderboardResponse,
  UnifiedParticipant,
  PaginatedResponse
} from '../types'

const api = axios.create({
  baseURL: '/api',
  timeout: 30000,
})

// Request interceptor for logging
api.interceptors.request.use(
  (config) => {
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`)
    return config
  },
  (error) => {
    console.error('API Request Error:', error)
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    console.error('API Response Error:', error.response?.data || error.message)
    return Promise.reject(error)
  }
)

// Session API
export const sessionApi = {
  create: (request: { name: string }): Promise<UserSession> =>
    api.post('/sessions', { name: request.name }).then((res) => res.data),
  
  get: (sessionId: string): Promise<UserSession> =>
    api.get(`/sessions/${sessionId}`).then((res) => res.data),
  
  updateActivity: (sessionId: string): Promise<void> =>
    api.post(`/sessions/${sessionId}/activity`).then((res) => res.data),
}

// Quiz API
export const quizApi = {
  getQuestions: (): Promise<QuizQuestion[]> =>
    api.get('/quiz/questions').then((res) => res.data),
  
  submit: (submission: QuizSubmission): Promise<QuizResult> =>
    api.post('/quiz/submit', submission).then((res) => res.data),
  
  getAttempt: (sessionId: string): Promise<any> =>
    api.get(`/quiz/attempt/${sessionId}`).then((res) => res.data),
  
  generateQuestions: (count: number = 10): Promise<QuizQuestion[]> =>
    api.post(`/quiz/generate?count=${count}`).then((res) => res.data),

  getHint: (questionId: string): Promise<{ hint: string }> =>
    api.get(`/quiz/hint/${questionId}`).then((res) => res.data),
}

// Game API
export const gameApi = {
  submitScore: (request: SubmitScoreRequest): Promise<GameScore> =>
    api.post('/game/score', request).then((res) => res.data),
  
  getLeaderboard: (sessionId?: string): Promise<LeaderboardResponse> =>
    api.get(`/game/leaderboard${sessionId ? `?sessionId=${sessionId}` : ''}`).then((res) => res.data),
  
  getPlayerScores: (sessionId: string): Promise<GameScore[]> =>
    api.get(`/game/scores/${sessionId}`).then((res) => res.data),
  
  getStats: (): Promise<GameStats> =>
    api.get('/game/stats').then((res) => res.data),
}

// Tips API
export const tipsApi = {
  getTips: (category?: string, sessionId?: string): Promise<TipsCollectionResponse> => {
    const params = new URLSearchParams()
    if (category) params.append('category', category)
    if (sessionId) params.append('sessionId', sessionId)
    return api.get(`/tips?${params.toString()}`).then((res) => res.data)
  },
  
  getTip: (tipId: string): Promise<Tip> =>
    api.get(`/tips/${tipId}`).then((res) => res.data),
  
  createTip: (request: CreateTipRequest, sessionId: string): Promise<Tip> =>
    api.post(`/tips?sessionId=${sessionId}`, request).then((res) => res.data),
  
  generateTips: (request: GenerateTipsRequest): Promise<Tip[]> =>
    api.post('/tips/generate', request).then((res) => res.data),
  
  likeTip: (tipId: string, sessionId: string): Promise<void> =>
    api.post(`/tips/${tipId}/like?sessionId=${sessionId}`).then((res) => res.data),
  
  unlikeTip: (tipId: string, sessionId: string): Promise<void> =>
    api.delete(`/tips/${tipId}/like?sessionId=${sessionId}`).then((res) => res.data),
  
  getCategories: (): Promise<string[]> =>
    api.get('/tips/categories').then((res) => res.data),
}

// Analytics API
export const analyticsApi = {
  getDashboard: (): Promise<DashboardData> =>
    api.get('/analytics/dashboard').then((res) => res.data),
  
  getActiveUsers: (): Promise<UserActivity[]> =>
    api.get('/analytics/users/active').then((res) => res.data),
  
  trackActivity: (sessionId: string, activity: string): Promise<void> =>
    api.post(`/analytics/track/${sessionId}`, { activity }).then((res) => res.data),
  
  getHourlyActivity: (date: Date): Promise<HourlyActivity[]> =>
    api.get(`/analytics/activity/${date.toISOString().split('T')[0]}`).then((res) => res.data),
  
  getUnifiedLeaderboard: (): Promise<UnifiedLeaderboardResponse> =>
    api.get('/analytics/unified-leaderboard').then((res) => res.data),
  
  // Separate leaderboard endpoints with pagination support
  getQuizParticipants: (limit: number = 10, offset: number = 0): Promise<PaginatedResponse<UnifiedParticipant>> =>
    api.get(`/analytics/leaderboard/quiz?limit=${limit}&offset=${offset}`).then((res) => res.data),
  
  getGameParticipants: (limit: number = 10, offset: number = 0): Promise<PaginatedResponse<UnifiedParticipant>> =>
    api.get(`/analytics/leaderboard/game?limit=${limit}&offset=${offset}`).then((res) => res.data),
  
  getTipsContributors: (limit: number = 10, offset: number = 0): Promise<PaginatedResponse<UnifiedParticipant>> =>
    api.get(`/analytics/leaderboard/tips?limit=${limit}&offset=${offset}`).then((res) => res.data),
}

export default api