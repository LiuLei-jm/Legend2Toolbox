<template>
    <el-dialog title="配置卡号文件路径" :model-value="dialogVisible" @update:model-value="$emit('update:visible', $event)"
        width="500px" @open="fetchCurrentPath" @close="resetForm">
        <el-form :model="form" :rules="rules" ref="formRef" label-width="120px" v-loading="loading">
            <el-form-item label="基础路径" prop="basePath">
                <el-input v-model="form.basePath" placeholder="例如：C:\Cards" />
            </el-form-item>
            <el-form-item label="文件路径" prop="fileName">
                <el-input v-model="form.fileName" placeholder="例如：data\cards.txt" />
            </el-form-item>
            <el-form-item label="允许自定义路径">
                <el-switch v-model="form.allowCustomPaths" active-text="允许" inactive-text="禁止" />
            </el-form-item>
        </el-form>
        <template #footer>
            <span class="dialog-footer">
                <el-button @click="$emit('update:visible', false)">取消</el-button>
                <el-button type="primary" @click="handleSubmit" :loading="submitLoading">保存</el-button>
            </span>
        </template>
    </el-dialog>
</template>

<script setup lang='ts'>
import { ref, reactive,computed } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { CardNumberService } from '@/api/generated/services/CardNumberService'
import type { UpdateCardNumberPathRequest } from '@/api/generated/models/UpdateCardNumberPathRequest'
import { handleApiError } from '@/utils/errorHandler'

const emit = defineEmits(['update:visible', 'success'])
const props = defineProps<{
    visible: boolean
}>()

const dialogVisible = computed ({
    get: () =>props.visible,
    set : (val) => emit('update:visible', val) 
}

) 
const formRef = ref<FormInstance>()
const loading = ref(false)
const submitLoading = ref(false)

const form = reactive({
    basePath: '',
    fileName: '',
    allowCustomPaths: false
})

const rules = reactive<FormRules>({
    basePath: [{ required: true, message: '基础路径不能为空', trigger: 'blur' }],
    fileName: [{ required: true, message: '文件路径不能为空', trigger: 'blur' }]
})

const fetchCurrentPath = async () => {
    loading.value = true
    try {
        const res = await CardNumberService.getApiCardPath()
        Object.assign(form, res)
    } catch (error) {
        handleApiError(error, '获取路径配置失败')
    }
    finally {
        loading.value = false
    }
}

const handleSubmit = async () => {
    if (!formRef.value) return
    const valid = await formRef.value.validate()
    if (!valid) return
    submitLoading.value = true
    try {
        const requestData: UpdateCardNumberPathRequest = {
            basePath: form.basePath,
            fileName: form.fileName,
            allowCustomPath: form.allowCustomPaths
        }
        await CardNumberService.putApiCardPathUpdate(requestData)
        ElMessage.success('路径配置保存成功')
        emit('success')
        emit('update:visible', false)
    }
    catch (error) {
        handleApiError(error, '保存配置失败')
    }
    finally {
        submitLoading.value = false
    }
}

const resetForm = () => {
    formRef.value?.resetFields()
}
</script>

<style scoped></style>