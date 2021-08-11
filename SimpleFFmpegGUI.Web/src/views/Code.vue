<template>
  <div>
    <el-form ref="form" label-width="120px">
      <el-form-item label="媒体文件">
        <file-select ref="files" @select="selectFile"></file-select>
      </el-form-item>
      <el-form-item label="">
        <a>视频编码： </a> <el-switch v-model="code.enableVideo"> </el-switch>
        <a style="margin-left: 24px"> 音频编码：</a>
        <el-switch v-model="code.enableAudio"> </el-switch>
      </el-form-item>
      <el-form-item label="视频参数" v-show="code.enableVideo">
        <el-form-item label="编码">
          <el-select v-model="code.video.code">
            <el-option
              v-for="c in videoCodes"
              :key="c"
              :label="c"
              :value="c"
            ></el-option> </el-select
        ></el-form-item>
        <el-form-item label="预设">
          <el-slider
            style="width: 90%"
            :max="9"
            :show-tooltip="false"
            v-model="code.video.preset"
            :marks="presets"
          >
          </el-slider
        ></el-form-item>
        <el-form-item label="CRF" style="margin-top: 24px">
          <el-switch v-model="code.enableCrf"> </el-switch>
          <el-slider
            v-show="code.enableCrf"
            style="width: 90%"
            :max="40"
            :min="10"
            show-input
            :step="1"
            show-stops
            v-model="code.video.crf"
          >
          </el-slider
        ></el-form-item>
        <el-form-item label="分辨率" style="margin-top: 24px">
          <el-switch v-model="code.enableScale"> </el-switch>
          <el-input-number
            size="small"
            style="margin-left: 24px"
            :min="1"
            :max="20000"
            placeholder="宽度"
            v-model="width"
            :controls="false"
            v-show="code.enableScale"
          ></el-input-number>
          <a
            v-show="code.enableScale"
            style="margin-left: 24px; margin-right: 0px"
            >×</a
          >
          <el-input-number
            size="small"
            style="margin-left: 24px"
            :min="1"
            :max="20000"
            placeholder="高度"
            v-model="width"
            :controls="false"
            v-show="code.enableScale"
          ></el-input-number>
        </el-form-item>
      </el-form-item>
          <el-form-item label="音频参数" v-show="code.enableAudio">
        <el-form-item label="编码">
          <el-select v-model="code.audio.code">
            <el-option
              v-for="c in audioCodes"
              :key="c"
              :label="c"
              :value="c"
            ></el-option> </el-select
        ></el-form-item>
        <el-form-item label="码率">
          <el-slider
            style="width: 90%"
            :max="320" :min="32"
            :show-tooltip="false" :step="32"
            v-model="code.audio.bitrate"
            :marks="audioBitrates"
          >
          </el-slider
        ></el-form-item></el-form-item>
      <el-form-item style="margin-top: 36px">
        <el-button type="primary" @click="add">加入队列</el-button>
        <el-button @click="addAndStart">加入队列并立即开始</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import { withToken, getUrl, showError, jump, formatDateTime, showSuccess } from "../common";
export default Vue.extend({
  name: "Home",
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
      audioBitrates:{32:"32",64:"64",96:"96",128:"128",192:"192",256:"256",320:"320"},
      file: "",
      videoCodes: ["H264", "H265"],
      audioCodes: ["AAC"],
      code: {
        enableVideo: true,
        enableAudio: true,
        video: {
          code: "H265",
          preset: 3,
          crf: 23,
          enableCrf: true,
          width: null,
          height: null,
          enableScale: false,
        },
        audio:{
          code:"AAC",
          bitrate:128
        }
      },
    };
  },
  computed: {},
  methods: {
    jump: jump,
    selectFile(file:string){
      this.file=file
    },
    add(){
      this.addCode(false)
    },
    addAndStart(){
      this.addCode(true)
    },
    addCode(start:boolean) {
      let videoArg=this.code.enableVideo?{
        code:this.code.video.code,
        preset:this.code.video.preset,
        crf: this.code.video.enableCrf?this.code.video.crf:null,
        width: this.code.video.enableScale?this.code.video.width:null,
        height: this.code.video.enableScale?this.code.video.height:null,
      }:null
      let audioArg=this.code.enableAudio?{
code:this.code.audio.code,
bitrate:this.code.audio.bitrate
      }:null
      let arg={video:videoArg,audio:audioArg,input:null}
   
      Vue.axios
        .post(getUrl("Task/Add/Code"), {
          input: [this.file],
          output: this.file,
          argument: arg,
          start:start
        })
        .then((response) => {
          showSuccess("已加入队列")
        })
        .catch(showError);
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
