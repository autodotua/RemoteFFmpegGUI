<template>
  <div>
    <div>
      <file-select ref="files" @select="fileSelect"></file-select>

      <el-button
        type="primary"
        v-on:click="query"
        :disabled="file == ''"
        style=""
        >查询</el-button
      >
    </div>
    <div v-if="info != null">
      <el-form ref="form" :model="info" label-width="80px">
        <el-form-item label="持续时间">
          {{ info.duration.hours }}小时{{ info.duration.minutes }}分{{
            info.duration.seconds
          }}.{{ info.duration.milliseconds }}秒
        </el-form-item>
        <el-form-item label="格式">{{
          info.format.formatLongName
        }}</el-form-item>
        <el-form-item label="码率"
          >{{
            (info.format.bitRate / 1024 / 1024).toFixed(2)
          }}Mbps</el-form-item
        >

        <el-form-item
          v-for="s in info.videoStreams"
          :key="s.index"
          :label="'视频流 ' + (s.index + 1)"
        >
          <el-form-item label="码率"
            >{{
              s.bitRate == null ? "" : (s.bitRate / 1024 / 1024).toFixed(2)
            }}Mbps</el-form-item
          >
          <el-form-item label="平均帧率" v-if="s.avgFrameRate != null"
            >{{
              s.avgFrameRate == null ? "" : s.avgFrameRate.toFixed(3)
            }}FPS</el-form-item
          >
          <el-form-item label="编码"
            >{{ s.codecName }} （{{ s.codecLongName }}）</el-form-item
          >

          <el-form-item label="分辨率"
            >{{ s.width }} × {{ s.height }}</el-form-item
          >
          <el-form-item label="语言">{{ s.language }} </el-form-item>
          <el-form-item label="像素格式">{{ s.pixelFormat }} </el-form-item>
          <el-form-item label="编码配置">{{ s.profile }} </el-form-item>
          <el-form-item label="旋转角度">{{ s.rotation }} </el-form-item>
          <el-form-item label="标签">
            <el-form-item
              label-width="180px"
              v-for="(value, name) in s.tags"
              :key="name"
              :label="name"
              >{{ value }}
            </el-form-item>
          </el-form-item>
        </el-form-item>

        <el-form-item
          v-for="s in info.audioStreams"
          :key="s.index"
          :label="'音频流 ' + (s.index + 1)"
        >
          <el-form-item label="码率"
            >{{ (s.bitRate / 1024).toFixed(0) }}Kbps</el-form-item
          >
          <el-form-item label="编码"
            >{{ s.codecName }} （{{ s.codecLongName }}）</el-form-item
          >

          <el-form-item label="声道数">{{ s.channels }}</el-form-item>
          <el-form-item label="语言">{{ s.language }} </el-form-item>
          <el-form-item label="编码配置">{{ s.profile }} </el-form-item>
          <el-form-item label="采样率">{{ s.sampleRateHz }} </el-form-item>
          <el-form-item label="标签">
            <el-form-item
              label-width="180px"
              v-for="(value, name) in s.tags"
              :key="name"
              :label="name"
              >{{ value }}
            </el-form-item>
          </el-form-item>
        </el-form-item>
        <el-form-item label="标签">
          <el-form-item
            label-width="180px"
            v-for="(value, name) in info.format.tags"
            :key="name"
            :label="name"
            >{{ value }}
          </el-form-item>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import { withToken,  showError, jump, formatDateTime } from "../common";
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
    fileSelect(item: string) {
      this.file = item;
      console.log(item);
    },
    query() {
    net.getMediaInfo(this.file)
        .then((response) => {
          this.info = response.data;
          console.log(response.data);
        })
        .catch(showError);
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
