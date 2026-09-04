<template>
    <div class="user-management-container">
        <el-card shadow="hover">
            <div class="header-actions">
                <el-input id="searchUserName" name="searchUserName" class="search-user-name" v-model="searchUserName" placeholder="请输入人用户名搜索" clearable
                    @clear="handleSearch" @keyup.enter="handleSearch" />
                <el-button typep="primary" @click="handleSearch">搜索</el-button>
                <el-button @click="loadUsers">刷新</el-button>
            </div>

            <div class="desktop-table">
                <el-table :data="userList" v-loading="loading" class="user-table">
                    <el-table-column prop="username" label="用户名" width="150" />
                    <el-table-column prop="email" label="邮箱" width="180" />
                    <el-table-column  label="角色" width="180">
                        <template #default="{row}">
                            <el-tag v-for="role in row.roles" :key="role" :type="RoleTagTypeMap[role]"
                                effect="plain">
                                {{ RoleDisplayMap[role] }}
                            </el-tag>
                        </template>
                    </el-table-column>
                    <el-table-column label="状态" width="100">
                        <template #default="{ row }">
                            <el-tag :type="row.isLockedOut ? 'danger' : 'success'">
                                {{ row.isLockedOut ? '已锁定' : '正常' }}
                            </el-tag>
                        </template>
                    </el-table-column>
                    <el-table-column label="结束时间" width="180">
                        <template #default="{ row }">
                            {{ formatDate(row.lockoutEnd) }}
                        </template>
                    </el-table-column>
                    <el-table-column label="操作" min-width="280">
                        <template #default="{ row }">
                            <el-button size="small" type="warning" @click="openLockDialog(row)">
                                {{ row.isLockedOut ? '解锁' : '锁定' }}
                            </el-button>
                            <el-button size="small" type="primary" @click="openRoleDialog(row)">分配角色</el-button>
                            <el-button size="small" type="success" @click="openUpdateDialog(row)">编辑</el-button>
                            <el-button size="small" type="danger" @click="handleDelete(row.id)">删除</el-button>
                        </template>
                    </el-table-column>
                </el-table>
            </div>

            <div class="mobile-card-list" v-loading="loading">
                <div v-if="userList.length === 0 && !loading" class="moblie-list-empty">
                    暂无数据
                </div>
                <div v-for="row in userList" :key="row.id" class="mobile-card-item">
                    <div class="card-header">
                        <span class="username">{{ row.username || '未命名' }}</span>
                        <el-tag size="small" :type="row.isLockedOut ? 'danger' : 'success'">
                            {{ row.isLockedOut ? '已锁定' : '正常' }}
                        </el-tag>
                    </div>
                    <div class="card-body">
                        <div class="info-row">
                            <span class="label">邮箱：</span>
                            <span class="value">{{ row.email || '-' }}</span>
                        </div>
                        <div class="info-row">
                            <span class="label">结束时间：</span>
                            <span class="value">{{ formatDate(row.lockoutEnd) }}</span>
                        </div>
                    </div>
                    <div class="card-footer">
                        <el-button size="small" plain :type="row.isLockedOut ? 'success' : 'warning'"
                            @click="openLockDialog(row)">
                            {{ row.isLockedOut ? '解锁' : '锁定' }}
                        </el-button>
                        <el-button size="small" plain type="primary" @click="openRoleDialog(row)">分配角色</el-button>
                        <el-button size="small" plain type="success" @click="openUpdateDialog(row)">编辑</el-button>
                        <el-button size="small" plain type="danger" @click="handleDelete(row.id)">删除</el-button>
                    </div>
                </div>
            </div>

            <div class="pagination-container">
                <el-pagination v-model:current-page="pageNumber" v-model:page-size="pageSize" :page-sizes="[10, 20, 50]"
                    :total="totalCount" layout="total,sizes,prev,pager,next,jumper" @size-change="loadUsers"
                    @current-change="loadUsers" />
            </div>
        </el-card>

        <RoleEditDialog v-if="selectedUser" v-model:visible="roleEditVisible" :user-info="selectedUser" @success="loadUsers" />
        <UpdateUserDialog v-if="selectedUser" v-model:visible="updateUserVisible" :user-info="selectedUser" @success="loadUsers" />

    </div>
</template>

<script setup lang='ts'>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { AdminUserManagementService } from '@/api/generated/services/AdminUserManagementService'
import type { GetUserByNameRequest } from '@/api/generated/models/GetUserByNameRequest'
import type { ToggleLockRequest } from '@/api/generated/models/ToggleLockRequest'
import { handleApiError } from '@/utils/errorHandler'
import type { AdminUserInfo } from '@/types/user'
import { RoleDisplayMap, RoleTagTypeMap } from '@/utils/role'

import RoleEditDialog from './components/RoleEditDialog.vue'
import UpdateUserDialog from './components/UpdateUserDialog.vue'


