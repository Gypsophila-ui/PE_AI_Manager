<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-10">


      <!-- 顶部 Banner -->
      <div class="relative w-full rounded-3xl overflow-hidden shadow-2xl">
        <img src="../assets/HomeHeader.jpg" class="w-full h-96 object-cover opacity-50" />
        <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent">
          <h2
            class="absolute inset-0 flex items-center justify-center text-6xl font-display font-medium tracking-widest text-white drop-shadow-2xl">
            智慧体育课堂
          </h2>
          <h3 class="absolute bottom-8 left-0 right-0 text-center text-2xl text-white font-medium">科学管理，高效教学</h3>
        </div>
      </div>

      <!-- 快捷操作按钮 -->
      <section>
        <h2 class="text-3xl font-bold mb-6 text-gray-800">🚀 快捷操作</h2>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <button @click="goToPublish"
            class="p-6 rounded-3xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">📝</div>
            <h3 class="text-xl font-bold">发布新作业</h3>
          </button>
          <button @click="goToGrade"
            class="p-6 rounded-3xl bg-orange-500 text-white hover:bg-orange-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">✏️</div>
            <h3 class="text-xl font-bold">批改成绩</h3>
          </button>
          <button @click="goToVideos"
            class="p-6 rounded-3xl bg-teal-500 text-white hover:bg-teal-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">🎥</div>
            <h3 class="text-xl font-bold">教学视频</h3>
          </button>
          <button @click="goToAssignments"
            class="p-6 rounded-3xl bg-purple-500 text-white hover:bg-purple-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">📊</div>
            <h3 class="text-xl font-bold">作业统计</h3>
          </button>
        </div>
      </section>

      <!-- 课程列表 -->
      <section>
        <div class="flex justify-between items-center mb-6">
          <h2 class="text-3xl font-bold text-gray-800">📚 我的课程</h2>
        </div>

        <div class="space-y-3">
          <div v-for="course in teacherCourses" :key="course.id"
            class="bg-white rounded-xl shadow-md border border-gray-100 p-4">
            <div class="flex flex-col md:flex-row md:items-center justify-between">
              <div class="flex-1">
                <h3 class="text-lg font-semibold text-gray-800 mb-1">{{ course.name }}</h3>
                <p class="text-sm text-gray-600 mb-2">{{ course.description }}</p>
                <div class="flex items-center space-x-4">
                  <span class="text-xs text-gray-500">{{ course.subject }}</span>
                  <span
                    :class="['text-xs px-2 py-1 rounded-full',
                      course.status === '进行中' ? 'bg-blue-100 text-blue-800' : 'bg-green-100 text-green-800']">
                    {{ course.status }}
                  </span>
                  <span class="text-xs text-gray-500">作业数量: {{ course.assignments.length }}</span>
                </div>
              </div>
              <button @click="viewCourseDetails(course.id)" class="mt-3 md:mt-0 text-blue-500 hover:text-blue-700 text-sm font-medium">
                查看详情
              </button>
            </div>
          </div>
        </div>
      </section>

    </div>

  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { courses as mockCourses } from '../data/mockData.js'


const router = useRouter()

// 状态管理
const successMessage = ref('');

// 获取课程数据
const teacherCourses = ref([...mockCourses]);

// 获取班级名称 - 暂时返回默认值，因为班级数据已删除
const getClassName = (classId) => {
  // 由于班级数据已删除，这里返回默认班级名称
  // 实际应用中应该从API或其他数据源获取班级信息
  return '默认班级';
};

// 导航函数
const goToPublish = () => {
  router.push('/teacher/publish')
}

const goToGrade = () => {
  router.push('/teacher/grade')
}

const goToVideos = () => {
  router.push('/teacher/videos')
}

const goToAssignments = () => {
  router.push('/teacher/assignments')
}

// 查看课程详情
const viewCourseDetails = (courseId) => {
  router.push(`/teacher/course/${courseId}`)
};


</script>
