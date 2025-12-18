<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-10">
      <!-- 顶部导航 -->
      <div class="flex justify-between items-center py-4">
        <div class="flex items-center gap-2">
          <button @click="goBack" class="text-2xl text-gray-600 hover:text-gray-800 transition-colors">
            ←
          </button>
          <h1 class="text-2xl font-bold text-gray-800">体育作业平台</h1>
        </div>
        <button @click="logout" class="px-4 py-2 rounded-xl bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
          退出登录
        </button>
      </div>

      <!-- 标题 + 课程筛选 -->
      <section class="flex flex-col md:flex-row md:justify-between md:items-center gap-6">
        <div>
          <h2 class="text-4xl font-bold text-gray-800 mb-2">🎥 教学视频</h2>
          <p class="text-gray-600">观看教师发布的体育教学视频，提升技能</p>
        </div>
        <select v-model="selectedCourse"
                class="px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 transition-all shadow-sm">
          <option value="all">所有课程</option>
          <option v-for="course in courses" :key="course.id" :value="course.id">
            {{ course.name }}
          </option>
        </select>
      </section>

      <!-- 视频网格 -->
      <section class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        <div v-for="video in filteredVideos"
             :key="video.id"
             class="bg-white rounded-3xl shadow-xl overflow-hidden transition-all hover:shadow-2xl cursor-pointer"
             @click="playVideo(video)">
          <div class="relative">
            <img :src="video.cover" class="w-full h-48 object-cover" alt="视频封面" />
            <div class="absolute inset-0 bg-black/30 flex items-center justify-center">
              <div class="w-16 h-16 rounded-full bg-white/90 flex items-center justify-center text-3xl text-blue-600">
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
            <h3 class="text-xl font-bold text-gray-800 mb-2">{{ video.title }}</h3>
            <p class="text-sm text-gray-600 mb-4 line-clamp-3">{{ video.description }}</p>
            <div class="text-sm text-gray-500">
              发布时间：{{ formatDate(video.createdAt) }}
            </div>
          </div>
        </div>
      </section>

      <!-- 空状态 -->
      <section v-if="filteredVideos.length === 0" class="bg-white rounded-3xl shadow-xl p-16 text-center">
        <div class="text-6xl text-gray-300 mb-4">📹</div>
        <h3 class="text-xl font-bold text-gray-800 mb-2">暂无教学视频</h3>
        <p class="text-gray-500">教师尚未发布视频，请耐心等待～</p>
      </section>

      <!-- 全屏视频播放器 -->
      <div v-if="playingVideo" class="fixed inset-0 bg-black z-50 flex items-center justify-center p-8">
        <div class="relative w-full max-w-5xl">
          <button @click="playingVideo = null" class="absolute -top-12 right-0 text-white text-3xl hover:text-gray-300">
            ✕
          </button>
          <video :src="playingVideo.url" controls autoplay class="w-full rounded-2xl shadow-2xl">
            您的浏览器不支持视频播放
          </video>
          <div class="mt-6 text-white">
            <h3 class="text-2xl font-bold mb-2">{{ playingVideo.title }}</h3>
            <p class="text-lg opacity-90">{{ playingVideo.description }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

// 模拟数据（实际调用 get_class_id_by_course + get_info_by_class_id）
const courses = ref([
  { id: 'PE2025-01', name: '初三（1）班 体育' },
  { id: 'PE2025-02', name: '初三（2）班 田径专项' },
  { id: 'PE2025-03', name: '游泳选修' },
])

const allVideos = ref([
  {
    id: 1,
    courseId: 'PE2025-01',
    title: '50米折返跑标准动作示范',
    description: '详细讲解折返跑的起跑、转体、冲刺技巧，帮助你跑出更好成绩',
    cover: 'https://images.unsplash.com/photo-1570545887596-2a6c5cbcf9c3?w=800',
    url: 'https://example.com/video1.mp4',
    duration: '06:42',
    createdAt: '2025-12-01'
  },
  // ...更多
])

const selectedCourse = ref('all')
const playingVideo = ref(null)

const filteredVideos = computed(() => {
  if (selectedCourse.value === 'all') return allVideos.value
  return allVideos.value.filter(v => v.courseId === selectedCourse.value)
})

const getCourseName = (courseId) => {
  const c = courses.value.find(item => item.id === courseId)
  return c ? c.name : '未知课程'
}

const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' })
}

const playVideo = (video) => {
  playingVideo.value = video
}

const goBack = () => router.push('/student')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}
</script>
