<template>
  <div id="app">
    <el-container>
      <el-header
        class="header one-line"
        :class="status != null && status.isProcessing ? 'header-tall' : ''"
      >
        <h2>FFmpeg工具</h2>
        <div class="status-bar" v-if="status != null && status.isProcessing">
          <el-row>
            <el-col :span="5">
              <el-row><b>码率：</b>{{ status.bitrate }}</el-row>
              <el-row
                >已用：{{
                  formatCSharpTimeSpan(status.progress.duration)
                }}</el-row
              >
            </el-col>
            <el-col :span="5">
              <el-row
                >速度：{{ status.fps }}FPS{{ "   " }}
                {{ status.speed }}X</el-row
              >
              <el-row
                >剩余：{{
                  formatCSharpTimeSpan(status.progress.lastTime)
                }}</el-row
              >
            </el-col>
            <el-col :span="5">
              <el-row
                >进度：{{ status.f }}帧
                {{ formatCSharpTimeSpan(status.time, true) }}
              </el-row>
              <el-row
                >预计完成： {{ formatDateTime(finishTime, false) }}</el-row
              >
            </el-col>

            <el-col :span="6"
              ><el-progress
                style="width: 80%; margin-top: 4px"
                :text-inside="true"
                :stroke-width="30"
                :percentage="Math.round(status.progress.percent * 10000) / 100"
              ></el-progress
            ></el-col>
            <el-col :span="3">
              <el-button
                type="danger"
                icon="el-icon-close"
                circle
                @click="cancel"
              ></el-button>
            </el-col>
          </el-row></div
      ></el-header>
      <el-container class="center">
        <el-aside width="200px">
          <el-menu router default-active="1">
            <!-- <el-submenu index="1"    router="true">
              <template #title>
                <i class="el-icon-location"></i>
                <span>工具</span>
              </template>
              <el-menu-item-group>
                <template #title>工具</template>
                <el-menu-item index="info">媒体信息查询</el-menu-item>
              </el-menu-item-group>
       
            </el-submenu> -->
            <el-menu-item index="/">
              <i class="el-icon-s-home"></i>
              <template #title>欢迎</template>
            </el-menu-item>
            <el-menu-item index="info">
              <i class="el-icon-search"></i>
              <template #title>媒体信息查询</template>
            </el-menu-item>

            <el-submenu index="new">
              <template #title>
                <i class="el-icon-location"></i>
                <span>新建任务</span>
              </template>
              <el-menu-item index="code">转码</el-menu-item>
            </el-submenu>
            <el-menu-item index="tasks">
              <i class="el-icon-document"></i>
              <template #title>任务列表</template>
            </el-menu-item>
            <el-menu-item index="about">
              <i class="el-icon-info"></i>
              <template #title>关于</template>
            </el-menu-item>
          </el-menu></el-aside
        >

        <el-main> <router-view :status="status"></router-view> </el-main>
      </el-container>
    </el-container>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import {
  jump,
  getUrl,
  showError,
  formatDateTime,
  formatCSharpTimeSpan,
} from "./common";
export default Vue.extend({
  name: "App",
  data: function () {
    return {
      showHeader: true,
      status: null,
    };
  },
  computed: {
    username() {
      return Cookies.get("username");
    },
    finishTime() {
      return new Date((this.status as any).progress.finishTime);
    },
  },
  mounted: function () {
    this.$nextTick(function () {
      setInterval(this.getStatus, 2000);
      const url = window.location.href;
      // if (url.indexOf("login") >= 0) {
      //   this.showHeader = false;
      // } else {
      //   if (Cookies.get("userID") == undefined) {
      //    jump("login");
      //   }
      // }
    });
  },
  methods: {
    jump: jump,
    formatCSharpTimeSpan: formatCSharpTimeSpan,
    formatDateTime: formatDateTime,
    cancel() {
      this.$confirm("是否终止队列？", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      }).then(() => {
        Vue.axios
          .post(getUrl("Task/Cancel"))
          .then((r) => {
            return;
          })
          .catch(showError);
      });
    },
    getStatus() {
      Vue.axios
        .get(getUrl("Task/Status"))
        .then((response) => {
          this.status = response.data;
        })
        .catch(showError);
    },
    clickUsername() {
      this.$confirm("是否注销账户？", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      }).then(() => {
        Cookies.remove("username");
        Cookies.remove("userID");
        Cookies.remove("token");
        location.reload();
      });
    },
  },
});
</script>
<style >
.header-title {
  float: left;
  margin-top: 0px;
}
.el-message-box {
  width: auto !important;
}
body {
  overflow-x: hidden;
}
</style>
<style scoped>
/* #app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}
header a {
  text-decoration: none;
} */

.header {
  margin-left: -12px;
  margin-right: -12px;
  margin-top: -12px;
  background: #ebeef5;
  color: #606266;
}
.header-tall {
  height: 126px !important;
}
.status-bar {
  background-color: lightgreen;
  padding-left: 24px;
  padding-top: 12px;
  padding-bottom: 12px;
  margin-left: -30px;
  margin-right: -30px;
  margin-top: -12px;
}
.one-line {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
/* .center {
  height: calc(100% - 120px)!important;
} */
</style>
