<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-10">

      <!-- 加载状态 -->
      <div v-if="loading" class="text-center py-32">
        <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-4 border-b-4 border-blue-500"></div>
        <p class="mt-6 text-xl text-gray-600">加载中...</p>
      </div>

      <!-- 错误状态 -->
      <div v-else-if="errorMsg" class="text-center py-32">
        <p class="text-2xl text-red-600 mb-6">{{ errorMsg }}</p>
        <button @click="loadData" class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
          重试
        </button>
      </div>

      <!-- 正常内容 -->
      <div v-else class="space-y-12">  <!-- ⭐ 关键：大间距分隔区 -->

        <!-- 标题 + 操作栏 -->
        <section class="bg-white rounded-3xl shadow-lg p-8">
          <div class="flex flex-col md:flex-row md:justify-between md:items-center gap-6">
            <div>
              <h2 class="text-4xl font-bold text-gray-800 mb-2">🎥 教学视频管理</h2>
              <p class="text-gray-600">发布和管理体育教学视频</p>
            </div>
            <div class="flex items-center gap-4">
              <select v-model="selectedCourseFilter"
                      @change="loadVideos"
                      class="px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm">
                <option value="all">所有课程</option>
                <option v-for="course in courses" :key="course.id" :value="course.id">
                  {{ course.name }}
                </option>
              </select>
              <button @click="openAddModal"
                      class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
                + 发布新视频
              </button>
            </div>
          </div>
        </section>

        <!-- 视频内容区 -->
        <section class="bg-white rounded-3xl shadow-lg p-8">
          <h3 class="text-2xl font-bold text-gray-800 mb-8">
            视频列表
            <span class="text-lg font-normal text-gray-500 ml-3">
              (共 {{ filteredVideos.length }} 个)
            </span>
          </h3>

          <!-- 有视频：网格列表 -->
          <div v-if="filteredVideos.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            <div v-for="video in filteredVideos"
                 :key="video.id"
                 @click="openPlayDialog(video)"
                 class="bg-gray-50 rounded-2xl overflow-hidden transition-all hover:shadow-xl hover:-translate-y-1 cursor-pointer">
              <!-- 卡片内容保持不变 -->
              <div class="relative">
                <img :src="video.cover" class="w-full h-48 object-cover" alt="视频封面" />
                <div class="absolute inset-0 bg-black/20 flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity">
                  <div class="w-16 h-16 rounded-full bg-white/90 flex items-center justify-center text-3xl">
                    ▶️
                  </div>
                </div>
                <div class="absolute bottom-3 right-3 bg-black/70 text-white text-xs px-2 py-1 rounded-lg">
                  {{ video.duration }}
                </div>
                <div class="absolute top-3 left-3 bg-blue-600 text-white text-xs px-2 py-1 rounded-lg">
                  {{ getCourseName(video.courseId) }}
                </div>
              </div>
              <div class="p-6">
                <h4 class="text-lg font-bold text-gray-800 mb-2 line-clamp-1">{{ video.title }}</h4>
                <p class="text-sm text-gray-600 mb-4 line-clamp-2">{{ video.description }}</p>
                <div class="flex justify-between items-center text-sm">
                  <span class="text-gray-500">{{ formatDate(video.createdAt) }}</span>
                  <div class="flex gap-3">
                    <button @click.stop="openEditModal(video)" class="text-blue-500 hover:text-blue-700">✏️</button>
                    <button @click.stop="deleteVideo(video.id)" class="text-red-500 hover:text-red-700">🗑️</button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 无视频：空状态 -->
          <div v-else class="text-center py-16">
            <div class="text-8xl text-gray-200 mb-6">📹</div>
            <h4 class="text-2xl font-bold text-gray-700 mb-3">暂无教学视频</h4>
            <p class="text-gray-500 mb-8 max-w-md mx-auto">
              当前筛选条件下还没有发布任何视频，赶紧添加第一个吧！
            </p>
            <button @click="openAddModal"
                    class="px-8 py-4 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg text-lg">
              + 发布第一个视频
            </button>
          </div>
        </section>
      </div>


      <!-- 弹窗播放器 -->
      <VideoDialogPlayer
        v-model:visible="playDialogVisible"
        :video-url="currentPlayUrl"
        :title="currentPlayTitle"
      />

      <!-- 发布/编辑模态框 -->
      <div v-if="showUploadModal" class="fixed inset-0 bg-black/50 flex items-center justify-center p-6 z-50">
        <div class="bg-white rounded-3xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-y-auto">
          <div class="flex justify-between items-center p-8 border-b border-gray-200">
            <h3 class="text-2xl font-bold text-gray-800">
              {{ isEditMode ? '编辑教学视频' : '发布教学视频' }}
            </h3>
            <button @click="closeModal" class="text-2xl text-gray-400 hover:text-gray-600 transition-colors">
              ✕
            </button>
          </div>

          <div class="p-8">
            <form @submit.prevent="submitVideo">
              <!-- 所属课程 -->
              <div class="mb-6">
                <label class="block text-sm font-medium text-gray-700 mb-2">所属课程 <span class="text-red-500">*</span></label>
                <select v-model="videoForm.courseId"
                        class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm"
                        required>
                  <option value="" disabled>请选择课程</option>
                  <option v-for="course in courses" :key="course.id" :value="course.id">
                    {{ course.name }}
                  </option>
                </select>
              </div>

              <!-- 编辑时预览当前视频（默认暂停） -->
              <div v-if="isEditMode && videoForm.url" class="mb-8">
                <p class="text-sm text-gray-500 mt-3">重新上传将替换当前视频</p>
              </div>

              <!-- 视频上传 -->
              <div class="mb-6">
                <label class="block text-sm font-medium text-gray-700 mb-2">
                  {{ isEditMode ? '替换视频（可选）' : '上传视频' }} <span class="text-red-500">*</span>
                </label>
                <FileUploader max-width="100%" :max-file-size="2048" @uploaded="onVideoUploaded" />
              </div>

              <!-- 视频标题 -->
              <div class="mb-6">
                <label class="block text-sm font-medium text-gray-700 mb-2">视频标题 <span class="text-red-500">*</span></label>
                <input v-model="videoForm.title"
                       type="text"
                       class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm"
                       placeholder="例如：50米折返跑技巧教学"
                       required />
              </div>

              <!-- 视频描述 -->
              <div class="mb-6">
                <label class="block text-sm font-medium text-gray-700 mb-2">视频描述 <span class="text-red-500">*</span></label>
                <textarea v-model="videoForm.description"
                          rows="4"
                          class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm"
                          placeholder="详细描述视频内容和教学要点"
                          required></textarea>
              </div>

              <!-- 提交按钮 -->
              <div class="flex gap-4 justify-end">
                <button type="button"
                        @click="closeModal"
                        class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow">
                  取消
                </button>
                <button type="submit"
                        :disabled="!videoForm.url"
                        class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed">
                  {{ isEditMode ? '保存修改' : '发布视频' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import apiClient from '../../services/axios.js'
import FileUploader from '@/components/FileUploader.vue'
import VideoDialogPlayer from '@/components/VideoDialogPlayer.vue'

const courses = ref([])
const videos = ref([])
const selectedCourseFilter = ref('all')
const showUploadModal = ref(false)
const isEditMode = ref(false)
const editingVideoId = ref('')

const playDialogVisible = ref(false)
const currentPlayUrl = ref('')
const currentPlayTitle = ref('')

const loading = ref(true)
const errorMsg = ref('')

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.token || ''

const defaultCover = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8Xw8AAoMBgDTDzwAAAABJRU5ErkJggg=='
const videoForm = ref({
  courseId: '',
  title: '',
  description: '',
  url: ''
})

const filteredVideos = computed(() => {
  if (selectedCourseFilter.value === 'all') return videos.value
  return videos.value.filter(v => v.courseId === selectedCourseFilter.value)
})

// 路径修正：将完整URL转换为相对路径
const getPlayUrl = (url) => {
  if (!url) return ''
  if (url.startsWith('http://47.121.177.100:5002')) {
    let filename = ''
    if (url) {
      const lastSlashIndex = url.lastIndexOf('/')
      if (lastSlashIndex !== -1) {
        filename = url.substring(lastSlashIndex + 1)
      } else {
        filename = url
      }
    }
    return `/Teaching-video/files/${filename}`
  }
  return url
}

// 点击卡片弹窗播放
const openPlayDialog = (video) => {
  currentPlayUrl.value = getPlayUrl(video.url)
  currentPlayTitle.value = video.title
  playDialogVisible.value = true
}

// 动态生成封面和时长
const generateVideoMeta = (url, callback) => {
  if (!url) return callback(defaultCover, '未知')

  const video = document.createElement('video')
  video.src = getPlayUrl(url)
  video.crossOrigin = 'anonymous'

  let cover = defaultCover
  let duration = '未知'

  video.onloadedmetadata = () => {
    const mins = Math.floor(video.duration / 60).toString().padStart(2, '0')
    const secs = Math.floor(video.duration % 60).toString().padStart(2, '0')
    duration = `${mins}:${secs}`

    // 截取第1秒作为封面
    video.currentTime = Math.min(1, video.duration)
  }

  video.onseeked = () => {
    const canvas = document.createElement('canvas')
    canvas.width = video.videoWidth
    canvas.height = video.videoHeight
    const ctx = canvas.getContext('2d')
    ctx.drawImage(video, 0, 0, canvas.width, canvas.height)
    cover = canvas.toDataURL('image/jpeg')
    callback(cover, duration)
  }

  video.onerror = () => callback(defaultCover, '未知')
}

// 上传成功回调
const onVideoUploaded = (result) => {
  if (result && result.url) {
    // 将完整URL转换为相对路径存储
    videoForm.value.url = getPlayUrl(result.url)
    ElMessage.success('视频上传成功！封面和时长加载中...')

    // 立即生成封面和时长（仅用于当前表单预览）
    generateVideoMeta(result.url, (newCover, newDuration) => {
      ElMessage.success('封面和时长已生成')
    })
  }
}

const openAddModal = () => {
  isEditMode.value = false
  editingVideoId.value = ''
  videoForm.value = {
    courseId: courses.value[0]?.id || '',
    title: '',
    description: '',
    url: ''
  }
  showUploadModal.value = true
}

const openEditModal = (video) => {
  isEditMode.value = true
  editingVideoId.value = video.id
  videoForm.value = {
    courseId: video.courseId,
    title: video.title,
    description: video.description,
    url: video.url
  }
  showUploadModal.value = true
}

const closeModal = () => {
  showUploadModal.value = false
  videoForm.value = { courseId: '', title: '', description: '', url: '' }
}

const submitVideo = async () => {
  if (!videoForm.value.courseId || !videoForm.value.title ||
      !videoForm.value.description || !videoForm.value.url) {
    alert('请填写所有必填项并上传视频')
    return
  }

  try {
    let resp
    if (isEditMode.value) {
      resp = await apiClient.post('/Class/edit_class', {
        first: teacherId,
        second: jwt,
        third: videoForm.value.courseId,
        fourth: editingVideoId.value,
        fifth: videoForm.value.title,
        sixth: videoForm.value.description,
        seventh: videoForm.value.url
      })
    } else {
      resp = await apiClient.post('/Class/new_class', {
        first: teacherId,
        second: jwt,
        third: videoForm.value.courseId,
        fourth: videoForm.value.title,
        fifth: videoForm.value.description,
        sixth: videoForm.value.url
      })
    }

    if (resp.data.success) {
      ElMessage.success(isEditMode.value ? '修改成功！' : '发布成功！')
      closeModal()
      await loadVideos()
    } else {
      alert(isEditMode.value ? '修改失败' : '发布失败')
    }
  } catch (err) {
    console.error(err)
    alert('网络错误，请重试')
  }
}

const deleteVideo = async (classId) => {
  if (!confirm('确定删除此教学视频吗？删除后不可恢复')) return

  const video = videos.value.find(v => v.id === classId)
  if (!video) return

  try {
    const resp = await apiClient.post('/Class/delete_class', {
      first: teacherId,
      second: jwt,
      third: video.courseId,
      fourth: classId
    })

    if (resp.data.success) {
      ElMessage.success('删除成功')
      await loadVideos()
    } else {
      alert('删除失败')
    }
  } catch (err) {
    console.error(err)
    alert('网络错误')
  }
}

const getCourseName = (courseId) => {
  const c = courses.value.find(item => item.id === courseId)
  return c ? c.name : '未知课程'
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' })
}

const loadData = async () => {
  loading.value = true
  errorMsg.value = ''

  try {
    const resp = await apiClient.post('/Course/get_course_id_by_teacher', {
      first: teacherId,
      second: jwt
    })

    if (!resp.data.success) {
      errorMsg.value = '获取课程失败'
      return
    }

    const courseIds = resp.data.data.trim().replace(/\t\r$/g, '').split('\t\r').filter(Boolean)

    const promises = courseIds.map(id =>
      apiClient.post('/Course/get_info_by_course_id', { first: id })
    )
    const resps = await Promise.all(promises)

    courses.value = resps
      .filter(r => r.data.success)
      .map((r, i) => ({
        id: courseIds[i],
        name: r.data.data.trim().replace(/\t\r$/g, '').split('\t\r').filter(Boolean)[1] || '未知课程'
      }))

    await loadVideos()
  } catch (err) {
    console.error(err)
    errorMsg.value = '加载失败，请检查网络或登录状态'
  } finally {
    loading.value = false
  }
}

const loadVideos = async () => {
  const tempVideos = []

  const targetIds = selectedCourseFilter.value === 'all'
    ? courses.value.map(c => c.id)
    : [selectedCourseFilter.value]

  for (const courseId of targetIds) {
    const resp = await apiClient.post('/Class/get_class_id_by_course', {
      first: '1',
      second: teacherId,
      third: jwt,
      fourth: courseId
    })

    if (!resp.data.success) continue

    const classIds = resp.data.data.trim().replace(/\t\r$/g, '').split('\t\r').filter(Boolean)

    const infoPromises = classIds.map(classId =>
      apiClient.post('/Class/get_info_by_class_id', {
        first: courseId,
        second: classId
      })
    )
    const infoResps = await Promise.all(infoPromises)

    infoResps.forEach((infoResp, idx) => {
      if (infoResp.data.success) {
        const d = infoResp.data.data.trim().replace(/\t\r$/g, '').split('\t\r').filter(Boolean)
        tempVideos.push({
          id: classIds[idx],
          courseId: courseId,
          title: d[0] || '无标题',
          description: d[1] || '暂无描述',
          url: getPlayUrl(d[2] || ''),
          cover: defaultCover,  // 占位
          duration: '加载中...',
          createdAt: d[3] || ''
        })
      }
    })
  }

  videos.value = tempVideos

  // 动态计算每个视频的封面和时长
  videos.value.forEach(video => {
    if (video.url) {
      video.cover = defaultCover
      generateVideoMeta(video.url, (cover, duration) => {
        video.cover = cover
        video.duration = duration
      })
    }
  })
}

onMounted(() => {
  if (!teacherId || !jwt) {
    errorMsg.value = '请先登录'
    loading.value = false
    return
  }
  loadData()
})
</script>
