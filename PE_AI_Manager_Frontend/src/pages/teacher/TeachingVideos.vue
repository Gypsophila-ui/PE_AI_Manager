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

      <!-- 页面标题和发布按钮 + 课程筛选 -->
      <section class="flex flex-col md:flex-row md:justify-between md:items-center gap-6">
        <div>
          <h2 class="text-4xl font-bold text-gray-800 mb-2">🎥 教学视频管理</h2>
          <p class="text-gray-600">发布和管理体育教学视频</p>
        </div>
        <div class="flex items-center gap-4">
          <!-- 课程筛选 -->
          <select v-model="selectedCourseFilter"
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
        <div
          v-for="video in filteredVideos"
          :key="video.id"
          class="bg-white rounded-3xl shadow-xl overflow-hidden transition-all hover:shadow-2xl"
        >
          <!-- 视频封面 -->
          <div class="relative">
            <img :src="video.cover" class="w-full h-48 object-cover" />
            <div class="absolute inset-0 bg-black/20 flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity">
              <div class="w-16 h-16 rounded-full bg-white/80 flex items-center justify-center text-2xl text-blue-500 hover:scale-110 transition-transform">
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

          <!-- 视频信息 -->
          <div class="p-6">
            <h3 class="text-xl font-bold text-gray-800 mb-2">{{ video.title }}</h3>
            <p class="text-sm text-gray-600 mb-4 line-clamp-2">{{ video.description }}</p>
            <div class="flex justify-between items-center">
              <div class="text-sm text-gray-500">
                {{ formatDate(video.createdAt) }}
              </div>
              <div class="flex gap-2">
                <button @click="editVideo(video.id)" class="text-blue-500 hover:text-blue-700 transition-colors">
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
        <button
          @click="showUploadModal = true"
          class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg"
        >
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
          <button @click="showUploadModal = false" class="text-2xl text-gray-400 hover:text-gray-600 transition-colors">
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
                @click="showUploadModal = false"
                class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow"
              >
                取消
              </button>
              <button
                type="submit"
                class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg"
              >
                发布视频
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

// 模拟数据（实际从后端获取）
const courses = ref([
  { id: 'PE2025-01', name: '初三（1）班 体育' },
  { id: 'PE2025-02', name: '初三（2）班 田径专项' },
  { id: 'PE2025-03', name: '游泳选修' },
])

const videos = ref([
  {
    id: 1,
    courseId: 'PE2025-01',
    title: '50米折返跑标准动作示范',
    description: '详细讲解折返跑的起跑、转体、冲刺技巧',
    cover: 'https://images.unsplash.com/photo-1570545887596-2a6c5cbcf9c3?w=800',
    url: 'https://example.com/video1.mp4',
    duration: '06:42',
    createdAt: '2025-12-01'
  },
  // ...更多视频
])

const router = useRouter()

// 教学视频数据
// const videos = ref([...teachingVideos])

// 上传模态框
const showUploadModal = ref(false)
const selectedCourseFilter = ref('all')

// 文件上传相关
const coverInput = ref(null)
// 过滤视频
const filteredVideos = computed(() => {
  if (selectedCourseFilter.value === 'all') return videos.value
  return videos.value.filter(v => v.courseId === selectedCourseFilter.value)
})

// 视频表单数据
const videoForm = ref({
  courseId: '',
  title: '',
  description: '',
  url: '',
  cover: '',
  duration: ''
})

// 触发封面上传
const triggerCoverInput = () => {
  coverInput.value.click()
}

// 处理封面上传
const handleCoverChange = (event) => {
  const file = event.target.files[0]
  if (file) {
    const reader = new FileReader()
    reader.onload = (e) => {
      videoForm.value.cover = e.target.result
    }
    reader.readAsDataURL(file)
  }
}

// 移除封面
const removeCover = () => {
  videoForm.value.cover = ''
  if (coverInput.value) {
    coverInput.value.value = ''
  }
}

// 根据课程ID获取课程名称
const getCourseName = (courseId) => {
  const course = courses.value.find(c => c.id === courseId)
  return course ? course.name : '未知课程'
}

// 提交视频
const submitVideo = () => {
  // 验证表单
  if (!videoForm.value.courseId || !videoForm.value.title || !videoForm.value.description || !videoForm.value.url || !videoForm.value.cover || !videoForm.value.duration) {
    alert('请填写完整信息')
    return
  }

  // 模拟发布视频
  const newVideo = {
    id: Date.now(),
    ...videoForm.value,
    createdAt: new Date().toISOString()
  }

  videos.value.unshift(newVideo)
  showUploadModal.value = false
  videoForm.value = { courseId: '', title: '', description: '', url: '', cover: '', duration: '' }
  alert('视频发布成功！')
}

// 编辑视频
const editVideo = (videoId) => {
  console.log('编辑视频:', videoId)
  // 这里可以实现编辑功能
}

// 删除视频
const deleteVideo = (videoId) => {
  if (confirm('确定要删除这个视频吗？')) {
    const index = videos.value.findIndex(v => v.id === videoId)
    if (index > -1) {
      videos.value.splice(index, 1)
      alert('视频删除成功！')
    }
  }
}

// 格式化日期
const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
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
