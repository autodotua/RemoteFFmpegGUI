
<template>
  <el-form label-width="96px">
    <h3>容器</h3>
    <el-form-item label="指定输出容器">
      <el-switch v-model="code.enableFormat" class="right24"> </el-switch>
      <el-select
        v-if="code.enableFormat"
        v-model="code.format"
        placeholder="指定容器格式"
      >
        <el-option
          v-for="item in formats"
          :key="item.name"
          :label="item.extension"
          :value="item.name"
        >
          <span style="float: left">{{ item.extension }}</span>
          <span style="float: right; color: #8492a6; font-size: 13px">{{
            item.name
          }}</span>
        </el-option>
      </el-select>
      <div    v-if="code.enableFormat" class="gray">指定输出容器后，输出时会根据格式修改文件扩展名</div>
    </el-form-item>
    <h3>视频编码</h3>
    <el-form-item label="重编码">
      <el-switch v-model="code.enableVideo" class="right24"> </el-switch>
      <a v-show="!code.enableVideo" class="right12 gray">不导出视频</a>
      <el-switch
        v-show="!code.enableVideo"
        v-model="code.disableVideo"
      ></el-switch
    ></el-form-item>
    <div v-show="code.enableVideo">
      <el-form-item label="编码" size="small">
        <el-select v-model="code.video.code">
          <el-option
            v-for="c in videoCodes"
            :key="c"
            :label="c"
            :value="c"
          ></el-option> </el-select
      ></el-form-item>
      <el-form-item label="预设" class="bottom24">
        <el-slider
          style="width: 90%"
          :max="8"
          :show-tooltip="false"
          v-model="code.video.preset"
          :marks="presets"
        >
        </el-slider
      ></el-form-item>
      <el-form-item label="CRF" class="top24">
        <el-switch v-model="code.video.enableCrf"> </el-switch>
        <el-slider
          v-show="code.video.enableCrf"
          style="width: 90%"
          :max="40"
          :min="10"
          show-input
          :step="1"
          v-model="code.video.crf"
        >
        </el-slider
      ></el-form-item>
      <el-form-item label="平均码率" class="bottom24">
        <el-switch v-model="code.video.enableBitrate"> </el-switch>
        <el-slider
          v-show="code.video.enableBitrate"
          style="width: 90%"
          :max="500"
          :min="0.1"
          show-input
          :step="0.1"
          v-model="code.video.bitrate"
        >
        </el-slider
      ></el-form-item>
      <el-form-item label="最大码率" class="bottom24">
        <el-switch v-model="code.video.enableMaxBitrate"> </el-switch>
        <el-slider
          v-show="code.video.enableMaxBitrate"
          style="width: 90%"
          :max="500"
          :min="0.1"
          show-input
          :step="0.1"
          v-model="code.video.maxBitrate"
        >
        </el-slider
      ></el-form-item>
      <el-form-item
        label="缓冲倍率"
        class="bottom24"
        v-show="code.video.enableMaxBitrate"
      >
        <el-slider
          style="width: 90%"
          :max="10"
          :min="1"
          show-input
          show-stops
          :step="0.5"
          v-model="code.video.maxBitrateBuffer"
        >
        </el-slider
      ></el-form-item>
      <el-form-item label="帧率"
        ><el-switch v-model="code.video.enableFps" class="right24"> </el-switch>
        <div v-show="code.video.enableFps" style="display: inline">
          <el-input-number
            size="small"
            v-model="code.video.fps"
            :precision="3"
            :min="1"
            class="right24"
            :max="120"
          >
          </el-input-number>
          <el-button type="text" class="right24" @click="code.video.fps = 10"
            >10帧</el-button
          >
          <el-button type="text" class="right24" @click="code.video.fps = 24"
            >24帧</el-button
          >
          <el-button type="text" class="right24" @click="code.video.fps = 25"
            >25帧</el-button
          >
          <el-button type="text" class="right24" @click="code.video.fps = 30"
            >30帧</el-button
          >
          <el-button type="text" @click="code.video.fps = 60">60帧</el-button>
        </div>
      </el-form-item>
      <el-form-item label="分辨率">
        <el-switch v-model="code.video.enableScale" class="right24">
        </el-switch>
        <el-input-number
          size="small"
          class="right24 width80"
          :min="1"
          :max="20000"
          placeholder="宽度"
          v-model="code.video.width"
          :controls="false"
          v-show="code.video.enableScale"
        ></el-input-number>
        <a v-show="code.video.enableScale" class="right24">×</a>
        <el-input-number
          size="small"
          class="width80"
          :min="1"
          :max="20000"
          placeholder="高度"
          v-model="code.video.height"
          :controls="false"
          v-show="code.video.enableScale"
        ></el-input-number>
      </el-form-item>
    </div>
    <h3>音频编码</h3>
    <el-form-item label="重编码">
      <el-switch v-model="code.enableAudio" class="right24"> </el-switch>
      <a v-show="!code.enableAudio" class="right12 gray">不导出音频</a>
      <el-switch
        v-show="!code.enableAudio"
        v-model="code.disableAudio"
      ></el-switch>
    </el-form-item>
    <div v-show="code.enableAudio">
      <el-form-item label="编码">
        <el-select v-model="code.audio.code" size="small">
          <el-option
            v-for="c in audioCodes"
            :key="c"
            :label="c"
            :value="c"
          ></el-option> </el-select
      ></el-form-item>
      <el-form-item label="码率" class="bottom24">
        <el-switch v-model="code.audio.enableBitrate"> </el-switch>
        <el-slider
          v-show="code.audio.enableBitrate"
          style="width: 90%"
          :max="320"
          :min="32"
          :show-tooltip="false"
          :step="32"
          v-model="code.audio.bitrate"
          :marks="audioBitrates"
        >
        </el-slider
      ></el-form-item>
      <el-form-item label="采样率" style="margin-top: 24px">
        <el-switch v-model="code.audio.enableSample" class="right24">
        </el-switch>
        <el-select v-model="code.audio.sample" v-show="code.audio.enableSample">
          <el-option
            v-for="c in audioSamples"
            :key="c"
            :label="c"
            :value="c"
          ></el-option
        ></el-select>
      </el-form-item>
    </div>
    <h3>额外参数</h3>
    <el-form-item label="ffmpeg参数">
      <el-input
        v-model="code.extra"
        placeholder="请输入ffmpeg的运行参数"
      ></el-input>
    </el-form-item>
  </el-form>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";
