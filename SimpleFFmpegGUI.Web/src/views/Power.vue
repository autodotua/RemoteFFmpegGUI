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

    <div class="top24">
      <h2>CPU占用率</h2>
      <div
        v-for="item in cpuCoreUsages"
        :key="item.id"
        style="display: inline-block"
        class="bottom12 right12"
      >
        <el-progress
          :percentage="item.usage"
          :width="48"
          type="circle"
          :color="colors"
        ></el-progress>
      </div>
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
      cpuCoreUsages: Array<any>(),
      colors: [
        { color: "green", percentage: 50 },
        { color: "orange", percentage: 80 },
        { color: "red", percentage: 100 },
      ],
    };
  },
  computed: {},
  methods: {
    getHeader: net.getHeader,
    getUploadUrl: net.getUploadUrl,
    formatDateTime: formatDateTime,
    shutdown() {
      net
        .postShutdown()
        .then(() => {
          showSuccess("发送关机命令成功，计算机将在3分钟后关机。");
        })
        .catch(() => {
          showSuccess("发送关机命令失败");
        });
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
      net.postShutdownQueue(value);
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
    loadCpuCoreUsage() {
      net.getCpuCoreUsage().then((r) => {
        this.cpuCoreUsages = [];
        r.data.forEach((p: any) => {
          if (p.cpuIndex >= 0) {
            p.id = p.cpuIndex * 1000 + p.coreIndex;
            p.usage = Math.round(p.usage * 100);
            if (p.usage > 100) p.usage = 100;
            if (p.usage < 0) p.usage = 0;
            this.cpuCoreUsages.push(p);
          }
        });
      });
    },
  },
  components: {},
  mounted: function () {
    showLoading();
    this.$nextTick(function () {
      this.updateShutdownQueue();
      this.loadCpuCoreUsage();
      setInterval(() => {
        this.loadCpuCoreUsage();
      }, 5000);
    });
  },
});
</script>
