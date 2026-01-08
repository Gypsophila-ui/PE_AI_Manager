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
          <div class="flex gap-3">
            <button @click="openHealthReportDialog" class="bg-gradient-to-r from-green-500 to-green-600 hover:from-green-600 hover:to-green-700 text-white font-semibold py-2 px-6 rounded-xl shadow-lg transition-all duration-300 transform hover:scale-105">
              导出报告
            </button>
            <button @click="openAddCourseModal" class="bg-purple-600 hover:bg-purple-700 text-white font-semibold py-2 px-6 rounded-xl shadow-lg transition-all duration-300 transform hover:scale-105">
              加入课程
            </button>
          </div>
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
                :to="`/student/course/${course.id}`"
                class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium"
              >
                查看
              </router-link>
            </div>
          </div>
        </div>
      </section>

    </div>

    <!-- 个性化健康报告对话框 -->
    <div
      v-if="showHealthReportDialog"
      class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
      @click="closeHealthReportDialog"
    >
      <div
        class="bg-white rounded-3xl shadow-2xl max-w-4xl w-full max-h-[95vh] overflow-hidden"
        @click.stop
      >
        <div class="p-6 border-b border-gray-200">
          <div class="flex justify-between items-center">
            <h3 class="text-2xl font-bold text-gray-800">生成个性化健康报告</h3>
            <button @click="closeHealthReportDialog" class="text-gray-500 hover:text-gray-700">
              <span class="text-3xl">×</span>
            </button>
          </div>
        </div>

        <div class="p-6 overflow-y-auto max-h-[75vh]">
          <div class="mb-6 p-4 bg-gray-50 rounded-xl">
            <h4 class="font-medium text-gray-700 mb-4">学生信息（可选）</h4>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-600 mb-2">身高（cm）</label>
                <input
                  v-model="studentHeight"
                  type="number"
                  placeholder="例如：175"
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500"
                >
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-600 mb-2">体重（kg）</label>
                <input
                  v-model="studentWeight"
                  type="number"
                  placeholder="例如：65"
                  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500"
                >
              </div>
            </div>
          </div>

          <div class="mb-6 p-4 bg-gray-50 rounded-xl">
            <h4 class="font-medium text-gray-700 mb-4">您的健康问题</h4>
            <textarea
              v-model="healthReportQuery"
              placeholder="请输入您想要咨询的健康问题，例如：根据我的情况给出长期训练建议"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500 resize-none"
              rows="3"
            ></textarea>
          </div>

          <button
            @click="generateHealthReport"
            :disabled="healthReportLoading || !healthReportQuery.trim()"
            class="mt-4 w-full px-6 py-3 bg-gradient-to-r from-green-500 to-green-600 text-white rounded-xl font-medium hover:shadow-lg transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ healthReportLoading ? '生成中...' : '生成个性化健康报告' }}
          </button>
        </div>

        <div v-if="healthReportLoading" class="flex justify-center items-center py-8">
          <div class="animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-green-500"></div>
        </div>

        <div v-else-if="healthReportError" class="bg-red-50 border border-red-200 rounded-xl p-6">
          <h4 class="text-lg font-bold text-red-800 mb-2">生成失败</h4>
          <p class="text-red-700">{{ healthReportError }}</p>
        </div>

        <div v-else-if="healthReportContent" class="prose prose-sm max-w-none bg-white p-6">
          <div v-html="renderMarkdown(healthReportContent)"></div>
        </div>
      </div>
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
import { marked } from 'marked'

const router = useRouter()

// 获取当前用户信息
const user = JSON.parse(localStorage.getItem('user') || '{}')

