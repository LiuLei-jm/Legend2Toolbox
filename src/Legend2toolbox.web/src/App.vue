<script lang="ts" setup>
  import {ref} from 'vue'
import {RouterView} from 'vue-router'
import {useAuthStore} from '@/stores/auth'
import {onMounted} from 'vue'
import zhCn from 'element-plus/es/locale/lang/zh-cn'

const locale =ref(zhCn)
const authStore = useAuthStore()

onMounted(() => {
  if (authStore.accessToken && !authStore.userInfo) {
    authStore.getUserInfo().catch(() => {
      authStore.logout()
    })
  }
})
</script>

<template>
  <el-config-provider :locale="locale">
    <div class="app-container" >
    <RouterView/>
    </div>
  </el-config-provider>
</template>

<style scoped>
.app-container {
  width: 100%;
  height: 100vh;
  overflow-x: hidden;
}
</style>
