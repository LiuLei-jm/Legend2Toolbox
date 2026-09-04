import {createRouter, createWebHashHistory, type RouteRecordRaw} from 'vue-router'
import {useAuthStore} from '@/stores/auth'


const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: "Home",
    component: () => import('@/views/HomeView.vue'),
    redirect: "/cards",
    meta: {
      requiresAuth: true,
      title: '首页'
    },
    children: [
      {
        path: 'profile',
        name: 'UserProfile',
        component: () => import('@/views/auth/ProfileInfo.vue'),
        meta: {title: '个人中心'}
      },
      {
        path: 'cards',
        name: 'CardNumber',
        component: () => import('@/views/cards/CardsView.vue'),
        meta: {title: '卡号管理'}
      },
      {
        path: 'connection-key',
        name: 'ConnectionKey',
        component: () => import('@/views/connection-key/ConnectionKeyView.vue'),
        meta: {title: '通讯密钥'}
      },
      {
        path: 'script',
        name: 'Script',
        component: () => import('@/views/script/ScriptSetManagement.vue'),
        meta: {title: '脚本管理'}
      },
      {
        path: 'admin/users',
        name: "UserManagement",
        component: () => import('@/views/dashboard/AdminUserManagement.vue'),
        meta:{
          requiresAdmin : true,
          title: '用户管理'
        }
      }
    ]
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/auth/LoginView.vue'),
    meta: {
      guestOnly: true,
      title: '用户登录'
    }
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/auth/RegisterView.vue'),
    meta: {
      guestOnly: true,
      title: '用户注册'
    }
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: () => import('@/views/auth/ForgotPasswordView.vue'),
    meta: {
      guestOnly: true,
      title: '忘记密码'
    }
  },
  {
    path: '/reset-password',
    name: 'ResetPassword',
    component: () => import('@/views/auth/ResetPasswordView.vue'),
    meta: {
      guestOnly: true,
      title: '重置密码'
    }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHashHistory(import.meta.env.BASE_URL),
  routes,
});

router.beforeEach((to) => {
  if (to.meta.title) {
    document.title = `${to.meta.title} - Toolbox`
  }
  const authStore = useAuthStore();

  const isAuthenticated = authStore.isAuthenticated || !!authStore.accessToken

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return {name: "Login", query: {redirect: to.fullPath}};
  } else if (to.meta.guestOnly && isAuthenticated) {
    return {name: "Home"};
  } else {
    return true;
  }
})


export default router;
