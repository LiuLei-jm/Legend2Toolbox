<template>
    <div class="reset-container">
        <el-card class="box-card" style="width: 400px;">
            <h2>重置新密码</h2>
            <el-form :model="form" :rules="rules" ref="formRef">
                <el-form-item prop="newPassword" label="新密码">
                    <el-input id="password" name="password" v-model="form.newPassword" type="password" placeholder="请输入新密码" show-password />
                </el-form-item>
                <el-form-item  prop="confirmPassword" label="确认密码">
                    <el-input id="confirmPassword" name="confirmPassword" v-model="form.confirmPassword" type="password" placeholder="请再次输入新密码" show-password />
                </el-form-item>
                <el-form-item>
                    <el-button type="success" :loading="loading" @click="handleResetPassword" style="width:100%;">
                        确认重置密码
                    </el-button>
                </el-form-item>
            </el-form>
        </el-card>
    </div>
</template>

<script setup lang='ts'>
import { ref, reactive, onMounted } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import { useRoute, useRouter } from 'vue-router'
import type { RuleItem } from 'async-validator'
import { ElMessage } from 'element-plus'
import { AuthorizationService } from '@/api/generated/services/AuthorizationService'
import type { ResetPasswordRequest } from '@/api/generated/models/ResetPasswordRequest'
import { handleApiError } from '@/utils/errorHandler'

const route = useRoute()
const router = useRouter()
const formRef = ref<FormInstance>()
const loading = ref<boolean>(false)

const form = reactive({
    email: '',
    token: '',
    newPassword: '',
    confirmPassword: ''
})

const validatePass2 = (rule: RuleItem, value: string, callback: (error?: Error) => void) => {
    if (value !== form.newPassword) {
        callback(new Error('两次输入密码不一致！'))
    }
    else {
        callback()
    }
}

const rules = reactive<FormRules>({
    newPassword: [
        { required: true, message: '请输入新密码', trigger: 'blur' },
        { min: 6, message: '密码长度不能小于6位', trigger: 'blur' }
    ],
    confirmPassword: [
        { required: true, message: '请再次输入新密码', trigger: 'blur' },
        { validator: validatePass2, trigger: 'blur' }
    ]
})

onMounted(() => {
    const emailQuery = route.query.email
    const tokenQuery = route.query.token
    form.email = Array.isArray(emailQuery) ? (emailQuery[0] || '') : (emailQuery || '')
    form.token = Array.isArray(tokenQuery) ? (tokenQuery[0] || '') : (tokenQuery || '')

    if (!form.email || !form.token) {
        ElMessage.error('重置连接无效或已过期!')
    }
})

const handleResetPassword = async () => {
    if (!formRef.value) return
    await formRef.value.validate(async (valid) => {
        if (!valid) return
        loading.value = true
        try {
            const requestData: ResetPasswordRequest = {
                email: form.email,
                token: form.token,
                newPassword: form.newPassword
            }
            AuthorizationService.postApiAuthResetPassword(requestData)
            ElMessage.success('密码重置成功，请使用新密码登录！')
            setTimeout(() => {
                router.push('/login')
            }, 1500)
        }
        catch (error) {
            handleApiError(error,'重置密码失败')
        }
        finally {
            loading.value = false
        }
    })
}
</script>

<style scoped>
.reset-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: #f5f7fa;
}
</style>