const userList = ref<AdminUserInfo[]>([])
const loading = ref<boolean>(false)
const pageNumber = ref<number>(1)
const pageSize = ref<number>(10)
const totalCount = ref<number>(0)
const searchUserName = ref<string>('')

const selectedUser = ref<AdminUserInfo>()
const roleEditVisible = ref<boolean>(false)
const updateUserVisible = ref<boolean>(false)

const loadUsers = async () => {
    loading.value = true;
    try {
        const res = await AdminUserManagementService.getApiAdminUsers(pageNumber.value, pageSize.value)
        userList.value = res.items || res
        totalCount.value = res.totalCount || userList.value.length;
    } catch (error) {
        handleApiError(error, '获取用户列表失败')
    }
    finally {
        loading.value = false
    }
}

const handleSearch = async () => {
    if (!searchUserName.value.trim()) {
        loadUsers()
        return
    }
    loading.value = true
    try {
        const requestData: GetUserByNameRequest = {
            userName: searchUserName.value
        }
        const res = await AdminUserManagementService.getApiAdminUsersName(requestData)
        userList.value = Array.isArray(res) ? res : [res];
    } catch (error) {
        handleApiError(error, "搜索失败")
    }
    finally {
        loading.value = false
    }
}

const openLockDialog = async (row: AdminUserInfo) => {
    const newLockState = !row.isLockedOut
    try {
        const requestData: ToggleLockRequest = {
            lockUser: newLockState
        }
        await AdminUserManagementService.putApiAdminUsersLock(row.id, requestData)
        ElMessage.success(newLockState ? '已锁定用户' : '已解锁用户')
        loadUsers();
    } catch (error) {
        handleApiError(error, "切换状态失败")
    }
}

const openUpdateDialog = (row: AdminUserInfo) => {
    selectedUser.value = { ...row }
    updateUserVisible.value = true
}

const openRoleDialog = async (row: AdminUserInfo) => {
    selectedUser.value = { ...row }
    roleEditVisible.value = true
}


const handleDelete = (userId: string) => {
    ElMessageBox.confirm('确定要彻底删除该用户吗？', '警告', {
        confirmButtonType: 'danger',
    }).then(async () => {
        try {
            await AdminUserManagementService.deleteApiAdminUsers(userId);
            ElMessage.success('删除成功');
            loadUsers();
        } catch (error) {
            handleApiError(error, '删除用户失败')
        }
    }).catch(() => { })
}


const formatDate = (dateStr?: string) => {
    if (!dateStr) return '-'
    const date = new Date(dateStr)
    return isNaN(date.getTime()) ? dateStr : date.toLocaleString()
}

onMounted(() => {
    loadUsers();
})
</script>

<style scoped>
.user-management-container {
    padding: 20px;
}

.serach-user-name {
    width: 250px;
    margin-right: 15px;
}

.user-table {
    width: 100%;
    margin-top: 20px;
}


.header-actions {
    display: flex;
    align-items: center;
}

.search-input {
    width: 250px;
    margin-right: 15px;
}

.pagination-container {
    margin-top: 20px;
    display: flex;
    justify-content: flex-end;
    overflow-x: auto;
}

.desktop-table {
    display: block;
}

.mobile-card-list {
    display: none;
}

@media screen and (max-width: 768px) {
    .user-management-container {
        padding: 10px;
    }

    .desktop-table {
        display: none;
    }

    .mobile-card-list {
        display: flex;
        flex-direction: column;
        gap: 12px;
        margin-top: 15px;
    }

    .filter-form {
        flex-direction: column;
        align-items: stretch;
        gap: 10px;
    }

    .search-input {
        width: 100%;
        margin-right: 0;
    }


    .mobile-list-empty {
        text-align: center;
        color: #909399;
        padding: 30px 0;
    }

    .action-buttons {
        display: flex;
        gap: 10px;
    }

    .action-buttons .el-button {
        flex: 1;
        margin: 0;
    }

    .mobile-card-item {
        background: #fdfdfd;
        border: 1px solid #ebeef5;
        border-radius: 8px;
        padding: 12px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
    }

    .card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 8px;
        padding-bottom: 8px;
        border-bottom: 1px solid #f2f6fc;
    }

    .card-header .username {
        font-weight: bold;
        font-size: 15px;
        color: #303133;
    }

    .card-body {
        display: flex;
        flex-direction: column;
        gap: 6px;
        margin-bottom: 10px
    }

    .info-row {
        display: flex;
        justify-content: space-between;
        font-size: 13px
    }

    .info-row .label {
        color: #909399
    }

    .info-row .value {
        color: #606266;
        word-break: break-all;
        text-align: right
    }

    .card-footer {
        display: flex;
        justify-content: flex-end;
        gap: 6px
    }

    .card-footer .el-button {
        margin-left: 0;
    }
}
</style>