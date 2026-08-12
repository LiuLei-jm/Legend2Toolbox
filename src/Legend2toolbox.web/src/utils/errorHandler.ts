import {ElMessage} from 'element-plus'

export const handleApiError = (error: unknown, fallbackMessage: string = '操作失败，请稍后重试') => {
  const apiError = error as any

  const errorData = apiError?.body ?? apiError?.response?.data;
  
  console.log('handleApiError errorData:', errorData)
  
  if(!errorData){
    ElMessage.error(fallbackMessage)
    return
  }
  
  if(errorData.defail){
    ElMessage.error(errorData.detail)
    return
  }
  
  if(errorData.errors && typeof errorData.errors === 'object'){
    const messages = Object.values(errorData.errors)
    .flat()
    .filter(Boolean)
    
    if(messages.length > 0){
      ElMessage.error(messages.join("; "))
      return
    }
  }
  
  if(errorData.title){
    ElMessage.error(errorData.title)
    return
  }

  ElMessage.error(fallbackMessage);
}
