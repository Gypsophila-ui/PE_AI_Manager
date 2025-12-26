<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-8">
      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">📚 课程详情</h2>
      </section>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
      </div>

      <!-- 错误信息 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-3xl p-6">
        <div class="flex items-center gap-3 mb-3">
          <div class="text-3xl text-red-500">❌</div>
          <h3 class="text-xl font-bold text-red-800">加载失败</h3>
        </div>
        <p class="text-red-700">{{ errorMessage }}</p>
        <button @click="fetchCourseDetails" class="mt-4 px-6 py-2 rounded-xl bg-red-500 text-white hover:bg-red-600 transition-all shadow">
          重试
        </button>
      </div>

      <!-- 课程信息卡片 -->
      <section v-else-if="course" class="bg-white rounded-3xl shadow-xl p-6">
        <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6">
          <div>
            <h3 class="text-3xl font-bold text-gray-800 mb-2">{{ course.name }}</h3>
            <p class="text-gray-600 mb-4">{{ course.description }}</p>
            <div class="flex items-center gap-4">
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📚</span>
                <span>{{ course.subject }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📊</span>
                <span>{{ course.status }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">📝</span>
                <span>{{ course.assignments.length }} 个作业</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- 教学视频列表 -->
      <section v-if="course" class="bg-white rounded-3xl shadow-xl p-6">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-2xl font-bold text-gray-800">🎥 教学视频</h3>
          <span v-if="teachingVideos.length > 0" class="text-sm text-gray-500">{{ teachingVideos.length }} 个视频</span>
        </div>

        <!-- 视频加载状态 -->
        <div v-if="videosLoading" class="flex justify-center items-center h-32">
          <div class="animate-spin rounded-full h-10 w-10 border-t-4 border-b-4 border-blue-500"></div>
        </div>

        <!-- 视频错误信息 -->
        <div v-else-if="videosError" class="bg-red-50 border border-red-200 rounded-xl p-4">
          <p class="text-red-700">{{ videosErrorMessage }}</p>
        </div>

        <!-- 视频列表 -->
        <div v-else-if="teachingVideos.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="video in teachingVideos" :key="video.id"
               class="bg-gradient-to-br from-blue-50 to-purple-50 rounded-xl shadow-md p-4 hover:shadow-lg transition-all cursor-pointer"
               @click="openVideoPlayer(video)">
            <!-- 视频封面 -->
            <div class="relative aspect-video bg-gray-200 rounded-lg mb-3 overflow-hidden">
              <img v-if="video.cover" :src="video.cover" :alt="video.title" class="w-full h-full object-cover">
              <div v-else class="w-full h-full flex items-center justify-center bg-gray-300">
                <span class="text-4xl text-gray-400">🎬</span>
              </div>
              <!-- 播放按钮覆盖层 -->
              <div class="absolute inset-0 bg-black bg-opacity-30 flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity">
                <div class="w-12 h-12 bg-white rounded-full flex items-center justify-center shadow-lg">
                  <span class="text-blue-500 text-2xl">▶</span>
                </div>
              </div>
              <!-- 时长标签 -->
              <div class="absolute bottom-2 right-2 bg-black bg-opacity-70 text-white text-xs px-2 py-1 rounded">
                {{ video.duration }}
              </div>
            </div>

            <!-- 视频信息 -->
            <h4 class="text-lg font-semibold text-gray-800 mb-1 truncate">{{ video.title }}</h4>
            <p class="text-sm text-gray-600 mb-2 line-clamp-2">{{ video.description }}</p>
            <div class="flex items-center justify-between text-xs text-gray-500">
              <span v-if="video.uploadDate">{{ formatDate(video.uploadDate) }}</span>
              <span class="text-blue-500 font-medium">点击播放</span>
            </div>
          </div>
        </div>

        <!-- 无视频提示 -->
        <div v-else class="bg-gray-50 rounded-xl p-10 text-center">
          <div class="text-6xl text-gray-300 mb-4">🎬</div>
          <h3 class="text-xl font-bold text-gray-800 mb-2">暂无教学视频</h3>
          <p class="text-gray-500">该课程目前还没有发布任何教学视频</p>
        </div>
      </section>

      <!-- 作业列表 -->
      <section v-if="course && course.assignments.length > 0" class="bg-white rounded-3xl shadow-xl p-6">
        <h3 class="text-2xl font-bold text-gray-800 mb-4">课程作业</h3>
        <div class="space-y-3">
          <div v-for="assignment in course.assignments" :key="assignment.id"
               class="bg-white rounded-xl shadow-md border border-gray-100 p-4 hover:shadow-lg transition-all">
            <div class="flex flex-col md:flex-row md:items-center justify-between">
              <div class="flex-1">
                <h4 class="text-lg font-semibold text-gray-800 mb-1">{{ assignment.title }}</h4>
                <p class="text-sm text-gray-600 mb-2">{{ assignment.description }}</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs text-gray-500">{{ assignment.subject }}</span>
                  <span :class="['text-xs px-2 py-1 rounded-full',
                                assignment.status === '进行中' ? 'bg-blue-100 text-blue-800' :
                                assignment.status === '已完成' ? 'bg-green-100 text-green-800' :
                                'bg-gray-100 text-gray-800']">
                    {{ assignment.status }}
                  </span>
                  <span class="text-xs text-gray-500">截止时间: {{ formatDate(assignment.deadline) }}</span>
                </div>
              </div>
              <router-link :to="`/course/${course.id}/assignments/${assignment.id}`"
                          class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium">
                查看详情
              </router-link>
            </div>
          </div>
        </div>
      </section>

      <!-- 无作业提示 -->
      <section v-else-if="course && course.assignments.length === 0" class="bg-white rounded-3xl shadow-xl p-10 text-center">
        <div class="text-6xl text-gray-300 mb-4">📝</div>
        <h3 class="text-2xl font-bold text-gray-800 mb-2">暂无作业</h3>
        <p class="text-gray-500">该课程目前还没有发布任何作业</p>
      </section>

      <!-- 未找到课程 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-10 text-center">
        <div class="text-6xl text-gray-300 mb-4">🔍</div>
        <h3 class="text-2xl font-bold text-gray-800 mb-2">未找到课程</h3>
        <p class="text-gray-500 mb-6">无法找到指定ID的课程信息</p>
        <button @click="goBack" class="px-6 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
          返回首页
        </button>
      </section>
    </div>

    <!-- 视频播放器模态框 -->
    <div v-if="showVideoModal && selectedVideo" class="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-2xl shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-hidden">
        <!-- 模态框头部 -->
        <div class="flex items-center justify-between p-4 border-b">
          <h3 class="text-xl font-bold text-gray-800">{{ selectedVideo.title }}</h3>
          <button @click="closeVideoPlayer" class="text-gray-500 hover:text-gray-700 text-2xl font-bold">
            ×
          </button>
        </div>

        <!-- 视频播放区域 -->
        <div class="aspect-video bg-black">
          <video v-if="selectedVideo.url" :src="selectedVideo.url" controls class="w-full h-full" autoplay>
            您的浏览器不支持视频播放
          </video>
          <div v-else class="w-full h-full flex items-center justify-center text-white">
            <div class="text-center">
              <div class="text-6xl mb-4">⚠️</div>
              <p class="text-xl">视频地址不可用</p>
            </div>
          </div>
        </div>

        <!-- 视频信息 -->
        <div class="p-4">
          <p class="text-gray-700 mb-2">{{ selectedVideo.description }}</p>
          <div class="flex items-center gap-4 text-sm text-gray-500">
            <span v-if="selectedVideo.duration">时长: {{ selectedVideo.duration }}</span>
            <span v-if="selectedVideo.uploadDate">上传时间: {{ formatDate(selectedVideo.uploadDate) }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { apiClient } from '../services/axios'

const router = useRouter()
const route = useRoute()

// 课程和作业相关
const course = ref(null)
const loading = ref(true)
const error = ref(false)
const errorMessage = ref('')

// 教学视频相关
const teachingVideos = ref([])
const videosLoading = ref(false)
const videosError = ref(false)
const videosErrorMessage = ref('')
const showVideoModal = ref(false)
const selectedVideo = ref(null)

// 获取课程ID
const courseId = route.params.courseId || route.params.id

// 获取课程详情和作业列表
const fetchCourseDetails = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''

  try {
    // 获取JWT token
    const token = localStorage.getItem('token')
    if (!token) {
      throw new Error('未找到认证token，请重新登录')
    }

    // 调用get_info_by_course_id接口获取课程详情
    const courseResponse = await apiClient.post('/get_info_by_course_id', {
      course_id: courseId,
      jwt: token
    })

    if (courseResponse.data.code === 0 && courseResponse.data.data) {
      const courseData = courseResponse.data.data

      // 调用get_homework_id_by_course接口获取作业列表
      const homeworkResponse = await apiClient.post('/get_homework_id_by_course', {
        course_id: courseId,
        user_type: '0', // 学生
        jwt: token
      })

      let assignments = []
      if (homeworkResponse.data.code === 0 && homeworkResponse.data.data) {
        // 解析作业ID列表（用\t\r分隔）
        const homeworkIdList = homeworkResponse.data.data.split('\t\r').filter(id => id.trim())

        // 为每个作业ID获取作业详情
        const assignmentDetailsPromises = homeworkIdList.map(async (homeworkId) => {
          try {
            const assignmentResponse = await apiClient.post('/get_info_by_homework_id', {
              homework_id: homeworkId.trim(),
              jwt: token
            })

            if (assignmentResponse.data.code === 0 && assignmentResponse.data.data) {
              const assignmentData = assignmentResponse.data.data
              return {
                id: homeworkId.trim(),
                title: assignmentData.name || `作业 ${homeworkId.trim()}`,
                description: assignmentData.info || '暂无描述',
                deadline: assignmentData.deadline || '待定',
                create_time: assignmentData.create_time || '',
                course_id: courseId,
                subject: assignmentData.subject || '体育',
                status: assignmentData.is_active === '1' ? '进行中' : '未发布',
                points: assignmentData.points || 100
              }
            }
          } catch (error) {
            console.error(`获取作业 ${homeworkId} 详情失败:`, error)
            return {
              id: homeworkId.trim(),
              title: `作业 ${homeworkId.trim()}`,
              description: '暂无描述',
              deadline: '待定',
              create_time: '',
              course_id: courseId,
              subject: '体育',
              status: '进行中',
              points: 100
            }
          }
        })

        // 等待所有作业详情获取完成
        assignments = await Promise.all(assignmentDetailsPromises)
      }

      // 构造课程对象
      course.value = {
        id: courseId,
        name: courseData.name || '未命名课程',
        description: courseData.info || '暂无描述',
        subject: '体育',
        status: courseData.is_active === '1' ? '进行中' : '未发布',
        assignments: assignments
      }

      console.log('课程详情加载成功:', course.value)
    } else {
      throw new Error(courseResponse.data.message || '获取课程详情失败')
    }

    // 获取教学视频列表
    await fetchTeachingVideos()
  } catch (err) {
    console.error('获取课程详情失败:', err)
    error.value = true
    errorMessage.value = err.message

    // 如果API请求失败，使用默认的mock数据作为 fallback
    course.value = {
      id: courseId,
      name: '默认课程',
      description: '这是一个默认课程的描述。',
      subject: '体育',
      status: '进行中',
      assignments: []
    }
  } finally {
    loading.value = false
  }
}

// 获取教学视频列表
const fetchTeachingVideos = async () => {
  videosLoading.value = true
  videosError.value = false
  videosErrorMessage.value = ''

  try {
    // 获取JWT token
    const token = localStorage.getItem('token')
    if (!token) {
      throw new Error('未找到认证token，请重新登录')
    }

    // 调用获取教学视频的API接口
    const response = await apiClient.post('/get_teaching_videos', {
      course_id: courseId,
      jwt: token
    })

    if (response.data.code === 0 && response.data.data) {
      // 解析教学视频数据
      const videoData = response.data.data

      // 如果返回的是字符串（可能是视频ID列表），需要进一步处理
      if (typeof videoData === 'string') {
        // 假设返回的是视频ID列表，用\t\r分隔
        const videoIdList = videoData.split('\t\r').filter(id => id.trim())

        // 为每个视频ID获取视频详情
        const videoDetailsPromises = videoIdList.map(async (videoId) => {
          try {
            const videoResponse = await apiClient.post('/get_info_by_video_id', {
              video_id: videoId.trim(),
              jwt: token
            })

            if (videoResponse.data.code === 0 && videoResponse.data.data) {
              const videoInfo = videoResponse.data.data
              return {
                id: videoId.trim(),
                title: videoInfo.title || `教学视频 ${videoId.trim()}`,
                description: videoInfo.description || '暂无描述',
                url: videoInfo.url || '',
                duration: videoInfo.duration || '00:00',
                cover: videoInfo.cover || '',
                uploadDate: videoInfo.create_time || ''
              }
            }
          } catch (error) {
            console.error(`获取视频 ${videoId} 详情失败:`, error)
            return {
              id: videoId.trim(),
              title: `教学视频 ${videoId.trim()}`,
              description: '暂无描述',
              url: '',
              duration: '00:00',
              cover: '',
              uploadDate: ''
            }
          }
        })

        // 等待所有视频详情获取完成
        teachingVideos.value = await Promise.all(videoDetailsPromises)
      } else if (Array.isArray(videoData)) {
        // 如果返回的是视频数组，直接使用
        teachingVideos.value = videoData.map(video => ({
          id: video.id || video.video_id || '',
          title: video.title || '未命名视频',
          description: video.description || '暂无描述',
          url: video.url || '',
          duration: video.duration || '00:00',
          cover: video.cover || '',
          uploadDate: video.create_time || video.upload_date || ''
        }))
      } else {
        // 如果返回的是单个视频对象
        teachingVideos.value = [{
          id: videoData.id || videoData.video_id || '',
          title: videoData.title || '未命名视频',
          description: videoData.description || '暂无描述',
          url: videoData.url || '',
          duration: videoData.duration || '00:00',
          cover: videoData.cover || '',
          uploadDate: videoData.create_time || videoData.upload_date || ''
        }]
      }

      console.log('教学视频加载成功:', teachingVideos.value)
    } else {
      // 如果没有教学视频，设置为空数组
      teachingVideos.value = []
      console.log('该课程暂无教学视频')
    }
  } catch (err) {
    console.error('获取教学视频失败:', err)
    videosError.value = true
    videosErrorMessage.value = err.message

    // 如果API请求失败，设置为空数组
    teachingVideos.value = []
  } finally {
    videosLoading.value = false
  }
}

// 打开视频播放器
const openVideoPlayer = (video) => {
  selectedVideo.value = video
  showVideoModal.value = true
}

// 关闭视频播放器
const closeVideoPlayer = () => {
  showVideoModal.value = false
  selectedVideo.value = null
}

// 格式化日期
const formatDate = (dateString) => {
  if (!dateString || dateString === '待定') {
    return '待定'
  }

  const date = new Date(dateString)
  if (isNaN(date.getTime())) {
    return dateString
  }

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
  router.push('/student')
}

// 组件挂载时获取课程详情
onMounted(() => {
  fetchCourseDetails()
})
</script>