import { withToken, showError, jump, formatDateTime } from "../common";
export default Vue.component("code-arguments", {
  data() {
    return {
      presets: {
        0: "",
        1: "更慢",
        2: "慢",
        3: {
          style: {
            color: "#1989FA",
          },
          label: this.$createElement("strong", "平衡"),
        },
        4: "快",
        5: "更快",
        6: "很快",
        7: "超快",
        8: "",
      },
      audioBitrates: {
        32: "32",
        64: "64",
        96: "96",
        128: "128",
        192: "192",
        256: "256",
        320: "320",
      },
      videoCodes: ["H264", "H265"],
      audioCodes: ["AAC"],
      audioSamples: [8000, 16000, 32000, 44100, 48000, 96000],
      formats: [],
      code: {
        enableVideo: true,
        enableAudio: true,
        enableFormat: true,
        format: "mp4",
        video: {
          code: "H265",
          preset: 3,
          crf: 23,
          enableCrf: true,
          width: 1920,
          height: 1080,
          enableScale: false,
          bitrate: 6,
          enableBitrate: false,
          maxBitrate: 24,
          maxBitrateBuffer: 2,
          enableMaxBitrate: false,
          fps: 30,
          enableFps: false,
        },
        audio: {
          code: "AAC",
          enableBitrate: true,
          bitrate: 128,
          enableSample: false,
          sample: 48000,
        },
        disableVideo: false,
        disableAudio: false,
        extra: "",
      },
    };
  },
  props: {},
  computed: {},
  created() {
    net
      .getFormats()
      .then((r) => (this.formats = r.data))
      .catch(showError);
  },
  methods: {
    getArgs() {
      const video = this.code.video;
      let videoArg = this.code.enableVideo
        ? {
            code: video.code,
            preset: video.preset,
            crf: video.enableCrf ? video.crf : null,
            width: video.enableScale ? video.width : null,
            height: video.enableScale ? video.height : null,
            fps: video.enableFps ? video.fps : null,
            averageBitrate: video.enableBitrate ? video.bitrate : null,
            maxBitrate: video.enableMaxBitrate ? video.maxBitrate : null,
            maxBitrateBuffer: video.enableMaxBitrate
              ? video.maxBitrateBuffer
              : null,
          }
        : null;
      const audio = this.code.audio;
      let audioArg = this.code.enableAudio
        ? {
            code: audio.code,
            bitrate: audio.enableBitrate ? audio.bitrate : null,
            samplingRate: audio.enableSample ? audio.sample : null,
          }
        : null;
      let arg = {
        video: videoArg,
        audio: audioArg,
        input: null,
        extra: this.code.extra,
        disableVideo: videoArg == null && this.code.disableVideo,
        disableAudio: audioArg == null && this.code.disableAudio,
        format: this.code.enableFormat ? this.code.format : null,
      };
      if (arg.disableVideo && arg.disableAudio) {
        showError("不可同时禁用视频和音频");
        return null;
      }
      return arg;
    },
    updateFromArgs(args: any) {
      console.log(args);

      const video = args.video;
      const audio = args.audio;

      if (video != null && !args.disableVideo) {
        this.code.enableVideo = true;
        const uiV = this.code.video;
        uiV.code = video.code;
        uiV.preset = video.preset;

        if (video.crf != null) {
          uiV.enableCrf = true;
          uiV.crf = video.crf;
        } else {
          uiV.enableCrf = false;
        }

        if (video.width != null && video.height != null) {
          uiV.enableScale = true;
          uiV.width = video.width;
          uiV.height = video.height;
        } else {
          uiV.enableScale = false;
        }

        if (video.fps != null) {
          uiV.enableFps = true;
          uiV.fps = video.fps;
        } else {
          uiV.enableFps = false;
        }

        if (video.averageBitrate != null) {
          uiV.enableBitrate = true;
          uiV.bitrate = video.averageBitrate;
        } else {
          uiV.enableBitrate = false;
        }

        if (video.maxBitrate != null) {
          uiV.enableMaxBitrate = true;
          uiV.maxBitrate = video.maxBitrate;
          uiV.maxBitrateBuffer = video.maxBitrateBuffer;
        } else {
          uiV.enableMaxBitrate = false;
        }
      } else {
        this.code.enableVideo = false;
      }

      if (audio != null && !args.disableAudio) {
        const uiA = this.code.audio;
        uiA.code = audio.code;
        if (audio.bitrate != null) {
          uiA.enableBitrate = true;
          uiA.bitrate = audio.bitrate;
        } else {
          uiA.enableBitrate = false;
        }
        if (audio.samplingRate != null) {
          uiA.enableSample = true;
          uiA.sample = audio.samplingRate;
        } else {
          uiA.enableSample = false;
        }
      } else {
        this.code.enableAudio = false;
      }
      this.code.disableVideo = args.disableVideo;
      this.code.disableAudio = args.disableAudio;
      this.code.enableFormat = args.format != null;
      this.code.format = args.format;
      this.code.extra = args.Extra;
    },
  },
  components: {},
  mounted: function () {
    this.$nextTick(function () {
      return;
    });
  },
});
</script>
<style scoped>
div[role="slider"] {
  min-width: 240px;
  max-width:480px;
}

.el-select {
  min-width: 160px;
}
</style>