<template>
  <div class="min-h-screen bg-white">
    <div class="max-w-6xl mx-auto p-6 space-y-8">


      <!-- 系统概览 -->
      <section class="bg-gradient-to-r from-blue-50 to-purple-50 rounded-2xl p-6 shadow-lg">
        <h2 class="text-xl font-bold text-gray-800 mb-4">智能化体育学习平台，让每一次锻炼都有意义</h2>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div class="text-center">
            <div class="text-3xl font-bold text-blue-600 mb-1">3</div>
            <div class="text-sm text-gray-600">总作业数</div>
          </div>
          <div class="text-center">
            <div class="text-3xl font-bold text-green-600 mb-1">30</div>
            <div class="text-sm text-gray-600">班级学生</div>
          </div>
          <div class="text-center">
            <div class="text-3xl font-bold text-orange-600 mb-1">7</div>
            <div class="text-sm text-gray-600">待评分作业</div>
          </div>
          <div class="text-center">
            <div class="text-3xl font-bold text-purple-600 mb-1">3</div>
            <div class="text-sm text-gray-600">教学视频</div>
          </div>
        </div>
      </section>

      <!-- 课堂作业管理 -->
      <section>
        <h2 class="text-2xl font-bold text-gray-800 mb-4">课堂作业管理</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <!-- 作业卡片1 -->
          <div class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <h3 class="text-lg font-semibold text-gray-800 mb-2">俯卧撑标准动作练习</h3>
            <p class="text-sm text-gray-600 mb-3">完成标准俯卧撑动作，要求动作规范，身体保持直线</p>
            <div class="flex items-center justify-between mb-3">
              <span class="text-xs text-gray-500">体能训练</span>
              <span class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">进行中</span>
            </div>
            <div class="flex justify-between items-center">
              <div>
                <p class="text-xs text-gray-500">截止时间: 2024-01-20</p>
                <p class="text-sm font-medium text-gray-800">提交情况: 25/30</p>
              </div>
              <button class="text-blue-500 hover:text-blue-700 text-sm font-medium">查看</button>
            </div>
          </div>

          <!-- 作业卡片2 -->
          <div class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <h3 class="text-lg font-semibold text-gray-800 mb-2">仰卧起坐耐力测试</h3>
            <p class="text-sm text-gray-600 mb-3">在规定时间内完成尽可能多的仰卧起坐，测试核心力量</p>
            <div class="flex items-center justify-between mb-3">
              <span class="text-xs text-gray-500">体能测试</span>
              <span class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">进行中</span>
            </div>
            <div class="flex justify-between items-center">
              <div>
                <p class="text-xs text-gray-500">截止时间: 2024-01-25</p>
                <p class="text-sm font-medium text-gray-800">提交情况: 18/30</p>
              </div>
              <button class="text-blue-500 hover:text-blue-700 text-sm font-medium">查看</button>
            </div>
          </div>

          <!-- 作业卡片3 -->
          <div class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <h3 class="text-lg font-semibold text-gray-800 mb-2">跳绳技巧练习</h3>
            <p class="text-sm text-gray-600 mb-3">掌握基本跳绳技巧，提高协调性和耐力</p>
            <div class="flex items-center justify-between mb-3">
              <span class="text-xs text-gray-500">协调训练</span>
              <span class="text-xs bg-green-100 text-green-800 px-2 py-1 rounded-full">已完成</span>
            </div>
            <div class="flex justify-between items-center">
              <div>
                <p class="text-xs text-gray-500">截止时间: 2024-01-15</p>
                <p class="text-sm font-medium text-gray-800">提交情况: 30/30</p>
              </div>
              <button class="text-blue-500 hover:text-blue-700 text-sm font-medium">查看</button>
            </div>
          </div>
        </div>
      </section>

      <!-- 作业列表 -->
      <section>
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-2xl font-bold text-gray-800">📝 作业列表</h2>
          <div class="flex gap-3">
            <button class="text-blue-500 hover:text-blue-700 text-sm font-medium">查看全部</button>
            <button @click="openAddCourseModal" class="px-4 py-2 rounded-xl bg-green-500 text-white hover:bg-green-600 transition-all shadow">
              加入课程
            </button>
          </div>
        </div>

        <!-- 作业列表 -->
        <div class="space-y-3">
          <div v-for="assignment in filteredAssignments" :key="assignment.id"
               class="p-4 rounded-lg shadow bg-white border border-gray-100 hover:shadow-md transition-all">
            <div class="flex justify-between items-start mb-2">
              <div>
                <h3 class="text-lg font-semibold text-gray-800">{{ assignment.title }}</h3>
                <p class="text-xs text-gray-500 mt-1">{{ assignment.subject }} • 截止日期: {{ assignment.deadline }}</p>
              </div>
              <span :class="['px-2 py-1 rounded-full text-xs font-medium',
                             assignment.status === 'pending' ? 'bg-red-100 text-red-700' :
                             assignment.status === 'submitted' ? 'bg-yellow-100 text-yellow-700' :
                             'bg-green-100 text-green-700']">
                {{ assignment.status === 'pending' ? '未提交' :
                   assignment.status === 'submitted' ? '已提交' : '已批改' }}
              </span>
            </div>

            <div class="flex justify-between items-center">
              <p class="text-sm text-gray-600">{{ assignment.description }}</p>
              <button v-if="assignment.status === 'pending'"
                      class="text-sm text-blue-500 hover:text-blue-700 font-medium">
                去提交
              </button>
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
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { assignments, submissions, validateCourseCode, addStudentToClass } from '../data/mockData.js'

const router = useRouter()

  // 搜索和过滤
  const searchQuery = ref('')
  const statusFilter = ref('all')

  // 使用从mockData导入的作业数据
  const mockAssignments = assignments
const courseCodeInput = ref('')
const showAddCourseModal = ref(false)
const addCourseMessage = ref('')
const addCourseSuccess = ref(false)

const filteredAssignments = computed(() => {
    return mockAssignments.filter(assignment => {
      const matchesSearch = assignment.title.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                        assignment.description.toLowerCase().includes(searchQuery.value.toLowerCase())
      const matchesStatus = statusFilter.value === 'all' || assignment.status === statusFilter.value
      return matchesSearch && matchesStatus
    })
  })

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
}


</script>
