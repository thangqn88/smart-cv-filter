// src/stores/jobPost.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { jobService } from '../services/apiService'
import { dummyData } from '../services/apiService'

export interface JobPost {
  id: number
  title: string
  description: string
  location: string
  department: string
  employmentType: string
  experienceLevel: string
  requiredSkills: string
  preferredSkills: string
  responsibilities: string
  benefits: string
  salaryMin: number
  salaryMax: number
  status: string
  postedDate: string
  applicants?: number
}

export const useJobPostStore = defineStore('jobPost', () => {
  // State
  const jobPosts = ref<JobPost[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const currentJobPost = ref<JobPost | null>(null)

  // Getters
  const activeJobPosts = computed(() => 
    jobPosts.value.filter(job => job.status === 'Active')
  )
  
  const draftJobPosts = computed(() => 
    jobPosts.value.filter(job => job.status === 'Draft')
  )

  const totalApplicants = computed(() => 
    jobPosts.value.reduce((total, job) => total + (job.applicants || 0), 0)
  )

  // Actions
  const loadJobPosts = async (): Promise<void> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await jobService.getJobPosts()
      jobPosts.value = response.data || []
    } catch (apiError) {
      console.warn('API not available, using dummy data:', apiError)
      // Fallback to dummy data
      jobPosts.value = dummyData.jobPosts.map(job => ({
        ...job,
        department: 'Engineering',
        applicants: job.applicants || 0,
        description: 'Job description here',
        location: 'Remote',
        employmentType: 'Full-time',
        experienceLevel: 'Mid',
        requiredSkills: 'C#, .NET, Azure',
        preferredSkills: 'Vue.js, React',
        responsibilities: 'Develop and maintain applications',
        benefits: 'Health, Dental, Vision',
        salaryMin: 80000,
        salaryMax: 120000
      }))
    } finally {
      isLoading.value = false
    }
  }

  const loadJobPost = async (id: number): Promise<JobPost | null> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await jobService.getJobPost(id)
      currentJobPost.value = response.data
      return response.data
    } catch (apiError) {
      console.warn('API not available, using dummy data:', apiError)
      // Fallback to dummy data
      const job = dummyData.jobPosts.find(j => j.id === id)
      if (job) {
        currentJobPost.value = {
          ...job,
          department: 'Engineering',
          applicants: job.applicants || 0,
          description: 'Job description here',
          location: 'Remote',
          employmentType: 'Full-time',
          experienceLevel: 'Mid',
          requiredSkills: 'C#, .NET, Azure',
          preferredSkills: 'Vue.js, React',
          responsibilities: 'Develop and maintain applications',
          benefits: 'Health, Dental, Vision',
          salaryMin: 80000,
          salaryMax: 120000
        }
        return currentJobPost.value
      }
      return null
    } finally {
      isLoading.value = false
    }
  }

  const createJobPost = async (jobPostData: Partial<JobPost>): Promise<JobPost | null> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await jobService.createJobPost(jobPostData)
      const newJobPost = response.data
      jobPosts.value.push(newJobPost)
      return newJobPost
    } catch (apiError) {
      console.warn('API not available, creating dummy job post:', apiError)
      // Create dummy job post
      const newJobPost: JobPost = {
        id: Date.now(), // Simple ID generation
        title: jobPostData.title || 'New Job Post',
        description: jobPostData.description || 'Job description here',
        location: jobPostData.location || 'Remote',
        department: jobPostData.department || 'Engineering',
        employmentType: jobPostData.employmentType || 'Full-time',
        experienceLevel: jobPostData.experienceLevel || 'Mid',
        requiredSkills: jobPostData.requiredSkills || 'C#, .NET, Azure',
        preferredSkills: jobPostData.preferredSkills || 'Vue.js, React',
        responsibilities: jobPostData.responsibilities || 'Develop and maintain applications',
        benefits: jobPostData.benefits || 'Health, Dental, Vision',
        salaryMin: jobPostData.salaryMin || 80000,
        salaryMax: jobPostData.salaryMax || 120000,
        status: jobPostData.status || 'Active',
        postedDate: new Date().toISOString(),
        applicants: 0
      }
      jobPosts.value.push(newJobPost)
      return newJobPost
    } finally {
      isLoading.value = false
    }
  }

  const updateJobPost = async (id: number, jobPostData: Partial<JobPost>): Promise<JobPost | null> => {
    isLoading.value = true
    error.value = null
    
    try {
      const response = await jobService.updateJobPost(id, jobPostData)
      const updatedJobPost = response.data
      
      // Update in local list
      const index = jobPosts.value.findIndex(job => job.id === id)
      if (index !== -1) {
        jobPosts.value[index] = updatedJobPost
      }
      
      if (currentJobPost.value?.id === id) {
        currentJobPost.value = updatedJobPost
      }
      
      return updatedJobPost
    } catch (apiError) {
      console.warn('API not available, updating dummy job post:', apiError)
      // Update dummy job post
      const index = jobPosts.value.findIndex(job => job.id === id)
      if (index !== -1) {
        jobPosts.value[index] = { ...jobPosts.value[index], ...jobPostData }
        return jobPosts.value[index]
      }
      return null
    } finally {
      isLoading.value = false
    }
  }

  const deleteJobPost = async (id: number): Promise<boolean> => {
    isLoading.value = true
    error.value = null
    
    try {
      await jobService.deleteJobPost(id)
      
      // Remove from local list
      jobPosts.value = jobPosts.value.filter(job => job.id !== id)
      
      if (currentJobPost.value?.id === id) {
        currentJobPost.value = null
      }
      
      return true
    } catch (apiError) {
      console.warn('API not available, removing dummy job post:', apiError)
      // Remove dummy job post
      jobPosts.value = jobPosts.value.filter(job => job.id !== id)
      
      if (currentJobPost.value?.id === id) {
        currentJobPost.value = null
      }
      
      return true
    } finally {
      isLoading.value = false
    }
  }

  const clearError = (): void => {
    error.value = null
  }

  const clearCurrentJobPost = (): void => {
    currentJobPost.value = null
  }

  return {
    // State
    jobPosts,
    isLoading,
    error,
    currentJobPost,
    
    // Getters
    activeJobPosts,
    draftJobPosts,
    totalApplicants,
    
    // Actions
    loadJobPosts,
    loadJobPost,
    createJobPost,
    updateJobPost,
    deleteJobPost,
    clearError,
    clearCurrentJobPost
  }
})
