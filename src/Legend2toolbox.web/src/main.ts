import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from "pinia"
import App from './App.vue'
import router from './routers'
import { OpenAPI } from './api/generated/core/OpenAPI';

import axios from 'axios'
import { useAuthStore } from '@/stores/auth'


const app = createApp(App)
const pinia = createPinia()
app.use(pinia)
app.use(router)

OpenAPI.BASE = 'https://localhost:7113';
OpenAPI.TOKEN = async () => {
  return localStorage.getItem('access_token') || '';
}
interface QueueItem {
  resolve: (value?: any) => void;
  reject: (reason?: any) => void;
}

let isRefreshing = false;
let requestsQueue: Array<QueueItem> = [];

const processQueue = (error: any, token: string | null = null) => {
  requestsQueue.forEach(({ resolve, reject }) => {
    if (error) {
      reject(error)
    } else {
      resolve(token)
    }
  });
  requestsQueue = []
}


axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token')

  if (token) {
    config.headers = config.headers || {};
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config;
}, (error) => {
  return Promise.reject(error);
})

axios.interceptors.response.use((response) => {
  return response;
},
  async (error) => {
    const originalRequest = error.config;

    if (error.response && error.response.status === 401 && !originalRequest.url?.includes('/refresh')) {
      if (originalRequest._retry) {
        const authStore = useAuthStore();
        authStore.logout();
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          requestsQueue.push({
            resolve: (newToken: string) => {
              originalRequest.headers['Authorization'] = `Bearer ${newToken}`;
              resolve(axios(originalRequest));
            },
            reject: (err: any) => {
              reject(err)
            }
          })
        })
      }

      originalRequest._retry = true;
      isRefreshing = true;
      const authStore = useAuthStore();

      try {
        const refreshSuccess = await authStore.refreshTokenAction();

        if (refreshSuccess) {
          const newToken = authStore.accessToken as string;

          processQueue(null, newToken);

          originalRequest.headers['Authorization'] = `Bearer ${newToken}`;
          return axios(originalRequest);
        } else {
          requestsQueue = [];
          authStore.logout();
          return Promise.reject(error);
        }
      } catch (error) {
        requestsQueue = [];
        authStore.logout();
        return Promise.reject(error);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  })

const authStore = useAuthStore()
if (authStore.expiresAt)
  authStore.startAutoRefresh()

app.mount('#app')
