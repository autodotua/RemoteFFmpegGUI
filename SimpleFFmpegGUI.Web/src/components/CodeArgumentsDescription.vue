
<template>
  <div>
    <el-descriptions title="视频" v-if="type == 0">
      <el-descriptions-item label="策略">{{
        args.video == null
          ? args.disableVideo
            ? "不导出"
            : "不重新编码"
          : "重新编码"
      }}</el-descriptions-item>
      <el-descriptions-item label="编码" v-if="showVideo">{{
        args.video.code == "" || args.video.code == null
          ? "自动"
          : args.video.code
      }}</el-descriptions-item>
      <el-descriptions-item label="预设" v-if="showVideo">{{
        args.video.preset
      }}</el-descriptions-item>
      <el-descriptions-item label="CRF" v-if="showVideo">{{
        args.video.crf ? args.video.crf : "未定义"
      }}</el-descriptions-item>
      <el-descriptions-item label="帧率" v-if="showVideo">{{
        args.video.fps ? args.video.fps : "未定义"
      }}</el-descriptions-item>
      <el-descriptions-item label="平均码率" v-if="showVideo">{{
        args.video.averageBitrate ? args.video.averageBitrate : "未定义"
      }}</el-descriptions-item>
      <el-descriptions-item label="最高码率" v-if="showVideo">{{
        args.video.maxBitrate ? args.video.maxBitrate : "未定义"
      }}</el-descriptions-item>
      <el-descriptions-item label="最高码率缓冲倍率" v-if="showVideo">{{
        args.video.maxBitrateBuffer ? args.video.maxBitrateBuffer : "未定义"
      }}</el-descriptions-item>
    </el-descriptions>
    <el-descriptions title="音频" v-if="type == 0">
      <el-descriptions-item label="策略">{{
        args.audio == null
          ? args.disableAudio
            ? "不导出"
            : "不重新编码"
          : "重新编码"
      }}</el-descriptions-item>
      <el-descriptions-item label="编码" v-if="showAudio">{{
        args.audio.code == "" || args.audio.code == null
          ? "自动"
          : args.audio.code
      }}</el-descriptions-item>

      <el-descriptions-item label="码率" v-if="showAudio">{{
        args.audio.bitrate ? args.audio.bitrate : "未定义"
      }}</el-descriptions-item>
      <el-descriptions-item label="采样率" v-if="showAudio">{{
        args.audio.samplingRate ? args.audio.samplingRate : "未定义"
      }}</el-descriptions-item>
    </el-descriptions>
    <el-descriptions title="容器">
      <el-descriptions-item label="格式" v-if="showAudio">{{
        args.format ? args.format : "未定义"
      }}</el-descriptions-item>
    </el-descriptions>
    <el-descriptions title="合并参数" v-if="type == 1">
      <el-descriptions-item label="格式">{{
        args.combine
          ? args.combine.shortest
            ? "裁剪到最短的媒体"
            : "最后部分静帧或黑屏"
          : "未定义"
      }}</el-descriptions-item>
    </el-descriptions>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";
import { showError, showSuccess } from "../common";
export default Vue.component("code-arguments-description", {
  data() {
    return {
      speedPresets: {
        0: "最慢",
        1: "更慢",
        2: "慢",
        3: "平衡",
        4: "快",
        5: "更快",
        6: "很快",
        7: "超快",
        8: "极快",
      },
    };
  },
  props: ["args", "type"],
  created() {
    return;
  },
  computed: {
    showVideo() {
      return (
        (this.args as any).video && (this.args as any).disableVideo == false
      );
    },
    showAudio() {
      return (
        (this.args as any).audio && (this.args as any).disableAudio == false
      );
    },
  },
  methods: {},
  components: {},
});
</script>
<style scoped>
.el-descriptions{
  margin-top:18px
}
</style>