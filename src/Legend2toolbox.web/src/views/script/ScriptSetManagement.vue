<template>
    <div class="app-container">
        <el-card header="脚本套管理">
            <div class="toolbar">
                <el-button type="primary" :icon="Plus" @click="openCreateSetModal">新建脚本套</el-button>
                <el-button :icon="Refresh" @click="fetchScriptSets">刷新</el-button>
            </div>

            <el-table :data="scriptSetList" v-loading="loading" highlight-current-row @current-change="handleSelectSet">
                <el-table-column prop="name" label="脚本套名称" />
                <el-table-column prop="description" label="描述" />
                <el-table-column label="操作" width="180" align="center">
                    <template #default="{ row }">
                        <el-button link type="primary" @click.stop="openEditSetModal(row)">编辑</el-button>
                        <el-button link type="danger" @click.stop="handleDeleteSet(row.id)">删除</el-button>
                    </template>
                </el-table-column>
            </el-table>

            <el-pagination v-model:current-page="pageNumber" v-model:page-size="pageSize" :total="total"
                layout="total,prev,pager,next" class="pagination" @current-change="fetchScriptSets" />
        </el-card>

        <el-card v-if="currentSet" class="detail-card">
            <template #header>
                <span>当前选中的脚本套:<strong>{{ currentSet.name }}</strong></span>
            </template>

            <el-tabs v-model="activeTab">
                <el-tab-pane label="脚本文件" name="scriptFiles">
                    <ScriptFileTab :script-set-id="currentSet.id" />
                </el-tab-pane>

                <el-tab-pane label="素材文件" name="materialFiles">
                    <MaterialFileTab :script-set-id="currentSet.id" />
                </el-tab-pane>

                <el-tab-pane label="数据库追加数据" name="dbDatas">
                    <ScriptSetDbDataTab :script-set-id="currentSet.id" />
                </el-tab-pane>
            </el-tabs>
        </el-card>
        
        <el-dialog v-model="showSetModal" :title="isEdit ? '编辑脚本套' : '新建脚本套'" width="500px">
            <el-form :model="setForm" label-width="100px">
                <el-form-item label="名称" required>
                    <el-input id="name" name="name" v-model="setForm.name" placeholder="请输入脚本套名称"/>
                </el-form-item>
                <el-form-item label="描述">
                    <el-input id="description" name="description" v-model="setForm.description" type="textarea" :row="3" placeholder="请输入脚本套描述"/>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="showSetModal = false">取消</el-button>
                <el-button type="primary" :loading="submitting" @click="handleSaveSet">保存</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang='ts'>
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus';
import { Refresh, Plus } from '@element-plus/icons-vue'
import { ScriptSetService } from '@/api/generated/services/ScriptSetService'
import type {UpdateScriptSetRequest} from '@/api/generated/models/UpdateScriptSetRequest'
import ScriptFileTab from './components/ScriptFileTab.vue'
import MaterialFileTab from './components/MaterialFileTab.vue'
import ScriptSetDbDataTab from './components/ScriptSetDbDataTab.vue'
import { handleApiError } from '@/utils/errorHandler'

const loading = ref<boolean>(false)
const submitting = ref<boolean>(false)
const scriptSetList = ref<any[]>([])
const currentSet = ref<any>(null)
const activeTab = ref<string>('scriptFiles')

const pageNumber = ref<number>(1)
const pageSize = ref<number>(10)
const total = ref<number>(0)

const showSetModal = ref<boolean>(false)
const isEdit = ref<boolean>(false)
const setForm = ref({
    id: '',
    name: '',
    description: ''
})

const fetchScriptSets = async () => {
    loading.value = true
    try {
        const res = await ScriptSetService.getApiScriptsSets(pageNumber.value, pageSize.value);
        scriptSetList.value = res.items || []
        total.value = res.totalCount || 0
    } catch (err: any) {
        handleApiError(err, '获取脚本套失败')
    } finally {
        loading.value = false
    }
}

const handleSelectSet = (row: any) => {
    if (row) currentSet.value = row
}

const handleDeleteSet = (id: string) => {
    ElMessageBox.confirm('确定要删除该脚本套吗？', '提示', { type: 'warning' }).then(async () => {
        try {
            await ScriptSetService.deleteApiScriptsSets( id )
            ElMessage.success('成功')
            if (currentSet.value?.id === id) currentSet.value = null;
            fetchScriptSets()
        }
        catch (err: any) {
            handleApiError(err, '删除脚本套失败')
        }
    })
}

const openCreateSetModal = () => { 
    isEdit.value= false;
    setForm.value = {id: '', name:'', description:''};
    showSetModal.value=  true
}

const openEditSetModal = (row: any) => { 
    isEdit.value = true
    setForm.value = {
        id: row.id,
        name: row.name,
        description: row.desctription || ''
    }
    showSetModal.value = true
}

const handleSaveSet = async () =>{
    if(!setForm.value.name.trim()){
        return ElMessage.warning("脚本套名称不能为空")
    }
    submitting.value = true
    try{
        if(isEdit.value){
        const requestData : UpdateScriptSetRequest = {
            name: setForm.value.name,
            description : setForm.value.description
        }
            await ScriptSetService.putApiScriptsSets(setForm.value.id, 
                requestData
            )
            ElMessage.success("更新成功")
        }else{
            await ScriptSetService.postApiScriptsSets({
                name: setForm.value.name,
                description: setForm.value.description
            })
            ElMessage.success("创建成功")
        }
        showSetModal.value = false
        fetchScriptSets()
    }catch(err: any){
        handleApiError(err,"操作失败")
    }finally{
        submitting.value = false
    }
}


onMounted(() => {
    fetchScriptSets();
})
</script>

<style scoped>
.app-container {
    padding: 20px;
}

.toolbar {
    margin-bottom: 16px;
}

.pagination {
    margin-top: 16px;
    display: flex;
    justify-content: flex-end;
}

.detail-card {
    margin-top: 20px;
}
</style>