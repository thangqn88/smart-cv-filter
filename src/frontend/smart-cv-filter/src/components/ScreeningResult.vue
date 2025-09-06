<template>
  <div class="screening-result-page p-6 bg-gray-100 min-h-screen">
    <button @click="$router.go(-1)" class="text-blue-600 hover:underline mb-4">
      <i class="fas fa-arrow-left mr-2"></i> Quay lại
    </button>

    <h1 class="text-3xl font-bold mb-6 text-gray-800">Kết quả Sàng lọc CV</h1>
    <div class="bg-white rounded-lg shadow-xl p-6">
      <div v-if="result">
        <div class="mb-6 pb-4 border-b border-gray-200">
          <h2 class="text-xl font-semibold text-gray-700">
            Ứng viên: {{ result.applicantName }}
          </h2>
          <p class="text-gray-500">Vị trí ứng tuyển: {{ result.jobTitle }}</p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div class="bg-blue-50 p-6 rounded-lg shadow-inner">
            <h3 class="text-lg font-bold text-blue-800 mb-4 flex items-center">
              <i class="fas fa-star mr-2 text-yellow-500"></i> Điểm số tổng quan
            </h3>
            <div class="text-center">
              <p class="text-6xl font-bold text-blue-600">
                {{ result.overallScore }}<span class="text-3xl">%</span>
              </p>
            </div>
            <p class="mt-4 text-center text-gray-700 italic">
              "{{ result.summary }}"
            </p>
          </div>

          <div>
            <div class="bg-green-50 p-6 rounded-lg shadow-inner mb-4">
              <h3
                class="text-lg font-bold text-green-800 mb-4 flex items-center"
              >
                <i class="fas fa-check-circle mr-2"></i> Điểm mạnh từ CV
              </h3>
              <ul class="list-disc list-inside text-gray-700">
                <li v-for="(strength, index) in result.strengths" :key="index">
                  {{ strength }}
                </li>
              </ul>
            </div>

            <div class="bg-red-50 p-6 rounded-lg shadow-inner">
              <h3 class="text-lg font-bold text-red-800 mb-4 flex items-center">
                <i class="fas fa-times-circle mr-2"></i> Điểm yếu/Cần cải thiện
              </h3>
              <ul class="list-disc list-inside text-gray-700">
                <li v-for="(weakness, index) in result.weaknesses" :key="index">
                  {{ weakness }}
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
      <div v-else class="text-center text-gray-500">
        <p>Không tìm thấy kết quả sàng lọc cho ứng viên này.</p>
      </div>
    </div>
  </div>
</template>

<script>
import { jobService, dummyData } from "../services/apiService";

export default {
  name: "ScreeningResult",
  props: ["resultId"],
  data() {
    return {
      result: null,
    };
  },
  async created() {
    // Sử dụng dữ liệu mẫu
    this.result = dummyData.screeningResult;

    // Gọi API thật để lấy kết quả
    // try {
    //   const response = await jobService.getScreeningResults(this.resultId);
    //   this.result = response.data;
    // } catch (error) {
    //   console.error("Lỗi khi tải kết quả sàng lọc:", error);
    //   // Fallback to dummy data
    //   this.result = dummyData.screeningResult;
    // }
  },
};
</script>
