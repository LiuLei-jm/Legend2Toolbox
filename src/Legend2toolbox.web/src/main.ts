import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import { OpenAPI } from './api/generated/core/OpenAPI';
import ElementPlus from 'element-plus';

OpenAPI.BASE = 'https://localhost:7113';

OpenAPI.TOKEN = async () => {
  const token = localStorage.getItem('access_token');
  return token ? `Bearer ${token}` : "";
}

const app = createApp(App)
app.use(ElementPlus)
app.mount('#app')
