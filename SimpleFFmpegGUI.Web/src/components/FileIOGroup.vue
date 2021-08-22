
<template>
  <div>
    <el-form-item label="输入">
      <el-collapse v-model="activeInput">
        <el-collapse-item
          :name="index"
          v-for="(value, index) in inputFiles"
          :key="index"
        >
          <template slot="title">
            <a>
              文件{{ index + 1 }}
              {{ value.name }}
              {{ value.enableFrom ? `从${value.from}s` : "" }}
              {{ value.enableTo ? `到${value.to}s` : "" }}
              {{ value.enableDuration ? `经过${value.duration}s` : "" }}
            </a>
          </template>
          <div class="top12">
            <file-select
              style="margin-left: 7px"
              ref="files"
              @update:file="(f) => updateFile(f, index)"
              :file="value.name"
              class="right24"
            ></file-select>
          </div>

          <div>
            <time-input
              :enabled.sync="value.enableFrom"
              label="开始"
              :time.sync="value.from"
            ></time-input>
            <time-input
              :enabled.sync="value.enableTo"
              label="结束"
              :time.sync="value.to"
            ></time-input>
            <time-input
              :enabled.sync="value.enableDuration"
              label="经过"
              :time.sync="value.duration"
            ></time-input>
            <a class="gray" v-if="value.enableTo && value.enableDuration"
              >同时设置“结束”和“经过”时间后，优先取经过时间</a
            >
          </div>
        </el-collapse-item>
      </el-collapse>
      <div class="top12">
        <el-button
          @click="addFile"
          icon="el-icon-plus"
          circle
          class="right12"
        ></el-button>
        <el-button
          @click="removeFile"
          icon="el-icon-close"
          circle
          v-if="inputFiles.length > min"
        ></el-button>
      </div>
    </el-form-item>
    <el-form-item label="输出">
      <el-input
        placeholder="输出文件名"
        style="width: 300px; display: block"
        v-model="outputFile"
        :disabled="inputFiles.length > 1"
        @change="(value) => $emit('update:output', value)"
      >
      </el-input>
      <a v-if="inputFiles.length > 1" class="gray"
        >输入多个文件时，输出文件名为首个不重复的原文件名</a
      >
    </el-form-item>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";

import TimeInput from "@/components/TimeInput.vue";
import { showError, jump, formatDateTime } from "../common";
export default Vue.component("file-io-group", {
  data() {
    return {
      activeInput: [0],
      outputFile: "",
      inputFiles: [
        {
          name: "",
          enableFrom: false,
          enableTo: false,
          enableDuration: false,
          from: 0,
          to: 0,
          duration: 0,
        },
      ],
    };
  },
  props: {
    inputs: {
      default: [],
    },
    output: { 
      default: "" 
    },
    min:{
      default:1
    }
  },
  computed: {},
  watch: {
    inputs() {
      this.updateFromArgs(this.inputs);
    },
    output() {
      this.outputFile = this.output;
    },
  },
  methods: {
    addFile() {
      this.inputFiles.push(this.getNewFile());
      this.activeInput = [this.inputFiles.length - 1];
      this.$emit("update:inputs", this.getArgs());
    },
    getNewFile() {
      return {
        name: "",
        enableFrom: false,
        enableTo: false,
        enableDuration: false,
        from: 0,
        to: 0,
        duration: 0,
      };
    },
    updateFile(file: string, index: number) {
      this.inputFiles[index].name = file;
      if (index == 0 && this.output == "") {
        this.outputFile = file;
      }
      this.$emit("update:inputs", this.getArgs());
      this.$emit("update:output", this.output);
    },
    removeFile() {
      this.inputFiles.splice(-1);
      this.$emit("update:inputs", this.getArgs());
    },
    getArgs() {
      const inputs: any[] = [];
      this.inputFiles
        .filter((p) => p.name != "")
        .forEach((file) => {
          console.log(file);
          if (file.to && file.from && file.to <= file.from) {
            showError("结束时间需要小于开始时间");
            return;
          }
          inputs.push({
            filePath: file.name,
            from: file.enableFrom ? file.from : null,
            to: file.enableTo ? file.to : null,
            duration: file.enableDuration ? file.duration : null,
          });
        });
      return inputs;
    },
    updateFromArgs(args: any): any {
      this.inputFiles = [];

      args.forEach((file: any) => {
        this.inputFiles.push({
          name: file.filePath,
          enableFrom: file.from != null,
          from: file.from != null ? file.from : null,

          enableTo: file.to != null,
          to: file.to != null ? file.to : null,

          enableDuration: file.duration != null,
          duration: file.duration != null ? file.duration : null,
        });
        console.log(this.inputFiles);
      });
    },
  },
  components: { TimeInput },
  mounted: function () {
    this.$nextTick(function () {
      return;
    });
  },
});
</script>
<style scoped>
</style>