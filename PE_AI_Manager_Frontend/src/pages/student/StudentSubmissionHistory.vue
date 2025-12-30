<template>
  <div class="min-h-screen bg-gray-100">
    <div class="max-w-6xl mx-auto p-6 space-y-10">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-4xl font-bold text-gray-800 mb-2">提交历史</h2>
          <p class="text-gray-600">查看和管理您的作业提交记录</p>
        </div>
        <button @click="goBack" class="px-6 py-3 rounded-xl bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
          返回
        </button>
      </div>

      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
      </div>

      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-3xl p-6">
        <h3 class="text-xl font-bold text-red-800 mb-3">加载失败</h3>
        <p class="text-red-700 mb-4">{{ errorMessage }}</p>
        <button @click="loadSubmissions" class="px-6 py-2 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow">
          重试
        </button>
      </div>

      <div v-else-if="submissions.length === 0" class="bg-white rounded-3xl shadow-xl p-12 text-center">
        <h3 class="text-2xl font-bold text-gray-800 mb-2">暂无提交记录</h3>
        <p class="text-gray-600">您还没有提交任何作业</p>
      </div>

      <div v-else class="space-y-6">
        <div
          v-for="submission in submissions"
          :key="submission.id"
          class="bg-white rounded-3xl shadow-xl overflow-hidden hover:shadow-2xl transition-shadow"
        >
          <div class="p-6">
            <div class="flex justify-between items-start mb-4">
              <div class="flex-1">
                <h3 class="text-2xl font-bold text-gray-800 mb-2">{{ submission.title }}</h3>
                <div class="flex items-center gap-4 text-gray-600">
                  <span class="flex items-center gap-1">
                    {{ submission.courseName }}
                  </span>
                  <span>•</span>
                  <span class="flex items-center gap-1">
                    {{ formatDate(submission.CREATE_TIME) }}
                  </span>
                </div>
              </div>
              <div class="text-right">
                <div class="text-3xl font-bold mb-1" :class="submission.score !== null ? 'text-green-600' : 'text-orange-500'">
                  {{ submission.score !== null ? submission.score + ' 分' : '待批改' }}
                </div>
                <span
                  class="px-3 py-1 rounded-full text-sm font-medium"
                  :class="submission.score !== null ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'"
                >
                  {{ submission.score !== null ? '已批改' : '待批改' }}
                </span>
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h4 class="text-lg font-semibold text-gray-700 mb-3">AI 分析视频</h4>
                <div v-if="submission.video_url" class="relative aspect-video bg-black rounded-xl overflow-hidden shadow-lg">
                  <video :src="submission.video_url" controls class="w-full h-full object-contain">
                    您的浏览器不支持视频播放。
                  </video>
                </div>
                <div v-else class="aspect-video bg-gray-100 rounded-xl flex items-center justify-center border-2 border-dashed border-gray-300">
                  <p class="text-gray-500">暂无AI分析视频</p>
                </div>
              </div>

              <div class="space-y-4">
                <div>
                  <h4 class="text-lg font-semibold text-gray-700 mb-2">AI 智能评价</h4>
                  <div v-if="submission.AI_feedback" class="bg-indigo-50 rounded-xl p-4 border border-indigo-200 max-h-32 overflow-y-auto">
                    <p class="text-indigo-900 text-sm whitespace-pre-wrap">{{ submission.AI_feedback }}</p>
                  </div>
                  <div v-else class="text-center py-4 text-gray-500 bg-gray-50 rounded-xl">
                    AI 反馈暂未生成
                  </div>
                </div>

                <div>
                  <h4 class="text-lg font-semibold text-gray-700 mb-2">教师评语</h4>
                  <div v-if="submission.teacher_feedback" class="bg-blue-50 rounded-xl p-4 border border-blue-200 max-h-32 overflow-y-auto">
                    <p class="text-blue-900 text-sm whitespace-pre-wrap">{{ submission.teacher_feedback }}</p>
                  </div>
                  <div v-else class="text-center py-4 text-gray-500 bg-gray-50 rounded-xl">
                    教师尚未留下评语
                  </div>
                </div>
              </div>
            </div>

            <div class="flex justify-end gap-3 mt-6">
              <button
                @click="viewDetail(submission)"
                class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg"
              >
                查看详情
              </button>
              <button
                v-if="submission.video_url"
                @click="deleteVideo(submission)"
                class="px-6 py-2 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow-lg"
              >
                删除视频
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '../../services/axios.js'

const router = useRouter()
const route = useRoute()

const submissions = ref([])
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const studentId = currentUser.id || ''
const jwt = currentUser.token || ''

const courseId = route.params.courseId || ''
const assignmentId = route.params.assignmentId || ''

