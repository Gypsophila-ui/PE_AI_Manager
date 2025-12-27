<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-4xl mx-auto p-6 flex justify-between items-center">
      <div class="flex items-center gap-2">
        <button @click="goBack" class="text-2xl text-gray-600 hover:text-gray-800 transition-colors">
          ←
        </button>
        <h1 class="text-xl font-bold text-gray-800">作业管理</h1>
      </div>
      <div class="flex gap-3">
        <button @click="handleEdit" class="px-4 py-2 rounded-xl bg-white text-blue-600 border border-blue-200 hover:bg-blue-50 transition-all shadow-sm flex items-center gap-1">
          <span>✏️</span> 编辑
        </button>
        <button @click="handleDelete" class="px-4 py-2 rounded-xl bg-white text-red-600 border border-red-200 hover:bg-red-50 transition-all shadow-sm flex items-center gap-1">
          <span>🗑️</span> 删除
        </button>
      </div>
    </div>

    <div class="max-w-4xl mx-auto p-6 space-y-8 pb-20">

      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
      </div>

      <template v-else-if="assignment">
        <section class="bg-white rounded-3xl shadow-xl p-8 border border-white/50">
          <div class="flex justify-between items-start mb-6">
            <div class="space-y-2">
              <span class="px-3 py-1 rounded-full text-xs font-bold bg-purple-100 text-purple-700 uppercase tracking-wide">
                {{ assignment.aiTypeDisplay || '标准动作' }}
              </span>
              <h2 class="text-3xl font-extrabold text-gray-800">{{ assignment.title }}</h2>
            </div>
          </div>

          <div class="grid grid-cols-2 md:grid-cols-4 gap-4 py-6 border-y border-gray-50">
            <div class="text-center md:text-left">
              <p class="text-xs text-gray-400 font-bold mb-1">提交人数</p>
              <p class="text-xl font-bold text-gray-700">{{ stats.submittedCount }} / {{ stats.totalStudents }}</p>
            </div>
            <div class="text-center md:text-left">
              <p class="text-xs text-gray-400 font-bold mb-1">待批改</p>
              <p class="text-xl font-bold text-orange-500">{{ stats.pendingCount }}</p>
            </div>
            <div class="text-center md:text-left">
              <p class="text-xs text-gray-400 font-bold mb-1">平均分</p>
              <p class="text-xl font-bold text-gray-700">{{ stats.avgScore }}</p>
            </div>
            <div class="text-center md:text-left">
              <p class="text-xs text-gray-400 font-bold mb-1">当前状态</p>
              <span :class="['text-sm font-black italic', isDeadlinePassed ? 'text-red-500' : 'text-green-500']">
                {{ isDeadlinePassed ? '已截止' : '进行中' }}
              </span>
            </div>
          </div>

          <div class="mt-8">
            <h4 class="font-bold text-gray-800 mb-4 flex items-center gap-2">
              <span class="p-1.5 bg-gray-100 rounded-lg text-sm">📋</span>
              作业要求与描述
            </h4>
            <div class="bg-gray-50 rounded-2xl p-6 text-gray-600 leading-relaxed whitespace-pre-line text-sm border border-gray-100">
              {{ assignment.description }}
            </div>
          </div>

          <div class="mt-8 pt-4 flex flex-wrap items-center gap-x-8 gap-y-2 text-[13px] text-gray-400 border-t border-gray-50">
            <div class="flex items-center gap-1.5">
              <span class="opacity-60">创建时间:</span>
              <span class="font-medium">{{ formatDate(assignment.create_time) }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <span class="opacity-60">截止日期:</span>
              <span :class="['font-medium', isDeadlinePassed ? 'text-red-400' : 'text-gray-500']">
                {{ formatDate(assignment.deadline) }}
              </span>
            </div>
          </div>
        </section>

        <section class="flex flex-col items-center pt-8">
          <button
            @click="goToGrading"
            class="group relative px-16 py-4 rounded-2xl bg-blue-600 text-white font-bold text-lg hover:bg-blue-700 transition-all shadow-2xl shadow-blue-200 hover:-translate-y-1 flex items-center gap-3"
          >
            进入批改工作台
            <span class="text-2xl group-hover:translate-x-1 transition-transform">→</span>
          </button>
          <p class="mt-4 text-gray-400 text-xs font-medium tracking-wide">
            查看已提交学生名单并进行 AI 辅助评分
          </p>
        </section>
      </template>

      <section v-else class="bg-white rounded-3xl shadow-xl p-16 text-center">
        <div class="text-7xl mb-6 grayscale opacity-50">🔎</div>
        <h3 class="text-xl font-bold text-gray-800 mb-2">未找到作业</h3>
        <p class="text-gray-400 mb-8">该作业可能已被删除或路径不正确</p>
        <button @click="goBack" class="px-8 py-3 rounded-xl bg-gray-100 text-gray-600 font-bold hover:bg-gray-200 transition-all">
          返回课程详情
        </button>
      </section>
    </div>

    <!-- 编辑作业模态框 -->
    <div v-if="showEditModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="relative bg-white rounded-3xl shadow-2xl w-full max-w-4xl mx-4 max-h-[90vh] overflow-y-auto">
        <div class="p-8">
          <div class="flex justify-between items-center mb-6">
            <h3 class="text-2xl font-bold text-gray-800">✏️ 编辑作业</h3>
            <button @click="showEditModal = false" class="text-3xl text-gray-400 hover:text-gray-600">×</button>
          </div>

          <form @submit.prevent="submitEdit" class="space-y-6">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- 作业标题 -->
              <div class="col-span-1 md:col-span-2">
                <label class="block text-sm font-medium text-gray-700 mb-2">作业标题</label>
                <input
                  v-model="editForm.title"
                  type="text"
                  required
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                  placeholder="例如：深蹲标准动作测试"
                />
              </div>

              <!-- AI识别运动类型 -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">AI识别运动类型</label>
                <select
                  v-model="editForm.aiType"
                  required
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                >
                  <option value="squat">深蹲</option>
                  <option value="pushup">俯卧撑</option>
                  <option value="deadlift">硬拉</option>
                </select>
              </div>

              <!-- 截止日期 -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">截止日期</label>
                <input
                  v-model="editForm.deadline"
                  type="datetime-local"
                  required
                  max="2999-12-31T23:59"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                />
                <p class="text-xs text-gray-500 mt-1">输入4位年份后自动跳转</p>
              </div>
            </div>

            <!-- 作业描述 -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">作业描述</label>
              <textarea
                v-model="editForm.description"
                rows="5"
                required
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm resize-none"
                placeholder="详细描述动作要求、次数、评分标准等"
              ></textarea>
            </div>

            <div class="mt-8 flex gap-4 justify-end">
              <button
                type="button"
                @click="showEditModal = false"
                class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow"
              >
                取消
              </button>
              <button
                type="submit"
                class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg"
              >
                更新作业
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '../../services/axios.js'

const router = useRouter()
const route = useRoute()

const { courseId, assignmentId } = route.params

const loading = ref(true)
const assignment = ref(null)
const showEditModal = ref(false)

const stats = ref({
  submittedCount: 0,
  totalStudents: 0,
  pendingCount: 0,
  avgScore: '-'
})

const editForm = ref({
  title: '',
  aiType: 'squat',
  description: '',
  deadline: ''
})

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.jwt || ''

// AI类型映射
const aiTypeMap = {
  squat: '深蹲',
  pushup: '俯卧撑',
  deadlift: '硬拉'
}

const fetchDetail = async () => {
  loading.value = true
  try {
    // 1. 获取作业基本信息
    const infoResp = await apiClient.post('/api/get_info_by_homework_id', {
      First: courseId,
      Second: assignmentId
    })

    if (!Array.isArray(infoResp.data) || infoResp.data.length < 4) {
      assignment.value = null
      return
    }

    const d = infoResp.data

    // 2. 获取当前AI类型
    const aiResp = await apiClient.post('/api/get_AI_type', { First: assignmentId })
    const currentAiType = aiResp.data?.[0] || 'squat'

    assignment.value = {
      title: d[0],
      description: d[1],
      deadline: d[2],
      create_time: d[3],
      aiType: currentAiType,
      aiTypeDisplay: aiTypeMap[currentAiType] || '标准动作'
    }

    // 模拟统计（可替换为真实接口）
    stats.value = {
      submittedCount: 24,
      totalStudents: 45,
      pendingCount: 6,
      avgScore: 89.2
    }

  } catch (err) {
    console.error('加载作业详情失败:', err)
    assignment.value = null
  } finally {
    loading.value = false
  }
}

const isDeadlinePassed = computed(() => {
  if (!assignment.value?.deadline) return false
  return new Date(assignment.value.deadline) < new Date()
})

const formatDate = (dateString) => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleString('zh-CN', {
    year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit'
  })
}

