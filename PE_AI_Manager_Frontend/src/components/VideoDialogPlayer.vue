<template>
  <el-dialog
    v-model="visible"
    :title="title"
    width="96%"
    :fullscreen="isMobile"
    top="3vh"
    :close-on-click-modal="false"
    :close-on-press-escape="true"
    :destroy-on-close="true"
    center
    custom-class="video-dialog"
  >
    <div class="video-container">
      <video
        v-if="videoUrl"
        :src="videoUrl"
        controls
        autoplay
        muted
        playsinline
        preload="metadata"
        class="video-element"
      >
        您的浏览器不支持 video 标签。
      </video>
      <div v-else class="no-video">
        暂无视频
      </div>
    </div>

    <template #footer>
      <el-button @click="visible = false" size="large">关闭</el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref, watch, computed } from 'vue'

const props = defineProps({
  visible: Boolean,
  videoUrl: {
    type: String,
    default: ''
  },
  title: {
    type: String,
    default: '视频播放'
  }
})

const emit = defineEmits(['update:visible'])

const visible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

// 检测是否移动端
const isMobile = computed(() => {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
})
</script>

<style scoped>
:deep(.video-dialog .el-dialog) {
  max-width: 1600px;
  height: 94vh;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
}

:deep(.video-dialog .el-dialog__body) {
  flex: 1;
  padding: 16px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #000;
}

.video-container {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #000;
  border-radius: 12px;
  overflow: hidden;
}

.video-element {
  width: 100%;
  height: 100%;
  max-height: 85vh;
  object-fit: contain;
  background: #000;
}

.no-video {
  color: #888;
  font-size: 24px;
}

/* 移动端优化 */
@media (max-width: 768px) {
  :deep(.video-dialog .el-dialog) {
    width: 100% !important;
    height: 100vh !important;
    max-height: 100vh;
    border-radius: 0;
    margin: 0 !important;
  }

  :deep(.video-dialog .el-dialog__header) {
    padding: 16px;
  }

  :deep(.video-dialog .el-dialog__body) {
    padding: 0;
  }
}
</style>
