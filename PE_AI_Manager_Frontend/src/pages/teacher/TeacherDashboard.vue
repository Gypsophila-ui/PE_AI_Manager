<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-7xl mx-auto p-6 space-y-10">
      <!-- 顶部导航栏 -->
      <div class="flex justify-between items-center py-4">
        <div class="flex items-center gap-3">
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
        <h2 class="text-4xl font-bold text-gray-800 mb-4">📊 数据看板</h2>
        <p class="text-gray-600">查看课程作业整体完成情况、成绩分布与趋势分析</p>
      </section>

      <!-- 查询条件卡片 -->
      <section class="bg-white rounded-3xl shadow-xl p-8">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <!-- 选择课程 -->
          <div>
            <label for="courseId" class="block text-sm font-medium text-gray-700 mb-2">选择课程（班级）</label>
            <select
              id="courseId"
              v-model="query.courseId"
              @change="fetchData"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
            >
              <option value="">全部课程</option>
              <option v-for="cls in classes" :key="cls.id" :value="cls.id">
                {{ cls.name }}
              </option>
            </select>
          </div>

          <!-- 开始日期 -->
          <div>
            <label for="startDate" class="block text-sm font-medium text-gray-700 mb-2">开始日期</label>
            <input
              id="startDate"
              type="date"
              v-model="query.startDate"
              @change="fetchData"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
            />
          </div>

          <!-- 结束日期 -->
          <div>
            <label for="endDate" class="block text-sm font-medium text-gray-700 mb-2">结束日期</label>
            <input
              id="endDate"
              type="date"
              v-model="query.endDate"
              @change="fetchData"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
            />
          </div>
        </div>

        <!-- 当前筛选提示 -->
        <div class="mt-4 text-sm text-gray-500">
          当前筛选：{{ filterText }}
        </div>
      </section>

      <!-- 数据加载状态 -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-purple-600"></div>
        <p class="mt-4 text-gray-600">正在加载统计数据...</p>
      </div>

      <!-- 图表区域 -->
      <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- 1. 作业提交率柱状图（每个作业的提交人数 / 总人数） -->
        <div class="bg-white rounded-3xl shadow-xl p-8">
          <h3 class="text-2xl font-bold text-gray-800 mb-6">📊 作业提交情况</h3>
          <canvas ref="submissionChart"></canvas>
        </div>

        <!-- 2. 成绩趋势折线图（时间轴上的平均分、AI平均分） -->
        <div class="bg-white rounded-3xl shadow-xl p-8">
          <h3 class="text-2xl font-bold text-gray-800 mb-6">📈 成绩趋势</h3>
          <canvas ref="scoreTrendChart"></canvas>
        </div>

        <!-- 3. 成绩分布饼图（优秀 90-100、良好 80-89、及格 60-79、不及格 <60） -->
        <div class="bg-white rounded-3xl shadow-xl p-8">
          <h3 class="text-2xl font-bold text-gray-800 mb-6">🥧 最终成绩分布</h3>
          <canvas ref="scoreDistChart"></canvas>
        </div>

        <!-- 4. 关键指标卡片组 -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
          <div class="bg-gradient-to-r from-green-500 to-green-600 rounded-3xl p-8 text-white shadow-xl">
            <div class="text-3xl font-bold">{{ stats.totalSubmissions }}</div>
            <div class="text-green-100 mt-2">总提交次数</div>
          </div>
          <div class="bg-gradient-to-r from-blue-500 to-blue-600 rounded-3xl p-8 text-white shadow-xl">
            <div class="text-3xl font-bold">{{ stats.avgFinalScore?.toFixed(1) ?? '-' }}</div>
            <div class="text-blue-100 mt-2">平均最终成绩</div>
          </div>
          <div class="bg-gradient-to-r from-purple-500 to-purple-600 rounded-3xl p-8 text-white shadow-xl">
            <div class="text-3xl font-bold">{{ stats.completionRate?.toFixed(1) ?? '0' }}%</div>
            <div class="text-purple-100 mt-2">整体提交率</div>
          </div>
        </div>
      </div>

      <!-- 无数据时 -->
      <div v-if="!loading && stats.totalSubmissions === 0" class="py-16 text-center">
        <div class="text-6xl text-gray-300 mb-4">📭</div>
        <h3 class="text-xl font-bold text-gray-800 mb-2">暂无数据</h3>
        <p class="text-gray-500">请选择课程和时间范围后查看统计数据</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Chart from 'chart.js/auto'

// 模拟数据（实际项目中替换为从后端获取）
import { classes as mockClasses } from '../../data/mockData'

const router = useRouter()

const classes = ref(mockClasses)

// 查询条件
const query = ref({
  courseId: '',
  startDate: '',
  endDate: ''
})

// 默认时间范围：最近30天
onMounted(() => {
  const end = new Date()
  const start = new Date()
  start.setDate(end.getDate() - 30)

  query.value.startDate = start.toISOString().split('T')[0]
  query.value.endDate = end.toISOString().split('T')[0]

  // 首次加载
  fetchData()
})

