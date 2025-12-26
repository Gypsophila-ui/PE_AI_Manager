<template>
  <div class="min-h-screen bg-white flex items-center justify-center p-4">
    <div class="max-w-md w-full rounded-3xl shadow-2xl overflow-hidden">
      <!-- 顶部樱花背景 -->
      <div class="h-48 relative">
        <!-- 同济大学樱花照片 -->
        <img src="../assets/Login/1.jpg"
             class="w-full h-full object-cover"
             alt="同济大学樱花">
        <div class="absolute inset-0 bg-gradient-to-t from-black/50 to-transparent flex items-center justify-center">
          <h1 class="text-4xl font-bold text-white drop-shadow-lg">同济大学智慧体育课堂</h1>
        </div>
      </div>

      <!-- 登录表单 -->
      <div class="p-8 bg-white">
        <h2 class="text-3xl font-bold mb-6 text-center text-gray-800">登录</h2>

        <form @submit.prevent="login" class="space-y-4 relative">
          <!-- 校徽背景图片 - 整个表单共用 -->
          <div class="absolute inset-0 opacity-20 flex items-center justify-center pointer-events-none">
            <img src="../assets/Login/2.jpg"
                 alt="同济大学校徽"
                 class="w-120 h-120 object-contain">
          </div>
          <!-- 角色选择 -->
          <div class="flex gap-4">
            <button type="button"
                    :class="['flex-1 py-3 rounded-xl font-medium transition-all', role === 'student' ? 'bg-sky-500 text-white shadow-lg' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
                    @click="role = 'student'">
              学生
            </button>
            <button type="button"
                    :class="['flex-1 py-3 rounded-xl font-medium transition-all', role === 'teacher' ? 'bg-sky-500 text-white shadow-lg' : 'bg-gray-100 text-gray-700 hover:bg-gray-200']"
                    @click="role = 'teacher'">
              教师
            </button>
          </div>

          <!-- 用户名输入框 - 带校徽背景 -->
          <div class="relative">
            <label class="block text-sm font-medium text-gray-700 mb-1">用户名</label>
            <div class="relative">

              <input type="text" v-model="username"
                       class="w-full px-4 py-3 rounded-xl border border-gray-300 bg-white/90 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent transition-all"
                       placeholder="请输入用户名" required>
            </div>
          </div>

          <!-- 密码输入框 - 带校徽背景 -->
          <div class="relative">
            <label class="block text-sm font-medium text-gray-700 mb-1">密码</label>
            <div class="relative">

              <input type="password" v-model="password"
                       class="w-full px-4 py-3 rounded-xl border border-gray-300 bg-white/90 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent transition-all"
                       placeholder="请输入密码" required>
            </div>
          </div>

          <!-- 错误信息显示 -->
          <div v-if="errorMessage" class="text-sm text-red-500 mt-2">{{ errorMessage }}</div>

          <!-- 登录按钮 -->
          <button type="submit"
                  class="w-full py-3 bg-sky-500 text-white font-bold rounded-xl shadow-lg hover:shadow-xl transform hover:scale-105 transition-all disabled:opacity-70 disabled:cursor-not-allowed"
                  :disabled="loading">
            <span v-if="loading">登录中...</span>
            <span v-else>登录</span>
          </button>
        </form>

        <!-- 测试账户信息 -->
        <div class="mt-6 p-4 bg-sky-500/5 rounded-xl border border-sky-500/20">
          <h3 class="text-lg font-semibold text-sky-500 mb-3">测试账户</h3>
          <div class="space-y-3">
            <div>
              <p class="text-sm text-gray-600">学生端：</p>
              <p class="text-sm font-medium">用户名：student001 | 密码：123456</p>
            </div>
            <div>
              <p class="text-sm text-gray-600">教师端：</p>
              <p class="text-sm font-medium">用户名：teacher001 | 密码：123456</p>
            </div>
          </div>
        </div>

        <!-- 注册链接 -->
        <div class="mt-6 text-center">
          <p class="text-gray-700">还没有账号？ <a href="/register" class="text-sky-500 font-semibold hover:underline">立即注册</a></p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { loginTeacher, loginStudent } from '../services/auth'

const router = useRouter()
const role = ref('student')
const username = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

// 登录功能
const login = async () => {
  // 表单验证
  if (!username.value || !password.value) {
    errorMessage.value = '请输入用户名和密码'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    let result
    // 测试账户特判
    if ((role.value === 'student' && username.value === 'student001' && password.value === '123456') ||
        (role.value === 'teacher' && username.value === 'teacher001' && password.value === '123456')) {
      // 直接模拟登录成功，避免网络请求
      result = {
        success: true,
        data: {
          jwt_ans: 'mock_token_123456'
        }
      }
    } else {
      // 根据角色调用不同的登录API
      if (role.value === 'student') {
        result = await loginStudent(username.value, password.value)
      } else {
        result = await loginTeacher(username.value, password.value)
      }
    }

    if (result.success) {
      // 登录成功，保存JWT token到localStorage
      localStorage.setItem('token', result.data.jwt_ans)

      // 保存用户信息
      localStorage.setItem('user', JSON.stringify({
        id: username.value,
        role: role.value,
        username: username.value,
        token: result.data.jwt_ans
      }))

      // 根据角色跳转到对应首页
      if (role.value === 'student') {
        router.push('/student')
      } else {
        router.push('/teacher')
      }
    } else {
      // 登录失败，显示错误信息
      errorMessage.value = result.message || '登录失败，请重试'
    }
  } catch (error) {
    errorMessage.value = '登录过程中发生错误，请稍后重试'
    console.error('登录错误:', error)
  } finally {
    loading.value = false
  }
}
</script>
