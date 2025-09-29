// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router';
import JobPostList from '../components/JobPostList.vue';
import ApplicantList from '../components/ApplicantList.vue';
import ScreeningResult from '../components/ScreeningResult.vue';
import Login from '../components/auth/Login.vue';
import Register from '../components/auth/Register.vue';
import AuthGuard from '../components/auth/AuthGuard.vue';
import { useAuthStore } from '../stores/auth';

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: { requiresGuest: true }
  },
  {
    path: '/register',
    name: 'Register',
    component: Register,
    meta: { requiresGuest: true }
  },
  {
    path: '/',
    component: AuthGuard,
    children: [
      {
        path: '',
        name: 'JobPostList',
        component: JobPostList
      },
      {
        path: 'jobs',
        name: 'Jobs',
        component: JobPostList
      },
      {
        path: 'applicants',
        name: 'Applicants',
        component: ApplicantList
      },
      {
        path: 'jobs/:jobId/applicants',
        name: 'ApplicantList',
        component: ApplicantList,
        props: true
      },
      {
        path: 'results/:resultId',
        name: 'ScreeningResult',
        component: ScreeningResult,
        props: true
      }
    ]
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  
  // Check if route requires authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login');
    return;
  }
  
  // Check if route requires guest (not authenticated)
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/');
    return;
  }
  
  next();
});

export default router;