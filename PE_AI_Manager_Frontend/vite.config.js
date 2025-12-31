import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    proxy: {
      '/User': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/Class': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/Course': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/Course_student': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/Homework': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      },
      '/video': {
        target: 'http://118.25.145.4:8000',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/video/, '')
      },
      '/chat': {
        target: 'http://118.25.145.4:5000',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/chat/, '')
      }


    }
  }
})

