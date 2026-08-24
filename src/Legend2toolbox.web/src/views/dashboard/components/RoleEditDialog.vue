<template>
    <el-dialog v-model="dialogVisible" title="分配角色" width="400px" @open="initForm" @close="handleClose">
        <el-form ref="formRef" :model="formData" :rules="rules" label-width="80px" v-loading="fetchingRoles">
            <el-form-item label="当前用户">
                <el-input :model-value="userInfo?.username" disabled />
            </el-form-item>
            <el-form-item label="选择角色" prop="roles">
                <el-checkbox-group v-model="formData.roles" class="role-radio-group">
                    <el-checkbox v-for="role in authStore.roleList" :key="role.value || role.label || role"
                        :value="role.value || role" border size="default">
                        {{ role.label || role }}
                    </el-checkbox>
                </el-checkbox-group>
            </el-form-item>
        </el-form>

        <template #footer>
            <div class="dialog-footer">
                <el-button @click="handleClose">
                    取消
                </el-button>
                <el-button type="primary" :loading="loading" @click="handleSubmit">保存</el-button>
            </div>
        </template>
    </el-dialog>
</template>

<script setup lang='ts'>
import { ref, computed } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from 'element-plus'
import {useAuthStore} from '@/stores/auth'
import { AdminUserManagementService } from '@/api/generated/services/AdminUserManagementService'
import type { AssignRoleRequest } from '@/api/generated/models/AssignRoleRequest'
import type { AdminUserInfo } from '@/types/user'
import { handleApiError } from '@/utils/errorHandler'

const props = defineProps<{
    visible: boolean
    userInfo: AdminUserInfo
}>()

const emit = defineEmits<{
    'update:visible': [value: boolean]
    success: []
}>()

const dialogVisible = computed({
    get: () => props.visible,
    set: (val) => emit('update:visible', val)
})

const authStore = useAuthStore()

const loading = ref<boolean>(false)
const fetchingRoles = ref<boolean>(false)
const formRef = ref<FormInstance>()
const formData = ref<{ roles: string[] }>(
    {
        roles: []
    }
)

const rules: FormRules = {
    roles: [
        {
            type: 'array',
            required: true,
            message: '请选择角色',
            trigger: 'change'
        }
    ]
}

const initForm = async () => {
    try{
        await authStore.fetchRoleList()
    }catch(error){
        handleApiError(error,'获取角色列表失败')
    }
    if (props.userInfo) {
        if (Array.isArray(props.userInfo.roles)) {
            formData.value.roles = [...props.userInfo.roles]
        } else if (typeof props.userInfo.roles === 'string' && props.userInfo.roles) {
            formData.value.roles = [props.userInfo.roles]
        } else {
            formData.value.roles = []
        }
    }
    else {
        formData.value.roles = []
    }
}


const handleSubmit = async () => {
    if (!formRef.value || !props.userInfo?.id) return
    await formRef.value.validate(async (valid) => {
        if (valid) {
            loading.value = true
            try {
                const requestData: AssignRoleRequest = {
                    roleNames: formData.value.roles
                }
                await AdminUserManagementService.postApiAdminUsersRoles(props.userInfo.id, requestData)
                ElMessage.success('角色分配成功')
                emit('success')
                handleClose()
            } catch (error) {
                handleApiError(error, '配置失败')
            } finally {
                loading.value = false
            }
        }
    })
}

const handleClose = () => {
    dialogVisible.value = false
}
</script>

<style scoped>
.role-radio-group {
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
}

.role-radio-group .el-radio {
    margin-right: 0;
    width: 100%;
}
</style>