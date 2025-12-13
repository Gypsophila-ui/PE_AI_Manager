import axios from 'axios';

// 创建axios实例
const apiClient = axios.create({
  baseURL: '/api', // API基础URL
  timeout: 10000, // 请求超时时间
  headers: {
    'Content-Type': 'application/json'
  }
});

// 请求拦截器
apiClient.interceptors.request.use(
  config => {
    // 从localStorage获取token
    const token = localStorage.getItem('token');
    // 如果token存在，添加到请求头
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// 响应拦截器
apiClient.interceptors.response.use(
  response => {
    // 直接返回响应数据
    return response;
  },
  error => {
    // 处理错误响应
    if (error.response) {
      // 服务器返回错误状态码
      if (error.response.status === 401) {
        // 未授权，清除token并重定向到登录页
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
      }
    } else if (error.request) {
      // 请求已发送但没有收到响应
      console.error('网络错误，无法连接到服务器');
    } else {
      // 请求配置出错
      console.error('请求错误', error.message);
    }
    return Promise.reject(error);
  }
);

export default apiClient;
