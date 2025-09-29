<template>
  <div v-if="hasActiveProcessing" class="fixed bottom-4 right-4 z-50">
    <div class="bg-white rounded-lg shadow-lg border border-gray-200 p-4 max-w-sm">
      <div class="flex items-center justify-between mb-3">
        <h3 class="text-sm font-medium text-gray-900">Processing Status</h3>
        <button @click="showDetails = !showDetails" class="text-gray-400 hover:text-gray-600">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 9l-7 7-7-7"
            ></path>
          </svg>
        </button>
      </div>

      <!-- Summary -->
      <div class="space-y-2">
        <div class="flex items-center justify-between text-sm">
          <span class="text-gray-600">Active:</span>
          <span class="font-medium text-blue-600">{{ activeProcessingCount }}</span>
        </div>
        <div class="flex items-center justify-between text-sm">
          <span class="text-gray-600">Total:</span>
          <span class="font-medium text-gray-900">{{ processingCount }}</span>
        </div>
      </div>

      <!-- Details -->
      <div v-if="showDetails" class="mt-4 space-y-3">
        <div
          v-for="status in activeProcessingStatuses"
          :key="status.id"
          class="border border-gray-200 rounded-lg p-3"
        >
          <div class="flex items-center justify-between mb-2">
            <span class="text-xs font-medium text-gray-700 capitalize">
              {{ status.type }}
            </span>
            <span
              :class="{
                'text-yellow-600': status.status === 'pending',
                'text-blue-600': status.status === 'processing',
                'text-green-600': status.status === 'completed',
                'text-red-600': status.status === 'error',
              }"
              class="text-xs font-medium"
            >
              {{ status.status }}
            </span>
          </div>

          <div class="text-xs text-gray-600 mb-2">
            {{ status.message }}
          </div>

          <div v-if="status.status === 'processing'" class="space-y-1">
            <div class="flex justify-between text-xs text-gray-500">
              <span>Progress</span>
              <span>{{ status.progress }}%</span>
            </div>
            <div class="w-full bg-gray-200 rounded-full h-1.5">
              <div
                class="bg-blue-600 h-1.5 rounded-full transition-all duration-300"
                :style="{ width: `${status.progress}%` }"
              ></div>
            </div>
          </div>

          <div v-if="status.status === 'completed'" class="text-xs text-green-600">
            ✓ Completed successfully
          </div>

          <div v-if="status.status === 'error'" class="text-xs text-red-600">
            ✗ {{ status.error }}
          </div>

          <div class="flex justify-between items-center mt-2">
            <span class="text-xs text-gray-400">
              {{ formatDuration(status.startTime, status.endTime) }}
            </span>
            <button
              @click="clearStatus(status.id)"
              class="text-xs text-gray-400 hover:text-gray-600"
            >
              Clear
            </button>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="mt-4 flex space-x-2">
        <button
          @click="clearAllStatuses"
          class="flex-1 px-3 py-1 text-xs bg-gray-100 text-gray-700 rounded hover:bg-gray-200"
        >
          Clear All
        </button>
        <button
          @click="refreshStatuses"
          class="flex-1 px-3 py-1 text-xs bg-blue-100 text-blue-700 rounded hover:bg-blue-200"
        >
          Refresh
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { realtimeService, type ProcessingStatus } from '../../services/realtimeService'

// Reactive data
const showDetails = ref(false)
const processingStatuses = ref<ProcessingStatus[]>([])

// Computed properties
const hasActiveProcessing = computed(() => processingStatuses.value.length > 0)

const activeProcessingCount = computed(
  () =>
    processingStatuses.value.filter(
      (status) => status.status === 'processing' || status.status === 'pending',
    ).length,
)

const processingCount = computed(() => processingStatuses.value.length)

const activeProcessingStatuses = computed(() =>
  processingStatuses.value.filter(
    (status) => status.status === 'processing' || status.status === 'pending',
  ),
)

// Methods
const refreshStatuses = () => {
  processingStatuses.value = realtimeService.getAllProcessingStatuses()
}

const clearStatus = (id: string) => {
  realtimeService.clearProcessingStatus(id)
  refreshStatuses()
}

const clearAllStatuses = () => {
  realtimeService.clearAllProcessingStatuses()
  refreshStatuses()
}

const formatDuration = (startTime: Date, endTime?: Date): string => {
  const end = endTime || new Date()
  const duration = end.getTime() - startTime.getTime()
  const seconds = Math.floor(duration / 1000)

  if (seconds < 60) {
    return `${seconds}s`
  } else if (seconds < 3600) {
    const minutes = Math.floor(seconds / 60)
    return `${minutes}m ${seconds % 60}s`
  } else {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    return `${hours}h ${minutes}m`
  }
}

// Lifecycle
onMounted(() => {
  refreshStatuses()

  // Subscribe to real-time updates
  const unsubscribe = realtimeService.subscribe('status_update', (update) => {
    refreshStatuses()
  })

  // Store unsubscribe function for cleanup
  onUnmounted(unsubscribe)
})

// Auto-refresh every 5 seconds
let refreshInterval: NodeJS.Timeout
onMounted(() => {
  refreshInterval = setInterval(refreshStatuses, 5000)
})

onUnmounted(() => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
  }
})
</script>
