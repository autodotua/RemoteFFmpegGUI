<template>
  <div>
    <el-form label-width="100px">
      <h2>输入和输出</h2>
      <el-form-item
        label="输入文件"
        v-for="(value, index) in files"
        :key="index"
      >
        <a class="el-form-item__label">{{ index + 1 }}</a>
        <file-select
          ref="files"
          @update:file="(f) => updateFile(f, index)"
          :file="value"
          class="right24"
        ></file-select>
        <el-button
          @click="files.push('')"
          icon="el-icon-plus"
          circle
          v-if="index == files.length - 1"
          class="right12"
        ></el-button>
        <el-button
          @click="files.splice(-1)"
          icon="el-icon-close"
          circle
          v-if="index == files.length - 1 && index > 0"
        ></el-button>
      </el-form-item>
      <el-form-item v-if="files.length > 1" label="">
        <a style="margin-left: 18.21px" class="gray"
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

      <el-form-item label="裁剪">
        <div>
          <br>
          <time-input :enabled.sync="inputArgs.enableFrom" label="从" :time.sync="inputArgs.from"></time-input>
          <time-input :enabled.sync="inputArgs.enableTo" label="到" :time.sync="inputArgs.to">></time-input>
          <time-input :enabled.sync="inputArgs.enableDuration" label="经过" :time.sync="inputArgs.duration">></time-input>
      
        </div>
        <a v-if="inputArgs.timeParseError != ''" style="color: red">{{
          inputArgs.timeParseError
        }}</a>
      </el-form-item>

      <h2>参数</h2>
    </el-form>

    <code-arguments ref="args" :type="0" />
    <add-to-task-buttons :addFunc="addTask"></add-to-task-buttons>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import { showError, jump, showSuccess, loadArgs } from "../../common";
import * as net from "../../net";
import CodeArguments from "@/components/CodeArguments.vue";
import PresetSelect from "@/components/PresetSelect.vue";
import AddToTaskButtons from "@/components/AddToTaskButtons.vue";
import TimeInput from "@/components/TimeInput.vue";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      files: [""],
      output: "",
      inputArgs: {
       from:0,
       to:0,
       duration:0,
        enableFrom: false,
        enableTo: false,
        enableDuration: false,
      },
    };
  },
  computed: {},
  methods: {
    jump: jump,
 
    updateFile(file: string, index: number) {
      this.files[index] = file;
      if (index == 0 && this.output == "") {
        this.output = file;
      }
    },

    addTask(start: boolean) {
      if (this.files.filter((p) => p != "").length == 0) {
        showError("请选择输入文件");
        return;
      }
      let args = (this.$refs.args as any).getArgs();
      if (args == null) {
        return;
      }
      args.input = {};


      if (this.inputArgs.enableFrom) {
        args.input.from =this.inputArgs.from;
      }
      if (this.inputArgs.enableTo) {
        args.input.to =this.inputArgs.to;
      }
      if (this.inputArgs.enableDuration) {
        args.input.duration =this.inputArgs.duration;
      }
      if (
        args.input.to &&
        args.input.from &&
        args.input.to <= args.input.from
      ) {
        showError("结束时间需要小于开始时间");
        return;
      }console.log(args.input);

      net
        .postAddCodeTask({
          input: this.files.filter((p) => p != ""),
          output: this.output,
          argument: args,
          start: start,
        })
        .then((response) => {
          this.files = [];
          this.files.push("");
          this.output = "";
          showSuccess("已加入队列");
        })
        .catch(showError);
    },
  },
  components: { CodeArguments, AddToTaskButtons, TimeInput },
  mounted: function () {
    this.$nextTick(function () {
      const inputOutput = loadArgs(this.$refs.args);
      if (inputOutput.inputs) {
        this.files = [];
        for (let i = 0; i < inputOutput.inputs.length; i++) {
          this.files.push(inputOutput.inputs[i]);
        }
      }
      if (inputOutput.output) {
        this.output = inputOutput.output;
      }
    });
  },
});
</script>
<style scoped>

</style>
