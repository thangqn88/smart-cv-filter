// src/services/cvUploadService.ts
import { apiService } from './apiService'

export interface CVUploadResult {
  success: boolean
  fileName?: string
  fileId?: string
  message?: string
}

export interface CVUploadProgress {
  loaded: number
  total: number
  percentage: number
}

class CVUploadService {
  /**
   * Upload a CV file for a specific applicant
   */
  async uploadCV(
    applicantId: number, 
    file: File, 
    onProgress?: (progress: CVUploadProgress) => void
  ): Promise<CVUploadResult> {
    try {
      const formData = new FormData()
      formData.append('file', file)
      
      const response = await apiService.uploadCV(applicantId, file)
      
      return {
        success: true,
        fileName: file.name,
        fileId: response.data?.id || response.data?.fileId,
        message: 'CV uploaded successfully'
      }
    } catch (error: any) {
      console.error('CV upload error:', error)
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to upload CV file'
      }
    }
  }

  /**
   * Get CV file information for an applicant
   */
  async getCVInfo(applicantId: number): Promise<{ success: boolean; data?: any; message?: string }> {
    try {
      const response = await apiService.getApplicant(applicantId)
      return {
        success: true,
        data: response.data?.cvFile
      }
    } catch (error: any) {
      console.error('Get CV info error:', error)
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to get CV information'
      }
    }
  }

  /**
   * Delete a CV file
   */
  async deleteCV(applicantId: number): Promise<{ success: boolean; message?: string }> {
    try {
      await apiService.deleteApplicant(applicantId) // This might need to be a specific CV delete endpoint
      return {
        success: true,
        message: 'CV deleted successfully'
      }
    } catch (error: any) {
      console.error('Delete CV error:', error)
      return {
        success: false,
        message: error.response?.data?.message || 'Failed to delete CV file'
      }
    }
  }

  /**
   * Validate file before upload
   */
  validateFile(file: File, maxSizeMB: number = 10): { valid: boolean; message?: string } {
    // Check file size
    const maxSizeBytes = maxSizeMB * 1024 * 1024
    if (file.size > maxSizeBytes) {
      return {
        valid: false,
        message: `File size must be less than ${maxSizeMB}MB`
      }
    }

    // Check file type
    const allowedTypes = ['.pdf', '.doc', '.docx']
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase()
    if (!allowedTypes.includes(fileExtension)) {
      return {
        valid: false,
        message: `File type must be one of: ${allowedTypes.join(', ')}`
      }
    }

    return { valid: true }
  }

  /**
   * Format file size for display
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  /**
   * Get file icon based on file type
   */
  getFileIcon(fileName: string): string {
    const extension = fileName.split('.').pop()?.toLowerCase()
    switch (extension) {
      case 'pdf':
        return 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
      case 'doc':
      case 'docx':
        return 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
      default:
        return 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
    }
  }
}

// Create and export a singleton instance
export const cvUploadService = new CVUploadService()

// Export the class for testing
export { CVUploadService }
