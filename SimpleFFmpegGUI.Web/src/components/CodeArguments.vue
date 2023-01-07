
<template>
  <el-form label-width="100px">
    <h3 v-if="showPresets">预设</h3>

    <div v-if="showPresets">
      <el-form-item label="选择和更新">
        <el-select
          @change="selectPreset"
          placeholder="加载预设"
          v-model="preset"
          class="right24"
        >
          <el-option
            v-for="p in presets"
            :key="p.id"
            :label="p.name"
            :value="p.id"
          ></el-option
        ></el-select>
        <el-button :disabled="preset == null" @click="updatePreset"
          >更新</el-button
        >
      </el-form-item>
      <el-form-item label="新增">
        <el-input
          v-model="newPresetName"
          style="width: 128px"
          class="right24"
        ></el-input>
        <el-button
          style="display: inline"
          :disabled="newPresetName == null || newPresetName.trim() == ''"
          @click="savePreset"
          >保存或更新“{{ newPresetName }}”</el-button
        >
      </el-form-item>
    </div>
    <div v-if="type == 1">
      <h3>合并参数</h3>
      <el-form-item label="音频比视频长">
        <el-switch
          v-model="code.combine.shortest"
          active-text="裁剪到最短的媒体"
          inactive-text="最后部分静帧或黑屏"
        ></el-switch>
      </el-form-item>
    </div>
    <div v-if="type == 4">
      <h3>拼接参数</h3>
      <el-form-item label="拼接方法">
        <el-select
          v-model="code.concat.type"
          value-key="id"
          style="width: 320px"
        >
          <el-option
            v-for="value in concatTypes"
            :key="value.id"
            :label="value.name"
            :value="value.id"
          ></el-option>
        </el-select>
      </el-form-item>
    </div>
    <div v-if="showFormats">
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
        <div v-if="code.enableFormat" class="gray">
          指定输出容器后，输出时会根据格式修改文件扩展名
        </div>
      </el-form-item>
    </div>
    <div v-if="showVideosAndAudios">
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
            :marks="speedPresets"
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
        <el-form-item label="二次编码" class="top24">
          <el-switch v-model="code.video.twoPass"> </el-switch>
        </el-form-item>
        <el-form-item label="平均码率" class="bottom24">
          <el-switch v-model="code.video.enableBitrate"> </el-switch>
          <el-slider
            v-show="code.video.enableBitrate"
            style="width: 90%"
            :max="200"
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
          ><el-switch v-model="code.video.enableFps" class="right24">
          </el-switch>
          <div v-show="code.video.enableFps" class="inline">
            <el-input-number
              size="small"
              v-model="code.video.fps"
              :precision="3"
              :min="1"
              class="right24"
              :max="120"
            >
            </el-input-number>
            <el-button
              type="text"
              class="right24"
              @click="code.video.fps = f"
              v-for="f in fpses"
              :key="f"
              >{{ f }}帧</el-button
            >
          </div>
        </el-form-item>
        <el-form-item label="分辨率">
          <el-switch v-model="code.video.enableSize" class="right24">
          </el-switch>
          <div class="inline" v-show="code.video.enableSize">
            <el-input
              size="small"
              class="right24 width160"
              placeholder="示例：640:480 或 640:-1 或 iw/2:ih/2"
              v-model="code.video.size"
            ></el-input>

            <el-button
              v-for="(v, k) in sizes"
              :key="k"
              type="text"
              class="right24"
              @click="code.video.size = v"
              >{{ k }}</el-button
            >
          </div>
        </el-form-item>

        <el-form-item label="画面比例">
          <el-switch v-model="code.video.enableAspectRatio" class="right24">
          </el-switch>
          <div class="inline" v-show="code.video.enableAspectRatio">
            <el-input
              size="small"
              class="right24 width160"
              placeholder="示例：4:3或1.3333"
              v-model="code.video.aspectRatio"
            ></el-input>

            <el-button
              v-for="i in aspectRatios"
              :key="i"
              type="text"
              class="right24"
              @click="code.video.aspectRatio = i"
              >{{ i }}</el-button
            >
          </div></el-form-item
        >

        <el-form-item label="像素格式">
          <el-switch v-model="code.video.enablePixelFormat" class="right24">
          </el-switch>
          <div class="inline" v-show="code.video.enablePixelFormat">
            <el-input
              size="small"
              class="right24 width160"
              v-model="code.video.pixelFormat"
            ></el-input>

            <el-button
              v-for="p in pixelFormats"
              :key="p"
              type="text"
              class="right24"
              @click="code.video.pixelFormat = p"
              >{{ p }}</el-button
            >
          </div></el-form-item
        >
      </div>
    </div>
    <div v-if="showVideosAndAudios">
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
          <el-select
            v-model="code.audio.sample"
            v-show="code.audio.enableSample"
          >
            <el-option
              v-for="c in audioSamples"
              :key="c"
              :label="c"
              :value="c"
            ></el-option
          ></el-select>
        </el-form-item>
      </div>
    </div>

    <div>
      <h3>{{ type == 3 ? "参数" : "额外参数" }}</h3>
      <el-form-item label="ffmpeg参数">
        <el-input
          v-model="code.extra"
          type="textarea"
          autosize
          spellcheck="false"
          autocorrect="off"
          placeholder="请输入ffmpeg的运行参数"
        ></el-input>
      </el-form-item>
    </div>
  </el-form>
</template>




