<template>
  <div>
    <h2>输入文件夹</h2>
    <a>{{ status.inputOn ? "已启动，端口号" + status.inputPort : "未启动" }}</a>
    <el-button
      style="margin-left: 24px"
      :type="status.inputOn ? 'danger' : 'primary'"
      @click="post(true,status.inputOn?false:true)"
    >
      {{ status.inputOn ? "关闭" : "开启" }}
    </el-button>
    <h2>输出文件夹</h2>
    <a>{{ status.outputOn ? "已启动，端口号" + status.outputPort : "未启动" }}</a>
    <el-button
      style="margin-left: 24px"
      :type="status.outputOn ? 'danger' : 'primary'"
      @click="post(false,status.outputOn?false:true)"
    >
      {{ status.outputOn ? "关闭" : "开启" }}
    </el-button>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import {
  withToken,
  showError,
  jump,
  formatDateTime,
  formatDoubleTimeSpan,
} from "../common";
import * as net from "../net";
import FileSelect from "@/components/FileSelect.vue";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      status: null,
    };
  },
  computed: {},
  methods: {
    getStatus() {
      net
        .getFtpStatus()
        .then((r) => {
          this.status = r.data;
        })
        .catch(showError);
    },
    post(input: boolean, on: boolean) {
      net
        .postFtp(input, on)
        .then((r) => {
          this.getStatus();
        })
        .catch(showError);
    },
  },
  components: {},
  mounted: function () {
    this.$nextTick(function () {
      this.getStatus();
    });
  },
});
</script>
