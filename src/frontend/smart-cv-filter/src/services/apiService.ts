// src/services/apiService.ts
import axios, { AxiosInstance, AxiosResponse } from 'axios'
import type { LoginRequest, RegisterRequest, AuthResponse, User } from '../types/auth'

const API_BASE_URL = 'http://localhost:5000/api'

class ApiService {
  private api: AxiosInstance

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token')
        if (token) {
          config.headers.Authorization = `Bearer ${token}`
        }
        return config
      },
      (error) => {
        return Promise.reject(error)
      }
    )

    // Response interceptor to handle auth errors
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Token expired or invalid, clear it
          localStorage.removeItem('token')
          window.location.href = '/login'
        }
        return Promise.reject(error)
      }
    )
  }

  // Auth methods
  async login(credentials: LoginRequest): Promise<{ success: boolean; data?: AuthResponse; message?: string }> {
    try {
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/login', credentials)
      return { success: true, data: response.data }
    } catch (error: any) {
      return { 
        success: false, 
        message: error.response?.data?.message || 'Login failed' 
      }
    }
  }

  async register(userData: RegisterRequest): Promise<{ success: boolean; data?: AuthResponse; message?: string }> {
    try {
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register', userData)
      return { success: true, data: response.data }
    } catch (error: any) {
      return { 
        success: false, 
        message: error.response?.data?.message || 'Registration failed' 
      }
    }
  }

  async logout(): Promise<void> {
    try {
      await this.api.post('/auth/logout')
    } catch (error) {
      console.error('Logout error:', error)
    }
  }

  async validateToken(): Promise<{ success: boolean; data?: User; message?: string }> {
    try {
      const response: AxiosResponse<User> = await this.api.get('/auth/userinfo')
      return { success: true, data: response.data }
    } catch (error: any) {
      return { 
        success: false, 
        message: error.response?.data?.message || 'Token validation failed' 
      }
    }
  }

  // Job service methods
  async getJobPosts() {
    return this.api.get('/jobposts')
  }

  async getJobPost(id: number) {
    return this.api.get(`/jobposts/${id}`)
  }

  async createJobPost(jobPost: any) {
    return this.api.post('/jobposts', jobPost)
  }

  async updateJobPost(id: number, jobPost: any) {
    return this.api.put(`/jobposts/${id}`, jobPost)
  }

  async deleteJobPost(id: number) {
    return this.api.delete(`/jobposts/${id}`)
  }

  // Applicant service methods
  async getApplicants(jobId: number) {
    return this.api.get(`/jobposts/${jobId}/applicants`)
  }

  async getApplicant(id: number) {
    return this.api.get(`/applicants/${id}`)
  }

  async createApplicant(applicant: any) {
    return this.api.post('/applicants', applicant)
  }

  async updateApplicant(id: number, applicant: any) {
    return this.api.put(`/applicants/${id}`, applicant)
  }

  async deleteApplicant(id: number) {
    return this.api.delete(`/applicants/${id}`)
  }

  // CV Upload methods
  async uploadCV(applicantId: number, file: File) {
    const formData = new FormData()
    formData.append('file', file)
    return this.api.post(`/applicants/${applicantId}/cv`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
  }

  // Screening methods
  async screenApplicants(jobId: number, applicantIds: number[]) {
    return this.api.post(`/jobposts/${jobId}/screen`, { applicantIds })
  }

  async getScreeningResults(resultId: number) {
    return this.api.get(`/screening-results/${resultId}`)
  }

  async getScreeningResult(applicantId: number) {
    return this.api.get(`/applicants/${applicantId}/screening-result`)
  }

  // Utility methods
  setAuthToken(token: string): void {
    this.api.defaults.headers.common['Authorization'] = `Bearer ${token}`
  }

  clearAuthToken(): void {
    delete this.api.defaults.headers.common['Authorization']
  }
}

// Create and export a singleton instance
export const apiService = new ApiService()

// Export individual service objects for backward compatibility
export const jobService = {
  getJobPosts: () => apiService.getJobPosts(),
  getJobPost: (id: number) => apiService.getJobPost(id),
  createJobPost: (jobPost: any) => apiService.createJobPost(jobPost),
  updateJobPost: (id: number, jobPost: any) => apiService.updateJobPost(id, jobPost),
  deleteJobPost: (id: number) => apiService.deleteJobPost(id),
  getApplicants: (jobId: number) => apiService.getApplicants(jobId),
  screenApplicants: (jobId: number, applicantIds: number[]) => apiService.screenApplicants(jobId, applicantIds),
  getScreeningResults: (resultId: number) => apiService.getScreeningResults(resultId),
}

export const applicantService = {
  getApplicants: (jobId: number) => apiService.getApplicants(jobId),
  getApplicant: (id: number) => apiService.getApplicant(id),
  createApplicant: (applicant: any) => apiService.createApplicant(applicant),
  updateApplicant: (id: number, applicant: any) => apiService.updateApplicant(id, applicant),
  deleteApplicant: (id: number) => apiService.deleteApplicant(id),
  uploadCV: (applicantId: number, file: File) => apiService.uploadCV(applicantId, file),
  getScreeningResult: (applicantId: number) => apiService.getScreeningResult(applicantId),
}

// Dummy data for development/testing
export const dummyData = {
  jobPosts: [
    {
      id: 1,
      title: 'Senior .NET Developer',
      applicants: 50,
      status: 'Đang hoạt động',
      postedDate: '2025-09-01',
    },
    {
      id: 2,
      title: 'Vue.js Frontend Engineer',
      applicants: 32,
      status: 'Đang hoạt động',
      postedDate: '2025-08-20',
    },
  ],
  applicants: [
    {
      id: 101,
      name: 'Nguyễn Văn A',
      email: 'vana@example.com',
      submittedDate: '2025-09-05',
      aiScore: null,
      status: 'Chưa xử lý',
    },
    {
      id: 102,
      name: 'Trần Thị B',
      email: 'thib@example.com',
      submittedDate: '2025-09-04',
      aiScore: 85,
      status: 'Hoàn thành',
    },
    {
      id: 103,
      name: 'Lê Văn C',
      email: 'vanc@example.com',
      submittedDate: '2025-09-03',
      aiScore: null,
      status: 'Đang xử lý',
    },
  ],
  screeningResult: {
    id: 1,
    applicantName: 'Trần Thị B',
    jobTitle: 'Senior .NET Developer',
    overallScore: 85,
    summary:
      'Ứng viên có kinh nghiệm vững về .NET và kiến thức sâu về PostgreSQL. Phù hợp với 85% yêu cầu công việc.',
    strengths: [
      '5 năm kinh nghiệm .NET 8',
      'Thông thạo PostgreSQL',
      'Có kinh nghiệm với hệ thống phân tán',
    ],
    weaknesses: [
      'Thiếu kinh nghiệm với Vue.js',
      'Kinh nghiệm quản lý dự án còn hạn chế',
    ],
  },
}
