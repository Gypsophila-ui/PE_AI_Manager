<template>
  <div class="flex h-screen bg-gradient-to-br from-blue-50 to-white">
    <!-- 会话侧边栏 -->
    <div class="w-80 bg-white border-r border-blue-200 flex flex-col">
      <!-- 侧边栏头部 -->
      <div class="p-4 border-b border-blue-200 bg-white">
        <h1 class="text-2xl font-bold text-gray-800 mb-4">🏋️‍♂️ AI运动助手</h1>
        <button @click="createNewSession"
                class="w-full px-4 py-3 bg-gradient-to-r from-blue-500 to-blue-600 text-white rounded-xl font-medium hover:shadow-lg transition-all transform hover:scale-105 flex items-center justify-center gap-2">
          <span class="text-xl">+</span>
          新建对话
        </button>
      </div>

      <!-- 会话列表 -->
      <div class="flex-1 overflow-y-auto p-4 space-y-2">
        <div v-if="loadingSessions" class="text-center text-blue-500 py-8">
          加载中...
        </div>
        <div v-else-if="sessions.length === 0" class="text-center text-blue-300 py-8">
          暂无会话
        </div>
        <div v-else>
          <div v-for="session in sessions" :key="session.session_id"
               @click="switchSession(session.session_id)"
               :class="['p-3 rounded-xl cursor-pointer transition-all',
                        currentSessionId === session.session_id
                          ? 'bg-blue-50 border-2 border-blue-500'
                          : 'hover:bg-blue-50 border-2 border-transparent']">
            <div class="flex items-center justify-between">
              <div class="flex-1 min-w-0">
                <div class="font-medium text-gray-800 truncate">{{ session.title }}</div>
                <div class="text-xs text-blue-500 mt-1">{{ session.model }}</div>
              </div>
              <button @click.stop="deleteSession(session.session_id)"
                      class="ml-2 text-blue-400 hover:text-red-500 transition-colors">
                <span class="text-lg">×</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 侧边栏底部 -->
      <div class="p-4 border-t border-blue-200">
        <button @click="goBack" class="w-full px-4 py-3 bg-blue-50 text-blue-700 rounded-xl font-medium hover:bg-blue-100 transition-all">
          返回首页
        </button>
      </div>
    </div>

    <!-- 主聊天区域 -->
    <div class="flex-1 flex flex-col">
      <!-- 顶部工具栏 -->
      <div class="bg-white border-b border-blue-200 p-4 flex items-center justify-between">
        <div class="flex items-center gap-4">
          <h2 class="text-xl font-bold text-blue-800">
            {{ currentSession?.title || '新对话' }}
          </h2>
          <select v-model="selectedModel"
                  @change="changeModel"
                  class="px-3 py-1.5 border border-blue-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
            <option v-for="model in availableModels" :key="model" :value="model">
              {{ model }}
            </option>
          </select>
        </div>
        <div class="flex gap-2">
          <button @click="clearCurrentSession"
                  class="px-4 py-2 bg-blue-50 text-blue-700 rounded-lg font-medium hover:bg-blue-100 transition-all flex items-center gap-2">
            <span>🗑️</span>
            清空
          </button>
          <button @click="exportCurrentSession"
                  class="px-4 py-2 bg-gradient-to-r from-blue-500 to-blue-600 text-white rounded-lg font-medium hover:shadow-lg transition-all flex items-center gap-2">
            <span>📥</span>
            导出
          </button>
        </div>
      </div>

      <!-- 聊天记录 -->
      <div class="flex-1 overflow-y-auto p-6 space-y-4" ref="chatContainer">
        <div v-if="loadingMessages" class="text-center text-blue-500 py-8">
          加载消息中...
        </div>
        <div v-else-if="messages.length === 0" class="text-center text-blue-300 py-8">
          开始新的对话吧！
        </div>
        <div v-else>
          <div v-for="(message, index) in messages" :key="index"
               :class="['flex', message.role === 'user' ? 'justify-end' : 'justify-start']">
            <div :class="['max-w-[80%] p-4 rounded-2xl shadow',
                         message.role === 'user'
                           ? 'bg-gradient-to-r from-blue-500 to-blue-600 text-white'
                           : 'bg-white text-gray-800 border border-blue-200']">
              <div class="whitespace-pre-wrap">{{ message.content }}</div>
              <div v-if="message.model" class="text-xs mt-2 opacity-70">
                {{ message.model }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 输入区域 -->
      <div class="bg-white border-t border-blue-200 p-4">
        <div class="flex gap-3">
          <textarea v-model="inputMessage"
                    @keyup.enter.exact="sendMessage"
                    @keydown.enter.shift.prevent="inputMessage += '\n'"
                    class="flex-1 px-4 py-3 rounded-xl border border-blue-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all resize-none"
                    placeholder="请输入你的问题...（Shift+Enter换行）"
                    rows="1"
                    style="min-height: 48px; max-height: 200px;"></textarea>
          <button @click="sendMessage"
                  class="px-6 py-3 bg-gradient-to-r from-blue-500 to-blue-600 text-white rounded-xl font-medium hover:shadow-lg transition-all transform hover:scale-105 disabled:opacity-50 disabled:pointer-events-none self-end"
                  :disabled="!inputMessage.trim() || sendingMessage">
            {{ sendingMessage ? '发送中...' : '发送' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import * as aiChat from '../services/aiChat'

const router = useRouter()

// 响应式数据
const chatContainer = ref(null)
const inputMessage = ref('')
const messages = ref([])
const sessions = ref([])
const currentSessionId = ref(null)
const currentSession = ref(null)
const loadingSessions = ref(false)
const loadingMessages = ref(false)
const sendingMessage = ref(false)
const selectedModel = ref('Qwen')
const availableModels = ref(['Qwen', 'ERNIE', 'Moonshot'])

// 获取当前用户ID
const getUserId = () => {
  const user = JSON.parse(localStorage.getItem('user') || '{}')
  return user.id || 'default_user'
}

// 加载会话列表
const loadSessions = async () => {
  loadingSessions.value = true
  try {
    const userId = getUserId()
    const result = await aiChat.getUserSessions(userId)
    if (result.success && result.data) {
      sessions.value = result.data
    }
  } catch (error) {
    console.error('加载会话列表失败:', error)
  } finally {
    loadingSessions.value = false
  }
}

// 创建新会话
const createNewSession = async () => {
  try {
    const userId = getUserId()
    const result = await aiChat.createSession(userId, selectedModel.value)
    if (result.success && result.data) {
      currentSessionId.value = result.data.session_id
      currentSession.value = result.data.session
      messages.value = result.data.session.messages || []

      // 刷新会话列表
      await loadSessions()

      // 滚动到底部
      scrollToBottom()
    }
  } catch (error) {
    console.error('创建会话失败:', error)
    // 如果API调用失败，创建本地会话
    createLocalSession()
  }
}

// 创建本地会话（当API不可用时）
const createLocalSession = () => {
  const newSession = {
    session_id: Date.now(),
    title: `新对话-${new Date().toLocaleString('zh-CN')}`,
    model: selectedModel.value,
    messages: [
      {
        role: 'system',
        content: '你是一个体育健身数智化教学平台的AI助手，专门为用户提供运动健身相关的建议和指导。',
        model: selectedModel.value
      },
      {
        role: 'assistant',
        content: '🏋️‍♂️ 欢迎使用体育健身数智化教学平台！我是您的AI运动助手，有什么关于健康运动的问题可以问我。',
        model: selectedModel.value
      }
    ]
  }

  currentSessionId.value = newSession.session_id
  currentSession.value = newSession
  messages.value = newSession.messages
  sessions.value.unshift(newSession)

  scrollToBottom()
}

// 切换会话
const switchSession = async (sessionId) => {
  if (currentSessionId.value === sessionId) return

  loadingMessages.value = true
  try {
    const result = await aiChat.getSessionDetails(sessionId)
    if (result.success && result.data) {
      currentSessionId.value = sessionId
      currentSession.value = result.data
      messages.value = result.data.messages || []
      selectedModel.value = result.data.model || 'Qwen'

      // 滚动到底部
      scrollToBottom()
    }
  } catch (error) {
    console.error('切换会话失败:', error)
    // 从本地会话列表中查找
    const localSession = sessions.value.find(s => s.session_id === sessionId)
    if (localSession) {
      currentSessionId.value = sessionId
      currentSession.value = localSession
      messages.value = localSession.messages || []
      selectedModel.value = localSession.model || 'Qwen'
      scrollToBottom()
    }
  } finally {
    loadingMessages.value = false
  }
}

// 删除会话
const deleteSession = async (sessionId) => {
  if (!confirm('确定要删除这个会话吗？')) return

  try {
    await aiChat.deleteSession(sessionId)

    // 从会话列表中移除
    sessions.value = sessions.value.filter(s => s.session_id !== sessionId)

    // 如果删除的是当前会话，切换到其他会话或创建新会话
    if (currentSessionId.value === sessionId) {
      if (sessions.value.length > 0) {
        await switchSession(sessions.value[0].session_id)
      } else {
        await createNewSession()
      }
    }
  } catch (error) {
    console.error('删除会话失败:', error)
    // 从本地会话列表中移除
    sessions.value = sessions.value.filter(s => s.session_id !== sessionId)

    if (currentSessionId.value === sessionId) {
      if (sessions.value.length > 0) {
        await switchSession(sessions.value[0].session_id)
      } else {
        await createNewSession()
      }
    }
  }
}

// 发送消息
const sendMessage = async () => {
  if (!inputMessage.value.trim() || sendingMessage.value) return

  const userMessage = inputMessage.value.trim()
  inputMessage.value = ''

  // 添加用户消息
  messages.value.push({
    role: 'user',
    content: userMessage,
    model: selectedModel.value
  })

  scrollToBottom()

  sendingMessage.value = true
  try {
    const result = await aiChat.sendMessage(currentSessionId.value, userMessage, selectedModel.value)
    if (result.success && result.data) {
      // 更新会话和消息
      currentSession.value = result.data.session
      messages.value = result.data.session.messages || []

      // 更新会话列表中的标题
      const sessionIndex = sessions.value.findIndex(s => s.session_id === currentSessionId.value)
      if (sessionIndex !== -1) {
        sessions.value[sessionIndex].title = result.data.session.title
        sessions.value[sessionIndex].model = result.data.session.model
      }
    }
  } catch (error) {
    console.error('发送消息失败:', error)
    // 模拟AI回复
    setTimeout(() => {
      const responses = [
        '拉伸运动应该在热身之后进行，每个动作保持15-30秒，避免过度拉伸。',
        '运动后肌肉酸痛是正常现象，可以通过适当的拉伸、热敷和按摩来缓解。',
        '每天最佳的运动时间是早晨或傍晚，避免在饭后立即运动。',
        '科学的训练计划应该包括有氧运动、力量训练和柔韧性训练的结合。',
        '运动前需要进行5-10分钟的热身，提高心率和肌肉温度。',
        '避免运动损伤的关键是：做好热身、使用正确的姿势、逐渐增加运动强度。'
      ]
      messages.value.push({
        role: 'assistant',
        content: responses[Math.floor(Math.random() * responses.length)],
        model: selectedModel.value
      })
    }, 1000)
  } finally {
    sendingMessage.value = false
    scrollToBottom()
  }
}

// 清空当前会话
const clearCurrentSession = async () => {
  if (!confirm('确定要清空当前会话的所有消息吗？')) return

  try {
    await aiChat.clearSession(currentSessionId.value)
    messages.value = []
    scrollToBottom()
  } catch (error) {
    console.error('清空会话失败:', error)
    // 本地清空
    messages.value = []
    scrollToBottom()
  }
}

// 导出当前会话
const exportCurrentSession = async () => {
  try {
    const response = await aiChat.exportSession(currentSessionId.value)

    // 创建下载链接
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `会话_${currentSessionId.value}_${new Date().toISOString().slice(0, 10)}.md`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('导出会话失败:', error)
    // 本地导出
    const content = messages.value.map(msg => {
      const role = msg.role === 'user' ? '用户' : '助手'
      return `## ${role}\n\n${msg.content}\n\n---\n`
    }).join('\n')

    const blob = new Blob([content], { type: 'text/markdown' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `会话_${currentSessionId.value}_${new Date().toISOString().slice(0, 10)}.md`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  }
}

// 切换模型
const changeModel = () => {
  console.log('切换模型为:', selectedModel.value)
  // 模型切换会在发送消息时生效
}

// 滚动到底部
const scrollToBottom = () => {
  nextTick(() => {
    if (chatContainer.value) {
      chatContainer.value.scrollTop = chatContainer.value.scrollHeight
    }
  })
}

// 返回首页
const goBack = () => {
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

// 页面加载时初始化
onMounted(async () => {
  try {
    // 加载模型列表
    const modelsResult = await aiChat.getModels()
    if (modelsResult.success && modelsResult.data) {
      availableModels.value = modelsResult.data
    }

    // 加载会话列表
    await loadSessions()

    // 获取用户的最新会话
    const userId = getUserId()
    const latestSessionResult = await aiChat.getLatestSession(userId)

    if (latestSessionResult.success && latestSessionResult.data) {
      await switchSession(latestSessionResult.data.session_id)
    } else if (sessions.value.length > 0) {
      // 如果没有最新会话，使用第一个会话
      await switchSession(sessions.value[0].session_id)
    } else {
      // 如果没有会话，创建新会话
      await createNewSession()
    }
  } catch (error) {
    console.error('初始化失败:', error)
    // 如果API调用失败，创建本地会话
    createLocalSession()
  }
})
</script>

<style scoped>
/* 自定义滚动条样式 */
::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
}

::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #555;
}

/* 文本域自动调整高度 */
textarea {
  overflow-y: auto;
}
</style>
