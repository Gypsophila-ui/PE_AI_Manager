<template>
  <div class="min-h-screen bg-gray-100">
    <!-- 同济大学校徽 -->
    <div class="fixed inset-0 z-10 flex items-center justify-center opacity-5 pointer-events-none">
      <img src="@/assets/Login/2.jpg" alt="同济大学校徽" class="w-21 h-21 object-contain" />
    </div>
    <div class="max-w-4xl mx-auto p-6 space-y-10">

      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">作业详情</h2>
        <p class="text-gray-600">查看作业要求和提交状态</p>
      </section>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
      </div>

      <!-- 错误信息 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-3xl p-6">
        <div class="flex items-center gap-3 mb-3">
          <h3 class="text-xl font-bold text-red-800">加载失败</h3>
        </div>
        <p class="text-red-700">{{ errorMessage }}</p>
        <button @click="fetchAssignmentDetails" class="mt-4 px-6 py-2 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow">
          重试
        </button>
      </div>

      <!-- 作业信息卡片 -->
      <section v-else-if="assignment" class="bg-white rounded-3xl shadow-xl p-6">
        <h3 class="text-2xl font-bold text-gray-800 mb-4">{{ assignment.title }}</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-xs text-gray-400">创建时间</div>
              <div>{{ formatDate(assignment.create_time) }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-xs text-gray-400">截止时间</div>
              <div>{{ formatDate(assignment.deadline) }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-xs text-gray-400">科目</div>
              <div>{{ assignment.subject }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-xs text-gray-400">状态</div>
              <div>
                <span :class="[
                  'px-2 py-1 rounded-full text-xs font-medium',
                  assignment.status === '进行中' ? 'bg-blue-100 text-blue-800' :
                  assignment.status === '已完成' ? 'bg-green-100 text-green-800' :
                  'bg-gray-100 text-gray-800'
                ]">
                  {{ assignment.status }}
                </span>
              </div>
            </div>
          </div>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-xs text-gray-400">课程ID</div>
              <div>{{ assignment.course_id }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-xs text-gray-400">分值</div>
              <div>{{ assignment.points }}分</div>
            </div>
          </div>
        </div>

        <!-- 作业描述与视频上传上下布局 -->
        <div class="space-y-6">
          <!-- 作业描述和AI评分说明 -->
          <div class="w-full">
            <!-- 作业描述 -->
            <div class="mt-4 p-4 bg-blue-50 rounded-xl">
              <div class="assignment-description-wrapper">
                <h4 class="font-medium text-blue-800 mb-2">作业描述：</h4>
                <p class="text-blue-700 whitespace-pre-line max-h-32 overflow-y-auto">{{ assignment.description }}</p>
              </div>
            </div>

            <!-- AI评分说明 -->
            <div class="mt-4 p-4 bg-purple-50 rounded-xl">
              <div>
                <h4 class="font-medium text-purple-800 mb-1">AI评分说明：</h4>
                <p class="text-sm text-purple-700">
                  提交视频后，AI将自动分析你的动作规范度、完成度和技术要点，给出初步评分和详细反馈。
                  教师将根据AI评分和实际情况进行最终评分。
                </p>
              </div>
            </div>
          </div>

          <!-- 视频上传区域 -->
          <div class="w-full bg-white rounded-3xl shadow-xl p-8">
            <div class="flex flex-col items-center space-y-6">
              <!-- 上传区域 -->
              <div
                class="w-full max-w-2xl border-2 border-dashed rounded-2xl p-6 text-center transition-all hover:bg-gray-50"
                :disabled="assignment.status === '已完成'"
                :class="assignment.status === '已完成' ? 'opacity-50 cursor-not-allowed' : ''"
              >
                <div class="text-6xl text-gray-300 mb-4">🎥</div>
                <h3 class="text-xl font-bold text-gray-800 mb-2">上传作业视频</h3>
                <p class="text-gray-500 mb-4">支持 MP4、AVI、MOV 格式，文件大小不超过 200MB</p>
                <button
                  @click="triggerFileInput"
                  class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow"
                >
                  选择视频文件
                </button>
                <input
                  ref="fileInput"
                  type="file"
                  accept="video/*"
                  class="hidden"
                  @change="handleFileChange"
                  :disabled="assignment.status === '已完成'"
                />
              </div>

              <!-- 已选择视频预览 -->
              <div v-if="selectedFile" class="w-full max-w-2xl">
                <div class="bg-gray-100 rounded-xl p-6 mb-4">
                  <div class="flex items-center justify-between mb-4">
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

              <!-- 处理后的视频预览 -->
              <div v-if="showProcessedVideo" class="w-full mt-8">
                <div class="bg-gray-100 rounded-xl p-6 mb-4">
                  <div class="flex items-center justify-between mb-4">
                    <div>
                      <h4 class="font-medium text-gray-800">AI处理后的视频</h4>
                      <p class="text-sm text-gray-500">AI已完成评分并生成处理后的视频</p>
                    </div>
                  </div>
                </div>
                <!-- AI处理后的视频预览 - 宽度充满整个容器 -->
                <div class="rounded-lg overflow-hidden border border-gray-300 w-full">
                  <video
                    ref="processedVideoPreview"
                    controls
                    class="w-full h-auto"
                  ></video>
                </div>
                <div class="mt-4 flex justify-center">
                  <button
                    @click="downloadProcessedVideo"
                    class="px-6 py-2 rounded-xl bg-green-500 text-white hover:bg-green-600 transition-all shadow"
                  >
                    下载处理后的视频
                  </button>
                </div>
              </div>

              <!-- 视频处理状态区域 -->
              <div v-if="isProcessing" class="w-full max-w-2xl space-y-4">
                <!-- 处理状态信息 -->
                <div
                  id="processingStats"
                  class="p-4 rounded-xl bg-gray-50 border border-gray-200"
                  v-html="processingStats"
                ></div>

                <!-- 处理中的视频帧预览 -->
                <div v-if="processingVideoFrame" class="flex justify-center">
                  <img
                    :src="processingVideoFrame"
                    alt="处理过程预览"
                    class="max-w-full max-h-64 rounded-lg shadow"
                  />
                </div>
              </div>

              <!-- 提交按钮 -->
              <button
                @click="submitAssignment"
                :disabled="!selectedFile || isUploading || assignment.status === '已完成'"
                class="px-10 py-4 rounded-2xl bg-blue-500 text-white font-bold text-lg hover:bg-blue-600 transition-all shadow-lg"
                :class="{ 'opacity-50 cursor-not-allowed': !selectedFile || isUploading || assignment.status === '已完成' }"
              >
                {{ isUploading ? '上传中...' : '提交作业' }}
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- 未找到作业 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-10 text-center">
        <h3 v-if="!assignment && !loading && !error" class="text-2xl font-bold text-gray-800 mb-2">未找到作业</h3>
        <p class="text-gray-500 mb-6">无法找到指定ID的作业信息</p>
        <button @click="goBack" class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
          返回上一页
        </button>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from '../../services/axios'
import { apiClient, aiClient } from '../../services/axios'

const router = useRouter()
const route = useRoute()

// 作业详情相关
const assignment = ref(null)
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')

// 文件上传相关（集成提交作业功能）
const fileInput = ref(null)
const videoPreview = ref(null)
const processedVideoPreview = ref(null)
const selectedFile = ref(null)
const uploadProgress = ref(0)
const isUploading = ref(false)
const processedVideoUrl = ref(null)
const showProcessedVideo = ref(false)

// 获取课程ID和作业ID
// 支持两种路由格式：/course/:courseId/assignments/:assignmentId 和 /assignment/:id
const courseId = route.params.courseId || 'PE101' // 默认为PE101课程
const assignmentId = route.params.assignmentId || route.params.id

// 获取作业详情（直接显示示例数据，不调用API）
const fetchAssignmentDetails = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    // 直接加载示例作业数据，不调用API
    console.log('直接加载示例作业数据，作业ID:', assignmentId)

    // 使用mock数据，根据不同的作业ID返回不同的作业详情
    const mockAssignments = {
      '1': {
        title: '俯卧撑标准动作练习',
        description: '完成标准俯卧撑动作，要求动作规范，身体保持直线。注意：1. 双手与肩同宽；2. 身体从头部到脚踝保持一条直线；3. 下降时胸部接近地面；4. 上升时手臂完全伸直。',
        deadline: '2024-01-20T23:59:59',
        create_time: '2024-01-10T08:00:00',
        course_id: 'PE101',
        subject: '体能训练',
        status: '进行中',
        points: 100
      },
      '2': {
        title: '仰卧起坐耐力测试',
        description: '在规定时间内完成尽可能多的仰卧起坐，测试核心力量。要求：1. 双腿弯曲90度；2. 双手交叉抱头；3. 起身时肘部触及膝盖；4. 躺下时肩部完全接触地面。',
        deadline: '2024-01-25T23:59:59',
        create_time: '2024-01-15T10:30:00',
        course_id: 'PE101',
        subject: '体能测试',
        status: '进行中',
        points: 100
      },
      '3': {
        title: '跳绳技巧练习',
        description: '掌握基本跳绳技巧，提高协调性和耐力。练习内容：1. 单摇跳绳（每分钟至少120次）；2. 双摇跳绳（尝试完成10次连续双摇）；3. 交叉跳绳（左右交叉各50次）。',
        deadline: '2024-01-15T23:59:59',
        create_time: '2024-01-05T14:20:00',
        course_id: 'PE101',
        subject: '协调训练',
        status: '已完成',
        points: 100
      }
    }

    // 获取对应的作业详情，如果找不到则使用默认作业
    assignment.value = mockAssignments[assignmentId] || {
      title: '默认作业',
      description: '这是一个默认作业的描述。',
      deadline: '2024-01-30T23:59:59',
      create_time: '2024-01-10T08:00:00',
      course_id: 'PE101',
      subject: '体育',
      status: '进行中',
      points: 100
    }

    console.log('示例作业加载成功:', assignment.value)
  } catch (err) {
    console.error('加载示例作业失败:', err)
    error.value = true
    errorMessage.value = err.message
  } finally {
    loading.value = false
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

// 导航函数
const goBack = () => {
  router.push(`/student/course/${courseId}`)
}

// 文件上传相关函数（集成提交作业功能）

// 触发文件选择
const triggerFileInput = () => {
  if (assignment.value && assignment.value.status !== '已完成') {
    fileInput.value.click()
  }
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

// 格式化文件大小
const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

// 获取处理后的视频 - 备用方法
const getProcessedVideo = async (homeworkId, studentId) => {
  try {
    console.log('开始获取处理后的视频...')

    // 构建请求URL
    const url = `${import.meta.env.VITE_API_BASE_URL || 'http://118.25.145.4:8000'}/get_processed_video`;

    const response = await fetch(url + `?homework_id=${homeworkId}&student_id=${studentId}`);

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    // 将响应转换为Blob
    const videoBlob = await response.blob();
    if (response.status === 200) {
      // 创建视频 URL
      const videoUrl = URL.createObjectURL(videoBlob)
      console.log('成功获取处理后的视频')

      // 更新处理后的视频URL和预览
      processedVideoUrl.value = videoUrl
      showProcessedVideo.value = true
      processedVideoBlob.value = videoBlob

      // 初始化视频预览
      if (processedVideoPreview.value) {
        processedVideoPreview.value.src = videoUrl
        // 手动加载视频，确保视频正确播放
        processedVideoPreview.value.load()
        console.log('处理后的视频预览已设置并开始加载')
      }

      return videoUrl
    } else if (response.status === 404) {
      throw new Error('未找到处理后的视频文件')
    } else {
      throw new Error('获取视频时发生错误')
    }
  } catch (error) {
    console.error('获取视频时发生错误:', error)
    throw error
  }
}

// 视频处理状态相关
const processingStats = ref('')
const isProcessing = ref(false)
const processingVideoFrame = ref('')
const processedVideoBlob = ref(null)

// 提交作业
const submitAssignment = async () => {
  if (!selectedFile.value || assignment.value.status === '已完成') return

  try {
    // 设置上传状态
    isUploading.value = true
    uploadProgress.value = 0
    isProcessing.value = true
    processingStats.value = '正在准备上传文件...'
    processingVideoFrame.value = ''

    // 获取当前用户信息
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const studentId = user.id || 'student1'

    // 创建 FormData 对象
    const formData = new FormData()
    formData.append('file', selectedFile.value)

    // 从后端获取AI类型（动作类型）根据作业ID
     const aiTypeResponse = await apiClient.post('/Homework/get_AI_type', {
      first: assignmentId
    })
    let poseType = 'squat'; // 默认值

    if (aiTypeResponse.success) {
      poseType = aiTypeResponse.data.data || 'squat'; // 使用返回的动作类型，或默认为squat
      console.log('获取到的动作类型:', poseType);
    } else {
      console.warn('获取AI类型失败，使用默认动作类型: squat');
    }

    // 构造请求URL，将pose_type作为URL查询参数传递
    const url = `${import.meta.env.VITE_API_BASE_URL || 'http://118.25.145.4:8000'}/process_video?pose_type=${encodeURIComponent(poseType)}`

    console.log('开始上传视频到AI后端服务...')
    processingStats.value = '正在连接到流式处理服务...'

    // 使用Fetch API和ReadableStream处理SSE流
    const response = await fetch(url, {
      method: 'POST',
      body: formData,
      headers: {
        // 如果需要认证，可以添加认证头
      }
    })

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }

    // 处理SSE流
    const reader = response.body.getReader()
    const decoder = new TextDecoder()
    let buffer = ''
    let videoChunks = []
    let aiResult = null

    function processStream() {
      reader.read().then(({done, value}) => {
        if (done) {
          // 处理完成
          processingStats.value += '<br>处理完成!'
          isUploading.value = false
          isProcessing.value = false

          // 创建视频下载
          if (videoChunks.length > 0) {
            const processedVideoBlobValue = new Blob(videoChunks, { type: 'video/mp4' })
            processedVideoBlob.value = processedVideoBlobValue
            const videoUrl = URL.createObjectURL(processedVideoBlobValue)

            // 更新处理后的视频URL和预览
            processedVideoUrl.value = videoUrl
            showProcessedVideo.value = true

            // 初始化视频预览
            if (processedVideoPreview.value) {
              processedVideoPreview.value.src = videoUrl
              processedVideoPreview.value.load()
            }
          }

          // 如果有AI结果，保存作业提交信息
          if (aiResult) {
            saveAssignmentSubmission(aiResult, studentId)
          }

          return
        }

        // 将接收到的数据添加到缓冲区
        buffer += decoder.decode(value, {stream: true})

        // 处理缓冲区中的数据
        let lines = buffer.split('\n\n')
        buffer = lines.pop() // 保留不完整的最后一行

        for (const chunk of lines) {
          if (chunk.startsWith('data: ')) {
            try {
              const jsonData = chunk.slice(6) // 去掉 'data: ' 前缀
              const data = JSON.parse(jsonData)

              switch (data.event) {
                case 'init':
                  processingStats.value = `<strong>初始化:</strong> ${data.data.message}<br>`
                  if (data.data.fps) {
                    processingStats.value += `FPS: ${data.data.fps}, 分辨率: ${data.data.width}x${data.data.height}<br>`
                  }
                  break

                case 'frame':{
                  // 显示处理后的帧
                  processingVideoFrame.value = `data:image/jpeg;base64,${data.data.image}`
                  // 构建处理状态信息，只包含后端实际返回的字段
                  let statsText = `<strong>处理中...</strong><br>`
                  statsText += `当前帧: ${data.data.processed_frame_count}<br>`
                  statsText += `计数: ${data.data.count}<br>`
                  // 只有当后端返回correct字段时才显示正确计数
                  if (data.data.correct !== undefined && data.data.correct !== null) {
                    statsText += `正确计数: ${data.data.correct}<br>`
                  }
                  statsText += `最大计数: ${data.data.max_count}`
                  processingStats.value = statsText
                  break
                }
                case 'final_stats':
                  // 保存处理后的视频URL
                  if (data.data.download_url) {
                    processedVideoUrl.value = data.data.download_url
                  }

                  aiResult = {
                    final_count: data.data.max_count,
                    processed_frame_count: data.data.processed_frame_count,
                    total_time: data.data.total_time,
                    video_url: data.data.download_url || ''
                  }

                  // 显示最终处理结果
                  processingStats.value = `<strong>处理完成!</strong><br>`
                  processingStats.value += `最终计数: ${data.data.max_count}<br>`
                  processingStats.value += `处理帧数: ${data.data.processed_frame_count}<br>`
                  processingStats.value += `总时间: ${parseFloat(data.data.total_time).toFixed(2)} 秒<br>`
                  if (data.data.download_url || data.data.video_size || processedVideoBlob.value) {
                    processingStats.value += `<button @click="downloadProcessedVideo" class="bg-green-500 hover:bg-green-700 text-white font-bold py-2 px-4 rounded mt-2">下载处理后视频</button>`
                  }
                  break

                case 'error':
                  throw new Error(data.data.message)
              }
            } catch (e) {
              // 如果不是JSON格式，可能是视频数据的一部分
              videoChunks.push(value)
            }
          } else {
            // 收集非SSE格式的数据作为视频块
            videoChunks.push(value)
          }
        }

        // 继续处理流
        processStream()
      }).catch(error => {
        console.error('Error:', error)
        processingStats.value += `<br>错误: ${error.message}`
        alert(`流式处理过程中发生错误: ${error.message}`)
        isUploading.value = false
        isProcessing.value = false
      })
    }

    // 开始处理流
    processStream()

  } catch (error) {
    console.error('视频上传和处理失败:', error)
    alert(`作业提交失败: ${error.message}`)
    isUploading.value = false
    isProcessing.value = false
  }
}

// 保存作业提交信息
const saveAssignmentSubmission = async (aiResult, studentId) => {
  try {
    console.log('开始保存作业提交信息...')

    // 获取JWT token
    const token = localStorage.getItem('token')
    if (!token) {
      throw new Error('未找到认证token，请重新登录')
    }

    // 准备视频URL - 优先使用AI处理后的视频URL，如果没有则使用原始视频
    let videoUrl = aiResult.video_url
    if (!videoUrl && processedVideoBlob.value) {
      // 如果没有AI返回的视频URL但有Blob，创建临时URL
      videoUrl = URL.createObjectURL(processedVideoBlob.value)
    }

    // 根据API文档构造请求参数
    const submitData = {
      student_id: studentId,
      jwt: token,
      course_id: courseId,
      homework_id: assignmentId.toString(),
      video_url: videoUrl
    }

    console.log('提交作业参数:', submitData)

    // 调用submit_homework接口
    const submitResponse = await apiClient.post('/submit_homework', submitData, {
      headers: {
        'Content-Type': 'application/json'
      }
    })

    // 上传成功处理
    if (submitResponse.status === 200 || submitResponse.status === 201) {
      console.log('作业提交成功:', submitResponse.data)
      alert(`作业提交成功！视频已处理完成。\n可在作业详情查看提交记录。`)
      // 更新作业状态为已完成
      if (assignment.value) {
        assignment.value.status = '已完成'
      }
    } else {
      console.error('作业提交失败，状态码:', submitResponse.status)
      alert('视频处理成功，但作业提交记录保存失败，请稍后重试。')
    }
  } catch (error) {
    console.error('保存作业提交信息失败:', error)
    if (error.response) {
      console.error('错误响应:', error.response.data)
      const errorMsg = error.response.data?.message || '作业提交记录保存失败，请稍后重试。'
      alert(errorMsg)
    } else {
      alert('作业提交记录保存失败，请稍后重试。')
    }
  }
}

// 下载处理后的视频
const downloadProcessedVideo = () => {
  if (!processedVideoUrl.value && !processedVideoBlob.value) return

  // 创建下载链接
  const link = document.createElement('a')
  let downloadUrl = processedVideoUrl.value

  // 如果没有URL但有Blob，创建新的URL
  if (!downloadUrl && processedVideoBlob.value) {
    downloadUrl = URL.createObjectURL(processedVideoBlob.value)
  }

  link.href = downloadUrl
  link.download = `processed_video_${assignmentId}_${new Date().getTime()}.mp4`

  // 触发下载
  document.body.appendChild(link)
  link.click()

  // 清理
  document.body.removeChild(link)

  // 如果是临时创建的URL，清理它
  if (!processedVideoUrl.value && downloadUrl) {
    setTimeout(() => {
      URL.revokeObjectURL(downloadUrl)
    }, 1000)
  }

  console.log('视频下载已触发')
}

// 组件挂载时获取作业详情
onMounted(() => {
  fetchAssignmentDetails()
})
</script>
