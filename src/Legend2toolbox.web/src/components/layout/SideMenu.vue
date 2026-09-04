<template>
  <el-aside :width="isCollapse ? '64px' : '220px'" class="side-menu-container">
    <div class="logo-area">
      <el-icon class="logo-icon">
        <Box />
      </el-icon>
      <h1 v-show="!isCollapse" class="logo-title">工具集合</h1>
    </div>

    <el-menu :collapse="isCollapse" :collapse-transition="false" :default-active="$route.path" :router="true"
      active-text-color="#409EFF" background-color="#304156" class="side-el-menu" text-color="#bfcbd9">

      <el-menu-item index="/card-number">
        <el-icon>
          <Ticket />
        </el-icon>
        <template #title>卡号管理</template>
      </el-menu-item>

      <el-menu-item index="/connection-key">
        <el-icon>
          <Key />
        </el-icon>
        <template #title>通讯密钥</template>
      </el-menu-item>

      <el-menu-item index="/script">
        <el-icon>
          <Document />
        </el-icon>
        <template #title>脚本管理</template>
      </el-menu-item>

      <el-menu-item index="download-gateway" @click="downloadWpfGateway">
        <el-icon>
          <Download />
        </el-icon>
        <template #title>下载网关</template>
      </el-menu-item>
      <el-sub-menu v-if="hasSuperAdminRole" index="/admin">
        <template #title>
          <el-icon>
            <Setting />
          </el-icon>
          <span>控制台</span>
        </template>

        <el-menu-item index="/admin/users">
          <el-icon>
            <User />
          </el-icon>
          <span>用户管理</span>
        </el-menu-item>
      </el-sub-menu>

    </el-menu>
  </el-aside>
</template>

<script lang='ts' setup>
import { ref } from 'vue'
import { Box, Key, Ticket, User, Setting, Download, Document } from '@element-plus/icons-vue'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import { computed } from 'vue'

const authStore = useAuthStore()

const lastDownloadTime = ref<number>(0)

defineProps<{
  isCollapse: boolean
}>()

const hasSuperAdminRole = computed(() => {
  return authStore.userInfo?.roles.includes('SuperAdmin')
});

const downloadWpfGateway = () => {
  const now = Date.now()
  const COOLDOWN_TIME = 60 * 1000

  if (now - lastDownloadTime.value < COOLDOWN_TIME) {
    const remainingSeconds = Math.ceil((COOLDOWN_TIME - (now - lastDownloadTime.value)) / 1000)
    ElMessage.warning(`下载太频繁，请等待 ${remainingSeconds} 秒后再试`)
    return
  }

  lastDownloadTime.value = now

  const downloadUrl = '/downloads/CardGateway.zip'

  ElMessage.success('正在开始下载 WPF 客户端网关...')

  const link = document.createElement('a')
  link.href = downloadUrl
  link.download = 'CardGateway.zip'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}
</script>

<style scoped>
.side-menu-container {
  background-color: #304156;
  transition: width 0.3s ease-in-out;
  overflow-x: hidden;
  overflow-y: auto;
  box-shadow: 2px 0 6px rgba(0, 21, 41, 0.35);
  display: flex;
  flex-direction: column;
  height: 100%;
}

.logo-area {
  height: 60px;
  background-color: #2b2f3a;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: #ffffff;
  overflow: hidden;
  white-space: nowrap;
  padding: 0 12px;
}

.logo-icon {
  font-size: 22px;
  color: #409eff;
  flex-shrink: 0;
}

.logo-title {
  font-size: 15px;
  font-weight: 600;
  margin: 0;
  letter-spacing: 0.5px;
}

.side-el-menu {
  border-right: none;
  flex: 1;
}

/* 优化 Element Plus 菜单在收缩时的样式体验 */
:deep(.el-menu--collapse) {
  width: 64px;
}
</style>
