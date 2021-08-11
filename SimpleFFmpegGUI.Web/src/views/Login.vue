<template>
  <div class="login">
    <h1>登录</h1>
    <div class="box">
      <el-input id="name" v-model="username" placeholder="请输入帐号">
        <template slot="prepend">帐号</template>
      </el-input>

      <br />
      <el-input id="password" v-model="password" type="password" placeholder="请输入密码">
        <template slot="prepend">密码</template>
      </el-input>

      <br />
      <br />

      <el-button
        id="login"
        v-on:click="login"
        :disabled="buttonsDisabled"
        style="width:100%"
        type="primary"
      >登录</el-button>

      <br />
      <br />
   

      <!-- <el-link href="register" >注册</el-link> -->
    </div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import { Notification } from "element-ui";
import { showError, getUrl, jump ,showSuccess} from "../common";
import { AxiosResponse } from "axios";
export default Vue.extend({
  data: function() {
    return { username: "", password: "", buttonsDisabled: false };
  },
  methods: {
    afterLogin(response: AxiosResponse<any>, register = false) {
      if (response.data.succeed) {
        Cookies.set("userID", response.data.data.userID);
        Cookies.set("token", response.data.data.token);
        Cookies.set("username", response.data.data.carOwner.username);

        jump("");
      } else {
        Notification.error(
          (register ? "注册失败：" : "登陆失败：") + response.data.message
        );
      }
    },
    login() {
      Vue.axios
        .post(getUrl("User/Login"), {
          UserName: this.username,
          Password: this.password
        })
        .then(response => {
          this.afterLogin(response, false);
        })
        .catch(showError);
    },
  },
  mounted: function() {
    this.$nextTick(function() {
     if(this.$route.query.username)
     {
       showSuccess("正在登陆，请稍后")
       this.username=this.$route.query.username as string;
       this.password=this.$route.query.password as string;
       this.login();
     }
    });
  }
});
</script>
<style>
.box {
  width: 100%;
}
.box .el-row {
  width: 100%;
}
</style>