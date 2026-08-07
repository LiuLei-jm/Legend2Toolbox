import {fileURLToPath, URL} from 'node:url'
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    server: {
        port: 30457,
    },
    resolve :{
        alias:{
            '@': fileURLToPath(new URL('./src',import.meta.url))
        }
    }
})
