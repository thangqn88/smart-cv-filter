// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router';
import JobPostList from '../components/JobPostList.vue';
import ApplicantList from '../components/ApplicantList.vue';
import ScreeningResult from '../components/ScreeningResult.vue';

const routes = [
  { path: '/', name: 'JobPostList', component: JobPostList },
  { path: '/jobs/:jobId/applicants', name: 'ApplicantList', component: ApplicantList, props: true },
  { path: '/results/:resultId', name: 'ScreeningResult', component: ScreeningResult, props: true },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;