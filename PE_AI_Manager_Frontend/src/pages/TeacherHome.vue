<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-6xl mx-auto p-6 space-y-10">
  
      
      <!-- 顶部 Banner -->
      <div class="relative w-full rounded-3xl overflow-hidden shadow-2xl">
        <img src="https://images.unsplash.com/photo-1521412644187-c49fa049e84d" class="w-full h-96 object-cover opacity-70" />
        <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent">
          <h1 class="absolute inset-0 flex items-center justify-center text-6xl font-extrabold tracking-widest text-white drop-shadow-2xl">
            教 师 端
          </h1>
          <p class="absolute bottom-8 left-8 text-xl text-white font-medium">科学管理，高效教学</p>
        </div>
      </div>
      
      <!-- 快捷操作按钮 -->
      <section>
        <h2 class="text-3xl font-bold mb-6 text-gray-800">🚀 快捷操作</h2>
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <button @click="goToPublish" class="p-6 rounded-3xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">📝</div>
            <h3 class="text-xl font-bold">发布新作业</h3>
          </button>
          <button @click="goToGrade" class="p-6 rounded-3xl bg-orange-500 text-white hover:bg-orange-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">✏️</div>
            <h3 class="text-xl font-bold">批改成绩</h3>
          </button>
          <button @click="goToVideos" class="p-6 rounded-3xl bg-teal-500 text-white hover:bg-teal-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">🎥</div>
            <h3 class="text-xl font-bold">教学视频</h3>
          </button>
          <button @click="goToAssignments" class="p-6 rounded-3xl bg-purple-500 text-white hover:bg-purple-600 transition-all shadow-xl">
            <div class="text-4xl mb-2">📊</div>
            <h3 class="text-xl font-bold">作业统计</h3>
          </button>
        </div>
      </section>
      
      <!-- 班级列表 -->
      <section>
        <div class="flex justify-between items-center mb-6">
          <h2 class="text-3xl font-bold text-gray-800">🏫 我的班级</h2>
          <button class="px-4 py-2 rounded-xl bg-green-500 text-white hover:bg-green-600 transition-all shadow">
            管理班级
          </button>
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div v-for="classItem in classes" :key="classItem.id" 
               class="p-6 rounded-3xl shadow-lg bg-white border border-gray-200 hover:shadow-xl transition-all">
            <div class="flex justify-between items-center mb-4">
              <div>
                <h3 class="text-2xl font-bold text-gray-800">{{ classItem.name }}</h3>
                <p class="text-gray-500 text-sm">学生人数：{{ classItem.studentCount }}人</p>
              </div>
              <div class="text-4xl text-gray-300">{{ classItem.grade }}</div>
            </div>
            
            <!-- 班级作业统计 -->
            <div class="space-y-3">
              <div v-for="stat in classItem.stats" :key="stat.id" class="flex justify-between items-center">
                <span class="text-sm text-gray-600">{{ stat.assignment }}</span>
                <div class="flex items-center gap-2">
                  <span class="text-green-600 font-medium">{{ stat.submitted }}</span>
                  <span class="text-gray-400">/</span>
                  <span class="text-gray-600">{{ stat.total }}</span>
                  <span :class="['w-2 h-2 rounded-full', 
                                 stat.submitted === stat.total ? 'bg-green-500' : 
                                 stat.submitted > stat.total * 0.5 ? 'bg-yellow-500' : 'bg-red-500']"></span>
                </div>
              </div>
            </div>
            
            <!-- 班级操作按钮 -->
            <div class="flex gap-3 mt-6">
              <button @click="viewClassAssignments(classItem.id)" 
                      class="flex-1 px-4 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
                查看作业
              </button>
              <button @click="viewClassStudents(classItem.id)" 
                      class="flex-1 px-4 py-2 rounded-xl bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
                查看学生
              </button>
            </div>
          </div>
        </div>
      </section>
      
      <!-- 最近作业完成情况 -->
      <section>
        <div class="flex justify-between items-center mb-6">
          <h2 class="text-3xl font-bold text-gray-800">📊 作业完成情况</h2>
          <button @click="goToAssignments" class="px-4 py-2 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow">
            查看全部
          </button>
        </div>
        
        <div class="space-y-4">
          <div v-for="assignment in recentAssignments" :key="assignment.id" 
               class="p-6 rounded-xl shadow bg-white border border-gray-200 hover:shadow-lg transition-all">
            <div class="flex justify-between items-start mb-4">
              <div>
                <h3 class="text-xl font-bold text-gray-800">{{ assignment.title }}</h3>
                <p class="text-gray-500 text-sm mt-1">{{ assignment.deadline }}截止</p>
              </div>
              <div class="text-xl font-bold text-gray-400">{{ assignment.subject }}</div>
            </div>
            
            <div class="flex items-center gap-6">
              <div class="flex-1">
                <div class="flex justify-between items-center text-sm mb-1">
                  <span class="text-gray-600">提交情况</span>
                  <span class="text-blue-600 font-medium">{{ assignment.submitted }}/{{ assignment.total }}</span>
                </div>
                <div class="w-full bg-gray-200 rounded-full h-3">
                  <div class="bg-blue-500 h-3 rounded-full" 
                       :style="{ width: `${(assignment.submitted / assignment.total) * 100}%` }"></div>
                </div>
              </div>
              
              <div class="flex-1">
                <div class="flex justify-between items-center text-sm mb-1">
                  <span class="text-gray-600">批改情况</span>
                  <span class="text-green-600 font-medium">{{ assignment.graded }}/{{ assignment.total }}</span>
                </div>
                <div class="w-full bg-gray-200 rounded-full h-3">
                  <div class="bg-green-500 h-3 rounded-full" 
                       :style="{ width: `${(assignment.graded / assignment.total) * 100}%` }"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

