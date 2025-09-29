<template>
  <div class="file-upload-container">
    <!-- Upload Area -->
    <div
      ref="dropZone"
      class="file-upload-area"
      :class="{
        'drag-over': isDragOver,
        uploading: isUploading,
        error: hasError,
        success: isSuccess,
      }"
      @dragover.prevent="handleDragOver"
      @dragleave.prevent="handleDragLeave"
      @drop.prevent="handleDrop"
      @click="triggerFileInput"
    >
      <input
        ref="fileInput"
        type="file"
        accept=".pdf,.doc,.docx"
        @change="handleFileSelect"
        class="hidden"
      />

      <div class="upload-content">
        <div v-if="isUploading" class="upload-progress">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
          <p class="mt-2 text-sm text-gray-600">Uploading...</p>
          <div v-if="uploadProgress > 0" class="w-full bg-gray-200 rounded-full h-2 mt-2">
            <div
              class="bg-blue-600 h-2 rounded-full transition-all duration-300"
              :style="{ width: `${uploadProgress}%` }"
            ></div>
          </div>
        </div>

        <div v-else-if="hasError" class="upload-error">
          <svg
            class="w-12 h-12 text-red-500 mx-auto"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            ></path>
          </svg>
          <p class="mt-2 text-sm text-red-600">{{ errorMessage }}</p>
          <button
            @click.stop="resetUpload"
            class="mt-2 px-4 py-2 bg-red-600 text-white text-sm rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
          >
            Try Again
          </button>
        </div>

        <div v-else-if="isSuccess" class="upload-success">
          <svg
            class="w-12 h-12 text-green-500 mx-auto"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M5 13l4 4L19 7"
            ></path>
          </svg>
          <p class="mt-2 text-sm text-green-600">File uploaded successfully!</p>
          <p class="text-xs text-gray-500">{{ uploadedFileName }}</p>
          <button
            @click.stop="resetUpload"
            class="mt-2 px-4 py-2 bg-green-600 text-white text-sm rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
          >
            Upload Another
          </button>
        </div>

        <div v-else class="upload-prompt">
          <svg
            class="w-12 h-12 text-gray-400 mx-auto"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
            ></path>
          </svg>
          <p class="mt-2 text-sm font-medium text-gray-900">
            {{ dragText }}
          </p>
          <p class="text-xs text-gray-500">
            or <span class="text-blue-600 cursor-pointer">browse files</span>
          </p>
          <p class="text-xs text-gray-400 mt-1">PDF, DOC, DOCX up to {{ maxFileSizeMB }}MB</p>
        </div>
      </div>
    </div>

    <!-- File Preview -->
    <div v-if="selectedFile && !isUploading && !isSuccess" class="file-preview">
      <div class="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
        <div class="flex items-center space-x-3">
          <div class="flex-shrink-0">
            <svg
              class="w-8 h-8 text-gray-400"
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
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">{{ selectedFile.name }}</p>
            <p class="text-xs text-gray-500">{{ formatFileSize(selectedFile.size) }}</p>
          </div>
        </div>
        <div class="flex items-center space-x-2">
          <button
            @click="uploadFile"
            :disabled="isUploading"
            class="px-3 py-1 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Upload
          </button>
          <button
            @click="removeFile"
            class="p-1 text-gray-400 hover:text-red-500 focus:outline-none"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              ></path>
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Error Message -->
    <div
      v-if="hasError && errorMessage"
      class="mt-2 p-3 bg-red-50 border border-red-200 rounded-md"
    >
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
          <p class="text-sm text-red-800">{{ errorMessage }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

// Props
interface Props {
  maxFileSizeMB?: number
  acceptedTypes?: string[]
  disabled?: boolean
  dragText?: string
}

const props = withDefaults(defineProps<Props>(), {
  maxFileSizeMB: 10,
  acceptedTypes: () => ['.pdf', '.doc', '.docx'],
  disabled: false,
  dragText: 'Drop your CV file here',
})

// Emits
const emit = defineEmits<{
  upload: [file: File]
  success: [fileName: string]
  error: [message: string]
  progress: [progress: number]
}>()

// Refs
const fileInput = ref<HTMLInputElement>()
const dropZone = ref<HTMLElement>()

// State
const selectedFile = ref<File | null>(null)
const isDragOver = ref(false)
const isUploading = ref(false)
const isSuccess = ref(false)
const hasError = ref(false)
const errorMessage = ref('')
const uploadProgress = ref(0)
const uploadedFileName = ref('')

// Computed
const maxFileSizeBytes = computed(() => props.maxFileSizeMB * 1024 * 1024)

// Methods
const triggerFileInput = () => {
  if (props.disabled || isUploading.value) return
  fileInput.value?.click()
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    selectFile(file)
  }
}

