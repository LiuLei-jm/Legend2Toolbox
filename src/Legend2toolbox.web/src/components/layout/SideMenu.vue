<template>
  <el-aside :width="isCollapse ? '64px' : '220px'" class="side-menu-container">
    <div class="logo-area">
      <el-icon class="logo-icon">
        <Box/>
      </el-icon>
      <h1 v-show="!isCollapse" class="logo-title">会员管理系统</h1>
    </div>

    <el-menu
      :collapse="isCollapse"
      :collapse-transition="false"
      :default-active="$route.path"
      :router="true"
      active-text-color="#409EFF"
      background-color="#304156"
      class="side-el-menu"
      text-color="#bfcbd9">

      <el-menu-item index="/home/dashboard">
        <el-icon>
          <DataBoard/>
        </el-icon>
        <template #title>系统概览</template>
      </el-menu-item>

      <el-sub-menu index="member-group">
        <template #title>
          <el-icon>
            <User/>
          </el-icon>
          <span>会员中心</span>
        </template>
        <el-menu-item index="/home/members">会员列表</el-menu-item>
        <el-menu-item index="/home/member-levels">等级管理</el-menu-item>
      </el-sub-menu>

      <el-menu-item index="/home/settings">
        <el-icon>
          <Setting/>
        </el-icon>
        <template #title>系统设置</template>
      </el-menu-item>
    </el-menu>
  </el-aside>
</template>

<script lang='ts' setup>
import {Box, DataBoard, Setting, User} from '@element-plus/icons-vue'

defineProps<{
  isCollapse: boolean
}>()
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
