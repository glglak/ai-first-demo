import { ColumnDef } from '@tanstack/react-table'

// Quiz Participant Interface
export interface QuizParticipant {
  name: string
  ipHash: string
  score: number
  lastActive: string
}

// Game Participant Interface  
export interface GameParticipant {
  name: string
  ipHash: string
  score: number
  lastActive: string
}

// Tips Contributor Interface
export interface TipsContributor {
  name: string
  activity: string
  score: number
  lastActive: string
}

// Quiz Analytics Columns
export const quizColumns: ColumnDef<QuizParticipant>[] = [
  {
    id: 'rank',
    header: 'Rank',
    cell: ({ row }) => (
      <div className="flex items-center">
        <span className="font-bold text-purple-600">#{row.index + 1}</span>
      </div>
    ),
    enableSorting: false,
  },
  {
    accessorKey: 'name',
    header: 'Participant Name',
    cell: ({ row }) => (
      <div>
        <div className="font-medium text-gray-900">{row.getValue('name')}</div>
        <div className="text-xs text-gray-500">
          ID: {(row.original.ipHash || '').substring(0, 8)}...
        </div>
      </div>
    ),
  },
  {
    accessorKey: 'score',
    header: 'Score',
    cell: ({ row }) => {
      const score = row.getValue('score') as number;
      const percentage = (score / 10) * 100; // Assuming 10 total questions
      return (
        <div className="text-right">
          <div className="text-lg font-bold text-purple-600">
            {percentage.toFixed(0)}%
          </div>
          <div className="text-xs text-gray-500">quiz score</div>
        </div>
      );
    },
    sortingFn: 'basic',
  },
  {
    accessorKey: 'lastActive',
    header: 'Last Active',
    cell: ({ row }) => (
      <div className="text-sm text-gray-600">
        {new Date(row.getValue('lastActive')).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        })}
      </div>
    ),
    sortingFn: (rowA, rowB) => {
      const dateA = new Date(rowA.getValue('lastActive')).getTime()
      const dateB = new Date(rowB.getValue('lastActive')).getTime()
      return dateA - dateB
    },
  },
]

// Game Analytics Columns
export const gameColumns: ColumnDef<GameParticipant>[] = [
  {
    id: 'rank',
    header: 'Rank',
    cell: ({ row }) => (
      <div className="flex items-center">
        {row.index === 0 && <span className="text-yellow-500 mr-1">🏆</span>}
        {row.index === 1 && <span className="text-gray-400 mr-1">🥈</span>}
        {row.index === 2 && <span className="text-amber-600 mr-1">🥉</span>}
        <span className="font-bold text-blue-600">#{row.index + 1}</span>
      </div>
    ),
    enableSorting: false,
  },
  {
    accessorKey: 'name',
    header: 'Player Name',
    cell: ({ row }) => (
      <div>
        <div className="font-medium text-gray-900">{row.getValue('name')}</div>
        <div className="text-xs text-gray-500">
          ID: {(row.original.ipHash || '').substring(0, 8)}...
        </div>
      </div>
    ),
  },
  {
    accessorKey: 'score',
    header: 'High Score',
    cell: ({ row }) => (
      <div className="text-right">
        <div className="text-lg font-bold text-blue-600">
          {(row.getValue('score') as number)?.toLocaleString() || '0'}
        </div>
        <div className="text-xs text-gray-500">points</div>
      </div>
    ),
    sortingFn: 'basic',
  },
  {
    accessorKey: 'lastActive',
    header: 'Last Played',
    cell: ({ row }) => (
      <div className="text-sm text-gray-600">
        {new Date(row.getValue('lastActive')).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        })}
      </div>
    ),
    sortingFn: (rowA, rowB) => {
      const dateA = new Date(rowA.getValue('lastActive')).getTime()
      const dateB = new Date(rowB.getValue('lastActive')).getTime()
      return dateA - dateB
    },
  },
]

// Tips Analytics Columns
export const tipsColumns: ColumnDef<TipsContributor>[] = [
  {
    id: 'rank',
    header: 'Rank',
    cell: ({ row }) => (
      <div className="flex items-center">
        <span className="font-bold text-green-600">#{row.index + 1}</span>
      </div>
    ),
    enableSorting: false,
  },
  {
    accessorKey: 'name',
    header: 'Contributor',
    cell: ({ row }) => (
      <div>
        <div className="font-medium text-gray-900">{row.getValue('name')}</div>
        <div className="text-xs text-gray-500 truncate max-w-40">
          {row.original.activity}
        </div>
      </div>
    ),
  },
  {
    accessorKey: 'score',
    header: 'Likes',
    cell: ({ row }) => (
      <div className="text-right">
        <div className="text-lg font-bold text-green-600">
          ❤️ {row.getValue('score')}
        </div>
        <div className="text-xs text-gray-500">likes</div>
      </div>
    ),
    sortingFn: 'basic',
  },
  {
    accessorKey: 'lastActive',
    header: 'Last Active',
    cell: ({ row }) => (
      <div className="text-sm text-gray-600">
        {new Date(row.getValue('lastActive')).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        })}
      </div>
    ),
    sortingFn: (rowA, rowB) => {
      const dateA = new Date(rowA.getValue('lastActive')).getTime()
      const dateB = new Date(rowB.getValue('lastActive')).getTime()
      return dateA - dateB
    },
  },
] 