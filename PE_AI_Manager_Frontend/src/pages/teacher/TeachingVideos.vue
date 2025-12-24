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
      <div v-else>
        <!-- 页面标题和发布按钮 + 课程筛选 -->
        <section class="flex flex-col md:flex-row md:justify-between md:items-center gap-6">
          <div>
            <h2 class="text-4xl font-bold text-gray-800 mb-2">🎥 教学视频管理</h2>
            <p class="text-gray-600">发布和管理体育教学视频</p>
          </div>
          <div class="flex items-center gap-4">
            <!-- 课程筛选 -->
            <select v-model="selectedCourseFilter"
                    @change="loadVideos"
                    class="px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm">
              <option value="all">所有课程</option>
              <option v-for="course in courses" :key="course.id" :value="course.id">
                {{ course.name }}
              </option>
            </select>
            <button @click="showUploadModal = true"
                    class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
              + 发布新视频
            </button>
          </div>
        </section>

        <!-- 视频列表 -->
        <section class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          <div v-for="video in filteredVideos"
               :key="video.id"
               class="bg-white rounded-3xl shadow-xl overflow-hidden transition-all hover:shadow-2xl">
            <!-- 视频封面 -->
            <div class="relative">
              <img :src="video.cover || defaultCover" class="w-full h-48 object-cover" />
              <div class="absolute inset-0 bg-black/20 flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity">
                <div class="w-16 h-16 rounded-full bg-white/80 flex items-center justify-center text-2xl text-blue-500 hover:scale-110 transition-transform">
                  ▶️
                </div>
              </div>
              <div class="absolute bottom-3 right-3 bg-black/70 text-white text-xs px-2 py-1 rounded-lg">
                {{ video.duration || '未知' }}
              </div>
              <div class="absolute top-3 left-3 bg-blue-600 text-white text-xs px-2 py-1 rounded-lg">
                {{ getCourseName(video.courseId) }}
              </div>
            </div>
            <!-- 视频信息 -->
            <div class="p-6">
              <h3 class="text-xl font-bold text-gray-800 mb-2">{{ video.title }}</h3>
              <p class="text-sm text-gray-600 mb-4 line-clamp-2">{{ video.description }}</p>
              <div class="flex justify-between items-center">
                <div class="text-sm text-gray-500">
                  {{ formatDate(video.createdAt) }}
                </div>
                <div class="flex gap-2">
                  <button @click="openEditModal(video)" class="text-blue-500 hover:text-blue-700 transition-colors">
                    ✏️
                  </button>
                  <button @click="deleteVideo(video.id)" class="text-red-500 hover:text-red-700 transition-colors">
                    🗑️
                  </button>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- 空状态 -->
        <section v-if="filteredVideos.length === 0" class="bg-white rounded-3xl shadow-xl p-16 text-center">
          <div class="text-6xl text-gray-300 mb-4">📹</div>
          <h3 class="text-xl font-bold text-gray-800 mb-2">暂无教学视频</h3>
          <p class="text-gray-500 mb-6">当前筛选条件下还没有视频</p>
          <button @click="showUploadModal = true"
                  class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg">
            + 发布第一个视频
          </button>
        </section>
      </div>

      <!-- 视频上传模态框 -->
      <div v-if="showUploadModal" class="fixed inset-0 bg-black/50 flex items-center justify-center p-6 z-50">
        <div class="bg-white rounded-3xl shadow-2xl w-full max-w-3xl max-h-[90vh] overflow-y-auto">
          <!-- 模态框头部 -->
          <div class="flex justify-between items-center p-8 border-b border-gray-200">
            <h3 class="text-2xl font-bold text-gray-800">发布教学视频</h3>
            <button @click="closeModal" class="text-2xl text-gray-400 hover:text-gray-600 transition-colors">
              ✕
            </button>
          </div>

          <!-- 模态框内容 -->
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
              <!-- 视频封面上传 -->
              <div class="mb-6">
                <label class="block text-sm font-medium text-gray-700 mb-2">视频封面</label>
                <div
                  class="border-2 border-dashed rounded-2xl p-6 text-center transition-all hover:bg-gray-50 cursor-pointer"
                  @click="triggerCoverInput"
                >
                  <div v-if="videoForm.cover" class="relative">
                    <img :src="videoForm.cover" class="w-full max-h-48 object-cover rounded-xl mb-2" />
                    <button
                      type="button"
                      @click.stop="removeCover"
                      class="absolute top-2 right-2 bg-red-500 text-white p-1 rounded-full hover:bg-red-600 transition-all"
                    >
                      ✕
                    </button>
                  </div>
                  <div v-else>
                    <div class="text-4xl text-gray-300 mb-2">🖼️</div>
                    <p class="text-sm text-gray-500">点击上传封面图片</p>
                  </div>
                  <input
                    ref="coverInput"
                    type="file"
                    accept="image/*"
                    class="hidden"
                    @change="handleCoverChange"
                  />
                </div>
              </div>

              <!-- 视频标题 -->
              <div class="mb-6">
                <label for="title" class="block text-sm font-medium text-gray-700 mb-2">视频标题</label>
                <input
                  id="title"
                  v-model="videoForm.title"
                  type="text"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                  placeholder="例如：50米折返跑技巧教学"
                  required
                />
              </div>

              <!-- 视频描述 -->
              <div class="mb-6">
                <label for="description" class="block text-sm font-medium text-gray-700 mb-2">视频描述</label>
                <textarea
                  id="description"
                  v-model="videoForm.description"
                  rows="4"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                  placeholder="详细描述视频内容和教学要点"
                  required
                ></textarea>
              </div>

              <!-- 视频URL -->
              <div class="mb-6">
                <label for="url" class="block text-sm font-medium text-gray-700 mb-2">视频URL</label>
                <input
                  id="url"
                  v-model="videoForm.url"
                  type="text"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                  placeholder="https://example.com/video.mp4"
                  required
                />
              </div>

              <!-- 视频时长 -->
              <div class="mb-6">
                <label for="duration" class="block text-sm font-medium text-gray-700 mb-2">视频时长</label>
                <input
                  id="duration"
                  v-model="videoForm.duration"
                  type="text"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                  placeholder="例如：05:23"
                  required
                />
              </div>

              <!-- 提交按钮 -->
              <div class="flex gap-4 justify-end">
                <button
                  type="button"
                  @click="closeModal"
                  class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow"
                >
                  取消
                </button>
                <button
                  type="submit"
                  class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg"
                >
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
import { useRouter } from 'vue-router'
import apiClient from '../../services/axios.js'

