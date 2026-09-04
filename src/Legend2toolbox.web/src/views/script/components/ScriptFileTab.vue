<template>
    <div>
        <div class="tab-toolbar">
            <el-button type="primary" :icon="DocumentAdd" @click="openCreateModal">添加脚本文件</el-button>
        </div>

        <el-table :data="fileList" v-loading="loading">
            <el-table-column prop="fileName" label="脚本文件名" />
            <el-table-column prop="filePath" label="相对路径" />
            <el-table-column prop="type" label="类型" width="120">
                <template #default="{ row }">
                    <el-tag :type="row.type === 1 ? 'info' : 'warning'">
                        {{ row.type === 1 ? '全量替换' : '片段注入' }}
                    </el-tag>
                </template>
            </el-table-column>
            <el-table-column label="操作" width="150" align="center">
                <template #default="{ row }">
                    <el-button link type="primary" @click="openEditModal(row)">编辑</el-button>
                    <el-button link type="danger" @click="handleDelete(row.id)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>

        <el-dialog v-model="showModal" :title="isEdit ? '编辑脚本文件' : '新建脚本文件'" width="850px"
            :close-on-click-modal="false">
            <el-form :model="form" label-width="110px" label-position="left">
                <el-form-item label="脚本文件类型" required>
                    <el-radio-group v-model="form.type" :disabled="isEdit">
                        <el-radio-button :value="1">全量脚本</el-radio-button>
                        <el-radio-button :value="2">片段脚本</el-radio-button>
                    </el-radio-group>
                </el-form-item>
                <el-form-item label="文件名">
                    <el-input id="fileName" name="fileName" v-model="form.fileName" placeholder="例如：QFunction-0.txt" @input="updateEditorLanguage" />
                </el-form-item>
                <el-form-item label="文件路径">
                    <el-input id="filePath" name="filePath" v-model="form.filePath" placeholder="例如：Mir200/Envir/Market_Def/" />
                </el-form-item>

                <el-form-item v-if="form.type === 1" label="完整脚本内容">
                    <div class="editor-container">
                        <vue-monaco-editor v-model:value="form.wholeContent" :language="editorLanguage" theme="vs-dark"
                            :options="editorOptions" height="380px" />
                    </div>
                </el-form-item>

                <el-form-item v-else label="脚gg本片段配置">
                    <div class="segment-wrapper">
                        <div v-for="(segment, index) in form.segments" :key="index" class="segment-card">
                            <div class="segment-header">
                                <span class="segment-title">片段 #{{ index + 1 }}</span>
                                <el-button type="danger" link :icon="Delete" @click="removeSegment(index)">
                                    删除片段
                                </el-button>
                            </div>

                            <el-form-item label="触发标识" label-width="80px" class="segment-input">
                                <el-input id="triggerField" name="triggerField" v-model="segment.triggerField"
                                    placeholder="例如：[@PlayDie] 或 @Main (特殊标识：#top{文件头部}，#bottom{文件末尾})" />
                            </el-form-item>

                            <el-form-item label="片段内容" label-width="80px" class="segment-input">
                                <div class="editor-container">
                                    <vue-monaco-editor v-model:value="segment.content" :language="editorLanguage"
                                        theme="vs-dark" :options="editorOptions" height="180px" />
                                </div>
                            </el-form-item>
                        </div>
                        <el-button :icon="Plus" class="add-segment-btn" @click="addSegment">
                            添加新片段
                        </el-button>
                    </div>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="showModal = false">取消</el-button>
                <el-button type="primary" :loading="saveLoading" @click="handleSave">保存</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang='ts'>
