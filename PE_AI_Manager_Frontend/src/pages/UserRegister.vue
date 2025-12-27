<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-blue-100 flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-white rounded-3xl shadow-2xl overflow-hidden">
      <!-- 顶部图片 -->
      <div class="h-48 bg-gradient-to-r from-blue-400 to-blue-600 flex items-center justify-center">
        <h1 class="text-4xl font-extrabold text-white drop-shadow-lg">体育作业平台</h1>
      </div>

      <!-- 注册表单 -->
      <div class="p-8">
        <h2 class="text-3xl font-bold mb-6 text-center text-gray-800">注册</h2>

        <form @submit.prevent="register" class="space-y-4">
          <!-- 角色选择 -->
          <div class="flex gap-4">
            <button type="button"
                    :class="['flex-1 py-3 rounded-xl font-medium transition-all', role === 'student' ? 'bg-blue-500 text-white shadow-lg' : 'bg-blue-100 text-blue-700 hover:bg-blue-200']"
                    @click="role = 'student'">
              学生
            </button>
            <button type="button"
                    :class="['flex-1 py-3 rounded-xl font-medium transition-all', role === 'teacher' ? 'bg-blue-600 text-white shadow-lg' : 'bg-blue-100 text-blue-700 hover:bg-blue-200']"
                    @click="role = 'teacher'">
              教师
            </button>
          </div>

          <!-- 用户名/ID -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">{{ role === 'student' ? '学号' : '工号' }}</label>
            <input type="text" v-model="formData.id"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   :placeholder="role === 'student' ? '请输入学号' : '请输入工号'" required maxlength="50">
          </div>

          <!-- 密码 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">密码</label>
            <input type="password" v-model="formData.password"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入密码" required minlength="6" maxlength="255">
          </div>

          <!-- 确认密码 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">确认密码</label>
            <input type="password" v-model="confirmPassword"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请再次输入密码" required minlength="6" maxlength="255">
          </div>

          <!-- 姓名 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">姓名</label>
            <input type="text" v-model="formData.name"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入姓名" required maxlength="100">
          </div>

          <!-- 性别 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">性别</label>
            <select v-model="formData.gender"
                    class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all">
              <option value="男">男</option>
              <option value="女">女</option>
            </select>
          </div>

          <!-- 教师特有字段：职称 -->
          <div v-if="role === 'teacher'">
            <label class="block text-sm font-medium text-gray-700 mb-1">职称</label>
            <input type="text" v-model="formData.title"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入职称（如：讲师、副教授）" required maxlength="100">
          </div>

          <!-- 学生特有字段：专业 -->
          <div v-if="role === 'student'">
            <label class="block text-sm font-medium text-gray-700 mb-1">专业</label>
            <input type="text" v-model="formData.major"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入专业（如：网信、嵌入式）" required maxlength="100">
          </div>

          <!-- 学院 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">所属学院</label>
            <input type="text" v-model="formData.college"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入所属学院" required maxlength="100">
          </div>

          <!-- 系 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">所属系</label>
            <input type="text" v-model="formData.department"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入所属系" required maxlength="100">
          </div>

          <!-- 错误信息显示 -->
          <div v-if="errorMessage" class="text-sm text-red-500 mt-2">{{ errorMessage }}</div>

          <!-- 注册按钮 -->
          <button type="submit"
                  class="w-full py-3 bg-gradient-to-r from-blue-400 to-blue-600 text-white font-bold rounded-xl shadow-lg hover:shadow-xl transform hover:scale-105 transition-all disabled:opacity-70 disabled:cursor-not-allowed"
                  :disabled="loading">
            <span v-if="loading">注册中...</span>
            <span v-else>注册</span>
          </button>
        </form>

        <!-- 登录链接 -->
        <div class="mt-6 text-center">
          <p class="text-gray-600">已有账号？ <a href="/login" class="text-blue-600 font-semibold hover:underline">立即登录</a></p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { registerTeacher, registerStudent } from '../services/auth'

const router = useRouter()
const role = ref('student')
const confirmPassword = ref('')
const loading = ref(false)
const errorMessage = ref('')

// 表单数据
const formData = reactive({
  id: '',
  password: '',
  name: '',
  gender: '男',
  title: '', // 教师特有
  major: '', // 学生特有
  college: '',
  department: ''
})

// 注册功能
const register = async () => {
  // 表单验证
  if (!formData.id || !formData.password || !formData.name || !formData.college || !formData.department) {
    errorMessage.value = '请填写所有必填字段'
    return
  }

  if (role.value === 'teacher' && !formData.title) {
    errorMessage.value = '教师请填写职称'
    return
  }

  if (role.value === 'student' && !formData.major) {
    errorMessage.value = '学生请填写专业'
    return
  }

  if (formData.password !== confirmPassword.value) {
    errorMessage.value = '两次输入的密码不一致'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    let result

    // 根据角色调用不同的注册API
    if (role.value === 'student') {
      result = await registerStudent(
        formData.id,
        formData.password,
        formData.name,
        formData.gender,
        formData.major,
        formData.college,
        formData.department
      )
    } else {
      result = await registerTeacher(
        formData.id,
        formData.password,
        formData.name,
        formData.gender,
        formData.title,
        formData.college,
        formData.department
      )
    }

    if (result.success) {
      // 注册成功，跳转到登录页面
      alert('注册成功，请登录')
      router.push('/login')
    } else {
      // 注册失败，显示错误信息
      errorMessage.value = result.message || '注册失败，请重试'
    }
  } catch (error) {
    errorMessage.value = '注册过程中发生错误，请稍后重试'
    console.error('注册错误:', error)
  } finally {
    loading.value = false
  }
}
</script>
