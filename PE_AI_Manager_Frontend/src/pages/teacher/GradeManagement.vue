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

      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">✏️ 成绩管理</h2>
        <p class="text-gray-600">查看和管理学生作业成绩</p>
      </section>

      <!-- 筛选条件 -->
      <section class="bg-white rounded-3xl shadow-xl p-6">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <!-- 选择班级 -->
          <div>
            <label for="classId" class="block text-sm font-medium text-gray-700 mb-2">选择班级</label>
            <select
              id="classId"
              v-model="filter.classId"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
              @change="filterSubmissions"
            >
              <option value="">全部班级</option>
              <option v-for="classItem in classes" :key="classItem.id" :value="classItem.id">
                {{ classItem.name }}
              </option>
            </select>
          </div>

          <!-- 选择作业 -->
          <div>
            <label for="assignmentId" class="block text-sm font-medium text-gray-700 mb-2">选择作业</label>
            <select
              id="assignmentId"
              v-model="filter.assignmentId"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
              @change="filterSubmissions"
            >
              <option value="">全部作业</option>
              <option v-for="assignment in assignments" :key="assignment.id" :value="assignment.id">
                {{ assignment.title }}
              </option>
            </select>
          </div>

          <!-- 选择状态 -->
          <div>
            <label for="status" class="block text-sm font-medium text-gray-700 mb-2">提交状态</label>
            <select
              id="status"
              v-model="filter.status"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
              @change="filterSubmissions"
            >
              <option value="">全部状态</option>
              <option value="submitted">已提交</option>
              <option value="graded">已批改</option>
              <option value="pending">未提交</option>
            </select>
          </div>
        </div>
      </section>

      <!-- 成绩列表 -->
      <section class="bg-white rounded-3xl shadow-xl p-8">
        <div class="overflow-x-auto">
          <table class="w-full min-w-full">
            <thead>
              <tr class="border-b border-gray-200">
                <th class="text-left py-4 px-6 font-medium text-gray-600">学生信息</th>
                <th class="text-left py-4 px-6 font-medium text-gray-600">作业名称</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">AI评分</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">教师评分</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">总分</th>
                <th class="text-left py-4 px-6 font-medium text-gray-600">AI反馈</th>
                <th class="text-left py-4 px-6 font-medium text-gray-600">教师评价</th>
                <th class="text-center py-4 px-6 font-medium text-gray-600">操作</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="submission in filteredSubmissions"
                :key="submission.id"
                class="border-b border-gray-100 hover:bg-gray-50 transition-colors"
              >
                <!-- 学生信息 -->
                <td class="py-6 px-6">
                  <div class="flex items-center gap-4">
                    <div class="w-12 h-12 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-xl">
                      {{ submission.studentName.charAt(0) }}
                    </div>
                    <div>
                      <div class="font-medium text-gray-800">{{ submission.studentName }}</div>
                      <div class="text-sm text-gray-500">班级：{{ getClassName(submission.classId) }}</div>
                    </div>
                  </div>
                </td>

                <!-- 作业名称 -->
                <td class="py-6 px-6">
                  <div class="font-medium text-gray-800">{{ getAssignmentName(submission.assignmentId) }}</div>
                  <div class="text-sm text-gray-500">{{ formatDate(submission.submissionTime) }}</div>
                </td>

                <!-- AI评分 -->
                <td class="py-6 px-6 text-center">
                  <div v-if="submission.aiFeedback" class="text-lg font-bold text-green-600">
                    {{ submission.aiFeedback ? '90' : '-' }}
                  </div>
                  <div v-else class="text-gray-400">-</div>
                </td>

                <!-- 教师评分 -->
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
                    {{ submission.score || '-' }}
                  </div>
                </td>

                <!-- 总分 -->
                <td class="py-6 px-6 text-center">
                  <div class="text-lg font-bold text-purple-600">
                    {{ submission.score ? submission.score : '-' }}
                  </div>
                </td>

                <!-- AI反馈 -->
                <td class="py-6 px-6">
                  <div class="max-w-xs text-sm text-gray-600 line-clamp-2">
                    {{ submission.aiFeedback || '-' }}
                  </div>
                </td>

                <!-- 教师评价 -->
                <td class="py-6 px-6">
                  <div v-if="editingId === submission.id" class="flex justify-center">
                    <input
                      v-model="editingComment"
                      type="text"
                      class="w-40 px-3 py-1 border border-gray-300 rounded-lg"
                      placeholder="添加评价..."
                    />
                  </div>
                  <div v-else class="max-w-xs text-sm text-gray-600 line-clamp-2">
                    {{ submission.teacherComment || '-' }}
                  </div>
                </td>

                <!-- 操作按钮 -->
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
                      编辑
                    </button>
                    <button
                      @click="viewVideo(submission.id)"
                      class="px-4 py-2 bg-gray-200 text-gray-800 rounded-xl hover:bg-gray-300 transition-all shadow"
                    >
                      查看视频
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- 空状态 -->
        <div v-if="filteredSubmissions.length === 0" class="py-16 text-center">
          <div class="text-6xl text-gray-300 mb-4">📭</div>
          <h3 class="text-xl font-bold text-gray-800 mb-2">暂无数据</h3>
          <p class="text-gray-500">没有找到符合条件的作业提交记录</p>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { classes, assignments, submissions } from '../../data/mockData'

const router = useRouter()

// 筛选条件
const filter = ref({
  classId: '',
  assignmentId: '',
  status: ''
})

// 编辑状态
const editingId = ref(null)
const editingScore = ref(0)
const editingComment = ref('')

// 计算筛选后的提交记录
const filteredSubmissions = computed(() => {
  return submissions.filter(submission => {
    let match = true

    if (filter.value.classId && submission.classId !== parseInt(filter.value.classId)) {
      match = false
    }

    if (filter.value.assignmentId && submission.assignmentId !== parseInt(filter.value.assignmentId)) {
      match = false
    }

    if (filter.value.status && submission.status !== filter.value.status) {
      match = false
    }

    return match
  })
})

// 开始编辑
const startEdit = (submission) => {
  editingId.value = submission.id
  editingScore.value = submission.score || 0
  editingComment.value = submission.teacherComment || ''
}

// 保存成绩
const saveGrade = (submissionId) => {
  const submission = submissions.find(s => s.id === submissionId)
  if (submission) {
    submission.score = editingScore.value
    submission.teacherComment = editingComment.value
    submission.status = 'graded'
  }

  editingId.value = null
  alert('成绩保存成功！')
}

// 筛选提交记录
const filterSubmissions = () => {
  // 这里可以添加额外的筛选逻辑
  console.log('筛选条件:', filter.value)
}

// 获取班级名称
const getClassName = (classId) => {
  const classItem = classes.find(c => c.id === classId)
  return classItem ? classItem.name : '未知班级'
}

// 获取作业名称
const getAssignmentName = (assignmentId) => {
  const assignment = assignments.find(a => a.id === assignmentId)
  return assignment ? assignment.title : '未知作业'
}

// 格式化日期
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 查看视频
const viewVideo = (submissionId) => {
  console.log('查看视频:', submissionId)
  // 这里可以跳转到视频查看页面
}

// 导航函数
const goBack = () => {
  router.push('/teacher')
}

const goToAssistant = () => {
  router.push('/teacher/assistant')
}

const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}
</script>
