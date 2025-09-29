// src/stores/applicant.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { applicantService } from '../services/apiService'
import { dummyData } from '../services/apiService'

export interface Applicant {
  id: number
  jobPostId: number
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  linkedInProfile?: string
  portfolioUrl?: string
  coverLetter?: string
  status: string
  appliedDate: string
  cvFile?: {
    id: string
    name: string
    size: number
    type: string
    url: string
  }
  aiScore?: number
  screeningResult?: {
    id: number
    overallScore: number
    summary: string
    strengths: string[]
    weaknesses: string[]
    detailedAnalysis: string
    createdAt: string
  }
}

export const useApplicantStore = defineStore('applicant', () => {
  // State
  const applicants = ref<Applicant[]>([])
  const currentApplicant = ref<Applicant | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const applicantsByJob = computed(() => (jobId: number) => 
    applicants.value.filter(applicant => applicant.jobPostId === jobId)
  )
  
  const screenedApplicants = computed(() => 
    applicants.value.filter(applicant => applicant.status === 'Screened')
  )

  const pendingApplicants = computed(() => 
    applicants.value.filter(applicant => applicant.status === 'Applied')
  )

  const totalApplicants = computed(() => applicants.value.length)

  // Actions
  const loadApplicants = async (jobId?: number): Promise<void> => {
    isLoading.value = true
    error.value = null
    
    try {
      if (jobId) {
        const response = await applicantService.getApplicants(jobId)
        applicants.value = response.data || []
      } else {
        // Load all applicants (if API supports it)
        applicants.value = []
      }
    } catch (apiError) {
      console.warn('API not available, using dummy data:', apiError)
      // Fallback to dummy data
      applicants.value = dummyData.applicants.map(applicant => ({
        ...applicant,
        jobPostId: jobId || 1,
        firstName: applicant.name.split(' ')[0],
        lastName: applicant.name.split(' ').slice(1).join(' '),
        phoneNumber: '123-456-7890',
        linkedInProfile: 'linkedin.com/in/example',
        portfolioUrl: 'portfolio.com/example',
        coverLetter: 'Cover letter content here',
        status: applicant.status === 'Hoàn thành' ? 'Screened' : 
                applicant.status === 'Đang xử lý' ? 'Screening' : 'Applied',
        appliedDate: applicant.submittedDate,
        aiScore: applicant.aiScore,
        cvFile: applicant.cvFile || null
      }))
    } finally {
      isLoading.value = false
    }
  }

  const loadApplicant = async (id: number): Promise<Applicant | null> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await applicantService.getApplicant(id)
      currentApplicant.value = response.data
      return response.data
    } catch (apiError) {
      console.warn('API not available, using dummy data:', apiError)
      // Fallback to dummy data
      const applicant = dummyData.applicants.find(a => a.id === id)
      if (applicant) {
        currentApplicant.value = {
          ...applicant,
          jobPostId: 1,
          firstName: applicant.name.split(' ')[0],
          lastName: applicant.name.split(' ').slice(1).join(' '),
          phoneNumber: '123-456-7890',
          linkedInProfile: 'linkedin.com/in/example',
          portfolioUrl: 'portfolio.com/example',
          coverLetter: 'Cover letter content here',
          status: applicant.status === 'Hoàn thành' ? 'Screened' : 
                  applicant.status === 'Đang xử lý' ? 'Screening' : 'Applied',
          appliedDate: applicant.submittedDate,
          aiScore: applicant.aiScore,
          cvFile: applicant.cvFile || null
        }
        return currentApplicant.value
      }
      return null
    } finally {
      isLoading.value = false
    }
  }

  const createApplicant = async (applicantData: Partial<Applicant>): Promise<Applicant | null> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await applicantService.createApplicant(applicantData)
      const newApplicant = response.data
      applicants.value.push(newApplicant)
      return newApplicant
    } catch (apiError) {
      console.warn('API not available, creating dummy applicant:', apiError)
      // Create dummy applicant
      const newApplicant: Applicant = {
        id: Date.now(),
        jobPostId: applicantData.jobPostId || 1,
        firstName: applicantData.firstName || 'New',
        lastName: applicantData.lastName || 'Applicant',
        email: applicantData.email || 'applicant@example.com',
        phoneNumber: applicantData.phoneNumber || '123-456-7890',
        linkedInProfile: applicantData.linkedInProfile,
        portfolioUrl: applicantData.portfolioUrl,
        coverLetter: applicantData.coverLetter,
        status: applicantData.status || 'Applied',
        appliedDate: new Date().toISOString(),
        cvFile: applicantData.cvFile
      }
      applicants.value.push(newApplicant)
      return newApplicant
    } finally {
      isLoading.value = false
    }
  }

  const updateApplicant = async (id: number, applicantData: Partial<Applicant>): Promise<Applicant | null> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await applicantService.updateApplicant(id, applicantData)
      const updatedApplicant = response.data
      
      // Update in local list
      const index = applicants.value.findIndex(applicant => applicant.id === id)
      if (index !== -1) {
        applicants.value[index] = updatedApplicant
      }
      
      if (currentApplicant.value?.id === id) {
        currentApplicant.value = updatedApplicant
      }
      
      return updatedApplicant
    } catch (apiError) {
      console.warn('API not available, updating dummy applicant:', apiError)
      // Update dummy applicant
      const index = applicants.value.findIndex(applicant => applicant.id === id)
      if (index !== -1) {
        applicants.value[index] = { ...applicants.value[index], ...applicantData }
        return applicants.value[index]
      }
      return null
    } finally {
      isLoading.value = false
    }
  }

  const deleteApplicant = async (id: number): Promise<boolean> => {
    isLoading.value = true
    error.value = null
    
    try {
      await applicantService.deleteApplicant(id)
      
      // Remove from local list
      applicants.value = applicants.value.filter(applicant => applicant.id !== id)
      
      if (currentApplicant.value?.id === id) {
        currentApplicant.value = null
      }
      
      return true
    } catch (apiError) {
      console.warn('API not available, removing dummy applicant:', apiError)
      // Remove dummy applicant
      applicants.value = applicants.value.filter(applicant => applicant.id !== id)
      
      if (currentApplicant.value?.id === id) {
        currentApplicant.value = null
      }
      
      return true
    } finally {
      isLoading.value = false
    }
  }

  const uploadCV = async (applicantId: number, file: File): Promise<boolean> => {
    isLoading.value = true
    error.value = null
    
    try {
      await applicantService.uploadCV(applicantId, file)
      
      // Update applicant in local list
      const applicant = applicants.value.find(a => a.id === applicantId)
      if (applicant) {
        applicant.cvFile = {
          id: Date.now().toString(),
          name: file.name,
          size: file.size,
          type: file.type,
          url: URL.createObjectURL(file)
        }
      }
      
      return true
    } catch (apiError) {
      console.warn('API not available, simulating CV upload:', apiError)
      // Simulate CV upload
      const applicant = applicants.value.find(a => a.id === applicantId)
      if (applicant) {
        applicant.cvFile = {
          id: Date.now().toString(),
          name: file.name,
          size: file.size,
          type: file.type,
          url: URL.createObjectURL(file)
        }
      }
      return true
    } finally {
      isLoading.value = false
    }
  }

  const getScreeningResult = async (applicantId: number): Promise<any> => {
    try {
      const response = await applicantService.getScreeningResult(applicantId)
      return response.data
    } catch (apiError) {
      console.warn('API not available, using dummy screening result:', apiError)
      // Return dummy screening result
      return dummyData.screeningResult
    }
  }

  const clearError = (): void => {
    error.value = null
  }

  const clearCurrentApplicant = (): void => {
    currentApplicant.value = null
  }

  return {
    // State
    applicants,
    currentApplicant,
    isLoading,
    error,
    
    // Getters
    applicantsByJob,
    screenedApplicants,
    pendingApplicants,
    totalApplicants,
    
    // Actions
    loadApplicants,
    loadApplicant,
    createApplicant,
    updateApplicant,
    deleteApplicant,
    uploadCV,
    getScreeningResult,
    clearError,
    clearCurrentApplicant
  }
})
