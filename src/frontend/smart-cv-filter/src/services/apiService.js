// src/services/apiService.js
import axios from "axios";

const API_BASE_URL = "http://localhost:5000/api"; // Thay đổi nếu API của bạn chạy trên một port khác

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export const jobService = {
  getJobPosts() {
    return api.get("/jobposts");
  },
  getApplicants(jobId) {
    return api.get(`/jobposts/${jobId}/applicants`);
  },
  screenApplicants(jobId, applicantIds) {
    return api.post(`/jobposts/${jobId}/screen`, { applicantIds });
  },
  getScreeningResults(resultId) {
    return api.get(`/screening-results/${resultId}`);
  },
};

export const dummyData = {
  jobPosts: [
    {
      id: 1,
      title: "Senior .NET Developer",
      applicants: 50,
      status: "Đang hoạt động",
      postedDate: "2025-09-01",
    },
    {
      id: 2,
      title: "Vue.js Frontend Engineer",
      applicants: 32,
      status: "Đang hoạt động",
      postedDate: "2025-08-20",
    },
  ],
  applicants: [
    {
      id: 101,
      name: "Nguyễn Văn A",
      email: "vana@example.com",
      submittedDate: "2025-09-05",
      aiScore: null,
      status: "Chưa xử lý",
    },
    {
      id: 102,
      name: "Trần Thị B",
      email: "thib@example.com",
      submittedDate: "2025-09-04",
      aiScore: 85,
      status: "Hoàn thành",
    },
    {
      id: 103,
      name: "Lê Văn C",
      email: "vanc@example.com",
      submittedDate: "2025-09-03",
      aiScore: null,
      status: "Đang xử lý",
    },
  ],
  screeningResult: {
    id: 1,
    applicantName: "Trần Thị B",
    jobTitle: "Senior .NET Developer",
    overallScore: 85,
    summary:
      "Ứng viên có kinh nghiệm vững về .NET và kiến thức sâu về PostgreSQL. Phù hợp với 85% yêu cầu công việc.",
    strengths: [
      "5 năm kinh nghiệm .NET 8",
      "Thông thạo PostgreSQL",
      "Có kinh nghiệm với hệ thống phân tán",
    ],
    weaknesses: [
      "Thiếu kinh nghiệm với Vue.js",
      "Kinh nghiệm quản lý dự án còn hạn chế",
    ],
  },
};