const router = useRouter()

const courses = ref([])
const videos = ref([])
const selectedCourseFilter = ref('all')
const showUploadModal = ref(false)
const isEditMode = ref(false)  // 是否为编辑模式
const editingVideoId = ref('')  // 当前编辑的视频ID

const loading = ref(true)
const errorMsg = ref('')

const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.jwt || 'valid_teacher_jwt'

const defaultCover = 'https://images.unsplash.com/photo-1570545887596-2a6c5cbcf9c3?w=800'

const videoForm = ref({
  courseId: '',
  title: '',
  description: '',
  url: '',
  cover: '',
  duration: ''
})

const coverInput = ref(null)

const filteredVideos = computed(() => {
  if (selectedCourseFilter.value === 'all') return videos.value
  return videos.value.filter(v => v.courseId === selectedCourseFilter.value)
})

const loadData = async () => {
  loading.value = true
  errorMsg.value = ''

  try {
    // 获取教师课程
    const resp = await apiClient.post('/api/get_course_id_by_teacher', {
      teacher_id: teacherId,
      jwt: jwt
    })

    if (resp.data[0] < 0) {
      errorMsg.value = '获取课程失败'
      return
    }

    const courseIds = resp.data[0].split('\t\r').filter(Boolean)

    const promises = courseIds.map(id => apiClient.post('/api/get_info_by_course_id', { course_id: id }))
    const resps = await Promise.all(promises)

    courses.value = resps.map((r, i) => ({
      id: courseIds[i],
      name: r.data[1]
    }))

    await loadVideos()

  } catch (err) {
    errorMsg.value = '加载失败'
  } finally {
    loading.value = false
  }
}

