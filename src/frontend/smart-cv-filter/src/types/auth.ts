export interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  companyName: string
  role: string
  createdAt: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
  companyName: string
}

export interface AuthResponse {
  token: string
  user: User
  expiration: string
}

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  errors?: Record<string, string[]>
}
