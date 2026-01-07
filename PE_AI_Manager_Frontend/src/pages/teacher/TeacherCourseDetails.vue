<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-8">
      <!-- 顶部导航栏 -->
      <div class="flex justify-between items-center py-4">
        <div class="flex items-center gap-2">
          <button @click="goBack" class="px-4 py-2 text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors">
            返回
          </button>
        </div>

      </div>

      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">📚 课程详情</h2>
      </section>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
      </div>

      <!-- 错误信息 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-3xl p-6">
        <div class="flex items-center gap-3 mb-3">
          <div class="text-3xl text-red-500">❌</div>
          <h3 class="text-xl font-bold text-red-800">加载失败</h3>
        </div>
        <p class="text-red-700">{{ errorMessage }}</p>
        <button @click="fetchCourseDetails" class="mt-4 px-6 py-2 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow">
          重试
        </button>
      </div>

      <!-- 课程信息卡片 -->
      <section v-else-if="course" class="bg-white rounded-3xl shadow-xl p-6">
        <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-6">
          <div class="flex-1">
            <h3 class="text-3xl font-bold text-gray-800 mb-2">{{ course.name }}</h3>
            <p class="text-gray-600 mb-4">{{ course.info || '暂无描述' }}</p>

            <!-- 课程基本信息 -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📚</span>
                <span>{{ course.subject }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📊</span>
                <span>{{ course.is_active === '1' ? '进行中' : '未发布' }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📝</span>
                <span>{{ course.assignments.length }} 个作业</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="text-gray-400">🔑</span>
                <span>邀请码</span>
                <span class="font-mono text-sm bg-gray-100 px-3 py-1 rounded-lg">
                  {{ course.code || '加载中...' }}
                </span>
                <button
                  @click="copyCode"
                  class="ml-2 text-xs px-2 py-1 bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition"
                  title="复制邀请码"
                >
                  复制
                </button>
              </div>
            </div>
          </div>

    <!-- 操作按钮组 -->
    <div class="flex flex-wrap gap-3">
      <button @click="editCourse" class="px-5 py-2 rounded-xl bg-green-500 text-white hover:bg-green-600 transition-all shadow">
        ✏️ 编辑课程
      </button>
      <button @click="manageStudents" class="px-5 py-2 rounded-xl bg-indigo-500 text-white hover:bg-indigo-600 transition-all shadow">
        👥 学生管理
      </button>
      <button @click="showPublishAssignment = true" class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
        📝 发布作业
      </button>
    </div>
  </div>
</section>

      <!-- 作业列表 -->
      <section v-if="course && course.assignments.length > 0" class="bg-white rounded-3xl shadow-xl p-6">
        <h3 class="text-2xl font-bold text-gray-800 mb-4">课程作业</h3>
        <div class="space-y-3">
          <div v-for="assignment in course.assignments" :key="assignment.id"
               class="bg-white rounded-xl shadow-md border border-gray-100 p-4 hover:shadow-lg transition-all">
            <div class="flex flex-col md:flex-row md:items-center justify-between">
              <div class="flex-1">
                <h4 class="text-lg font-semibold text-gray-800 mb-1">{{ assignment.title }}</h4>
                <p class="text-sm text-gray-600 mb-2">{{ assignment.description || '暂无描述' }}</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs px-3 py-1 rounded-full bg-purple-100 text-purple-800">
                    {{ assignment.aiTypeDisplay }}
                  </span>
                  <span :class="['text-xs px-2 py-1 rounded-full',
                                assignment.status === '进行中' ? 'bg-blue-100 text-blue-800' :
                                'bg-red-100 text-red-800']">
                    {{ assignment.status }}
                  </span>
                  <span class="text-xs text-gray-500">截止: {{ formatDate(assignment.deadline) }}</span>
                </div>
              </div>
              <router-link :to="`${course.id}/assignment/${assignment.id}`"
                           class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium">
                查看详情 →
              </router-link>
            </div>
          </div>
        </div>
      </section>

      <!-- 无作业提示 -->
      <section v-else-if="course && course.assignments.length === 0" class="bg-white rounded-3xl shadow-xl p-10 text-center">
        <div class="text-6xl text-gray-300 mb-4">📝</div>
        <h3 class="text-2xl font-bold text-gray-800 mb-2">暂无作业</h3>
        <p class="text-gray-500 mb-6">该课程目前还没有发布任何作业</p>
        <button @click="showPublishAssignment = true" class="px-6 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
          📝 发布第一个作业
        </button>
      </section>

      <!-- 未找到课程 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-10 text-center">
        <div class="text-6xl text-gray-300 mb-4">🔍</div>
        <h3 class="text-2xl font-bold text-gray-800 mb-2">未找到课程</h3>
        <p class="text-gray-500 mb-6">无法找到指定ID的课程信息</p>
        <button @click="router.push('/teacher')" class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
          返回首页
        </button>
      </section>

      <!-- 发布作业弹窗 -->
      <div v-if="showPublishAssignment" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
        <div class="bg-white rounded-3xl shadow-xl p-8 max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-2xl font-bold text-gray-800">📝 发布新作业</h3>
            <button @click="showPublishAssignment = false" class="text-2xl text-gray-400 hover:text-gray-600 transition-colors">
              ×
            </button>
          </div>

          <form @submit.prevent="submitForm">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <!-- 作业标题 -->
              <div class="col-span-1 md:col-span-3">
                <label for="title" class="block text-sm font-medium text-gray-700 mb-2">作业标题</label>
                <input
                  id="title"
                  v-model="newAssignment.title"
                  type="text"
                  required
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                  placeholder="例如：深蹲标准动作测试"
                />
              </div>

              <!-- AI识别运动类型 -->
              <div>
                <label for="aiType" class="block text-sm font-medium text-gray-700 mb-2">运动类型</label>
                <select
                  id="aiType"
                  v-model="newAssignment.aiType"
                  required
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                >
                  <option value="">请选择运动类型</option>
                  <option value="squat">深蹲</option>
                  <option value="pushup">俯卧撑</option>
                  <option value="deadlift">硬拉</option>
                </select>
              </div>

              <!-- 要求完成次数 -->
              <div>
                <label for="requiredCount" class="block text-sm font-medium text-gray-700 mb-2">要求完成次数</label>
                <input
                  id="requiredCount"
                  v-model.number="newAssignment.requiredCount"
                  type="number"
                  min="1"
                  max="999"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                  placeholder="30"
                  required
                />
                <p class="text-xs text-gray-500 mt-1">学生需完成该次数的动作</p>
              </div>

              <!-- 截止日期 -->
              <div>
                <label for="deadline" class="block text-sm font-medium text-gray-700 mb-2">截止日期</label>
                <input
                  id="deadline"
                  v-model="newAssignment.deadline"
                  type="datetime-local"
                  required
                  max="2999-12-31T23:59"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                />
              </div>
            </div>

            <!-- 作业描述 -->
            <div class="mt-6">
              <label for="description" class="block text-sm font-medium text-gray-700 mb-2">作业描述</label>
                <textarea
                  id="description"
                  v-model="newAssignment.description"
                  rows="4"
                  required
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                  placeholder="详细描述动作要求、次数、评分标准等"
                ></textarea>
            </div>

            <div class="mt-10 flex gap-4 justify-end">
              <button
                type="button"
                @click="showPublishAssignment = false"
                class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow"
              >
                取消
              </button>
              <button
                type="submit"
                class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg"
              >
                发布作业
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import dayjs from 'dayjs';
import apiClient from '../../services/axios.js'
import { cacheService } from '../../services/DataCacheService.js'

const router = useRouter()
const route = useRoute()

const course = ref(null)
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')
const showPublishAssignment = ref(false)

const newAssignment = ref({
  title: '',
  aiType: '',
  requiredCount: 30,
  description: '',
  deadline: ''
})

const courseId = route.params.courseId

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.token || 'valid_teacher_jwt'

// AI类型中英文映射
const aiTypeMap = {
  squat: '深蹲',
  pushup: '俯卧撑',
  deadlift: '硬拉'
}

const copyCode = async () => {
  if (!course.value?.code) {
    alert('邀请码尚未加载')
    return
  }
  try {
    await navigator.clipboard.writeText(course.value.code)
    alert('邀请码已复制到剪贴板！')
  } catch (err) {
    alert('复制失败，请手动选中复制')
  }
}

const editCourse = () => {
  router.push({
    path: `/teacher/course/${courseId}/edit`,
  })
}

const manageStudents = () => {
  router.push(`/teacher/course/${courseId}/students`)
}

const fetchCourseDetails = async () => {
  loading.value = true
  error.value = false

  try {
    // 1. 获取课程基本信息 (使用缓存)
    const courseResp = await cacheService.fetchWithCache(`course_info:${courseId}`, () =>
      apiClient.post('/Course/get_info_by_course_id', { First: courseId })
    )
    if (!courseResp.data.success) {
      errorMessage.value = '课程不存在或已被删除'
      error.value = true
      return
    }
    const courseRespData = courseResp.data.data.trim().replace(/\t\r$/g, '');
    const courseRespDataArray = courseRespData.split(/\t\r/).filter(item => item !== '');

    const c = courseRespDataArray
    course.value = {
      id: courseId,
      name: c[1],
      info: c[2] || '',
      code: c[3],
      subject: c[3] || '未知课号',
      is_active: c[5],
      assignments: []
    }

    // 2. 获取作业ID列表 (使用缓存)
    const homeworkResp = await cacheService.fetchWithCache(`course_homework_ids:${courseId}`, () =>
      apiClient.post('/Homework/get_homework_id_by_course', {
        First: '1', Second: teacherId, Third: jwt, Fourth: courseId
      })
    )

    if (!homeworkResp.data.success || !homeworkResp.data.data.trim()) {
      course.value.assignments = []
      loading.value = false
      return
    }

    const homeworkIds = homeworkResp.data.data.split('\t\r').filter(Boolean)

    // 3. 并行获取每个作业的详情 + AI类型 (使用缓存)
    const assignmentPromises = homeworkIds.map(async (id) => {
      const [infoResp, aiResp] = await Promise.all([
        cacheService.fetchWithCache(`homework_info:${id}`, () =>
          apiClient.post('/Homework/get_info_by_homework_id', { First: courseId, Second: id })
        ),
        cacheService.fetchWithCache(`homework_ai_config:${id}`, () =>
          apiClient.post('/Homework/get_AI_type', { First: id })
        )
      ])


      if (!infoResp.data.success || !infoResp.data.data.trim()) return null


      const infoRespData = infoResp.data.data.trim().replace(/\t\r$/g, '');
      const infoRespDataArray = infoRespData.split(/\t\r/).filter(item => item !== '');
      const d = infoRespDataArray

      let rawAiType = ''
      if (aiResp.data.success) {
          const config = aiResp.data.data.trim()
          const parts = config.split('\t\r')
          rawAiType = parts[0] || 'squat'

      }
      const aiTypeDisplay = aiTypeMap[rawAiType] || '未知动作'

      return {
        id,
        title: d[0],
        description: d[1],
        deadline: d[2],
        create_time: d[3],
        status: new Date(d[2]) > new Date() ? '进行中' : '已截止',
        aiType: rawAiType,
        aiTypeDisplay
      }
    })

    const assignments = (await Promise.all(assignmentPromises)).filter(Boolean)
    course.value.assignments = assignments

  } catch (err) {
    console.error(err)
    error.value = true
    errorMessage.value = '加载失败，请检查网络'
  } finally {
    loading.value = false
  }
}

const submitForm = async () => {
  if (!newAssignment.value.title || !newAssignment.value.description || !newAssignment.value.deadline || !newAssignment.value.aiType || !newAssignment.value.requiredCount) {
    alert('请完整填写所有字段')
    return
  }

  try {
    // 1. 创建作业
    const addResp = await apiClient.post('/Homework/new_homework', {
      First: teacherId,
      Second: jwt,
      Third: courseId,
      Fourth: newAssignment.value.title,
      Fifth: newAssignment.value.description,
      Sixth: dayjs(newAssignment.value.deadline).format('YYYY-MM-DD HH:mm:ss')
    })

    const homeworkId = addResp.data?.data?.trim()
    if (!homeworkId) {
      alert('作业创建失败：未返回ID')
      return
    }

    // 2. 设置AI类型
    const setResp = await apiClient.post('/Homework/set_AI_type', {
      First: teacherId,
      Second: jwt,
      Third: courseId,
      Fourth: homeworkId,
      Fifth: newAssignment.value.aiType,
      Sixth: newAssignment.value.requiredCount.toString()
    })

    if (!setResp.data.success) {
      alert('警告：AI识别模型设置失败，但作业已创建')
    }

    alert('作业发布成功！')
    cacheService.invalidate(`course_homework_ids:${courseId}`);
    showPublishAssignment.value = false
    newAssignment.value = { title: '', aiType: '', description: '', deadline: '' }
    await fetchCourseDetails()  // 刷新列表，显示新作业和正确运动类型

  } catch (err) {
    console.error(err)
    alert('发布失败，请重试')
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const goBack = () => router.push('/teacher')

onMounted(fetchCourseDetails)
</script>
