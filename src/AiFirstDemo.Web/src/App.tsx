import { Routes, Route, useNavigate, useLocation } from 'react-router-dom'
import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'

// Import our actual components
import SpaceshipGame from './features/game/components/SpaceshipGame'
import TipsAndTricks from './features/tips/components/TipsAndTricks'
import Analytics from './features/analytics/components/Analytics'

// Environment-based API configuration
const getApiBaseUrl = () => {
  // In production (Azure), use relative URLs
  if (import.meta.env.PROD) {
    return '/api'
  }
  
  // In development, use configured proxy or environment variable
  return import.meta.env.VITE_API_URL || '/api'
}

const API_BASE = getApiBaseUrl()

// Types based on backend models
interface QuizQuestion {
  id: string
  text: string
  options: string[]
  correctAnswer: string
  category: string
  difficulty: string
}

interface UserSession {
  sessionId: string
  name: string
  createdAt: string
  hasCompletedQuiz: boolean
}

interface QuizSubmission {
  sessionId: string
  answers: Array<{
    questionId: string
    selectedAnswer: string
  }>
}

interface QuizResult {
  score: number
  totalQuestions: number
  percentage: number
  results: Array<{
    questionText: string
    selectedAnswer: string
    correctAnswer: string
    isCorrect: boolean
    explanation: string
  }>
  aiAnalysis: string
  recommendations: string[]
}

// API Service
const apiService = {
  createSession: async (name: string, title: string): Promise<{ session: UserSession, quizAttempts: number, canTakeQuiz: boolean, maxAttempts: number }> => {
    const response = await fetch(`${API_BASE}/sessions`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: `${name} (${title})` })
    })
    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Failed to create session: ${errorText}`)
    }
    
    const session = await response.json()
    
    // Extract quiz attempt info from headers
    const quizAttempts = parseInt(response.headers.get('X-Quiz-Attempts') || '0')
    const canTakeQuiz = response.headers.get('X-Can-Take-Quiz') === 'true'
    const maxAttempts = parseInt(response.headers.get('X-Max-Quiz-Attempts') || '3')
    
    return { session, quizAttempts, canTakeQuiz, maxAttempts }
  },

  getQuestions: async (): Promise<QuizQuestion[]> => {
    const response = await fetch(`${API_BASE}/quiz/questions`)
    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Failed to fetch questions: ${errorText}`)
    }
    return response.json()
  },

  submitQuiz: async (submission: QuizSubmission): Promise<QuizResult> => {
    const response = await fetch(`${API_BASE}/quiz/submit`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(submission)
    })
    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Failed to submit quiz: ${errorText}`)
    }
    return response.json()
  },

  getSession: async (sessionId: string): Promise<UserSession> => {
    const response = await fetch(`${API_BASE}/sessions/${sessionId}`)
    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Failed to get session: ${errorText}`)
    }
    return response.json()
  }
}

// Job titles for dropdown
const JOB_TITLES = [
  'Software Engineer',
  'Senior Software Engineer', 
  'Software Specialist',
  'Senior Software Specialist',
  'Software Tech Lead',
  'Senior Tech Lead',
  'Dev Manager',
  'Senior Dev Manager',
  'Principal Dev Manager',
  'Cursor Expert',
  'AI First Crew'
]

