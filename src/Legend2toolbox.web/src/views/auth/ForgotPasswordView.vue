<template>
    <div class="forgot-container">
        <el-card class="box-card" style="width: 420px;">
            <h2>找回密码</h2>
            <p class="sub-title">请输入您绑定的注册邮箱，我们将向您发送重置密码连接</p>
            <el-form :model="form" :rules="rules" ref="formRef" label-position="top">
                <el-form-item prop="email" label="注册邮箱">
                    <el-input v-model="form.email" placeholder="请输入绑定的邮箱地址" prefix-icon="Message" />
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" :loading="loading" @click="handleSendEmail" style="width:100%;">
                        发送重置邮件
                    </el-button>
                </el-form-item>
            </el-form>
        </el-card>
    </div>
</template>

<script setup lang='ts'>
import { ref, reactive } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from "element-plus"
import { AuthorizationService } from '@/api/generated/services/AuthorizationService'
import type { ForgotPasswordRequest } from '@/api/generated/models/ForgotPasswordRequest'
import { handleApiError } from '@/utils/errorHandler'

const formRef = ref<FormInstance>()
const loading = ref(false)
const form = reactive({
    email: ''
})

const rules = reactive<FormRules>({
    email: [
        { required: true, message: '请输入绑定的邮箱地址', trigger: 'blur' },
        { type: 'email', message: '请输入正确的邮箱格式', trigger: 'blur' }
    ]
})

const handleSendEmail = async () => {
    if (!formRef.value) return
    await formRef.value.validate(async (valid) => {
        if (!valid) return
        loading.value = true
        try {
            const clientResetUrl = `${window.location.origin}/#/reset-password`
            const requestData: ForgotPasswordRequest = {
                email: form.email,
                clientResetUrl: clientResetUrl
            } as ForgotPasswordRequest;
            AuthorizationService.postApiAuthForgotPassword(requestData);
        }
        catch (error) {
            handleApiError(error, '发送重置邮件失败')
        }
        finally {
            loading.value = false
        }
    })
}
</script>

<style scoped>
.forgot-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: #f5f7fa;
}

.sub-title {
    font-size: 13px;
    color: #909399;
    margin-bottom: 20px;
}

.footer-links {
    text-align: right;
    margin-top: 10px;
}

.footer-links a {
    color: #409eff;
    text-decoration: none;
    font-size: 14px;
}
</style>