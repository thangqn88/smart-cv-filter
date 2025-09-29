import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiService } from '../services/apiService'
import type { User, LoginRequest, RegisterRequest, AuthResponse } from '../types/auth'

export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(localStorage.getItem('token'))
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const userInfo = computed(() => user.value)

  // Actions
  const login = async (credentials: LoginRequest): Promise<boolean> => {
    try {
      isLoading.value = true
      error.value = null

      const response = await apiService.login(credentials)
      
      if (response.success && response.data) {
        token.value = response.data.token
        user.value = response.data.user
        
        // Store token in localStorage
        localStorage.setItem('token', response.data.token)
        
        // Set default authorization header
        apiService.setAuthToken(response.data.token)
        
        return true
      } else {
        error.value = response.message || 'Login failed'
        return false
      }
    } catch (err: any) {
      error.value = err.message || 'Login failed'
      return false
    } finally {
      isLoading.value = false
    }
  }

  const register = async (userData: RegisterRequest): Promise<boolean> => {
    try {
      isLoading.value = true
      error.value = null

      const response = await apiService.register(userData)
      
      if (response.success && response.data) {
        token.value = response.data.token
        user.value = response.data.user
        
        // Store token in localStorage
        localStorage.setItem('token', response.data.token)
        
        // Set default authorization header
        apiService.setAuthToken(response.data.token)
        
        return true
      } else {
        error.value = response.message || 'Registration failed'
        return false
      }
    } catch (err: any) {
      error.value = err.message || 'Registration failed'
      return false
    } finally {
      isLoading.value = false
    }
  }

  const logout = async (): Promise<void> => {
    try {
      // Call logout endpoint if needed
      await apiService.logout()
    } catch (err) {
      console.error('Logout error:', err)
    } finally {
      // Clear local state
      user.value = null
      token.value = null
      error.value = null
      
      // Remove token from localStorage
      localStorage.removeItem('token')
      
      // Clear authorization header
      apiService.clearAuthToken()
    }
  }

  const checkAuth = async (): Promise<boolean> => {
    if (!token.value) {
      return false
    }

    try {
      isLoading.value = true
      error.value = null

      // Set authorization header
      apiService.setAuthToken(token.value)

      const response = await apiService.validateToken()
      
      if (response.success && response.data) {
        user.value = response.data
        return true
      } else {
        // Token is invalid, clear it
        await logout()
        return false
      }
    } catch (err: any) {
      console.error('Auth check error:', err)
      // Token is invalid, clear it
      await logout()
      return false
    } finally {
      isLoading.value = false
    }
  }

  const clearError = (): void => {
    error.value = null
  }

  return {
    // State
    user,
    token,
    isLoading,
    error,
    
    // Getters
    isAuthenticated,
    userInfo,
    
    // Actions
    login,
    register,
    logout,
    checkAuth,
    clearError
  }
})
