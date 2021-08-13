<template>
  <div>
    <el-form label-width="120px">
      <el-form-item label="输入文件" v-for="value in files" :key="value.index">
        <a class="el-form-item__label">{{ value.index + 1 }}</a>
        <file-select
          ref="files"
          @select="(f) => selectFile(f, value.index)"
        ></file-select>
        <el-button
          @click="addFile"
          icon="el-icon-plus"
          circle
          class="left24"
          v-if="value.index == files.length - 1"
        ></el-button>
        <el-button
          @click="files.splice(-1)"
          icon="el-icon-close"
          circle
          class="left24"
          v-if="value.index == files.length - 1 && value.index > 0"
        ></el-button>
      </el-form-item>
      <el-form-item v-if="files.length > 1" label="">
        <a style="color: gray; margin-left: 18.21px"
          >输入多个文件时，将按顺序进行拼接</a
        >
      </el-form-item>
      <el-form-item label="输出">
        <el-input
          placeholder="输出文件名"
          style="width: 300px; margin-left: 18px; display: block"
          v-model="output"
        />
        <a style="color: gray; margin-left: 18.21px"
          >输出文件名在处理时会自动重命名为首个不存在重复文件的文件名</a
        >
      </el-form-item>
    </el-form>
    <code-arguments ref="args" />
    <el-form label-width="120px">
      <el-form-item style="margin-top: 36px">
        <el-button type="primary" @click="add">加入队列</el-button>
        <el-button @click="addAndStart">加入队列并立即开始</el-button>
      </el-form-item>
    </el-form>
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
  showSuccess,
} from "../common";
import * as net from "../net";
import CodeArguments from "@/components/CodeArguments.vue";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      files: [
        {
          index: 0,
          path: "",
        },
      ],
      output: "",
    };
  },
  computed: {},
  methods: {
    jump: jump,
    selectFile(file: string, index: number) {
      this.files[index].path = file;
      if (index == 0 && this.output == "") {
        this.output = file;
      }
    },
    addFile() {
      this.files.push({ index: this.files.length, path: "" });
    },
    add() {
      this.addCode(false);
    },
    addAndStart() {
      this.addCode(true);
    },
    addCode(start: boolean) {
      if (this.files.filter((p) => p.path != "").length == 0) {
        showError("请选择输入文件");
        return;
      }

      net
        .postAddCodeTask({
          input: this.files.filter((p) => p.path != "").map((p) => p.path),
          output: this.output,
          argument: (this.$refs.args as any).getArgs(),
          start: start,
        })
        .then((response) => {
          (this.$refs.files as any).file = "";
          this.files.length = 1;
          this.files[0].path = "";
          showSuccess("已加入队列");
        })
        .catch(showError);
    },
  },
  components: { CodeArguments },
  mounted: function () {
    this.$nextTick(function () {
      return;
    });
  },
});
</script>
<style scoped>
.with-slider {
  margin-bottom: 24px;
}

.left24 {
  margin-left: 24px;
}
</style>