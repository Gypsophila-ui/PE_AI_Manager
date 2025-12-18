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
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'

// 模拟数据（实际项目中调用 get_submit_info API 获取）
const submission = ref({
  submit_id: 'sub001',
  title: '50米折返跑测试',
  courseId: 'PE2025-01',
  description: '请录制完整50米折返跑过程，确保计时准确，动作标准。',
  deadline: '2025-12-20',
  CREATE_TIME: '2025-12-15T14:30:00',
  score: 92,
  video_url: 'https://example.com/ai-analysis/sub001-processed.mp4',  // AI分析后视频
  AI_feedback: `动作整体流畅，起跑爆发力优秀。\n优点：\n- 折返转体迅速，无多余停顿\n- 臂腿协调性好\n改进建议：\n- 最后5米冲刺时上身稍前倾，可进一步提升速度\n- 注意呼吸节奏，避免憋气`,
  teacher_feedback: '很好！动作很标准，比上次进步明显，继续保持！下次可以尝试在折返时更低重心。',
})

const courses = ref([
  { id: 'PE2025-01', name: '初三（1）班 体育' },
  { id: 'PE2025-03', name: '游泳选修' },
])

// 实际项目中：调用 get_submit_info(submit_id = route.params.id)

// 辅助方法
const getCourseName = (courseId) => {
  const c = courses.value.find(item => item.id === courseId)
  return c ? c.name : '未知课程'
}

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

const route = useRoute()
const router = useRouter()

// 从当前路由解析出 courseId 和 assignmentId 路由设计为'/course/:courseId/assignments/:assignmentId/submission/:submitId'
const courseId = route.params.courseId
const assignmentId = route.params.assignmentId

// 判断是否截止
const isDeadlinePassed = computed(() => {
  if (!submission.value.deadline) return false
  const deadlineDate = new Date(submission.value.deadline)
  const now = new Date()
  return now > deadlineDate
})

// 重新提交跳转
const reSubmit = () => {
  // 直接用当前路由中的参数跳转到提交页面
  router.push(`/course/${courseId}/submit/${assignmentId}`)
}

const goBack = () => {
  // 根据来源返回（可通过 router.options.history.state.back 判断，或固定返回一览页）
  router.push('/student/assignments')
}

const goToHome = () => router.push('/student')

const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}
</script>
