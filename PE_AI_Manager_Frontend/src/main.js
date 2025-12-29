import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

// 引入 Element Plus
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import zhCn from 'element-plus/es/locale/lang/zh-cn'

// 添加错误处理
console.log('Vue应用初始化开始...')

const app = createApp(App)

// 全局错误处理
app.config.errorHandler = (err, instance, info) => {
  console.error('Vue全局错误:', err)
  console.error('错误信息:', info)
}

// 全局未捕获错误处理
window.addEventListener('error', (event) => {
  console.error('全局未捕获错误:', event.error)
})

// 路由错误处理
router.onError((error) => {
  console.error('路由错误:', error)
})

app.use(ElementPlus, {
  locale: zhCn
})

console.log('应用挂载前...')
app.use(router)
app.mount('#app')
console.log('应用挂载完成!')

