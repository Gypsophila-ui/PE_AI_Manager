<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-8">
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
        <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6">
          <div>
            <h3 class="text-3xl font-bold text-gray-800 mb-2">{{ course.name }}</h3>
            <p class="text-gray-600 mb-4">{{ course.description }}</p>
            <div class="flex items-center gap-4">
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📚</span>
                <span>{{ course.subject }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📊</span>
                <span>{{ course.status }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📝</span>
                <span>{{ course.assignments.length }} 个作业</span>
              </div>
            </div>
          </div>
          <!-- 教师操作按钮 -->
          <div class="flex gap-4 mt-4 md:mt-0">
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
                <p class="text-sm text-gray-600 mb-2">{{ assignment.description }}</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs text-gray-500">{{ assignment.subject }}</span>
                  <span :class="['text-xs px-2 py-1 rounded-full',
                                assignment.status === '进行中' ? 'bg-blue-100 text-blue-800' :
                                assignment.status === '已完成' ? 'bg-green-100 text-green-800' :
                                'bg-gray-100 text-gray-800']">
                    {{ assignment.status }}
                  </span>
                  <span class="text-xs text-gray-500">截止时间: {{ formatDate(assignment.deadline) }}</span>
                </div>
              </div>
              <router-link :to="`/teacher/assignments/${assignment.id}`"
                          class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium">
                查看详情
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
        <button @click="goBack" class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
          返回首页
        </button>
      </section>
    </div>

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
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <!-- 作业标题 -->
            <div class="col-span-1 md:col-span-2">
              <label for="title" class="block text-sm font-medium text-gray-700 mb-2">作业标题</label>
              <input
                id="title"
                v-model="assignment.title"
                type="text"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                placeholder="例如：50米折返跑测试"
                required
              />
            </div>

            <!-- 作业科目 -->
            <div>
              <label for="subject" class="block text-sm font-medium text-gray-700 mb-2">作业科目</label>
              <select
                id="subject"
                v-model="assignment.subject"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                required
              >
                <option value="">请选择科目</option>
                <option value="田径">田径</option>
                <option value="力量">力量</option>
                <option value="弹跳">弹跳</option>
                <option value="柔韧性">柔韧性</option>
                <option value="球类">球类</option>
              </select>
            </div>

            <!-- 作业分值 -->
            <div>
              <label for="points" class="block text-sm font-medium text-gray-700 mb-2">作业分值</label>
              <input
                id="points"
                v-model.number="assignment.points"
                type="number"
                min="0"
                max="100"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                placeholder="100"
                required
              />
            </div>

            <!-- 截止日期 -->
            <div>
              <label for="deadline" class="block text-sm font-medium text-gray-700 mb-2">截止日期</label>
              <input
                id="deadline"
                v-model="assignment.deadline"
                type="datetime-local"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                required
              />
            </div>

            <!-- 视频要求 -->
            <div class="flex items-center gap-3">
              <input
                id="videoRequired"
                v-model="assignment.videoRequired"
                type="checkbox"
                class="w-5 h-5 text-blue-600 rounded focus:ring-blue-500"
              />
              <label for="videoRequired" class="text-sm font-medium text-gray-700">要求提交视频</label>
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
              placeholder="详细描述作业要求、评分标准等信息"
              required
            ></textarea>
          </div>

          <!-- 提交按钮 -->
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
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from '../../services/axios'
import { classes, courses } from '../../data/mockData'

const router = useRouter()
const route = useRoute()

// 课程和作业相关
const course = ref(null)
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')
const showPublishAssignment = ref(false)

// 作业表单数据
const assignment = ref({
  title: '',
  subject: '',
  description: '',
  deadline: '',
  points: 100,
  videoRequired: true,
  courseId: route.params.courseId || route.params.id
})

// 获取课程ID
const courseId = route.params.courseId || route.params.id

// 获取课程详情和作业列表
const fetchCourseDetails = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    // 从mock数据中获取课程详情
    const foundCourse = courses.find(c => c.id === courseId)
    if (foundCourse) {
      course.value = foundCourse
    } else {
      // 对于非示例课程ID，使用默认mock数据
      console.log('课程不存在，使用默认数据')
      course.value = {
        id: courseId,
        name: '默认课程',
        description: '这是一个默认课程的描述。',
        subject: '体育',
        status: '进行中',
        assignments: []
      }
    }
  } catch (err) {
    console.error('获取课程详情失败:', err)
    error.value = true
    errorMessage.value = err.message
  } finally {
    loading.value = false
  }
}

// 提交表单
const submitForm = () => {
  // 验证表单
  if (!assignment.value.title || !assignment.value.description || !assignment.value.deadline) {
    alert('请填写所有必填字段')
    return
  }

  // 模拟发布作业
  console.log('发布作业:', assignment.value)

  // 在真实环境中，这里应该调用API发布作业
  // 模拟成功后更新课程作业列表
  const newAssignment = {
    id: Date.now(),
    ...assignment.value,
    status: '进行中',
    create_time: new Date().toISOString()
  }

  // 更新本地课程作业列表
  if (course.value) {
    course.value.assignments.push(newAssignment)
  }

  // 显示发布成功
  alert('作业发布成功！')

  // 关闭弹窗并重置表单
  showPublishAssignment.value = false
  resetForm()
}

// 重置表单
const resetForm = () => {
  assignment.value = {
    title: '',
    subject: '',
    description: '',
    deadline: '',
    points: 100,
    videoRequired: true,
    courseId: courseId
  }
}

// 格式化日期
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 导航函数
const goBack = () => {
  router.push('/teacher')
}

const goToAssistant = () => {
  router.push('/teacher/assistant')
}

const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

// 组件挂载时获取课程详情
onMounted(() => {
  fetchCourseDetails()
})
</script>
