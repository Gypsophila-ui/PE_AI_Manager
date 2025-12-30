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

      <!-- 筛选条件 + 导出按钮 -->
      <section class="bg-white rounded-3xl shadow-xl p-8">
        <div class="flex justify-between items-end mb-6">
          <h3 class="text-xl font-bold text-gray-800">筛选条件</h3>
          <button
            @click="exportToExcel"
            :disabled="loading || stats.totalSubmissions === 0"
            class="px-6 py-3 bg-green-600 text-white rounded-xl hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-all shadow-lg flex items-center gap-2"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            导出为 Excel
          </button>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">选择课程</label>
            <select v-model="query.courseId" @change="fetchData" class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm">
              <option value="">全部课程</option>
              <option v-for="cls in courses" :key="cls.id" :value="cls.id">{{ cls.name }}</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">开始日期</label>
            <input type="date" v-model="query.startDate" @change="fetchData" class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm"/>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">结束日期</label>
            <input type="date" v-model="query.endDate" @change="fetchData" class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm"/>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">运动类型</label>
            <select v-model="query.aiType" @change="fetchData" class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm">
              <option value="">全部</option>
              <option value="squat">深蹲</option>
              <option value="pushup">俯卧撑</option>
              <option value="deadlift">硬拉</option>
            </select>
          </div>
        </div>

        <div class="mt-4 text-sm text-gray-500">
          当前筛选：{{ filterText }}
        </div>
      </section>

      <!-- 加载中 -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-purple-600"></div>
        <p class="mt-4 text-gray-600">正在加载统计数据...</p>
      </div>

      <!-- 有数据 -->
      <div v-else-if="stats.totalSubmissions > 0">
        <!-- 关键指标卡片（4个） -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-10">
          <div class="bg-gradient-to-r from-green-500 to-green-600 rounded-3xl p-8 text-white shadow-xl">
            <div class="text-3xl font-bold">{{ stats.totalSubmissions }}</div>
            <div class="text-green-100 mt-2">总提交次数</div>
          </div>
          <div class="bg-gradient-to-r from-blue-500 to-blue-600 rounded-3xl p-8 text-white shadow-xl">
            <div class="text-3xl font-bold">{{ stats.avgFinalScore !== null ? stats.avgFinalScore.toFixed(1) : '-' }}</div>
            <div class="text-blue-100 mt-2">平均成绩</div>
          </div>
          <div class="bg-gradient-to-r from-purple-500 to-purple-600 rounded-3xl p-8 text-white shadow-xl">
            <div class="text-3xl font-bold">{{ stats.completionRate.toFixed(1) }}%</div>
            <div class="text-purple-100 mt-2">整体提交率</div>
          </div>
          <div class="bg-gradient-to-r from-orange-500 to-orange-600 rounded-3xl p-8 text-white shadow-xl">
            <div class="text-3xl font-bold">{{ stats.excellentRate.toFixed(1) }}%</div>
            <div class="text-orange-100 mt-2">优秀率 (≥90分)</div>
          </div>
        </div>

        <!-- 图表区域 -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <!-- 作业提交情况 -->
          <div class="bg-white rounded-3xl shadow-xl p-8 flex flex-col">
            <h3 class="text-2xl font-bold text-gray-800 mb-6">📊 作业提交情况</h3>
            <div class="flex-1 flex items-center justify-center">
              <canvas ref="submissionChart" class="max-h-96 w-full"></canvas>
            </div>
          </div>

          <!-- 各作业平均成绩趋势 -->
          <div class="bg-white rounded-3xl shadow-xl p-8 flex flex-col">
            <h3 class="text-2xl font-bold text-gray-800 mb-6">📈 各作业平均成绩趋势</h3>
            <div class="flex-1 flex items-center justify-center">
              <canvas ref="scoreTrendChart" class="max-h-96 w-full"></canvas>
            </div>
          </div>

          <!-- 各作业优秀率对比 -->
          <div class="bg-white rounded-3xl shadow-xl p-8 flex flex-col">
            <h3 class="text-2xl font-bold text-gray-800 mb-6">🏆 各作业优秀率对比</h3>
            <div class="flex-1 flex items-center justify-center">
              <canvas ref="excellentRateChart" class="max-h-96 w-full"></canvas>
            </div>
          </div>

          <!-- 作业成绩分布 -->
          <div class="bg-white rounded-3xl shadow-xl p-8 flex flex-col">
            <h3 class="text-2xl font-bold text-gray-800 mb-6">🥧 提交成绩分布</h3>
            <div class="flex-1 flex items-center justify-center">
              <canvas ref="scoreDistChart" class="max-h-96 w-full"></canvas>
            </div>
          </div>
        </div>
      </div>

      <!-- 无数据 -->
      <div v-else class="py-16 text-center">
        <div class="text-6xl text-gray-300 mb-4">📭</div>
        <h3 class="text-xl font-bold text-gray-800 mb-2">暂无数据</h3>
        <p class="text-gray-500">当前筛选条件下没有作业提交记录</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import Chart from 'chart.js/auto'