const handleDragOver = (event: DragEvent) => {
  if (props.disabled || isUploading.value) return
  event.preventDefault()
  isDragOver.value = true
}

const handleDragLeave = (event: DragEvent) => {
  if (props.disabled || isUploading.value) return
  event.preventDefault()
  isDragOver.value = false
}

const handleDrop = (event: DragEvent) => {
  if (props.disabled || isUploading.value) return
  event.preventDefault()
  isDragOver.value = false

  const files = event.dataTransfer?.files
  if (files && files.length > 0) {
    selectFile(files[0])
  }
}

const selectFile = (file: File) => {
  // Reset states
  resetUpload()

  // Validate file
  if (!validateFile(file)) {
    return
  }

  selectedFile.value = file
}

const validateFile = (file: File): boolean => {
  // Check file size
  if (file.size > maxFileSizeBytes.value) {
    showError(`File size must be less than ${props.maxFileSizeMB}MB`)
    return false
  }

  // Check file type
  const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase()
  if (!props.acceptedTypes.includes(fileExtension)) {
    showError(`File type must be one of: ${props.acceptedTypes.join(', ')}`)
    return false
  }

  return true
}

const uploadFile = async () => {
  if (!selectedFile.value || isUploading.value) return

  isUploading.value = true
  hasError.value = false
  errorMessage.value = ''
  uploadProgress.value = 0

  try {
    // Simulate upload progress
    const progressInterval = setInterval(() => {
      if (uploadProgress.value < 90) {
        uploadProgress.value += Math.random() * 10
      }
    }, 200)

    // Emit upload event
    emit('upload', selectedFile.value)

    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 2000))

    clearInterval(progressInterval)
    uploadProgress.value = 100

    // Success
    isSuccess.value = true
    uploadedFileName.value = selectedFile.value.name
    emit('success', selectedFile.value.name)
  } catch (error: any) {
    showError(error.message || 'Upload failed')
    emit('error', error.message || 'Upload failed')
  } finally {
    isUploading.value = false
  }
}

const removeFile = () => {
  selectedFile.value = null
  resetUpload()
}

const resetUpload = () => {
  isUploading.value = false
  isSuccess.value = false
  hasError.value = false
  errorMessage.value = ''
  uploadProgress.value = 0
  uploadedFileName.value = ''
  selectedFile.value = null

  // Reset file input
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const showError = (message: string) => {
  hasError.value = true
  errorMessage.value = message
  emit('error', message)
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}
</script>

<style scoped>
.file-upload-container {
  @apply w-full;
}

.file-upload-area {
  @apply border-2 border-dashed border-gray-300 rounded-lg p-8 text-center cursor-pointer transition-colors;
}

.file-upload-area:hover {
  @apply border-gray-400 bg-gray-50;
}

.file-upload-area.drag-over {
  @apply border-blue-500 bg-blue-50;
}

.file-upload-area.uploading {
  @apply border-blue-500 bg-blue-50 cursor-not-allowed;
}

.file-upload-area.error {
  @apply border-red-500 bg-red-50;
}

.file-upload-area.success {
  @apply border-green-500 bg-green-50;
}

.upload-content {
  @apply flex flex-col items-center justify-center;
}

.file-preview {
  @apply mt-4;
}
</style>
