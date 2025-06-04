import { useState } from 'react'
import {
  useReactTable,
  getCoreRowModel,
  getFilteredRowModel,
  getSortedRowModel,
  getPaginationRowModel,
  ColumnDef,
  SortingState,
  ColumnFiltersState,
  flexRender,
} from '@tanstack/react-table'

interface PaginationInfo {
  pageIndex: number
  pageSize: number
  pageCount: number
  total: number
  hasNext: boolean
  hasPrevious: boolean
  onPageChange: (pageIndex: number) => void
  onPageSizeChange: (pageSize: number) => void
}

interface DataTableProps<T> {
  data: T[]
  columns: ColumnDef<T>[]
  title: string
  subtitle?: string
  isLoading?: boolean
  error?: string | null
  searchPlaceholder?: string
  emptyStateIcon?: string
  emptyStateMessage?: string
  emptyStateSubMessage?: string
  colorTheme?: 'purple' | 'blue' | 'green'
  pagination?: PaginationInfo // Optional external pagination
  loadingQuote?: string // Optional quote to show during loading
}

export function DataTable<T>({
  data,
  columns,
  title,
  subtitle,
  isLoading = false,
  error = null,
  searchPlaceholder = "Search...",
  emptyStateIcon = "📊",
  emptyStateMessage = "No data available",
  emptyStateSubMessage = "Data will appear here when available",
  colorTheme = 'blue',
  pagination,
  loadingQuote
}: DataTableProps<T>) {
  const [sorting, setSorting] = useState<SortingState>([])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [globalFilter, setGlobalFilter] = useState('')

  const table = useReactTable({
    data,
    columns,
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    ...(pagination ? {} : { getPaginationRowModel: getPaginationRowModel() }), // Only use internal pagination if no external pagination
    ...(pagination ? {} : { getSortedRowModel: getSortedRowModel() }), // Only use client-side sorting if no external pagination
    getFilteredRowModel: getFilteredRowModel(),
    state: {
      sorting,
      columnFilters,
      globalFilter,
      ...(pagination ? { 
        pagination: { 
          pageIndex: pagination.pageIndex, 
          pageSize: pagination.pageSize 
        } 
      } : {}),
    },
    ...(pagination ? {
      manualPagination: true,
      manualSorting: true, // Disable client-side sorting for server-side pagination
      pageCount: pagination.pageCount,
    } : {
      initialState: {
        pagination: {
          pageSize: 5,
        },
      },
    }),
  })

  const colorClasses = {
    purple: {
      header: 'text-purple-600',
      headerBg: 'bg-purple-50',
      border: 'border-purple-200',
      button: 'bg-purple-600 hover:bg-purple-700 text-white',
      buttonOutline: 'border-purple-600 text-purple-600 hover:bg-purple-50',
      accent: 'text-purple-500'
    },
    blue: {
      header: 'text-blue-600',
      headerBg: 'bg-blue-50',
      border: 'border-blue-200',
      button: 'bg-blue-600 hover:bg-blue-700 text-white',
      buttonOutline: 'border-blue-600 text-blue-600 hover:bg-blue-50',
      accent: 'text-blue-500'
    },
    green: {
      header: 'text-green-600',
      headerBg: 'bg-green-50',
      border: 'border-green-200',
      button: 'bg-green-600 hover:bg-green-700 text-white',
      buttonOutline: 'border-green-600 text-green-600 hover:bg-green-50',
      accent: 'text-green-500'
    }
  }

  const colors = colorClasses[colorTheme]

  // Determine if pagination controls should be shown
  const showPagination = pagination ? 
    (pagination.total > pagination.pageSize) : 
    (table.getPageCount() > 1)

  if (isLoading) {
    return (
      <div className="card">
        <div className="animate-pulse">
          <div className="flex items-center justify-between mb-4">
            <div className={`h-6 ${colors.headerBg} rounded w-48`}></div>
            <div className={`h-4 ${colors.headerBg} rounded w-24`}></div>
          </div>
          <div className={`h-10 ${colors.headerBg} rounded mb-4`}></div>
          <div className="space-y-3">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="flex space-x-4">
                <div className="h-4 bg-gray-200 rounded flex-1"></div>
                <div className="h-4 bg-gray-200 rounded w-20"></div>
                <div className="h-4 bg-gray-200 rounded w-24"></div>
              </div>
            ))}
          </div>
          {loadingQuote && (
            <div className="mt-6 text-center">
              <div className="inline-block bg-gray-50 rounded-lg px-4 py-3 border-l-4 border-gray-400">
                <p className="text-gray-700 italic">"{loadingQuote}"</p>
                <p className="text-xs text-gray-500 mt-1">- The Family</p>
              </div>
            </div>
          )}
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="card">
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
          <div className="text-center">
            <p className="text-red-800 font-medium text-sm">❌ {error}</p>
            <button 
              onClick={() => window.location.reload()} 
              className="mt-2 text-xs text-red-700 hover:text-red-900 underline"
            >
              Try Again
            </button>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="card">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <h3 className={`text-lg font-semibold ${colors.header} flex items-center`}>
            {title}
          </h3>
          {subtitle && (
            <p className="text-sm text-gray-600 mt-1">{subtitle}</p>
          )}
        </div>
        <div className="text-sm text-gray-500">
          {pagination ? `${pagination.total} total` : `${data.length} ${data.length === 1 ? 'record' : 'records'}`}
        </div>
      </div>

      {/* Search - disabled for external pagination since it should be handled server-side */}
      {data.length > 0 && !pagination && (
        <div className="mb-4">
          <input
            placeholder={searchPlaceholder}
            value={globalFilter ?? ''}
            onChange={(e) => setGlobalFilter(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
        </div>
      )}

      {/* Table */}
      {data.length === 0 ? (
        <div className="p-6 text-center">
          <div className="text-3xl mb-2">{emptyStateIcon}</div>
          <p className="text-sm font-medium text-gray-800 mb-1">{emptyStateMessage}</p>
          <p className="text-xs text-gray-500">{emptyStateSubMessage}</p>
        </div>
      ) : (
        <>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className={colors.headerBg}>
                {table.getHeaderGroups().map((headerGroup) => (
                  <tr key={headerGroup.id}>
                    {headerGroup.headers.map((header) => (
                      <th
                        key={header.id}
                        className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                        onClick={header.column.getToggleSortingHandler()}
                      >
                        <div className="flex items-center space-x-1">
                          <span>
                            {header.isPlaceholder
                              ? null
                              : flexRender(header.column.columnDef.header, header.getContext())}
                          </span>
                          {header.column.getCanSort() && (
                            <span className="text-gray-400">
                              {{
                                asc: '↑',
                                desc: '↓',
                              }[header.column.getIsSorted() as string] ?? '↕️'}
                            </span>
                          )}
                        </div>
                      </th>
                    ))}
                  </tr>
                ))}
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {table.getRowModel().rows.map((row) => (
                  <tr key={row.id} className="hover:bg-gray-50">
                    {row.getVisibleCells().map((cell) => (
                      <td key={cell.id} className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {showPagination && (
            <div className="flex flex-col sm:flex-row items-center justify-between mt-4 pt-4 border-t border-gray-200 gap-4">
              <div className="flex items-center space-x-2">
                {pagination ? (
                  // External pagination controls
                  <>
                    <button
                      onClick={() => pagination.onPageChange(0)}
                      disabled={!pagination.hasPrevious}
                      className={`px-3 py-1 text-sm border rounded ${!pagination.hasPrevious 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'<<'}
                    </button>
                    <button
                      onClick={() => pagination.onPageChange(pagination.pageIndex - 1)}
                      disabled={!pagination.hasPrevious}
                      className={`px-3 py-1 text-sm border rounded ${!pagination.hasPrevious 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'<'}
                    </button>
                    <button
                      onClick={() => pagination.onPageChange(pagination.pageIndex + 1)}
                      disabled={!pagination.hasNext}
                      className={`px-3 py-1 text-sm border rounded ${!pagination.hasNext 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'>'}
                    </button>
                    <button
                      onClick={() => pagination.onPageChange(pagination.pageCount - 1)}
                      disabled={!pagination.hasNext}
                      className={`px-3 py-1 text-sm border rounded ${!pagination.hasNext 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'>>'}
                    </button>
                  </>
                ) : (
                  // Internal pagination controls
                  <>
                    <button
                      onClick={() => table.setPageIndex(0)}
                      disabled={!table.getCanPreviousPage()}
                      className={`px-3 py-1 text-sm border rounded ${!table.getCanPreviousPage() 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'<<'}
                    </button>
                    <button
                      onClick={() => table.previousPage()}
                      disabled={!table.getCanPreviousPage()}
                      className={`px-3 py-1 text-sm border rounded ${!table.getCanPreviousPage() 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'<'}
                    </button>
                    <button
                      onClick={() => table.nextPage()}
                      disabled={!table.getCanNextPage()}
                      className={`px-3 py-1 text-sm border rounded ${!table.getCanNextPage() 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'>'}
                    </button>
                    <button
                      onClick={() => table.setPageIndex(table.getPageCount() - 1)}
                      disabled={!table.getCanNextPage()}
                      className={`px-3 py-1 text-sm border rounded ${!table.getCanNextPage() 
                        ? 'border-gray-300 text-gray-400 cursor-not-allowed' 
                        : colors.buttonOutline}`}
                    >
                      {'>>'}
                    </button>
                  </>
                )}
              </div>
              
              <div className="flex items-center space-x-4 text-sm text-gray-600">
                <div className="flex items-center space-x-2">
                  <span>Show:</span>
                  <select
                    value={pagination ? pagination.pageSize : table.getState().pagination.pageSize}
                    onChange={(e) => {
                      const newPageSize = Number(e.target.value)
                      if (pagination) {
                        pagination.onPageSizeChange(newPageSize)
                      } else {
                        table.setPageSize(newPageSize)
                      }
                    }}
                    className="border border-gray-300 rounded px-2 py-1 text-sm"
                  >
                    {[5, 10, 20, 50].map((pageSize) => (
                      <option key={pageSize} value={pageSize}>
                        {pageSize}
                      </option>
                    ))}
                  </select>
                  <span>per page</span>
                </div>
                
                <div className="flex items-center space-x-2">
                  {pagination ? (
                    <>
                      <span>
                        Page {pagination.pageIndex + 1} of {pagination.pageCount}
                      </span>
                      <span>|</span>
                      <span>
                        {pagination.total} total records
                      </span>
                    </>
                  ) : (
                    <>
                      <span>
                        Page {table.getState().pagination.pageIndex + 1} of {table.getPageCount()}
                      </span>
                      <span>|</span>
                      <span>
                        {table.getFilteredRowModel().rows.length} total records
                      </span>
                    </>
                  )}
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
} 