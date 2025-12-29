<template>
  <div class="video-player">
    <video
      v-if="src"
      :src="src"
      controls
      autoplay
      muted
      playsinline
      preload="metadata"
      class="video-element"
    >
      您的浏览器不支持 video 标签。
    </video>
  </div>
</template>

<script setup>
defineProps({
  src: {
    type: String,
    default: '',
  },
});
</script>

<style scoped>
.video-player {
  width: 100%;
  height: 100%;
  min-height: 60vh;
  max-height: 92vh;
  padding: 16px;
  background: #000;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
}

.video-element {
  width: 100%;
  height: 100%;
  max-width: 100%;
  max-height: 92vh;
  border-radius: 8px;
  object-fit: contain;
  background: #000;
}

.placeholder {
  color: #888;
  font-size: 24px;
  text-align: center;
}

@media (max-width: 768px) {
  .video-player {
    min-height: 70vh;
    padding: 8px;
    border-radius: 8px;
  }
}
</style>
