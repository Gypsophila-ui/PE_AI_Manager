<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-8">
      <!-- 顶部导航 -->
      <div class="flex justify-between items-center py-4">
        <div class="flex items-center gap-2">
          <button @click="goBack" class="text-2xl text-gray-600 hover:text-gray-800 transition-colors">
            ←
          </button>
          <h1 class="text-2xl font-bold text-gray-800">体育作业平台</h1>
        </div>
        <div class="flex gap-4">
          <button @click="goToAssistant" class="px-4 py-2 rounded-xl bg-purple-500 text-white hover:bg-purple-600 transition-all shadow-lg">
            💬 AI助手
          </button>
          <button @click="logout" class="px-4 py-2 rounded-xl bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
            退出登录
          </button>
        </div>
      </div>

      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-2">👥 学生管理</h2>
        <p class="text-lg text-gray-600">课程课号：{{ courseId }}</p>
      </section>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
        <p class="ml-4 text-xl text-gray-600">加载学生名单与信息中...</p>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-3xl p-8 text-center">
        <div class="text-6xl mb-4">❌</div>
        <h3 class="text-2xl font-bold text-red-800 mb-4">加载失败</h3>
        <p class="text-red-700 mb-6">{{ errorMessage }}</p>
        <button @click="fetchStudents" class="px-8 py-3 rounded-xl bg-red-500 text-white hover:bg-red-600 shadow-lg">
          重试
        </button>
      </div>

      <!-- 学生列表 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-8">
        <div class="flex justify-between items-center mb-6">
          <h3 class="text-2xl font-bold text-gray-800">
            已加入学生（{{ students.length }} 人）
          </h3>
          <button
            v-if="students.length > 0"
            @click="copyAllStudentIds"
            class="px-5 py-2 rounded-xl bg-gray-500 text-white hover:bg-gray-600 transition-all shadow"
          >
            复制所有学号
          </button>
        </div>

        <!-- 无学生 -->
        <div v-if="students.length === 0" class="text-center py-12">
          <div class="text-6xl text-gray-300 mb-6">👨‍🎓👩‍🎓</div>
          <h4 class="text-xl font-bold text-gray-700 mb-3">暂无学生</h4>
          <p class="text-gray-500">
            学生可通过课程邀请码加入<br />
            当前课程尚未有学生加入
          </p>
        </div>

        <!-- 学生列表 -->
        <div v-else class="space-y-4">
          <div
            v-for="student in students"
            :key="student.id"
            class="bg-gray-50 rounded-xl p-5 hover:bg-gray-100 transition-all shadow-sm"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-5">
                <!-- 头像缩写（姓名首字） -->
                <div class="w-14 h-14 bg-blue-100 rounded-full flex items-center justify-center text-blue-700 font-bold text-xl">
                  {{ student.name ? student.name[0] : student.id.slice(0, 2).toUpperCase() }}
                </div>

                <div class="space-y-1">
                  <!-- 学号 + 姓名 -->
                  <div class="flex items-center gap-3">
                    <p class="text-lg font-mono font-semibold text-gray-800">{{ student.id }}</p>
                    <p class="text-lg font-semibold text-gray-900">
                      {{ student.name || '（姓名加载中...）' }}
                    </p>
                  </div>

                  <!-- 详细信息 -->
                  <div class="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                    <span>性别：{{ student.gender || '未知' }}</span>
                    <span v-if="student.college">学院：{{ student.college }}</span>
                    <span v-if="student.department">系：{{ student.department }}</span>
                    <span v-if="student.major">专业：{{ student.major }}</span>
                    <span v-else class="text-gray-400">个人信息加载失败</span>
                  </div>
                </div>
              </div>

              <!-- 踢出按钮 -->
              <button
                @click="removeStudent(student.id)"
                class="px-6 py-3 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow font-medium"
              >
                踢出课程
              </button>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '../../services/axios.js'

const router = useRouter()
const route = useRoute()

const courseId = route.params.courseId

const students = ref([])
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.jwt || ''

const fetchStudents = async () => {
  loading.value = true
  error.value = false
  students.value = []

  try {
    // 1. 获取学生ID列表
    const idResp = await apiClient.post('/api/get_student_id_by_course', {
      First: teacherId,
      Second: jwt,
      Third: courseId
    })

    if (idResp.data[0] < 0 || !idResp.data[1]) {
      loading.value = false
      return
    }

    const studentIdStr = idResp.data[1].trim()
    if (studentIdStr === '') {
      loading.value = false
      return
    }

    const studentIds = studentIdStr.split('\t\r').filter(Boolean)

    // 2. 并行获取每个学生的个人信息
    const studentPromises = studentIds.map(async (id) => {
      try {
        const infoResp = await apiClient.post('/api/get_student_info', {
          First: teacherId,      // 查询者ID
          Second: jwt,
          Third: '1',            // user_type: 1=教师
          Fourth: id             // student_id
        })

        if (infoResp.data[0] < 0 || infoResp.data.length < 5) {
          return { id, name: null, gender: null, major: null, college: null, department: null }
        }

        const d = infoResp.data
        return {
          id,
          name: d[1] || '未知',
          gender: d[2] || '未知',
          major: d[3] || null,
          college: d[4] || null,
          department: d[5] || null
        }
      } catch (err) {
        console.error(`获取学生 ${id} 信息失败`, err)
        return { id, name: null, gender: null, major: null, college: null, department: null }
      }
    })

    students.value = await Promise.all(studentPromises)

  } catch (err) {
    console.error(err)
    error.value = true
    errorMessage.value = '无法加载学生名单，请检查网络或课程权限'
  } finally {
    loading.value = false
  }
}

const removeStudent = async (studentId) => {
  if (!confirm(`确定要将学号 ${studentId} 的学生踢出本课程吗？此操作不可恢复。`)) {
    return
  }

  try {
    const resp = await apiClient.post('/api/exit_course_by_teacher', {
      First: teacherId,
      Second: jwt,
      Third: courseId,
      Fourth: studentId
    })

    if (resp.data[0] >= 0 || resp.data[0] === '1') {
      alert('学生已成功移除')
      students.value = students.value.filter(s => s.id !== studentId)
    } else {
      alert('移除失败：' + (resp.data[1] || '未知错误'))
    }
  } catch (err) {
    console.error(err)
    alert('操作失败，请重试')
  }
}

const copyAllStudentIds = async () => {
  const text = students.value.map(s => s.id).join('\n')
  try {
    await navigator.clipboard.writeText(text)
    alert('所有学号已复制到剪贴板！')
  } catch {
    alert('复制失败，请手动选择')
  }
}

const goBack = () => router.push(`/teacher/course/${courseId}`)
const goToAssistant = () => router.push('/teacher/assistant')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

onMounted(() => {
  if (!courseId) {
    alert('无效的课程ID')
    router.push('/teacher')
  } else {
    fetchStudents()
  }
})
</script>
