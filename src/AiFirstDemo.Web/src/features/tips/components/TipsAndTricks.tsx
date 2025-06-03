import React, { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useSession } from '../../../shared/contexts/SessionContext'
import { tipsApi, sessionApi } from '../../../shared/services/api'
import { Tip, TipsCollectionResponse } from '../../../shared/types'

// Predefined comprehensive tips for Cursor and Windsurf
const PREDEFINED_TIPS: Tip[] = [
  // Cursor Basics
  {
    id: 'cursor-1',
    title: 'Use @ to Reference Files',
    content: 'Type @ followed by a filename to give AI context about specific files. Example: "@App.tsx can you add a new component?" This helps AI understand your codebase structure.',
    category: 'cursor-basics',
    tags: ['context', 'files', 'reference'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-2',
    title: 'Cmd+K for Inline Editing',
    content: 'Use Cmd+K (Ctrl+K on Windows) to edit code inline. Select the code you want to modify, press Cmd+K, and describe what you want to change. Perfect for quick refactoring.',
    category: 'cursor-basics',
    tags: ['shortcuts', 'editing', 'refactoring'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-3',
    title: 'Cmd+L for Chat Mode',
    content: 'Press Cmd+L (Ctrl+L on Windows) to open the AI chat sidebar. This is perfect for asking questions, getting explanations, or planning your next steps without editing code directly.',
    category: 'cursor-basics',
    tags: ['chat', 'shortcuts', 'planning'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-4',
    title: 'Tab to Accept AI Suggestions',
    content: 'When AI shows inline suggestions (ghost text), press Tab to accept them. Press Escape to dismiss. You can also use Cmd+→ to accept word by word.',
    category: 'cursor-basics',
    tags: ['autocomplete', 'suggestions', 'shortcuts'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-5',
    title: 'Use .cursorrules for Project Context',
    content: 'Create a .cursorrules file in your project root to give AI persistent context about your coding standards, architecture, and preferences. This improves all AI suggestions.',
    category: 'cursor-basics',
    tags: ['configuration', 'context', 'standards'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },

  // Advanced Cursor
  {
    id: 'cursor-adv-1',
    title: 'Multi-File Editing with @',
    content: 'Reference multiple files in one request: "@components/Header.tsx @styles/globals.css update the header to use the new color scheme". AI will edit multiple files simultaneously.',
    category: 'cursor-advanced',
    tags: ['multi-file', 'context', 'batch-editing'],
    difficulty: 'advanced',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-adv-2',
    title: 'Use Composer for Complex Features',
    content: 'Open Composer (Cmd+I) for building entire features. It can create multiple files, update existing ones, and handle complex architectural changes across your codebase.',
    category: 'cursor-advanced',
    tags: ['composer', 'features', 'architecture'],
    difficulty: 'advanced',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-adv-3',
    title: 'Terminal Integration',
    content: 'Use Cmd+Shift+P → "Cursor: Ask AI about terminal output" to get AI help with error messages, build failures, or command explanations directly from your terminal.',
    category: 'cursor-advanced',
    tags: ['terminal', 'debugging', 'errors'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-adv-4',
    title: 'Code Review with AI',
    content: 'Select code and ask "Review this code for potential issues" or "Suggest improvements for performance". AI can spot bugs, security issues, and optimization opportunities.',
    category: 'cursor-advanced',
    tags: ['code-review', 'quality', 'security'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'cursor-adv-5',
    title: 'Custom Instructions',
    content: 'In Settings → Features → AI, add custom instructions like "Always use TypeScript strict mode" or "Follow React functional component patterns". AI will remember these preferences.',
    category: 'cursor-advanced',
    tags: ['settings', 'preferences', 'customization'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },

  // AI-First Development Principles
  {
    id: 'ai-first-1',
    title: 'Vertical Slice Architecture with AI',
    content: 'When planning features, think in vertical slices. Ask AI to help implement a full slice from UI to database for faster, more integrated development. Example: "Implement the user registration feature including React form, API endpoint, and database interaction using .NET 8 and Redis."',
    category: 'ai-first-dev',
    tags: ['architecture', 'planning', 'dotnet', 'react'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'ai-first-2',
    title: 'Iterative Prompting for Complex Tasks',
    content: 'For complex tasks, break them down. Prompt AI iteratively. Start broad, then refine with more specific instructions. Example: 1. "Draft a React component for a user profile page." 2. "Now add an edit mode to this profile component." 3. "Integrate it with the user update API."',
    category: 'ai-first-dev',
    tags: ['prompting', 'iteration', 'react'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'ai-first-3',
    title: 'Leverage AI for API Design (.NET)',
    content: 'Use AI to help design and scaffold your .NET API endpoints. Example: "Design a RESTful API endpoint in .NET 8 for managing user preferences, including GET, POST, and PUT methods. Store preferences in Redis."',
    category: 'ai-first-dev',
    tags: ['api', 'dotnet', 'redis', 'design'],
    difficulty: 'advanced',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },

  // .NET 8 & React Development
  {
    id: 'dotnet-react-1',
    title: 'Efficient State Management in React with AI',
    content: 'Struggling with React state? Ask AI for suggestions! Example: "Refactor this React component to use Zustand for state management instead of useState for these complex fields..." or "Explain when to use useMemo and useCallback here."',
    category: 'dotnet-react',
    tags: ['react', 'state-management', 'performance'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'dotnet-react-2',
    title: 'Optimizing .NET API Performance with Redis',
    content: 'Use AI to identify caching opportunities in your .NET API. Example: "Review this .NET service method. Can I use Redis caching here to improve performance for frequently accessed data? Show me how."',
    category: 'dotnet-react',
    tags: ['dotnet', 'performance', 'caching', 'redis'],
    difficulty: 'advanced',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },

  // Best Practices
  {
    id: 'best-1',
    title: 'Be Specific in Requests',
    content: 'Instead of "fix this", say "fix the TypeScript error on line 23 by adding proper type annotations". Specific requests get better results and save time.',
    category: 'best-practices',
    tags: ['communication', 'specificity', 'efficiency'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'best-2',
    title: 'Provide Context',
    content: 'Always provide context about what you\'re building, the tech stack, and any constraints. "I\'m building a React e-commerce app with TypeScript and need a product card component" is much better than "create a component".',
    category: 'best-practices',
    tags: ['context', 'communication', 'clarity'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'best-3',
    title: 'Review AI Suggestions',
    content: 'Always review AI-generated code before accepting. Check for security issues, performance problems, and alignment with your project standards. AI is powerful but not infallible.',
    category: 'best-practices',
    tags: ['review', 'security', 'quality'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'best-4',
    title: 'Iterative Development',
    content: 'Break large features into smaller chunks. Ask AI to implement one piece at a time, test it, then move to the next. This leads to better code quality and easier debugging.',
    category: 'best-practices',
    tags: ['iteration', 'development', 'testing'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'best-5',
    title: 'Learn from AI Explanations',
    content: 'Don\'t just copy AI code - ask "explain how this works" or "why did you choose this approach?". This helps you learn and make better decisions in the future.',
    category: 'best-practices',
    tags: ['learning', 'understanding', 'growth'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },

  // Productivity Tips
  {
    id: 'productivity-1',
    title: 'Use AI for Documentation',
    content: 'Ask AI to "generate JSDoc comments for this function" or "create a README for this component". AI excels at creating clear, comprehensive documentation.',
    category: 'productivity',
    tags: ['documentation', 'comments', 'readme'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'productivity-2',
    title: 'Automated Testing',
    content: 'Generate unit tests with "create tests for this component" or "add test cases for edge cases". AI can create comprehensive test suites quickly.',
    category: 'productivity',
    tags: ['testing', 'automation', 'quality'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'productivity-3',
    title: 'Code Refactoring',
    content: 'Use AI for refactoring: "extract this logic into a custom hook", "convert this class component to functional", or "optimize this function for performance".',
    category: 'productivity',
    tags: ['refactoring', 'optimization', 'modernization'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'productivity-4',
    title: 'Quick Prototyping',
    content: 'AI is perfect for rapid prototyping. Describe your idea and let AI create a working prototype that you can then refine and improve.',
    category: 'productivity',
    tags: ['prototyping', 'rapid-development', 'mvp'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },

  // Troubleshooting
  {
    id: 'troubleshooting-1',
    title: 'Debug with AI',
    content: 'Copy error messages and ask "what does this error mean and how do I fix it?". AI can explain complex error messages and provide step-by-step solutions.',
    category: 'troubleshooting',
    tags: ['debugging', 'errors', 'solutions'],
    difficulty: 'beginner',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'troubleshooting-2',
    title: 'Performance Analysis',
    content: 'Ask AI to "analyze this code for performance bottlenecks" or "suggest optimizations for this React component". AI can spot inefficient patterns and suggest improvements.',
    category: 'troubleshooting',
    tags: ['performance', 'optimization', 'analysis'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  },
  {
    id: 'troubleshooting-3',
    title: 'Dependency Issues',
    content: 'When facing package conflicts or version issues, ask AI to "help resolve this dependency conflict" with your package.json. AI understands npm/yarn ecosystems well.',
    category: 'troubleshooting',
    tags: ['dependencies', 'packages', 'conflicts'],
    difficulty: 'intermediate',
    createdAt: new Date().toISOString(),
    createdBy: 'system',
    likes: 0,
    isLiked: false,
    isAiGenerated: false
  }
]

const CATEGORIES = [
  { id: 'all', name: 'All Tips', icon: '📚' },
  { id: 'cursor-basics', name: 'Cursor Basics', icon: '🎯' },
  { id: 'cursor-advanced', name: 'Cursor Advanced', icon: '🚀' },
  { id: 'ai-first-dev', name: 'AI-First Development', icon: '💡' },
  { id: 'dotnet-react', name: '.NET & React', icon: '💻' },
  { id: 'best-practices', name: 'Best Practices', icon: '✨' },
  { id: 'productivity', name: 'Productivity', icon: '⚡' },
  { id: 'troubleshooting', name: 'Troubleshooting', icon: '🔧' }
]

const DIFFICULTY_COLORS: Record<string, string> = {
  beginner: 'bg-green-100 text-green-800',
  intermediate: 'bg-yellow-100 text-yellow-800',
  advanced: 'bg-red-100 text-red-800'
}

const TipsAndTricks: React.FC = () => {
  const [selectedCategory, setSelectedCategory] = useState('all')
  const [searchQuery, setSearchQuery] = useState('')
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [showNameForm, setShowNameForm] = useState(false)
  const [userName, setUserName] = useState('')
  const [newTip, setNewTip] = useState({
    title: '',
    content: '',
    category: 'best-practices',
    difficulty: 'beginner',
    tags: ''
  })
  const { session, setSession } = useSession()
  const queryClient = useQueryClient()

  // API queries - fetch tips from backend with session context
  const { data: tipsResponse, isLoading } = useQuery({
    queryKey: ['tips', selectedCategory, session?.sessionId],
    queryFn: () => tipsApi.getTips(
      selectedCategory === 'all' ? undefined : selectedCategory,
      session?.sessionId
    ),
    staleTime: 30000, // 30 seconds
  })

  // Use API tips or fallback to empty array
  const allTips = tipsResponse?.tips || []

  // Filter tips based on search
  const filteredTips = allTips.filter(tip => {
    const matchesSearch = searchQuery === '' || 
      tip.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      tip.content.toLowerCase().includes(searchQuery.toLowerCase()) ||
      tip.tags.some(tag => tag.toLowerCase().includes(searchQuery.toLowerCase()))
    
    return matchesSearch
  })

  // Session creation mutation
  const createSessionMutation = useMutation({
    mutationFn: (name: string) => sessionApi.create({ name }),
    onSuccess: (newSession) => {
      setSession(newSession)
      setShowNameForm(false)
      setShowCreateForm(true)
      setUserName('')
    },
    onError: (error) => {
      console.error('Failed to create session:', error)
      alert('Failed to create session. Please try again.')
    }
  })

  const createTipMutation = useMutation({
    mutationFn: (tip: any) => tipsApi.createTip(tip, session?.sessionId || ''),
    onSuccess: () => {
      // Refetch tips to get updated list
      queryClient.invalidateQueries({ queryKey: ['tips'] })
      setShowCreateForm(false)
      setNewTip({ title: '', content: '', category: 'best-practices', difficulty: 'beginner', tags: '' })
    }
  })

  const likeTipMutation = useMutation({
    mutationFn: (tipId: string) => tipsApi.likeTip(tipId, session?.sessionId || ''),
    onSuccess: () => {
      // Refetch tips to get updated like status
      queryClient.invalidateQueries({ queryKey: ['tips'] })
    },
    onError: (error) => {
      console.error('Failed to like tip:', error)
      // Don't show alert for already liked tips (400 error)
      if (!error.message.includes('400')) {
        alert('Failed to like tip. Please try again.')
      }
    }
  })

  const handleAddTipClick = () => {
    if (!session) {
      setShowNameForm(true)
    } else {
      setShowCreateForm(true)
    }
  }

  const handleCreateSession = () => {
    if (!userName.trim()) return
    createSessionMutation.mutate(userName.trim())
  }

  const handleCreateTip = () => {
    if (!newTip.title.trim() || !newTip.content.trim()) return

    const tip = {
      ...newTip,
      tags: newTip.tags.split(',').map(tag => tag.trim()).filter(Boolean)
    }

    createTipMutation.mutate(tip)
  }

  const handleLikeTip = (tipId: string) => {
    if (!session) {
      alert('Please create a session first to like tips!')
      return
    }
    
    const tip = allTips.find(t => t.id === tipId)
    if (tip && !tip.isLikedByUser) {
      likeTipMutation.mutate(tipId)
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="text-center">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">💡 Tips & Tricks</h1>
        <p className="text-gray-700">
          Master Cursor and Windsurf with these expert tips and community wisdom
        </p>
        {session && (
          <p className="text-sm text-purple-600 mt-2">
            Welcome back, {session.name}! You can now add tips and like content.
          </p>
        )}
      </div>

      {/* Search and Add */}
      <div className="card">
        <div className="flex flex-col lg:flex-row gap-4 mb-6">
          <div className="flex-1">
            <input
              type="text"
              placeholder="Search tips..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500"
            />
          </div>
          <button
            onClick={handleAddTipClick}
            className="btn btn-primary whitespace-nowrap"
          >
            ➕ Add Tip
          </button>
        </div>

        {/* Category Filter */}
        <div className="flex flex-wrap gap-2">
          {CATEGORIES.map(category => (
            <button
              key={category.id}
              onClick={() => setSelectedCategory(category.id)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                selectedCategory === category.id
                  ? 'bg-purple-500 text-white'
                  : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
              }`}
            >
              {category.icon} {category.name}
            </button>
          ))}
        </div>
      </div>

      {/* Tips Grid */}
      {isLoading ? (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="card animate-pulse">
              <div className="h-4 bg-gray-200 rounded mb-3"></div>
              <div className="h-3 bg-gray-200 rounded mb-2"></div>
              <div className="h-3 bg-gray-200 rounded mb-4"></div>
              <div className="flex justify-between items-center">
                <div className="h-3 bg-gray-200 rounded w-16"></div>
                <div className="h-3 bg-gray-200 rounded w-12"></div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredTips.map(tip => (
            <div key={tip.id} className="card hover:shadow-xl transition-all">
              <div className="flex justify-between items-start mb-3">
                <h3 className="text-lg font-semibold text-gray-900">{tip.title}</h3>
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${DIFFICULTY_COLORS[tip.difficulty]}`}>
                  {tip.difficulty}
                </span>
              </div>
              
              <p className="text-gray-700 text-sm mb-4 leading-relaxed">
                {tip.content}
              </p>
              
              <div className="flex flex-wrap gap-1 mb-4">
                {tip.tags.map(tag => (
                  <span key={tag} className="px-2 py-1 bg-blue-100 text-blue-700 text-xs rounded-full">
                    #{tag}
                  </span>
                ))}
              </div>
              
              <div className="flex justify-between items-center">
                <span className="text-xs text-gray-500">
                  by {tip.createdBy}
                </span>
                <button
                  onClick={() => handleLikeTip(tip.id)}
                  disabled={tip.isLikedByUser || likeTipMutation.isPending}
                  className={`flex items-center space-x-1 text-sm ${
                    tip.isLikedByUser 
                      ? 'text-red-500 cursor-not-allowed' 
                      : 'text-gray-500 hover:text-red-500 cursor-pointer'
                  } ${likeTipMutation.isPending ? 'opacity-50' : ''}`}
                >
                  <span>{tip.isLikedByUser ? '❤️' : '🤍'}</span>
                  <span>{tip.likes}</span>
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {!isLoading && filteredTips.length === 0 && (
        <div className="text-center py-12">
          <div className="text-6xl mb-4">🔍</div>
          <h3 className="text-xl font-semibold text-gray-900 mb-2">No tips found</h3>
          <p className="text-gray-600">
            Try adjusting your search or category filter, or create a new tip!
          </p>
        </div>
      )}

      {/* Create Tip Modal */}
      {showCreateForm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-xl font-bold mb-4">Share Your Tip</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
                <input
                  type="text"
                  value={newTip.title}
                  onChange={(e) => setNewTip(prev => ({ ...prev, title: e.target.value }))}
                  placeholder="Enter tip title..."
                  className="w-full p-3 border border-gray-300 rounded-lg"
                  maxLength={100}
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Content</label>
                <textarea
                  value={newTip.content}
                  onChange={(e) => setNewTip(prev => ({ ...prev, content: e.target.value }))}
                  placeholder="Share your tip or trick..."
                  className="w-full p-3 border border-gray-300 rounded-lg h-32 resize-none"
                  maxLength={500}
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
                <select
                  value={newTip.category}
                  onChange={(e) => setNewTip(prev => ({ ...prev, category: e.target.value }))}
                  className="w-full p-3 border border-gray-300 rounded-lg"
                >
                  {CATEGORIES.slice(1).map(category => (
                    <option key={category.id} value={category.id}>
                      {category.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Difficulty</label>
                <select
                  value={newTip.difficulty}
                  onChange={(e) => setNewTip(prev => ({ ...prev, difficulty: e.target.value }))}
                  className="w-full p-3 border border-gray-300 rounded-lg"
                >
                  <option value="beginner">🟢 Beginner</option>
                  <option value="intermediate">🟡 Intermediate</option>
                  <option value="advanced">🔴 Advanced</option>
                </select>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Tags (comma-separated)</label>
                <input
                  type="text"
                  value={newTip.tags}
                  onChange={(e) => setNewTip(prev => ({ ...prev, tags: e.target.value }))}
                  placeholder="shortcuts, productivity, debugging"
                  className="w-full p-3 border border-gray-300 rounded-lg"
                />
              </div>
            </div>
            
            <div className="flex space-x-3 mt-6">
              <button
                onClick={handleCreateTip}
                disabled={!newTip.title.trim() || !newTip.content.trim() || createTipMutation.isPending}
                className="flex-1 btn btn-primary disabled:opacity-50"
              >
                {createTipMutation.isPending ? 'Creating...' : 'Create Tip'}
              </button>
              <button
                onClick={() => setShowCreateForm(false)}
                className="flex-1 btn btn-secondary"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Name Input Modal */}
      {showNameForm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-md w-full">
            <h3 className="text-xl font-bold mb-4">👋 Welcome to Tips & Tricks!</h3>
            <p className="text-gray-600 mb-4">
              To add tips and like content, please tell us your name. This helps us track contributions and build a community.
            </p>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Your Name</label>
                <input
                  type="text"
                  value={userName}
                  onChange={(e) => setUserName(e.target.value)}
                  placeholder="Enter your name..."
                  className="w-full p-3 border border-gray-300 rounded-lg text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500"
                  maxLength={50}
                  onKeyPress={(e) => e.key === 'Enter' && handleCreateSession()}
                />
              </div>
            </div>
            
            <div className="flex space-x-3 mt-6">
              <button
                onClick={handleCreateSession}
                disabled={!userName.trim() || createSessionMutation.isPending}
                className="flex-1 btn btn-primary disabled:opacity-50"
              >
                {createSessionMutation.isPending ? 'Creating Session...' : 'Continue'}
              </button>
              <button
                onClick={() => {
                  setShowNameForm(false)
                  setUserName('')
                }}
                className="flex-1 btn btn-secondary"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Quick Start Guide */}
      <div className="bg-gradient-to-r from-purple-500 to-blue-500 rounded-xl p-6">
        <h3 className="text-2xl font-bold text-white mb-4">🚀 Quick Start Guide</h3>
        <div className="grid md:grid-cols-2 gap-6 text-white">
          <div>
            <h4 className="font-semibold mb-2">New to Cursor?</h4>
            <ul className="space-y-1 text-sm opacity-90">
              <li>• Start with @ to reference files</li>
              <li>• Use Cmd+K for inline editing</li>
              <li>• Try Cmd+L for chat mode</li>
              <li>• Create a .cursorrules file</li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-2">New to Windsurf?</h4>
            <ul className="space-y-1 text-sm opacity-90">
              <li>• Try Cascade for autonomous development</li>
              <li>• Use Flow for collaborative editing</li>
              <li>• Ask for project analysis</li>
              <li>• Request task planning</li>
            </ul>
          </div>
        </div>
      </div>

      {/* Statistics */}
      <div className="card">
        <h3 className="text-xl font-bold text-gray-900 mb-4">📊 Tip Statistics</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="text-center">
            <div className="text-2xl font-bold text-purple-600">{allTips.length}</div>
            <div className="text-sm text-gray-600">Total Tips</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-blue-600">{CATEGORIES.length - 1}</div>
            <div className="text-sm text-gray-600">Categories</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-green-600">
              {allTips.filter(tip => tip.createdBy !== 'system').length}
            </div>
            <div className="text-sm text-gray-600">Community Tips</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold text-orange-600">
              {allTips.reduce((sum, tip) => sum + tip.likes, 0)}
            </div>
            <div className="text-sm text-gray-600">Total Likes</div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default TipsAndTricks 