// 模拟班级数据
const classes = ref([
  {
    id: 1,
    name: '初三（1）班',
    grade: '🏅',
    studentCount: 45,
    stats: [
      { id: 1, assignment: '50米折返跑', submitted: 42, total: 45 },
      { id: 2, assignment: '仰卧起坐', submitted: 38, total: 45 },
      { id: 3, assignment: '立定跳远', submitted: 40, total: 45 }
    ]
  },
  {
    id: 2,
    name: '初三（2）班',
    grade: '🥈',
    studentCount: 42,
    stats: [
      { id: 1, assignment: '50米折返跑', submitted: 35, total: 42 },
      { id: 2, assignment: '仰卧起坐', submitted: 38, total: 42 },
      { id: 3, assignment: '立定跳远', submitted: 32, total: 42 }
    ]
  },
  {
    id: 3,
    name: '初三（3）班',
    grade: '🥉',
    studentCount: 43,
    stats: [
      { id: 1, assignment: '50米折返跑', submitted: 40, total: 43 },
      { id: 2, assignment: '仰卧起坐', submitted: 36, total: 43 },
      { id: 3, assignment: '立定跳远', submitted: 39, total: 43 }
    ]
  }
])

// 模拟最近作业数据
const recentAssignments = ref([
  {
    id: 1,
    title: '50米折返跑测试',
    subject: '田径',
    deadline: '11月30日',
    submitted: 117,
    graded: 85,
    total: 130
  },
  {
    id: 2,
    title: '仰卧起坐测试',
    subject: '力量',
    deadline: '12月2日',
    submitted: 95,
    graded: 45,
    total: 130
  }
])

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



const viewClassAssignments = (classId) => {
  console.log('查看班级作业:', classId)
  // 这里可以跳转到班级作业页面
}

const viewClassStudents = (classId) => {
  console.log('查看班级学生:', classId)
  // 这里可以跳转到班级学生页面
}
</script>