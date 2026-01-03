<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-10">
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

      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">✏️ {{ assignmentTitle }} - 成绩管理</h2>
        <p class="text-gray-600 mb-8">查看学生提交并进行评分</p>
      </section>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center py-20">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
      </div>

      <!-- 成绩表格 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-8">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="border-b border-gray-200">
                <th class="text-left py-4 px-6 font-medium text-gray-600">学生信息</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">提交时间</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">成绩</th>
                <th class="text-left py-4 px-6 font-medium text-gray-600">AI反馈</th>
                <th class="text-left py-4 px-6 font-medium text-gray-600">教师评价</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">操作</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="sub in studentSubmissions"
                :key="sub.studentId"
                class="border-b border-gray-100 hover:bg-gray-50 transition-colors"
              >
                <td class="py-6 px-6">
                  <div class="flex items-center gap-4">
                    <div class="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-xl font-bold">
                      {{ sub.studentName.charAt(0) }}
                    </div>
                    <div>
                      <div class="font-medium text-gray-800">{{ sub.studentName }}</div>
                      <div class="text-sm text-gray-500">学号：{{ sub.studentId }}</div>
                    </div>
                  </div>
                </td>

                <td class="py-6 px-6 text-center text-sm text-gray-600">
                  {{ formatDate(sub.createTime) }}
                </td>

                <!-- 统一成绩列（字符串处理） -->
                <td class="py-6 px-6 text-center">
                  <div v-if="editingStudentId === sub.studentId" class="flex justify-center gap-2">
                    <input
                      v-model="editingScore"
                      type="text"
                      class="w-20 px-3 py-1 border border-gray-300 rounded-lg text-center"
                      placeholder="分数"
                    />
                  </div>
                  <div v-else class="text-2xl font-black" :class="sub.score !== null ? 'text-purple-600' : 'text-gray-400'">
                    {{ sub.score ?? '-' }}
                  </div>
                </td>

                <td class="py-6 px-6">
                  <div class="max-w-xs text-sm text-gray-600 line-clamp-3" :title="sub.aiFeedback">
                    {{ sub.aiFeedback || '暂无反馈' }}
                  </div>
                </td>

                <td class="py-6 px-6">
                  <div v-if="editingStudentId === sub.studentId">
                    <input
                      v-model="editingComment"
                      type="text"
                      class="w-full max-w-xs px-3 py-1 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-400 outline-none"
                      placeholder="输入评价..."
                    />
                  </div>
                  <div v-else class="max-w-xs text-sm text-gray-600 line-clamp-3" :title="sub.teacherFeedback">
                    {{ sub.teacherFeedback || '-' }}
                  </div>
                </td>

                <td class="py-6 px-6 text-center">
                  <div class="flex gap-2 justify-center">
                    <button
                      v-if="editingStudentId === sub.studentId"
                      @click="saveGrade(sub)"
                      class="px-4 py-2 bg-green-500 text-white rounded-xl hover:bg-green-600 transition-all shadow text-sm"
                    >
                      保存
                    </button>
                    <button
                      v-else-if="sub.submitId && sub.submitId !== '-1' && sub.submitId !== '-2'"
                      @click="startEdit(sub)"
                      class="px-4 py-2 bg-blue-500 text-white rounded-xl hover:bg-blue-600 transition-all shadow text-sm"
                    >
                      修改评分
                    </button>

                    <button
                      v-if="sub.videoUrl && !sub.videoUrl.includes('test')"
                      @click="viewVideo(sub.studentId, sub.studentName)"
                      class="px-4 py-2 bg-purple-500 text-white rounded-xl hover:bg-purple-600 transition-all shadow text-sm"
                    >
                      播放视频
                    </button>
                    <span v-else class="text-gray-400 text-xs">尚未提交</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- 无提交记录 -->
        <div v-if="studentSubmissions.length === 0" class="py-16 text-center">
          <div class="text-6xl text-gray-300 mb-4">📭</div>
          <h3 class="text-xl font-bold text-gray-800 mb-2">暂无提交记录</h3>
          <p class="text-gray-500">该作业目前还没有学生提交</p>
        </div>
      </section>
    </div>

    <!-- 视频播放对话框 -->
    <VideoDialogPlayer
      v-model:visible="videoDialogVisible"
      :video-url="currentVideoUrl"
      :title="currentVideoTitle"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {apiClient, aiClient} from '../../services/axios.js'
import VideoDialogPlayer from '@/components/VideoDialogPlayer.vue'

const route = useRoute()
const router = useRouter()

const courseId = route.params.courseId
const assignmentId = route.params.assignmentId

const loading = ref(true)
const assignmentTitle = ref('加载中...')
const studentSubmissions = ref([])

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.token || ''

// 编辑状态
const editingStudentId = ref(null)
const editingScore = ref('')
const editingComment = ref('')

// 视频播放对话框控制
const videoDialogVisible = ref(false)
const currentVideoUrl = ref('')
const currentVideoTitle = ref('AI 分析视频')

// 根据学号查询学生姓名
const fetchStudentName = async (studentId) => {
  try {
    const resp = await apiClient.post('/User/get_student_info', {
      First: teacherId,
      Second: jwt,
      Third: '1',        // 教师身份
      Fourth: studentId
    })

    if (resp.data.success && resp.data.data) {
      const parts = resp.data.data.trim().replace(/\t\r$/g, '').split('\t\r').filter(Boolean)
      return parts[0] || `学生${studentId}`
    }
  } catch (err) {
    console.error(`查询学生${studentId}姓名失败:`, err)
  }
  return `学生${studentId}`
}

