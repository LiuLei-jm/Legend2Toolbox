<template>
    <div class="card-management-container">
        <el-card shadow="never" class="filter-card">
            <el-form :inline="true" :model="searchForm" class="filter-form">
                <el-form-item label="客户名">
                    <el-input v-model="searchForm.owner" placeholder="请输入客户名称" clearable />
                </el-form-item>
                <el-form-item label="卡号">
                    <el-input v-model="searchForm.cdk" placeholder="请输入完整或部分卡号" clearable />
                </el-form-item>
                <el-form-item label="创建时间">
                    <el-date-picker v-model="searchForm.timeRange" type="daterange" range-separator="至"
                        start-placeholder="开始日期" end-placeholder="结束日期" value-format="YYYY-MM-DD" />
                </el-form-item>
                <el-form-item>
                    <el-button type="primary" @click="handleSearch" icon="Search">查询</el-button>
                    <el-button @click="resetSearch" icon="Refresh">重置</el-button>
                </el-form-item>
            </el-form>
        </el-card>

        <el-card shadow="never" class="table-card" style="margin-top: 15px">
            <div class="toolbar" style="margin-bottom: 20px; display: flex; gap: 10px;">
                <el-button type="primary" icon="Plus" @click="openCreateDialog">新建卡号</el-button>
                <el-button type="success" icon="Settings" @click="pathDialogVisible = true">配置路径</el-button>
                <el-switch v-model="showOnlyUnexpired" active-text="仅看未过期" inactive-text="全部卡号" @change="fetchData(1)"
                    style="margin-left: auto;" />
            </div>

            <el-table class="desktop-table" v-loading="loading" :data="tableData" border style="width:100%">
                <el-table-column prop="owner" label="客户名" width="150" />
                <el-table-column prop="cdk" label="卡号" min-width="200">
                    <template #default="{ row }">
                        <el-tooltip content="点击复制卡号" placement="top" :show-after="300">
                            <el-tag size="large" type="info" class="copyable-tag" @click="copyToClipboard(row.cdk)">
                                <span>{{ row.cdk }}</span>
                                <el-icon class="copy-icon">
                                    <DocumentCopy />
                                </el-icon>
                            </el-tag>
                        </el-tooltip>
                    </template>
                </el-table-column>
                <el-table-column prop="durationInDays" label="天数" width="80" align="center" />
                <el-table-column prop="endTime" label="到期时间" width="180" align="center">
                    <template #default="{ row }">
                        {{ formatDate(row.endTime) }}
                    </template>
                </el-table-column>
                <el-table-column label="状态" width="100" align="center">
                    <template #default="{ row }">
                        <el-tag :type="isCardExpired(row.endTime) ? 'danger' : 'success'">
                            {{ isCardExpired(row.endTime) ? '已过期' : '生效中' }}
                        </el-tag>
                    </template>
                </el-table-column>
                <el-table-column label="操作" width="280" fixed="right" align="center">
                    <template #default="{ row }">
                        <el-button size="small" type="primary" link @click="openEditDialog(row)">修改</el-button>
                        <el-button size="small" type="warning" link @click="handleReissue(row)">补发</el-button>
                        <el-button size="small" type="info" link @click="handleClear(row)">清理</el-button>
                        <el-button size="small" type="danger" link @click="handleDelete(row)">删除</el-button>
                    </template>
                </el-table-column>
            </el-table>

            <div class="mobile-card-list" v-loading="loading">
                <div v-if="tableData.length === 0 && !loading"
                    style="text-align: center; color: #909399;padding: 30px 0;">暂无数据
                </div>
                <div v-for="row in tableData" :key="row.id"
                    style="background: #fdfdfd;border: 1px solid #ebeef5; border-radius: 8px; padding: 12px; margin-bottom:12px; box-shadow: 0 2px 12px 0 rgba(0,0,0,0.05);">
                    <div style="display:flex; justify-content: space-between; align-items:center; margin-bottom: 8px;">
                        <span style="font-weight:bold; font-size:16px; color: #303133;">{{ row.owner }}</span>
                        <el-tag size="small" :type="isCardExpired(row.endTime) ? 'danger' : 'success'">
                            {{ isCardExpired(row.endTime) ? '已过期' : '生效中' }}
                        </el-tag>
                    </div>
                    <div style="margin-bottom: 8px; word-break: break-all;">
                        <el-tag size="default" type="info" style="width: 100%;justify-content:center;"
                            @click="copyToClipboard(row.cdk)">{{ row.cdk
                            }}</el-tag>
                    </div>
                    <div
                        style="font-size: 13px; color: #606266;display:flex;justify-content:space-between;margin-bottom:10px;border-bottom:1px dashed #ebeef5; padding-bottom: 8px; ">
                        <span>有效天数：<b>{{ row.durationInDays }}</b></span>
                        <span>到期：{{ formatDate(row.endTime) }}</span>
                    </div>
                    <div style="display:flex;justify-content:flex-end;gap:8px;">
                        <el-button size="small" type="primary" link @click="openEditDialog(row)">修改</el-button>
                        <el-button size="small" type="warning" link @click="handleReissue(row)">补发</el-button>
                        <el-button size="small" type="info" link @click="handleClear(row)">清理</el-button>
                        <el-button size="small" type="danger" link @click="handleDelete(row)">删除</el-button>
                    </div>
                </div>
            </div>

            <div class="pagination-wrapper"
                style="margin-top: 20px; display: flex; justify-content: flex-end;overflow-x:auto">
                <el-pagination v-model:current-page="pagination.pageNumber" v-model:page-size="pagination.pageSize"
                    :page-sizes="[10, 20, 50, 100]" layout="total,sizes,prev,pager,next,jumper"
                    :total="pagination.total" @size-change="fetchData(1)" @current-change="fetchData" />
            </div>
        </el-card>

        <PathConfigDialog v-model:visible="pathDialogVisible" />
        <CardFormDialog v-model:visible="cardDialogVisible" :is-edit="isEditMode" :initial-data="selectedCard"
            @success="fetchData" />
    </div>