const handleEdit = () => {
  if (!assignment.value) return

  editForm.value = {
    title: assignment.value.title,
    aiType: assignment.value.aiType || 'squat',
    description: assignment.value.description,
    deadline: assignment.value.deadline.slice(0, 16)  // 转为 datetime-local 格式
  }
  showEditModal.value = true
}

const submitEdit = async () => {
  try {
    // 1. 更新基本信息
    const editResp = await apiClient.post('/api/edit_homework', {
      First: teacherId,
      Second: jwt,
      Third: courseId,
      Fourth: assignmentId,
      Fifth: editForm.value.title,
      Sixth: editForm.value.description,
      Seventh: editForm.value.deadline
    })

    if (editResp.data[0] !== 0) {
      alert(`基本信息更新失败 (代码: ${editResp.data[0]})`)
      return
    }

    // 2. 更新AI类型
    const aiResp = await apiClient.post('/api/edit_AI_type', {
      First: teacherId,
      Second: jwt,
      Third: courseId,
      Fourth: assignmentId,
      Fifth: editForm.value.aiType
    })

    if (aiResp.data[0] !== 0) {
      alert('警告：AI识别类型更新失败，但其他信息已保存')
    }

    alert('作业更新成功！')
    showEditModal.value = false
    await fetchDetail()  // 刷新显示

  } catch (err) {
    console.error(err)
    alert('更新失败，请检查网络')
  }
}

const handleDelete = async () => {
  if (!confirm('确定要删除此作业吗？')) return

  try {
    const resp = await apiClient.post('/api/delete_homework', {
      First: teacherId,
      Second: jwt,
      Third: courseId,
      Fourth: assignmentId
    })

    if (resp.data[0] === 0) {
      alert('作业已删除')
      router.push(`/teacher/course/${courseId}`)
    } else {
      alert(`删除失败 (代码: ${resp.data[0]})`)
    }
  } catch (err) {
    alert('删除请求失败')
  }
}

const goToGrading = () => router.push(`/teacher/grade/course/${courseId}/assignment/${assignmentId}`)
const goBack = () => router.push(`/teacher/course/${courseId}`)

onMounted(fetchDetail)
</script>