const loadSubmissions = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    if (!studentId || !jwt) {
      throw new Error('未找到用户信息，请重新登录')
    }

    let targetHomeworkId = null
    let courseName = '未命名课程'

    // 如果有指定作业ID，只获取该作业的提交记录
    if (assignmentId) {
      targetHomeworkId = assignmentId

      // 获取课程名称
      try {
        const courseDetailResponse = await apiClient.post('/Course/get_info_by_course_id', {
          first: courseId,
          second: jwt
        })

        if (courseDetailResponse.data.success && courseDetailResponse.data.data) {
          courseName = courseDetailResponse.data.data.name || '未命名课程'
        }
      } catch (err) {
        console.error('获取课程信息失败:', err)
      }
    }

    // 调用 get_submit_id_by_student 获取提交ID列表
    const submitIdResponse = await apiClient.post('/Homework/get_submit_id_by_student', {
      first: '0',
      second: studentId,
      third: jwt,
      fourth: targetHomeworkId || '1',
      fifth: studentId
    })

    if (!submitIdResponse.data.success || !submitIdResponse.data.data || submitIdResponse.data.data.trim() === '' || submitIdResponse.data.data === 'NULL') {
      submissions.value = []
      loading.value = false
      return
    }

    const submitIdList = submitIdResponse.data.data.split('\t\r').filter(id => id.trim())

    const allSubmissions = []

    for (const submitId of submitIdList) {
      try {
        // 获取提交详细信息
        const submitInfoResponse = await apiClient.post('/Homework/get_submit_info', {
          first: '0',
          second: studentId,
          third: jwt,
          fourth: submitId.trim()
        })

        console.log('get_submit_info:', submitInfoResponse.data)
        if (submitInfoResponse.data.success && submitInfoResponse.data.data) {
          // 解析提交数据字符串，格式为: video_url\t\r score\t\r AI_feedback\t\r teacher_feedback\t\r CREATE_TIME
          const submitDataArray = submitInfoResponse.data.data.split('\t\r');

          console.log('提交数据:', submitDataArray)

          // 安全地解析提交数据，处理字段可能为空的情况
          const videoUrl = submitDataArray[0] || null;
          const score = submitDataArray[1] ? parseFloat(submitDataArray[1]) : null;
          const aiFeedback = submitDataArray[2] || '';
          const teacherFeedback = submitDataArray[3] || '';
          const createTime = submitDataArray[4] || '';

          // 获取作业详情
          let homeworkTitle = `作业 ${submitId.trim()}`
          let submitCourseId = courseId
          let submitCourseName = courseName

          try {
            const homeworkDetailResponse = await apiClient.post('/Homework/get_info_by_homework_id', {
              first: courseId || '',
              second: targetHomeworkId
            })

            if (homeworkDetailResponse.data[0] >= 0) {
              homeworkTitle = homeworkDetailResponse.data[0] || `作业 ${homeworkId}`
            }
          } catch (err) {
            console.error(`获取作业 ${targetHomeworkId } 详情失败:`, err)
          }

          allSubmissions.push({
            id: submitId.trim(),
            courseId: submitCourseId || '',
            courseName: submitCourseName,
            title: homeworkTitle,
            video_url: videoUrl,
            score: score,
            AI_feedback: aiFeedback,
            teacher_feedback: teacherFeedback,
            CREATE_TIME: createTime
          })
        } else {
          console.log(`提交 ${submitId} 信息获取失败或为空`);
          continue;
        }
      } catch (err) {
        console.error(`获取提交 ${submitId} 信息失败:`, err)
      }
    }

    // 按submitId数值大小排序
    const sortedSubmissions = allSubmissions.sort((a, b) => parseInt(a.id) - parseInt(b.id))

    // 根据排序后的顺序更新提交标题为"第1次提交"、"第2次提交"等
    sortedSubmissions.forEach((submission, index) => {
      submission.title = `第${index + 1}次提交`
    })

    submissions.value = sortedSubmissions
  } catch (err) {
    error.value = true
    errorMessage.value = err.message || '加载提交历史失败'
    console.error('加载提交历史失败:', err)
  } finally {
    loading.value = false
  }
}

const viewDetail = (submission) => {
  router.push(`/student/course/${submission.courseId}/submission/${submission.id}`)
}

const deleteVideo = async (submission) => {
  if (!confirm(`确定要删除"${submission.title}"的AI分析视频吗？此操作不可恢复。`)) {
    return
  }

  try {
    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://118.25.145.4:8000'
    const url = `${baseUrl}/delete_homework?homework_id=${encodeURIComponent(submission.id)}`

    const response = await fetch(url, {
      method: 'DELETE'
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.detail || `HTTP ${response.status}: ${response.statusText}`)
    }

    const result = await response.json()
    if (result.status === 'success') {
      alert('视频删除成功')
      submission.video_url = null
    } else {
      throw new Error(result.message || '删除视频失败')
    }
  } catch (err) {
    alert(`删除视频失败: ${err.message}`)
    console.error('删除视频失败:', err)
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const goBack = () => {
  if (courseId && assignmentId) {
    router.push(`/student/course/${courseId}/assignments/${assignmentId}`)
  } else if (courseId) {
    router.push(`/student/course/${courseId}`)
  } else {
    router.push('/student/assignments')
  }
}

onMounted(() => {
  loadSubmissions()
})
</script>