import { ref, watch, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { DocumentAdd, Delete, Plus } from '@element-plus/icons-vue'
import { ScriptFileService } from '@/api/generated/services/ScriptFileService'
import { handleApiError } from '@/utils/errorHandler'

import { VueMonacoEditor } from '@guolao/vue-monaco-editor'

interface ScriptSegmentDto {
    triggerField: string
    content: string
}

const props = defineProps<{ scriptSetId: string }>()
const loading = ref<boolean>(false)
const saveLoading = ref<boolean>(false)
const showModal = ref<boolean>(false)
const isEdit = ref<boolean>(false)
const editingId = ref<string | null>(null)
const fileList = ref<any[]>([])

const editorLanguage = ref('ini')

const form = ref<{
    type: number
    fileName: string
    filePath: string
    wholeContent: string
    segments: ScriptSegmentDto[]
}>({
    type: 1,
    fileName: '',
    filePath: '',
    wholeContent: '',
    segments: []
})

const editorOptions = {
    automaticLayout: true,
    formatOnPaste: true,
    minimap: { enabled: false },
    scrollBeyondLastLine: false,
    fontSize: 14,
    fontFamily: 'consolas, "Courier New", monospace',
    tabSize: 4
}

const openCreateModal = () => {
    isEdit.value = false
    editingId.value = null
    form.value = {
        type: 1,
        fileName: '',
        filePath: '',
        wholeContent: '',
        segments: []
    }
    updateEditorLanguage()
    showModal.value = true
}

const openEditModal = async (row: any) => {
    isEdit.value = true
    editingId.value = row.id
    loading.value = true
    try {
        const detail = await ScriptFileService.getApiScriptsFiles(row.id);
        form.value = {
            type: detail.type,
            fileName: detail.fileName || '',
            filePath: detail.filePath || '',
            wholeContent: detail.wholeContent || '',
            segments: detail.segments ? JSON.parse(JSON.stringify(detail.segments)) : []
        }
        updateEditorLanguage()
        showModal.value = true
    } catch (err: any) {
        handleApiError(err, "获取文件详情失败")
    } finally {
        loading.value = false
    }
}

const addSegment = () => {
    form.value.segments.push({
        triggerField: '',
        content: ''
    })
}

const removeSegment = (index: number) => {
    form.value.segments.splice(index, 1)
}

const updateEditorLanguage = () => {
    const fileName = form.value.fileName.toLowerCase();
    if (fileName.endsWith('.lua')) {
        editorLanguage.value = 'lua';
    } else if (fileName.endsWith('.json')) {
        editorLanguage.value = 'json'
    } else if (fileName.endsWith('.sql')) {
        editorLanguage.value = 'sql'
    } else if (fileName.endsWith('.ini')) {
        editorLanguage.value = 'ini'
    } else {
        editorLanguage.value = 'ini'
    }
}

const fetchFiles = async () => {
    if (!props.scriptSetId) return
    loading.value = true;
    try {
        const res = await ScriptFileService.getApiScriptsFilesBySet(props.scriptSetId)
        fileList.value = res || []
    } catch (err: any) {
        handleApiError(err, "获取文件失败")
    } finally {
        loading.value = false
    }
}

const handleSave = async () => {
    if (!form.value.fileName || !form.value.filePath) {
        return ElMessage.warning("请填写文件名与路径")
    }
    if (form.value.type === 2 && form.value.segments.length === 0) {
        return ElMessage.warning("片段模式下至少需要添加一个脚本片段")
    }
    saveLoading.value = true
    try {
        const payload = {
            scriptSetId: props.scriptSetId,
            fileName: form.value.fileName,
            filePath: form.value.filePath,
            type: form.value.type as any,
            wholeContent: form.value.type === 1 ? form.value.wholeContent : null,
            segments: form.value.type === 2 ? form.value.segments : null
        }
        if (isEdit.value && editingId.value) {
            await ScriptFileService.putApiScriptsFiles(
                editingId.value,
                payload
            )
            ElMessage.success("修改成功")
        } else {
            await ScriptFileService.postApiScriptsFiles(payload
            )
            ElMessage.success("保存成功")
        }
        showModal.value = false;
        fetchFiles();
    } catch (err: any) {
        handleApiError(err, "保存失败")
    } finally {
        saveLoading.value = false
    }
}

const handleDelete = async (id: string) => {
    try {
        await ElMessageBox.confirm('确定要删除该脚本文件吗？此操作不可逆!', '警告', {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
        })
        loading.value = true
        await ScriptFileService.deleteApiScriptsFiles(id)
        ElMessage.success("已删除")
        fetchFiles()
    } catch (err: any) {
        handleApiError(err, '删除失败')
    } finally {
        loading.value = false
    }
}

watch(() => props.scriptSetId, () => fetchFiles())
onMounted(() => fetchFiles())
</script>

<style scoped>
.tab-toolbar {
    margin-bottom: 16px;
}

.editor-container {
    border: 1px solid #dcdfe6;
    border-radius: 4px;
    overflow: hidden;
    width: 100%;
}

.segment-wrapper {
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.segment-card {
    border: 1px border #e4e7ed;
    background-color: #f8f9fa;
    border-radius: 6px;
    padding: 12px;
}

.segment-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 12px;
    padding-bottom: 8px;
    border-bottom: 1px dashed #dcdfe6;
}

.segment-title {
    font-weight: bold;
    color: #409eff;
}

.segment-input {
    margin-bottom: 12px !important;
}

.add-segment-btn {
    width: 100%;
    margin-top: 8px;
    border-style: dashed;
}

.el-form-item {
    margin-bottom: 20px;
}
</style>