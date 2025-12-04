<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-100 to-purple-100 flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-white rounded-3xl shadow-2xl overflow-hidden">
      <!-- 顶部图片 -->
      <div class="h-48 bg-gradient-to-r from-blue-500 to-purple-600 flex items-center justify-center">
        <h1 class="text-4xl font-extrabold text-white drop-shadow-lg">体育作业平台</h1>
      </div>

      <!-- 注册表单 -->
      <div class="p-8">
        <h2 class="text-3xl font-bold mb-6 text-center text-gray-800">注册</h2>

        <form @submit.prevent="register" class="space-y-4">
          <!-- 角色选择 -->
          <div class="flex gap-4">
            <button type="button"
                    :class="['flex-1 py-3 rounded-xl font-medium transition-all', role === 'student' ? 'bg-blue-500 text-white shadow-lg' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
                    @click="role = 'student'">
              学生
            </button>
            <button type="button"
                    :class="['flex-1 py-3 rounded-xl font-medium transition-all', role === 'teacher' ? 'bg-purple-500 text-white shadow-lg' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
                    @click="role = 'teacher'">
              教师
            </button>
          </div>

          <!-- 用户名 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">用户名</label>
            <input type="text" v-model="username"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入用户名" required>
          </div>

          <!-- 密码 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">密码</label>
            <input type="password" v-model="password"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请输入密码" required minlength="6">
          </div>

          <!-- 确认密码 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">确认密码</label>
            <input type="password" v-model="confirmPassword"
                   class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                   placeholder="请再次输入密码" required minlength="6">
          </div>

          <!-- 注册按钮 -->
          <button type="submit"
                  class="w-full py-3 bg-gradient-to-r from-blue-500 to-purple-600 text-white font-bold rounded-xl shadow-lg hover:shadow-xl transform hover:scale-105 transition-all">
            注册
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
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const role = ref('student')
const username = ref('')
const password = ref('')
const confirmPassword = ref('')

// 模拟注册功能
const register = () => {
  if (password.value !== confirmPassword.value) {
    alert('两次输入的密码不一致')
    return
  }

  // 这里应该是实际的注册逻辑，现在使用模拟数据
  console.log('注册信息:', { role: role.value, username: username.value, password: password.value })

  // 注册成功后跳转到登录页面
  alert('注册成功，请登录')
  router.push('/login')
}
</script>
