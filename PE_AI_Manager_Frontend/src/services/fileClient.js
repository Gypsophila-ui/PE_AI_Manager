import axios from 'axios';

const BASE_URL = 'http://47.121.177.100:5002';

export const fileClient = axios.create({
  baseURL: BASE_URL,
  timeout: 0, // 文件上传/下载不设置超时
});

// 下载视频流时专用（返回 blob）
export const fileDownloadClient = axios.create({
  baseURL: BASE_URL,
  timeout: 0,
  responseType: 'blob',
});
