<template>
  <el-dialog title="编辑用户信息" v-model="dialogVisible" width="450px" @open="initForm" @close="handleClose">
    <el-form :model="formData" :rules="rules" ref="formRef" label-width="80px">
      <el-form-item label="用户名" prop="userName">
        <el-input v-model="formData.username" placeholder="请输入新用户名" clearable />
      </el-form-item>
      <el-form-item label="邮箱" prop="email">
        <el-input v-model="formData.email" placeholder="请输入新邮箱" clearable />
      </el-form-item>
    </el-form>
    <template #footer>
      <el-button @click="handleClose">取消</el-button>
      <el-button type="primary" :loading="loading" @click="handleSubmit">确定保存</el-button>
    </template>
  </el-dialog>
</template> 

<script setup lang='ts'>
  import {ref,reactive, computed} from 'vue'
  import type {FormInstance, FormRules} from 'element-plus'
  import {AdminUserManagementService} from '@/api/generated/services/AdminUserManagementService'
  import type { UpdateUserRequest } from '@/api/generated/models/UpdateUserRequest'
  import {handleApiError} from '@/utils/errorHandler'
  import type {AdminUserInfo} from '@/types/user'
  
  const props = defineProps<{
    visible:boolean
    userInfo: AdminUserInfo
  }>()

const emit = defineEmits(['update:visible','success'])

const dialogVisible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const loading= ref(false)
const formRef = ref<FormInstance>()
const formData = reactive({
  username:'',
  email:'',
})

const rules: FormRules ={
  userName: [{required: true, message: '用户名不能为空', trigger:'blur'}],
  email: [
    {required: true, message: '邮箱不能为空',trigger: 'blur'},
    {type: 'email', message: '请输入正确的邮箱地址',trigger: 'blur'}
  ]
}


const initForm = () =>{
  Object.assign(formData, props.userInfo)
}

const handleSubmit = async() =>{
  if(!formRef.value || !props.userInfo?.id) return
  await formRef.value.validate(async (valid) =>{
    if(valid){
      loading.value = true
      try{
      const requestData : UpdateUserRequest = {
        username: formData.username,
        email: formData.email
      }
        await AdminUserManagementService.putApiAdminUsersUpdate(props.userInfo.id, requestData)
      }
      catch(error){
        handleApiError(error,'保存失败')
      }
      finally{
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

</style>