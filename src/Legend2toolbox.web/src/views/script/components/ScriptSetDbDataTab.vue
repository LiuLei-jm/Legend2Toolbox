<template>
    <div>
        <div class="tab-toolbar">
            <el-button type="primary" :icon="Plus" @click="openCreateModal">新增数据库追加数据</el-button>
        </div>
        <el-table :data="dbDataList" v-loading="loading">
            <el-table-column prop="tableType" label="表类型" width="160">
                <template #default="{ row }">
                    <el-tag>{{ getTableTypeName(row.tableType) }}</el-tag>
                </template>
            </el-table-column>
            <el-table-column prop="name" label="名称" width="180" />
            <el-table-column prop="dataJson" label="数据内容(JSON/SQL)" show-overflow-tooltip />
            <el-table-column label="操作" width="150" align="center">
                <template #default="{ row }">
                    <el-button link type="primary" @click="openEditModal(row)">编辑</el-button>
                    <el-button link type="danger" @click="handleDelete(row.id)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>

        <el-dialog v-model="showModal" :title="isEdit ? '编辑 DB 追加数据' : '新建 DB 追加数据'" width="600px">
            <el-form :model="form" label-width="110px">
                <el-form-item label="数据库类型" required>
                    <el-select v-model="form.tableType" placeholder="请选择">
                        <el-option label="StdItems" :value="GameDbTableType._1" />
                        <el-option label="Monster" :value="GameDbTableType._2" />
                        <el-option label="Magic" :value="GameDbTableType._3" />
                    </el-select>
                </el-form-item>
                <el-form-item label="名称" required>
                    <el-input id="name" name="name" v-model="form.name" placeholder="例如：物品数据配置" />
                </el-form-item>
                <el-form-item label="数据内容" required>
                    <div class="kv-container">
                        <div v-for="(item, index) in kvList" :key="index" class="kv-row">
                            <el-input id="key" name="key" v-model="item.key" placeholder="键（Key）" class="kv-input" />
                            <span class="kv-colon">:</span>
                            <el-input id="value" name="value" v-model="item.value" placeholder="值（Value）" class="kv-input" />
                            <el-button link type="danger" :icon="Delete" @click="removeKvRow(index)" title="删除该行" />
                        </div>
                        <el-button type="primary" link :icon="Plus" @click="addKvRow" class="add-row-btn">
                            添加一行键值对
                        </el-button>
                    </div>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="showModal = false">取消</el-button>
                <el-button type="primary" :loading="submitting" @click="handleSave">保存</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang='ts'>
import { ref, watch, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Delete } from '@element-plus/icons-vue'
import { ScriptSetDbDataService } from '@/api/generated/services/ScriptSetDbDataService'
import { handleApiError } from '@/utils/errorHandler'
import { GameDbTableType } from '@/api/generated/models/GameDbTableType';

interface KeyValuePair {
    key: string
    value: string
}

const props = defineProps<{ scriptSetId: string }>()

const loading = ref<boolean>(false);
const submitting = ref<boolean>(false)
const showModal = ref<boolean>(false)
const isEdit = ref<boolean>(false)
const dbDataList = ref<any[]>([])

const kvList = ref<KeyValuePair[]>([])

const form = ref<{
    id: string,
    tableType: GameDbTableType,
    name: string,
}>({
    id: '',
    tableType: GameDbTableType._1,
    name: '',
})

const getTableTypeName = (type: GameDbTableType) => {
    const map: Record<number, string> = {
        [GameDbTableType._1]: 'StdItems',
        [GameDbTableType._2]: 'Monster',
        [GameDbTableType._3]: 'Magic',
    }
    return map[type] || `未知类型(${type})`
}

const createEmptyKvPairs = (count: number = 5): KeyValuePair[] => {
    return Array.from({ length: count }, () => ({ key: '', value: '' }))
}

const fetchDbDatas = async () => {
    if (!props.scriptSetId) return;
    loading.value = true;
    try {
        const res = await ScriptSetDbDataService.getApiScriptsDbDatasBySet(props.scriptSetId);
        dbDataList.value = res || []
    } catch (err: any) {
        handleApiError(err, "加载数据列表失败")
    } finally {
        loading.value = false
    }
}

const openCreateModal = () => {
    isEdit.value = false
    form.value = {
        id: '',
        tableType: GameDbTableType._1,
        name: '',
    }
    kvList.value = createEmptyKvPairs(5)
    showModal.value = true;
}

const openEditModal = (row: any) => {
    isEdit.value = true
    form.value = {
        id: row.id,
        tableType: row.tableType ?? GameDbTableType._1,
        name: row.name || ''
    }
    let parsedPairs: KeyValuePair[] = []
    if (row.dataJson) {
        try {
            const parsed = JSON.parse(row.dataJson)
            if (typeof parsed === 'object' && parsed !== null && !Array.isArray(parsed)) {
                parsedPairs = Object.entries(parsed).map(([k, v]) => ({
                    key: k,
                    value: typeof v === 'object' ? JSON.stringify(v) : String(v)
                }))
            }
        } catch {

        }
    }

    if (parsedPairs.length < 5) {
        parsedPairs.push(...createEmptyKvPairs(5 - parsedPairs.length))
    }

    kvList.value = parsedPairs
    showModal.value = true
}

const addKvRow = () => {
    kvList.value.push({ key: '', value: '' })
}

const removeKvRow = (index: number) => {
    kvList.value.splice(index, 1)
}

const handleSave = async () => {
    if (!form.value.name.trim()) {
        return ElMessage.warning("名称不能为空")
    }
    const validPairs = kvList.value.filter(item => item.key.trim() !== '')
    if (validPairs.length === 0) {
        return ElMessage.warning("至少需要写一组有效的 Key")
    }

    const payloadObj: Record<string, string> = {}
    validPairs.forEach(pair => {
        payloadObj[pair.key.trim()] = pair.value
    })

    const dataJson = JSON.stringify(payloadObj)

    submitting.value = true
    try {
        if (isEdit.value) {
            await ScriptSetDbDataService.putApiScriptsDbDatas(form.value.id, {
                tableType: form.value.tableType,
                name: form.value.name,
                dataJson: dataJson
            })
            ElMessage.success('修改成功')
        } else {
            await ScriptSetDbDataService.postApiScriptsDbDatas({
                scriptSetId: props.scriptSetId,
                tableType: form.value.tableType,
                name: form.value.name,
                dataJson: dataJson
            })
            ElMessage.success("创建成功")
        }
        showModal.value = false
        fetchDbDatas()
    } catch (err: any) {
        handleApiError(err, "保存失败")
    } finally {
        submitting.value = false
    }
}

const handleDelete = (id: string) => {
    ElMessageBox.confirm("确定要删除该条数据库数据吗？", '警告', { type: 'warning' }).then(async () => {
        try {
            await ScriptSetDbDataService.deleteApiScriptsDbDatas(id)
            ElMessage.success("删除成功")
            fetchDbDatas();
        } catch (err: any) {
            handleApiError(err, "删除失败")
        }
    })
}

watch(() => props.scriptSetId, () => fetchDbDatas())
onMounted(() => fetchDbDatas())
</script>

<style scoped>
.tab-toolbar {
    margin-bottom: 12px;
}

.kv-container {
    width: 100%;
}

.kv-row {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 8px;
}
</style>