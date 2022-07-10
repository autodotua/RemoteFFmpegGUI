<template>
  <div>
    <div class="top24">
      <h2>立即关机</h2>
      <el-popconfirm title="是否立即关机？" @confirm="cancel" class="right12">
        <el-button type="danger" slot="reference"
          >立即关机</el-button
        ></el-popconfirm
      >
    </div>

    <div class="top24">
      <h2>定时关机</h2>
      <a>定时：</a>
      <el-input style="width: 128px" class="right12"></el-input>
      <a class="right24">分钟</a>
      <el-popconfirm title="是否立即关机？" @confirm="cancel" class="right12">
        <el-button type="danger" slot="reference"
          >立即关机</el-button
        ></el-popconfirm
      >
    </div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import {
  showError,
  jump,
  formatDateTime,
  formatDoubleTimeSpan,
  showLoading,
  closeLoading,
} from "../common";
import * as net from "../net";
import FileSelect from "@/components/FileSelect.vue";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      status: null,
      files: [],
    };
  },
  computed: {},
  methods: {
    getHeader: net.getHeader,
    getUploadUrl: net.getUploadUrl,
    formatDateTime: formatDateTime,
    download(file: any) {
      const name = file.name;
      net.download(name);
    },
    upload() {
      (this.$refs.upload as any).submit();
    },
    getStatus() {
      return net
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
    fillData() {
      showLoading();
      return net
        .getMediaDetails()
        .then((response) => {
          this.files = response.data;
        })
        .catch(showError)
        .finally(closeLoading);
    },
  },
  components: {},
  mounted: function () {
    showLoading();
    this.$nextTick(function () {
      this.fillData();
      this.getStatus().finally(closeLoading);
    });
  },
});
</script>