const loadVideos = async () => {
  videos.value = []

  const targetIds = selectedCourseFilter.value === 'all' ? courses.value.map(c => c.id) : [selectedCourseFilter.value]

  for (const courseId of targetIds) {
    const resp = await apiClient.post('/api/get_class_id_by_course', {
      user_type: '1',
      user_id: teacherId,
      jwt: jwt,
      course_id: courseId
    })

    if (resp.data[0] < 0) continue

    const classIds = resp.data[0].split('\t\r').filter(Boolean)

    for (const classId of classIds) {
      const infoResp = await apiClient.post('/api/get_info_by_class_id', {
        course_id: courseId,
        class_id: classId
      })

      if (infoResp.data[0] < 0) continue

      const d = infoResp.data
      videos.value.push({
        id: classId,
        courseId: courseId,
        title: d[0],
        description: d[1],
        url: d[2],
        cover: defaultCover,
        duration: '未知',
        createdAt: d[3]
      })
    }
  }
}

// 打开新增模态框
const openAddModal = () => {
  isEditMode.value = false
  editingVideoId.value = ''
  videoForm.value = {
    courseId: courses.value[0]?.id || '',
    title: '',
    description: '',
    url: '',
    cover: '',
    duration: ''
  }
  showUploadModal.value = true
}

// 打开编辑模态框
const openEditModal = (video) => {
  isEditMode.value = true
  editingVideoId.value = video.id
  videoForm.value = {
    courseId: video.courseId,
    title: video.title,
    description: video.description,
    url: video.url,
    cover: video.cover,
    duration: video.duration
  }
  showUploadModal.value = true
}

// 关闭模态框
const closeModal = () => {
  showUploadModal.value = false
  // 可选：也清空表单，避免下次打开残留
  videoForm.value = { courseId: '', title: '', description: '', url: '', cover: '', duration: '' }
  if (coverInput.value) coverInput.value.value = ''
}

// 提交（新增或编辑）
const submitVideo = async () => {
  if (!videoForm.value.courseId || !videoForm.value.title || !videoForm.value.description || !videoForm.value.url) {
    alert('请填写必填项')
    return
  }

  const payload = {
    teacher_id: teacherId,
    jwt: jwt,
    course_id: videoForm.value.courseId,
    title: videoForm.value.title,
    description: videoForm.value.description,
    content_url: videoForm.value.url
  }

  try {
    const url = isEditMode.value ? '/api/edit_class' : '/api/add_class'
    if (isEditMode.value) {
      payload.class_id = editingVideoId.value
    }

    const resp = await apiClient.post(url, payload)

    if (resp.data[0] !== 0) {
      alert(isEditMode.value ? '修改失败' : '发布失败')
      return
    }

    alert(isEditMode.value ? '修改成功！' : '发布成功！')
    showUploadModal.value = false
    await loadVideos()
  } catch (err) {
    alert('网络错误')
  }
}

const deleteVideo = async (classId) => {
  if (!confirm('确定删除此视频吗？')) return

  const video = videos.value.find(v => v.id === classId)
  if (!video) return

  try {
    const resp = await apiClient.post('/api/delete_class', {
      teacher_id: teacherId,
      jwt: jwt,
      course_id: video.courseId,
      class_id: classId
    })

    if (resp.data[0] !== 0) {
      alert('删除失败')
      return
    }

    alert('删除成功')
    await loadVideos()
  } catch (err) {
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

const triggerCoverInput = () => coverInput.value?.click()
const handleCoverChange = (e) => {
  const file = e.target.files[0]
  if (file) {
    const reader = new FileReader()
    reader.onload = (ev) => videoForm.value.cover = ev.target.result
    reader.readAsDataURL(file)
  }
}
const removeCover = () => videoForm.value.cover = ''

onMounted(loadData)
</script>