// 个性化健康报告相关
const showHealthReportDialog = ref(false)
const studentHeight = ref('')
const studentWeight = ref('')
const healthReportQuery = ref('')
const healthReportContent = ref('')
const healthReportLoading = ref(false)
const healthReportError = ref('')

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

    if (response.data.success) {
      if (!response.data.data || response.data.data.trim() === '' || response.data.data === 'NULL') {
        courses.value = []
        console.log('暂无课程')
      } else {
        // 解析课程ID列表（用\t\r分隔）
        const courseIdList = response.data.data.split('\t\r').filter(id => id.trim())

        // 为每个课程ID获取课程详情
        const courseDetailsPromises = courseIdList.map(async (courseId) => {
          try {
            const detailResponse = await apiClient.post('/Course/get_info_by_course_id', {
              first: courseId.trim(),
              second: token
            })
            console.log('获取课程详情响应:', detailResponse.data)
            if (detailResponse.data.success && detailResponse.data.data) {
              // 解析课程数据字符串，格式为: 教师id\t\r课程名字\t\r课程描述\t\r课程码\t\r课程所在学期\t\r课程是否正在进行(1是0否)\t\r课程创建时间
              const courseDataArray = detailResponse.data.data.split('\t\r');

              console.log('课程数据:', courseDataArray)

              // 安全地解析课程数据，处理字段可能为空的情况
              const teacherId = courseDataArray[0] || '';
              const courseName = courseDataArray[1] || '未命名课程';
              const courseDescription = courseDataArray[2] || '暂无描述';
              const courseCode = courseDataArray[3] || '';
              const courseTerm = courseDataArray[4] || '';
              const isActive = courseDataArray[5] || '0';
              const createTime = courseDataArray[6] || '';

              console.log('解析后的课程数据:', {
                teacherId,
                courseName,
                courseDescription,
                courseCode,
                courseTerm,
                isActive,
                createTime
              })

              // 获取该课程的作业列表
              const homeworkResponse = await apiClient.post('/Homework/get_homework_id_by_course', {
                first: '0', // 学生
                second:  studentId,
                third: token,
                fourth:courseId.trim()
              })

              let assignments = []
              if (homeworkResponse.data.success && homeworkResponse.data.data) {
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
                name: courseName,
                description: courseDescription,
                code: courseCode,
                subject: courseCode || '未知课号',
                status: isActive === '1' ? '进行中' : '已结束',
                term: courseTerm,
                createTime: createTime,
                teacherId: teacherId,
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

    // Check if the error contains NULL or 暂无课程 in the response message or error message
    const errorMessage = err.response?.data?.message || err.message || '';
    if (errorMessage.includes('NULL') || errorMessage.includes('暂无课程')) {
      coursesError.value = false
      coursesErrorMessage.value = ''
      courses.value = []
    } else {
      coursesError.value = true
      coursesErrorMessage.value = err.response?.data?.message || err.message || '获取课程列表失败，请稍后重试'
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

const handleAddCourse = async () => {
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
  try {
    const token = currentUser.token
    console.log('加入课程请求:', {
      first: studentId,
      second: token,
      third: validationResult.courseCode.id
    })

    const response = await apiClient.post('/Course_student/add_course', {
      first: studentId,
      second: token,
      third: validationResult.courseCode.id
    })

    console.log('加入课程响应:', response.data)
    if (!response.data.success) {
      throw new Error(response.data.message || '加入课程失败')
    }
  } catch (err) {
    addCourseMessage.value = err.message || '加入课程失败，请稍后重试'
    addCourseSuccess.value = false
    return
  }

  addCourseMessage.value = `成功加入课程：${validationResult.courseCode.className}`
  addCourseSuccess.value = true

  // 关闭模态框
  setTimeout(() => {
    showAddCourseModal.value = false
    // 刷新课程列表
    fetchCourseList()
  }, 1500)
};

// 验证课程码格式：6位只包含数字
const validateCourseCode = (code) => {
  // 检查是否为6位字符
  if (code.length !== 6) {
    return {
      valid: false,
      message: '课程码必须为6位字符'
    }
  }

  // 检查是否只包含字母和数字
  const alphanumericRegex = /^[0-9]+$/
  if (!alphanumericRegex.test(code)) {
    return {
      valid: false,
      message: '课程码只能包含数字'
    }
  }

  // 验证通过，返回课程码信息
  return {
    valid: true,
    courseCode: {
      id: code,
      className: `课程 ${code}` // 临时显示名称，实际名称会在获取课程详情时更新
    }
  }
}

// 组件挂载时获取课程列表
onMounted(() => {
  fetchCourseList()
})

// Markdown渲染函数
const renderMarkdown = (content) => {
  return marked(content)
}

// 获取当前用户ID
const getUserId = () => {
  const user = JSON.parse(localStorage.getItem('user') || '{}')
  return user.id || 'default_user'
}

// 打开健康报告对话框
const openHealthReportDialog = () => {
  showHealthReportDialog.value = true
  healthReportContent.value = ''
  healthReportError.value = ''
  healthReportQuery.value = ''
}

// 关闭健康报告对话框
const closeHealthReportDialog = () => {
  showHealthReportDialog.value = false
}

// 生成健康报告
const generateHealthReport = async () => {
  healthReportLoading.value = true
  healthReportError.value = ''
  healthReportContent.value = ''

  try {
    const baseUrl = import.meta.env.VITE_API_BASE_URL || '/chat'

    const requestData = {
      student_id: getUserId(),
      analysis_type: 'personalized_tips',
      query: healthReportQuery.value
    }

    if (studentHeight.value || studentWeight.value) {
      requestData.student_info = {}
      if (studentHeight.value) {
        requestData.student_info.height = studentHeight.value
      }
      if (studentWeight.value) {
        requestData.student_info.weight = studentWeight.value
      }
    }

    const url = `${baseUrl}/api/analysis/generate`

    console.log('请求URL:', url)
    console.log('请求体:', JSON.stringify(requestData, null, 2))

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(requestData)
    })

    console.log('响应状态:', response.status)
    console.log('响应头:', Object.fromEntries(response.headers.entries()))

    if (!response.ok) {
      const errorText = await response.text()
      console.error('响应错误文本:', errorText)
      throw new Error(`HTTP ${response.status}: ${errorText}`)
    }

    const responseText = await response.text()
    console.log('响应文本:', responseText)

    let result
    try {
      result = JSON.parse(responseText)
      console.log('解析后的JSON:', result)
    } catch (parseError) {
      console.error('JSON解析错误:', parseError)
      console.error('无法解析的响应文本:', responseText)
      throw new Error(`JSON解析错误: ${parseError.message}`)
    }

    if (result.success && result.data) {
      healthReportContent.value = result.data
    } else {
      healthReportError.value = result.message || '生成失败，请重试'
    }
  } catch (err) {
    console.error('生成健康报告失败:', err)
    healthReportError.value = `生成失败: ${err.message}`
  } finally {
    healthReportLoading.value = false
  }
}
</script>
