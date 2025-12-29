<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-4xl mx-auto p-6 space-y-10">
      <!-- 顶部导航栏 -->
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
        <h2 class="text-4xl font-bold text-gray-800 mb-4">📝 发布新作业</h2>
        <p class="text-gray-600">填写作业信息，为学生发布新的体育作业</p>
      </section>

      <!-- 加载状态 -->
      <div v-if="loading" class="text-center py-20">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-blue-500"></div>
        <p class="mt-6 text-xl text-gray-600">加载课程中...</p>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-3xl p-6 text-center">
        <p class="text-2xl text-red-600 mb-6">{{ errorMsg }}</p>
        <button @click="loadCourses" class="px-8 py-3 rounded-xl bg-red-500 text-white hover:bg-red-600 shadow-lg">
          重试
        </button>
      </div>

      <!-- 表单 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-8">
        <form @submit.prevent="submitForm">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <!-- 作业标题 -->
            <div class="col-span-1 md:col-span-2">
              <label for="title" class="block text-sm font-medium text-gray-700 mb-2">作业标题</label>
              <input
                id="title"
                v-model="assignment.title"
                type="text"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                placeholder="例如：深蹲标准动作测试"
                required
              />
            </div>

            <!-- 运动类型 -->
            <div>
              <label for="aiType" class="block text-sm font-medium text-gray-700 mb-2">运动类型</label>
              <select
                id="aiType"
                v-model="assignment.aiType"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                required
              >
                <option value="">请选择运动类型</option>
                <option value="squat">深蹲</option>
                <option value="pushup">俯卧撑</option>
                <option value="deadlift">硬拉</option>
              </select>
            </div>

            <!-- 截止日期 -->
            <div>
              <label for="deadline" class="block text-sm font-medium text-gray-700 mb-2">截止日期</label>
              <input
                id="deadline"
                v-model="assignment.deadline"
                type="datetime-local"
                max="2999-12-31T23:59"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                required
              />
            </div>
          </div>

          <!-- 作业描述 -->
          <div class="mt-6">
            <label for="description" class="block text-sm font-medium text-gray-700 mb-2">作业描述</label>
            <textarea
              id="description"
              v-model="assignment.description"
              rows="4"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
              placeholder="详细描述作业要求、动作规范、评分标准等"
              required
            ></textarea>
          </div>

          <!-- 选择课程 -->
          <div class="mt-6">
            <label class="block text-sm font-medium text-gray-700 mb-2">选择发布课程（可多选）</label>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div
                v-for="courseItem in courses"
                :key="courseItem.id"
                class="flex items-center gap-3 p-4 rounded-xl border-2 transition-all cursor-pointer"
                :class="assignment.courseIds.includes(courseItem.id)
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-300 hover:border-gray-400'"
                @click="toggleCourse(courseItem.id)"
              >
                <input
                  type="checkbox"
                  :checked="assignment.courseIds.includes(courseItem.id)"
                  class="w-5 h-5 text-blue-600 rounded focus:ring-blue-500"
                  @click.stop.prevent
                />
                <div>
                  <h3 class="font-medium text-gray-800">{{ courseItem.name }}</h3>
                  <p class="text-sm text-gray-500">{{ courseItem.studentCount }}名学生</p>
                </div>
              </div>
            </div>
            <p v-if="courses.length === 0" class="text-gray-500 mt-4">暂无课程可发布</p>
          </div>

          <!-- 提交按钮 -->
          <div class="mt-10 flex gap-4 justify-end">
            <button type="button" @click="goBack" class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow">
              取消
            </button>
            <button type="submit" class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
              发布作业
            </button>
          </div>
        </form>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import dayjs from 'dayjs';
import apiClient from '../../services/axios.js'

const router = useRouter()

const courses = ref([])
const loading = ref(true)
const error = ref(false)
const errorMsg = ref('')

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.token || 'valid_teacher_jwt'

const assignment = ref({
  title: '',
  aiType: '',         // squat / pushup / deadlift
  description: '',
  deadline: '',
  courseIds: []       // 存储字符串类型的 course id
})

