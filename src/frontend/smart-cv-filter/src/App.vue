<script setup lang="ts">
import { onMounted } from 'vue'
import { useAuthStore } from './stores/auth'
import { useRouter } from 'vue-router'
import Navbar from './components/layout/Navbar.vue'
import LoadingSpinner from './components/common/LoadingSpinner.vue'

const authStore = useAuthStore()
const router = useRouter()

onMounted(async () => {
  // Check if user is already authenticated
  await authStore.checkAuth()
})
</script>

<template>
  <div id="app" class="min-h-screen bg-gray-50">
    <!-- Main Content -->
    <router-view />
  </div>
</template>

<style>
/* Global styles */
body {
  margin: 0;
  font-family:
    'Inter',
    -apple-system,
    BlinkMacSystemFont,
    'Segoe UI',
    Roboto,
    sans-serif;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
}

::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}
</style>
