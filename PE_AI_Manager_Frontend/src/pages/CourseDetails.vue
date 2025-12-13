<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-8">
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
              <router-link :to="`/course/${course.id}/assignments/${assignment.id}`" 
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
        <p class="text-gray-500">该课程目前还没有发布任何作业</p>
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
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from '../services/axios'

const router = useRouter()
const route = useRoute()

// 课程和作业相关
const course = ref(null)
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')

// 获取课程ID
const courseId = route.params.courseId || route.params.id

// 获取课程详情和作业列表
const fetchCourseDetails = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    // 定义示例课程ID列表
    const exampleCourseIds = ['PE101', 'PE202', 'PE303']

    // 如果是示例课程ID，直接使用mock数据，不发出真实API请求
    if (exampleCourseIds.includes(courseId)) {
      console.log('使用示例课程数据，跳过真实API请求')

      // 使用mock数据，根据不同的课程ID返回不同的课程详情和作业列表
      const mockCourses = {
        'PE101': {
          id: 'PE101',
          name: '体能训练课程',
          description: '学习和掌握各种体能训练技巧，包括俯卧撑、仰卧起坐等基础动作',
          subject: '体育',
          status: '进行中',
          assignments: [
            {
              id: 1,
              title: '俯卧撑标准动作练习',
              description: '完成标准俯卧撑动作，要求动作规范，身体保持直线',
              deadline: '2024-01-20T23:59:59',
              create_time: '2024-01-10T08:00:00',
              course_id: 'PE101',
              subject: '体能训练',
              status: '进行中',
              points: 100
            },
            {
              id: 2,
              title: '仰卧起坐耐力测试',
              description: '在规定时间内完成尽可能多的仰卧起坐，测试核心力量',
              deadline: '2024-01-25T23:59:59',
              create_time: '2024-01-15T10:30:00',
              course_id: 'PE101',
              subject: '体能测试',
              status: '进行中',
              points: 100
            }
          ]
        },
        'PE202': {
          id: 'PE202',
          name: '协调能力训练',
          description: '通过跳绳、敏捷梯等训练提升身体协调性和反应能力',
          subject: '体育',
          status: '已完成',
          assignments: [
            {
              id: 3,
              title: '跳绳技巧练习',
              description: '掌握基本跳绳技巧，提高协调性和耐力',
              deadline: '2024-01-15T23:59:59',
              create_time: '2024-01-05T14:20:00',
              course_id: 'PE202',
              subject: '协调训练',
              status: '已完成',
              points: 100
            }
          ]
        },
        'PE303': {
          id: 'PE303',
          name: '田径基础训练',
          description: '学习田径运动的基本技能，包括短跑、跳远等项目',
          subject: '体育',
          status: '进行中',
          assignments: [
            {
              id: 4,
              title: '50米短跑测试',
              description: '进行50米短跑测试，记录成绩',
              deadline: '2024-02-10T23:59:59',
              create_time: '2024-01-20T09:00:00',
              course_id: 'PE303',
              subject: '田径',
              status: '进行中',
              points: 100
            }
          ]
        }
      }

      // 获取对应的课程详情
      course.value = mockCourses[courseId]
    } else {
      // 对于非示例课程ID，使用真实的API调用
      console.log('使用真实API请求获取课程详情')

      // 这里应该调用真实API获取课程详情和作业列表
      // 暂时使用默认mock数据
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
  router.push('/student')
}

// 组件挂载时获取课程详情
onMounted(() => {
  fetchCourseDetails()
})
</script>