import { defineStore } from 'pinia'
import { ElMessage } from 'element-plus'
import router from '@/routers'

import type { UserInfo } from '@/types/user'
import { AuthorizationService } from '@/api/generated/services/AuthorizationService'
import type { RegisterRequest } from '@/api/generated/models/RegisterRequest'
import type { LoginRequest } from '@/api/generated/models/LoginRequest'
import {handleApiError} from '@/utils/errorHandler'

let clearAllStores: (() => void) | null = null

export const registerClearAllStores = (callback: () => void) => {
    clearAllStores = callback
}

interface AuthState {
    accessToken: string | null
    refreshToken: string | null
    tokenType: string | null
    user: UserInfo | null
}

export const useAuthStore = defineStore("auth", {
    state: (): AuthState => ({
        accessToken: localStorage.getItem('access_token') || null,
        refreshToken: localStorage.getItem('refrush_token') || null,
        tokenType: localStorage.getItem('token_type') || null,
        user: JSON.parse(localStorage.getItem('user') || 'null'),
    }),
    getters: {
        isAuthenticated: (state => !!state.accessToken),
        isAdmin: (state => state.user && state.user.roles.includes('admin'))
    },
    actions: {
        async register(credentials: RegisterRequest): Promise<boolean> {
            try {
                await AuthorizationService.postApiAuthRegister(credentials)
                return true
            } catch (error) {
                console.log("注册失败异常：",error)
                handleApiError(error,"注册失败，请检查输入信息")
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
                this.tokenType = response.tokenType || 'Bearer'
                if (!this.accessToken) {
                    throw new Error('登录失败，未收到令牌')
                }

                localStorage.setItem('access_token', this.accessToken);
                
                if (this.refreshToken) {
                    localStorage.setItem('refresh_token', this.refreshToken)
                }
                if (this.tokenType) {
                    localStorage.setItem('token_type', this.tokenType)
                }

                const userFetched = await this.getUserInfo()
                if (!userFetched) {
                    throw new Error('获取用户信息失败')
                }

                return true
            } catch (error) {
                console.log("登录失败异常",error)
                handleApiError(error,"登录失败，账号或密码错误")
                this.logout()
                return false
            }
        },
        async getUserInfo(): Promise<boolean> {
            try {
                const response = await AuthorizationService.getApiAuthUserinfo()
                this.user = {
                    userId: response.userId,
                    username: response.userName,
                    roles: response.roles
                }
                localStorage.setItem('user', JSON.stringify(this.user))
                return true
            }
            catch (error) {
                console.log('获取用户信息出错', error)
                return false
            }
        },
        logout() {
            if (clearAllStores) {
                clearAllStores()
            }
            this.accessToken = null
            this.refreshToken = null
            this.tokenType = null
            this.user = null
            localStorage.removeItem('access_token')
            localStorage.removeItem('user')
            ElMessage.info("已登出")
            router.push({ name: 'Login' })
        }
    }
})

