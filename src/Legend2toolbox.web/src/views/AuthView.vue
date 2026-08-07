<template>
    <div class="login-container">
        <el-card class="login-card">
            <el-tabs v-model="activeTab">

                <!-- 登录 -->
                <el-tab-pane label="登录" name="login">
                    <el-form ref="loginFormRef" :model="loginForm" :rule="loginRules" label-width="0px"
                        @submit.prevent="handleLogin">
                        <el-form-item prop="username">
                            <el-input v-model="loginForm.username" placeholder="用户名" prefix-icon="User" size="large" />
                        </el-form-item>
                        <el-form-item prop="password">
                            <el-input v-model="loginForm.password" placeholder="密码" prefix-icon="Lock" size="large" />
                        </el-form-item>
                        <el-form-item>
                            <el-button :loading="isLoading" type="primary" class="full-width-btn" native-type="submit"
                                size="large">登录</el-button>
                        </el-form-item>
                    </el-form>
                </el-tab-pane>
                <!-- 注册 -->
                <el-tab-pane label="注册" name="register">
                    <el-form ref="registerFormRef" :model="registerForm" :rules="registerRules" lable-width="0px"
                        @submit.prevent="handleRegister">
                        <el-form-item prop="username">
                            <el-input v-model="registerForm.username" placeholder="设置用户名" prefix-icon="User"
                                size="large" />
                        </el-form-item>
                        <el-form-item prop="email">
                            <el-input v-model="registerForm.email" placeholder="设置邮箱" prefix-icon="Email"
                                size="large" />
                        </el-form-item>
                        <el-form-item prop="passwords">
                            <el-input v-model="registerForm.password" type="password" placeholder="设置密码"
                                prefix-icon="Lock" show-password size="large" />
                        </el-form-item>
                        <el-form-item prop="confirmPassword">
                            <el-input v-model="registerForm.confirmPassword" type="password" placeholder="确认密码"
                                prefix-icon="Lock" show-password size="large" />
                        </el-form-item>
                        <el-form-item>
                            <el-button :loading="isLoading" type="primary" class="full-width-btn" native-type="submit"
                                size="large">
                                立即注册
                            </el-button>
                        </el-form-item>
                        <div class="login-link">
                            已有账号？
                            <el-link type="primary" @click="goToLogin">去登录 <el-icon class="el-icon--right">
                                    <ArrowRight />
                                </el-icon></el-link>
                        </div>
                    </el-form>
                </el-tab-pane>
            </el-tabs>
        </el-card>
    </div>
</template>

<script setup lang='ts'>
import {  ArrowRight } from "@element-plus/icons-vue";

import { ref, reactive } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import type { LoginRequest } from '@/api/generated/models/LoginRequest'
import type { RuleItem } from 'async-validator'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const activeTab = ref<string>('login')
const isLoading = ref<boolean>(false)
const loginFormRef = ref<FormInstance>()
const loginForm = reactive<LoginRequest>({ username: '', password: '' })
const loginRules = reactive<FormRules>({
    username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
    password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
})
const registerFormRef = ref<FormInstance>()
const registerForm = reactive({ username: '', email: '', password: '', confirmPassword: '' })
const validateConfirmPassword = (rule: RuleItem, value: string, callback: (error?: Error) => void) => {
    if (value === "") {
        callback(new Error("请再次输入密码"));
    } else if (value !== registerForm.password) {
        callback(new Error("两次输入密码不一致！"));
    } else {
        callback();
    }
};
const registerRules = reactive<FormRules>({
    username: [
        { required: true, message: "请输入用户名", trigger: "blur" },
        { min: 3, max: 20, message: "长度在 3 到 20 个字符", trigger: "blur" },
    ],
    email: [
        { required: true, message: "请输入邮箱地址", trigger: "blur" },
        { type: "email", message: "请输入正确的邮箱地址", trigger: "blur" },
    ],
    password: [
        { required: true, message: "请输入密码", trigger: "blur" },
        { min: 6, message: "密码长度不能小于 6 位", trigger: "blur" },
    ],
    confirmPassword: [{ required: true, validator: validateConfirmPassword, trigger: "blur" }],
})

const handleLogin = async () => {
    if (!loginFormRef.value) return
    await loginFormRef.value.validate(async (valid) => {
        if (valid) {
            isLoading.value = true
            await authStore.login(loginForm)
            isLoading.value = false;
        }
    })
}
const handleRegister = async () => {
    if (!registerFormRef.value) return
    await registerFormRef.value.validate(async (valid) => {
        if (valid) {
            isLoading.value = true
            const success = await authStore.register({
                username: registerForm.username,
                email: registerForm.email,
                password: registerForm.password
            })
            if (success) {
                activeTab.value = "login"
                registerFormRef.value?.resetFields()
            }
            isLoading.value = false
        }
    })
}
const goToLogin = async () => {
    activeTab.value = 'login'
}
</script>

<style scoped>
.login-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: #f0f2f5;
}

.login-card {
    width: 400px;
    padding: 20px;
}

.full-width-btn {
    width: 100%;
}

:deep(.el-tabs__nav-wrap::after) {
    background-color: transparent;
}

:deep(.el-tabs__nav) {
    float: none;
    text-align: center;
}

:dep(.el-tabs__active-bar) {
    transform: translateX(-50%) !important;
    left: 50%;
}
</style>