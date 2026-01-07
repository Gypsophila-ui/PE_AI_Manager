import axios from 'axios';

const BASE_URL = '/Teaching-video';

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
