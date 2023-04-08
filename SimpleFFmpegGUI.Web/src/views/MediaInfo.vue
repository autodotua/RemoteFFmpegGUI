<template>
  <div>
    <div>
      <file-select :file.sync="file" class="right24 bottom12"></file-select>

      <el-button type="primary" @click="query" :disabled="file == ''"
        >查询</el-button
      >
    </div>
    <div v-if="info != null">
      <el-form ref="form" :model="info" label-width="80px">
        <el-form-item label="长度">
          {{ formatDoubleTimeSpan(info.duration, true) }}
        </el-form-item>
        <el-form-item label="格式">{{ info.format }}</el-form-item>
        <el-form-item label="码率"
          >{{
            (info.overallBitRate / 1024 / 1024).toFixed(2)
          }}
          Mbps</el-form-item
        >

        <el-form-item
          v-for="s in info.videos"
          :key="s.index"
          :label="'视频 ' + s.index"
        >
          <el-form-item label="编码">{{ s.format }} </el-form-item>

          <el-form-item label="编码预设">{{ s.format_Profile }} </el-form-item>
          <el-form-item label="码率"
            >{{
              s.bitRate == null ? "" : (s.bitRate / 1024 / 1024).toFixed(2)
            }}
            Mbps</el-form-item
          >
          <el-form-item label="帧率"
            >{{ s.frameRate.toFixed(3) }} FPS</el-form-item
          >

          <el-form-item label="分辨率"
            >{{ s.width }} × {{ s.height }}</el-form-item
          >
          <el-form-item label="比例">{{ s.displayAspectRatio }} </el-form-item>
          <el-form-item label="像素格式"
            >{{ s.colorSpace }} {{ s.chromaSubsampling }}
          </el-form-item>
          <el-form-item label="色彩深度">{{ s.bitDepth }} </el-form-item>
          <el-form-item label="旋转">{{ s.rotation }} </el-form-item>
        </el-form-item>
        <el-form-item
          v-for="s in info.audios"
          :key="s.index"
          :label="'音频 ' + s.index"
          ><el-form-item label="编码">{{ s.format }} </el-form-item>
          <el-form-item label="码率"
            >{{ (s.bitRate / 1024).toFixed(0) }} Kbps</el-form-item
          >
          <el-form-item label="声道数">{{ s.channels }}</el-form-item>
          <el-form-item label="声道布局">{{ s.channelLayout }} </el-form-item>
          <el-form-item label="采样率">{{ s.samplingRate }} Hz</el-form-item>
          <el-form-item label="默认">{{ s.default }}</el-form-item>
        </el-form-item>
        <el-form-item
          v-for="s in info.texts"
          :key="s.index"
          :label="'字幕 ' + s.index"
          ><el-form-item label="编码">{{ s.format }} </el-form-item>
          <el-form-item label="语言">{{ s.language }}</el-form-item>
          <el-form-item label="标题">{{ s.title }}</el-form-item>
          <el-form-item label="默认">{{ s.default }}</el-form-item>
        </el-form-item>
        <el-form-item label="详细信息">
          <br />
          <a style="font-family: Consolas" class="s">{{ info.raw }}</a>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import {
  showError,
  jump,
  formatDateTime,
  formatDoubleTimeSpan,
  showLoading,
  closeLoading,
} from "../common";
import * as net from "../net";
import FileSelect from "@/components/FileSelect.vue";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      file: "",
      info: null,
    };
  },
  computed: {},
  methods: {
    jump: jump,
    formatDoubleTimeSpan,

    query() {
      showLoading();
      net
        .getMediaInfo(this.file)
        .then((response) => {
          this.info = response.data;
          console.log(response.data);
        })
        .catch(showError)
        .finally(closeLoading);
    },
  },
  components: { FileSelect },
  mounted: function () {
    this.$nextTick(function () {
      return;
    });
  },
});
</script>
