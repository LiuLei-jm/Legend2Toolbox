<template>
    <div class="profile-container">
        <el-row :gutter="20">
            <el-col :lg="8" :md="8" :sm="24" :xs="24" class="mb-4">
                <el-card class="box-card profile-card" shadow="hover">
                    <template #header>
                        <span>个人信息</span>
                    </template>

                    <div class="user-avator-section">
                        <el-avatar :icon="UserFilled" :size="90" class="avatar-large" />
                        <h3 class="username-title">{{ currentUser }}</h3>
                        <p class="user-role">
                            <el-tag v-for="role in userInfo.roles" :key="role" :type="RoleTagTypeMap[role]"
                                effect="plain">{{
                                RoleDisplayMap[role] }}
                            </el-tag>
                        </p>
                    </div>

                    <div class="user-info-list">
                        <div class="info-item">
                            <span class="label"><el-icon>
                                    <Message />
                                </el-icon> 电子邮件</span>
                            <span class="value">{{ userInfo.email }}</span>
                        </div>
                        <div class="info-item">
                            <span class="label"><el-icon>
                                    <Phone />
                                </el-icon> 联系电话</span>
                            <span class="value">{{ userInfo.phoneNumber }}</span>
                        </div>
                        <div class="info-item">
                            <span class="label"><el-icon>
                                    <Calendar />
                                </el-icon> 上次登录</span>
                            <span class="value">{{ userInfo.lastLoginAt }}</span>
                        </div>
                    </div>
                </el-card>
            </el-col>

            <el-col :lg="16" :md="16" :sm="24" :xs="24">
                <el-card class="box-card" shadow="hover">
                    <el-tabs v-model="activeTab" class="profile-tabs">
                        <el-tab-pane label="基本资料" name="info">
                            <el-form ref="infoFormRef" :model="infoForm" :rules="infoRules" class="mt-3"
                                label-position="top">
                                <el-form-item label="用户昵称" prop="username">
                                    <el-input id="nickname" name="nickname" v-model="infoForm.nickname" placeholder="请输入用户昵称" />
                                </el-form-item>
                                <el-form-item label="电子邮箱" prop="email">
                                    <el-input id="email" name="email" v-model="infoForm.email" placeholder="请输入电子邮箱" />
                                </el-form-item>
                                <el-form-item label="联系电话" prop="phone">
                                    <el-input id="phoneNumber" name="phoneNumber" v-model="infoForm.phoneNumber" placeholder="请输入联系电话" />
                                </el-form-item>
                                <el-form-item>
                                    <el-button :loading="infoLoading" type="primary" @click="handleUpdateInfo">
                                        保存修改
                                    </el-button>
                                </el-form-item>
                            </el-form>
                        </el-tab-pane>

                        <el-tab-pane label="安全设置" name="security">
                            <el-form ref="pwdFormRef" :model="pwdForm" :rules="pwdRules" class="mt-3"
                                label-position="top">
                                <el-form-item label="当前密码" prop="oldPassword">
                                    <el-input id="oldPassword" name="oldPassword" v-model="pwdForm.oldPassword" placeholder="请输入当前密码" show-password
                                        type="password" />
                                </el-form-item>
                                <el-form-item label="新密码" prop="newPassword">
                                    <el-input id="newPassword" name="newPassword" v-model="pwdForm.newPassword" placeholder="请输入新密码" show-password
                                        type="password" />
                                </el-form-item>
                                <el-form-item label="确认新密码" prop="confirmPassword">
                                    <el-input id="confirmPassword" name="confirmPassword" v-model="pwdForm.confirmPassword" placeholder="请再次输入当前密码" show-password
                                        type="password" />
                                </el-form-item>
                                <el-form-item>
                                    <el-button :loading="pwdLoading" type="primary" @click="handleChangePassword">
                                        修改密码
                                    </el-button>
                                </el-form-item>
                            </el-form>
                        </el-tab-pane>
                    </el-tabs>
                </el-card>
            </el-col>
        </el-row>
    </div>
</template>

<script lang='ts' setup>
import { computed, reactive, ref } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import type { RuleItem } from 'async-validator'
import { Calendar, Message, Phone, UserFilled } from '@element-plus/icons-vue'
import { AuthorizationService } from '@/api/generated/services/AuthorizationService'
import type { ChangePasswordRequest } from '@/api/generated/models/ChangePasswordRequest'
import type { UpdateUserProfileRequest } from '@/api/generated/models/UpdateUserProfileRequest'
import { useAuthStore } from '@/stores/auth'
import { handleApiError } from '@/utils/errorHandler'
import { RoleDisplayMap, RoleTagTypeMap } from '@/utils/role'

