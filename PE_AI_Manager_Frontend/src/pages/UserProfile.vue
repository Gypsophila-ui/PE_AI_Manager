<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-blue-100 p-8 pt-28">
    <div class="max-w-3xl mx-auto bg-white rounded-3xl shadow-2xl overflow-hidden">
      <!-- 页面标题 -->
      <div class="p-8 border-b border-gray-100">
        <h1 class="text-3xl font-bold text-gray-800">个人信息</h1>
        <p class="text-gray-500 mt-1">管理您的账号信息</p>
      </div>

      <!-- 用户基本信息 -->
      <div class="p-8">
        <div class="flex items-center space-x-6 mb-8">
          <!-- 用户头像 -->
          <div class="w-24 h-24 bg-gradient-to-br from-blue-400 to-blue-600 rounded-full flex items-center justify-center text-white text-4xl font-bold">
            {{ userInitial }}
          </div>
          <!-- 用户信息 -->
          <div>
            <h2 class="text-2xl font-bold text-gray-800">{{ userInfo.name || userInfo.username }}</h2>
            <p class="text-gray-500 mt-1">{{ userRoleText }}</p>
          </div>
        </div>

        <!-- 详细信息卡片 -->
        <div class="bg-gray-50 rounded-2xl p-6 mb-8">
          <div class="flex justify-between items-center mb-4">
            <h3 class="text-lg font-semibold text-gray-800">账号详情</h3>
            <button
              @click="showEditInfoModal = true"
              class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-all text-sm"
            >
              编辑
            </button>
          </div>
          <div class="space-y-3">
            <div class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">账号类型:</span>
              <span class="col-span-2 text-gray-800">{{ userRoleText }}</span>
            </div>
            <div class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">{{ userInfo.role === 'student' ? '学号' : '工号' }}:</span>
              <span class="col-span-2 text-gray-800">{{ userInfo.username }}</span>
            </div>
            <div class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">姓名:</span>
              <span class="col-span-2 text-gray-800">{{ userInfo.name || '-' }}</span>
            </div>
            <div class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">性别:</span>
              <span class="col-span-2 text-gray-800">{{ userInfo.gender || '-' }}</span>
            </div>
            <div v-if="userInfo.role === 'student'" class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">专业:</span>
              <span class="col-span-2 text-gray-800">{{ userInfo.major || '-' }}</span>
            </div>
            <div v-if="userInfo.role === 'teacher'" class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">职称:</span>
              <span class="col-span-2 text-gray-800">{{ userInfo.title || '-' }}</span>
            </div>
            <div class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">所属学院:</span>
              <span class="col-span-2 text-gray-800">{{ userInfo.college || '-' }}</span>
            </div>
            <div class="grid grid-cols-3 gap-4">
              <span class="text-gray-500">所属系:</span>
              <span class="col-span-2 text-gray-800">{{ userInfo.department || '-' }}</span>
            </div>
          </div>
        </div>

        <!-- 修改密码区域 -->
        <div class="bg-gray-50 rounded-2xl p-6">
          <h3 class="text-lg font-semibold text-gray-800 mb-4">账号安全</h3>
          <button
            @click="showChangePasswordModal = true"
            class="w-full py-3 bg-blue-500 text-white font-bold rounded-xl shadow-lg hover:bg-blue-600 transition-all"
          >
            修改密码
          </button>
        </div>
      </div>
    </div>

    <!-- 编辑个人信息模态框 -->
    <div v-if="showEditInfoModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-3xl shadow-2xl max-w-md w-full">
        <div class="p-6 border-b border-gray-100">
          <h2 class="text-2xl font-bold text-gray-800">编辑个人信息</h2>
          <button @click="showEditInfoModal = false" class="absolute top-4 right-4 text-gray-400 hover:text-gray-600">
            <span class="text-2xl">&times;</span>
          </button>
        </div>
        <div class="p-6">
          <!-- 姓名 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">姓名</label>
            <input
              type="text"
              v-model="editInfo.name"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请输入姓名"
              maxlength="100"
            >
          </div>

          <!-- 性别 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">性别</label>
            <select v-model="editInfo.gender"
                    class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all">
              <option value="男">男</option>
              <option value="女">女</option>
            </select>
          </div>

          <!-- 教师特有字段：职称 -->
          <div v-if="userInfo.role === 'teacher'" class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">职称</label>
            <input
              type="text"
              v-model="editInfo.title"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请输入职称"
              maxlength="100"
            >
          </div>

          <!-- 学生特有字段：专业 -->
          <div v-if="userInfo.role === 'student'" class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">专业</label>
            <input
              type="text"
              v-model="editInfo.major"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请输入专业"
              maxlength="100"
            >
          </div>

          <!-- 学院 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">所属学院</label>
            <input
              type="text"
              v-model="editInfo.college"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请输入所属学院"
              maxlength="100"
            >
          </div>

          <!-- 系 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">所属系</label>
            <input
              type="text"
              v-model="editInfo.department"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请输入所属系"
              maxlength="100"
            >
          </div>

          <!-- 错误信息 -->
          <div v-if="infoError" class="text-red-500 text-sm mb-4">
            {{ infoError }}
          </div>

          <!-- 成功信息 -->
          <div v-if="infoSuccess" class="text-green-500 text-sm mb-4">
            {{ infoSuccess }}
          </div>

          <!-- 操作按钮 -->
          <div class="flex space-x-4">
            <button
              @click="showEditInfoModal = false"
              class="flex-1 py-3 bg-gray-200 text-gray-800 font-bold rounded-xl hover:bg-gray-300 transition-all"
            >
              取消
            </button>
            <button
              @click="handleChangeInfo"
              class="flex-1 py-3 bg-blue-500 text-white font-bold rounded-xl shadow-lg hover:bg-blue-600 transition-all"
              :disabled="infoLoading"
            >
              <span v-if="infoLoading">保存中...</span>
              <span v-else>保存</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 修改密码模态框 -->
    <div v-if="showChangePasswordModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-3xl shadow-2xl max-w-md w-full">
        <div class="p-6 border-b border-gray-100">
          <h2 class="text-2xl font-bold text-gray-800">修改密码</h2>
          <button @click="showChangePasswordModal = false" class="absolute top-4 right-4 text-gray-400 hover:text-gray-600">
            <span class="text-2xl">&times;</span>
          </button>
        </div>
        <div class="p-6">
          <!-- 旧密码 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">旧密码</label>
            <input
              type="password"
              v-model="oldPassword"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请输入旧密码"
            >
          </div>

          <!-- 新密码 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">新密码</label>
            <input
              type="password"
              v-model="newPassword"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请输入新密码"
            >
          </div>

          <!-- 确认新密码 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-1">确认新密码</label>
            <input
              type="password"
              v-model="confirmPassword"
              class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              placeholder="请再次输入新密码"
            >
          </div>

          <!-- 错误信息 -->
          <div v-if="passwordError" class="text-red-500 text-sm mb-4">
            {{ passwordError }}
          </div>

          <!-- 成功信息 -->
          <div v-if="passwordSuccess" class="text-green-500 text-sm mb-4">
            {{ passwordSuccess }}
          </div>

          <!-- 操作按钮 -->
          <div class="flex space-x-4">
            <button
              @click="showChangePasswordModal = false"
              class="flex-1 py-3 bg-gray-200 text-gray-800 font-bold rounded-xl hover:bg-gray-300 transition-all"
            >
              取消
            </button>
            <button
              @click="handleChangePassword"
              class="flex-1 py-3 bg-blue-500 text-white font-bold rounded-xl shadow-lg hover:bg-blue-600 transition-all"
              :disabled="passwordLoading"
            >
              <span v-if="passwordLoading">修改中...</span>
              <span v-else>确认修改</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { changeTeacherPassword, changeStudentPassword, changeTeacherInfo, changeStudentInfo, getTeacherInfo, getStudentInfo } from '../services/auth'

