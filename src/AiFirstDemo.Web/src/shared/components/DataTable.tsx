import React, { useState } from 'react'
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
  colorTheme = 'blue'
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
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    state: {
      sorting,
      columnFilters,
      globalFilter,
    },
    initialState: {
      pagination: {
        pageSize: 10,
      },
    },
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
          {data.length} {data.length === 1 ? 'record' : 'records'}
        </div>
      </div>

      {/* Search */}
      {data.length > 0 && (
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
          {table.getPageCount() > 1 && (
            <div className="flex items-center justify-between mt-4 pt-4 border-t border-gray-200">
              <div className="flex items-center space-x-2">
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
              </div>
              
              <div className="flex items-center space-x-2 text-sm text-gray-600">
                <span>
                  Page {table.getState().pagination.pageIndex + 1} of {table.getPageCount()}
                </span>
                <span>|</span>
                <span>
                  {table.getFilteredRowModel().rows.length} total records
                </span>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
} 