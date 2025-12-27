<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-10">
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
        <p class="text-gray-600">查看和管理该作业的学生提交成绩</p>
      </section>

      <section class="bg-white rounded-3xl shadow-xl p-8">
        <div class="overflow-x-auto">
          <table class="w-full min-w-full">
            <thead>
              <tr class="border-b border-gray-200">
                <th class="text-left py-4 px-6 font-medium text-gray-600">学生信息</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">提交时间</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">AI评分</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">教师评分</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">最终成绩</th>
                <th class="text-left py-4 px-6 font-medium text-gray-600">AI反馈</th>
                <th class="text-left py-4 px-6 font-medium text-gray-600">教师评价</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">操作</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="submission in studentLatestSubmissions"
                :key="submission.studentId"
                class="border-b border-gray-100 hover:bg-gray-50 transition-colors"
              >
                <td class="py-6 px-6">
                  <div class="flex items-center gap-4">
                    <div class="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-xl font-bold">
                      {{ submission.studentName.charAt(0) }}
                    </div>
                    <div>
                      <div class="font-medium text-gray-800">{{ submission.studentName }}</div>
                      <div class="text-sm text-gray-500">班级：{{ submission.className }}</div>
                    </div>
                  </div>
                </td>

                <td class="py-6 px-6 text-center text-sm text-gray-600">
                  {{ formatDate(submission.submissionTime) }}
                </td>

                <td class="py-6 px-6 text-center">
                  <div v-if="submission.aiScore !== undefined" class="text-lg font-bold text-green-600">
                    {{ submission.aiScore }}
                  </div>
                  <div v-else class="text-gray-400">-</div>
                </td>

                <td class="py-6 px-6 text-center">
                  <div v-if="editingId === submission.id" class="flex justify-center">
                    <input
                      v-model.number="editingScore"
                      type="number"
                      min="0"
                      max="100"
                      class="w-16 px-3 py-1 border border-gray-300 rounded-lg text-center"
                    />
                  </div>
                  <div v-else class="text-lg font-bold text-blue-600">
                    {{ submission.score !== undefined ? submission.score : '-' }}
                  </div>
                </td>

                <td class="py-6 px-6 text-center">
                  <div class="text-lg font-bold text-purple-600">
                    {{ submission.score ?? submission.aiScore ?? '-' }}
                  </div>
                </td>

                <td class="py-6 px-6">
                  <div class="max-w-xs text-sm text-gray-600 line-clamp-2" :title="submission.aiFeedback">
                    {{ submission.aiFeedback || '暂无反馈' }}
                  </div>
                </td>

                <td class="py-6 px-6">
                  <div v-if="editingId === submission.id">
                    <input
                      v-model="editingComment"
                      type="text"
                      class="w-40 px-3 py-1 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-400 outline-none"
                      placeholder="添加评价..."
                    />
                  </div>
                  <div v-else class="max-w-xs text-sm text-gray-600 line-clamp-2">
                    {{ submission.teacherComment || '-' }}
                  </div>
                </td>

                <td class="py-6 px-6 text-center">
                  <div class="flex gap-2 justify-center">
                    <button
                      v-if="editingId === submission.id"
                      @click="saveGrade(submission.id)"
                      class="px-4 py-2 bg-green-500 text-white rounded-xl hover:bg-green-600 transition-all shadow"
                    >
                      保存
                    </button>
                    <button
                      v-else
                      @click="startEdit(submission)"
                      class="px-4 py-2 bg-blue-500 text-white rounded-xl hover:bg-blue-600 transition-all shadow"
                    >
                      评分
                    </button>
                    <button
                      @click="viewVideo(submission.id)"
                      class="px-4 py-2 bg-gray-200 text-gray-800 rounded-xl hover:bg-gray-300 transition-all shadow"
                    >
                      视频
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="studentLatestSubmissions.length === 0" class="py-16 text-center">
          <div class="text-6xl text-gray-300 mb-4">📭</div>
          <h3 class="text-xl font-bold text-gray-800 mb-2">暂无提交记录</h3>
          <p class="text-gray-500">该作业目前还没有学生提交</p>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { classes as mockClasses, assignments as mockAssignments, submissions as mockSubmissions } from '../../data/mockData'

const route = useRoute()
const router = useRouter()

// 从路由获取参数
const courseId = computed(() => parseInt(route.params.courseId))
const assignmentId = computed(() => parseInt(route.params.assignmentId))

// 响应式数据
const classes = ref(mockClasses)
const assignments = ref(mockAssignments)
const allSubmissions = ref([...mockSubmissions])

// 当前作业标题
const assignmentTitle = computed(() => {
  return assignments.value.find(a => a.id === assignmentId.value)?.title || '未知作业'
})

// 过滤出当前课程 + 当前作业的所有提交记录
const currentSubmissions = computed(() => {
  return allSubmissions.value.filter(s =>
    s.classId === courseId.value && s.assignmentId === assignmentId.value
  )
})

// 每位学生取最新一次提交（按 submissionTime 降序）
const studentLatestSubmissions = computed(() => {
  const map = new Map()

  currentSubmissions.value.forEach(sub => {
    const key = sub.studentId
    const existing = map.get(key)

    if (!existing || new Date(sub.submissionTime) > new Date(existing.submissionTime)) {
      // 补充班级名称、学生姓名等便于显示
      const classInfo = classes.value.find(c => c.id === sub.classId)
      map.set(key, {
        ...sub,
        studentName: sub.studentName,
        className: classInfo ? classInfo.name : '未知班级'
      })
    }
  })

  // 转成数组并按学生姓名排序（可选）
  return Array.from(map.values()).sort((a, b) => a.studentName.localeCompare(b.studentName))
})

// 编辑状态
const editingId = ref(null)
const editingScore = ref(0)
const editingComment = ref('')

const startEdit = (submission) => {
  editingId.value = submission.id
  editingScore.value = submission.score ?? submission.aiScore ?? 0
  editingComment.value = submission.teacherComment || ''
}

const saveGrade = (submissionId) => {
  const index = allSubmissions.value.findIndex(s => s.id === submissionId)
  if (index !== -1) {
    allSubmissions.value[index] = {
      ...allSubmissions.value[index],
      score: editingScore.value,
      teacherComment: editingComment.value,
      status: 'graded'
    }
    editingId.value = null
    console.log('保存成功', allSubmissions.value[index])
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const viewVideo = (id) => console.log('查看视频:', id)

const goBack = () => router.push('/teacher')
const goToAssistant = () => router.push('/teacher/assistant')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}
</script>
