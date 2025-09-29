<template>
  <div class="job-post-list-page p-6 bg-gray-100 min-h-screen">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-3xl font-bold text-gray-800">Quản lý Tin tuyển dụng</h1>
      <div class="flex space-x-2">
        <button
          @click="refreshJobPosts"
          :disabled="isLoading"
          class="bg-gray-600 hover:bg-gray-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-colors disabled:opacity-50"
        >
          <i class="fas fa-refresh mr-2" :class="{ 'animate-spin': isLoading }"></i>
          Refresh
        </button>
        <button
          @click="showCreateModal = true"
          class="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-colors"
        >
          <i class="fas fa-plus mr-2"></i> Đăng tin tuyển dụng mới
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading && jobPosts.length === 0" class="flex justify-center items-center py-12">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600">Loading job posts...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-md p-4 mb-6">
      <div class="flex">
        <div class="flex-shrink-0">
          <svg class="h-5 w-5 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            ></path>
          </svg>
        </div>
        <div class="ml-3">
          <p class="text-sm text-red-800">{{ error }}</p>
          <button
            @click="refreshJobPosts"
            class="mt-2 text-sm text-red-600 hover:text-red-800 underline"
          >
            Try again
          </button>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="jobPosts.length === 0" class="text-center py-12">
      <svg
        class="mx-auto h-12 w-12 text-gray-400"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
        ></path>
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900">No job posts</h3>
      <p class="mt-1 text-sm text-gray-500">Get started by creating a new job post.</p>
      <div class="mt-6">
        <button
          @click="showCreateModal = true"
          class="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
        >
          <i class="fas fa-plus mr-2"></i>
          New Job Post
        </button>
      </div>
    </div>

    <!-- Job Posts Table -->
    <div v-else class="bg-white rounded-lg shadow-xl p-4">
      <table class="min-w-full table-auto">
        <thead>
          <tr class="bg-gray-200 text-gray-600 uppercase text-sm leading-normal">
            <th class="py-3 px-6 text-left">Tên vị trí</th>
            <th class="py-3 px-6 text-left">Lượt ứng tuyển</th>
            <th class="py-3 px-6 text-left">Trạng thái</th>
            <th class="py-3 px-6 text-left">Ngày đăng</th>
            <th class="py-3 px-6 text-center">Tác vụ</th>
          </tr>
        </thead>
        <tbody class="text-gray-600 text-sm font-light">
          <tr
            v-for="job in jobPosts"
            :key="job.id"
            class="border-b border-gray-200 hover:bg-gray-100"
          >
            <td class="py-3 px-6 text-left whitespace-nowrap">
              <div class="flex items-center">
                <div>
                  <div class="text-sm font-medium text-gray-900">{{ job.title }}</div>
                  <div class="text-sm text-gray-500">{{ job.department }}</div>
                </div>
              </div>
            </td>
            <td class="py-3 px-6 text-left">
              <span
                class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
              >
                {{ job.applicants || 0 }} hồ sơ
              </span>
            </td>
            <td class="py-3 px-6 text-left">
              <span
                :class="{
                  'bg-green-200 text-green-600': job.status === 'Active',
                  'bg-yellow-200 text-yellow-600': job.status === 'Draft',
                  'bg-red-200 text-red-600': job.status === 'Closed',
                  'bg-gray-200 text-gray-600': job.status === 'Paused',
                }"
                class="py-1 px-3 rounded-full text-xs font-semibold"
              >
                {{ job.status }}
              </span>
            </td>
            <td class="py-3 px-6 text-left">{{ formatDate(job.postedDate) }}</td>
            <td class="py-3 px-6 text-center">
              <div class="flex items-center justify-center space-x-2">
                <router-link
                  :to="{ name: 'ApplicantList', params: { jobId: job.id } }"
                  class="bg-indigo-500 hover:bg-indigo-600 text-white py-1 px-3 rounded-md text-xs font-semibold"
                >
                  Xem ứng viên
                </router-link>
                <button
                  @click="editJobPost(job)"
                  class="bg-yellow-500 hover:bg-yellow-600 text-white py-1 px-3 rounded-md text-xs font-semibold"
                >
                  Sửa
                </button>
                <button
                  @click="deleteJobPost(job.id)"
                  class="bg-red-500 hover:bg-red-600 text-white py-1 px-3 rounded-md text-xs font-semibold"
                >
                  Xóa
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { jobService, dummyData } from '../services/apiService'

// Reactive data
const jobPosts = ref<any[]>([])
const isLoading = ref(false)
const error = ref<string | null>(null)
const showCreateModal = ref(false)

// Lifecycle
onMounted(async () => {
  await loadJobPosts()
})

// Methods
const loadJobPosts = async () => {
  isLoading.value = true
  error.value = null

  try {
    // Try to load from real API first
    const response = await jobService.getJobPosts()
    jobPosts.value = response.data || []
  } catch (apiError) {
    console.warn('API not available, using dummy data:', apiError)
    // Fallback to dummy data
    jobPosts.value = dummyData.jobPosts.map((job) => ({
      ...job,
      department: 'Engineering', // Add missing field
      applicants: job.applicants || 0,
    }))
  } finally {
    isLoading.value = false
  }
}

const refreshJobPosts = async () => {
  await loadJobPosts()
}

const editJobPost = (job: any) => {
  // TODO: Implement edit modal or navigate to edit page
  console.log('Edit job post:', job)
  alert(`Edit job post: ${job.title}`)
}

const deleteJobPost = async (jobId: number) => {
  if (!confirm('Are you sure you want to delete this job post?')) {
    return
  }

  try {
    await jobService.deleteJobPost(jobId)
    // Remove from local list
    jobPosts.value = jobPosts.value.filter((job) => job.id !== jobId)
  } catch (error) {
    console.error('Error deleting job post:', error)
    alert('Failed to delete job post')
  }
}

const formatDate = (dateString: string) => {
  if (!dateString) return '-'
  try {
    const date = new Date(dateString)
    return date.toLocaleDateString('vi-VN')
  } catch {
    return dateString
  }
}
</script>
