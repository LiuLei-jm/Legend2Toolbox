import {ElMessage} from 'element-plus'

export const handleApiError = (error: any , fallbackMessage: string = '操作失败，请稍后重试')=> {
    const errorData = error.body || error.response?.data;
    if(errorData){
        if(errorData.errors && typeof errorData.errors === 'object'){
            const errorMessages = Object.values(errorData.errors)
            .flat()
            .join('<br/>');
            ElMessage({
                type: 'error',
                dangerouslyUseHTMLString:true,
                message: errorMessages || "输入数据校验失败"
            });
            return ;
        }
        const businessMessage = errorData.detail || errorData.message || errorData.title;
        if(businessMessage){
            ElMessage.error(businessMessage);
            return;
        }
    }
    ElMessage.error(fallbackMessage);
}