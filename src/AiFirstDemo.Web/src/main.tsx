import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { SignalRProvider } from './shared/contexts/SignalRContext'
import { SessionProvider } from './shared/contexts/SessionContext'
import App from './App.tsx'
import './index.css'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <SessionProvider>
        <SignalRProvider>
          <BrowserRouter>
            <App />
          </BrowserRouter>
        </SignalRProvider>
      </SessionProvider>
    </QueryClientProvider>
  </React.StrictMode>,
) 