const authStore = useAuthStore()
const userInfo = authStore.userInfo || {
    username: 'xxx',
    nickname: '',
    email: 'xxx@example.com',
    phoneNumber: 'xxxxxxxxxxx',
    lastLoginAt: "......",
    roles: []
}

const currentUser = computed<string>(() => {
    return userInfo?.nickname ? userInfo?.nickname : userInfo?.username
})

const activeTab = ref<string>('info')
const infoLoading = ref<boolean>(false)
const pwdLoading = ref<boolean>(false)

const infoFormRef = ref<FormInstance>()
const pwdFormRef = ref<FormInstance>()

const infoForm = reactive({
    nickname: userInfo.nickname || '',
    email: userInfo.email || '',
    phoneNumber: userInfo.phoneNumber || '',
})

const infoRules = reactive<FormRules>({
    nickname: [{ required: true, message: '请输入用户昵称', trigger: 'blur' }],
    email: [{
        required: true, message: '请输入电子邮箱', trigger: 'blur'
    }],
    phoneNumber: [{ required: true, message: '请输入联系电话', trigger: 'blur' }]
})

const pwdForm = reactive({
    oldPassword: '',
    newPassword: '',
    confirmPassword: '',
})

const validatePass2 = (_rule: RuleItem, value: string, callback: (error?: Error) => void) => {
    if (value === '') {
        callback(new Error('请再次输入密码'))
    } else if (value !== pwdForm.newPassword) {
        callback(new Error('两次输入密码不一致'))
    } else {
        callback()
    }
}

const pwdRules = reactive<FormRules>({
    oldPassword: [{ required: true, message: '请输入当前密码', trigger: 'blur' }],
    newPassword: [{ required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, message: '密码长度不能小于6位', trigger: 'blur' }
    ],
    confirmPassword: [{ required: true, validatePass2, trigger: 'blur' }]
})

const handleUpdateInfo = async () => {
    if (!infoFormRef.value) return
    await infoFormRef.value.validate(async (valid) => {
        if (valid) {
            infoLoading.value = true
            try {
                const dataReqeust: UpdateUserProfileRequest = {
                    nickName: infoForm.nickname,
                    email: infoForm.email,
                    phoneNumber: infoForm.phoneNumber
                } as UpdateUserProfileRequest;
                await AuthorizationService.putApiAuthUpdate(dataReqeust);
                ElMessage.success('个人资料修改成功')
            } catch (error) {
                handleApiError(error, '修改失败，请稍后重试')
            } finally {
                infoLoading.value = false
            }
        }
    })
}

const handleChangePassword = async () => {
    if (!pwdFormRef.value) return
    await pwdFormRef.value.validate(async (valid) => {
        if (valid) {
            pwdLoading.value = true
            try {
                const dataRequest: ChangePasswordRequest = {
                    oldPassword: pwdForm.oldPassword,
                    newPassword: pwdForm.newPassword
                } as ChangePasswordRequest
                await AuthorizationService.postApiAuthChangePassword(dataRequest)
                ElMessage.success('密码修改成功，请重新登录')
                pwdForm.oldPassword = ''
                pwdForm.newPassword = ''
                pwdForm.confirmPassword = ''
            } catch (error) {
                handleApiError(error, '密码修改失败')
            } finally {
                pwdLoading.value = false
            }
        }
    })
}

</script>

<style scoped>
.profile-container {
    padding: 10px;
}

.box-card {
    border-radius: 8px;
    border: none;
}

.card-header {
    font-weight: 600;
    font-size: 16px;
    color: #1f2937;
}

.user-avatar-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 20px 0;
    border-bottom: 1px solid #f3f4f6;
}

.avatar-large {
    background: linear-gradient(135deg, #409eff 0%, #3656ff 100%);
    color: #fff;
    margin-bottom: 12px;
}

.username-title {
    font-size: 18px;
    font-weight: 600;
    color: #111827;
    margin: 4px 0 8px 0;
}

.user-info-list {
    padding: 15px 10px;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.info-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 14px;
}

.info-item .label {
    display: flex;
    align-items: center;
    gap: 6px;
    color: #6b7280;
}

.info-item .value {
    color: #374151;
    font-weight: 500;
}

.mb-4 {
    margin-bottom: 16px;
}
</style>
