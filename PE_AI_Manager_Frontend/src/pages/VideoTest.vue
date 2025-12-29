<template>
  <div class="file-manager">
    <h2>视频文件管理</h2>

    <FileUploader @uploaded="onUploaded" />

    <hr />

    <FileUploader max-width="1500px" @uploaded="onUploaded" />

    <h3>已上传文件（{{ files.length }} 个）</h3>
    <ul class="file-list">
      <li v-for="file in files" :key="file.filename" class="file-item">
        <div class="file-info">
          <strong>{{ file.filename }}</strong><br />
          <small>{{ formatBytes(file.size) }} - {{ file.modified }}</small>
        </div>
        <div class="actions">
          <!-- 弹窗播放 -->
          <el-button type="primary" size="small" @click="openVideoDialog(file.filename)">
            弹窗播放
          </el-button>

          <!-- 页面内直接播放 -->
          <el-button type="success" size="small" @click="playInline(file.filename)">
            页面播放
          </el-button>

          <!-- 删除 -->
          <el-button type="danger" size="small" @click="deleteFile(file.filename)">
            删除
          </el-button>
        </div>
      </li>
    </ul>

    <div class="inline-player-section">
      <h3>页面内播放器</h3>
      <InlineVideoPlayer :src="inlineVideoUrl" />
    </div>

    <VideoDialogPlayer
      :visible="dialogVisible"
      :video-url="currentVideoUrl"
      @update:visible="dialogVisible = $event"
      title="test_title"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import FileUploader from '@/components/FileUploader.vue';
import VideoDialogPlayer from '@/components/VideoDialogPlayer.vue';
import InlineVideoPlayer from '@/components/InlineVideoPlayer.vue';
import { fileClient } from '@/services/fileClient';
import { ElMessageBox } from 'element-plus';

const files = ref([]);
const dialogVisible = ref(false);
const currentVideoUrl = ref('');
const inlineVideoUrl = ref('');

const fetchFileList = async () => {
  try {
    const res = await fileClient.get('/list');
    files.value = res.data.files || [];
  } catch (error) {
    console.error(error);
  }
};

// 弹窗播放
const openVideoDialog = (filename) => {
  currentVideoUrl.value = `${fileClient.defaults.baseURL}/files/${filename}`;
  dialogVisible.value = true;
};

// 页面内直接播放
const playInline = (filename) => {
  inlineVideoUrl.value = `${fileClient.defaults.baseURL}/files/${filename}`;
  // 滚动到播放器区域
  document.querySelector('.inline-player-section').scrollIntoView({ behavior: 'smooth' });
};

const deleteFile = async (filename) => {
  ElMessageBox.confirm('确定要删除这个视频吗？', '警告', { type: 'warning' })
    .then(async () => {
      await fileClient.delete(`/delete/${filename}`);
      await fetchFileList();
      // 如果正在播放的视频被删了，清空播放器
      if (currentVideoUrl.value.includes(filename)) currentVideoUrl.value = '';
      if (inlineVideoUrl.value.includes(filename)) inlineVideoUrl.value = '';
    })
    .catch(() => {});
};

const onUploaded = () => {
  fetchFileList();
};

const formatBytes = (bytes) => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

onMounted(fetchFileList);
</script>

<style scoped>
.file-list {
  list-style: none;
  padding: 0;
  margin: 20px 0;
}
.file-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border-bottom: 1px solid #eee;
  background: #fff;
  border-radius: 8px;
  margin-bottom: 8px;
}
.file-info {
  flex: 1;
}
.actions .el-button {
  margin-left: 8px;
}
.inline-player-section {
  margin-top: 40px;
  padding: 20px;
  background: #f9f9f9;
  border-radius: 12px;
}
.inline-player-section h3 {
  margin-bottom: 20px;
  color: #333;
}
</style>
