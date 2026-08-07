import './assets/main.css'

import axios from 'axios'
import { createApp } from 'vue'
import { createPinia } from "pinia"
import App from './App.vue'
import router from './routers'
import { OpenAPI } from './api/generated/core/OpenAPI';
import ElementPlus from 'element-plus';
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import 'element-plus/dist/index.css'

OpenAPI.BASE = 'http://localhost:5098';


axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')
  const tokenType = localStorage.getItem('token_type') || 'Bearer'

  if (token) {
    config.headers = config.headers || {};
    config.headers['Authorization'] = `${tokenType} ${token}`
  }
  return config;
}, (error) => {
  return Promise.reject(error);
})

const app = createApp(App)
const pinia = createPinia()
app.use(pinia)
app.use(router)

app.use(ElementPlus, {
  locale: zhCn,
})

app.mount('#app')
