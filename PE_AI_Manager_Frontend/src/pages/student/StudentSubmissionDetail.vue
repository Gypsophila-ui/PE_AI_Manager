<template>
  <div class="min-h-screen bg-white">
    <div class="max-w-7xl mx-auto p-6 space-y-8">
      <!-- 顶部导航栏 -->
      <div class="flex justify-between items-center py-4">
        <h1 class="text-2xl font-bold text-gray-800">体育作业平台 - 学生端</h1>
        <div class="flex gap-4">
          <button @click="goToHome" class="px-4 py-2 rounded-full bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-md">
            🏠 首页
          </button>
          <button @click="goBack" class="px-4 py-2 rounded-full bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
            ← 返回列表
          </button>
          <button @click="logout" class="px-4 py-2 rounded-full bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
            退出登录
          </button>
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="text-center py-32">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-blue-500"></div>
        <p class="mt-6 text-xl text-gray-600">加载提交详情中...</p>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="errorMsg" class="text-center py-32">
        <p class="text-2xl text-red-600 mb-6">{{ errorMsg }}</p>
        <button @click="reloadPage" class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
          🔄 重试加载
        </button>
      </div>

      <!-- 作业基本信息卡片 -->
      <div class="bg-gradient-to-r from-blue-50 to-purple-50 rounded-2xl shadow-lg p-8">
        <div class="flex justify-between items-start mb-6">
          <div>
            <h2 class="text-3xl font-bold text-gray-800 mb-3">{{ submission.title || '加载中...' }}</h2>
            <div class="flex items-center gap-6 text-lg text-gray-700">
              <span>{{ getCourseName(submission.courseId) }}</span>
              <span>•</span>
              <span>提交时间：{{ formatFullDate(submission.CREATE_TIME) }}</span>
            </div>
          </div>
          <div class="text-right">
            <div class="text-5xl font-bold mb-2" :class="submission.score !== null ? 'text-green-600' : 'text-orange-500'">
              {{ submission.score !== null ? submission.score + ' 分' : '待批改' }}
            </div>
            <span class="px-4 py-2 rounded-full text-lg font-medium"
                  :class="submission.score !== null ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'">
              {{ submission.score !== null ? '已批改' : '待批改' }}
            </span>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-8">
          <div>
            <p class="text-sm text-gray-500 mb-1">作业截止时间</p>
            <p class="text-xl font-semibold text-gray-800">{{ formatFullDate(submission.deadline) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 mb-1">作业描述</p>
            <p class="text-lg text-gray-700">{{ submission.description || '暂无描述' }}</p>
          </div>
        </div>
      </div>

      <!-- AI 分析视频 -->
      <div class="bg-white rounded-2xl shadow-lg border border-gray-100 p-8">
        <h3 class="text-2xl font-bold text-gray-800 mb-6">🎥 AI 分析视频</h3>
        <div v-if="submission.video_url" class="relative aspect-video bg-black rounded-xl overflow-hidden shadow-xl">
          <video :src="submission.video_url" controls class="w-full h-full object-contain">
            您的浏览器不支持视频播放。
          </video>
        </div>
        <div v-else class="aspect-video bg-gray-100 rounded-xl flex items-center justify-center border-2 border-dashed border-gray-300">
          <p class="text-gray-500 text-lg">暂无AI分析视频（可能正在处理中）</p>
        </div>
      </div>

      <!-- AI 反馈 -->
      <div class="bg-white rounded-2xl shadow-lg border border-gray-100 p-8">
        <h3 class="text-2xl font-bold text-gray-800 mb-6">🤖 AI 智能评价</h3>
        <div v-if="submission.AI_feedback" class="bg-indigo-50 rounded-xl p-6 border border-indigo-200">
          <p class="text-lg text-indigo-900 leading-relaxed whitespace-pre-wrap">{{ submission.AI_feedback }}</p>
        </div>
        <div v-else class="text-center py-8 text-gray-500">
          AI 反馈暂未生成（可能待批改）
        </div>
      </div>

      <!-- 教师反馈 -->
      <div class="bg-white rounded-2xl shadow-lg border border-gray-100 p-8">
        <h3 class="text-2xl font-bold text-gray-800 mb-6">👩‍🏫 教师评语</h3>
        <div v-if="submission.teacher_feedback" class="bg-blue-50 rounded-xl p-6 border border-blue-200">
          <p class="text-lg text-blue-900 leading-relaxed whitespace-pre-wrap">{{ submission.teacher_feedback }}</p>
        </div>
        <div v-else class="text-center py-8 text-gray-500">
          教师尚未留下评语
        </div>
      </div>

      <!-- 操作按钮 -->
      <div class="flex justify-center gap-6 mt-12">
        <div v-if="isDeadlinePassed">
          <p class="text-xl font-semibold text-red-600 bg-red-50 px-8 py-4 rounded-xl shadow">
            作业已截止
          </p>
        </div>
        <button v-else
            @click="reSubmit"
            class="px-8 py-4 rounded-xl bg-orange-500 text-white text-xl font-bold hover:bg-orange-600 transition-all shadow-lg flex items-center gap-3">
          <span>🔄</span> 重新提交此作业
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '../../services/axios.js'

const route = useRoute()
const router = useRouter()

const submission = ref({})   // 提交记录详情
const homework = ref({})     // 作业信息
const course = ref({})       // 课程信息（新增）

const loading = ref(true)
const errorMsg = ref('')

// 从路由获取参数
const submitId = route.params.submitId
const homeworkId = route.params.assignmentId
const courseId = route.params.courseId

// 当前登录学生信息
const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const studentId = currentUser.id || ''
const jwt = currentUser.jwt || ''

onMounted(async () => {
  if (!submitId || !homeworkId || !courseId || !studentId || !jwt) {
    errorMsg.value = '缺少必要参数或未登录'
    loading.value = false
    return
  }

  loading.value = true
  errorMsg.value = ''

  try {
    // 并行发起三个请求，提高速度
    const [submitResp, homeworkResp, courseResp] = await Promise.all([
      apiClient.post('/api/get_submit_info', {
        user_type: '0',       // 学生身份
        user_id: studentId,
        jwt: jwt,
        submit_id: submitId
      }),
      apiClient.post('/api/get_info_by_homework_id', {
        course_id: courseId,
        homework_id: homeworkId
      }),
      apiClient.post('/api/get_info_by_course_id', {
        course_id: courseId
      })
    ])

    // 处理 get_submit_info
    if (submitResp.data[0] < 0) {
      handleSubmitError(submitResp.data[0])
      return
    }
    submission.value = {
      video_url: submitResp.data[0],
      score: submitResp.data[1],
      AI_feedback: submitResp.data[2] || '',
      teacher_feedback: submitResp.data[3] || '',
      CREATE_TIME: submitResp.data[4],
    }

    // 处理 get_info_by_homework_id
    if (homeworkResp.data[0] < 0) {
      errorMsg.value = getHomeworkErrorMsg(homeworkResp.data[0])
      return
    }
    homework.value = {
      title: homeworkResp.data[0],
      description: homeworkResp.data[1],
      deadline: homeworkResp.data[2],
      create_time: homeworkResp.data[3],
    }

    // 处理 get_info_by_course_id（新增）
    if (courseResp.data[0] < 0) {
      errorMsg.value = getCourseErrorMsg(courseResp.data[0])
      return
    }
    course.value = {
      teacher_id: courseResp.data[0],
      name: courseResp.data[1],
      info: courseResp.data[2],
      code: courseResp.data[3],
      semester: courseResp.data[4],
      is_active: courseResp.data[5],
      created_time: courseResp.data[6],
    }

    // 合并数据供模板使用
    Object.assign(submission.value, {
      title: homework.value.title,
      description: homework.value.description,
      deadline: homework.value.deadline,
      courseId: courseId,
      courseName: course.value.name  // 直接用真实课程名
    })

  } catch (err) {
    errorMsg.value = '网络请求失败，请检查网络连接'
    console.error(err)
  } finally {
    loading.value = false
  }
})

// 错误处理函数
const handleSubmitError = (code) => {
  if ([ -21, -22, -23, -24 ].includes(code)) {
    errorMsg.value = '登录已失效，请重新登录'
    logout()
  } else if (code === -25) {
    errorMsg.value = '提交记录不存在'
  } else {
    errorMsg.value = '加载提交信息失败'
  }
}

const getHomeworkErrorMsg = (code) => {
  if (code === -21) return '作业不存在'
  return '加载作业信息失败'
}

const getCourseErrorMsg = (code) => {
  if (code === -21) return '课程不存在'
  return '加载课程信息失败'
}

// 是否已截止
const isDeadlinePassed = computed(() => {
  if (!submission.value.deadline) return false
  const deadlineDate = new Date(submission.value.deadline)
  const now = new Date()
  return now > deadlineDate
})

// 重新提交
const reSubmit = () => {
  router.push(`/student/course/${courseId}/submit/${homeworkId}`)
}

const goBack = () => router.push('/student/assignments')
const goToHome = () => router.push('/student')

const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

const getCourseName = () => submission.value.courseName || '加载中...'

const formatFullDate = (dateStr) => {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const reloadPage = () => window.location.reload()
</script>