</template>

<script setup lang='ts'>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { DocumentCopy } from '@element-plus/icons-vue'
import { CardNumberService } from '@/api/generated/services/CardNumberService'
import type { ReissueCardRequest } from '@/api/generated/models/ReissueCardRequest'
import type { CleanUpCardRequest } from '@/api/generated/models/CleanUpCardRequest'
import type { CardItem } from '@/types/cardItem'
import { handleApiError } from '@/utils/errorHandler'

import PathConfigDialog from './components/PathConfigDialog.vue'
import CardFormDialog from './components/CardFormDialog.vue'

const loading = ref(false)
const tableData = ref<CardItem[]>([])
const showOnlyUnexpired = ref(true)

const searchForm = reactive({
    owner: '',
    cdk: '',
    timeRange: [] as string[],
})

const pagination = reactive({
    pageNumber: 1,
    pageSize: 10,
    total: 0
})

const pathDialogVisible = ref(false)
const cardDialogVisible = ref(false)
const isEditMode = ref(false)
const selectedCard = ref<CardItem>()


onMounted(() => {
    fetchData()
})

const fetchData = async (page = pagination.pageNumber) => {
    pagination.pageNumber = page
    loading.value = true
    try {
        const startTime = searchForm.timeRange?.length === 2 ? searchForm.timeRange[0] : undefined
        const endTime = searchForm.timeRange?.length === 2 ? searchForm.timeRange[1] : undefined
        const apiCall = showOnlyUnexpired.value
            ? CardNumberService.getApiCardUnexpiredcards(pagination.pageNumber, pagination.pageSize,
                searchForm.owner,
                searchForm.cdk,
                startTime,
                endTime,
            )
            : CardNumberService.getApiCardCards(pagination.pageNumber, pagination.pageSize,
                searchForm.owner,
                searchForm.cdk,
                startTime,
                endTime
            )

        const res = await apiCall
        tableData.value = res.items || []
        pagination.total = res.totalCount || 0
    }
    catch (error) {
        handleApiError(error, "获取卡号列表失败")
    }
    finally {
        loading.value = false
    }
}

