<template>
  <div class="applicant-list-page p-6 bg-gray-100 min-h-screen">
    <h1 class="text-3xl font-bold mb-6 text-gray-800">Danh sách Ứng viên</h1>
    <h2 class="text-xl font-semibold mb-4 text-gray-700">Vị trí: {{ jobTitle }}</h2>

    <!-- File Upload Section -->
    <div class="mb-6 bg-white rounded-lg shadow-lg p-6">
      <h3 class="text-lg font-semibold mb-4 text-gray-800">Upload CV Files</h3>
      <FileUpload
        :max-file-size-m-b="10"
        :accepted-types="['.pdf', '.doc', '.docx']"
        :drag-text="'Drop CV files here or click to browse'"
        @upload="handleFileUpload"
        @success="handleUploadSuccess"
        @error="handleUploadError"
      />
    </div>

    <div class="flex justify-between mb-4">
      <button
        @click="showBulkUpload = !showBulkUpload"
        class="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-colors"
      >
        <i class="fas fa-upload mr-2"></i> {{ showBulkUpload ? 'Hide' : 'Show' }} Bulk Upload
      </button>
      <button
        @click="screenApplicants"
        :disabled="selectedApplicants.length === 0 || isProcessing"
        class="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
      >
        <i class="fas fa-robot mr-2"></i> Sàng lọc bằng AI ({{ selectedApplicants.length }})
      </button>
    </div>

    <!-- Bulk Upload Section -->
    <div v-if="showBulkUpload" class="mb-6 bg-white rounded-lg shadow-lg p-6">
      <h3 class="text-lg font-semibold mb-4 text-gray-800">Bulk Upload CVs</h3>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="(upload, index) in bulkUploads"
          :key="index"
          class="border border-gray-200 rounded-lg p-4"
        >
          <h4 class="font-medium text-gray-700 mb-2">Applicant {{ index + 1 }}</h4>
          <FileUpload
            :max-file-size-m-b="10"
            :accepted-types="['.pdf', '.doc', '.docx']"
            :drag-text="'Drop CV here'"
            @upload="(file) => handleBulkUpload(file, index)"
            @success="(fileName) => handleBulkUploadSuccess(fileName, index)"
            @error="(error) => handleBulkUploadError(error, index)"
          />
        </div>
      </div>
      <div class="mt-4 flex justify-end space-x-2">
        <button
          @click="addBulkUpload"
          class="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Add Applicant
        </button>
        <button
          @click="removeBulkUpload"
          :disabled="bulkUploads.length <= 1"
          class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:opacity-50"
        >
          Remove Last
        </button>
      </div>
    </div>

    <div class="bg-white rounded-lg shadow-xl p-4">
      <table class="min-w-full table-auto">
        <thead>
          <tr class="bg-gray-200 text-gray-600 uppercase text-sm leading-normal">
            <th class="py-3 px-6 text-center"></th>
            <th class="py-3 px-6 text-left">Tên ứng viên</th>
            <th class="py-3 px-6 text-left">Email</th>
            <th class="py-3 px-6 text-left">Ngày nộp</th>
            <th class="py-3 px-6 text-left">CV File</th>
            <th class="py-3 px-6 text-left">Điểm số AI</th>
            <th class="py-3 px-6 text-left">Trạng thái</th>
            <th class="py-3 px-6 text-center">Tác vụ</th>
          </tr>
        </thead>
        <tbody class="text-gray-600 text-sm font-light">
          <tr
            v-for="applicant in applicants"
            :key="applicant.id"
            class="border-b border-gray-200 hover:bg-gray-100"
          >
            <td class="py-3 px-6 text-center">
              <input type="checkbox" :value="applicant.id" v-model="selectedApplicants" />
            </td>
            <td class="py-3 px-6 text-left">{{ applicant.name }}</td>
            <td class="py-3 px-6 text-left">{{ applicant.email }}</td>
            <td class="py-3 px-6 text-left">{{ applicant.submittedDate }}</td>
            <td class="py-3 px-6 text-left">
              <div v-if="applicant.cvFile" class="flex items-center space-x-2">
                <svg
                  class="w-4 h-4 text-gray-400"
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
                <span class="text-sm text-gray-600">{{ applicant.cvFile.name }}</span>
                <button
                  @click="downloadCV(applicant.id)"
                  class="text-blue-600 hover:text-blue-800 text-xs"
                >
                  Download
                </button>
              </div>
              <div v-else class="flex items-center space-x-2">
                <span class="text-gray-400 text-sm">No CV</span>
                <button
                  @click="uploadCVForApplicant(applicant.id)"
                  class="text-green-600 hover:text-green-800 text-xs"
                >
                  Upload
                </button>
              </div>
            </td>
            <td class="py-3 px-6 text-left">
              <span v-if="applicant.aiScore">{{ applicant.aiScore }}%</span>
              <span v-else>-</span>
            </td>
            <td class="py-3 px-6 text-left">
              <span
                :class="{
                  'bg-yellow-200 text-yellow-600': applicant.status === 'Đang xử lý',
                  'bg-green-200 text-green-600': applicant.status === 'Hoàn thành',
                  'bg-gray-200 text-gray-600': applicant.status === 'Chưa xử lý',
                }"
                class="py-1 px-3 rounded-full text-xs font-semibold"
              >
                {{ applicant.status }}
              </span>
            </td>
            <td class="py-3 px-6 text-center">
              <router-link
                v-if="applicant.status === 'Hoàn thành'"
                :to="{ name: 'ScreeningResult', params: { resultId: 1 } }"
                class="bg-indigo-500 hover:bg-indigo-600 text-white py-1 px-3 rounded-md text-xs font-semibold"
              >
                Xem chi tiết
              </router-link>
              <span v-else>-</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Real-time Processing Status -->
    <ProcessingStatus />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { jobService, dummyData } from '../services/apiService'
