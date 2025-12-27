import { createRouter, createWebHistory } from 'vue-router'
import StudentHome from '../pages/StudentHome.vue'
import TeacherHome from '../pages/TeacherHome.vue'
import Assistant from '../pages/Assistant.vue'
import Login from '../pages/UserLogin.vue'
import Register from '../pages/UserRegister.vue'
import UserProfile from '../pages/UserProfile.vue'
import TeacherAssignments from '../pages/teacher/TeacherAssignments.vue'
import StudentAssignments from '../pages/student/StudentAssignments.vue'

import PublishAssignment from '../pages/teacher/PublishAssignment.vue'
import GradeManagement from '../pages/teacher/GradeManagement.vue'
import TeachingVideos from '../pages/teacher/TeachingVideos.vue'
import CourseDetails from '../pages/CourseDetails.vue'
import TeacherCourseDetails from '../pages/teacher/TeacherCourseDetails.vue'
import TeacherAssignmentDetcail from '../pages/teacher/TeacherAssignmentDetcail.vue'
import CourseCreateEdit from '../pages/teacher/CourseCreateEdit.vue'
import CourseStudents from '../pages/teacher/CourseStudents.vue'
import TeacherDashboard from '../pages/teacher/TeacherDashboard.vue'

const routes = [
  { path: '/', component: Login },
  { path: '/login', component: Login },
  { path: '/register', component: Register },

  // 学生端路由
  { path: '/student', component: StudentHome },
  { path: '/student/course/:courseId', component: CourseDetails },
  { path: '/student/course/:courseId/assignments/:assignmentId', component: StudentAssignments },

  // 教师端路由
  { path: '/teacher', component: TeacherHome },
  { path: '/teacher/assignments', component: TeacherAssignments },
  { path: '/teacher/publish', component: PublishAssignment },
  { path: '/teacher/grade/course/:courseId/assignment/:assignmentId', component: GradeManagement },
  { path: '/teacher/course/:courseId/assignment/:assignmentId', component: TeacherAssignmentDetcail },
  { path: '/teacher/videos', component: TeachingVideos },
  { path: '/teacher/course/:courseId', component: TeacherCourseDetails },
  { path: '/teacher/course/:courseId/edit', component: CourseCreateEdit },
  { path: '/teacher/createCourse/', component: CourseCreateEdit },
  { path: '/teacher/course/:courseId/students', component: CourseStudents },
  { path: '/teacher/dashboard', component: TeacherDashboard },

  // 公共路由
  { path: '/assistant', component: Assistant },
  // 个人信息页面
  { path: '/profile', component: UserProfile }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 路由守卫，实现根据登录身份显示不同主页和权限控制
router.beforeEach((to, from, next) => {
  // 获取用户信息
  const user = localStorage.getItem('user')
  const userInfo = user ? JSON.parse(user) : null

  // 定义需要认证的路由
  const requiresAuth = [
    '/student',
    '/teacher',
    '/teacher/assignments',
    '/teacher/publish',
    '/teacher/grade/:id',
    '/teacher/videos',
    '/teacher/assistant',
    '/teacher/course',
    '/student/assignments/:id',
    '/student/submit/:id',
    '/student/assistant',
    '/course',
    '/profile'
  ]

  // 检查当前路由是否需要认证
  const isAuthRequired = requiresAuth.some(route =>
    to.path.startsWith(route.replace(':id', ''))
  )

  // 如果需要认证但用户未登录
  if (isAuthRequired && !userInfo) {
    next('/login')
    return
  }

  // 根据角色控制页面访问权限
  if (userInfo) {
    // 学生角色只能访问学生相关页面
    if (userInfo.role === 'student' && to.path.startsWith('/teacher')) {
      next('/student')
      return
    }

    // 教师角色只能访问教师相关页面
    if (userInfo.role === 'teacher' && to.path.startsWith('/student')) {
      next('/teacher')
      return
    }

    // 如果用户已登录且访问的是根路径或登录页面，跳转到对应角色的主页
    if (to.path === '/' || to.path === '/login') {
      if (userInfo.role === 'student') {
        next('/student')
      } else if (userInfo.role === 'teacher') {
        next('/teacher')
      } else {
        next()
      }
      return
    }
  }

  next()
})

export default router
