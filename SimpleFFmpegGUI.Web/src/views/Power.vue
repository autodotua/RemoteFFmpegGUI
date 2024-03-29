<template>
  <div>
    <div class="top24">
      <h2>立即关机</h2>
      <el-popconfirm title="是否立即关机？" @confirm="shutdown" class="right12">
        <el-button type="danger" slot="reference">立即关机</el-button></el-popconfirm>
      <el-button type="secondary" slot="reference" @click="abortShutdown">终止关机</el-button>
    </div>

    <div class="top24">
      <h2>队列结束后关机</h2>
      <a class="right12">是否在完成当前队列后自动关机</a>
      <el-switch v-model="shutdown_queue" @change="setShutdownQueue"></el-switch>
    </div>

    <div class="top24">
      <h2>进程优先级</h2>
      <a>默认进程优先级</a>
        <el-slider style="width: 240px" class="left12" @change="updateDefaultProcessPriority" :max="4" :show-tooltip="false"
        v-model="defaultProcessPriority" :marks="processPriorities"></el-slider>
    </div>

    <div class="top24">
      <h2>CPU占用率</h2>
      <div v-for="item in cpuCoreUsages" :key="item.id" style="display: inline-block" class="bottom12 right12">
        <el-progress :percentage="item.usage" :width="48" type="circle" :color="colors"></el-progress>
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
      defaultProcessPriority: 2,
      colors: [
        { color: "green", percentage: 50 },
        { color: "orange", percentage: 80 },
        { color: "red", percentage: 100 },
      ],
      processPriorities: {
        0: "空闲",
        1: "低于正常",
        2: {
          style: {
            color: "#1989FA",
          },
          label: this.$createElement("strong", "正常"),
        },
        3: "高于正常",
        4: "高",
      },
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
          showError("发送关机命令失败");
        });
    },
    abortShutdown() {
      net
        .postAbortShutdown()
        .then(() => {
          showSuccess("发送终止关机命令成功");
        })
        .catch(() => {
          showError("发送终止关机命令失败");
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
    updateDefaultProcessPriority(priority: number) {
      net.postDefaultProcessPriority(priority)
        .catch(() => {
          showError("修改默认进程优先级失败");
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
    loadDefaultProcessPriority() {
      net.getDefaultProcessPriority()
        .then((response) => {
          this.defaultProcessPriority = response.data
        })
        .catch(() => {
          showError("加载默认进程优先级失败");
        });
    }
  },
  components: {},
  mounted: function () {
    showLoading();
    this.$nextTick(function () {
      this.updateShutdownQueue();
      this.loadDefaultProcessPriority();
      this.loadCpuCoreUsage();
      setInterval(() => {
        this.loadCpuCoreUsage();
      }, 5000);
    });
  },
});
</script>
