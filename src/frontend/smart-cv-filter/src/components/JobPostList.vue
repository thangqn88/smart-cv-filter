<template>
  <div class="job-post-list-page p-6 bg-gray-100 min-h-screen">
    <h1 class="text-3xl font-bold mb-6 text-gray-800">
      Quản lý Tin tuyển dụng
    </h1>
    <div class="flex justify-end mb-4">
      <button
        class="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-colors"
      >
        <i class="fas fa-plus mr-2"></i> Đăng tin tuyển dụng mới
      </button>
    </div>

    <div class="bg-white rounded-lg shadow-xl p-4">
      <table class="min-w-full table-auto">
        <thead>
          <tr
            class="bg-gray-200 text-gray-600 uppercase text-sm leading-normal"
          >
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
              {{ job.title }}
            </td>
            <td class="py-3 px-6 text-left">{{ job.applicants }} hồ sơ</td>
            <td class="py-3 px-6 text-left">
              <span
                class="bg-green-200 text-green-600 py-1 px-3 rounded-full text-xs font-semibold"
                >{{ job.status }}</span
              >
            </td>
            <td class="py-3 px-6 text-left">{{ job.postedDate }}</td>
            <td class="py-3 px-6 text-center">
              <router-link
                :to="{ name: 'ApplicantList', params: { jobId: job.id } }"
                class="bg-indigo-500 hover:bg-indigo-600 text-white py-1 px-3 rounded-md text-xs font-semibold"
              >
                Xem ứng viên
              </router-link>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { jobService, dummyData } from "../services/apiService";

export default {
  name: "JobPostList",
  data() {
    return {
      jobPosts: [],
    };
  },
  async created() {
    // Sử dụng dữ liệu mẫu nếu không có backend
    this.jobPosts = dummyData.jobPosts;

    // Sử dụng API thật khi backend đã sẵn sàng
    // try {
    //   const response = await jobService.getJobPosts();
    //   this.jobPosts = response.data;
    // } catch (error) {
    //   console.error("Lỗi khi tải danh sách tin tuyển dụng:", error);
    //   // Fallback to dummy data
    //   this.jobPosts = dummyData.jobPosts;
    // }
  },
};
</script>
