<template>
    <el-dialog :title="isEdit ? '修改卡号' : '新建卡号'" :model-value="visible"
        @update:model-value="$emit('update:visible', $event)" width="500px" @open="initForm" @close="resetForm">
        <el-form :model="form" :rules="rules" ref="formRef" label-width="100px">
            <el-form-item label="客户名" prop="owner">
                <el-input v-model="form.owner" placeholder="请输入客户名称" />
            </el-form-item>
            <el-form-item label="持续天数" prop="durationInDays">
                <el-input-number v-model="form.durationInDays" :min="1" style="width: 100%" />
            </el-form-item>
            <el-form-item  label="面值" prop="faceValue">
                <el-input-number v-model="form.faceValue" :min="0" :precision="1" style="width: 100%" />
            </el-form-item>
            <el-form-item  label="实际金额" prop="amount">
                <el-input-number v-model="form.amount" :min="0" :precision="1" style="width:100%" />
            </el-form-item>
            <el-form-item v-if="isEdit" label="开始时间" prop="startTime">
                <el-date-picker v-model="form.startTime" type="datetime" placeholder="选择开始时间" 
                 style="width: 100%" />
            </el-form-item>
            <el-form-item label="备注">
                <el-input v-model="form.notes" type="textarea" placeholder="请输入备注信息" />
            </el-form-item>
        </el-form>

        <template #footer>
            <span class="dialog-footer">
                <el-button @click="$emit('update:visible', false)">取消</el-button>
                <el-button type="primary" @click="handleSubmit" :loading="submitLoading">确定</el-button>
            </span>
        </template>
    </el-dialog>
</template>

<script setup lang='ts'>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { CardNumberService } from '@/api/generated/services/CardNumberService'
import type { CreateCardNumberRequest } from '@/api/generated/models/CreateCardNumberRequest'
import type { UpdateCardNumberRequest } from '@/api/generated/models/UpdateCardNumberRequest'
import { handleApiError } from '@/utils/errorHandler'

const props = defineProps<{
    visible: boolean
    isEdit: boolean
    initialData?: any
}>()

const emit = defineEmits(['update:visible', 'success'])

const formRef = ref<FormInstance>()
const submitLoading = ref(false)

const form = reactive({
    id: '',
    owner: '',
    durationInDays: 0,
    faceValue: 0,
    amount: 0,
    startTime: '',
    notes: ''
})

const rules = reactive<FormRules>({
    owner: [{ required: true, message: '客户名不能为空', trigger: 'blur' },
    { max: 100, message: '客户名称不能超过100个字符', trigger: 'blur' }
    ],
    durationInDays: [
        { required: true, message: '请输入持续天数', trigger: 'blur' },
        { type: 'number', min: 1, message: '卡号持续时间必须大于 0 天', trigger: 'blur' }
    ],
    faceValue: [
        { required: true, message: '请输入面值', trigger: 'blur' },
        { type: 'number', min: 0, max: 9999, message: '面值在0~9999之间', trigger: 'blur' }
    ],
    amount: [
        { required: true, message: '请输入金额', trigger: 'blur' },
        { type: 'number', min: 0, message: '金额不能为负数', trigger: 'blur' }
    ],
    notes: [
        { max: 500, message: '备注不能超过500个字符', trigger: 'blur' }
    ]
})

const initForm = () => {
    if (props.isEdit && props.initialData) {
        Object.assign(form, props.initialData)
    } else {
        Object.assign(form, {
            id: '',
            owner: '',
            durationInDays: 30,
            faceValue: 300,
            amount: 300,
            startTime: '',
            notes: ''
        })
    }
}

const handleSubmit = async () => {
    if (!formRef.value) return
    const valid = formRef.value.validate()
    if (!valid) return
    submitLoading.value = true
    try {
        if (props.isEdit) {
            const requestData: UpdateCardNumberRequest = {
                owner: form.owner,
                durationInDays: form.durationInDays,
                faceValue: form.faceValue,
                amount: form.amount,
                startTime: form.startTime,
                notes: form.notes
            }
            await CardNumberService.putApiCardUpdate(form.id, requestData)
            ElMessage.success('修改成功')
        }
        else {
            const requestData: CreateCardNumberRequest = {
                owner: form.owner,
                durationInDays: form.durationInDays,
                faceValue: form.faceValue,
                amount: form.amount,
                notes: form.notes
            }
            await CardNumberService.postApiCardCreate(requestData)
            ElMessage.success('新建成功')
        }
        emit('success')
        emit('update:visible', false)
    }
    catch (error) {
        handleApiError(error, props.isEdit ? '修改失败' : '新建失败')
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