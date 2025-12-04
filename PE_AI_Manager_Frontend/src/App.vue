<template>
  <div>
    <!-- 只在非登录页面显示导航栏 -->
    <nav v-if="!isLoginPage" class="flex items-center justify-between px-6 py-4 bg-white shadow fixed top-0 left-0 right-0 z-50">
      <h1 class="text-3xl font-bold text-blue-600">运动课堂</h1>
      <div class="space-x-4">
        <RouterLink to="/" class="text-gray-700 hover:text-blue-600">学生端</RouterLink>
        <RouterLink to="/teacher" class="text-gray-700 hover:text-blue-600">教师端</RouterLink>
        <RouterLink to="/assistant" class="text-gray-700 hover:text-blue-600">AI助手</RouterLink>
        <button @click="logout" class="text-gray-700 hover:text-blue-600 cursor-pointer">退出登录</button>
      </div>
    </nav>

    <!-- 根据是否显示导航栏调整内容区域的padding-top -->
    <div :class="isLoginPage ? '' : 'pt-20'">
      <router-view />
    </div>
  </div>
</template>
<script setup>
import { computed } from 'vue'
import { RouterLink, RouterView } from 'vue-router'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

// 判断当前是否为登录页面
const isLoginPage = computed(() => {
  return route.path === '/login' || route.path === '/register'
})

const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}
</script>
