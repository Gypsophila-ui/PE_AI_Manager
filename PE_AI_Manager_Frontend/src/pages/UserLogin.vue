<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-100 to-purple-100 flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-white rounded-3xl shadow-2xl overflow-hidden">
      <!-- 顶部图片 -->
      <div class="h-48 bg-gradient-to-r from-blue-500 to-purple-600 flex items-center justify-center">
        <h1 class="text-4xl font-extrabold text-white drop-shadow-lg">体育作业平台</h1>
      </div>

      <!-- 登录表单 -->
      <div class="p-8">
        <h2 class="text-3xl font-bold mb-6 text-center text-gray-800">登录</h2>

        <form @submit.prevent="login" class="space-y-4">
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
                   placeholder="请输入密码" required>
          </div>

          <!-- 错误信息显示 -->
          <div v-if="errorMessage" class="text-sm text-red-500 mt-2">{{ errorMessage }}</div>

          <!-- 登录按钮 -->
          <button type="submit"
                  class="w-full py-3 bg-gradient-to-r from-blue-500 to-purple-600 text-white font-bold rounded-xl shadow-lg hover:shadow-xl transform hover:scale-105 transition-all disabled:opacity-70 disabled:cursor-not-allowed"
                  :disabled="loading">
            <span v-if="loading">登录中...</span>
            <span v-else>登录</span>
          </button>
        </form>

        <!-- 测试账户信息 -->
        <div class="mt-6 p-4 bg-gray-50 rounded-xl">
          <h3 class="text-lg font-semibold text-gray-800 mb-3">测试账户</h3>
          <div class="space-y-3">
            <div>
              <p class="text-sm text-gray-600">学生端：</p>
              <p class="text-sm text-gray-800">用户名：student001 | 密码：123456</p>
            </div>
            <div>
              <p class="text-sm text-gray-600">教师端：</p>
              <p class="text-sm text-gray-800">用户名：teacher001 | 密码：123456</p>
            </div>
          </div>
        </div>

        <!-- 注册链接 -->
        <div class="mt-6 text-center">
          <p class="text-gray-600">还没有账号？ <a href="/register" class="text-blue-600 font-semibold hover:underline">立即注册</a></p>
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
      // 登录成功，保存用户信息和JWT
      localStorage.setItem('user', JSON.stringify({
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
