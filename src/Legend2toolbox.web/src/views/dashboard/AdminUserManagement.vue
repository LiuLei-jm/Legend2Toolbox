<template>
    <div class="user-management-container">
        <el-card shadow="hover">
            <div class="header-actions">
                <el-input v-model="searchUserName" placeholder="请输入人用户名搜索" clearable
                    style="width:250px; margin-right: 15px;" @clear="handleSearch" @keyup.enter="handleSearch" />
                <el-button typep="primary" @click="handleSearch">搜索</el-button>
                <el-button @click="loadUsers">刷新</el-button>
            </div>

            <el-table :data="userList" v-loading="loading" style="width:100%;margin-top: 20px;">
                <el-table-column prop="id" label="用户 ID" width="220" />
                <el-table-column prop="userName" label="用户名" width="150" />
                <el-table-column prop="email" label="邮箱" width="180" />
                <el-table-column label="状态" width="100">
                    <template #default="{ row }">
                        <el-tag :type="row.isLocked ? 'daner' : 'success'">
                            {{ row.isLocked ? '已锁定' : '正常' }}
                        </el-tag>
                    </template>
                </el-table-column>
                <el-table-column label="操作" min-width="280">
                    <template #default="{row}">
                        <el-button size="small" type="warning" @click="openLockDialog(row)">
                            {{ row.isLocked ? '解锁' : '锁定'}}
                        </el-button>
                        <el-button size="small" type="primary" @click="openRoleDialog(row)">分配角色</el-button>
                        <el-button size="small" type="success" @click="openUpdateDialog(row)">编辑</el-button>
                        <el-button size="small" type="danger" @click="handleDelete(row.id)">删除</el-button>
                    </template>
                </el-table-column>
            </el-table>
            
            <div class="pagination-container" style="margin-top: 20px; text-align: right;" >
                <el-pagination
                v-model:current-page="pageNumber"
                        v-model:page-size="pageSize"
                        :page-sizes="[10,20,50]"
                        :total="totalCount"
                        layout="total,sizes,prev,pager,next,jumper"
                        @size-change="loadUsers"
                        @current-change="loadUsers"/>
            </div>
        </el-card>
        
    </div>
</template>

<script setup lang='ts'>
    import {ref,reactive} from 'vue'
    import {ElMessage, ElMessageBox} from 'element-plus'
    import {AdminUserManagemenetService} from '@/api/generated/services/AdminUserManagementService'
    
    const userList = ref([])
    const loading = ref(false)
    const pageNumber = ref(1)
    const pageSize = ref(10)
    const totalCount = ref(0)
    const searchUserName = ref('')
    
    const updateDialogVisible = ref(false);
    const currentUserId = ref('')
    const updateForm = ref({userName: '', email: ''})
    
    
</script>

<style scoped></style>