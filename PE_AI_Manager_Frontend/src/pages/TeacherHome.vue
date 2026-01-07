<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-10">
      <!-- 顶部 Banner -->
      <div class="relative w-full rounded-3xl overflow-hidden shadow-2xl">
        <img src="../assets/HomeHeader.jpg" class="w-full h-96 object-cover opacity-50" />
        <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent">
          <h2 class="absolute inset-0 flex items-center justify-center text-6xl font-display font-medium tracking-widest text-white drop-shadow-2xl">
            智慧体育课堂
          </h2>
          <h3 class="absolute bottom-8 left-0 right-0 text-center text-2xl text-white font-medium">科学管理，高效教学</h3>
        </div>
      </div>

      <!-- 快捷操作按钮 -->
      <section>
        <h2 class="text-3xl font-bold mb-6 text-gray-800">🚀 快捷操作</h2>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <button @click="goToPublish" class="p-6 rounded-3xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">📝</div>
            <h3 class="text-xl font-bold">发布新作业</h3>
          </button>
          <button @click="goToVideos" class="p-6 rounded-3xl bg-teal-500 text-white hover:bg-teal-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">🎥</div>
            <h3 class="text-xl font-bold">教学视频</h3>
          </button>
          <button @click="goToAssignments" class="p-6 rounded-3xl bg-purple-500 text-white hover:bg-purple-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">📚</div>
            <h3 class="text-xl font-bold">作业统计</h3>
          </button>
          <button @click="goToDashboard" class="p-6 rounded-3xl bg-orange-500 text-white hover:bg-orange-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">📊</div>
            <h3 class="text-xl font-bold">数据看板</h3>
          </button>
        </div>
      </section>

      <!-- 课程列表 -->
      <section>
        <div class="flex justify-between items-center mb-6">
          <h2 class="text-3xl font-bold text-gray-800">📚 我的课程</h2>
          <!-- 新建课程按钮 -->
          <button @click="goToCreateCourse" class="px-6 py-3 bg-green-500 text-white rounded-xl hover:bg-green-600 shadow-lg flex items-center space-x-2">
            <span class="text-2xl">+ 新建课程</span>
          </button>
        </div>

        <!-- 加载状态 -->
        <div v-if="loading" class="text-center py-20">
          <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-blue-500"></div>
          <p class="mt-6 text-xl text-gray-600">加载您的课程中...</p>
        </div>

        <!-- 错误状态 -->
        <div v-else-if="errorMsg" class="text-center py-20">
          <p class="text-2xl text-red-600 mb-6">{{ errorMsg }}</p>
          <button @click="loadCourses" class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 shadow-lg">
            重试
          </button>
        </div>

        <!-- 课程列表 -->
        <div v-else class="space-y-3">
          <div v-if="teacherCourses.length === 0" class="bg-white rounded-xl shadow-md border border-gray-100 p-8 text-center">
            <p class="text-xl text-gray-600 mb-4">暂无课程</p>
            <p class="text-gray-500">您尚未创建任何课程，点击上方“+ 新建课程”开始吧！</p>
          </div>
          <div v-else v-for="course in teacherCourses" :key="course.id"
            class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-800 mb-1">{{ course.name }}</h3>
                <p class="text-sm text-gray-600 mb-2">{{ course.info || '暂无描述' }}</p>
                <div class="flex items-center space-x-4 flex-wrap">
                  <span class="text-xs text-gray-500">{{ course.subject || '体育' }}</span>
                  <span :class="['text-xs px-2 py-1 rounded-full',
                    course.is_active === '1' ? 'bg-blue-100 text-blue-800' : 'bg-gray-100 text-gray-800']">
                    {{ course.is_active === '1' ? '进行中' : '未发布' }}
                  </span>
                  <span class="text-xs text-gray-500">作业: {{ course.assignmentCount > 0 ? course.assignmentCount : '暂无' }}</span>
                  <span class="text-xs text-gray-500">邀请码: {{ course.code || '暂无' }}</span>
                </div>
              </div>
              <div class="flex flex-wrap gap-3">
                <button @click="viewCourseDetails(course.id)" class="text-blue-500 hover:text-blue-700 text-sm font-medium">
                  查看详情
                </button>
                <button @click="editCourse(course)" class="text-green-600 hover:text-green-800 text-sm font-medium">
                  编辑
                </button>
                <button @click="manageStudents(course.id)" class="text-indigo-600 hover:text-indigo-800 text-sm font-medium">
                  学生管理
                </button>
                <button @click="deleteCourse(course.id)" class="text-red-600 hover:text-red-800 text-sm font-medium">
                  删除
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '../services/axios.js'
import { controllers } from 'chart.js'
import { cacheService } from '../services/DataCacheService.js'

