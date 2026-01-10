import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import ElementPlus from 'unplugin-element-plus/vite'

export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    ElementPlus(),
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
        target: 'http://localhost:8000',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/video/, '')
      },
      '/chat': {
        target: 'http://localhost:5555',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/chat/, '')
      },
      '/Teaching-video': {
        target: 'http://47.121.177.100:5002',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/Teaching-video/, '')
      }
    }
  }
})

