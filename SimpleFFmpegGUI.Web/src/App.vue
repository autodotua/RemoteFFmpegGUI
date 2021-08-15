<template>
  <div id="app">
    <el-container>
      <el-header
        class="header one-line"
        :class="status != null && status.isProcessing ? 'header-tall' : ''"
      >
        <div>
          <h2 style="display: inline-block; margin-top: 12px">
            远程FFmpeg工具箱
          </h2>
          <a
            v-show="netError"
            style="
              color: red;
              display: inline-block;
              float: right;
              margin-top: 20px;
            "
            >获取状态失败</a
          >
        </div>
        <div class="status-bar" v-if="status != null && status.isProcessing">
          <div v-if="status.hasDetail">
            <el-row>
              <el-col :span="7">
                <el-row><b>码率：</b>{{ status.bitrate }}</el-row>
                <el-row
                  >已用：{{
                    formatDoubleTimeSpan(status.progress.duration)
                  }}</el-row
                >
              </el-col>
              <el-col :span="7">
                <el-row
                  >速度：{{ status.fps }}FPS{{ "   " }}
                  {{ status.speed }}X</el-row
                >
                <el-row
                  >剩余：{{
                    formatDoubleTimeSpan(status.progress.lastTime)
                  }}</el-row
                >
              </el-col>
              <el-col :span="7">
                <el-row
                  >进度：{{ status.f }}帧
                  {{ formatDoubleTimeSpan(status.time, true) }}
                </el-row>
                <el-row
                  >预计完成： {{ formatDateTime(finishTime(), false) }}</el-row
                >
              </el-col>

              <el-col :span="3">
                <el-popconfirm title="真的要取消任务吗？" @onConfirm="cancel">
                  <el-button
                    type="danger"
                    icon="el-icon-close"
                    circle
                    slot="reference"
                  ></el-button
                ></el-popconfirm>
              </el-col>
            </el-row>
            <el-row style="padding-bottom: 4px">
              <el-progress
                style="margin-right: 24px"
                :percentage="Math.round(status.progress.percent * 10000) / 100"
              ></el-progress>
            </el-row>
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
              style="float: right; margin-right: 24px; margin-top: 8px"
              @onConfirm="cancel"
            >
              <el-button
                type="danger"
                icon="el-icon-close"
                circle
                slot="reference"
              ></el-button
            ></el-popconfirm>
          </div>
        </div>
      </el-header>
      <el-container class="center">
        <el-aside :width="(menuCollapse ? 68 : 200) + 'px'">
          <el-button @click="changeMenuSize"
            :icon="menuCollapse?'el-icon-s-unfold':'el-icon-s-fold'"
            style="color: #909399; font-size: 24px; font-weight: 400;border:0"
          ></el-button>
          <el-menu router default-active="1" :collapse="menuCollapse">
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
                <i class="el-icon-document-add"></i>
                <span>新建任务</span>
              </template>
              <el-menu-item index="code">
                <i class="el-icon-circle-plus-outline"></i>
                <span>转码</span></el-menu-item
              >
            </el-submenu>
            <el-menu-item index="tasks">
              <i class="el-icon-document"></i>
              <template #title>任务列表</template>
            </el-menu-item>
            <el-menu-item index="preset">
              <i class="el-icon-document-copy"></i>
              <template #title>预设</template>
            </el-menu-item>
            <el-menu-item index="ftp">
              <i class="el-icon-folder-opened"></i>
              <template #title>FTP服务</template>
            </el-menu-item>
            <el-menu-item index="log">
              <i class="el-icon-takeaway-box"></i>
              <template #title>日志</template>
            </el-menu-item>
            <el-menu-item index="about">
              <i class="el-icon-info"></i>
              <template #title>关于</template>
            </el-menu-item>
          </el-menu></el-aside
        >

        <el-main :style="{height:'calc(100% - '+(status != null && status.isProcessing?130:60)+'px)'}"> 
          
          <router-view :status="status"></router-view> </el-main>
      </el-container>
    </el-container>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import {
  jump,
  showError,
  formatDateTime,
  formatDoubleTimeSpan,
} from "./common";
import * as net from "./net";

export default Vue.extend({
  name: "App",
  data: function () {
    return {
      showHeader: true,
      status: null,
      netError: false,
      menuCollapse: true,
    };
  },
  computed: {
    username() {
      return Cookies.get("username");
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
  created() {
    this.resizeMenu();
    window.addEventListener("resize", this.resizeMenu);
  },
  methods: {
    jump: jump,
    formatDoubleTimeSpan: formatDoubleTimeSpan,
    formatDateTime: formatDateTime,
    changeMenuSize(){
      this.menuCollapse=!this.menuCollapse;
    },
    resizeMenu() {
      this.menuCollapse = window.innerWidth < 500;
    },
    finishTime() {
      return new Date((this.status as any).progress.finishTime);
    },
    cancel() {
      net
        .postCancelQueue()
        .then((r) => {
          return;
        })
        .catch(showError);
    },
    getStatus() {
      net
        .getQueueStatus()
        .then((response) => {
          this.netError = false;
          this.status = response.data;
        })
        .catch((e) => (this.netError = true));
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
  height: 130px !important;
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

</style>