import { cvUploadService } from '../services/cvUploadService'
import { realtimeService } from '../services/realtimeService'
import FileUpload from '../common/FileUpload.vue'
import ProcessingStatus from '../common/ProcessingStatus.vue'

// Props
interface Props {
  jobId?: number
}

const props = withDefaults(defineProps<Props>(), {
  jobId: 0,
})

const route = useRoute()

// Reactive data
const applicants = ref<any[]>([])
const selectedApplicants = ref<number[]>([])
const jobTitle = ref('')
const isProcessing = ref(false)
const showBulkUpload = ref(false)
const bulkUploads = ref<Array<{ file: File | null; uploaded: boolean; error: string | null }>>([
  { file: null, uploaded: false, error: null },
])

// Get jobId from props or route
const currentJobId = computed(() => props.jobId || Number(route.params.jobId) || 0)

// Lifecycle
onMounted(async () => {
  await loadApplicants()
})

// Methods
const loadApplicants = async () => {
  try {
    // Use dummy data for now
    applicants.value = dummyData.applicants.map((applicant) => ({
      ...applicant,
      cvFile: applicant.cvFile || null,
    }))

    const job = dummyData.jobPosts.find((j) => j.id === currentJobId.value)
    jobTitle.value = job ? job.title : 'Vị trí không xác định'

    // TODO: Replace with real API call
    // const response = await jobService.getApplicants(currentJobId.value)
    // applicants.value = response.data
  } catch (error) {
    console.error('Error loading applicants:', error)
  }
}

const screenApplicants = async () => {
  isProcessing.value = true

  try {
    // Start real-time processing for each selected applicant
    for (const applicantId of selectedApplicants.value) {
      const processingId = `screening_${applicantId}_${Date.now()}`

      // Start processing status
      realtimeService.startProcessing(
        processingId,
        'screening',
        `Screening applicant ${applicantId}...`,
      )

      // Update applicant status
      const applicant = applicants.value.find((a) => a.id === applicantId)
      if (applicant) {
        applicant.status = 'Đang xử lý'
      }

      // Simulate processing with real-time updates
      simulateScreeningProcess(processingId, applicantId)
    }

    // TODO: Replace with real API call
    // await jobService.screenApplicants(currentJobId.value, selectedApplicants.value)

    alert('Yêu cầu sàng lọc đã được gửi đi. Kết quả sẽ có sau ít phút.')
  } catch (error) {
    console.error('Error screening applicants:', error)
    alert('Đã xảy ra lỗi trong quá trình sàng lọc.')
  } finally {
    isProcessing.value = false
    selectedApplicants.value = []
  }
}

