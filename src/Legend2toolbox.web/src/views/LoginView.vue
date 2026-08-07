<template>
    <div class="login-layout">
        <div class="bg-shape shape-1"></div>
        <div class="bg-shape shape-2"></div>

        <div class="login-container">
            <el-card class="glass-card" :body-style="{ padding: '24px 28px' }">
                <div class="header-section">
                    <div class="logo-box">
                        <el-icon :size="20" color="#409eff">
                            <Platform />
                        </el-icon>
                    </div>
                    <h2 class="title">欢迎登录</h2>
                </div>

                <el-form ref="loginFormRef" :model="formData" :rules="formRules" label-width="70px" size="default"
                    @keyup.enter="handleLogin" class="modern-form">
                    <el-form-item label="账号" prop="username">
                        <el-input v-model="formData.username" placeholder="请输入用户名" clearable :prefix-icon="User" />
                    </el-form-item>
                    <el-form-item label="密码" prop="password">
                        <el-input v-model="formData.password" type="password" placeholder="请输入密码" show-password
                            :prefix-icon="Lock" />
                    </el-form-item>
                    <el-form-item label-width="0" class="btn-form-item">
                        <el-button type="primary" class="submit-btn" :loading="isLoading" @click="handleLogin">
                            登录</el-button>
                    </el-form-item>
                    <div class="footer-links">
                        <el-link type="info" underline="hover" @click="handleForgotPassword">忘记密码? </el-link>
                        <div class="register-link">
                            还没有账号?
                            <el-link type="primary" underline="hover" @click="goToRegister">
                                立即注册
                                <el-icon class="el-icon--right">
                                    <ArrowRight />
                                </el-icon>
                            </el-link>
                        </div>
                    </div>
                </el-form>
            </el-card>
        </div>
    </div>
</template>

<script setup lang='ts'>
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from "vue-router"
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

import { useAuthStore } from '@/stores/auth'
import { User, Lock, Platform, ArrowRight } from '@element-plus/icons-vue'
import type { LoginRequest } from '@/api/generated/models/LoginRequest'

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const loginFormRef = ref<FormInstance>();
const isLoading = ref(false);

const formData = reactive({
    username: '',
    password: '',
});

const formRules = reactive<FormRules>({
    username: [{ required: true, message: "请输入用户名", trigger: "blur" }],
    password: [{ required: true, message: "请输入密码", trigger: "blur" }],
})

const handleLogin = async () => {
    if (!loginFormRef.value) return;
    try {
        const valid = await loginFormRef.value.validate();
        if (!valid) return;
        isLoading.value = true;

        const requestData: LoginRequest = {
            username: formData.username,
            password: formData.password,
        } as LoginRequest;
        const success = await authStore.login(requestData);
        if (success) {
            ElMessage.success("登录成功!");
            const redirectPath = (route.query.redirect as string) || {name: 'Home'};
            router.push(redirectPath);
        }
    }
    catch (error) {
        console.error("登录失败:", error)
    }
    finally {
        isLoading.value = false;
    }
}

const handleForgotPassword = async () => {
    ElMessage.info("请联系管理员重置密码");
}
const goToRegister = async () => {
    router.push({name: 'Register'});
}
</script>

<style scoped>
.login-layout {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    padding: 20px;
    box-sizing: border-box;
    background-color: #f3f4f6;
    overflow: hidden;
}

.bg-shape {
    position: absolute;
    border-radius: 50%;
    filter: blur(80px);
    z-index: 0;
}

.shape-1 {
    width: 350px;
    height: 350px;
    background: rgba(64, 158, 255, 0.25);
    top: -80px;
    left: -80px;
}

.shape-1 {
    width: 350px;
    height: 350px;
    background: rgba(64, 158, 255, 0.25);
    top: -80px;
    left: -80px;
}

.login-container {
    position: relative;
    z-index: 1;
    width: 100%;
    max-width: 300px;
}

.glass-card {
    width: 100%;
    border: none;
    border-radius: 12px;
    background: rgba(255, 255, 255, 0.88);
    backdrop-filter: blur(12px);
    box-shadow: 0 15px 35px -5px rgba(0, 0, 0, 0.08);
}

.header-section {
    text-align: center;
    margin-bottom: 24px;
}

.logo-box {
    width: 40px;
    height: 40px;
    margin: 0 auto 8px;
    background: #ecf5ff;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.title {
    margin: 0;
    font-size: 18px;
    color: #1f2937;
}

.modern-form {
    width: 100%;
}

.btn-form-item {
    margin-top: 10px;
    margin-bottom: 12px !important;
}

.submit-btn {
    width: 100%;
    height: 40px;
    border-radius: 6px;
    font-size: 14px;
}

.footer-links {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 12px;
    font-size: 13px;
    color: #6b7280;
}

.register-link {
    display: flex;
    align-items: center;
    gap: 4px;
}

:deep(.el-form-item:not(:last-child)) {
    margin-bottom: 18px;
}

:deep(.el-icon) {
    font-size: 14px !important;
}
</style>