// Navigation Component
const Navigation = () => {
  const location = useLocation()
  const navigate = useNavigate()
  
  const tabs = [
    { path: '/', label: '🧠 AI Quiz', color: 'from-purple-500 to-blue-500' },
    { path: '/game', label: '🎮 Spaceship Game', color: 'from-orange-500 to-red-500' },
    { path: '/tips', label: '💡 Tips & Tricks', color: 'from-green-500 to-teal-500' },
    { path: '/analytics', label: '📊 Analytics', color: 'from-indigo-500 to-purple-500' }
  ]

  return (
    <nav className="bg-white/90 backdrop-blur-sm shadow-lg rounded-2xl p-4 mb-6">
      <div className="flex flex-wrap justify-center gap-2">
        {tabs.map((tab) => (
          <button
            key={tab.path}
            onClick={() => navigate(tab.path)}
            className={`px-6 py-3 rounded-xl font-medium transition-all duration-200 transform hover:scale-105 ${
              location.pathname === tab.path
                ? `bg-gradient-to-r ${tab.color} text-white shadow-lg`
                : 'text-gray-600 hover:bg-gray-100 hover:text-gray-800'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>
    </nav>
  )
}

// Creative Background Component
const CreativeBackground = () => (
  <div className="fixed inset-0 -z-10 overflow-hidden">
    {/* Animated gradient background */}
    <div className="absolute inset-0 bg-gradient-to-br from-blue-50 via-indigo-100 to-purple-100 animate-gradient-xy"></div>
    
    {/* Floating geometric shapes */}
    <div className="absolute top-10 left-10 w-32 h-32 bg-gradient-to-br from-blue-200/30 to-purple-200/30 rounded-full blur-xl animate-float-slow"></div>
    <div className="absolute top-40 right-20 w-24 h-24 bg-gradient-to-br from-green-200/30 to-teal-200/30 rounded-full blur-lg animate-float-medium"></div>
    <div className="absolute bottom-20 left-20 w-40 h-40 bg-gradient-to-br from-purple-200/20 to-pink-200/20 rounded-full blur-2xl animate-float-fast"></div>
    <div className="absolute bottom-40 right-40 w-20 h-20 bg-gradient-to-br from-orange-200/30 to-red-200/30 rounded-full blur-md animate-float-slow"></div>
    
    {/* Grid pattern overlay */}
    <div className="absolute inset-0 bg-grid-white/10 bg-[size:50px_50px] opacity-20"></div>
    
    {/* Subtle noise texture */}
    <div className="absolute inset-0 bg-noise opacity-5"></div>
  </div>
)

// Main Layout Component
const Layout = ({ children }: { children: React.ReactNode }) => (
  <div className="min-h-screen relative">
    <CreativeBackground />
    <div className="relative z-10 container mx-auto px-4 py-6 max-w-6xl">
      <div className="text-center mb-8">
        <h1 className="text-4xl md:text-5xl font-bold bg-gradient-to-r from-purple-600 via-blue-600 to-indigo-600 bg-clip-text text-transparent mb-2">
          🚀 AI First Demo
        </h1>
        <p className="text-gray-600 text-lg">
          Showcase of AI-First Development with Cursor
        </p>
      </div>
      <Navigation />
      <main>{children}</main>
    </div>
  </div>
)

// Quiz Component
const QuizPage = () => {
  const [currentSession, setCurrentSession] = useState<UserSession | null>(null)
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0)
  const [answers, setAnswers] = useState<Record<string, string>>({})
  const [showResults, setShowResults] = useState(false)
  const [showQuizForm, setShowQuizForm] = useState(true)
  const [userName, setUserName] = useState('')
  const [selectedTitle, setSelectedTitle] = useState('')
  const [quizAttempts, setQuizAttempts] = useState(0)
  const [canTakeQuiz, setCanTakeQuiz] = useState(true)
  const [maxAttempts, setMaxAttempts] = useState(3)
  const [hints, setHints] = useState<Record<string, string>>({})
  const [loadingHints, setLoadingHints] = useState<Record<string, boolean>>({})
  const queryClient = useQueryClient()

  // Auto-detect user info on component mount
  useEffect(() => {
    const detectUserInfo = () => {
      // Try to get saved info from localStorage
      const savedName = localStorage.getItem('userName')
      if (savedName) {
        setUserName(savedName)
      }

      // Try to detect from browser/system info
      const userAgent = navigator.userAgent
      const platform = navigator.platform
      
      // Simple heuristics for role detection
      if (userAgent.includes('Windows') && platform.includes('Win')) {
        setSelectedTitle('Software Engineer')
      } else if (userAgent.includes('Mac')) {
        setSelectedTitle('Senior Software Engineer')
      }
    }

    detectUserInfo()

    // Check for existing session
    const savedSessionId = localStorage.getItem('quizSessionId')
    if (savedSessionId) {
      apiService.getSession(savedSessionId)
        .then(session => {
          setCurrentSession(session)
          setShowQuizForm(false)
          if (session.hasCompletedQuiz) {
            setShowResults(true)
          }
        })
        .catch(() => {
          // Session invalid, remove from storage
          localStorage.removeItem('quizSessionId')
        })
    }
  }, [])

  // Mutations and queries
  const createSessionMutation = useMutation({
    mutationFn: ({ name, title }: { name: string, title: string }) => 
      apiService.createSession(name, title),
    onSuccess: (response) => {
      setCurrentSession(response.session)
      setQuizAttempts(response.quizAttempts)
      setCanTakeQuiz(response.canTakeQuiz)
      setMaxAttempts(response.maxAttempts)
      setShowQuizForm(false)
      localStorage.setItem('quizSessionId', response.session.sessionId)
      localStorage.setItem('userName', userName)
    },
    onError: (error: Error) => {
      console.error('Session creation failed:', error)
      // Handle session creation errors
    }
  })

  const { data: questions = [], isLoading: questionsLoading } = useQuery({
    queryKey: ['questions'],
    queryFn: apiService.getQuestions,
    enabled: !!currentSession && !currentSession.hasCompletedQuiz
  })

  const submitQuizMutation = useMutation({
    mutationFn: apiService.submitQuiz,
    onSuccess: (_result) => {
      setShowResults(true)
      queryClient.invalidateQueries({ queryKey: ['session', currentSession?.sessionId] })
    },
    onError: (error: Error) => {
      console.error('Quiz submission failed:', error)
      // If it's a limit error, reset to show the form with error message
      if (error.message.includes('limit reached') || error.message.includes('attempts')) {
        setShowQuizForm(true)
        setCurrentSession(null)
        localStorage.removeItem('quizSessionId')
      }
    }
  })

  const handleStartQuiz = () => {
    if (userName.trim() && selectedTitle) {
      createSessionMutation.mutate({ name: userName.trim(), title: selectedTitle })
    }
  }

  const handleAnswerSelect = (questionId: string, answer: string) => {
    setAnswers(prev => ({ ...prev, [questionId]: answer }))
  }

  const handleNext = () => {
    if (currentQuestionIndex < questions.length - 1) {
      setCurrentQuestionIndex(prev => prev + 1)
    } else {
      // Submit quiz
      if (currentSession) {
        const submission: QuizSubmission = {
          sessionId: currentSession.sessionId,
          answers: questions.map(q => ({
            questionId: q.id,
            selectedAnswer: answers[q.id] || ''
          }))
        }
        submitQuizMutation.mutate(submission)
      }
    }
  }

  const handlePrevious = () => {
    if (currentQuestionIndex > 0) {
      setCurrentQuestionIndex(prev => prev - 1)
    }
  }

  const resetQuiz = () => {
    localStorage.removeItem('quizSessionId')
    setCurrentSession(null)
    setCurrentQuestionIndex(0)
    setAnswers({})
    setShowResults(false)
    setShowQuizForm(true)
    setHints({})
    setLoadingHints({})
  }

  const handleGetHint = async (questionId: string) => {
    if (hints[questionId] || loadingHints[questionId]) return

    setLoadingHints(prev => ({ ...prev, [questionId]: true }))
    
    try {
      const response = await fetch(`${API_BASE}/quiz/hint/${questionId}`)
      if (response.ok) {
        const data = await response.json()
        setHints(prev => ({ ...prev, [questionId]: data.hint }))
      } else {
        // Handle case where hint is not available (e.g., hard questions)
        setHints(prev => ({ ...prev, [questionId]: "Hints are not available for this question to maintain scoring integrity." }))
      }
    } catch (error) {
      console.error('Error fetching hint:', error)
      setHints(prev => ({ ...prev, [questionId]: "Sorry, couldn't load hint at this time." }))
    } finally {
      setLoadingHints(prev => ({ ...prev, [questionId]: false }))
    }
  }

  // Show quiz form if no session
  if (showQuizForm) {
    return (
      <div className="max-w-2xl mx-auto">
        <div className="bg-white/90 backdrop-blur-sm rounded-3xl shadow-2xl p-8 border border-white/20">
          <div className="text-center mb-8">
            <div className="text-6xl mb-4">🧠</div>
            <h2 className="text-4xl font-bold text-gray-800 mb-4">AI Quiz Challenge</h2>
            <p className="text-gray-600 text-lg">
              Test your knowledge of AI-First development with Cursor and Windsurf!
            </p>
            
            {/* Quiz Attempt Information */}
            {quizAttempts > 0 && (
              <div className={`mt-4 p-4 rounded-xl ${canTakeQuiz ? 'bg-blue-50 border border-blue-200' : 'bg-red-50 border border-red-200'}`}>
                <p className={`text-sm font-medium ${canTakeQuiz ? 'text-blue-800' : 'text-red-800'}`}>
                  📊 Quiz Attempts Today: {quizAttempts}/{maxAttempts}
                </p>
                {!canTakeQuiz && (
                  <p className="text-red-600 text-sm mt-2">
                    ⏰ You've reached your daily limit. Please try again tomorrow!
                  </p>
                )}
              </div>
            )}
          </div>

          {/* Show error if quiz submission failed */}
          {createSessionMutation.isError && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl">
              <p className="text-red-800 text-sm font-medium">
                ❌ Error: {createSessionMutation.error?.message || 'Failed to start quiz'}
              </p>
            </div>
          )}

          <div className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                👤 Your Name
              </label>
              <input
                type="text"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                placeholder="Enter your name..."
                className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                maxLength={50}
                disabled={!canTakeQuiz}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                💼 Your Role
              </label>
              <select
                value={selectedTitle}
                onChange={(e) => setSelectedTitle(e.target.value)}
                className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                disabled={!canTakeQuiz}
              >
                <option value="">Select your role...</option>
                {JOB_TITLES.map(title => (
                  <option key={title} value={title}>{title}</option>
                ))}
              </select>
            </div>

            <button
              onClick={handleStartQuiz}
              disabled={!userName.trim() || !selectedTitle || createSessionMutation.isPending || !canTakeQuiz}
              className={`w-full font-bold py-4 px-6 rounded-xl transform hover:scale-105 transition-all duration-200 shadow-lg ${
                canTakeQuiz 
                  ? 'bg-gradient-to-r from-purple-500 to-blue-500 text-white hover:from-purple-600 hover:to-blue-600 disabled:opacity-50 disabled:cursor-not-allowed'
                  : 'bg-gray-400 text-gray-600 cursor-not-allowed'
              }`}
            >
              {!canTakeQuiz 
                ? '🚫 Daily Limit Reached'
                : createSessionMutation.isPending 
                  ? '🔄 Starting...' 
                  : '🎯 Start Quiz!'
              }
            </button>
            
            {userName && selectedTitle && canTakeQuiz && (
              <p className="text-xs text-gray-500 text-center">
                💡 Auto-detected info? Feel free to change it!
              </p>
            )}
            
            {!canTakeQuiz && (
              <div className="text-center">
                <p className="text-sm text-gray-600 mb-4">
                  🎮 While you wait, try our other features:
                </p>
                <div className="flex flex-wrap justify-center gap-2">
                  <button 
                    onClick={() => window.location.href = '/game'}
                    className="px-4 py-2 bg-orange-100 text-orange-800 rounded-lg text-sm hover:bg-orange-200 transition-colors"
                  >
                    🚀 Spaceship Game
                  </button>
                  <button 
                    onClick={() => window.location.href = '/tips'}
                    className="px-4 py-2 bg-green-100 text-green-800 rounded-lg text-sm hover:bg-green-200 transition-colors"
                  >
                    💡 Tips & Tricks
                  </button>
                  <button 
                    onClick={() => window.location.href = '/analytics'}
                    className="px-4 py-2 bg-blue-100 text-blue-800 rounded-lg text-sm hover:bg-blue-200 transition-colors"
                  >
                    📊 Analytics
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    )
  }

  // Loading questions
  if (questionsLoading) {
    return (
      <div className="text-center">
        <div className="bg-white/90 backdrop-blur-sm rounded-3xl shadow-2xl p-12 border border-white/20">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-purple-500 mx-auto mb-4"></div>
          <h2 className="text-2xl font-bold text-gray-800">🔄 Loading Quiz Questions...</h2>
        </div>
      </div>
    )
  }

  // Quiz completed - show results
  if (showResults || currentSession?.hasCompletedQuiz) {
    const quizResultData = submitQuizMutation.data;

    return (
      <div className="max-w-3xl mx-auto">
        <div className="bg-white/90 backdrop-blur-sm rounded-3xl shadow-2xl p-8 border border-white/20">
          <div className="text-center">
            <div className="text-6xl mb-4">🎉</div>
            <h2 className="text-4xl font-bold text-gray-800 mb-2">Quiz Completed!</h2>
            <p className="text-gray-600 mb-6">
              Well done, <span className="font-bold text-purple-600">{currentSession?.name}</span>!
            </p>

            {quizResultData && (
              <div className="mb-8 text-left">
                <div className="bg-purple-50 p-6 rounded-xl mb-6 border border-purple-200">
                  <h3 className="text-2xl font-semibold text-purple-700 mb-4">Your Score:</h3>
                  <div className="grid grid-cols-3 gap-4 text-center">
                    <div>
                      <p className="text-3xl font-bold text-purple-600">{quizResultData.score}</p>
                      <p className="text-sm text-purple-500">Correct</p>
                    </div>
                    <div>
                      <p className="text-3xl font-bold text-gray-700">{quizResultData.totalQuestions}</p>
                      <p className="text-sm text-gray-500">Total</p>
                    </div>
                    <div>
                      <p className="text-3xl font-bold text-purple-600">{quizResultData.percentage.toFixed(0)}%</p>
                      <p className="text-sm text-purple-500">Percentage</p>
                    </div>
                  </div>
                </div>

                <h3 className="text-2xl font-semibold text-gray-800 mb-6">Detailed Results:</h3>
                <div className="space-y-6">
                  {quizResultData.results.map((result, index) => (
                    <div key={index} className={`p-6 rounded-xl border ${result.isCorrect ? 'border-green-300 bg-green-50' : 'border-red-300 bg-red-50'}`}>
                      <p className="font-semibold text-gray-700 mb-2">
                        <span className="mr-2">{index + 1}.</span>{result.questionText}
                      </p>
                      <p className={`text-sm mb-1 ${result.isCorrect ? 'text-green-700' : 'text-red-700'}`}>
                        Your answer: <span className="font-medium">{result.selectedAnswer || "Not answered"}</span>
                        {result.isCorrect ? <span className="ml-2">✅ Correct</span> : <span className="ml-2">❌ Incorrect</span>}
                      </p>
                      {!result.isCorrect && (
                        <p className="text-sm text-gray-600 mb-3">
                          Correct answer: <span className="font-medium text-green-700">{result.correctAnswer}</span>
                        </p>
                      )}
                      <div className="mt-3 pt-3 border-t border-gray-300/50">
                        <p className="text-xs text-gray-500 mb-1 font-medium">AI Explanation:</p>
                        <p className="text-xs text-gray-600 leading-relaxed">{result.explanation}</p>
                      </div>
                    </div>
                  ))}
                </div>

                {quizResultData.aiAnalysis && (
                  <div className="mt-8 p-6 bg-blue-50 border border-blue-200 rounded-xl">
                    <h4 className="text-xl font-semibold text-blue-700 mb-3">💡 AI Performance Analysis:</h4>
                    <p className="text-sm text-blue-600 leading-relaxedwhitespace-pre-line">{quizResultData.aiAnalysis}</p>
                  </div>
                )}

                {quizResultData.recommendations && quizResultData.recommendations.length > 0 && (
                  <div className="mt-8 p-6 bg-indigo-50 border border-indigo-200 rounded-xl">
                    <h4 className="text-xl font-semibold text-indigo-700 mb-3">📚 AI Recommendations:</h4>
                    <ul className="list-disc list-inside space-y-1 text-sm text-indigo-600">
                      {quizResultData.recommendations.map((rec, i) => (
                        <li key={i}>{rec}</li>
                      ))}
                    </ul>
                  </div>
                )}
              </div>
            )}
            
            <button
              onClick={resetQuiz}
              className="bg-gradient-to-r from-purple-500 to-blue-500 text-white font-bold py-3 px-8 rounded-xl hover:from-purple-600 hover:to-blue-600 transform hover:scale-105 transition-all duration-200 shadow-lg"
            >
              🔄 Take Quiz Again (New Session)
            </button>
          </div>
        </div>
      </div>
    )
  }

  // Quiz in progress
  const currentQuestion = questions[currentQuestionIndex]
  const progress = ((currentQuestionIndex + 1) / questions.length) * 100

  return (
    <div className="max-w-3xl mx-auto">
      {/* Progress Bar */}
      <div className="mb-6">
        <div className="w-full bg-gray-200 rounded-full h-3 mb-2">
          <div 
            className="bg-gradient-to-r from-purple-500 to-blue-500 h-3 rounded-full transition-all duration-300"
            style={{ width: `${progress}%` }}
          ></div>
        </div>
        <p className="text-sm text-gray-600 text-center">
          Question {currentQuestionIndex + 1} of {questions.length}
        </p>
      </div>

      {/* Show error if quiz submission failed */}
      {submitQuizMutation.isError && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl">
          <p className="text-red-800 text-sm font-medium">
            ❌ Quiz Submission Error: {submitQuizMutation.error?.message || 'Failed to submit quiz'}
          </p>
          <p className="text-red-600 text-xs mt-1">
            Please try again or start a new session if you've reached your daily limit.
          </p>
        </div>
      )}

      {/* Question Card */}
      <div className="bg-white/90 backdrop-blur-sm rounded-2xl shadow-xl p-8 mb-6 border border-white/20">
        <div className="mb-6">
          <div className="flex justify-between items-start mb-4">
            <span className="inline-block bg-purple-100 text-purple-800 text-xs font-semibold px-3 py-1 rounded-full">
              {currentQuestion?.category} • {currentQuestion?.difficulty}
            </span>
            <button
              onClick={() => handleGetHint(currentQuestion?.id || '')}
              disabled={loadingHints[currentQuestion?.id || ''] || !!hints[currentQuestion?.id || '']}
              className="bg-yellow-100 hover:bg-yellow-200 text-yellow-800 text-xs font-semibold px-3 py-1 rounded-full transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loadingHints[currentQuestion?.id || ''] ? '🔄 Loading...' : hints[currentQuestion?.id || ''] ? '💡 Hint Shown' : '💡 Get Hint'}
            </button>
          </div>
          <h3 className="text-xl font-bold text-gray-800 leading-relaxed">
            {currentQuestion?.text}
          </h3>
        </div>

        {/* Hint Display */}
        {hints[currentQuestion?.id || ''] && (
          <div className="mb-6 p-4 bg-yellow-50 border border-yellow-200 rounded-xl">
            <div className="flex items-start gap-2">
              <span className="text-yellow-600 text-lg">💡</span>
              <div>
                <p className="text-yellow-800 font-medium text-sm mb-1">AI Hint:</p>
                <p className="text-yellow-700 text-sm">{hints[currentQuestion?.id || '']}</p>
              </div>
            </div>
          </div>
        )}

        <div className="space-y-3">
          {currentQuestion?.options.map((option, index) => (
            <button
              key={index}
              onClick={() => handleAnswerSelect(currentQuestion.id, option)}
              className={`w-full p-4 text-left rounded-xl border-2 transition-all duration-200 ${
                answers[currentQuestion.id] === option
                  ? 'border-purple-500 bg-purple-50 text-purple-800'
                  : 'border-gray-200 hover:border-purple-300 hover:bg-purple-25'
              }`}
            >
              <span className="font-medium">{String.fromCharCode(65 + index)}.</span> {option}
            </button>
          ))}
        </div>
      </div>

      {/* Navigation */}
      <div className="flex justify-between items-center">
        <button
          onClick={handlePrevious}
          disabled={currentQuestionIndex === 0}
          className="bg-gray-500 text-white font-bold py-3 px-6 rounded-xl hover:bg-gray-600 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
        >
          ← Previous
        </button>

        <span className="text-gray-600 font-medium">
          {currentQuestionIndex + 1} / {questions.length}
        </span>

        <button
          onClick={handleNext}
          disabled={!answers[currentQuestion?.id] || submitQuizMutation.isPending}
          className="bg-gradient-to-r from-purple-500 to-blue-500 text-white font-bold py-3 px-6 rounded-xl hover:from-purple-600 hover:to-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
        >
          {currentQuestionIndex === questions.length - 1 
            ? (submitQuizMutation.isPending ? '🔄 Submitting...' : '🎯 Submit Quiz')
            : 'Next →'
          }
        </button>
      </div>
    </div>
  )
}

// Main App Component
function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<QuizPage />} />
        <Route path="/quiz" element={<QuizPage />} />
        <Route path="/game" element={<SpaceshipGame />} />
        <Route path="/tips" element={<TipsAndTricks />} />
        <Route path="/analytics" element={<Analytics />} />
      </Routes>
    </Layout>
  )
}

export default App 