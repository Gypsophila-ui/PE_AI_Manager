<template>
  <div class="min-h-screen bg-white">
    <div class="max-w-7xl mx-auto p-6 space-y-8">
      <!-- 顶部导航栏 -->
      <div class="flex justify-between items-center py-4">
        <h1 class="text-2xl font-bold text-gray-800">体育作业平台 - 教师端</h1>
        <div class="flex gap-4">
          <button @click="goToHome" class="px-4 py-2 rounded-full bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-md">
            🏠 首页
          </button>
          <button @click="logout" class="px-4 py-2 rounded-full bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
            退出登录
          </button>
        </div>
      </div>

      <!-- 页面标题 -->
      <div class="flex justify-between items-center">
        <h2 class="text-2xl font-bold text-gray-800">📝 作业管理</h2>
        <button @click="goToPublishAssignment" class="px-4 py-2 rounded-full bg-green-500 text-white hover:bg-green-600 transition-all shadow-md">
          发布新作业
        </button>
      </div>

      <!-- 筛选条件 -->
      <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
        <div class="flex flex-col md:flex-row gap-3">
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-500 mb-1">选择班级</label>
            <select v-model="selectedClass"
                    class="w-full px-3 py-2 rounded-lg border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400 transition-all text-sm">
              <option value="all">所有班级</option>
              <option v-for="cls in classes" :key="cls.id" :value="cls.id">{{ cls.name }}</option>
            </select>
          </div>
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-500 mb-1">选择科目</label>
            <select v-model="selectedSubject"
                    class="w-full px-3 py-2 rounded-lg border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400 transition-all text-sm">
              <option value="all">所有科目</option>
              <option value="体育">体育</option>
              <option value="篮球">篮球</option>
              <option value="足球">足球</option>
              <option value="游泳">游泳</option>
              <option value="田径">田径</option>
            </select>
          </div>
          <div class="flex-1">
            <label class="block text-xs font-medium text-gray-500 mb-1">作业状态</label>
            <select v-model="selectedStatus"
                    class="w-full px-3 py-2 rounded-lg border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400 transition-all text-sm">
              <option value="all">所有状态</option>
              <option value="published">已发布</option>
              <option value="pending">未发布</option>
              <option value="completed">已完成</option>
            </select>
          </div>
        </div>
      </div>

      <!-- 统计卡片 -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
          <div class="text-xs font-medium text-gray-500 mb-1">总作业数</div>
          <div class="text-2xl font-bold text-gray-800">{{ totalAssignments }}</div>
        </div>
        <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
          <div class="text-xs font-medium text-gray-500 mb-1">已完成作业</div>
          <div class="text-2xl font-bold text-gray-800">{{ completedAssignments }}</div>
        </div>
        <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
          <div class="text-xs font-medium text-gray-500 mb-1">待批改作业</div>
          <div class="text-2xl font-bold text-gray-800">{{ pendingGrading }}</div>
        </div>
      </div>

      <!-- 作业列表 -->
      <div class="bg-white p-4 rounded-xl shadow-md border border-gray-100">
        <h3 class="text-lg font-semibold text-gray-800 mb-3">📋 作业详情</h3>
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead>
              <tr class="border-b">
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">班级</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">作业标题</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">科目</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">截止时间</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">提交人数</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">状态</th>
                <th class="text-left py-2 px-3 text-xs font-medium text-gray-500">操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="assignment in filteredAssignments" :key="assignment.id" class="border-b hover:bg-gray-50">
                <td class="py-2 px-3 text-sm text-gray-700">{{ getClassName(assignment.classId) }}</td>
                <td class="py-2 px-3 text-sm font-medium text-gray-800">{{ assignment.title }}</td>
                <td class="py-2 px-3 text-sm text-gray-700">{{ assignment.subject }}</td>
                <td class="py-2 px-3 text-sm text-gray-700">{{ assignment.deadline }}</td>
                <td class="py-2 px-3 text-sm text-gray-700">{{ getSubmissionCount(assignment.id) }}/{{ getStudentCount(assignment.classId) }}</td>
                <td class="py-2 px-3">
                  <span :class="['px-2 py-1 rounded-full text-xs font-medium',
                                 assignment.status === 'published' ? 'bg-green-100 text-green-700' :
                                 assignment.status === 'pending' ? 'bg-yellow-100 text-yellow-700' :
                                 'bg-red-100 text-red-700']">
                    {{ assignment.status === 'published' ? '已发布' :
                       assignment.status === 'pending' ? '未发布' : '已完成' }}
                  </span>
                </td>
                <td class="py-2 px-3">
                  <div class="flex gap-1">
                    <button class="px-2 py-1 rounded-md bg-blue-500 text-white text-xs hover:bg-blue-600 transition-all">
                      查看
                    </button>
                    <button class="px-2 py-1 rounded-md bg-yellow-500 text-white text-xs hover:bg-yellow-600 transition-all">
                      编辑
                    </button>
                    <button @click="deleteAssignment(assignment.id)"
                            class="px-2 py-1 rounded-md bg-red-500 text-white text-xs hover:bg-red-600 transition-all">
                      删除
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="filteredAssignments.length === 0" class="text-center py-6 text-gray-500">
          暂无作业数据
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { classes, assignments, assignmentsSubmissions } from '../../data/mockData'

