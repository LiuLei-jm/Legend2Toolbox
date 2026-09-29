<template>
  <el-dialog v-model="dialogVisible" title="调整会员时间" width="420px" @close="handleClose">
    <el-form label-position="top">
      <el-form-item label="用户">
        <el-input :model-value="userInfo?.username" disabled />
      </el-form-item>
      <el-form-item label="调整天数" required>
        <el-input-number v-model="days" :min="-3650" :max="3650" :step="1" controls-position="right"
          class="days-input" />
        <div class="hint">正数增加会员时间，负数减少会员时间。</div>
      </el-form-item>
      <div class="quick-actions">
        <el-button size="small" @click="days = 30">增加 30 天</el-button>
        <el-button size="small" @click="days = -30">减少 30 天</el-button>
      </div>
    </el-form>

    <template #footer>
      <el-button @click="handleClose">取消</el-button>
      <el-button type="primary" :loading="loading" :disabled="days === 0" @click="handleSubmit">
        保存调整
      </el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { AdminMembershipService } from '@/api/adminMembershipService'
import type { AdminUserInfo } from '@/types/user'
import { handleApiError } from '@/utils/errorHandler'

const props = defineProps<{
  visible: boolean
  userInfo?: AdminUserInfo
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  success: []
}>()

const dialogVisible = computed({
  get: () => props.visible,
  set: (value: boolean) => emit('update:visible', value),
})

const days = ref(30)
const loading = ref(false)

const handleSubmit = async () => {
  if (!props.userInfo?.id || days.value === 0) return
  loading.value = true
  try {
    await AdminMembershipService.adjustDays(props.userInfo.id, days.value)
    ElMessage.success(days.value > 0 ? '会员时间已增加' : '会员时间已减少')
    emit('success')
    handleClose()
  } catch (error) {
    handleApiError(error, '调整会员时间失败')
  } finally {
    loading.value = false
  }
}

const handleClose = () => {
  days.value = 30
  dialogVisible.value = false
}
</script>

<style scoped>
.days-input {
  width: 100%;
}

.hint {
  color: var(--el-text-color-secondary);
  font-size: 12px;
  margin-top: 6px;
}

.quick-actions {
  display: flex;
  gap: 8px;
}
</style>
