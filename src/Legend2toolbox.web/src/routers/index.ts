import { createRouter, createWebHashHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'


const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        name: "Home",
        component: () => import('@/views/HomeView.vue'),
        meta: {
            requiresAuth: true,
            title: '首页'
        },
    },
    {
        path: '/login',
        name: 'Login',
        component: () => import('@/views/LoginView.vue'),
        meta: {
            guestOnly: true,
            title: '用户登录'
        }
    },
    {
        path: '/register',
        name: 'Register',
        component: () => import('@/views/RegisterView.vue'),
        meta: {
            guestOnly: true,
            title: '用户注册'
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
    if(to.meta.title){
        document.title = `${to.meta.title} - Legend2 Toolbox`
    }
    const authStore = useAuthStore();

    const isAuthenticated = authStore.isAuthenticated || !!authStore.token

    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
        return { name: "Login", query: { redirect: to.fullPath } };
    } else if (to.meta.guestOnly && isAuthenticated) {
        return { name: "Home" };
    } else {
        return true;
    }
})


export default router;