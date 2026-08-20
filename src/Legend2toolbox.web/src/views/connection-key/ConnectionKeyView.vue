<template>
  <div class="connection-key-container">
    <el-card class="box-card mb-4" shadow="hover">
      <template #header>
        <div class="card-header">
          <span class="card-title">连接密钥管理</span>
          <el-button type="primary" :icon="Refresh" circle plain :loading="keyLoading" @click="fetchConnectionKey" />
        </div>
      </template>

      <div class="key-content" v-loading="keyLoading">
        <el-form label-width="100px" label-position="left">
          <el-form-item label="当前密钥">
            <div class="key-input-group">
              <el-input v-model="keyInfo.key" :type="showKey ? 'text' : 'password'" readonly placeholder="暂无密钥，请点击生成"
                style="max-width:420px;">
                <template #append>
                  <el-button :icon="showKey ? View : Hide" @click="showKey = !showKey" />
                </template>
              </el-input>

              <el-button type="primary" class="ml-2" :disabled="!keyInfo.key" @click="copyKey">
                复制密钥
              </el-button>
              <el-popconfirm title="重新生成密钥后，当前所有已连接的客户端需要更新密钥才能继续通讯，确定重置吗？" confirm-button-text="确定重置"
                cancel-button-text="取消" confirm-button-type="danger" width="280" @confirm="handleGenerateKey">
                <template #reference>
                  <el-button type="danger" class="ml-2" :loading="generating">
                    {{ keyInfo.key ? '重置密钥' : '生成密钥' }}
                  </el-button>
                </template>
              </el-popconfirm>
            </div>
          </el-form-item>

          <el-form-item v-if="keyInfo.createdAt" label="生成时间">
            <span class="text-gray">{{ formatDate(keyInfo.createdAt) }}</span>
          </el-form-item>

        </el-form>
      </div>
    </el-card>

    <el-card class="box-card" shadow="hover">
      <template #header>
        <div class="card-header">
          <span class="card-title">已连接客户端</span>
          <el-button type="primary" :icon="Refresh" circle plain :loading="clientsLoading" @click="fetchClients" />
        </div>
      </template>

      <div v-loading="clientsLoading">
        <el-empty v-if="clients.length === 0" description="暂无已连接客户端" />
        <template v-else>
          <div class="desktop-table">
            <el-table :data="clients" style="width:100%" stripe border>
              <el-table-column prop="connectionId" label="客户端 ID" min-width="140" show-overflow-tooltip />
              <el-table-column prop="deviceName" label="设备/客户端名称" min-width="160" />
              <el-table-column prop="ipAddress" label="IP 地址" min-width="140" />
              <el-table-column prop="connectionAt" label="连接时间" min-width="180">
                <template #default="{ row }">
                  {{ formatDate(row.connectionAt) }}
                </template>
              </el-table-column>
            </el-table>
          </div>

          <div class="mobile-card-list">
            <div :data="clients" v-for="row in clients" :key="row.connectionId" class="client-card-time">
              <div class="client-card-header">
                <span class="client-name">{{ row.deviceName }}</span>
              </div>
              <div class="client-card-body">
                <div class="info-row">
                  <span class="label">客户端 ID：</span>
                  <span class="value text-ellipsis">{{ row.connectionId }}</span>
                </div>
                <div class="info-row">
                  <span class="label">IP 地址：</span>
                  <span class="value">{{ row.ipAddress }}</span>
                </div>
                <div class="info-row">
                  <span class="label">连接时间：</span>
                  <span class="value">{{ formatDate(row.connectionAt) }}</span>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </el-card>
  </div>
</template>

<script setup lang='ts'>
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Refresh, View, Hide } from '@element-plus/icons-vue'
import { ConnectionKeyService } from '@/api/generated/services/ConnectionKeyService'
import { handleApiError } from '@/utils/errorHandler'

interface ConnectionKeyInfo {
  key?: string
  createdAt?: string
}

interface ConnectionClient {
  connectionId: string
  deviceName?: string
  ipAddress?: string
  connectionAt?: string
}

const keyInfo = ref<ConnectionKeyInfo>({})
const clients = ref<ConnectionClient[]>([])

const keyLoading = ref(false)
const clientsLoading = ref(false)
const generating = ref(false)
const showKey = ref(false)

const fetchConnectionKey = async () => {
  keyLoading.value = true
  try {
    const res = await ConnectionKeyService.getApiConnectionKey()
    if (typeof res === 'string') {
      keyInfo.value = { key: res }
    } else {
      keyInfo.value = (res as ConnectionKeyInfo) || {}
    }
  }
  catch (error) {
    handleApiError(error, '获取密钥失败')
  }
  finally {
    keyLoading.value = false
  }
}

const handleGenerateKey = async () => {
  generating.value = true
  try {
    const res = await ConnectionKeyService.postApiConnectionKey()
    ElMessage.success('密钥生成成功！')
    if (typeof res === 'string') {
      keyInfo.value = { key: res, createdAt: new Date().toISOString() }
    } else {
      keyInfo.value = (res as ConnectionKeyInfo) || {}
    }
    showKey.value = true
  } catch (error) {
    handleApiError(error, "生成密钥失败")
  }
  finally {
    generating.value = false
  }
}

const fetchClients = async () => {
  clientsLoading.value = true
  try {
    const res = await ConnectionKeyService.getApiConnectionKeyClients()
    clients.value = (res as ConnectionClient[]) || []
  }
  catch (error) {
    handleApiError(error, '获取客户端失败')
  } finally {
    clientsLoading.value = false
  }
}

const copyKey = async () => {
  if (!keyInfo.value.key) return
  try {
    await navigator.clipboard.writeText(keyInfo.value.key)
    ElMessage.success('密钥已复制到剪贴板')
  } catch (error) {
    handleApiError(error, '复制密钥失败')
  }
}

const formatDate = (dateStr?: string) => {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  return isNaN(date.getTime()) ? dateStr : date.toLocaleString()
}

onMounted(() => {
  fetchConnectionKey()
  fetchClients()
})
</script>

<style scoped>
.connection-key-container {
  padding: 20px;
}

.box-card {
  border-radius: 8px;
}

.mb-4 {
  margin-bottom: 20px;
}

.ml-2 {
  margin-left: 10px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-title {
  font-size: 16px;
  font-weight: bold;
  color: #303133;
}

.key-input-group {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
}

.text-gray {
  color: #909399;
}

.desktop-table {
  display: block
}

.mobile-card-list {
  display: none
}

@media screen and (max-width:768px) {
  .desktop-table {
    display: none;
  }

  .mobile-card-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .client-card-item {
    background: #fdfdfd;
    border: 1px solid #ebeef5;
    border-radius: 8px;
    padding: 12px 16px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
  }

  .client-card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-bottom: 8px;
    border-bottom: 1px solid #f2f6fc;
    margin-bottom: 8px;
  }

  .client-name {
    font-weight: bold;
    font-size: 15px;
    color: #303133;
  }

  .client-card-body {
    display: flex;
    flex-direction: column;
    gap: 6px;
  }

  .info-row {
    display: flex;
    justify-content: space-between;
    font-size: 13px;
  }

  .info-row .label {
    color: #909399;
    flex-shrink: 0;
    margin-right: 10px;
  }

  .info-row .value {
    color: #606266;
    text-align: right;
    word-break: break-all;
  }
}
</style>