<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-4xl mx-auto p-6 space-y-10">
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

      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-4">📝 发布新作业</h2>
        <p class="text-gray-600">填写作业信息，为学生发布新的体育作业</p>
      </section>

      <!-- 作业发布表单 -->
      <section class="bg-white rounded-3xl shadow-xl p-8">
        <form @submit.prevent="submitForm">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <!-- 作业标题 -->
            <div class="col-span-1 md:col-span-2">
              <label for="title" class="block text-sm font-medium text-gray-700 mb-2">作业标题</label>
              <input
                id="title"
                v-model="assignment.title"
                type="text"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                placeholder="例如：50米折返跑测试"
                required
              />
            </div>

            <!-- 作业科目 -->
            <div>
              <label for="subject" class="block text-sm font-medium text-gray-700 mb-2">作业科目</label>
              <select
                id="subject"
                v-model="assignment.subject"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                required
              >
                <option value="">请选择科目</option>
                <option value="田径">田径</option>
                <option value="力量">力量</option>
                <option value="弹跳">弹跳</option>
                <option value="柔韧性">柔韧性</option>
                <option value="球类">球类</option>
              </select>
            </div>

            <!-- 作业分值 -->
            <div>
              <label for="points" class="block text-sm font-medium text-gray-700 mb-2">作业分值</label>
              <input
                id="points"
                v-model.number="assignment.points"
                type="number"
                min="0"
                max="100"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                placeholder="100"
                required
              />
            </div>

            <!-- 截止日期 -->
            <div>
              <label for="deadline" class="block text-sm font-medium text-gray-700 mb-2">截止日期</label>
              <input
                id="deadline"
                v-model="assignment.deadline"
                type="datetime-local"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
                required
              />
            </div>

            <!-- 视频要求 -->
            <div class="flex items-center gap-3">
              <input
                id="videoRequired"
                v-model="assignment.videoRequired"
                type="checkbox"
                class="w-5 h-5 text-blue-600 rounded focus:ring-blue-500"
              />
              <label for="videoRequired" class="text-sm font-medium text-gray-700">要求提交视频</label>
            </div>
          </div>

          <!-- 作业描述 -->
          <div class="mt-6">
            <label for="description" class="block text-sm font-medium text-gray-700 mb-2">作业描述</label>
            <textarea
              id="description"
              v-model="assignment.description"
              rows="4"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all shadow-sm"
              placeholder="详细描述作业要求、评分标准等信息"
              required
            ></textarea>
          </div>

          <!-- 选择班级 -->
          <div class="mt-6">
            <label class="block text-sm font-medium text-gray-700 mb-2">选择班级</label>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div
                v-for="classItem in classes"
                :key="classItem.id"
                class="flex items-center gap-3 p-4 rounded-xl border-2 transition-all cursor-pointer"
                :class="assignment.classIds.includes(classItem.id)
                  ? 'border-blue-500 bg-blue-50'
                  : 'border-gray-300 hover:border-gray-400'"
                @click="toggleClass(classItem.id)"
              >
                <input
                  type="checkbox"
                  :checked="assignment.classIds.includes(classItem.id)"
                  class="w-5 h-5 text-blue-600 rounded focus:ring-blue-500"
                  @click.stop
                />
                <div>
                  <h3 class="font-medium text-gray-800">{{ classItem.name }}</h3>
                  <p class="text-sm text-gray-500">{{ classItem.studentCount }}名学生</p>
                </div>
              </div>
            </div>
          </div>

          <!-- 提交按钮 -->
          <div class="mt-10 flex gap-4 justify-end">
            <button
              type="button"
              @click="goBack"
              class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow"
            >
              取消
            </button>
            <button
              type="submit"
              class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg"
            >
              发布作业
            </button>
          </div>
        </form>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { classes } from '../../data/mockData'

const router = useRouter()

// 作业表单数据
const assignment = ref({
  title: '',
  subject: '',
  description: '',
  deadline: '',
  points: 100,
  videoRequired: true,
  classIds: []
})

// 提交表单
const submitForm = () => {
  // 验证班级选择
  if (assignment.value.classIds.length === 0) {
    alert('请至少选择一个班级')
    return
  }

  // 模拟发布作业
  console.log('发布作业:', assignment.value)

  // 显示发布成功
  alert('作业发布成功！')

  // 跳转到教师首页
  router.push('/teacher')
}

// 切换班级选择
const toggleClass = (classId) => {
  const index = assignment.value.classIds.indexOf(classId)
  if (index > -1) {
    assignment.value.classIds.splice(index, 1)
  } else {
    assignment.value.classIds.push(classId)
  }
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
