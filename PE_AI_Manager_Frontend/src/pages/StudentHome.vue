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
        </div>
      </div>

      <!-- 我的课程 -->
      <section>
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-2xl font-bold text-gray-800">我的课程</h2>
          <button @click="openAddCourseModal" class="bg-purple-600 hover:bg-purple-700 text-white font-semibold py-2 px-6 rounded-xl shadow-lg transition-all duration-300 transform hover:scale-105">
            加入课程
          </button>
        </div>

        <!-- 加载状态 -->
        <div v-if="loadingCourses" class="flex flex-col items-center justify-center py-12">
          <div class="w-12 h-12 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mb-4"></div>
          <p class="text-gray-600">正在加载课程列表...</p>
        </div>

        <!-- 错误状态 -->
        <div v-else-if="coursesError" class="bg-red-50 border border-red-200 rounded-xl p-6 text-center">
          <p class="text-red-600 mb-4">{{ coursesErrorMessage }}</p>
          <button @click="fetchCourseList" class="px-6 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors">
            重试
          </button>
        </div>

        <!-- 空状态 -->
        <div v-else-if="courses.length === 0" class="bg-gray-50 border border-gray-200 rounded-xl p-12 text-center">
          <p class="text-gray-600 text-lg mb-2">暂无课程</p>
          <p class="text-gray-500 text-sm">点击右侧"加入课程"按钮开始学习</p>
        </div>

        <!-- 课程列表 -->
        <div v-else class="space-y-3">
          <div
            v-for="course in courses"
            :key="course.id"
            class="bg-white rounded-xl shadow-md border border-gray-100 p-4"
          >
            <div class="flex flex-col md:flex-row md:items-center justify-between">
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-800 mb-1">{{ course.name }}</h3>
                <p class="text-sm text-gray-600 mb-2">{{ course.description }}</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs text-gray-500">{{ course.subject }}</span>
                  <span
                    :class="[
                      'text-xs px-2 py-1 rounded-full',
                      course.status === '进行中' ? 'bg-blue-100 text-blue-800' : 'bg-green-100 text-green-800'
                    ]"
                  >
                    {{ course.status }}
                  </span>
                  <span class="text-xs text-gray-500">作业数量: {{ course.completedAssignments }}/{{ course.totalAssignments }}</span>
                  <span class="text-sm font-medium text-gray-800">完成率: {{ course.completionRate }}%</span>
                </div>
              </div>
              <router-link
                :to="`/course/${course.id}`"
                class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium"
              >
                查看
              </router-link>
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
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { apiClient } from '../services/axios'

const router = useRouter()

// 获取当前用户信息
const user = JSON.parse(localStorage.getItem('user') || '{}')

const courseCodeInput = ref('')
const showAddCourseModal = ref(false)
const addCourseMessage = ref('')
const addCourseSuccess = ref(false)

// 课程列表相关
const courses = ref([])
const loadingCourses = ref(false)
const coursesError = ref(false)
const coursesErrorMessage = ref('')

// 获取课程列表
const fetchCourseList = async () => {
  loadingCourses.value = true
  coursesError.value = false
  coursesErrorMessage.value = ''

  try {
    // 获取当前学生ID
    const currentUser = JSON.parse(localStorage.getItem('user'))
    const studentId = currentUser ? currentUser.id : 'student1'

    // 获取JWT token
    const user = JSON.parse(localStorage.getItem('user'))
    const token = user.token
    console.log('user:', user)
    if (!token || token.trim() === '') {
      throw new Error('未找到认证token，请重新登录')
    }

    // 调用get_course_id_by_student接口获取课程ID列表
    const response = await apiClient.post('/Course_student/get_course_id_by_student', {
      first: studentId,
      second: token.trim()
    })

    if (response.data.code === 0) {
      if (!response.data.data || response.data.data.trim() === '' || response.data.data === 'NULL') {
        courses.value = []
        console.log('暂无课程')
      } else {
        // 解析课程ID列表（用\t\r分隔）
        const courseIdList = response.data.data.split('\t\r').filter(id => id.trim())

        // 为每个课程ID获取课程详情
        const courseDetailsPromises = courseIdList.map(async (courseId) => {
          try {
            const detailResponse = await apiClient.post('/Course_student/get_info_by_course_id', {
              first: courseId.trim(),
              second: token
            })

            if (detailResponse.data.code === 0 && detailResponse.data.data) {
              const courseData = detailResponse.data.data

              // 获取该课程的作业列表
              const homeworkResponse = await apiClient.post('/get_homework_id_by_course', {
                first: courseId.trim(),
                second: '0', // 学生
                third: token
              })

              let assignments = []
              if (homeworkResponse.data.code === 0 && homeworkResponse.data.data) {
                const homeworkIdList = homeworkResponse.data.data.split('\t\r').filter(id => id.trim())
                assignments = homeworkIdList.map(homeworkId => ({
                  id: homeworkId.trim(),
                  title: `作业 ${homeworkId.trim()}`,
                  description: '作业描述',
                  deadline: '待定',
                  status: '进行中'
                }))
              }

              return {
                id: courseId.trim(),
                name: courseData.name || '未命名课程',
                description: courseData.info || '暂无描述',
                subject: '体育',
                status: courseData.is_active === '1' ? '进行中' : '未发布',
                assignments: assignments,
                totalAssignments: assignments.length,
                completedAssignments: assignments.filter(a => a.status === '已完成').length,
                completionRate: assignments.length > 0
                  ? Math.round((assignments.filter(a => a.status === '已完成').length / assignments.length) * 100)
                  : 0
              }
            }
          } catch (error) {
            console.error(`获取课程 ${courseId} 详情失败:`, error)
            return null
          }
        })

        // 等待所有课程详情获取完成
        const courseDetails = await Promise.all(courseDetailsPromises)
        courses.value = courseDetails.filter(course => course !== null)

        console.log('课程列表加载成功:', courses.value)
      }
    } else {
      throw new Error(response.data.message || '获取课程列表失败')
    }
  } catch (err) {
    console.error('获取课程列表失败:', err)

    if (err.message && (err.message.includes('NULL') || err.message.includes('暂无课程'))) {
      coursesError.value = false
      coursesErrorMessage.value = ''
      courses.value = []
    } else {
      coursesError.value = true
      coursesErrorMessage.value = err.message || '获取课程列表失败，请稍后重试'
      courses.value = []
    }
  } finally {
    loadingCourses.value = false
  }
}

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
    // 刷新课程列表
    fetchCourseList()
  }, 1500)
};

// 组件挂载时获取课程列表
onMounted(() => {
  fetchCourseList()
})
</script>
