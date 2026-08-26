import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';

import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'
import ElementPlus from 'unplugin-element-plus/vite'

import { visualizer } from 'rollup-plugin-visualizer'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    plugin(),
    ElementPlus({}),
    AutoImport({
      resolvers: [
        ElementPlusResolver({
          importStyle: 'css'
        })
      ],
    }),
    Components({
      resolvers: [
        ElementPlusResolver({
          importStyle: 'css'
        })
      ]
    }),
    visualizer({
      open: true,
      filename: 'dist/stats.html',
      gzipSize: true,
      brotliSize: true
    }),
  ],
  build: {
    outDir: 'D:\\DotNetLearning\\Legend2Toolbox\\bin\\Web',
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (
              id.includes('vue') ||
              id.includes('vue-router') ||
              id.includes('pinia')
            ) {
              return 'vue'
            }
            if (id.includes('element-plus')) {
              return 'element'
            }
            if (id.includes('axios')) {
              return 'axios'
            }
          }
        }
      }
    }
  },

  server: {
    port: 30457,
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