import * as XLSX from 'xlsx'
import { saveAs } from 'file-saver'
import apiClient from '../../services/axios.js'

const router = useRouter()

const courses = ref([])
const submissions = ref([])
const loading = ref(true)

const query = ref({
  courseId: '',
  startDate: '',
  endDate: '',
  aiType: ''
})

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.token || ''

const aiTypeMap = {
  squat: '深蹲',
  pushup: '俯卧撑',
  deadlift: '硬拉'
}

// 数据加载
const loadData = async () => {
  loading.value = true
  courses.value = []
  submissions.value = []

  try {
    const courseResp = await apiClient.post('/Course/get_course_id_by_teacher', { First: teacherId, Second: jwt })
    if (!courseResp.data.success || !courseResp.data.data) return

    const courseIds = courseResp.data.data.split('\t\r').filter(Boolean)

    const coursePromises = courseIds.map(id => apiClient.post('/Course/get_info_by_course_id', { First: id }))
    const courseResps = await Promise.all(coursePromises)

    courses.value = courseResps
      .map((resp, i) => {
        if (!resp?.data?.data) return null
        const parts = resp.data.data.trim().replace(/\t\r$/g, '').split('\t\r').filter(Boolean)
        return parts.length > 1 ? { id: courseIds[i], name: parts[1] } : null
      })
      .filter(Boolean)

    for (const courseId of courseIds) {
      const hwResp = await apiClient.post('/Homework/get_homework_id_by_course', {
        First: '1', Second: teacherId, Third: jwt, Fourth: courseId
      })
      if (!hwResp.data.success || !hwResp.data.data) continue
      const hwIds = hwResp.data.data.split('\t\r').filter(Boolean)

      const studentResp = await apiClient.post('/Course_student/get_student_id_by_course', {
        First: teacherId, Second: jwt, Third: courseId
      })
      const totalStudents = studentResp.data.success && studentResp.data.data
        ? studentResp.data.data.split('\t\r').filter(Boolean).length
        : 0

      for (const hwId of hwIds) {
        const infoResp = await apiClient.post('/Homework/get_info_by_homework_id', { First: courseId, Second: hwId })
        if (!infoResp.data.success) continue
        const infoParts = infoResp.data.data.split('\t\r').filter(Boolean)
        const title = infoParts[0] || '未命名作业'

        const aiResp = await apiClient.post('/Homework/get_AI_type', { First: hwId })
        const rawAiType = aiResp.data.data.split('\t\r')[0] || 'squat'

        const submitResp = await apiClient.post('/Homework/get_final_submit', {
          First: teacherId, Second: jwt, Third: courseId, Fourth: hwId
        })
        if (!submitResp.data.success || !submitResp.data.data) continue

        const pairs = submitResp.data.data.split('\t\r').filter(Boolean)
        for (const pair of pairs) {
          const [studentId, submitId] = pair.split('\n')
          if (submitId === '-1' || submitId === '-2') continue

          const detailResp = await apiClient.post('/Homework/get_submit_info', {
            First: '1', Second: teacherId, Third: jwt, Fourth: submitId
          })

          if (detailResp.data.success && detailResp.data.data) {
            const raw = detailResp.data.data.trim().replace(/\t\r$/g, '')
            let detail = raw.split('\t\r')
            const score = parseInt(detail[1]) || 0
            const submissionTimeStr = detail[4] || ''

            const submissionTime = parseDate(submissionTimeStr)

            if (submissionTime) {
              submissions.value.push({
                courseId,
                homeworkId: hwId,
                homeworkTitle: title,
                aiType: rawAiType,
                studentId,
                score,
                submissionTime,
                totalStudents
              })
            }
          }
        }
      }
    }
  } catch (err) {
    console.error('加载失败', err)
  } finally {
    loading.value = false
  }
}