const router = useRouter()

// 原始数据
const classData = ref([...classes])
const assignmentData = ref([...assignments])
const submissionData = ref([...assignmentsSubmissions])

// 筛选条件
const selectedClass = ref('')
const selectedAssignment = ref('')
const timeRange = ref('all')

// 计算属性
const filteredClasses = computed(() => {
  if (!selectedClass.value) return classData.value
  return classData.value.filter(cls => cls.id === parseInt(selectedClass.value))
})

const filteredAssignments = computed(() => {
  if (!selectedClass.value) return assignmentData.value
  return assignmentData.value.filter(assign => assign.classId === parseInt(selectedClass.value))
})

const totalAssignments = computed(() => {
  return filteredAssignments.value.length
})

const submittedAssignments = computed(() => {
  return submissionData.value.filter(sub => {
    if (selectedAssignment.value && sub.assignmentId !== parseInt(selectedAssignment.value)) return false
    if (selectedClass.value) {
      const assignment = assignmentData.value.find(assign => assign.id === sub.assignmentId)
      if (!assignment || assignment.classId !== parseInt(selectedClass.value)) return false
    }
    return true
  }).length
})

const pendingAssignments = computed(() => {
  let count = 0
  filteredAssignments.value.forEach(assign => {
    const classItem = classData.value.find(cls => cls.id === assign.classId)
    const submittedCount = submissionData.value.filter(sub => sub.assignmentId === assign.id).length
    count += classItem.studentCount - submittedCount
  })
  return count
})

const averageCompletionRate = computed(() => {
  if (filteredAssignments.value.length === 0) return 0
  let totalRate = 0
  filteredAssignments.value.forEach(assign => {
    const classItem = classData.value.find(cls => cls.id === assign.classId)
    const submittedCount = submissionData.value.filter(sub => sub.assignmentId === assign.id).length
    const rate = (submittedCount / classItem.studentCount) * 100
    totalRate += rate
  })
  return Math.round(totalRate / filteredAssignments.value.length)
})

const displayAssignments = computed(() => {
  return filteredAssignments.value.map(assign => {
    const classItem = classData.value.find(cls => cls.id === assign.classId)
    const submittedCount = submissionData.value.filter(sub => sub.assignmentId === assign.id).length
    const pendingCount = classItem.studentCount - submittedCount
    const completionRate = Math.round((submittedCount / classItem.studentCount) * 100)

    return {
      ...assign,
      submittedCount,
      pendingCount,
      completionRate
    }
  })
})

// 方法
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const viewAssignmentDetails = (assignmentId) => {
  // 这里可以实现查看作业详情的功能
  console.log('查看作业详情:', assignmentId)
}

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

onMounted(() => {
  // 组件挂载时的初始化逻辑
})
</script>
