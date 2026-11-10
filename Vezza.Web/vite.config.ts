import { loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react()],
    test: {
      environment: 'jsdom',
      setupFiles: './src/test/setup.ts',
      css: true,
      restoreMocks: true,
    },
    server: {
      port: 5173,
      proxy: {
        '/v1': {
          target: env.VITE_API_PROXY_TARGET || 'http://localhost:5288',
          changeOrigin: true,
          secure: false,
        },
        '/health-check': {
          target: env.VITE_API_PROXY_TARGET || 'http://localhost:5288',
          changeOrigin: true,
          secure: false,
        },
      },
    },
  }
})
