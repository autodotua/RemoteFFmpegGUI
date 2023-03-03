
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
              class="unknown-progress"
              :stroke-width="20"
              color="transparent"
              :percentage="100"
              v-show="status.progress.isIndeterminate"
              style="margin-right: 24px; margin-top: 4px"
              :show-text="true"
              text-color="black"
              :format="(p) => '进度未知'"
              define-back-color="#CCCA"
            ></el-progress>
            <el-progress
              :text-inside="true"
              v-show="status.progress.isIndeterminate == false"
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
              class="unknown-progress"
              :stroke-width="20"
              color="transparent"
              :percentage="100"
              style="margin-right: 24px; margin-top: 4px"
              :show-text="true"
              v-show="status.progress.isIndeterminate"
              text-color="black"
              :format="(p) => '进度未知'"
              define-back-color="#CCCA"
            ></el-progress>
            <el-progress
              :text-inside="true"
              :stroke-width="20"
              :color="progressColor"
              style="margin-top: 10px"
              v-show="status.progress.isIndeterminate == false"
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
    return {
      walkingProgress: 0,
    };
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
      let delay = 0;
      // setInterval(() => {
      //   if (this.status.isPaused) {
      //     //this.walkingProgress=0;
      //     //return;
      //   }
      //   console.log(this.walkingProgress);
      //   if (delay > 0) {
      //     delay--;
      //     if (delay == 0) {
      //       this.walkingProgress = 0;
      //     }
      //     return;
      //   }
      //   if (this.walkingProgress >= 100) {
      //     delay = 10;
      //   } else {
      //     this.walkingProgress++;
      //   }
      // }, 50);
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
.unknown-progress > div > div > div {
  text-align: center;
}

/* .el-progress__text {
  font-size: 14px !important;
}

.el-progress-bar {
  margin-right: -60px !important;
  padding-right: 72px !important;
} */
</style>