const router = useRouter()

const teacherCourses = ref([])
const loading = ref(true)
const errorMsg = ref('')

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.token || ''

const loadCourses = async () => {
  loading.value = true
  errorMsg.value = ''

  try {
    const courseIdResp = await apiClient.post('/Course/get_course_id_by_teacher', {
      First: teacherId,
      Second: jwt
    })

    if (!courseIdResp.data.success) {
      errorMsg.value = '获取课程列表失败'
      return
    }
    const courseIdStr = courseIdResp.data.data
    const courseIds = courseIdStr ? courseIdStr.split('\t\r').filter(Boolean) : []

    if (courseIds.length === 0) {
      loading.value = false
      return
    }

    const coursePromises = courseIds.map(async (id) => {
      const [courseResp, homeworkResp] = await Promise.all([
        apiClient.post('/Course/get_info_by_course_id', { First: id }),
        apiClient.post('/Homework/get_homework_id_by_course', {
          First: '1',
          Second: teacherId,
          Third: jwt,
          Fourth: id
        })
      ])

      if (!courseResp.data.success) return null
      const courseRespData = courseResp.data.data.trim().replace(/\t\r$/g, '');
      const courseRespDataArray = courseRespData.split(/\t\r/).filter(item => item !== '');

      let assignmentCount = 0
      if (homeworkResp.data.success) {
        const homeworkIdStr = homeworkResp.data.data
        assignmentCount = homeworkIdStr.trim().split(/[\t\r\n]+/).filter(Boolean).length
      }

      return {
        id: id,
        name: courseRespDataArray[1],
        info: courseRespDataArray[2],
        code: courseRespDataArray[3],
        subject: courseRespDataArray[3] || '未知课号',
        semester: courseRespDataArray[4],
        is_active: courseRespDataArray[5],
        created_time: courseRespDataArray[6],
        assignmentCount: assignmentCount
      }
    })

    const results = await Promise.all(coursePromises)
    teacherCourses.value = results.filter(Boolean)

  } catch (err) {
    errorMsg.value = '加载失败，请检查网络'
    console.error(err)
  } finally {
    loading.value = false
  }
}

onMounted(loadCourses)

// 导航函数
const goToPublish = () => router.push('/teacher/publish')
const goToVideos = () => router.push('/teacher/videos')
const goToAssignments = () => router.push('/teacher/assignments')
const goToDashboard = () => router.push('/teacher/dashboard')
const viewCourseDetails = (courseId) => router.push(`/teacher/course/${courseId}`)

// 新建课程
const goToCreateCourse = () => {
  router.push('/teacher/createCourse')  // 新建模式
}

// 编辑课程
const editCourse = (course) => {
  router.push({
    path: `/teacher/course/${course.id}/edit`,
  })
}

// 学生管理
const manageStudents = (courseId) => {
  router.push(`/teacher/course/${courseId}/students`)
}

// 删除课程
const deleteCourse = async (courseId) => {
  if (!confirm(`确定要删除课程 ${courseId} 吗？删除后不可恢复！`)) return

  try {
    const resp = await apiClient.post('/Course/delete_course', {
      First: courseId,     // course_id
      Second: teacherId,   // teacher_id
      Third: jwt
    })

    if (resp.data.success) {  // 成功标志
      alert('课程删除成功')
      teacherCourses.value = teacherCourses.value.filter(c => c.id !== courseId)
      cacheService.invalidate(`teacher_course_ids:${teacherId}`)
      cacheService.invalidate(`course_info:${courseId}`)
    } else {
      alert('删除失败：' + (resp.data.message || '未知错误'))
    }
  } catch (err) {
    alert('删除失败，请检查网络')
    console.error(err)
  }
}
</script>
