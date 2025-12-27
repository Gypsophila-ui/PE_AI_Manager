<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-purple-50">
    <div class="max-w-4xl mx-auto p-6 space-y-8">
      <!-- 顶部导航 -->
      <div class="flex justify-between items-center py-4">
        <div class="flex items-center gap-2">
          <button @click="goBack" class="text-2xl text-gray-600 hover:text-gray-800 transition-colors">
            ←
          </button>
          <h1 class="text-2xl font-bold text-gray-800">体育作业平台</h1>
        </div>
        <div class="flex gap-4">
          <button @click="goToAssistant" class="px-4 py-2 rounded-xl bg-purple-500 text-white hover:bg-purple-600 transition-all shadow-lg">
            💬 AI助手
          </button>
          <button @click="logout" class="px-4 py-2 rounded-xl bg-gray-200 text-gray-800 hover:bg-gray-300 transition-all shadow">
            退出登录
          </button>
        </div>
      </div>

      <!-- 页面标题 -->
      <section>
        <h2 class="text-4xl font-bold text-gray-800 mb-8">
          {{ isEdit ? '✏️ 编辑课程' : '➕ 新建课程' }}
        </h2>
      </section>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center h-64">
        <div class="animate-spin rounded-full h-16 w-16 border-t-4 border-b-4 border-blue-500"></div>
      </div>

      <!-- 主表单卡片 -->
      <section v-else class="bg-white rounded-3xl shadow-xl p-8">
        <form @submit.prevent="submitForm">
          <div class="space-y-8">

            <!-- 课号（course_id） -->
            <div>
              <label for="courseId" class="block text-sm font-medium text-gray-700 mb-2">
                课号 <span class="text-red-500">*</span>
              </label>
              <input
                id="courseId"
                v-model="form.courseId"
                type="text"
                required
                maxlength="50"
                :disabled="isEdit"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm disabled:bg-gray-100"
                placeholder="例如：PE20250101（建议包含年份和班级）"
              />
              <p class="text-xs text-gray-500 mt-2">
                {{ isEdit ? '课号不可修改' : '唯一标识课程，创建后不可更改，请谨慎填写' }}
              </p>
            </div>

            <!-- 课程名称 -->
            <div>
              <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
                课程名称 <span class="text-red-500">*</span>
              </label>
              <input
                id="name"
                v-model="form.name"
                type="text"
                required
                maxlength="100"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                placeholder="例如：初三（1）班 体育 - 体能与技能提升"
              />
            </div>

            <!-- 开课学期 -->
            <div>
              <label for="semester" class="block text-sm font-medium text-gray-700 mb-2">
                开课学期 <span class="text-red-500">*</span>
              </label>
              <input
                id="semester"
                v-model="form.semester"
                type="text"
                required
                pattern="[0-9]{6}"
                placeholder="例如：202501"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
              />
              <p class="text-xs text-gray-500 mt-2">格式：6位数字，如 202501 表示 2025年第一学期</p>
            </div>

            <!-- 课程描述 -->
            <div>
              <label for="info" class="block text-sm font-medium text-gray-700 mb-2">课程描述（可选）</label>
              <textarea
                id="info"
                v-model="form.info"
                rows="5"
                maxlength="3000"
                class="w-full px-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-blue-500 shadow-sm"
                placeholder="介绍本课程的目标、教学安排、上课时间等..."
              ></textarea>
            </div>

            <!-- 是否发布 -->
            <div>
              <label class="flex items-center gap-3 cursor-pointer">
                <input
                  type="checkbox"
                  v-model="form.is_active"
                  class="w-5 h-5 text-blue-600 rounded focus:ring-blue-500"
                />
                <span class="text-lg font-medium text-gray-800">
                  课程状态：{{ form.is_active ? '进行中（学生可加入）' : '未发布（学生不可见）' }}
                </span>
              </label>
            </div>

            <!-- 邀请码（创建成功后显示） -->
            <div v-if="form.code">
              <label class="block text-sm font-medium text-gray-700 mb-2">课程邀请码</label>
              <div class="flex items-center gap-3">
                <span class="font-mono text-lg bg-gray-100 px-5 py-3 rounded-xl flex-1 text-center">
                  {{ form.code }}
                </span>
                <button
                  type="button"
                  @click="copyCode"
                  class="px-6 py-3 bg-blue-500 text-white rounded-xl hover:bg-blue-600 transition-all shadow"
                >
                  复制邀请码
                </button>
              </div>
              <p class="text-sm text-gray-500 mt-2">学生可通过此邀请码加入课程</p>
            </div>

            <!-- 新建提示 -->
            <div v-if="!isEdit && !form.code" class="bg-blue-50 border border-blue-200 rounded-xl p-4">
              <p class="text-blue-800">提交后将生成64位邀请码，可分享给学生加入</p>
            </div>

          </div>

          <!-- 提交按钮 -->
          <div class="mt-12 flex gap-4 justify-end">
            <button
              type="button"
              @click="goBack"
              class="px-8 py-3 rounded-xl border border-gray-300 text-gray-700 hover:bg-gray-50 transition-all shadow"
            >
              取消
            </button>
            <button
              type="submit"
              :disabled="submitting"
              class="px-8 py-3 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-all shadow-lg disabled:opacity-70"
            >
              {{ submitting ? '保存中...' : (isEdit ? '保存修改' : '创建课程') }}
            </button>
          </div>
        </form>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import apiClient from '../../services/axios.js'