const parseDate = (str) => {
  if (!str) return null
  const cleaned = str.trim()
  const isoStr = cleaned.replace(/\//g, '-').replace(' ', 'T')
  const date = new Date(isoStr)
  return isNaN(date.getTime()) ? null : date
}

// 筛选与统计
const filteredSubmissions = computed(() => {
  return submissions.value.filter(item => {
    const matchCourse = !query.value.courseId || item.courseId === query.value.courseId
    const matchAiType = !query.value.aiType || item.aiType === query.value.aiType
    const subTime = item.submissionTime
    const start = query.value.startDate ? new Date(query.value.startDate) : null
    const end = query.value.endDate ? new Date(query.value.endDate + 'T23:59:59') : null
    const matchDate = (!start || subTime >= start) && (!end || subTime <= end)
    return matchCourse && matchAiType && matchDate
  })
})

const stats = computed(() => {
  const list = filteredSubmissions.value
  if (list.length === 0) {
    return {
      totalSubmissions: 0,
      avgFinalScore: null,
      completionRate: 0,
      excellentRate: 0,
      assignmentSubmission: [],
      assignmentExcellent: [],
      scoreTrend: [],
      scoreDistribution: []
    }
  }

  // 按作业分组
  const hwMap = new Map()
  list.forEach(s => {
    const key = `${s.courseId}-${s.homeworkId}`
    if (!hwMap.has(key)) {
      hwMap.set(key, {
        title: s.homeworkTitle,
        submitted: 0,
        totalStudents: s.totalStudents,
        scores: [],
        excellentCount: 0
      })
    }
    const hw = hwMap.get(key)
    hw.submitted++
    if (s.score > 0) {
      hw.scores.push(s.score)
      if (s.score >= 90) hw.excellentCount++
    }
  })

  const assignmentSubmission = Array.from(hwMap.values())
    .map(hw => ({
      title: hw.title,
      submitted: hw.submitted,
      total: hw.totalStudents,
      avgScore: hw.scores.length > 0 ? Number((hw.scores.reduce((a,b)=>a+b,0)/hw.scores.length).toFixed(1)) : null,
      excellentRate: hw.submitted > 0 ? Number((hw.excellentCount / hw.submitted * 100).toFixed(1)) : 0
    }))
    .sort((a, b) => a.title.localeCompare(b.title)) // 可按需排序

  const totalSubmissions = list.length
  const totalStudentsAll = assignmentSubmission.reduce((sum, a) => sum + a.total, 0)
  const completionRate = totalStudentsAll > 0 ? Number((totalSubmissions / totalStudentsAll * 100).toFixed(1)) : 0

  const validScores = list.filter(s => s.score > 0).map(s => s.score)
  const avgFinalScore = validScores.length > 0 ? Number((validScores.reduce((a,b)=>a+b,0)/validScores.length).toFixed(1)) : null

  // 整体优秀率（作业平均分 ≥90 的比例）
  const excellentAssignments = assignmentSubmission.filter(a => a.avgScore >= 90).length
  const excellentRate = assignmentSubmission.length > 0 ? Number((excellentAssignments / assignmentSubmission.length * 100).toFixed(1)) : 0

  // 各作业优秀率（用于第四个图表）
  const assignmentExcellent = assignmentSubmission.map(a => ({
    title: a.title,
    excellentRate: a.excellentRate
  }))

  // 趋势图：各作业平均成绩
  const scoreTrend = assignmentSubmission.map(hw => ({
    date: hw.title,
    avgScore: hw.avgScore || 0
  }))

  const scoreDistribution = [
    { label: '优秀 (90-100)', count: list.filter(s => s.score >= 90).length, color: '#10b981' },
    { label: '良好 (80-89)', count: list.filter(s => s.score >= 80 && s.score < 90).length, color: '#3b82f6' },
    { label: '及格 (60-79)', count: list.filter(s => s.score >= 60 && s.score < 80).length, color: '#f59e0b' },
    { label: '不及格 (<60)', count: list.filter(s => s.score < 60).length, color: '#ef4444' }
  ]

  return {
    totalSubmissions,
    avgFinalScore,
    completionRate,
    excellentRate,
    assignmentSubmission,
    assignmentExcellent,
    scoreTrend,
    scoreDistribution
  }
})

const filterText = computed(() => {
  const parts = []
  if (query.value.courseId) {
    const c = courses.value.find(c => c.id === query.value.courseId)
    parts.push(c ? c.name : '未知课程')
  } else {
    parts.push('全部课程')
  }
  if (query.value.aiType) parts.push(aiTypeMap[query.value.aiType])
  parts.push(`${query.value.startDate || '开始'} 至 ${query.value.endDate || '现在'}`)
  return parts.join(' · ')
})

// 图表引用
const submissionChart = ref(null)
const scoreTrendChart = ref(null)
const excellentRateChart = ref(null)
const scoreDistChart = ref(null)

let subChart = null
let trendChart = null
let excellentChart = null
let distChart = null

const renderCharts = () => {
  [subChart, trendChart, excellentChart, distChart].forEach(c => c?.destroy())

  const s = stats.value

  // 1. 提交率
  if (submissionChart.value && s.assignmentSubmission.length > 0) {
    subChart = new Chart(submissionChart.value, {
      type: 'bar',
      data: {
        labels: s.assignmentSubmission.map(i => i.title),
        datasets: [{
          label: '提交率 (%)',
          data: s.assignmentSubmission.map(i => i.total > 0 ? (i.submitted / i.total * 100).toFixed(1) : 0),
          backgroundColor: '#8b5cf6',
          borderRadius: 8
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true, max: 100, ticks: { callback: v => v + '%' } } }
      }
    })
  }

  // 2. 平均成绩趋势
  if (scoreTrendChart.value && s.scoreTrend.length > 0) {
    trendChart = new Chart(scoreTrendChart.value, {
      type: 'line',
      data: {
        labels: s.scoreTrend.map(i => i.date),
        datasets: [{
          label: '作业平均成绩',
          data: s.scoreTrend.map(i => i.avgScore),
          borderColor: '#3b82f6',
          backgroundColor: '#3b82f640',
          tension: 0.4,
          fill: true
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { position: 'top' } },
        scales: { y: { beginAtZero: true, max: 100, ticks: { callback: v => v + '分' } } }
      }
    })
  }

  // 3. 新增：各作业优秀率（水平柱状图）
  if (excellentRateChart.value && s.assignmentExcellent.length > 0) {
    excellentChart = new Chart(excellentRateChart.value, {
      type: 'bar',
      data: {
        labels: s.assignmentExcellent.map(i => i.title),
        datasets: [{
          label: '优秀率 (%)',
          data: s.assignmentExcellent.map(i => i.excellentRate),
          backgroundColor: '#f97316',
          borderRadius: 8
        }]
      },
      options: {
        indexAxis: 'y',
        responsive: true,
        plugins: { legend: { display: false } },
        scales: { x: { beginAtZero: true, max: 100, ticks: { callback: v => v + '%' } } }
      }
    })
  }

  // 4. 成绩分布
  if (scoreDistChart.value && s.scoreDistribution.some(d => d.count > 0)) {
    distChart = new Chart(scoreDistChart.value, {
      type: 'doughnut',
      data: {
        labels: s.scoreDistribution.map(d => d.label),
        datasets: [{
          data: s.scoreDistribution.map(d => d.count),
          backgroundColor: s.scoreDistribution.map(d => d.color),
          borderWidth: 0
        }]
      },
      options: { responsive: true, plugins: { legend: { position: 'right' } } }
    })
  }
}

const fetchData = async () => {
  await loadData()
  await nextTick()
  renderCharts()
}

// 导出 Excel
const exportToExcel = () => {
  const s = stats.value
  if (s.assignmentSubmission.length === 0) {
    alert('暂无数据可导出')
    return
  }

  const tableData = s.assignmentSubmission.map(item => ({
    '作业名称': item.title,
    '应交人数': item.total,
    '已交人数': item.submitted,
    '提交率 (%)': item.total > 0 ? (item.submitted / item.total * 100).toFixed(1) : '0',
    '平均成绩': item.avgScore !== null ? item.avgScore.toFixed(1) : '-',
    '优秀率 (%)': s.assignmentExcellent.find(e => e.title === item.title)?.excellentRate || '0'
  }))

  tableData.push({
    '作业名称': '汇总',
    '应交人数': s.assignmentSubmission.reduce((sum, i) => sum + i.total, 0),
    '已交人数': s.totalSubmissions,
    '提交率 (%)': s.completionRate.toFixed(1),
    '平均成绩': s.avgFinalScore !== null ? s.avgFinalScore.toFixed(1) : '-',
    '优秀率 (%)': s.excellentRate.toFixed(1)
  })

  const ws = XLSX.utils.json_to_sheet(tableData)
  ws['!cols'] = [{ wch: 25 }, { wch: 12 }, { wch: 12 }, { wch: 14 }, { wch: 12 }, { wch: 14 }]

  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, '作业统计')

  const courseName = query.value.courseId ? courses.value.find(c => c.id === query.value.courseId)?.name || '未知课程' : '全部课程'
  const fileName = `体育作业统计_${courseName}_${query.value.startDate || '全部'}_至_${query.value.endDate || '全部'}.xlsx`

  const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' })
  const blob = new Blob([wbout], { type: 'application/octet-stream' })
  saveAs(blob, fileName)
}

// 导航
const goBack = () => router.push('/teacher')
const goToAssistant = () => router.push('/teacher/assistant')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

// 初始化
onMounted(() => {
  const end = new Date()
  const start = new Date()
  start.setDate(end.getDate() - 30)

  query.value.startDate = start.toISOString().split('T')[0]
  query.value.endDate = end.toISOString().split('T')[0]

  fetchData()
})
</script>
