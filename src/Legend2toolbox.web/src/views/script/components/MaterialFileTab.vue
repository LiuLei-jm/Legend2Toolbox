<template>
  <div>
    <div class="tab-toolbar">
      <el-button type="primary" :icon="Upload" @click="openUploadDialog">上传素材</el-button>
    </div>
    <el-table :data="materialList" v-loading="loading">
      <el-table-column prop="fileName" label="文件名" />
      <el-table-column prop="targetPath" label="安装路径" />
      <el-table-column prop="fileSize" label="大小 (Bytes)" width="120" />
      <el-table-column prop="password" label="密码" width="120" />
      <el-table-column label="操作" width="120" align="center">
        <template #default="{ row }">
          <el-button link type="primary" @click="openEditDialog(row)">编辑</el-button>
          <el-button link type="danger" @click="handleDelete(row.id)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="showDialog" :title="isEdit ? '编辑素材文件' : '上传素材文件'" width="500px">
      <el-form :model="form" label-width="120px">
        <el-form-item :label="isEdit ? '更换文件' : '选择文件'" required>
          <input type="file" @change="onFileChange" />
          <div v-if="isEdit" class="tip-text">若不修改文件内容可留空</div>
        </el-form-item>
        <el-form-item label="部署放置路径" required>
          <el-input id="targetPath" name="targetPath" v-model="form.targetPath" placeholder="例如：Data/Map.pak" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input id="password" name="password" v-model="form.password" placeholder="无密码可留空" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showDialog = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang='ts'>
import { ref, watch, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Upload } from '@element-plus/icons-vue'
import { MaterialFileService } from '@/api/generated/services/MaterialFileService'
import { handleApiError } from '@/utils/errorHandler'

const props = defineProps<{ scriptSetId: string }>()

const loading = ref<boolean>(false)
const submitting = ref<boolean>(false)
const showDialog = ref<boolean>(false)
const isEdit = ref<boolean>(false)
const editingId = ref<string>('')

const materialList = ref<any[]>([])
const selectedFile = ref<File | null>(null)
const fileInputRef = ref<HTMLInputElement | null>()


const form = ref({
  targetPath: 'Data/',
  password: '',
})

const fetchMaterials = async () => {
  if (!props.scriptSetId) return
  loading.value = true
  try {
    const res = await MaterialFileService.getApiScriptsMaterialBySet(props.scriptSetId)
    materialList.value = res || []
  } catch (err: any) {
    handleApiError(err, "获取素材列表失败")
  } finally {
    loading.value = false
  }
}

const onFileChange = (e: Event) => {
  const target = e.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    selectedFile.value = target.files[0] ?? null
  }
}

const resetFileInput = () => {
  selectedFile.value = null
  if (fileInputRef.value) {
    fileInputRef.value.value = ''
  }
}

const openUploadDialog = () => {
  isEdit.value = false
  editingId.value = ''
  form.value = {
    targetPath: 'Data/',
    password: ''
  }
  resetFileInput()
  showDialog.value = true
}

const openEditDialog = (row: any) => {
  isEdit.value = true
  editingId.value = row.id
  form.value = {
    targetPath: row.targetPath || 'Data/',
    password: row.password || ''
  }
  resetFileInput()
  showDialog.value = true
}

const handleSubmit = async () => {
  if (!isEdit.value && !selectedFile.value) {
    return ElMessage.warning("请选择文件")
  }

  submitting.value = true;
  try {
    if (isEdit.value) {
      await MaterialFileService.putApiScriptsMaterial(editingId.value, {
        scriptSetId: props.scriptSetId,
        file: selectedFile.value as any,
        targetPath: form.value.targetPath,
        password: form.value.password || '',
      } as any)
      ElMessage.success("修改成功")
    } else {
      await MaterialFileService.postApiScriptsMaterial({
        scriptSetId: props.scriptSetId,
        file: selectedFile.value,
        targetPath: form.value.targetPath,
        password: form.value.password || '',
        fileSize: selectedFile.value ? selectedFile.value.size : undefined
      } as any)
      ElMessage.success("上传成功")
    }

    showDialog.value = false
    fetchMaterials()
  } catch (err: any) {
    handleApiError(err, isEdit.value ? "修改失败" : "上传失败")
  } finally {
    submitting.value = false
  }
}

const handleDelete = (id: string) => {
  ElMessageBox.confirm('确定要删除此素材文件吗？', '警告', { type: 'warning' }).then(async () => {
    try {
      await MaterialFileService.deleteApiScriptsMaterial(id)
      ElMessage.success("删除成功")
      fetchMaterials()
    } catch (err: any) {
      handleApiError(err, '删除失败')
    }
  })
}

watch(() => props.scriptSetId, () => fetchMaterials())
onMounted(() => fetchMaterials())
</script>

<style scoped>
.tab-toolbar {
  margin-bottom: 12px;
}

.tip-text {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
}
</style>