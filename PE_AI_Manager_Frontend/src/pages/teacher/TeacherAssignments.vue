<template>
  <div class="min-h-screen bg-white">
    <div class="max-w-7xl mx-auto p-6 space-y-8">
      <div class="flex justify-between items-center">
        <h2 class="text-2xl font-bold text-gray-800">📝 作业统计</h2>
      </div>

      <!-- 筛选条件 -->
      <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
        <div class="flex flex-col md:flex-row gap-3">
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-500 mb-1">选择班级</label>
            <select v-model="selectedClass" class="w-full px-3 py-2 rounded-lg border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400 transition-all text-sm">
              <option value="all">所有班级</option>
              <option v-for="cls in courses" :key="cls.id" :value="cls.id">{{ cls.name }}</option>
            </select>
          </div>
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-500 mb-1">选择运动类型</label>
            <select v-model="selectedAiType" class="w-full px-3 py-2 rounded-lg border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400 transition-all text-sm">
              <option value="all">所有类型</option>
              <option value="squat">深蹲</option>
              <option value="pushup">俯卧撑</option>
              <option value="deadlift">硬拉</option>
            </select>
          </div>
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-500 mb-1">作业状态</label>
            <select v-model="selectedStatus" class="w-full px-3 py-2 rounded-lg border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400 transition-all text-sm">
              <option value="all">所有状态</option>
              <option value="进行中">进行中</option>
              <option value="已截止">已截止</option>
            </select>
          </div>
        </div>
      </div>

      <!-- 统计卡片 -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
          <div class="text-xs font-medium text-gray-500 mb-1">总作业数</div>
          <div class="text-2xl font-bold text-gray-800">{{ totalAssignmentsCount }}</div>
        </div>
        <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
          <div class="text-xs font-medium text-gray-500 mb-1">总提交人次</div>
          <div class="text-2xl font-bold text-green-600">{{ totalSubmittedCount }}</div>
        </div>
        <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
          <div class="text-xs font-medium text-gray-500 mb-1">整体平均分</div>
          <div class="text-2xl font-bold text-purple-600">{{ overallAvgScore }}</div>
        </div>
      </div>

      <!-- 作业详情表格 -->
      <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
        <h3 class="text-lg font-semibold text-gray-800 mb-3">📋 作业详情</h3>
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="border-b">
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">班级</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">作业标题</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">运动类型</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">截止时间</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">提交情况</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">平均分</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">状态</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="assignment in filteredAssignments" :key="assignment.id" class="border-b hover:bg-gray-50">
                <td class="py-2 px-3 text-sm text-gray-700">{{ getCourseName(assignment.courseId) }}</td>
                <td class="py-2 px-3 text-sm font-medium text-gray-800">{{ assignment.title }}</td>
                <td class="py-2 px-3 text-sm">
                  <span class="px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-700">
                    {{ assignment.aiTypeDisplay }}
                  </span>
                </td>
                <td class="py-2 px-3 text-sm text-gray-700">{{ formatDate(assignment.deadline) }}</td>
                <td class="py-2 px-3 text-sm text-gray-700">
                  {{ assignment.submittedCount }} / {{ assignment.totalStudents }}
                </td>
                <td class="py-2 px-3 text-sm font-semibold text-gray-800">
                  {{ assignment.avgScore || '-' }}
                </td>
                <td class="py-2 px-3">
                  <span :class="['px-2 py-1 rounded-full text-xs font-medium',
                                 assignment.status === '进行中' ? 'bg-blue-100 text-blue-700' : 'bg-red-100 text-red-700']">
                    {{ assignment.status }}
                  </span>
                </td>
                <td class="py-2 px-3">
                  <button @click="viewAssignmentDetails(assignment.courseId, assignment.id)"
                          class="px-3 py-1 rounded-md bg-blue-500 text-white text-xs hover:bg-blue-600 transition-all">
                    查看详情
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="filteredAssignments.length === 0" class="text-center py-6 text-gray-500">
          暂无符合条件的作业
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '../../services/axios.js'

const router = useRouter()

const courses = ref([])
const assignments = ref([])
const loading = ref(true)
const errorMsg = ref('')

const selectedClass = ref('all')
const selectedAiType = ref('all')
const selectedStatus = ref('all')

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.token || 'valid_teacher_jwt'

// AI 类型中英文映射
const aiTypeMap = {
  squat: '深蹲',
  pushup: '俯卧撑',
  deadlift: '硬拉'
}

