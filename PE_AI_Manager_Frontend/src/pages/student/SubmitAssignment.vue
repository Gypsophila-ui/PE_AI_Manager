<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-4xl mx-auto p-6 space-y-10">
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
        <h2 class="text-4xl font-bold text-gray-800 mb-4">📤 提交作业</h2>
        <p class="text-gray-600">上传你的作业视频，AI将为你评分</p>
      </section>

      <!-- 作业信息卡片 -->
      <section class="bg-white rounded-3xl shadow-xl p-6">
        <h3 class="text-2xl font-bold text-gray-800 mb-4">{{ assignment.title }}</h3>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">📚</span>
            <span>科目：{{ assignment.subject }}</span>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">⏰</span>
            <span>截止：{{ formatDate(assignment.deadline) }}</span>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <span class="text-gray-400">💯</span>
            <span>分值：{{ assignment.points }}</span>
          </div>
        </div>
        <div class="mt-4 p-4 bg-blue-50 rounded-xl">
          <h4 class="font-medium text-blue-800 mb-2">作业要求：</h4>
          <p class="text-blue-700">{{ assignment.description }}</p>
        </div>
      </section>

      <!-- 视频上传区域 -->
      <section class="bg-white rounded-3xl shadow-xl p-8">
        <div class="flex flex-col items-center space-y-6">
          <!-- 上传区域 -->
          <div
            class="w-full max-w-2xl border-2 border-dashed rounded-2xl p-10 text-center transition-all hover:bg-gray-50 cursor-pointer"
            @click="triggerFileInput"
          >
            <div class="text-6xl text-gray-300 mb-4">🎥</div>
            <h3 class="text-xl font-bold text-gray-800 mb-2">上传作业视频</h3>
            <p class="text-gray-500 mb-4">支持 MP4、AVI、MOV 格式，文件大小不超过 200MB</p>
            <button class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
              选择视频文件
            </button>
            <input
              ref="fileInput"
              type="file"
              accept="video/*"
              class="hidden"
              @change="handleFileChange"
            />
          </div>

          <!-- 已选择视频预览 -->
          <div v-if="selectedFile" class="w-full max-w-2xl">
            <div class="bg-gray-100 rounded-xl p-6">
              <div class="flex items-center justify-between mb-4">
                <div class="flex items-center gap-3">
                  <div class="text-3xl text-blue-500">📹</div>
                  <div>
                    <h4 class="font-medium text-gray-800">{{ selectedFile.name }}</h4>
                    <p class="text-sm text-gray-500">{{ formatFileSize(selectedFile.size) }}</p>
                  </div>
                </div>
                <button
                  @click="removeFile"
                  class="px-4 py-2 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow"
                >
                  移除
                </button>
              </div>
              <!-- 视频预览 -->
              <div class="rounded-lg overflow-hidden border border-gray-300">
                <video
                  ref="videoPreview"
                  controls
                  class="w-full h-auto max-h-60"
                ></video>
              </div>
            </div>
          </div>

          <!-- AI评分提示 -->
          <div class="w-full max-w-2xl p-4 bg-purple-50 rounded-xl">
            <div class="flex items-start gap-3">
              <div class="text-2xl text-purple-500">🤖</div>
              <div>
                <h4 class="font-medium text-purple-800 mb-1">AI评分说明：</h4>
                <p class="text-sm text-purple-700">
                  提交视频后，AI将自动分析你的动作规范度、完成度和技术要点，给出初步评分和详细反馈。
                  教师将根据AI评分和实际情况进行最终评分。
                </p>
              </div>
            </div>
          </div>

          <!-- 提交按钮 -->
          <button
            @click="submitAssignment"
            :disabled="!selectedFile"
            class="px-10 py-4 rounded-2xl bg-blue-500 text-white font-bold text-lg hover:bg-blue-600 transition-all shadow-lg"
            :class="!selectedFile ? 'opacity-50 cursor-not-allowed' : ''"
          >
            提交作业
          </button>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { assignments } from '../../data/mockData'

const router = useRouter()
const route = useRoute()

// 文件上传相关
const fileInput = ref(null)
const videoPreview = ref(null)
const selectedFile = ref(null)

// 获取作业ID
const assignmentId = parseInt(route.params.id) || 1

// 获取作业信息
const assignment = ref(assignments.find(a => a.id === assignmentId) || assignments[0])

// 触发文件选择
const triggerFileInput = () => {
  fileInput.value.click()
}

// 处理文件选择
const handleFileChange = (event) => {
  const file = event.target.files[0]
  if (file) {
    selectedFile.value = file

    // 预览视频
    const reader = new FileReader()
    reader.onload = (e) => {
      if (videoPreview.value) {
        videoPreview.value.src = e.target.result
      }
    }
    reader.readAsDataURL(file)
  }
}

// 移除文件
const removeFile = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
  if (videoPreview.value) {
    videoPreview.value.src = ''
  }
}

// 提交作业
const submitAssignment = () => {
  if (!selectedFile.value) return

  // 模拟提交作业
  alert('作业提交成功！AI正在评分，请稍后查看结果。')

  // 跳转到学生首页
  router.push('/student')
}

// 格式化日期
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 格式化文件大小
const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

// 导航函数
const goBack = () => {
  router.push('/student')
}

const goToAssistant = () => {
  router.push('/student/assistant')
}

const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}
</script>