const loadCourses = async () => {
  loading.value = true
  error.value = false
  errorMsg.value = ''

  try {
    const courseIdResp = await apiClient.post('/Course/get_course_id_by_teacher', {
      First: teacherId,
      Second: jwt
    })

    if (!courseIdResp.data.success) {
      errorMsg.value = '获取课程失败'
      error.value = true
      return
    }

    const courseIdStr = courseIdResp.data.data
    const courseIds = courseIdStr ? courseIdStr.split('\t\r').filter(Boolean) : []

    if (courseIds.length === 0) {
      errorMsg.value = '您尚未创建任何课程'
      error.value = true
      return
    }

    const promises = courseIds.map(async (id) => {
      const [infoResp, studentResp] = await Promise.all([
        apiClient.post('/Course/get_info_by_course_id', { First: id }),
        apiClient.post('/Course_student/get_student_id_by_course', {
          First: teacherId,
          Second: jwt,
          Third: id
        })
      ])

      if (!infoResp.data.success || infoResp.data.data.trim() === '') {
        return null
      }

      let studentCount = 0
      if (studentResp.data.success) {
        const studentIdStr = studentResp.data.data
        studentCount = studentIdStr ? studentIdStr.split('\t\r').filter(Boolean).length : 0
      }



    const courseRespData = infoResp.data.data.trim().replace(/\t\r$/g, '');
    const courseRespDataArray = courseRespData.split(/\t\r/).filter(item => item !== '');

      return {
        id: String(id),
        name: courseRespDataArray[1].trim(),
        studentCount: studentCount
      }
    })

    const results = await Promise.all(promises)
    courses.value = results.filter(item => item !== null && item.id && item.name)

  } catch (err) {
    console.error(err)
    error.value = true
    errorMsg.value = '网络错误，请检查网络后重试'
  } finally {
    loading.value = false
  }
}

const toggleCourse = (courseId) => {
  const id = String(courseId)
  const index = assignment.value.courseIds.indexOf(id)
  if (index > -1) {
    assignment.value.courseIds.splice(index, 1)
  } else {
    assignment.value.courseIds.push(id)
  }
}

const submitForm = async () => {
  if (assignment.value.courseIds.length === 0) {
    alert('请至少选择一个课程')
    return
  }
  if (!assignment.value.title || !assignment.value.description || !assignment.value.deadline || !assignment.value.aiType) {
    alert('请填写所有必填字段')
    return
  }

  try {
    const tasks = assignment.value.courseIds.map(async (courseId) => {
      const addResp = await apiClient.post('/Homework/new_homework', {
        First: teacherId,
        Second: jwt,
        Third: courseId,
        Fourth: assignment.value.title,
        Fifth: assignment.value.description,
        Sixth: dayjs(assignment.value.deadline).format('YYYY-MM-DD HH:mm:ss')
      })

      const homeworkId = addResp.data?.data?.trim()

      if (!homeworkId || homeworkId === '') {
        return { courseId, success: false, stage: 'add_homework' }
      }

      const setResp = await apiClient.post('/Homework/set_AI_type', {
        First: teacherId,
        Second: jwt,
        Third: courseId,
        Fourth: homeworkId,
        Fifth: assignment.value.aiType
      })

      if (!setResp.data.success) {
        return { courseId, success: false, stage: 'set_AI_type' }
      }

      return { courseId, success: true }
    })

    const results = await Promise.all(tasks)
    const failed = results.filter(r => !r.success)

    if (failed.length > 0) {
      alert(`有 ${failed.length} 个课程发布失败，请检查后重试`)
    } else {
      alert('所有作业发布成功！')
      router.push('/teacher')
    }
  } catch (err) {
    console.error(err)
    alert('网络错误，请稍后重试')
  }
}

const goBack = () => router.push('/teacher')
const goToAssistant = () => router.push('/teacher/assistant')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

onMounted(loadCourses)
</script>
