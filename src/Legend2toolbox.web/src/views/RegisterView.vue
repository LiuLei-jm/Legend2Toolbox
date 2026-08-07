<template>
  <div class="register-layout">
    <div class="bg-shape shape-1"></div>
    <div class="bg-shape shape-2"></div>

    <div class="register-container">
      <el-card class="glass-card" :body-style="{ padding: '24px 28px' }">
        <div class="header-section">
          <div class="logo-box">
            <el-icon :size="16" color="#409EFF">
              <Platform />
            </el-icon>
          </div>
          <h2 class="title">注册账号</h2>
        </div>

        <!-- 注册表单 -->
        <el-form ref="registerFormRef" :model="formData" :rules="formRules" label-width="80px" size="default"
          @keyup.enter="handleRegister" class="modern-form">
          <el-form-item label="用户名" prop="username">
            <el-input v-model="formData.username" placeholder="请输入用户名" clearable :prefix-icon="User" />
          </el-form-item>

          <el-form-item label="邮箱" prop="email">
            <el-input v-model="formData.email" placeholder="请输入邮箱" clearable :prefix-icon="Message" />
          </el-form-item>

          <el-form-item label="密码" prop="password">
            <el-input v-model="formData.password" type="password" placeholder="请输入密码" show-password
              :prefix-icon="Lock" />
          </el-form-item>

          <el-form-item label="确认密码" prop="confirmPassword">
            <el-input v-model="formData.confirmPassword" type="password" placeholder="请再次输入密码" show-password
              :prefix-icon="Key" />
          </el-form-item>

          <el-form-item label-width="0" class="btn-form-item">
            <el-button type="primary" class="submit-btn" :loading="isLoading" @click="handleRegister">
              立即注册
            </el-button>
          </el-form-item>

          <div class="login-link">
            已有账号？
            <el-link type="primary" underline="hover" @click="goToLogin">去登录 <el-icon class="el-icon--right">
                <ArrowRight />
              </el-icon></el-link>
          </div>
        </el-form>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { ElMessage } from "element-plus";
import type { FormInstance, FormRules } from "element-plus";
import type { RuleItem } from 'async-validator'
import { useAuthStore } from "@/stores/auth"
import { useRouter} from 'vue-router'

import { User, Message, Lock, Key, Platform, ArrowRight } from "@element-plus/icons-vue";

import type { RegisterRequest } from "@/api/generated/models/RegisterRequest";

const authStore = useAuthStore();
const registerFormRef = ref<FormInstance>();
const isLoading = ref(false);

const router = useRouter();

const formData = reactive({
  username: "",
  email: "",
  password: "",
  confirmPassword: "",
});

const validateConfirmPassword = (rule: RuleItem, value: string, callback: (error?: Error) => void) => {
  if (value === "") {
    callback(new Error("请再次输入密码"));
  } else if (value !== formData.password) {
    callback(new Error("两次输入密码不一致！"));
  } else {
    callback();
  }
};

const formRules = reactive<FormRules>({
  username: [
    { required: true, message: "请输入用户名", trigger: "blur" },
    { min: 3, max: 20, message: "长度在 3 到 20 个字符", trigger: "blur" },
  ],
  email: [
    { required: true, message: "请输入邮箱地址", trigger: "blur" },
    { type: "email", message: "请输入正确的邮箱地址", trigger: "blur" },
  ],
  password: [
    { required: true, message: "请输入密码", trigger: "blur" },
    { min: 6, message: "密码长度不能小于 6 位", trigger: "blur" },
  ],
  confirmPassword: [{ required: true, validator: validateConfirmPassword, trigger: "blur" }],
});

const handleRegister = async () => {
  if (!registerFormRef.value) return;

  try {
    const valid = await registerFormRef.value.validate();
    if (!valid) return;
    isLoading.value = true;
    const requestData: RegisterRequest = {
      username: formData.username,
      email: formData.email,
      password: formData.password,
    } as RegisterRequest;

    const success = await authStore.register(requestData);
    if (success) {
      ElMessage.success("注册成功！请登录.");
      goToLogin();
    }
  } catch (error) {
    console.error("注册过程发生错误：", error);
  }
  finally {
    isLoading.value = false;
  }
};

const goToLogin = () => {
  router.push({name: "Login"})
};
</script>

<style scoped>
.register-layout {
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 20px;
  box-sizing: border-box;
  background-color: #f3f4f6;
  overflow: hidden;
}

.bg-shape {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  z-index: 0;
}

.shape-1 {
  width: 350px;
  height: 350px;
  background: rgba(64, 158, 255, 0.25);
  top: -80px;
  left: -80px;
}

.shape-2 {
  width: 350px;
  height: 350px;
  background: rgba(142, 68, 173, 0.18);
  bottom: -40px;
  right: -40px;
}

.register-container {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 400px;
}

.glass-card {
  width: 100%;
  border: none;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.88);
  backdrop-filter: blur(12px);
  box-shadow: 0 15px 35px -5px rgba(0, 0, 0, 0.08);
}

.header-section {
  text-align: center;
  margin-bottom: 20px;
}

.modern-form {
  width: 100%;
}

.logo-box {
  width: 40px;
  height: 40px;
  margin: 0 auto 8px;
  background: #ecf5ff;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.title {
  margin: 0;
  font-size: 18px;
  color: #1f2937;
}

.btn-form-item {
  margin-top: 10px;
  margin-bottom: 12px !important;
}

.submit-btn {
  width: 100%;
  height: 40px;
  border-radius: 6px;
  font-size: 14px;
}

.login-link {
  text-align: center;
  margin-top: 20px;
  font-size: 13px;
  color: #6b7280;
}

/* Reduce spacing between form items */
:deep(.el-form-item:not(:last-child)) {
  margin-bottom: 12px;
}

/* Adjust icon sizes */
:deep(.el-input__prefix) {
  font-size: 14px !important;
}

:deep(.el-icon) {
  font-size: 14px !important;
}

/* Make input icons smaller */
:deep(.el-input__prefix .el-icon) {
  font-size: 14px !important;
}
</style>
