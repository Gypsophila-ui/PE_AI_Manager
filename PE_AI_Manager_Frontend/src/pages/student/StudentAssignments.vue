<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-4xl mx-auto p-6 space-y-10">


      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">📚 作业详情</h2>
        <p class="text-gray-600">查看作业要求和提交状态</p>
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
        <button @click="fetchAssignmentDetails" class="mt-4 px-6 py-2 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow">
          重试
        </button>
      </div>

      <!-- 作业信息卡片 -->
      <section v-else-if="assignment" class="bg-white rounded-3xl shadow-xl p-6">
        <h3 class="text-2xl font-bold text-gray-800 mb-4">{{ assignment.title }}</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">📅</span>
            <div>
              <div class="text-xs text-gray-400">创建时间</div>
              <div>{{ formatDate(assignment.create_time) }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">⏰</span>
            <div>
              <div class="text-xs text-gray-400">截止时间</div>
              <div>{{ formatDate(assignment.deadline) }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">📚</span>
            <div>
              <div class="text-xs text-gray-400">科目</div>
              <div>{{ assignment.subject }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">📋</span>
            <div>
              <div class="text-xs text-gray-400">状态</div>
              <div>
                <span :class="[
                  'px-2 py-1 rounded-full text-xs font-medium',
                  assignment.status === '进行中' ? 'bg-blue-100 text-blue-800' :
                  assignment.status === '已完成' ? 'bg-green-100 text-green-800' :
                  'bg-gray-100 text-gray-800'
                ]">
                  {{ assignment.status }}
                </span>
              </div>
            </div>
          </div>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">🏫</span>
            <div>
              <div class="text-xs text-gray-400">课程ID</div>
              <div>{{ assignment.course_id }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">💯</span>
            <div>
              <div class="text-xs text-gray-400">分值</div>
              <div>{{ assignment.points }}分</div>
            </div>
          </div>
        </div>
        <div class="mt-4 p-4 bg-blue-50 rounded-xl">
          <h4 class="font-medium text-blue-800 mb-2">作业描述：</h4>
          <p class="text-blue-700 whitespace-pre-line">{{ assignment.description }}</p>
        </div>
        <div class="mt-6 flex gap-4">
          <button
            @click="goToSubmitAssignment"
            class="px-6 py-2 rounded-xl bg-green-500 text-white hover:bg-green-600 transition-all shadow"
            :disabled="assignment.status === '已完成'"
            :class="assignment.status === '已完成' ? 'opacity-50 cursor-not-allowed' : ''"
          >
            {{ assignment.status === '已完成' ? '作业已完成' : '前往提交作业' }}
          </button>
        </div>
      </section>

      <!-- 未找到作业 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-10 text-center">
        <div class="text-6xl text-gray-300 mb-4">🔍</div>
        <h3 class="text-2xl font-bold text-gray-800 mb-2">未找到作业</h3>
        <p class="text-gray-500 mb-6">无法找到指定ID的作业信息</p>
        <button @click="goBack" class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
          返回上一页
        </button>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from '../../services/axios'

const router = useRouter()
const route = useRoute()

// 作业详情相关
const assignment = ref(null)
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')

// 获取作业ID
const assignmentId = route.params.id

// 获取作业详情
const fetchAssignmentDetails = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    // 定义示例作业ID列表
    const exampleAssignmentIds = ['1', '2', '3']

    // 如果是示例作业ID，直接使用mock数据，不发出真实API请求
    if (exampleAssignmentIds.includes(assignmentId)) {
      console.log('使用示例作业数据，跳过真实API请求')

      // 使用mock数据，根据不同的作业ID返回不同的作业详情
      const mockAssignments = {
        '1': {
          title: '俯卧撑标准动作练习',
          description: '完成标准俯卧撑动作，要求动作规范，身体保持直线。注意：1. 双手与肩同宽；2. 身体从头部到脚踝保持一条直线；3. 下降时胸部接近地面；4. 上升时手臂完全伸直。',
          deadline: '2024-01-20T23:59:59',
          create_time: '2024-01-10T08:00:00',
          course_id: 'PE101',
          subject: '体能训练',
          status: '进行中',
          points: 100
        },
        '2': {
          title: '仰卧起坐耐力测试',
          description: '在规定时间内完成尽可能多的仰卧起坐，测试核心力量。要求：1. 双腿弯曲90度；2. 双手交叉抱头；3. 起身时肘部触及膝盖；4. 躺下时肩部完全接触地面。',
          deadline: '2024-01-25T23:59:59',
          create_time: '2024-01-15T10:30:00',
          course_id: 'PE101',
          subject: '体能测试',
          status: '进行中',
          points: 100
        },
        '3': {
          title: '跳绳技巧练习',
          description: '掌握基本跳绳技巧，提高协调性和耐力。练习内容：1. 单摇跳绳（每分钟至少120次）；2. 双摇跳绳（尝试完成10次连续双摇）；3. 交叉跳绳（左右交叉各50次）。',
          deadline: '2024-01-15T23:59:59',
          create_time: '2024-01-05T14:20:00',
          course_id: 'PE101',
          subject: '协调训练',
          status: '已完成',
          points: 100
        }
      }

      // 获取对应的作业详情
      assignment.value = mockAssignments[assignmentId]
    } else {
      // 对于非示例作业ID，使用真实的API调用
      console.log('使用真实API请求获取作业详情')

      const response = await axios.post('/api/get_info_by_homework_id', {
        course_id: 'course123', // 实际应该从用户课程信息中获取
        homework_id: assignmentId,
        user_type: '0', // 学生
        user_id: 'user123', // 实际应该从登录信息中获取
        jwt: 'mock_jwt_token' // 实际应该从登录信息中获取
      })

      if (response.data.code === 0) {
        // 成功获取作业详情
        assignment.value = response.data.data
      } else {
        // 处理API错误
        throw new Error(`API错误：${response.data.message || '获取作业详情失败'}`)
      }
    }
  } catch (err) {
    console.error('获取作业详情失败:', err)
    error.value = true
    errorMessage.value = err.message

    // 如果API请求失败，使用默认的mock数据作为 fallback
    assignment.value = {
      title: '默认作业',
      description: '这是一个默认作业的描述。',
      deadline: '2024-01-30T23:59:59',
      create_time: '2024-01-10T08:00:00',
      course_id: 'PE101',
      subject: '体育',
      status: '进行中',
      points: 100
    }
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

const goToAssistant = () => {
  router.push('/student/assistant')
}

const goToSubmitAssignment = () => {
  router.push(`/student/submit/${assignmentId}`)
}

const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

// 组件挂载时获取作业详情
onMounted(() => {
  fetchAssignmentDetails()
})
</script>
