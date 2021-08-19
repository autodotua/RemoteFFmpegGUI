<template>
  <div>
    <el-form label-width="100px">
      <h2>输入和输出</h2>
      <el-form-item label="视频1">
        <file-select :file.sync="video1" class="right24"></file-select>
      </el-form-item>
      <el-form-item label="视频2">
        <file-select :file.sync="video2" class="right24"></file-select>
      </el-form-item>
    </el-form>

    <add-to-task-buttons :addFunc="addTask"></add-to-task-buttons>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import { showError, jump, showSuccess, loadArgs } from "../../common";
import * as net from "../../net";
import CodeArguments from "@/components/CodeArguments.vue";
import AddToTaskButtons from "@/components/AddToTaskButtons.vue";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      video1: "",
      video2: "",
    };
  },
  computed: {},
  methods: {
    add() {
      this.addTask(false);
    },
    addAndStart() {
      this.addTask(true);
    },

    addTask(start: boolean) {
      if (this.video1 == "" || this.video2 == "") {
        showError("请选择输入文件");
        return;
      }

      net
        .postAddCompareTask({
          input: [this.video1, this.video2],
          start: start,
        })
        .then((response) => {
          this.video1 = "";
          this.video2 = "";
          showSuccess("已加入队列");
        })
        .catch(showError);
    },
  },
  components: { AddToTaskButtons },
  mounted: function () {
    this.$nextTick(function () {
      const inputOutput = loadArgs(null);
      if (inputOutput.inputs) {
        this.video1 = inputOutput.inputs[0];
        this.video2 = inputOutput.inputs[1];
      }
    });
  },
});
</script>
<style scoped>
.bottom-div {
  display: inline-block;
  margin-top: 36px;
  margin-right: 24px;
}
</style>
