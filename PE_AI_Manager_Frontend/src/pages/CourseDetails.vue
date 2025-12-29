<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-8">
      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">课程详情</h2>
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
                <span class="text-gray-400">学科:</span>
                <span>{{ course.subject }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">教师:</span>
                <span>{{ course.teacherName }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">状态:</span>
                <span>{{ course.status }}</span>
              </div>
              <div class="flex items-center gap-2 text-gray-600">
                <span class="text-gray-400">作业:</span>
                <span>{{ course.assignments.length }} 个</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- 教学视频列表 -->
      <section v-if="course" class="bg-white rounded-3xl shadow-xl p-6">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-2xl font-bold text-gray-800">教学视频</h3>
          <div class="flex items-center gap-4">
            <span v-if="teachingVideos.length > 0" class="text-sm text-gray-500">{{ teachingVideos.length }} 个视频</span>
            <router-link :to="`/student/course/${courseId}/teaching-videos`"
                        class="text-sm text-blue-500 hover:text-blue-700 font-medium flex items-center gap-1">
              查看全部
            </router-link>
          </div>
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
               @click="goToTeachingVideos">
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
              <router-link :to="`/student/course/${course.id}/assignments/${assignment.id}`"
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


  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { apiClient } from '../services/axios'
import StudentAssignments from './student/StudentAssignments.vue'

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

// 获取课程ID
const courseId = route.params.courseId || route.params.id

// 获取课程详情和作业列表
const fetchCourseDetails = async () => {
  loading.value = true
  error.value = false
  errorMessage.value = ''
  const Student_id = localStorage.getItem('user')
  try {
    // 获取JWT token
    const token = localStorage.getItem('token')
    if (!token) {
      throw new Error('未找到认证token，请重新登录')
    }

    // 获取当前用户信息
    const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
    const studentId = currentUser ? currentUser.id : 'student1'

    // 调用get_info_by_course_id接口获取课程详情
    const courseResponse = await apiClient.post('/Course/get_info_by_course_id', {
      first: courseId,
    })

    if (courseResponse.data.success && courseResponse.data.data) {
      // 解析课程数据字符串，格式为: 教师id\t\r课程名字\t\r课程描述\t\r课程码\t\r课程所在学期\t\r课程是否正在进行(1是0否)\t\r课程创建时间
      const courseDataArray = courseResponse.data.data.split('\t\r');

      if (courseDataArray.length >= 7) {
        const [
          teacherId,      // 教师ID
          courseName,     // 课程名字
          courseDescription, // 课程描述
          courseCode,     // 课程码
          courseTerm,     // 课程所在学期
          isActive,       // 课程是否正在进行 (1是 0否)
          createTime     // 课程创建时间
        ] = courseDataArray;

        // 调用get_homework_id_by_course接口获取作业列表
        const homeworkResponse = await apiClient.post('/Homework/get_homework_id_by_course', {
          first: '0', // 学生
          second: studentId ,
          third: token,
          fourth:courseId
        })

        let assignments = []
        if (homeworkResponse.data.success && homeworkResponse.data.data) {
          // 解析作业ID列表（用\t\r分隔）
          const homeworkIdList = homeworkResponse.data.data.split('\t\r').filter(id => id.trim())

          // 为每个作业ID获取作业详情
          const assignmentDetailsPromises = homeworkIdList.map(async (homeworkId) => {
            try {
              const assignmentResponse = await apiClient.post('/Homework/get_info_by_homework_id', {
                second: homeworkId.trim(),
                first: courseId
              })

              if (assignmentResponse.data.success && assignmentResponse.data.data) {
                const assignmentData = assignmentResponse.data.data.split('\t\r');
                return {
                  id: homeworkId.trim(),
                  title: assignmentData[0] || `作业 ${homeworkId.trim()}`,
                  description: assignmentData[1] || '暂无描述',
                  deadline: assignmentData[2] || '待定',
                  create_time: assignmentData[3] || '',
                  course_id: courseId,
                  subject: '体育',
                  status: new Date(assignmentData[2]) > new Date() ? '进行中' : '已截止',
                  points: 100
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

        // 首先获取教师信息
        let teacherName = '未知教师'; // 默认值
        try {
          // 尝试获取教师信息 - 使用可能的API端点
          const teacherResponse = await apiClient.post('/User/get_teacher_info', {
            first: studentId,
            second: token,  // 需要认证token
            third: '0',
            fourth: teacherId
          });

          if (teacherResponse.data.success && teacherResponse.data.data) {
            // 解析教师数据字符串，格式为: 教师姓名\t\r其他信息...
            const teacherDataArray = teacherResponse.data.data.split('\t\r');
            if (teacherDataArray.length > 0) {
              teacherName = teacherDataArray[0] || '未知教师';  // 第一个字段是教师姓名
            }
          }
        } catch (teacherError) {
          console.warn(`获取教师信息失败 (ID: ${teacherId}):`, teacherError.message);
          // 如果获取教师信息失败，使用默认值
        }

        // 构造课程对象
        course.value = {
          id: courseId,
          name: courseName || '未命名课程',
          description: courseDescription || '暂无描述',
          subject: '体育',
          status: isActive === '1' ? '进行中' : '未发布',
          assignments: assignments,
          teacherId: teacherId,
          teacherName: teacherName,  // 添加教师姓名
          courseTerm: courseTerm,
          createTime: createTime
        }

        console.log('课程详情加载成功:', course.value)
      } else {
        throw new Error('课程数据格式不正确');
      }
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
      assignments: [
        {
          id: 'HW101',
          title: '俯卧撑训练',
          description: '完成3组俯卧撑，每组15个，注意保持身体平直',
          deadline: '2025-01-15',
          create_time: '2025-01-01',
          course_id: courseId,
          subject: '体育',
          status: '进行中',
          points: 100
        },
        {
          id: 'HW102',
          title: '仰卧起坐训练',
          description: '完成3组仰卧起坐，每组20个，注意动作规范',
          deadline: '2025-01-20',
          create_time: '2025-01-02',
          course_id: courseId,
          subject: '体育',
          status: '已完成',
          points: 100
        },
        {
          id: 'HW103',
          title: '深蹲训练',
          description: '完成4组深蹲，每组15个，注意膝盖不要超过脚尖',
          deadline: '2025-01-25',
          create_time: '2025-01-03',
          course_id: courseId,
          subject: '体育',
          status: '进行中',
          points: 100
        },
        {
          id: 'HW104',
          title: '平板支撑训练',
          description: '完成3组平板支撑，每组60秒，保持核心稳定',
          deadline: '2025-01-30',
          create_time: '2025-01-04',
          course_id: courseId,
          subject: '体育',
          status: '进行中',
          points: 100
        },
        {
          id: 'HW105',
          title: '引体向上训练',
          description: '完成3组引体向上，每组8个，注意动作完整',
          deadline: '2025-02-05',
          create_time: '2025-01-05',
          course_id: courseId,
          subject: '体育',
          status: '进行中',
          points: 100
        }
      ]
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

    const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
    const studentId = currentUser ? currentUser.id : 'student1'

    const classIdResp = await apiClient.post('/Class/get_class_id_by_course', {
      first: '0',
      second: studentId,
      third: token,
      fourth: courseId
    })

    if (classIdResp.data[0] < 0) {
      teachingVideos.value = []
      console.log('该课程暂无教学视频')
      return
    }

    const classIdStr = classIdResp.data[0]
    const classIds = classIdStr ? classIdStr.split('\t\r').filter(id => id) : []

    const videoDetailsPromises = classIds.map(async (classId) => {
      try {
        const infoResp = await apiClient.post('/api/get_info_by_class_id', {
          course_id: courseId,
          class_id: classId
        })

        if (infoResp.data[0] < 0) {
          return null
        }

        const d = infoResp.data
        return {
          id: classId,
          title: d[0],
          description: d[1],
          url: d[2],
          duration: '00:00',
          cover: '',
          uploadDate: d[3]
        }
      } catch (error) {
        console.error(`获取视频 ${classId} 详情失败:`, error)
        return null
      }
    })

    const videos = await Promise.all(videoDetailsPromises)
    teachingVideos.value = videos.filter(v => v !== null)

    console.log('教学视频加载成功:', teachingVideos.value)
  } catch (err) {
    console.error('获取教学视频失败:', err)
    videosError.value = true
    videosErrorMessage.value = err.message
    teachingVideos.value = []
  } finally {
    videosLoading.value = false
  }
}

// 导航函数
const goBack = () => {
  router.push('/student')
}

const goToTeachingVideos = () => {
  router.push(`/student/course/${courseId}/teaching-videos`)
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

// 组件挂载时获取课程详情
onMounted(() => {
  fetchCourseDetails()
})
</script>
