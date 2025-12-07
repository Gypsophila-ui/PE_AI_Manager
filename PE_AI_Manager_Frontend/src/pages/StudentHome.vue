<template>
  <div class="min-h-screen bg-white">
    <div class="max-w-6xl mx-auto p-6 space-y-8">

      <!-- 顶部 Banner -->
      <div class="relative w-full rounded-3xl overflow-hidden shadow-2xl">
        <img src="../assets/HomeHeader.jpg" class="w-full h-96 object-cover opacity-50" />
        <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent">
          <div class="absolute inset-0 flex flex-col items-center justify-center">
            <h1 class="text-6xl font-bold tracking-widest text-white drop-shadow-2xl">
              智慧运动课堂
            </h1>
            <p class="mt-4 text-xl text-white font-medium">记录每一次进步，见证运动的力量</p>
          </div>
          <div class="absolute bottom-5 left-0 right-0 flex justify-center gap-6">
            <button class="bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 px-8 rounded-xl shadow-lg transition-all duration-300 transform hover:scale-105">
              查看课程
            </button>
            <button class="bg-green-600 hover:bg-green-700 text-white font-semibold py-3 px-8 rounded-xl shadow-lg transition-all duration-300 transform hover:scale-105">
              查看总分
            </button>
            <button @click="openAddCourseModal" class="bg-purple-600 hover:bg-purple-700 text-white font-semibold py-3 px-8 rounded-xl shadow-lg transition-all duration-300 transform hover:scale-105">
              加入课程
            </button>
          </div>
        </div>
      </div>

      <!-- 课堂作业管理 -->
      <section>
        <h2 class="text-2xl font-bold text-gray-800 mb-4">课堂作业管理</h2>
        <div class="space-y-3">
          <!-- 作业列表项1 -->
          <div class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <div class="flex flex-col md:flex-row md:items-center justify-between">
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-800 mb-1">俯卧撑标准动作练习</h3>
                <p class="text-sm text-gray-600 mb-2">完成标准俯卧撑动作，要求动作规范，身体保持直线</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs text-gray-500">体能训练</span>
                  <span class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">进行中</span>
                  <span class="text-xs text-gray-500">截止时间: 2024-01-20</span>
                  <span class="text-sm font-medium text-gray-800">提交情况: 25/30</span>
                </div>
              </div>
              <router-link to="/student/assignments/1" class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium">查看</router-link>
            </div>
          </div>

          <!-- 作业列表项2 -->
          <div class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <div class="flex flex-col md:flex-row md:items-center justify-between">
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-800 mb-1">仰卧起坐耐力测试</h3>
                <p class="text-sm text-gray-600 mb-2">在规定时间内完成尽可能多的仰卧起坐，测试核心力量</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs text-gray-500">体能测试</span>
                  <span class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">进行中</span>
                  <span class="text-xs text-gray-500">截止时间: 2024-01-25</span>
                  <span class="text-sm font-medium text-gray-800">提交情况: 18/30</span>
                </div>
              </div>
              <router-link to="/student/assignments/2" class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium">查看</router-link>
            </div>
          </div>

          <!-- 作业列表项3 -->
          <div class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <div class="flex flex-col md:flex-row md:items-center justify-between">
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-800 mb-1">跳绳技巧练习</h3>
                <p class="text-sm text-gray-600 mb-2">掌握基本跳绳技巧，提高协调性和耐力</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs text-gray-500">协调训练</span>
                  <span class="text-xs bg-green-100 text-green-800 px-2 py-1 rounded-full">已完成</span>
                  <span class="text-xs text-gray-500">截止时间: 2024-01-15</span>
                  <span class="text-sm font-medium text-gray-800">提交情况: 30/30</span>
                </div>
              </div>
              <router-link to="/student/assignments/3" class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium">查看</router-link>
            </div>
          </div>
        </div>
      </section>


    </div>

    <!-- 加入课程模态框 -->
    <div v-if="showAddCourseModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-3xl p-8 w-full max-w-md shadow-2xl">
        <h2 class="text-2xl font-bold mb-6 text-gray-800">加入课程</h2>
        <div class="mb-6">
          <p class="mb-2 text-gray-700">请输入教师提供的课程码：</p>
          <input
            v-model="courseCodeInput"
            type="text"
            placeholder="例如：ABC123"
            class="w-full px-4 py-3 rounded-xl border-2 border-gray-200 focus:border-blue-500 focus:outline-none transition-all text-lg"
          />
        </div>

        <div v-if="addCourseMessage" :class="addCourseSuccess ? 'text-green-600' : 'text-red-600'" class="text-center mb-4">
          {{ addCourseMessage }}
        </div>

        <div class="flex gap-3">
          <button
            @click="handleAddCourse"
            class="flex-1 py-3 rounded-xl bg-green-500 text-white font-bold hover:bg-green-600 transition-all shadow-lg"
          >
            加入课程
          </button>
          <button
            @click="showAddCourseModal = false"
            class="flex-1 py-3 rounded-xl bg-gray-200 text-gray-800 font-bold hover:bg-gray-300 transition-all shadow-lg"
          >
            取消
          </button>
        </div>
      </div>
    </div>


  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { validateCourseCode, addStudentToClass } from '../data/mockData.js'

const router = useRouter()

// 获取当前用户信息
const user = JSON.parse(localStorage.getItem('user') || '{}')

const courseCodeInput = ref('')
const showAddCourseModal = ref(false)
const addCourseMessage = ref('')
const addCourseSuccess = ref(false)

// 加入课程功能
const openAddCourseModal = () => {
  showAddCourseModal.value = true
  courseCodeInput.value = ''
  addCourseMessage.value = ''
  addCourseSuccess.value = false
}

const handleAddCourse = () => {
  if (!courseCodeInput.value.trim()) {
    addCourseMessage.value = '请输入课程码'
    addCourseSuccess.value = false
    return
  }

  const validationResult = validateCourseCode(courseCodeInput.value.trim())
  if (!validationResult.valid) {
    addCourseMessage.value = validationResult.message
    addCourseSuccess.value = false
    return
  }

  // 获取当前学生ID（模拟）
  const currentUser = JSON.parse(localStorage.getItem('user'))
  const studentId = currentUser ? currentUser.id : 'student1'

  // 添加学生到课程
  addStudentToClass(studentId, validationResult.courseCode.className)

  addCourseMessage.value = `成功加入课程：${validationResult.courseCode.className}`
  addCourseSuccess.value = true

  // 关闭模态框
  setTimeout(() => {
    showAddCourseModal.value = false
  }, 1500)
};

</script>