<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";
import { showError, showSuccess } from "../common";
export default Vue.component("code-arguments", {
  data() {
    return {
      speedPresets: {
        0: "最慢",
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
        8: "极快",
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
      concatTypes: [
        { id: 0, name: "通过ts中转，支持不同格式" },
        {
          id: 1,
          name: "使用concat格式，不需要重新解码和编码，需要相同格式文件",
        },
      ],
      videoCodes: ["自动", "H264", "H265", "VP9"],
      audioCodes: ["自动", "AAC", "OPUS"],
      audioSamples: [8000, 16000, 32000, 44100, 48000, 96000],
      formats: [],
      aspectRatios: ["4:3", "16:9", "2.35"],
      fpses: [10, 24, 25, 29.97, 30, 59.94, 60],
      sizes: {
        "480P": "-1:480",
        "720P": "-1:720",
        "1080P": "-1:1080",
        "1440P": "-1:1440",
        "2160P": "-1:2160",
      },
      pixelFormats: [
        "yuv420p",
        "yuvj420p",
        "yuv422p",
        "yuvj422p",
        "rgb24",
        "gray",
        "yuv420p10le",
      ],
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
          twoPass: false,
          size: "1920:1080",
          enableSize: false,
          bitrate: 6,
          enableBitrate: false,
          maxBitrate: 24,
          maxBitrateBuffer: 2,
          enableMaxBitrate: false,
          fps: 30,
          enableFps: false,
          enableAspectRatio: false,
          aspectRatio: "16:9",
          enablePixelFormat: false,
          pixelFormat: "",
        },
        audio: {
          code: "AAC",
          enableBitrate: true,
          bitrate: 128,
          enableSample: false,
          sample: 48000,
        },
        combine: {
          shortest: false,
        },
        concat: {
          type: 0,
        },
        disableVideo: false,
        disableAudio: false,
        extra: "",
      },
      presets: [],
      preset: null,
      newPresetName: "新预设",
    };
  },
  props: {
    type: {
      default: 0,
    },
    showPresets: {
      default: true,
    },
  },
  computed: {
    showFormats(): boolean {
      return (
        [0, 1, 2, 4].includes(this.type) &&
        (this.type != 4 || this.code.concat.type != 1)
      );
    },
    showVideosAndAudios(): boolean {
      return (
        [0, 4].includes(this.type) &&
        (this.type != 4 || this.code.concat.type != 1)
      );
    },
  },
  created() {
    this.fillPresets();
    net
      .getFormats()
      .then((r) => (this.formats = r.data))
      .catch(showError);
  },
  methods: {
    updatePreset() {
      let args = this.getArgs();
      if (args == null) {
        return;
      }
      const name = (
        this.presets.filter((p) => (p as any).id == this.preset)[0] as any
      ).name;
      net
        .postAddOrUpdatePreset(name, this.type, args)
        .then((r) => {
          showSuccess("更新预设成功");
          this.fillPresetsAnd((p) => {
            this.preset = r.data;
          });
        })
        .catch(showError);
    },
    selectPreset(preset: number) {
      this.updateFromArgs(
        (this.presets.filter((p) => (p as any).id == preset)[0] as any)
          .arguments
      );
    },
    fillPresets() {
      this.fillPresetsAnd((id) => {
        return;
      });
    },
    fillPresetsAnd(action: (id: number) => void) {
      net
        .getPresets(this.type)
        .then((r) => {
          this.presets = r.data;

          action(r.data);
        })
        .catch(showError);
    },
    savePreset() {
      let args = this.getArgs();
      if (args == null) {
        return;
      }
      net
        .postAddOrUpdatePreset(this.newPresetName, this.type, args)
        .then((r) => {
          showSuccess("新建或更新预设成功");
          this.fillPresetsAnd((p) => {
            this.preset = r.data as any;
          });
        })
        .catch(showError);
    },
    getArgs() {
      const video = this.code.video;

      let videoArg = this.code.enableVideo
        ? {
            code: video.code,
            preset: video.preset,
            crf: video.enableCrf ? video.crf : null,
            twoPass: video.twoPass,
            size: video.enableSize ? video.size : null,
            fps: video.enableFps ? video.fps : null,
            averageBitrate: video.enableBitrate ? video.bitrate : null,
            maxBitrate: video.enableMaxBitrate ? video.maxBitrate : null,
            maxBitrateBuffer: video.enableMaxBitrate
              ? video.maxBitrateBuffer
              : null,
            aspectRatio: video.enableAspectRatio ? video.aspectRatio : null,
            pixelFormat: video.enablePixelFormat ? video.pixelFormat : null,
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
      let combine = { shortest: this.code.combine.shortest };
      let concat = { type: this.code.concat.type };
      let arg = {
        video: videoArg,
        audio: audioArg,
        input: null,
        combine: this.type == 1 ? combine : null,
        concat: this.type == 4 ? concat : null,
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
      const video = args.video;
      const audio = args.audio;
      const combine = args.combine;
      const concat = args.concat;

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
        uiV.twoPass = video.twoPass;

        if (video.size != null) {
          uiV.enableSize = true;
          uiV.size = video.size;
        } else {
          uiV.enableSize = false;
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

        if (video.aspectRatio) {
          uiV.enableAspectRatio = true;
          uiV.aspectRatio = video.aspectRatio;
        } else {
          uiV.enableAspectRatio = false;
        }

        if (video.pixelFormat) {
          uiV.enablePixelFormat = true;
          uiV.pixelFormat = video.pixelFormat;
        } else {
          uiV.enablePixelFormat = false;
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
      if (this.type == 1 && combine != null) {
        this.code.combine = {
          shortest: combine.shortest,
        };
      }
      if (this.type == 4 && concat != null) {
        this.code.concat = {
          type: concat.type,
        };
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
  max-width: 480px;
}

.el-select {
  min-width: 160px;
}
</style>