<template>
  <div id="app">
    <el-container>
      <el-header
        class="header one-line"
        style="height: auto; padding-left: 28px; padding-top: 8px"
      >
        <div>
          <h2
            style="display: inline-block; margin-top: 8px; margin-bottom: 8px"
          >
            远程FFmpeg工具箱
          </h2>
          <a
            v-show="netError"
            style="
              color: red;
              display: inline-block;
              float: right;
              margin-top: 18px;
              font-size: 12px;
            "
            >获取状态失败</a
          >
          <el-button
            style="display: inline-block; float: right; margin-top: 12px"
            type="text"
            v-show="logged"
            @click="logout"
            >已登录</el-button
          >
        </div>
      </el-header>
      <el-container
        class="center"
        :style="{
          height: 'calc(100% - ' + (headerHeight + footerHeight) + 'px)',
          marginBottom:
            status == null || status.isProcessing == false ? '0' : '12px',
          paddingBottom:
            status == null || status.isProcessing == false ? '0' : '8px',
        }"
      >
        <el-aside
          :width="(menuCollapse ? 68 : 200) + 'px'"
          v-if="this.$route.path != '/login'"
        >
          <el-button
            @click="changeMenuSize"
            :icon="menuCollapse ? 'el-icon-s-unfold' : 'el-icon-s-fold'"
            style="color: #909399; font-size: 24px; font-weight: 400; border: 0"
          ></el-button>
          <el-menu router default-active="1" :collapse="menuCollapse">
            <el-menu-item index="/">
              <i class="el-icon-s-home"></i>
              <template #title>欢迎</template>
            </el-menu-item>
            <el-menu-item index="/info">
              <i class="el-icon-search"></i>
              <template #title>媒体信息查询</template>
            </el-menu-item>

            <el-submenu index="new">
              <template #title>
                <i class="el-icon-document-add"></i>
                <span>新建任务</span>
              </template>
              <el-menu-item index="/add/code">
                <i class="el-icon-circle-plus-outline"></i>
                <span>转码</span></el-menu-item
              >
              <el-menu-item index="/add/combine">
                <i class="el-icon-circle-plus-outline"></i>
                <span>合并视音频</span></el-menu-item
              >
              <el-menu-item index="/add/compare">
                <i class="el-icon-circle-plus-outline"></i>
                <span>视频对比</span></el-menu-item
              >
              <el-menu-item index="/add/custom">
                <i class="el-icon-circle-plus-outline"></i>
                <span>自定义</span></el-menu-item
              >
            </el-submenu>
            <el-menu-item index="/tasks">
              <i class="el-icon-document"></i>
              <template #title>任务列表</template>
            </el-menu-item>
            <el-menu-item index="/preset">
              <i class="el-icon-document-copy"></i>
              <template #title>预设</template>
            </el-menu-item>
            <el-menu-item index="/file">
              <i class="el-icon-folder-opened"></i>
              <template #title>文件服务</template>
            </el-menu-item>
            <el-menu-item index="/log">
              <i class="el-icon-takeaway-box"></i>
              <template #title>日志</template>
            </el-menu-item>
            <el-menu-item index="/about">
              <i class="el-icon-info"></i>
              <template #title>关于</template>
            </el-menu-item>
          </el-menu></el-aside
        >

        <el-main>
          <router-view
            :status="status"
            @statusChanged="delayGetStatus"
          ></router-view>
        </el-main>
      </el-container>
      <el-footer class="footer" style="height: auto; z-index: 1000">
        <status-bar :status="status" :window-width="windowWidth"></status-bar>
      </el-footer>
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
import StatusBar from "./components/StatusBar.vue";

export default Vue.extend({
  name: "App",
  data: function () {
    return {
      showHeader: true,
      status: null,
      netError: false,
      menuCollapse: false,
      windowWidth: 0,
      headerHeight: 0,
      footerHeight: 0,
      logged: false,
    };
  },
  computed: {},
  mounted: function () {
    this.$nextTick(function () {
      this.resizeMenu();
      setInterval(this.getStatus, 2000);
      const url = window.location.href;
    });
  },
  created() {
    console.log("app created");

    net.getNeedToken().then((r) => {
      if (r.data == true) {
        if (Cookies.get("token")) {
          net.getCheckToken(Cookies.get("token") as string).then((r) => {
            if (!r.data) {
              //token错误
              jump("login");
            } else {
              net.setHeader();
              this.logged = true;
            }
          });
        } else {
          jump("login");
        }
      }
    });
    net.setHeader();
    this.getStatus();
    window.addEventListener("resize", this.resizeMenu);
  },
  methods: {
    jump: jump,
    formatDoubleTimeSpan: formatDoubleTimeSpan,
    formatDateTime: formatDateTime,
    changeMenuSize() {
      this.menuCollapse = !this.menuCollapse;
    },
    resizeMenu() {
      this.headerHeight =
        document.getElementsByClassName("header")[0].scrollHeight;
      this.footerHeight =
        document.getElementsByClassName("footer")[0].scrollHeight;

      this.windowWidth = window.innerWidth;
      this.menuCollapse = window.innerWidth < 500;
    },

 
    delayGetStatus() {
      setTimeout(this.getStatus, 500);
    },
    logout() {
      this.$confirm("是否注销？", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      }).then(() => {
        Cookies.remove("token");
        location.reload();
      });
    },
    getStatus() {
      net
        .getQueueStatus()
        .then((response) => {
          this.netError = false;
          this.status = response.data;
          this.$nextTick(this.resizeMenu);
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
  components: { StatusBar },
});
</script>

<style scoped>
#app {
  height: 100%;
}

.header {
  margin-left: -12px;
  margin-right: -12px;
  margin-top: -12px;
  background: #ebeef5;
  color: #606266;
}
</style>
