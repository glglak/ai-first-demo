// User Session Types
export interface UserSession {
  sessionId: string
  name: string
  createdAt: string
  hasCompletedQuiz: boolean
}

export interface CreateSessionRequest {
  name: string
  ipAddress: string
}

// Quiz Types
export interface QuizQuestion {
  id: string
  text: string
  options: string[]
  correctAnswer: string
  category: string
  difficulty: string
}

export interface QuizSubmission {
  sessionId: string
  answers: AnswerSubmission[]
}

export interface AnswerSubmission {
  questionId: string
  selectedAnswer: string
}

export interface QuizResult {
  score: number
  totalQuestions: number
  percentage: number
  results: QuestionResult[]
  aiAnalysis: string
  recommendations: string[]
}

export interface QuestionResult {
  questionText: string
  selectedAnswer: string
  correctAnswer: string
  isCorrect: boolean
  explanation: string
}

// Game Types
export interface GameScore {
  sessionId: string
  playerName: string
  score: number
  achievedAt: string
  gameDuration: string
  level: number
  asteroidsDestroyed: number
}

export interface SubmitScoreRequest {
  sessionId: string
  playerName: string
  score: number
  gameDuration: string
  level: number
  asteroidsDestroyed: number
}

export interface LeaderboardEntry {
  playerName: string
  score: number
  achievedAt: string
  level: number
  rank: number
}

export interface LeaderboardResponse {
  dailyLeaderboard: LeaderboardEntry[]
  weeklyLeaderboard: LeaderboardEntry[]
  allTimeLeaderboard: LeaderboardEntry[]
  playerBest?: LeaderboardEntry
}

export interface GameStats {
  totalGamesPlayed: number
  totalPlayersToday: number
  averageScore: number
  highestScoreToday: number
  topPlayerToday: string
}

// Tips Types
export interface Tip {
  id: string
  title: string
  content: string
  category: string
  difficulty: string
  tags: string[]
  createdAt: string
  likes: number
  isAiGenerated: boolean
}

export interface TipResponse extends Tip {
  isLikedByUser: boolean
}

export interface TipsCollectionResponse {
  tips: TipResponse[]
  totalCount: number
  category: string
  availableCategories: string[]
}

export interface CreateTipRequest {
  title: string
  content: string
  category: string
  difficulty: string
  tags: string[]
}

export interface GenerateTipsRequest {
  category: string
  count?: number
  difficulty?: string
  context?: string
}

// Analytics Types
export interface DashboardData {
  totalSessions: number
  activeSessionsNow: number
  quizzesCompleted: number
  gamesPlayed: number
  averageQuizScore: number
  topGameScore: number
  hourlyActivity: HourlyActivity[]
  quizPerformance: CategoryPerformance[]
  popularTips: PopularTip[]
}

export interface HourlyActivity {
  hour: string
  sessions: number
  quizAttempts: number
  gamePlays: number
}

export interface CategoryPerformance {
  category: string
  averageScore: number
  totalAttempts: number
}

export interface PopularTip {
  title: string
  likes: number
  category: string
}

export interface UserActivity {
  sessionId: string
  userName: string
  lastActivity: string
  completedQuiz: boolean
  gameAttempts: number
  bestScore: number
}

// Game Engine Types
export interface Vector2 {
  x: number
  y: number
}

export interface GameObject {
  position: Vector2
  velocity: Vector2
  rotation: number
  size: number
  active: boolean
}

export interface Spaceship extends GameObject {
  health: number
  maxHealth: number
  fireRate: number
  lastFired: number
}

export interface Asteroid extends GameObject {
  rotationSpeed: number
  points: number
}

export interface Laser extends GameObject {
  damage: number
  lifetime: number
  maxLifetime: number
}

export interface GameState {
  spaceship: Spaceship
  asteroids: Asteroid[]
  lasers: Laser[]
  score: number
  level: number
  lives: number
  gameStatus: 'playing' | 'paused' | 'gameOver' | 'starting'
  asteroidsDestroyed: number
  gameStartTime: number
}

// SignalR Types
export interface SignalRMessage {
  type: string
  data: any
  timestamp: string
}

export interface ScoreUpdate {
  playerName: string
  score: number
  timestamp: string
}

export interface GameEvent {
  eventType: string
  data: any
  timestamp: string
}

export interface AnalyticsUpdate {
  data: any
  timestamp: string
}