const loadData = async () => {
  loading.value = true
  errorMsg.value = ''

  try {
    // 1. 获取教师课程
    const courseResp = await apiClient.post('/Course/get_course_id_by_teacher', {
      First: teacherId,
      Second: jwt
    })

    if (!courseResp.data.success) {
      errorMsg.value = '获取课程失败'
      loading.value = false
      return
    }

    const courseIds = courseResp.data.data.split('\t\r').filter(Boolean)

    const coursePromises = courseIds.map(id => apiClient.post('/Course/get_info_by_course_id', { First: id }))
    const courseResps = await Promise.all(coursePromises)
    const processedResponses = courseResps.map(resp => {
      if (!resp?.data?.data) return [];

      const data = resp.data.data.trim().replace(/\t\r$/g, '');
      return data.split(/\t\r/).filter(item => item !== '');
    });

    courses.value = processedResponses
      .filter(r => r[0] >= 0)
      .map((r, i) => ({ id: courseIds[i], name: r[1] }))

    // 2. 获取所有作业 + AI类型 + 提交统计
    assignments.value = []

    for (const courseId of courseIds) {
      // 获取作业列表
      const hwResp = await apiClient.post('/Homework/get_homework_id_by_course', {
        First: '1',
        Second: teacherId,
        Third: jwt,
        Fourth: courseId
      })

      if (!hwResp.data.success || !hwResp.data.data) continue

      const hwIds = hwResp.data.data.split('\t\r').filter(Boolean)

      // 获取学生总数
      const studentResp = await apiClient.post('/Course_student/get_student_id_by_course', {
        First: teacherId,
        Second: jwt,
        Third: courseId
      })
      const totalStudents = studentResp.data.success && studentResp.data.data
        ? studentResp.data.data.split('\t\r').filter(Boolean).length
        : 0

      for (const hwId of hwIds) {
        // 获取作业基本信息
        const infoResp = await apiClient.post('/Homework/get_info_by_homework_id', { First: courseId, Second: hwId })
        if (!infoResp.data.success) continue

        const d = infoResp.data.data.split('\t\r').filter(Boolean)
        const deadline = new Date(d[2])
        const status = deadline > new Date() ? '进行中' : '已截止'

        // 获取 AI 类型
        const aiResp = await apiClient.post('/Homework/get_AI_type', { First: hwId })
        let rawAiType = 'squat'
        if (aiResp.data.success) {
          const config = aiResp.data.data.trim()
          const parts = config.split('\t\r')
          rawAiType = parts[0] || 'squat'

        }

        // 获取所有学生提交情况
        const submitResp = await apiClient.post('/Homework/get_final_submit', {
          First: teacherId,
          Second: jwt,
          Third: courseId,
          Fourth: hwId
        })

        let submittedCount = 0
        let totalScore = 0
        let scoreCount = 0

        if (submitResp.data.success && submitResp.data.data) {
          const pairs = submitResp.data.data.split('\t\r').filter(Boolean)

          for (const pair of pairs) {
            const [studentId, submitId] = pair.split('\n')
            if (submitId === '-1' || submitId === '-2') continue  // 未提交或未评分

            // 获取提交详情
            const detailResp = await apiClient.post('/Homework/get_submit_info', {
              First: '1',          // 教师身份
              Second: teacherId,
              Third: jwt,
              Fourth: submitId
            })

            if (detailResp.data.success && detailResp.data.data) {
              const detail = detailResp.data.data.split('\t\r').filter(Boolean)
              const score = parseInt(detail[1]) || 0
              submittedCount++
              if (score > 0) {
                totalScore += score
                scoreCount++
              }
            }
          }
        }

        const avgScore = scoreCount > 0 ? (totalScore / scoreCount).toFixed(1) : null

        assignments.value.push({
          id: hwId,
          courseId,
          title: d[0],
          description: d[1],
          deadline: d[2],
          create_time: d[3],
          aiType: rawAiType,
          aiTypeDisplay: aiTypeMap[rawAiType] || '标准动作',
          status,
          submittedCount,
          totalStudents,
          avgScore
        })
      }
    }

  } catch (err) {
    errorMsg.value = '加载失败'
    console.error(err)
  } finally {
    loading.value = false
  }
}

const filteredAssignments = computed(() => {
  return assignments.value.filter(item => {
    const matchClass = selectedClass.value === 'all' || item.courseId === selectedClass.value
    const matchAiType = selectedAiType.value === 'all' || item.aiType === selectedAiType.value
    const matchStatus = selectedStatus.value === 'all' || item.status === selectedStatus.value
    return matchClass && matchAiType && matchStatus
  })
})

const totalAssignmentsCount = computed(() => filteredAssignments.value.length)
const totalSubmittedCount = computed(() =>
  filteredAssignments.value.reduce((sum, a) => sum + a.submittedCount, 0)
)
const overallAvgScore = computed(() => {
  const validScores = filteredAssignments.value
    .filter(a => a.avgScore !== null)
    .map(a => parseFloat(a.avgScore))

  if (validScores.length === 0) return '-'
  const avg = validScores.reduce((sum, s) => sum + s, 0) / validScores.length
  return avg.toFixed(1)
})

const getCourseName = (courseId) => {
  const c = courses.value.find(item => item.id === courseId)
  return c ? c.name : '未知班级'
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' })
}

const viewAssignmentDetails = (courseId, homeworkId) => {
  router.push(`/teacher/course/${courseId}/assignment/${homeworkId}`)
}

onMounted(loadData)
</script>