// 筛选提示文字
const filterText = computed(() => {
  const parts = []
  if (query.value.courseId) {
    const cls = classes.value.find(c => c.id === parseInt(query.value.courseId))
    parts.push(cls ? cls.name : '未知课程')
  } else {
    parts.push('全部课程')
  }
  parts.push(`${query.value.startDate || '...'} 至 ${query.value.endDate || '...'}`)
  return parts.join(' · ')
})

// 图表引用
const submissionChart = ref(null)
const scoreTrendChart = ref(null)
const scoreDistChart = ref(null)

// 统计数据（实际由后端返回）
const stats = ref({
  totalSubmissions: 0,
  avgFinalScore: 0,
  completionRate: 0,
  // 以下为图表专用数据
  assignmentSubmission: [],     // [{ title: '跳绳', submitted: 28, total: 30 }, ...]
  scoreTrend: [],               // [{ date: '2025-12-01', avgAi: 85, avgFinal: 88 }, ...]
  scoreDistribution: []         // [{ label: '优秀', count: 12, color: '#10b981' }, ...]
})

const loading = ref(false)

// 模拟请求后端获取数据（实际替换为 axios 请求）
const fetchData = async () => {
  if (!query.value.startDate || !query.value.endDate) return

  loading.value = true

  // TODO: 实际调用后端接口
  // const res = await axios.get('/api/teacher/dashboard', { params: query.value })

  // 模拟延迟 + 假数据
  await new Promise(resolve => setTimeout(resolve, 800))

  // 这里用假数据演示效果
  stats.value = {
    totalSubmissions: 156,
    avgFinalScore: 86.4,
    completionRate: 92.7,
    assignmentSubmission: [
      { title: '跳绳30秒', submitted: 30, total: 32 },
      { title: '立定跳远', submitted: 28, total: 32 },
      { title: '仰卧起坐', submitted: 31, total: 32 },
      { title: '引体向上', submitted: 25, total: 32 }
    ],
    scoreTrend: [
      { date: '12-01', avgAi: 82, avgFinal: 84 },
      { date: '12-08', avgAi: 85, avgFinal: 87 },
      { date: '12-15', avgAi: 88, avgFinal: 89 },
      { date: '12-22', avgAi: 86, avgFinal: 88 }
    ],
    scoreDistribution: [
      { label: '优秀 (90-100)', count: 18, color: '#10b981' },
      { label: '良好 (80-89)', count: 8, color: '#3b82f6' },
      { label: '及格 (60-79)', count: 4, color: '#f59e0b' },
      { label: '不及格 (<60)', count: 2, color: '#ef4444' }
    ]
  }

  loading.value = false
  renderCharts()
}

// 渲染所有图表
const renderCharts = () => {
  // 销毁旧图表（避免重复创建）
  if (window.subChart) window.subChart.destroy()
  if (window.trendChart) window.trendChart.destroy()
  if (window.distChart) window.distChart.destroy()

  // 1. 作业提交率柱状图
  window.subChart = new Chart(submissionChart.value, {
    type: 'bar',
    data: {
      labels: stats.value.assignmentSubmission.map(i => i.title),
      datasets: [{
        label: '已提交 / 总人数',
        data: stats.value.assignmentSubmission.map(i => (i.submitted / i.total * 100).toFixed(1)),
        backgroundColor: '#8b5cf6',
        borderRadius: 8
      }]
    },
    options: {
      responsive: true,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, max: 100, ticks: { callback: v => v + '%' } }
      }
    }
  })

  // 2. 成绩趋势折线图
  window.trendChart = new Chart(scoreTrendChart.value, {
    type: 'line',
    data: {
      labels: stats.value.scoreTrend.map(i => i.date),
      datasets: [
        {
          label: 'AI平均分',
          data: stats.value.scoreTrend.map(i => i.avgAi),
          borderColor: '#10b981',
          backgroundColor: '#10b98140',
          tension: 0.4
        },
        {
          label: '最终平均分',
          data: stats.value.scoreTrend.map(i => i.avgFinal),
          borderColor: '#3b82f6',
          backgroundColor: '#3b82f640',
          tension: 0.4
        }
      ]
    },
    options: {
      responsive: true,
      plugins: { legend: { position: 'top' } }
    }
  })

  // 3. 成绩分布饼图
  window.distChart = new Chart(scoreDistChart.value, {
    type: 'doughnut',
    data: {
      labels: stats.value.scoreDistribution.map(i => i.label),
      datasets: [{
        data: stats.value.scoreDistribution.map(i => i.count),
        backgroundColor: stats.value.scoreDistribution.map(i => i.color),
        borderWidth: 0
      }]
    },
    options: {
      responsive: true,
      plugins: {
        legend: { position: 'right' }
      }
    }
  })
}

// 导航
const goBack = () => router.push('/teacher')
const goToAssistant = () => router.push('/teacher/assistant')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}
</script>
