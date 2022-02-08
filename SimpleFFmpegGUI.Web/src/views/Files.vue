<template>
  <div>
    <div v-if="status != null">
      <h2>FTP</h2>
      <el-row>
        <el-col :span="12">
          <h3>输入文件夹</h3>
          <a>{{
            status.inputOn ? "已启动，端口号" + status.inputPort : "未启动"
          }}</a>
          <el-button
            style="margin-left: 24px"
            :type="status.inputOn ? 'danger' : 'primary'"
            @click="post(true, status.inputOn ? false : true)"
          >
            {{ status.inputOn ? "关闭" : "开启" }}
          </el-button></el-col
        >
        <el-col :span="12">
          <h3>输出文件夹</h3>
          <a>{{
            status.outputOn ? "已启动，端口号" + status.outputPort : "未启动"
          }}</a>
          <el-button
            style="margin-left: 24px"
            :type="status.outputOn ? 'danger' : 'primary'"
            @click="post(false, status.outputOn ? false : true)"
          >
            {{ status.outputOn ? "关闭" : "开启" }}
          </el-button></el-col
        ></el-row
      >
    </div>
    <div class="top24">
      <h2>上传输入文件</h2>
      <a>仅支持小文件，大文件请通过其他方式上传</a>
      <el-upload
        class="top12"
        :action="getUploadUrl()"
        :auto-upload="false"
        :headers="getHeader()"
        ref="upload"
      >
        <el-button slot="trigger" size="small" type="primary" class="right24"
          >浏览文件</el-button
        >
        <el-button size="small" type="success" @click="upload"
          >上传到服务器</el-button
        >
      </el-upload>
    </div>
    <div v-if="files != null" class="top24">
      <h2>输出文件下载</h2>
      <el-table ref="table" :data="files">
        <el-table-column prop="name" label="文件名" min-width="120" />
        <el-table-column prop="lengthText" label="大小" width="120" />
        <el-table-column prop="lastWriteTime" label="修改时间" width="200">
          <template slot-scope="scope">
            {{ formatDateTime(scope.row.lastWriteTime) }}
          </template>
        </el-table-column>

        <el-table-column label="操作" width="50">
          <template slot-scope="scope">
            <el-button
              slot="reference"
              type="text"
              size="small"
              @click="download(scope.row)"
              >下载</el-button
            >
          </template>
        </el-table-column>

        <el-table-column align="right">
          <template slot="header">
            <el-button type="text" @click="fillData()">刷新</el-button>
          </template>
        </el-table-column>
      </el-table>
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
