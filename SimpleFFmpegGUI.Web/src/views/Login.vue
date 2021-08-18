<template>
  <div class="login">
    <h1>登录</h1>
    <div class="box center" style="max-width: 320px">
      <el-input id="name" v-model="token" placeholder="请输入Token">
        <template slot="prepend">Token</template>
      </el-input>

      <br />

      <br />

      <el-button
        :loading="btnLoading"
        id="login"
        v-on:click="login"
        style="width: 100%"
        type="primary"
        >登录</el-button
      >

      <br />
      <br />

      <!-- <el-link href="register" >注册</el-link> -->
    </div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";

import * as net from "../net";
import Cookies from "js-cookie";
import { Notification } from "element-ui";
import { showError, jump, showSuccess } from "../common";
import { AxiosResponse } from "axios";
export default Vue.extend({
  data: function () {
    return { token: "", btnLoading: false };
  },
  methods: {
    login() {
      this.btnLoading = true;
      net
        .getCheckToken(this.token)
        .then((r) => {
          if (!r.data) {
            showError("Token错误");
          }
          else{
            Cookies.set("token",this.token)
            net.setHeader();
            this.$router.push({path:"/"})
          }
        })
        .catch(showError)
        .finally(() => {
          this.btnLoading = false;
        });
    },
  },
  mounted: function () {
    this.$nextTick(function () {
      return;
    });
  },
});
</script>
<style scoped>
.box {
  width: 100%;
}
.box .el-row {
  width: 100%;
}
.center {
  display: table;
  margin: 0 auto;
  border: 0;
}
</style>