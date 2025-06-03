import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  // Load env file based on `mode` in the current working directory.
  // Set the third parameter to '' to load all env regardless of the `VITE_` prefix.
  const env = loadEnv(mode, process.cwd(), '')

  // Default API URL for development - updated to correct port
  const defaultApiUrl = 'http://localhost:5003'
  
  return {
    plugins: [react()],
    server: {
      port: 3000,
      open: true,
      cors: true,
      proxy: {
        '/api': {
          target: env.VITE_API_URL || defaultApiUrl,
          changeOrigin: true,
          secure: false,
          rewrite: (path) => path,
          configure: (proxy, _options) => {
            proxy.on('error', (err, _req, _res) => {
              console.log('Proxy error:', err)
            })
            proxy.on('proxyReq', (proxyReq, req, _res) => {
              console.log(`Proxying ${req.method} ${req.url} to ${proxyReq.getHeaders().host}`)
            })
          }
        },
        '/hubs': {
          target: env.VITE_API_URL || defaultApiUrl,
          changeOrigin: true,
          secure: false,
          ws: true,
          rewrite: (path) => path
        }
      }
    },
    build: {
      outDir: '../AiFirstDemo.Api/wwwroot',
      emptyOutDir: true,
      sourcemap: true,
      rollupOptions: {
        output: {
          manualChunks: {
            vendor: ['react', 'react-dom'],
            router: ['react-router-dom'],
            query: ['@tanstack/react-query']
          }
        }
      }
    },
    preview: {
      port: 3000,
      open: true
    },
    define: {
      // Make sure environment variables are available in the app
      __DEV__: mode === 'development'
    }
  }
}) 