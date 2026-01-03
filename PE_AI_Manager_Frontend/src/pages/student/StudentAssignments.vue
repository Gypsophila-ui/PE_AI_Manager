<template>
  <div class="min-h-screen bg-gray-100">
    <!-- 同济大学校徽 -->
    <div class="fixed inset-0 z-10 flex items-center justify-center opacity-5 pointer-events-none">
      <img src="@/assets/Login/2.jpg" alt="同济大学校徽" class="w-21 h-21 object-contain" />
    </div>
    <div class="max-w-4xl mx-auto p-6 space-y-10">

      <!-- 页面标题 -->
      <section class="flex justify-between items-center">
        <div class="flex items-center gap-4">
          <button @click="goBack" class="px-6 py-3 rounded-xl bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow flex items-center gap-2">
           返回
          </button>
          <div>
            <h2 class="text-4xl font-bold text-gray-800 mb-4">作业详情</h2>
            <p class="text-gray-600">查看作业要求和提交状态</p>
          </div>
        </div>
        <button @click="goToHistory" class="px-6 py-3 rounded-xl bg-purple-500 text-white hover:bg-purple-600 transition-all shadow-lg flex items-center gap-2">
          查看提交历史
        </button>
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
        <h3 class="text-3xl font-bold text-gray-800 mb-4">{{ assignment.title }}</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">创建时间</div>
              <div class="text-lg">{{ formatDate(assignment.create_time) }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">截止时间</div>
              <div class="text-lg">{{ formatDate(assignment.deadline) }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">科目</div>
              <div class="text-lg">{{ assignment.subject }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">状态</div>
              <div>
                <span :class="[
                  'px-3 py-1 rounded-full text-sm font-medium',
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
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">课程ID</div>
              <div class="text-lg">{{ assignment.course_id }}</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">分值</div>
              <div class="text-lg">{{ assignment.points }}分</div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">动作类型</div>
              <div>
                <span class="px-3 py-1 rounded-full text-sm font-medium bg-orange-100 text-orange-800">
                  {{ aiType || '加载中...' }}
                </span>
              </div>
            </div>
          </div>
          <div class="flex items-center gap-2 text-gray-600">
            <div>
              <div class="text-sm text-gray-400">要求动作个数</div>
              <div>
                <span class="px-3 py-1 rounded-full text-sm font-medium bg-purple-100 text-purple-800">
                  {{ requiredCount !== null ? requiredCount : '加载中...' }}
                </span>
              </div>
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
                <p class="text-gray-500 mb-4">仅支持 MP4 格式，单个文件最大 100MB</p>
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
                <!-- AI处理后的视频预览 - 使用组件库中的视频播放器 -->
                <div class="rounded-lg overflow-hidden border border-gray-300 w-full">
                  <InlineVideoPlayer
                    :src="processedVideoUrl"
                    v-if="processedVideoUrl"
                    class="w-full"
                  />
                  <div v-else class="flex items-center justify-center h-64 bg-gray-100 rounded-lg">
                    <p class="text-gray-500">视频预览待加载...</p>
                  </div>
                </div>
                <div class="mt-4 flex justify-center">
                  <button
                    v-if="processedVideoUrl || processedVideoBlob"
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
                    class="max-w-full max-h-100 rounded-lg shadow"
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
import { ElMessageBox } from 'element-plus'
import { StudentAssignmentService } from '../../services/studentAssignments'
import InlineVideoPlayer from '@/components/InlineVideoPlayer.vue'

const router = useRouter()
const route = useRoute()
const assignmentService = new StudentAssignmentService()

// 作业详情相关
const assignment = ref(null)
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')
const finalScore = ref(null)
const aiType = ref(null)
const requiredCount = ref(null)

// 文件上传相关（集成提交作业功能）
const fileInput = ref(null)
const videoPreview = ref(null)
const selectedFile = ref(null)
const uploadProgress = ref(0)
const isUploading = ref(false)
const processedVideoUrl = ref(null)
const downloadVideoUrl = ref(null)
const showProcessedVideo = ref(false)
const aiEvaluationSaved = ref(false)
const processingStats = ref('')
const isProcessing = ref(false)
const processingVideoFrame = ref('')
const processedVideoBlob = ref(null)

// 获取课程ID和作业ID
// 支持路由格式：/course/:courseId/assignments/:assignmentId
const courseId = route.params.courseId || 'PE101' // 默认为PE101课程
const assignmentId = route.params.assignmentId || route.params.id

// 获取作业详情（调用后端API）
const fetchAssignmentDetails = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    const assignmentData = await assignmentService.fetchAssignmentDetails(courseId, assignmentId);
    assignment.value = assignmentData;
    console.log('作业详情加载成功:', assignment.value);

    // 获取AI类型
    const poseTypeInfo = await assignmentService.getPoseType(assignmentId);
    aiType.value = poseTypeInfo.poseType;
    requiredCount.value = poseTypeInfo.requiredCount;
    console.log('AI类型:', aiType.value);
    console.log('要求数量:', requiredCount.value);
  } catch (err) {
    console.error('获取作业详情失败:', err);
    error.value = true;
    errorMessage.value = err.message || '获取作业详情失败，请稍后重试';
  } finally {
    loading.value = false;
  }
};

// 获取最终得分
const fetchFinalScore = async () => {
  try {
    const score = await assignmentService.fetchFinalScore(courseId, assignmentId);
    finalScore.value = score;
  } catch (err) {
    console.error('获取最终得分失败:', err);
    finalScore.value = null;
  }
};

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

const goToHistory = () => {
  router.push(`/student/course/${courseId}/assignments/${assignmentId}/submission-history`)
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

    const result = await assignmentService.getProcessedVideo(homeworkId, studentId);
    if (result) {
      // 更新处理后的视频URL和预览
      processedVideoUrl.value = result.videoUrl
      showProcessedVideo.value = true
      processedVideoBlob.value = result.videoBlob

      console.log('处理后的视频预览URL已更新')

      return result.videoUrl
    } else {
      throw new Error('未找到处理后的视频文件')
    }
  } catch (error) {
    console.error('获取处理后的视频失败:', error);
    throw error;
  }
}

