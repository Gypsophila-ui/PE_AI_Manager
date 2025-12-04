<template>
  <div class="max-w-4xl mx-auto p-6 space-y-8">
    <!-- 顶部导航 -->
    <div class="flex justify-between items-center">
      <h1 class="text-4xl font-bold text-gray-800">🏋️‍♂️ AI运动助手</h1>
      <button @click="goBack" class="px-6 py-3 bg-blue-500 text-white rounded-xl font-medium hover:bg-blue-600 transition-all shadow-lg">
        返回首页
      </button>
    </div>
    
    <!-- 介绍部分 -->
    <div class="p-8 rounded-3xl shadow-lg bg-gradient-to-br from-blue-50 to-purple-50">
      <h2 class="text-2xl font-bold mb-4 text-gray-800">💡 你可以问我这些问题</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-gray-700">
        <p>• 如何正确进行拉伸运动？</p>
        <p>• 运动后如何缓解肌肉酸痛？</p>
        <p>• 每天最佳的运动时间是什么时候？</p>
        <p>• 如何科学安排训练计划？</p>
        <p>• 运动前需要做哪些准备？</p>
        <p>• 如何避免运动损伤？</p>
      </div>
    </div>
    
    <!-- 聊天界面 -->
    <div class="bg-white rounded-3xl shadow-xl overflow-hidden">
      <!-- 聊天记录 -->
      <div class="p-6 h-96 overflow-y-auto space-y-4" ref="chatContainer">
        <div v-for="(message, index) in messages" :key="index" 
             :class="['flex', message.role === 'user' ? 'justify-end' : 'justify-start']">
          <div :class="['max-w-[80%] p-4 rounded-2xl shadow', 
                       message.role === 'user' ? 'bg-blue-500 text-white' : 'bg-gray-100 text-gray-800']">
            {{ message.content }}
          </div>
        </div>
      </div>
      
      <!-- 输入框 -->
      <div class="p-6 border-t">
        <div class="flex gap-3">
          <input type="text" v-model="inputMessage" 
                 @keyup.enter="sendMessage" 
                 class="flex-1 px-6 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                 placeholder="请输入你的问题...">
          <button @click="sendMessage" 
                  class="px-8 py-3 bg-gradient-to-r from-blue-500 to-purple-600 text-white rounded-xl font-medium hover:shadow-lg transition-all transform hover:scale-105 disabled:opacity-50 disabled:pointer-events-none"
                  :disabled="!inputMessage.trim()">
            发送
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const chatContainer = ref(null)
const inputMessage = ref('')
const messages = ref([
  { role: 'assistant', content: '你好！我是你的AI运动助手，有什么关于健康运动的问题可以问我。' }
])

// 模拟AI回复
const aiResponses = [
  '拉伸运动应该在热身之后进行，每个动作保持15-30秒，避免过度拉伸。',
  '运动后肌肉酸痛是正常现象，可以通过适当的拉伸、热敷和按摩来缓解。',
  '每天最佳的运动时间是早晨或傍晚，避免在饭后立即运动。',
  '科学的训练计划应该包括有氧运动、力量训练和柔韧性训练的结合。',
  '运动前需要进行5-10分钟的热身，提高心率和肌肉温度。',
  '避免运动损伤的关键是：做好热身、使用正确的姿势、逐渐增加运动强度。'
]

// 发送消息
const sendMessage = () => {
  if (!inputMessage.value.trim()) return
  
  // 添加用户消息
  messages.value.push({ role: 'user', content: inputMessage.value.trim() })
  
  // 清空输入框
  const userMessage = inputMessage.value.trim()
  inputMessage.value = ''
  
  // 滚动到底部
  scrollToBottom()
  
  // 模拟AI回复
  setTimeout(() => {
    // 简单匹配问题类型
    let response = ''
    if (userMessage.includes('拉伸')) {
      response = aiResponses[0]
    } else if (userMessage.includes('酸痛')) {
      response = aiResponses[1]
    } else if (userMessage.includes('时间')) {
      response = aiResponses[2]
    } else if (userMessage.includes('计划')) {
      response = aiResponses[3]
    } else if (userMessage.includes('准备')) {
      response = aiResponses[4]
    } else if (userMessage.includes('损伤')) {
      response = aiResponses[5]
    } else {
      // 随机回复
      response = aiResponses[Math.floor(Math.random() * aiResponses.length)]
    }
    
    messages.value.push({ role: 'assistant', content: response })
    scrollToBottom()
  }, 1000)
}

// 滚动到底部
const scrollToBottom = () => {
  setTimeout(() => {
    if (chatContainer.value) {
      chatContainer.value.scrollTop = chatContainer.value.scrollHeight
    }
  }, 100)
}

// 返回首页
const goBack = () => {
  // 根据当前路由判断返回哪个首页
  if (router.currentRoute.value.path.includes('/student')) {
    router.push('/student')
  } else if (router.currentRoute.value.path.includes('/teacher')) {
    router.push('/teacher')
  } else {
    router.push('/login')
  }
}

// 监听消息变化，自动滚动到底部
watch(() => messages.value.length, () => {
  scrollToBottom()
})

// 页面加载时滚动到底部
onMounted(() => {
  scrollToBottom()
})
</script>