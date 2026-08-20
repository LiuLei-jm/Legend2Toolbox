<template>
  <el-header class="top-head-container">
    <div class="head-left">
      <el-icon class="collapse-icon" @click="toggleCollapse">
        <Expand v-if="isCollapse"/>
        <Fold v-else/>
      </el-icon>
      <div class="welcome-box">
        <span class="welcome-text">欢迎回来，</span>
        <span class="user-highlight">{{ currentUsername }}</span>
      </div>
    </div>

    <div class="head-right">
      <el-dropdown trigger="click" @command="handleCommand">
                <span class="user-info-link">
                    <el-avatar :icon="UserFilled" :size="34" class="user-avatar"/>
                    <span class="username">{{ currentUsername }}</span>
                    <el-icon class="el-icon--right">
                        <ArrowDown/>
                    </el-icon>
                </span>
        <template #dropdown>
          <el-dropdown-menu class="user-dropdown-menu">
            <el-dropdown-item command="profile">
              <el-icon>
                <User/>
              </el-icon>
              个人中心
            </el-dropdown-item>
            <el-dropdown-item class="logout-item" command="logout" divided>
              <el-icon>
                <SwitchButton/>
              </el-icon>
              退出登录
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </el-header>
</template>

<script lang='ts' setup>
import {computed} from 'vue'
import {useRouter} from 'vue-router'
import {ElMessage} from 'element-plus'
import {ArrowDown, Expand, Fold, SwitchButton, User, UserFilled} from '@element-plus/icons-vue'
import {useAuthStore} from '@/stores/auth'
import { handleApiError} from '@/utils/errorHandler'

const authStore = useAuthStore()
const router = useRouter()

defineProps<{
  isCollapse: boolean
}>()

const emit = defineEmits<{
  (e: 'toggle-collapse'): void
}>()

// 获取当前登录用户名（如果没有则显示默认“系统用户”）
const currentUsername = computed(() => {
  return authStore.userInfo?.nickname ? authStore.userInfo?.nickname : authStore.userInfo?.username
})

const toggleCollapse = () => {
  emit('toggle-collapse')
}

const handleCommand = async (command: string) => {
  if (command === 'logout') {
    try {
      await authStore.logout()
      ElMessage.success('已安全退出登录')
      authStore.logout()
      router.push({name: 'Login'})
    } catch (error) {
      handleApiError(error,'退出登录失败:')
      // 兜底强制跳转
      router.push({name: 'Login'})
    }
  } else if (command === 'profile') {
    router.push({name: 'UserProfile'})
  }
}
</script>

<style scoped>
.top-head-container {
  height: 60px;
  background-color: #ffffff;
  border-bottom: 1px solid #e4e7ed;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  box-shadow: 0 1px 4px rgba(0, 21, 41, 0.05);
  z-index: 10;
}

.head-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.collapse-icon {
  font-size: 20px;
  cursor: pointer;
  color: #606266;
  transition: color 0.3s;
  border-radius: 4px;
}

.collapse-icon:hover {
  color: #409eff;
  background-color: #f3f4f6;
}

.welcome-box {
  display: flex;
  align-items: center;
  font-size: 14px;
}

.welcome-text {
  color: #6b7280;
}

.user-highlight {
  color: #1f2937;
  font-weight: 600;
}

.head-right {
  display: flex;
  align-items: center;
}

.user-info-link {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background-color 0.2s;
}

.user-info-link:hover {
  background-color: #f3f4f6;
}

.user-avatar {
  background: linear-gradient(135deg, #409eff 0%, #3656ff 100%);
  color: #fff;
}

.username {
  font-size: 14px;
  color: #374151;
  font-weight: 500;
}

/* 下拉菜单个性化调整 */
.logout-item {
  color: #f56c6c !important;
}

.logout-item:hover {
  background-color: #fef0f0 !important;
}
</style>
