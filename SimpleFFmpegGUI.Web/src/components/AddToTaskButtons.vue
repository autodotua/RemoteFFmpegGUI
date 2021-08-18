
<template>
  <div class="out">
    <div class="in">
      <el-button-group>
        <el-button type="primary" @click="add">加入队列</el-button>
        <el-button @click="addAndStart">加入队列并立即开始</el-button>
      </el-button-group>
    </div>
  </div>
</template>
<script >
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";
import { withToken, showError, jump, formatDateTime } from "../common";
export default Vue.component("add-to-task-buttons", {
  data() {
    return {
      files: null,
      file: null,
    };
  },
  props: ["addFunc"],
  computed: {},
  methods: {
    add() {
      this.addFunc(false);
    },
    addAndStart() {
      this.addFunc(true);
    },
  },
  components: {},
  mounted: function () {
    this.$nextTick(function () {
      return;
    });
  },
});
</script>
<style scoped>
.out {
  position: sticky;
  bottom: -20px;
  width: 100%;
  height: 48px;
  z-index: 10;
  background-color: white;
}
.in {
  float: right;
  margin-top: 8px;
}
</style>