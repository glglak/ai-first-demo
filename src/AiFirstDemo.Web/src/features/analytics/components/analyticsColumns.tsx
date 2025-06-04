import { ColumnDef } from '@tanstack/react-table'
import { UnifiedParticipant } from '../../../shared/types'

// Quiz Analytics Columns - with pagination offset for correct ranking
export const createQuizColumns = (offset: number = 0): ColumnDef<UnifiedParticipant>[] => [
  {
    id: 'rank',
    header: 'Rank',
    cell: ({ row }) => {
      const globalRank = row.index + 1 + offset;
      return (
        <div className="flex items-center">
          {globalRank === 1 && <span className="text-yellow-500 mr-1">🏆</span>}
          {globalRank === 2 && <span className="text-gray-400 mr-1">🥈</span>}
          {globalRank === 3 && <span className="text-amber-600 mr-1">🥉</span>}
          <span className="font-bold text-purple-600">#{globalRank}</span>
        </div>
      );
    },
    enableSorting: false,
  },
  {
    accessorKey: 'name',
    header: 'AI Genius',
    cell: ({ row }) => (
      <div className="font-medium text-gray-900">{row.getValue('name')}</div>
    ),
    sortingFn: 'alphanumeric',
  },
  {
    accessorKey: 'score',
    header: 'Intelligence Score',
    cell: ({ row }) => {
      const score = row.getValue('score') as number;
      const percentage = (score / 10) * 100; // Assuming 10 total questions
      return (
        <div className="text-right">
          <div className="text-lg font-bold text-purple-600">
            {score}/10 ({percentage.toFixed(0)}%)
          </div>
          <div className="text-xs text-gray-500">brain power</div>
        </div>
      );
    },
    sortingFn: 'basic',
  },
  {
    accessorKey: 'activity',
    header: 'Achievement',
    cell: ({ row }) => (
      <div className="text-sm text-gray-600">
        {row.getValue('activity')}
      </div>
    ),
    sortingFn: 'alphanumeric',
  },
  {
    accessorKey: 'lastActive',
    header: 'Last Seen',
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

// Game Analytics Columns - with pagination offset for correct ranking
export const createGameColumns = (offset: number = 0): ColumnDef<UnifiedParticipant>[] => [
  {
    id: 'rank',
    header: 'Rank',
    cell: ({ row }) => {
      const globalRank = row.index + 1 + offset;
      return (
        <div className="flex items-center">
          {globalRank === 1 && <span className="text-yellow-500 mr-1">🏆</span>}
          {globalRank === 2 && <span className="text-gray-400 mr-1">🥈</span>}
          {globalRank === 3 && <span className="text-amber-600 mr-1">🥉</span>}
          <span className="font-bold text-blue-600">#{globalRank}</span>
        </div>
      );
    },
    enableSorting: false,
  },
  {
    accessorKey: 'name',
    header: 'AI Warrior',
    cell: ({ row }) => (
      <div className="font-medium text-gray-900">{row.getValue('name')}</div>
    ),
    sortingFn: 'alphanumeric',
  },
  {
    accessorKey: 'score',
    header: 'Power Level',
    cell: ({ row }) => (
      <div className="text-right">
        <div className="text-lg font-bold text-blue-600">
          {(row.getValue('score') as number)?.toLocaleString() || '0'}
        </div>
        <div className="text-xs text-gray-500">combat points</div>
      </div>
    ),
    sortingFn: 'basic',
  },
  {
    accessorKey: 'activity',
    header: 'Battle Record',
    cell: ({ row }) => (
      <div className="text-sm text-gray-600">
        {row.getValue('activity')}
      </div>
    ),
    sortingFn: 'alphanumeric',
  },
  {
    accessorKey: 'lastActive',
    header: 'Last Battle',
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

// Tips Analytics Columns - with pagination offset for correct ranking
export const createTipsColumns = (offset: number = 0): ColumnDef<UnifiedParticipant>[] => [
  {
    id: 'rank',
    header: 'Rank',
    cell: ({ row }) => {
      const globalRank = row.index + 1 + offset;
      return (
        <div className="flex items-center">
          {globalRank === 1 && <span className="text-yellow-500 mr-1">🏆</span>}
          {globalRank === 2 && <span className="text-gray-400 mr-1">🥈</span>}
          {globalRank === 3 && <span className="text-amber-600 mr-1">🥉</span>}
          <span className="font-bold text-green-600">#{globalRank}</span>
        </div>
      );
    },
    enableSorting: false,
  },
  {
    accessorKey: 'name',
    header: 'AI Mentor',
    cell: ({ row }) => (
      <div>
        <div className="font-medium text-gray-900">{row.getValue('name')}</div>
        <div className="text-xs text-gray-500">
          {row.original.ipHash || 'Unknown Source'}
        </div>
      </div>
    ),
  },
  {
    accessorKey: 'activity',
    header: 'Wisdom Shared',
    cell: ({ row }) => (
      <div className="text-sm text-gray-600 truncate max-w-40">
        {row.getValue('activity')}
      </div>
    ),
    sortingFn: 'alphanumeric',
  },
  {
    accessorKey: 'score',
    header: 'Respect Earned',
    cell: ({ row }) => (
      <div className="text-right">
        <div className="text-lg font-bold text-green-600">
          {row.getValue('score')} ❤️
        </div>
        <div className="text-xs text-gray-500">community love</div>
      </div>
    ),
    sortingFn: 'basic',
  },
  {
    accessorKey: 'lastActive',
    header: 'Last Wisdom',
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

// Legacy exports for backward compatibility
export const quizColumns = createQuizColumns(0);
export const gameColumns = createGameColumns(0);
export const tipsColumns = createTipsColumns(0); 