const fetchData = async () => {
  loading.value = true
  try {
    const [infoResp, finalResp] = await Promise.all([
      apiClient.post('/Homework/get_info_by_homework_id', {
        First: courseId,
        Second: assignmentId
      }),
      apiClient.post('/Homework/get_final_submit', {
        First: teacherId,
        Second: jwt,
        Third: courseId,
        Fourth: assignmentId
      })
    ])

    // 处理作业标题
    if (infoResp.data.success && infoResp.data.data) {
      const d = infoResp.data.data.trim().replace(/\t\r$/g, '').split('\t\r').filter(Boolean)
      assignmentTitle.value = d[0] || '未知作业'
    }

    if (!finalResp.data.success || !finalResp.data.data) {
      studentSubmissions.value = []
      loading.value = false
      return
    }

    const pairs = finalResp.data.data.split('\t\r').filter(Boolean)
    const submissions = []

    // 提取所有 studentId 和需要查询详情的 submitId
    const studentNamePromises = []
    const detailPromises = []

    for (const pair of pairs) {
      const [studentId, submitId] = pair.split('\n')

      // 收集所有查姓名的 Promise
      studentNamePromises.push(
        fetchStudentName(studentId).then(name => ({ studentId, studentName: name }))
      )

      let subInfo = {
        studentId,
        studentName: '加载中...', // 临时占位
        submitId,
        createTime: null,
        score: null,
        aiFeedback: null,
        teacherFeedback: null,
        videoUrl: null
      }

      if (submitId === '-1' || submitId === '-2') {
        submissions.push(subInfo)
        continue
      }

      // 收集所有查提交详情的 Promise
      detailPromises.push(
        apiClient.post('/Homework/get_submit_info', {
          First: '1',
          Second: teacherId,
          Third: jwt,
          Fourth: submitId
        }).then(detailResp => {
          if (detailResp.data.success && detailResp.data.data) {
            const raw = detailResp.data.data.trim().replace(/\t\r$/g, '')
            const parts = raw.split('\t\r')
            return {
              studentId,
              submitId,
              videoUrl: parts[0] || null,
              score: parts[1] || null,
              aiFeedback: parts[2] || null,
              teacherFeedback: parts[3] || null,
              createTime: parts[4] || null
            }
          }
          return { studentId, submitId } // 失败时也返回
        })
      )

      submissions.push(subInfo) // 先 push 占位
    }

    // 并行执行所有查姓名和查详情的请求
    const [nameResults, detailResults] = await Promise.all([
      Promise.all(studentNamePromises),
      Promise.all(detailPromises)
    ])

    // 创建映射
    const nameMap = {}
    nameResults.forEach(item => {
      nameMap[item.studentId] = item.studentName
    })

    const detailMap = {}
    detailResults.forEach(item => {
      if (item.submitId) {
        detailMap[item.submitId] = item
      }
    })

    // 合并数据
    studentSubmissions.value = submissions.map(sub => {
      const name = nameMap[sub.studentId] || sub.studentName
      if (sub.submitId === '-1' || sub.submitId === '-2') {
        return { ...sub, studentName: name }
      }
      const detail = detailMap[sub.submitId] || {}
      return {
        ...sub,
        studentName: name,
        videoUrl: detail.videoUrl || null,
        score: detail.score || null,
        aiFeedback: detail.aiFeedback || null,
        teacherFeedback: detail.teacherFeedback || null,
        createTime: detail.createTime || null
      }
    }).sort((a, b) => {
      if (!a.createTime) return 1
      if (!b.createTime) return -1
      return new Date(b.createTime) - new Date(a.createTime)
    })

  } catch (err) {
    console.error('加载成绩失败:', err)
    alert('加载失败，请刷新重试')
  } finally {
    loading.value = false
  }
}

const startEdit = (sub) => {
  editingStudentId.value = sub.studentId
  editingScore.value = sub.score || ''
  editingComment.value = sub.teacherFeedback || ''
}

const saveGrade = async (sub) => {
  if (!sub.submitId || sub.submitId === '-1'|| sub.submitId === '-2') {
    alert('无法评分：学生未提交')
    return
  }

  try {
    const resp = await apiClient.post('/Homework/teacher_test', {
      First: teacherId,
      Second: jwt,
      Third: courseId,
      Fourth: assignmentId,
      Fifth: sub.submitId,
      Sixth: editingScore.value,           // 字符串传入
      Seventh: editingComment.value.trim()
    })

    if (resp.data[0] === 0 || resp.data.success) {
      alert('评分保存成功！')
      editingStudentId.value = null
      await fetchData()  // 刷新获取最新 score
    } else {
      alert('保存失败')
    }
  } catch (err) {
    console.error(err)
    alert('保存失败，请检查网络')
  }
}

const viewVideo = (studentId, studentName = '') => {
  if (!studentId) return
  currentVideoUrl.value = aiClient.defaults.baseURL + '/get_processed_video?homework_id=' + assignmentId + '&student_id=' + studentId + '&download=false'
  currentVideoTitle.value = `${studentName ? studentName + ' - ' + assignmentTitle.value + ' ' : ''}AI 分析视频`
  videoDialogVisible.value = true
}


const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const goBack = () => router.push('/teacher')
const goToAssistant = () => router.push('/teacher/assistant')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

onMounted(fetchData)
</script>
