<template>
  <el-card class="audit-page" shadow="never">
    <template #header>
      <h2>{{ all ? "全部审计记录" : "我的操作与登录记录" }}</h2>
      <p>{{ all ? "查看所有用户的操作、登录和系统变更。" : "查看自己的操作及账号登录记录。" }}</p>
    </template>

    <el-form :inline="true" class="filters" @submit.prevent="search">
      <el-form-item label="记录类型">
        <el-select v-model="category" aria-label="记录类型" class="filter-select">
          <el-option label="全部" value="" />
          <el-option label="登录记录" value="login" />
          <el-option label="用户管理" value="User" />
          <el-option label="卡号操作" value="CardNumber" />
          <el-option label="会员变更" value="Membership" />
          <el-option label="脚本集" value="ScriptSet" />
          <el-option label="脚本文件" value="ScriptFile" />
          <el-option label="素材文件" value="MaterialFile" />
          <el-option label="脚本数据库" value="ScriptSetDbData" />
          <el-option label="通讯密钥" value="ConnectionKey" />
          <el-option label="卡号路径" value="CardNumberPath" />
          <el-option label="会员支付" value="MembershipPayment" />
        </el-select>
      </el-form-item>
      <el-form-item label="结果">
        <el-select v-model="outcome" aria-label="结果" class="filter-select" clearable>
          <el-option label="成功" value="Succeeded" />
          <el-option label="失败" value="Failed" />
          <el-option label="已取消" value="Cancelled" />
        </el-select>
      </el-form-item>
      <el-form-item label="时间">
        <el-date-picker
          v-model="dates"
          type="datetimerange"
          start-placeholder="开始时间"
          end-placeholder="结束时间"
          class="date-range"
        />
      </el-form-item>
      <el-form-item label="登录 / 操作 IP">
        <el-input v-model="clientIp" placeholder="精确 IP 地址" clearable />
      </el-form-item>
      <el-form-item v-if="all" label="用户账号 / ID">
        <el-input v-model="account" placeholder="完整登录账号或用户 ID" clearable />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" native-type="submit">查询</el-button>
        <el-button @click="reset">重置</el-button>
      </el-form-item>
    </el-form>

    <el-alert v-if="loadError" :title="loadError" type="error" :closable="false" show-icon />
    <el-table v-loading="loading" :data="rows" empty-text="暂无审计记录" stripe>
      <el-table-column label="时间" min-width="180">
        <template #default="{ row }">{{ formatTime(row.occurredAtUnixMs) }}</template>
      </el-table-column>
      <el-table-column label="账号 / 操作者" min-width="150">
        <template #default="{ row }">
          {{ row.actorUserName || row.targetName || row.actorType }}
        </template>
      </el-table-column>
      <el-table-column label="模块" min-width="120">
        <template #default="{ row }">{{ label(row.module) }}</template>
      </el-table-column>
      <el-table-column label="操作" min-width="120">
        <template #default="{ row }">{{ label(row.action) }}</template>
      </el-table-column>
      <el-table-column prop="clientIp" label="IP" min-width="140" />
      <el-table-column label="结果" width="95">
        <template #default="{ row }">
          <el-tag :type="row.outcome === 'Succeeded' ? 'success' : 'danger'">
            {{ label(row.outcome) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="数据状态" min-width="135">
        <template #default="{ row }">{{ label(row.dataStatus) }}</template>
      </el-table-column>
      <el-table-column v-if="canViewDetail" label="详情" width="80" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="showDetail(row.id)">查看</el-button>
        </template>
      </el-table-column>
    </el-table>
    <div class="pagination">
      <el-pagination
        v-model:current-page="pageNumber"
        v-model:page-size="pageSize"
        :page-sizes="[10, 20, 50, 100]"
        :total="total"
        layout="total, sizes, prev, pager, next"
        @current-change="load"
        @size-change="search"
      />
    </div>

    <el-drawer v-if="canViewDetail" v-model="detailVisible" title="审计详情" size="min(820px, 100%)">
      <div v-loading="detailLoading">
        <template v-if="detail">
          <el-descriptions :column="1" border>
            <el-descriptions-item label="记录 ID">{{ detail.id }}</el-descriptions-item>
            <el-descriptions-item label="时间">
              {{ formatTime(detail.occurredAtUnixMs) }}
            </el-descriptions-item>
            <el-descriptions-item label="操作">
              {{ label(detail.module) }} / {{ label(detail.action) }}
            </el-descriptions-item>
            <el-descriptions-item label="操作者">
              {{ detail.actorUserName || label(detail.actorType) }}
            </el-descriptions-item>
            <el-descriptions-item label="用户 ID">
              {{ detail.actorUserId || detail.subjectUserId || "—" }}
            </el-descriptions-item>
            <el-descriptions-item label="目标 ID">{{
              detail.targetId || "—"
            }}</el-descriptions-item>
            <el-descriptions-item label="目标名称">
              {{ detail.targetName || "—" }}
            </el-descriptions-item>
            <el-descriptions-item label="客户端 IP">{{
              detail.clientIp || "—"
            }}</el-descriptions-item>
            <el-descriptions-item label="连接 IP">{{ detail.peerIp || "—" }}</el-descriptions-item>
            <el-descriptions-item label="结果">{{ label(detail.outcome) }}</el-descriptions-item>
            <el-descriptions-item label="数据状态">
              {{ label(detail.dataStatus) }}
            </el-descriptions-item>
            <el-descriptions-item v-if="detail.errorCode" label="失败原因">
              {{ label(detail.errorCode) }}
            </el-descriptions-item>
          </el-descriptions>
          <el-alert
            v-if="detail.captureIncomplete"
            title="本次变更明细采集不完整，请联系管理员核查。"
            type="warning"
            :closable="false"
          />
          <el-empty v-if="!detail.details.length" description="本次操作没有已提交的字段变更" />
          <section v-for="change in detail.details" :key="change.id" class="change">
            <h3>
              {{ change.sequence }}. {{ label(change.entityType) }} · {{ label(change.changeType) }}
            </h3>
            <p class="entity-id">{{ change.entityId }}</p>
            <div class="diff">
              <div>
                <h4>变更前</h4>
                <pre>{{ pretty(change.oldValues) }}</pre>
              </div>
              <div>
                <h4>变更后</h4>
                <pre>{{ pretty(change.newValues) }}</pre>
              </div>
            </div>
          </section>
        </template>
      </div>
    </el-drawer>
  </el-card>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { AuditService, type AuditLog } from "@/api/auditService";
import { useAuthStore } from "@/stores/auth";
import { handleApiError } from "@/utils/errorHandler";

const route = useRoute();
const auth = useAuthStore();
const all = computed(() => route.path === "/admin/audit");
const canViewDetail = computed(() => auth.userInfo?.roles.includes("SuperAdmin") === true);
const category = ref("");
const outcome = ref("");
const dates = ref<[Date, Date] | null>(null);
const clientIp = ref("");
const account = ref("");
const pageNumber = ref(1);
const pageSize = ref(20);
const total = ref(0);
const rows = ref<AuditLog[]>([]);
const loading = ref(false);
const loadError = ref("");
const detailVisible = ref(false);
const detailLoading = ref(false);
const detail = ref<AuditLog | null>(null);
let loadVersion = 0;
let detailVersion = 0;

const labels: Record<string, string> = {
  Auth: "账号登录",
  User: "用户",
  ApplicationUser: "用户",
  CardNumber: "卡号",
  Membership: "会员",
  UserMembership: "会员权益",
  MembershipPayment: "会员支付",
  MembershipPaymentOrder: "支付订单",
  ScriptSet: "脚本集",
  ScriptFile: "脚本文件",
  ScriptSegment: "脚本片段",
  MaterialFile: "素材文件",
  ScriptSetDbData: "脚本数据库",
  ConnectionKey: "通讯密钥",
  CardNumberPath: "卡号路径",
  ApplicationRole: "角色",
  "IdentityUserRole`1": "用户角色",
  Login: "登录",
  Create: "新增",
  Update: "修改",
  Delete: "删除",
  SoftDelete: "软删除",
  UpdateProfile: "修改个人资料",
  AssignRoles: "分配角色",
  Lock: "锁定",
  Unlock: "解锁",
  ChangePassword: "修改密码",
  ResetPassword: "重置密码",
  AdjustDays: "调整会员时间",
  Reissue: "补发",
  Cleanup: "清理",
  Rotate: "更新密钥",
  CreateOrder: "创建订单",
  PaymentNotification: "支付回调",
  Expire: "会员到期",
  Succeeded: "成功",
  Failed: "失败",
  Cancelled: "已取消",
  NoChanges: "无字段变更",
  Committed: "已提交",
  RolledBack: "已回滚",
  PartiallyCommitted: "部分已提交",
  Unknown: "待核查",
  Added: "新增",
  Modified: "修改",
  Deleted: "删除",
  SoftDeleted: "软删除",
  CascadeDeleted: "关联删除",
  LoginAttempt: "未认证的登录尝试",
  System: "系统",
  PaymentCallback: "支付平台",
  Registration: "注册请求",
  PasswordReset: "密码重置请求",
  BusinessRejected: "业务校验未通过",
  ValidationException: "输入校验未通过",
};
const label = (value: string) => labels[value] ?? value;
const formatTime = (value: number) => new Date(value).toLocaleString("zh-CN", { hour12: false });
const pretty = (value: string) => {
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
};

async function load() {
  const version = ++loadVersion;
  rows.value = [];
  total.value = 0;
  loading.value = true;
  loadError.value = "";
  try {
    const result = await AuditService.list(all.value, {
      pageNumber: pageNumber.value,
      pageSize: pageSize.value,
      module: category.value === "login" ? "Auth" : category.value || undefined,
      action: category.value === "login" ? "Login" : undefined,
      outcome: outcome.value || undefined,
      from: dates.value?.[0].toISOString(),
      to: dates.value?.[1].toISOString(),
      clientIp: clientIp.value.trim() || undefined,
      account: all.value ? account.value.trim() || undefined : undefined,
    });
    if (version !== loadVersion) return;
    rows.value = result.items;
    total.value = result.totalCount;
  } catch (error) {
    if (version !== loadVersion) return;
    loadError.value = "审计记录加载失败，请检查筛选条件后重试。";
    handleApiError(error, loadError.value);
  } finally {
    if (version === loadVersion) loading.value = false;
  }
}

function search() {
  pageNumber.value = 1;
  void load();
}

function reset() {
  category.value = "";
  outcome.value = "";
  dates.value = null;
  clientIp.value = "";
  account.value = "";
  search();
}

async function showDetail(id: string) {
  if (!canViewDetail.value) return;
  const version = ++detailVersion;
  detail.value = null;
  detailVisible.value = true;
  detailLoading.value = true;
  try {
    const result = await AuditService.detail(all.value, id);
    if (version === detailVersion) detail.value = result;
  } catch (error) {
    if (version !== detailVersion) return;
    detailVisible.value = false;
    handleApiError(error, "无法读取该审计记录");
  } finally {
    if (version === detailVersion) detailLoading.value = false;
  }
}

watch(
  [all, canViewDetail, () => auth.userInfo?.userId],
  () => {
    detailVersion++;
    detailVisible.value = false;
    detail.value = null;
    reset();
  },
  { immediate: true },
);
</script>

<style scoped>
h2 {
  margin: 0;
  font-size: 20px;
}
p {
  color: #606266;
}
.filter-select {
  width: 155px;
}
.date-range {
  max-width: 100%;
}
.pagination {
  display: flex;
  justify-content: flex-end;
  overflow-x: auto;
  margin-top: 20px;
}
.change {
  margin-top: 24px;
  border-top: 1px solid #ebeef5;
}
.change h3 {
  font-size: 16px;
}
.entity-id {
  overflow-wrap: anywhere;
}
.diff {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}
.diff > div {
  min-width: 0;
}
pre {
  background: #f5f7fa;
  padding: 12px;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}
@media (max-width: 640px) {
  .diff {
    grid-template-columns: 1fr;
  }
  .filters :deep(.el-form-item) {
    display: flex;
    margin-right: 0;
  }
  .filters :deep(.el-form-item__content) {
    min-width: 0;
  }
}
</style>