const router = useRouter()

// 用户信息
const userInfo = ref({
  role: '',
  username: '',
  name: '',
  gender: '',
  title: '',
  major: '',
  college: '',
  department: ''
})

// 编辑个人信息相关状态
const showEditInfoModal = ref(false)
const editInfo = ref({
  name: '',
  gender: '',
  title: '',
  major: '',
  college: '',
  department: ''
})
const infoError = ref('')
const infoSuccess = ref('')
const infoLoading = ref(false)

// 修改密码相关状态
const showChangePasswordModal = ref(false)
const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const passwordError = ref('')
const passwordSuccess = ref('')
const passwordLoading = ref(false)

// 计算属性
const userInitial = computed(() => {
  const name = userInfo.value.name || userInfo.value.username
  return name.charAt(0).toUpperCase()
})

const userRoleText = computed(() => {
  return userInfo.value.role === 'student' ? '学生' : '教师'
})

// 获取用户信息
onMounted(async () => {
  const user = localStorage.getItem('user')
  if (user) {
    userInfo.value = JSON.parse(user)

    try {
      let result
      const userId = userInfo.value.username
      const userType = userInfo.value.role === 'student' ? '0' : '1'

      if (userInfo.value.role === 'student') {
        result = await getStudentInfo(userId, userInfo.value.token, userType, userId)
      } else {
        result = await getTeacherInfo(userId, userInfo.value.token, userType, userId)
      }

      if (result.success) {
        userInfo.value = {
          ...userInfo.value,
          ...result.data
        }
        // 更新本地存储
        localStorage.setItem('user', JSON.stringify(userInfo.value))
      }
    } catch (error) {
      console.error('获取用户信息失败:', error)
    }

    // 初始化编辑信息
    editInfo.value = {
      name: userInfo.value.name || '',
      gender: userInfo.value.gender || '男',
      title: userInfo.value.title || '',
      major: userInfo.value.major || '',
      college: userInfo.value.college || '',
      department: userInfo.value.department || ''
    }
  } else {
    // 未登录，跳转到登录页
    router.push('/login')
  }
})

