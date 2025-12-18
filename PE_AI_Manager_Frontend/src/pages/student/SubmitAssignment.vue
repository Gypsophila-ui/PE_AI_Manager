<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-4xl mx-auto p-6 space-y-10">

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

          <!-- 上传进度显示 -->
          <div v-if="isUploading" class="w-full max-w-2xl">
            <div class="bg-gray-100 rounded-xl p-6">
              <div class="flex justify-between items-center mb-2">
                <span class="text-gray-700 font-medium">上传进度</span>
                <span class="text-blue-600 font-bold">{{ uploadProgress }}%</span>
              </div>
              <div class="w-full bg-gray-200 rounded-full h-2.5">
                <div
                  class="bg-blue-600 h-2.5 rounded-full transition-all duration-300"
                  :style="{ width: uploadProgress + '%' }"
                ></div>
              </div>
              <p class="text-sm text-gray-500 mt-2 text-center">视频正在上传，请不要关闭页面...</p>
            </div>
          </div>

          <!-- 提交按钮 -->
          <button
            @click="submitAssignment"
            :disabled="!selectedFile || isUploading"
            class="px-10 py-4 rounded-2xl bg-blue-500 text-white font-bold text-lg hover:bg-blue-600 transition-all shadow-lg"
            :class="{ 'opacity-50 cursor-not-allowed': !selectedFile || isUploading }"
          >
            {{ isUploading ? '上传中...' : '提交作业' }}
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
import { apiClient, aiClient } from '../../services/axios'

const router = useRouter()
const route = useRoute()

// 文件上传相关
const fileInput = ref(null)
const videoPreview = ref(null)
const selectedFile = ref(null)
const uploadProgress = ref(0)
const isUploading = ref(false)

// 获取课程ID和作业ID
const courseId = route.params.courseId
const assignmentId = parseInt(route.params.assignmentId) || 1

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
const submitAssignment = async () => {
  if (!selectedFile.value) return

  try {
    // 设置上传状态
    isUploading.value = true
    uploadProgress.value = 0

    // 获取当前用户信息
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const studentId = user.id || 'student1'

    // 1. 先调用AI后端服务处理视频，获取AI评分结果
    const aiFormData = new FormData()
    aiFormData.append('file', selectedFile.value)
    // 根据作业类型设置动作类型，统一使用深蹲动作类型
    aiFormData.append('pose_type', 'squat') // 统一使用深蹲动作类型

    // 调用AI后端API处理视频
    const aiResponse = await aiClient.post('/process_and_save_video', aiFormData, {
      params: {
        homework_id: assignmentId,
        student_id: studentId
      },
      onUploadProgress: (progressEvent) => {
        // 计算上传进度百分比
        if (progressEvent.total) {
          uploadProgress.value = Math.round((progressEvent.loaded * 100) / progressEvent.total)
        }
      },
      responseType: 'json'
    })

    // 获取AI评分结果
    const aiResult = aiResponse.data
    console.log('AI评分结果:', aiResult)

    // 2. 调用主应用API保存作业提交信息和AI评分
    const submitFormData = new FormData()
    submitFormData.append('video', selectedFile.value)
    submitFormData.append('courseId', courseId)
    submitFormData.append('assignmentId', assignmentId)
    submitFormData.append('studentId', studentId)
    submitFormData.append('aiScore', aiResult.final_count) // 使用AI返回的动作计数作为评分
    submitFormData.append('aiProcessedVideoUrl', aiResult.video_url)

    // 调用主应用API保存作业提交
    const submitResponse = await apiClient.post('/submit_assignment_video', submitFormData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    // 上传成功处理
    if (submitResponse.status === 200 || submitResponse.status === 201) {
      alert(`作业提交成功！AI评分结果：${aiResult.final_count}个动作。\n视频已处理完成，可在作业详情查看。`)
      // 跳转到课程详情页
      router.push(`/course/${courseId}`)
    } else {
      alert('作业提交失败，请稍后重试。')
    }
  } catch (error) {
    console.error('视频上传和处理失败:', error)
    alert('作业提交失败，请稍后重试。')
  } finally {
    // 重置上传状态
    isUploading.value = false
  }
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

</script>
