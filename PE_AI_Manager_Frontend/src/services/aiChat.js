import axios from 'axios'

// 创建axios实例
const aiChatClient = axios.create({
  baseURL: 'http://118.25.145.4:5000/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
aiChatClient.interceptors.request.use(
  config => {
    return config
  },
  error => {
    return Promise.reject(error)
  }
)

// 响应拦截器
aiChatClient.interceptors.response.use(
  response => {
    return response
  },
  error => {
    console.error('AI Chat API Error:', error)
    return Promise.reject(error)
  }
)

/**
 * 获取用户会话列表
 * @param {string} userId - 用户ID
 * @returns {Promise} 会话列表
 */
export const getUserSessions = async (userId) => {
  try {
    const response = await aiChatClient.get('/sessions', {
      params: { user_id: userId }
    })
    return response.data
  } catch (error) {
    console.error('获取会话列表失败:', error)
    throw error
  }
}

/**
 * 创建新会话
 * @param {string} userId - 用户ID
 * @param {string} model - AI模型（可选，默认为'Qwen'）
 * @returns {Promise} 新会话信息
 */
export const createSession = async (userId, model = 'Qwen') => {
  try {
    const response = await aiChatClient.post('/sessions', {
      user_id: userId,
      model: model
    })
    return response.data
  } catch (error) {
    console.error('创建会话失败:', error)
    throw error
  }
}

/**
 * 获取用户的最新会话
 * @param {string} userId - 用户ID
 * @returns {Promise} 最新会话信息
 */
export const getLatestSession = async (userId) => {
  try {
    const response = await aiChatClient.get(`/sessions/user/${userId}`)
    return response.data
  } catch (error) {
    console.error('获取最新会话失败:', error)
    throw error
  }
}

/**
 * 获取指定会话详情
 * @param {number} sessionId - 会话ID
 * @returns {Promise} 会话详情
 */
export const getSessionDetails = async (sessionId) => {
  try {
    const response = await aiChatClient.get(`/sessions/${sessionId}`)
    return response.data
  } catch (error) {
    console.error('获取会话详情失败:', error)
    throw error
  }
}

/**
 * 删除会话
 * @param {number} sessionId - 会话ID
 * @returns {Promise} 删除结果
 */
export const deleteSession = async (sessionId) => {
  try {
    const response = await aiChatClient.delete(`/sessions/${sessionId}`)
    return response.data
  } catch (error) {
    console.error('删除会话失败:', error)
    throw error
  }
}

/**
 * 发送消息
 * @param {number} sessionId - 会话ID
 * @param {string} message - 消息内容
 * @param {string} model - AI模型（可选）
 * @returns {Promise} AI回复
 */
export const sendMessage = async (sessionId, message, model) => {
  try {
    const payload = { message }
    if (model) {
      payload.model = model
    }
    const response = await aiChatClient.post(`/sessions/${sessionId}/messages`, payload)
    return response.data
  } catch (error) {
    console.error('发送消息失败:', error)
    throw error
  }
}

/**
 * 清空会话消息
 * @param {number} sessionId - 会话ID
 * @returns {Promise} 清空结果
 */
export const clearSession = async (sessionId) => {
  try {
    const response = await aiChatClient.post(`/sessions/${sessionId}/clear`)
    return response.data
  } catch (error) {
    console.error('清空会话失败:', error)
    throw error
  }
}

/**
 * 导出会话
 * @param {number} sessionId - 会话ID
 * @returns {Promise} Markdown文件
 */
export const exportSession = async (sessionId) => {
  try {
    const response = await aiChatClient.get(`/sessions/${sessionId}/export`, {
      responseType: 'blob'
    })
    return response
  } catch (error) {
    console.error('导出会话失败:', error)
    throw error
  }
}

/**
 * 获取支持的模型列表
 * @returns {Promise} 模型列表
 */
export const getModels = async () => {
  try {
    const response = await aiChatClient.get('/models')
    return response.data
  } catch (error) {
    console.error('获取模型列表失败:', error)
    throw error
  }
}

export default aiChatClient