// 修改个人信息处理函数
const handleChangeInfo = async () => {
  console.log('开始修改个人信息')
  console.log('当前用户信息:', userInfo.value)
  console.log('编辑信息:', editInfo.value)

  // 表单验证
  if (!editInfo.value.name) {
    infoError.value = '请输入姓名'
    return
  }
  if (!editInfo.value.college) {
    infoError.value = '请输入所属学院'
    return
  }
  if (!editInfo.value.department) {
    infoError.value = '请输入所属系'
    return
  }
  if (userInfo.value.role === 'teacher' && !editInfo.value.title) {
    infoError.value = '请输入职称'
    return
  }
  if (userInfo.value.role === 'student' && !editInfo.value.major) {
    infoError.value = '请输入专业'
    return
  }

  infoError.value = ''
  infoSuccess.value = ''
  infoLoading.value = true

  try {
    let result
    const userId = userInfo.value.username
    console.log('用户ID:', userId)
    console.log('用户角色:', userInfo.value.role)

    // 根据用户角色调用不同的修改信息API
    if (userInfo.value.role === 'student') {
      console.log('调用学生修改信息API')
      result = await changeStudentInfo(
        userId,
        userInfo.value.token,
        editInfo.value.name,
        editInfo.value.gender,
        editInfo.value.major,
        editInfo.value.college,
        editInfo.value.department
      )
    } else {
      console.log('调用教师修改信息API')
      console.log('JWT token:', userInfo.value.token)
      result = await changeTeacherInfo(
        userId,
        userInfo.value.token,
        editInfo.value.name,
        editInfo.value.gender,
        editInfo.value.title,
        editInfo.value.college,
        editInfo.value.department
      )
    }

    console.log('API返回结果:', result)

    if (result.success) {
      infoSuccess.value = '个人信息修改成功！'
      // 更新本地用户信息
      userInfo.value = {
        ...userInfo.value,
        name: editInfo.value.name,
        gender: editInfo.value.gender,
        title: editInfo.value.title,
        major: editInfo.value.major,
        college: editInfo.value.college,
        department: editInfo.value.department
      }
      localStorage.setItem('user', JSON.stringify(userInfo.value))
      // 2秒后关闭模态框
      setTimeout(() => {
        showEditInfoModal.value = false
      }, 2000)
    } else {
      infoError.value = result.message || '个人信息修改失败'
    }
  } catch (error) {
    infoError.value = '修改个人信息时发生错误，请稍后重试'
    console.error('修改个人信息错误:', error)
  } finally {
    infoLoading.value = false
  }
}

// 修改密码处理函数
const handleChangePassword = async () => {
  // 表单验证
  if (!oldPassword.value) {
    passwordError.value = '请输入旧密码'
    return
  }
  if (!newPassword.value) {
    passwordError.value = '请输入新密码'
    return
  }
  if (newPassword.value !== confirmPassword.value) {
    passwordError.value = '两次输入的新密码不一致'
    return
  }
  if (newPassword.value.length < 6) {
    passwordError.value = '新密码长度不能少于6位'
    return
  }

  passwordError.value = ''
  passwordSuccess.value = ''
  passwordLoading.value = true

  try {
    let result
    const userId = userInfo.value.username

    // 根据用户角色调用不同的修改密码API
    if (userInfo.value.role === 'student') {
      result = await changeStudentPassword(userId, oldPassword.value, newPassword.value)
    } else {
      result = await changeTeacherPassword(userId, oldPassword.value, newPassword.value)
    }

    if (result.success) {
      passwordSuccess.value = '密码修改成功！'
      // 重置表单
      oldPassword.value = ''
      newPassword.value = ''
      confirmPassword.value = ''
      // 3秒后关闭模态框
      setTimeout(() => {
        showChangePasswordModal.value = false
      }, 3000)
    } else {
      passwordError.value = result.message || '密码修改失败'
    }
  } catch (error) {
    passwordError.value = '修改密码时发生错误，请稍后重试'
    console.error('修改密码错误:', error)
  } finally {
    passwordLoading.value = false
  }
}
</script>
