<template>
  <div>
    <div class="top24">
      <h2>立即关机</h2>
      <el-popconfirm title="是否立即关机？" @confirm="shutdown" class="right12">
        <el-button type="danger" slot="reference"
          >立即关机</el-button
        ></el-popconfirm
      >
      <el-button type="secondary" slot="reference" @click="abortShutdown"
        >终止关机</el-button
      >
    </div>

    <div class="top24">
      <h2>队列结束后关机</h2>
      <a class="right12">是否在完成当前队列后自动关机</a>
      <el-switch
        v-model="shutdown_queue"
        @change="setShutdownQueue"
      ></el-switch>
    </div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import {
  showError,
  showSuccess,
  formatDateTime,
  showLoading,
  closeLoading,
} from "../common";
import * as net from "../net";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      shutdown_queue: false,
    };
  },
  computed: {},
  methods: {
    getHeader: net.getHeader,
    getUploadUrl: net.getUploadUrl,
    formatDateTime: formatDateTime,
    shutdown() {
      net.postShutdown();
    },
    abortShutdown() {
      net
        .postAbortShutdown()
        .then(() => {
          showSuccess("发送终止关机命令成功");
        })
        .catch(() => {
          showSuccess("发送终止关机命令失败");
        });
    },
    setShutdownQueue(value: boolean) {
      net
        .postShutdownQueue(value)
        .then(() => {
          showSuccess("发送关机命令成功");
        })
        .catch(() => {
          showSuccess("发送关机命令失败");
        });
    },
    updateShutdownQueue() {
      net.getShutdownQueue().then((v) => {
        if (v.data === true) {
          this.shutdown_queue = true;
        } else if (v.data === false) {
          this.shutdown_queue = false;
        } else {
          showError("获取是否在队列结束后自动关机的状态失败");
        }
        closeLoading();
      });
    },
  },
  components: {},
  mounted: function () {
    showLoading();
    this.$nextTick(function () {
      this.updateShutdownQueue();
    });
  },
});
</script>
