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
          
          <!-- 登录按钮 -->
          <button type="submit" 
                  class="w-full py-3 bg-gradient-to-r from-blue-500 to-purple-600 text-white font-bold rounded-xl shadow-lg hover:shadow-xl transform hover:scale-105 transition-all">
            登录
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

const router = useRouter()
const role = ref('student')
const username = ref('')
const password = ref('')

// 模拟登录功能
const login = () => {
  // 这里应该是实际的登录逻辑，现在使用模拟数据
  console.log('登录信息:', { role: role.value, username: username.value, password: password.value })
  
  // 保存用户信息到本地存储（模拟）
  localStorage.setItem('user', JSON.stringify({ role: role.value, username: username.value }))
  
  // 根据角色跳转到对应首页
  if (role.value === 'student') {
    router.push('/student')
  } else {
    router.push('/teacher')
  }
}
</script>