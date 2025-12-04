import { createRouter, createWebHistory } from 'vue-router'
import StudentHome from '../pages/StudentHome.vue'
import TeacherHome from '../pages/TeacherHome.vue'
import Assistant from '../pages/Assistant.vue'
import Login from '../pages/UserLogin.vue'
import Register from '../pages/UserRegister.vue'
import TeacherAssignments from '../pages/teacher/TeacherAssignments.vue'
//import StudentAssignments from '../pages/student/StudentAssignments.vue'
import SubmitAssignment from '../pages/student/SubmitAssignment.vue'
import PublishAssignment from '../pages/teacher/PublishAssignment.vue'
import GradeManagement from '../pages/teacher/GradeManagement.vue'
import TeachingVideos from '../pages/teacher/TeachingVideos.vue'

const routes = [
  { path: '/', component: Login },
  { path: '/login', component: Login },
  { path: '/register', component: Register },

  // 学生端路由
  { path: '/student', component: StudentHome },
  //{ path: '/student/assignments', component: StudentAssignments },
  { path: '/student/submit/:id', component: SubmitAssignment },
  { path: '/student/assistant', component: Assistant },

  // 教师端路由
  { path: '/teacher', component: TeacherHome },
  { path: '/teacher/assignments', component: TeacherAssignments },
  { path: '/teacher/publish', component: PublishAssignment },
  { path: '/teacher/grade/:id', component: GradeManagement },
  { path: '/teacher/videos', component: TeachingVideos },
  { path: '/teacher/assistant', component: Assistant },

  // 公共路由
  { path: '/assistant', component: Assistant }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫，实现根据登录身份显示不同主页
router.beforeEach((to, from, next) => {
  // 获取用户信息
  const user = localStorage.getItem('user')

  // 如果用户已登录且访问的是根路径或登录页面
  if (user && (to.path === '/' || to.path === '/login')) {
    const userInfo = JSON.parse(user)
    // 根据用户角色跳转到对应的主页
    if (userInfo.role === 'student') {
      next('/student')
    } else if (userInfo.role === 'teacher') {
      next('/teacher')
    } else {
      next()
    }
  } else {
    next()
  }
})

export default router