const router = useRouter()
const route = useRoute()

const loading = ref(true)
const submitting = ref(false)
const currentUser = JSON.parse(localStorage.getItem('user') || '{}')
const teacherId = currentUser.id || ''
const jwt = currentUser.jwt || ''

const isEdit = computed(() => route.path.includes('/edit'))

const form = ref({
  courseId: '',
  name: '',
  semester: '',
  info: '',
  is_active: true,
  code: ''
})

const loadCourseData = async () => {
  if (!isEdit.value) {
    loading.value = false
    return
  }

  const courseId = route.params.courseId
  if (!courseId) {
    alert('无效的课程ID')
    goBack()
    return
  }

  try {
    const resp = await apiClient.post('/api/get_info_by_course_id', { First: courseId })
    if (resp.data[0] < 0) {
      alert('课程不存在或加载失败')
      goBack()
      return
    }

    const d = resp.data
    form.value = {
      courseId: courseId,          // 只读显示
      name: d[1],
      semester: d[4],
      info: d[2] || '',
      is_active: d[5] === '1',
      code: d[3]
    }
  } catch (err) {
    console.error(err)
    alert('加载课程信息失败')
  } finally {
    loading.value = false
  }
}

const submitForm = async () => {
  // 基础校验
  if (!form.value.courseId.trim() || !form.value.name.trim() || !form.value.semester.match(/^\d{6}$/)) {
    alert('请正确填写课号、课程名称和学期')
    return
  }

  submitting.value = true

  try {
    if (isEdit.value) {
      // 编辑：更新可修改字段
      const courseId = route.params.courseId

      // 更新 name + semester
      const editResp = await apiClient.post('/api/edit_course', {
        First: courseId,
        Second: teacherId,
        Third: jwt,
        Fourth: form.value.name,
        Fifth: form.value.semester
      })

      if (editResp.data[0] !== '1') {
        alert('更新课程信息失败')
        return
      }

      // 更新描述
      await apiClient.post('/api/edit_info', {
        First: courseId,
        Second: teacherId,
        Third: jwt,
        Fourth: form.value.info || ''
      })

      // 更新发布状态
      await apiClient.post('/api/edit_is_active', {
        First: courseId,
        Second: teacherId,
        Third: jwt,
        Fourth: form.value.is_active ? '1' : '0'
      })

      alert('课程修改成功！')
    } else {
      // 新建课程
      const resp = await apiClient.post('/api/new_course', {
        First: teacherId,
        Second: jwt,
        Third: form.value.name,
        Fourth: form.value.semester,
        Fifth: form.value.courseId.trim()  // 手动输入的课号
      })

      if (resp.data[0] < 0) {
        alert('创建失败：课号可能已存在或格式错误')
        return
      }

      const newCode = resp.data[1]  // 返回的邀请码

      // 补充描述和状态
      await apiClient.post('/api/edit_info', {
        First: form.value.courseId.trim(),
        Second: teacherId,
        Third: jwt,
        Fourth: form.value.info || ''
      })

      await apiClient.post('/api/edit_is_active', {
        First: form.value.courseId.trim(),
        Second: teacherId,
        Third: jwt,
        Fourth: form.value.is_active ? '1' : '0'
      })

      form.value.code = newCode
      alert('课程创建成功！邀请码已生成，可复制分享给学生')
    }

    router.push('/teacher')
  } catch (err) {
    console.error(err)
    alert('操作失败，请检查网络或重试')
  } finally {
    submitting.value = false
  }
}

const copyCode = async () => {
  try {
    await navigator.clipboard.writeText(form.value.code)
    alert('邀请码已复制到剪贴板！')
  } catch {
    alert('复制失败，请手动选中')
  }
}

const goBack = () => router.push('/teacher')
const goToAssistant = () => router.push('/teacher/assistant')
const logout = () => {
  localStorage.removeItem('user')
  router.push('/login')
}

onMounted(loadCourseData)
</script>