// 提交作业
const submitAssignment = async () => {
  if (!selectedFile.value || assignment.value.status === '已完成') return

  try {
    // 重置AI评价保存状态
    aiEvaluationSaved.value = false

    // 设置上传状态
    isUploading.value = true
    uploadProgress.value = 0
    isProcessing.value = true
    processingStats.value = '正在准备上传文件...'
    processingVideoFrame.value = ''

    // 获取当前用户信息
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const studentId = user.id || 'student1'

    try {
      const result = await assignmentService.submitAssignment(
        courseId,
        assignmentId,
        selectedFile.value,
        processedVideoBlob.value,
        processedVideoUrl.value,
        (frameUrl) => {
          processingVideoFrame.value = frameUrl;
        }
      );

      const { processedVideoUrlValue, aiResult, poseTypeInfo } = result;

      // 保存作业提交信息
      await assignmentService.saveAssignmentSubmission(
        courseId,
        assignmentId,
        aiResult,
        studentId,
        processedVideoUrlValue,
        poseTypeInfo
      );

      // 更新本地状态
      processedVideoUrl.value = processedVideoUrlValue;

      // 更新作业状态为已完成
      if (assignment.value) {
        assignment.value.status = '已完成';
      }

      // 重新获取最终得分
      await fetchFinalScore();

      // 显示提交成功弹窗
      ElMessageBox.alert('作业提交成功！', '提示', {
        confirmButtonText: '确定',
        type: 'success'
      });
    } catch (error) {
      // AI服务调用失败，获取poseTypeInfo并创建空的AI评价结果
      console.error('AI服务调用失败:', error);
      processingStats.value = `AI服务暂时不可用，将直接提交作业。<br>错误: ${error.message}`;

      // 获取poseTypeInfo
      const poseTypeInfo = await assignmentService.getPoseType(assignmentId);

      // 创建空的AI评价结果
      const aiResult = {
        final_count: 0,
        processed_frame_count: 0,
        total_time: 0,
        video_url: null
      };

      // 直接保存作业提交信息
      await assignmentService.saveAssignmentSubmission(
        courseId,
        assignmentId,
        aiResult,
        studentId,
        null,
        poseTypeInfo
      );

      // 更新作业状态为已完成
      if (assignment.value) {
        assignment.value.status = '已完成';
      }

      // 重新获取最终得分
      await fetchFinalScore();
    }
  } catch (error) {
    console.error('作业提交失败:', error);
    alert(`作业提交失败: ${error.message}`);
  } finally {
    // 重置上传状态
    isUploading.value = false;
    isProcessing.value = false;
  }
}



// 下载处理后的视频
const downloadProcessedVideo = async () => {
  try {
    await assignmentService.downloadProcessedVideo(
      processedVideoUrl.value,
      downloadVideoUrl.value,
      processedVideoBlob.value,
      assignmentId
    );
  } catch (error) {
    console.error('视频下载失败:', error);
    alert(error.message || '无法下载视频文件');
  }
}

// 组件挂载时获取作业详情
onMounted(() => {
  fetchAssignmentDetails()
  fetchFinalScore()
})
</script>
