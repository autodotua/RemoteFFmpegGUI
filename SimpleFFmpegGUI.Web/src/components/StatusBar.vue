
<template>
  <div class="status-bar" v-if="status != null && status.isProcessing">
    <div v-if="status.hasDetail">
      <div v-if="windowWidth > 768">
        <el-row>
          <el-col :span="7">
            <el-row><b>码率：</b>{{ status.bitrate }}</el-row>
            <el-row
              ><b>已用：</b
              >{{ formatDoubleTimeSpan(status.progress.duration) }}</el-row
            >
          </el-col>
          <el-col :span="7">
            <el-row
              ><b>速度：</b>{{ status.fps }}FPS{{ "   " }}
              {{ status.speed }}X</el-row
            >
            <el-row
              ><b>剩余：</b
              >{{ formatDoubleTimeSpan(status.progress.lastTime) }}</el-row
            >
          </el-col>
          <el-col :span="7">
            <el-row
              ><b>进度：</b>{{ status.frame }}帧
              {{ formatDoubleTimeSpan(status.time, true) }}
            </el-row>
            <el-row
              ><b>预计：</b>
              {{ formatDateTime(finishTime(), true, true, false) }}</el-row
            >
          </el-col>

          <el-col :span="3">
            <el-popconfirm title="真的要取消任务吗？" @confirm="cancel">
              <el-button
                type="text"
                style="color: red"
                slot="reference"
                size="big"
                >取消</el-button
              ></el-popconfirm
            >
          </el-col>
        </el-row>
        <el-row class="right24">
          <el-col :span="8" class="one-line"
            ><b>任务：</b
            >{{ status.isPaused ? "暂停中" : status.progress.name }}</el-col
          >
          <el-col :span="16">
            <el-progress
              :text-inside="true"
              :stroke-width="20"
              :color="progressColor"
              style="margin-right: 24px; margin-top: 4px"
              :percentage="status.progress.percent * 100"
              :format="(p) => p.toFixed(2) + '%'"
              text-color="white"
              define-back-color="#CCCA"
            ></el-progress
          ></el-col>
        </el-row>
      </div>
      <div v-else>
        <el-row>
          <el-col :span="12">
            <el-row><b>码率：</b>{{ status.bitrate }}</el-row>
            <el-row
              ><b>速度：</b>{{ status.fps }}FPS{{ "   " }}
              {{ status.speed }}X</el-row
            >
            <el-row
              ><b>进度：</b>{{ status.frame }}帧
              {{ formatDoubleTimeSpan(status.time, true) }}
            </el-row>
          </el-col>
          <el-col :span="12">
            <el-row
              ><b>已用：</b
              >{{ formatDoubleTimeSpan(status.progress.duration) }}</el-row
            >
            <el-row
              ><b>剩余：</b
              >{{ formatDoubleTimeSpan(status.progress.lastTime) }}</el-row
            >
            <el-row
              ><b>预计：</b
              >{{ formatDateTime(finishTime(), true, true, false) }}</el-row
            >
          </el-col>
        </el-row>
        <el-row class="single-line">
          <b>任务：</b>{{ status.isPaused ? "暂停中" : status.progress.name }}
        </el-row>
        <el-row>
          <el-col :span="20">
            <el-progress
              :text-inside="true"
              :stroke-width="20"
              :color="progressColor"
              style="margin-top: 10px"
              :percentage="status.progress.percent * 100"
              :format="(p) => p.toFixed(2) + '%'"
              text-color="white"
              define-back-color="#CCCA"
            ></el-progress></el-col
          ><el-col :span="4">
            <el-popconfirm title="真的要取消任务吗？" @confirm="cancel">
              <el-button
                style="width: 75%; color: red"
                type="text"
                slot="reference"
                size="big"
                >取消</el-button
              ></el-popconfirm
            ></el-col
          >
        </el-row>
      </div>
    </div>
    <div v-else style="height: 60px">
      <i
        class="el-icon-loading"
        style="
          font-size: 24px;
          position: absolute;
          left: 50%;
          margin-left: -12px;
        "
      ></i>
      <a
        style="
          position: absolute;
          left: 50%;
          margin-left: -46px;
          margin-top: 32px;
        "
        >正在执行任务</a
      >
      <el-popconfirm
        title="真的要取消任务吗？"
        style="float: right; margin-right: 36px; margin-top: 8px"
        @confirm="cancel"
      >
        <el-button type="text" style="color: red" slot="reference" size="big"
          >取消</el-button
        ></el-popconfirm
      >
    </div>
  </div>
</template>
<script >
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";
import {
  showError,
  jump,
  formatDateTime,
  formatDoubleTimeSpan,
} from "../common";
export default Vue.component("status-bar", {
  data() {
    return {};
  },
  props: ["status", "windowWidth", "isPaused"],
  computed: {
    progressColor() {
      if (this.status && this.status.isPaused) {
        return "#777777";
      }
      return "#50a0fc";
    },
  },
  watch: {},
  methods: {
    formatDateTime: formatDateTime,
    formatDoubleTimeSpan: formatDoubleTimeSpan,
    finishTime() {
      return new Date(this.status.progress.finishTime);
    },
    cancel() {
      net
        .postCancelQueue()
        .then((r) => {
          return;
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
<style scoped>
.status-bar {
  background-color: lightgreen;
  padding-left: 24px;
  padding-top: 12px;
  padding-bottom: 8px;
  margin-left: -30px;
  margin-right: -30px;
  margin-top: -12px;
}
</style>

<style>
/* .el-progress__text {
  font-size: 14px !important;
}

.el-progress-bar {
  margin-right: -60px !important;
  padding-right: 72px !important;
} */
</style>