const copyToClipboard = async (text: string) => {
    if (!text) return;
    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text)
        }
        else {
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.opacity = '0';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            document.execCommand('copy')
            document.body.removeChild(textArea)
        }
        ElMessage.success('卡号已复制到剪贴板')
    } catch (error) {
        handleApiError(error, '复制失败')
    }
}

const handleSearch = () => {
    fetchData(1)
}

const resetSearch = () => {
    searchForm.owner = ''
    searchForm.cdk = ''
    searchForm.timeRange = []
    fetchData(1)
}

const openCreateDialog = () => {
    isEditMode.value = false
    selectedCard.value = undefined
    cardDialogVisible.value = true
}

const openEditDialog = (row: CardItem) => {
    isEditMode.value = true
    selectedCard.value = { ...row }
    cardDialogVisible.value = true
}

const handleDelete = (row: CardItem) => {
    ElMessageBox.confirm(`确定要永久删除[${row.cdk}]吗？`, '删除警告', {
        type: 'warning',
        confirmButtonText: '确定删除',
        cancelButtonText: '取消'
    }).then(async () => {
        try {
            await CardNumberService.deleteApiCardDelete(row.id)
            ElMessage.success('删除成功')
            fetchData()
        } catch (error) {
            handleApiError(error, '删除失败')
        }
    }).catch(() => { })
}

const handleReissue = async (row: CardItem) => {
    try {
        const requestData: ReissueCardRequest = {
            cardId: row.id,
            cdk: row.cdk
        }
        await CardNumberService.postApiCardReissue(requestData)
        ElMessage.success('补发成功')
    } catch (error) {
        handleApiError(error, "补发卡号失败")
    }
}

const handleClear = async (row: CardItem) => {
    try {
        const requestData: CleanUpCardRequest = {
            cardId: row.id,
            cdk: row.cdk
        }
        await CardNumberService.postApiCardCleanup(requestData)
        ElMessage.success('清理成功')
    } catch (error) {
        handleApiError(error, '清理卡号失败')
    }
}

const isCardExpired = (endTimeStr: string) => {
    if (!endTimeStr) return false
    return new Date(endTimeStr).getTime() < Date.now()
}

const formatDate = (dateStr: string) => {
    const date = new Date(dateStr)
    if (isNaN(date.getTime())) return '-'
    return new Intl.DateTimeFormat('zh-CN', {
        timeZone: 'Asia/Shanghai',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
    })
        .format(date)
        .replace(/\//g, '-')
}
</script>

<style scoped>
.card-management-container {
    padding: 20px;
}

.filter-form .el-form-item {
    margin-bottom: 15px;
}

.copyable-tag {
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 6px;
    user-select: none;
    transition: all 0.2s ease;
}

.copyable-tag:hover {
    filter: brightness(0.95);
    transform: translateY(-1px);
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.copyable-tag .copy-icon {
    font-size: 14px;
    color: #909399;
    transition: color 0.2s;
}

.copyable-tag:hover .copy-icon {
    color: #409eff;
}

@media screen and (max-width: 768px) {
    .card-management-container {
        padding: 10px;
    }

    .filter-form {
        display: none !important
    }

    .filter-form .el-form-item {
        margin-right: 0 !important;
        margin-bottom: 12px;
        width: 100%;
    }

    .filter-form .el-form-item__content {
        width: 100%;
    }

    .filter-form .el-input,
    .filter-form .rl-date-editor {
        width: 100% !important;
    }

    .toolbar {
        flex-direction: column;
        align-items: stretch !important;
        gap: 12px !important;
    }

    .toolbar .el-switch {
        margin-left: 0 !important;
        align-self: flex-start;
    }

    .desktop-table {
        display: none !important;
    }

    .mobile-card-list {
        display: block !important;
    }
}

.mobile-card-list {
    display: none;
}
</style>