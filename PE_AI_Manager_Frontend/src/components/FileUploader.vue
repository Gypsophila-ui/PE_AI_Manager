<template>
  <div class="file-uploader-wrapper">
    <div class="file-uploader" :style="{ maxWidth: maxWidth }">
      <!-- 拖拽上传区域 -->
      <div
        class="upload-area"
        :class="{ 'drag-over': dragOver }"
        @drop.prevent="handleDrop"
        @dragover.prevent="dragOver = true"
        @dragenter.prevent="dragOver = true"
        @dragleave.prevent="dragOver = false"
        @click="triggerFileInput"
      >
        <input
          type="file"
          ref="fileInput"
          accept="video/mp4,.mp4"
          style="display: none"
          @change="handleFileChange"
        />
        <div class="upload-hint">
          <p>点击或将 MP4 视频文件拖拽到这里上传</p>
          <p class="tip">仅支持 MP4 格式，单个文件最大 100MB</p>
        </div>
      </div>

      <!-- 上传进度 -->
      <div v-if="uploading" class="progress-container">
        <div class="info">
          <p>正在上传：{{ fileName }} ({{ formatBytes(totalSize) }})</p>
          <p>已上传：{{ formatBytes(uploadedSize) }} ({{ progress.toFixed(1) }}%)</p>
          <p>速度：{{ uploadSpeed }}/s</p>
          <p>预计剩余时间：{{ remainingTime }}</p>
        </div>
        <el-progress :percentage="progress" :stroke-width="12" />
        <button @click="cancelUpload" class="cancel-btn">取消上传</button>
      </div>

      <!-- 上传成功提示 -->
      <div v-if="uploadResult && !uploading" class="result">
        <p>上传成功！</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { fileClient } from '@/services/fileClient';
import axios from 'axios';
import { ElMessage } from 'element-plus';

const emit = defineEmits(['uploaded']);

const uploading = ref(false);
const dragOver = ref(false);
const progress = ref(0);
const fileName = ref('');
const totalSize = ref(0);
const uploadedSize = ref(0);
const uploadSpeed = ref('0 KB');
const remainingTime = ref('--');
const uploadResult = ref(null);

const fileInput = ref(null);

const MAX_SIZE = 100 * 1024 * 1024; // 100MB

let cancelTokenSource = null;
let startTime = null;
let lastLoadedTime = null;

const props = defineProps({
  maxWidth: {
    type: String,
    default: '600px'  // 默认 600px
  }
});

const formatBytes = (bytes) => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

const triggerFileInput = () => {
  fileInput.value.click();
};

// 校验文件函数（类型 + 大小）
const validateFile = (file) => {
  if (!file) return false;

  // 检查类型
  if (file.type !== 'video/mp4' && !file.name.toLowerCase().endsWith('.mp4')) {
    ElMessage.warning('只允许上传 MP4 格式的视频文件！');
    return false;
  }

  // 检查大小
  if (file.size > MAX_SIZE) {
    ElMessage.warning('文件大小不能超过 100MB！');
    return false;
  }

  return true;
};

const handleFileChange = (e) => {
  const file = e.target.files[0];
  if (file && validateFile(file)) {
    uploadFile(file);
  }
  // 清空 input 值，允许重复上传同一个文件
  e.target.value = '';
};

const handleDrop = (e) => {
  dragOver.value = false;
  const file = e.dataTransfer.files[0];
  if (file && validateFile(file)) {
    uploadFile(file);
  }
};

// 其余上传逻辑保持不变（速度、进度、取消等）
const calculateSpeedAndTime = (loaded, total, timeElapsed) => {
  const speed = loaded / (timeElapsed / 1000);
  uploadSpeed.value = formatBytes(speed);

  if (speed > 0) {
    const remainingSeconds = Math.round((total - loaded) / speed);
    if (remainingSeconds < 60) {
      remainingTime.value = `${remainingSeconds} 秒`;
    } else {
      const min = Math.floor(remainingSeconds / 60);
      const sec = remainingSeconds % 60;
      remainingTime.value = `${min} 分 ${sec} 秒`;
    }
  }
};

const uploadFile = async (file) => {
  fileName.value = file.name;
  totalSize.value = file.size;
  uploadedSize.value = 0;
  progress.value = 0;
  uploadSpeed.value = '0 KB';
  remainingTime.value = '--';
  uploadResult.value = null;
  uploading.value = true;

  cancelTokenSource = axios.CancelToken.source();
  startTime = Date.now();
  lastLoadedTime = Date.now();

  const formData = new FormData();
  formData.append('file', file);

  try {
    const response = await fileClient.post('/upload', formData, {
      cancelToken: cancelTokenSource.token,
      headers: { 'Content-Type': undefined },
      onUploadProgress: (progressEvent) => {
        const { loaded, total } = progressEvent;
        const now = Date.now();
        const timeElapsed = (now - startTime) / 1000;

        uploadedSize.value = loaded;
        progress.value = (loaded / total) * 100;

        if (now - lastLoadedTime >= 1000 || loaded === total) {
          calculateSpeedAndTime(loaded, total, timeElapsed);
          lastLoadedTime = now;
        }
      },
    });

    uploadResult.value = response.data;
    emit('uploaded', response.data);
    ElMessage.success('上传成功！');
  } catch (error) {
    if (axios.isCancel(error)) {
      ElMessage.info('上传已取消');
    } else {
      ElMessage.error('上传失败: ' + (error.response?.data?.error || error.message));
    }
  } finally {
    uploading.value = false;
    cancelTokenSource = null;
  }
};

const cancelUpload = () => {
  if (cancelTokenSource) cancelTokenSource.cancel();
};
</script>

<style scoped>
.file-uploader-wrapper {
  display: flex;
  justify-content: center;
  padding: 20px;
  width: 100%;
}
.file-uploader {
  width: 100%;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}
.upload-area {
  border: 3px dashed #dcdfe6;
  border-radius: 12px;
  padding: 40px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s;
  background: #fafafa;
}
.upload-area.drag-over {
  border-color: #409eff;
  background: #f0f8ff;
}
.tip {
  font-size: 14px;
  color: #909399;
}
.progress-container {
  margin-top: 30px;
}
.info p {
  margin: 8px 0;
}
.cancel-btn {
  margin-top: 15px;
  padding: 8px 16px;
  background: #f56c6c;
  color: white;
  border: none;
  border-radius: 6px;
}
.result {
  margin-top: 20px;
  padding: 15px;
  background: #f0f9eb;
  border-radius: 8px;
}
</style>
