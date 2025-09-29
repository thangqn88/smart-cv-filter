<template>
  <div v-if="isLoading" class="min-h-screen flex items-center justify-center">
    <div class="text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
      <p class="mt-4 text-gray-600">Loading...</p>
    </div>
  </div>
  <div
    v-else-if="!isAuthenticated"
    class="min-h-screen flex items-center justify-center bg-gray-50"
  >
    <div class="max-w-md w-full space-y-8 p-8">
      <div class="text-center">
        <div class="mx-auto h-12 w-12 bg-red-100 rounded-full flex items-center justify-center">
          <svg class="h-6 w-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
            ></path>
          </svg>
        </div>
        <h2 class="mt-4 text-2xl font-bold text-gray-900">Access Denied</h2>
        <p class="mt-2 text-gray-600">You need to be logged in to access this page.</p>
        <div class="mt-6 space-y-3">
          <router-link
            to="/login"
            class="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Sign In
          </router-link>
          <router-link
            to="/register"
            class="w-full flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Create Account
          </router-link>
        </div>
      </div>
    </div>
  </div>
  <div v-else class="min-h-screen bg-gray-50">
    <!-- Navigation -->
    <Navbar />

    <!-- Loading Spinner -->
    <LoadingSpinner v-if="authStore.isLoading" />

    <!-- Main Content -->
    <main class="container mx-auto px-4 py-8">
      <slot />
    </main>

    <!-- Footer -->
    <footer class="bg-white border-t border-gray-200 mt-12">
      <div class="container mx-auto px-4 py-6">
        <div class="text-center text-gray-600">
          <p>&copy; 2024 Smart CV Filter. All rights reserved.</p>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useAuthStore } from '../../stores/auth'
import Navbar from '../layout/Navbar.vue'
import LoadingSpinner from '../common/LoadingSpinner.vue'

const authStore = useAuthStore()
const isLoading = ref(true)

onMounted(async () => {
  await authStore.checkAuth()
  isLoading.value = false
})

const isAuthenticated = computed(() => authStore.isAuthenticated)
</script>
