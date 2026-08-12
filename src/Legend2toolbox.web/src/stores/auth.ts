import { defineStore } from 'pinia'
import { ElMessage } from 'element-plus'
import router from '@/routers'

import type { UserInfo } from '@/types/user'
import { AuthorizationService } from '@/api/generated/services/AuthorizationService'
import type { RegisterRequest } from '@/api/generated/models/RegisterRequest'
import type { LoginRequest } from '@/api/generated/models/LoginRequest'
import { handleApiError } from '@/utils/errorHandler'

let clearAllStores: (() => void) | null = null

export const registerClearAllStores = (callback: () => void) => {
  clearAllStores = callback
}

let refreshTimer: ReturnType<typeof setTimeout> | null = null;

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  expiresAt: string | null
  userInfo: UserInfo | null
}

export const useAuthStore = defineStore("auth", {
  state: (): AuthState => ({
    accessToken: localStorage.getItem('access_token') || null,
    refreshToken: localStorage.getItem('refrush_token') || null,
    expiresAt: localStorage.getItem('expires_at') || null,
    userInfo: JSON.parse(localStorage.getItem('user') || 'null'),
  }),
  getters: {
    isAuthenticated: (state => !!state.accessToken),
    isAdmin: (state => state.userInfo && state.userInfo.roles.includes('admin'))
  },
  actions: {
    startAutoRefresh() {
      this.stopAutoRefresh();

      if (!this.expiresAt) return;

      const expiresTimeMs = new Date(this.expiresAt).getTime();
      const now = Date.now();

      const bufferTime = 60 * 1000;
      const timeUntilRefresh = expiresTimeMs - now - bufferTime;

      if (timeUntilRefresh <= 0) {
        this.refreshTokenAction();
      } else {
        refreshTimer = setTimeout(() => {
          console.log("Token即将过期，触发主动防御式刷新...")
          this.refreshTokenAction()
        }, timeUntilRefresh)
      }
    },

    stopAutoRefresh() {
      if (refreshTimer) {
        clearTimeout(refreshTimer);
        refreshTimer = null;
      }
    },

    async register(credentials: RegisterRequest): Promise<boolean> {
      try {
        await AuthorizationService.postApiAuthRegister(credentials)
        return true
      } catch (error) {
        handleApiError(error, "注册失败，请检查输入信息")
        return false
      }
    },

    async login(credentials: LoginRequest): Promise<boolean> {
      try {

        if (clearAllStores) {
          clearAllStores()
        }
        const response = await AuthorizationService.postApiAuthLogin(credentials)

        this.accessToken = response.accessToken
        this.refreshToken = response.refreshToken
        this.expiresAt = response.expiresAt
        if (!this.accessToken) {
          throw new Error('登录失败，未收到令牌')
        }

        localStorage.setItem('access_token', this.accessToken);

        if (this.refreshToken) {
          localStorage.setItem('refresh_token', this.refreshToken)
        }
        if (this.expiresAt)
          localStorage.setItem('expires_at', this.expiresAt)

        const userFetched = await this.getUserInfo()
        if (!userFetched) {
          throw new Error('获取用户信息失败')
        }

        this.startAutoRefresh();

        return true
      } catch (error) {
        handleApiError(error, "登录失败，用户名或密码错误")
        return false
      }
    },

    async getUserInfo(): Promise<boolean> {
      try {
        const response = await AuthorizationService.getApiAuthUserinfo()
        const dateStr = response.lastLoginAt;
        const date = new Date(dateStr)
        const result = date.toLocaleString('zh-CN', { hour12: false })
        this.userInfo = {
          userId: response.userId,
          username: response.userName,
          nickname: response.nickName,
          email: response.email,
          phoneNumber: response.phoneNumber,
          lastLoginAt: result,
          roles: response.roles
        }
        localStorage.setItem('user', JSON.stringify(this.userInfo))
        return true
      } catch (error) {
        handleApiError(error,'获取用户信息出错')
        return false
      }
    },

    logout() {
      if (clearAllStores) {
        clearAllStores()
      }
      this.stopAutoRefresh();
      this.accessToken = null
      this.refreshToken = null
      this.expiresAt = null
      this.userInfo = null
      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
      localStorage.removeItem('expires_at')
      localStorage.removeItem('user')
      ElMessage.info("已登出")
      router.push({ name: 'Login' })
    },

    async refreshTokenAction(): Promise<boolean> {
      try {
        if (!this.refreshToken) {
          return false;
        }
        const response = await AuthorizationService.postApiAuthRefresh({
          refreshToken: this.refreshToken
        });

        this.accessToken = response.accessToken;
        this.refreshToken = response.refreshToken;
        this.expiresAt = response.expiresAt;

        if (this.accessToken)
          localStorage.setItem('access_token', this.accessToken);
        if (this.refreshToken)
          localStorage.setItem('refresh_token', this.refreshToken);
        if (this.expiresAt)
          localStorage.setItem('refresh_token', this.expiresAt);

        console.log("静默刷新 Token 成功!")
        this.startAutoRefresh();

        return true;
      } catch (error) {
        handleApiError(error,'安全会话已过期，请重新登录')
        this.logout();
        return false;
      }
    }
  }
})