const simulateScreeningProcess = (processingId: string, applicantId: number) => {
  // Simulate processing steps with real-time updates
  const steps = [
    { progress: 20, message: 'Analyzing CV content...' },
    { progress: 40, message: 'Extracting skills and experience...' },
    { progress: 60, message: 'Comparing with job requirements...' },
    { progress: 80, message: 'Generating AI assessment...' },
    { progress: 100, message: 'Finalizing results...' },
  ]

  let currentStep = 0

  const processStep = () => {
    if (currentStep < steps.length) {
      const step = steps[currentStep]
      realtimeService.updateProcessingStatus(
        processingId,
        'processing',
        step.progress,
        step.message,
      )

      currentStep++
      setTimeout(processStep, 2000) // 2 seconds per step
    } else {
      // Complete processing
      const result = {
        overallScore: Math.floor(Math.random() * 40) + 60,
        summary: 'AI analysis completed successfully',
        strengths: ['Relevant experience', 'Strong technical skills'],
        weaknesses: ['Limited leadership experience'],
      }

      realtimeService.completeProcessing(processingId, result)

      // Update applicant with results
      const applicant = applicants.value.find((a) => a.id === applicantId)
      if (applicant) {
        applicant.status = 'Hoàn thành'
        applicant.aiScore = result.overallScore
      }
    }
  }

  // Start processing after a short delay
  setTimeout(processStep, 1000)
}

// File upload methods
const handleFileUpload = async (file: File) => {
  const processingId = `upload_${Date.now()}`

  try {
    // Start upload processing
    realtimeService.startProcessing(processingId, 'upload', `Uploading ${file.name}...`)

    const result = await cvUploadService.uploadCV(0, file) // TODO: Use actual applicant ID
    if (result.success) {
      console.log('File uploaded successfully:', result.fileName)
      realtimeService.completeProcessing(processingId, { fileName: result.fileName })
    } else {
      throw new Error(result.message)
    }
  } catch (error: any) {
    console.error('File upload error:', error)
    realtimeService.failProcessing(processingId, error.message)
    throw error
  }
}

const handleUploadSuccess = (fileName: string) => {
  console.log('Upload successful:', fileName)
  // Refresh applicants list
  loadApplicants()
}

const handleUploadError = (error: string) => {
  console.error('Upload error:', error)
  alert(`Upload failed: ${error}`)
}

// Bulk upload methods
const addBulkUpload = () => {
  bulkUploads.value.push({ file: null, uploaded: false, error: null })
}

const removeBulkUpload = () => {
  if (bulkUploads.value.length > 1) {
    bulkUploads.value.pop()
  }
}

const handleBulkUpload = async (file: File, index: number) => {
  try {
    bulkUploads.value[index].file = file
    bulkUploads.value[index].error = null

    const result = await cvUploadService.uploadCV(0, file) // TODO: Use actual applicant ID
    if (result.success) {
      bulkUploads.value[index].uploaded = true
    } else {
      throw new Error(result.message)
    }
  } catch (error: any) {
    bulkUploads.value[index].error = error.message
    console.error('Bulk upload error:', error)
  }
}

const handleBulkUploadSuccess = (fileName: string, index: number) => {
  console.log('Bulk upload successful:', fileName)
  bulkUploads.value[index].uploaded = true
}

const handleBulkUploadError = (error: string, index: number) => {
  console.error('Bulk upload error:', error)
  bulkUploads.value[index].error = error
}

// Individual applicant CV methods
const uploadCVForApplicant = (applicantId: number) => {
  // TODO: Implement individual CV upload modal or redirect
  console.log('Upload CV for applicant:', applicantId)
}

const downloadCV = (applicantId: number) => {
  // TODO: Implement CV download
  console.log('Download CV for applicant:', applicantId)
}
</script>
