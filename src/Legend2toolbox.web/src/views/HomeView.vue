<template>
  <el-container class="home-layout">
    <SideMenu
      v-if="!isMobile"
      :is-collapse="isCollapse"
      class="desktop-side-menu"
    />

    <el-drawer
      v-model="mobileDrawerVisible"
      :with-header="false"
      class="mobile-drawer"
      direction="ltr"
      size="240px"
    >
      <SideMenu :is-collapse="false" @click="mobileDrawerVisible = false"/>
    </el-drawer>

    <el-container class="right-section">
      <TopHead
        :is-collapse="isCollapse"
        @toggle-collapse="handleToggleCollapse"
      />

      <el-main class="main-content-area">
        <router-view v-slot="{ Component }">
          <transition mode="out-in" name="fade-transform">
            <component :is="Component"/>
          </transition>
        </router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<script lang='ts' setup>
import {onMounted, onUnmounted, ref} from 'vue'
import SideMenu from '@/components/layout/SideMenu.vue'
import TopHead from '@/components/layout/TopHead.vue'

const isCollapse = ref(false)
const isMobile = ref(false)
const mobileDrawerVisible = ref(false)

// 检测当前屏幕尺寸是否为移动端
const checkScreenSize = () => {
  const width = window.innerWidth
  isMobile.value = width < 768
  // 如果从手机切回电脑，自动关闭移动端抽屉
  if (!isMobile.value) {
    mobileDrawerVisible.value = false
  }
}

// 顶部折叠/抽屉触发按钮核心逻辑
const handleToggleCollapse = () => {
  if (isMobile.value) {
    // 移动端：切换抽屉开关
    mobileDrawerVisible.value = !mobileDrawerVisible.value
  } else {
    // PC端：切换侧边栏宽窄折叠
    isCollapse.value = !isCollapse.value
  }
}

onMounted(() => {
  checkScreenSize()
  window.addEventListener('resize', checkScreenSize)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkScreenSize)
})
</script>

<style scoped>
.home-layout {
  height: 100vh;
  width: 100vw;
  overflow: hidden;
  background-color: #f3f4f6;
}

.desktop-side-menu {
  transition: width 0.3s ease-in-out;
}

.right-section {
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow: hidden;
  flex: 1;
  min-width: 0;
}

.main-content-area {
  background-color: #f5f7fa;
  padding: 16px;
  overflow-y: auto;
  overflow-x: hidden;
}

/* 移动端抽屉样式定制 */
:deep(.mobile-drawer) {
  width: 240px !important;
}

:deep(.mobile-drawer .el-drawer__body) {
  padding: 0 !important;
  background-color: #304156 !important;
}

/* 页面切换过渡动画 */
.fade-transform-leave-active,
.fade-transform-enter-active {
  transition: all 0.2s ease;
}

.fade-transform-enter-from {
  opacity: 0;
  transform: translateX(-10px);
}

.fade-transform-leave-to {
  opacity: 0;
  transform: translateX(10px);
}
</style>
