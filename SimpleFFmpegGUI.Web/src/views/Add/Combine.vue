<template>
  <div>
    <el-form label-width="100px">
      <h2>输入和输出</h2>
      <el-form-item label="视频">
        <file-select
          ref="videoFile"
          @select="(f) => selectFile(f, true)"
          class="right24"
        ></file-select>
      </el-form-item>
      <el-form-item label="音频">
        <file-select
          ref="audioFile"
          @select="(f) => selectFile(f, false)"
          class="right24"
        ></file-select>
      </el-form-item>

      <el-form-item label="输出">
        <el-input
          placeholder="输出文件名"
          style="width: 300px; display: block"
          v-model="output"
        />
        <a style="color: gray; margin-left: 18.21px"
          >输出文件名在处理时会自动重命名为首个不存在重复文件的文件名</a
        >
      </el-form-item>
    </el-form>
    <h2>参数</h2>
    <code-arguments type="combine" ref="args" />

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
      video: "",
      audio: "",
      output: "",
    };
  },
  computed: {},
  methods: {
    jump: jump,

    selectFile(file: string, isVideo: boolean) {
      if (isVideo) {
        this.video = file;
        this.output = file;
      } else {
        this.audio = file;
      }
    },
    add() {
      this.addTask(false);
    },
    addAndStart() {
      this.addTask(true);
    },

    addTask(start: boolean) {
      if (this.video == "" || this.audio == "") {
        showError("请选择输入文件");
        return;
      }

      net
        .postAddCombineTask({
          input: [this.video, this.audio],
          output: this.output,
          argument: (this.$refs.args as any).getArgs(),
          start: start,
        })
        .then((response) => {
          this.video = "";
          this.audio = "";
          (this.$refs.videoFile as any).file = "";
          (this.$refs.audioFile as any).file = "";
          this.output = "";
          showSuccess("已加入队列");
        })
        .catch(showError);
    },
  },
  components: { CodeArguments, AddToTaskButtons },
  mounted: function () {
    this.$nextTick(function () {
      loadArgs(this.$refs.args);
    });
  },
});
</script>
<style scoped>
.with-slider {
  margin-bottom: 24px;
}

.time {
  width: 72px;
}
.time-second {
  width: 108px;
}
.time-colon {
  margin-left: 6px;
  margin-right: 6px;
}
.time-text {
  max-width: 320px;
}
.bottom-div {
  display: inline-block;
  margin-top: 36px;
  margin-right: 24px;
}
</style>
