<template>
  <div class="applicant-list-page p-6 bg-gray-100 min-h-screen">
    <h1 class="text-3xl font-bold mb-6 text-gray-800">Danh sách Ứng viên</h1>
    <h2 class="text-xl font-semibold mb-4 text-gray-700">
      Vị trí: {{ jobTitle }}
    </h2>

    <div class="flex justify-between mb-4">
      <button
        class="bg-green-600 hover:bg-green-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-colors"
      >
        <i class="fas fa-upload mr-2"></i> Tải lên CV
      </button>
      <button
        @click="screenApplicants"
        :disabled="selectedApplicants.length === 0 || isProcessing"
        class="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded shadow-lg transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
      >
        <i class="fas fa-robot mr-2"></i> Sàng lọc bằng AI ({{
          selectedApplicants.length
        }})
      </button>
    </div>

    <div class="bg-white rounded-lg shadow-xl p-4">
      <table class="min-w-full table-auto">
        <thead>
          <tr
            class="bg-gray-200 text-gray-600 uppercase text-sm leading-normal"
          >
            <th class="py-3 px-6 text-center"></th>
            <th class="py-3 px-6 text-left">Tên ứng viên</th>
            <th class="py-3 px-6 text-left">Email</th>
            <th class="py-3 px-6 text-left">Ngày nộp</th>
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
              <input
                type="checkbox"
                :value="applicant.id"
                v-model="selectedApplicants"
              />
            </td>
            <td class="py-3 px-6 text-left">{{ applicant.name }}</td>
            <td class="py-3 px-6 text-left">{{ applicant.email }}</td>
            <td class="py-3 px-6 text-left">{{ applicant.submittedDate }}</td>
            <td class="py-3 px-6 text-left">
              <span v-if="applicant.aiScore">{{ applicant.aiScore }}%</span>
              <span v-else>-</span>
            </td>
            <td class="py-3 px-6 text-left">
              <span
                :class="{
                  'bg-yellow-200 text-yellow-600':
                    applicant.status === 'Đang xử lý',
                  'bg-green-200 text-green-600':
                    applicant.status === 'Hoàn thành',
                  'bg-gray-200 text-gray-600':
                    applicant.status === 'Chưa xử lý',
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
  </div>
</template>

<script>
import { jobService, dummyData } from "../services/apiService";

export default {
  name: "ApplicantList",
  props: ["jobId"],
  data() {
    return {
      applicants: [],
      selectedApplicants: [],
      jobTitle: "",
      isProcessing: false,
    };
  },
  async created() {
    // Sử dụng dữ liệu mẫu
    this.applicants = dummyData.applicants;
    const job = dummyData.jobPosts.find((j) => j.id == this.jobId);
    this.jobTitle = job ? job.title : "Vị trí không xác định";

    // Gọi API thật để lấy danh sách ứng viên
    // try {
    //   const response = await jobService.getApplicants(this.jobId);
    //   this.applicants = response.data;
    // } catch (error) {
    //   console.error("Lỗi khi tải danh sách ứng viên:", error);
    // }
  },
  methods: {
    async screenApplicants() {
      this.isProcessing = true;
      try {
        // Gửi request tới backend để bắt đầu quá trình sàng lọc
        // await jobService.screenApplicants(this.jobId, this.selectedApplicants);
        // Cập nhật trạng thái của các ứng viên được chọn
        this.applicants = this.applicants.map((applicant) => {
          if (this.selectedApplicants.includes(applicant.id)) {
            return { ...applicant, status: "Đang xử lý" };
          }
          return applicant;
        });
        alert("Yêu cầu sàng lọc đã được gửi đi. Kết quả sẽ có sau ít phút.");
      } catch (error) {
        console.error("Lỗi khi sàng lọc CV:", error);
        alert("Đã xảy ra lỗi trong quá trình sàng lọc.");
      } finally {
        this.isProcessing = false;
        this.selectedApplicants = [];
      }
    },
  },
};
</script>
