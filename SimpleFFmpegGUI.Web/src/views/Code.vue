<template>
  <div>
    <el-form label-width="120px">
      <el-form-item label="输入文件" v-for="value in files" :key="value.index">
        <a class="el-form-item__label">{{ value.index + 1 }}</a>
        <file-select
          ref="files"
          @select="(f) => selectFile(f, value.index)" class="right24"
        ></file-select>
        <el-button
          @click="addFile"
          icon="el-icon-plus"
          circle
          v-if="value.index == files.length - 1" class="right24"
        ></el-button>
        <el-button
          @click="files.splice(-1)"
          icon="el-icon-close"
          circle
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

      <el-form-item label="输入文件参数">
        <el-form-item label="裁剪">
          <el-switch v-model="inputArgs.enableClip"> </el-switch>
          <div v-show="inputArgs.enableClip">
            <el-input-number
              v-model="inputArgs.timeFromH"
              controls-position="right"
              :min="0"
              :max="100"
              size="small"
              class="time"
            ></el-input-number>
            <a>:</a>
            <el-input-number
              v-model="inputArgs.timeFromM"
              controls-position="right"
              :min="0"
              :max="59"
              size="small"
              class="time"
            ></el-input-number>
            <a>:</a>
            <el-input-number
              v-model="inputArgs.timeFromS"
              controls-position="right"
              :min="0"
              :precision="3"
              :max="59.999"
              size="small"
              class="time"
            ></el-input-number>
          </div>
          <div v-show="inputArgs.enableClip">
            <a style="margin-left: -28px">到：</a>
            <el-input-number
              v-model="inputArgs.timeToH"
              controls-position="right"
              :min="0"
              :max="100"
              size="small"
              class="time"
            ></el-input-number>
            <a>:</a>
            <el-input-number
              v-model="inputArgs.timeToM"
              controls-position="right"
              :min="0"
              :max="59"
              size="small"
              class="time"
            ></el-input-number>
            <a>:</a>
            <el-input-number
              v-model="inputArgs.timeToS"
              controls-position="right"
              :min="0"
              :precision="3"
              :max="59.999"
              size="small"
              class="time"
            ></el-input-number>
          </div>
        </el-form-item>
      </el-form-item>
      <el-form-item label="预设">
        <div>
          <el-select
            @change="selectPreset"
            placeholder="加载预设"
            v-model="preset" class="right24"
          >
            <el-option
              v-for="p in presets"
              :key="p.id"
              :label="p.name"
              :value="p.id"
            ></el-option
          ></el-select>
          <el-button
            :disabled="preset == null"
            @click="updatePreset"
            >更新</el-button
          >
        </div>
        <div style="margin-top: 12px">
          <el-input v-model="newPresetName" style="width: 128px" class="right24"></el-input>
          <el-button
            style="display: inline"
            :disabled="newPresetName == null || newPresetName.trim() == ''"
            @click="savePreset"
            >保存或更新“{{ newPresetName }}”</el-button
          >
        </div>
      </el-form-item>
    </el-form>

    <code-arguments ref="args" />
    <el-form label-width="120px">
      <el-form-item>
        <div class="bottom-div">
          <el-button type="primary" @click="add" class="right24 bottom12">加入队列</el-button>
          <el-button @click="addAndStart" style="margin-left:0">加入队列并立即开始</el-button>
        </div>
      </el-form-item>
    </el-form>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import { showError, jump, showSuccess } from "../common";
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
      inputArgs: {
        enableClip: false,
        timeFromH: 0,
        timeFromM: 0,
        timeFromS: 0,
        timeToH: 0,
        timeToM: 0,
        timeToS: 0,
      },
      presets: [],
      preset: null,
      newPresetName: "新预设",
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
    updatePreset() {
      let args = (this.$refs.args as any).getArgs();
      const name = (
        this.presets.filter((p) => (p as any).id == this.preset)[0] as any
      ).name;
      net
        .postAddOrUpdatePreset({ name: name, arguments: args })
        .then((r) => {
          showSuccess("更新预设成功");
          this.fillPresetsAnd((p) => {
            this.preset = r.data as any;
          });
        })
        .catch(showError);
    },
    selectPreset(preset: number) {
      (this.$refs.args as any).updateFromArgs(
        (this.presets.filter((p) => (p as any).id == preset)[0] as any)
          .arguments
      );
    },
    fillPresets() {
      this.fillPresetsAnd((id) => {
        return;
      });
    },
    fillPresetsAnd(action: (id: number) => void) {
      net
        .getPresets()
        .then((r) => {
          this.presets = r.data;
          console.log(r.data);

          action(r.data);
        })
        .catch(showError);
    },
    savePreset() {
      let args = (this.$refs.args as any).getArgs();
      net
        .postAddOrUpdatePreset({ name: this.newPresetName, arguments: args })
        .then((r) => {
          showSuccess("新建或更新预设成功");
          this.fillPresetsAnd((p) => {
            this.preset = r.data as any;
          });
        })
        .catch(showError);
    },
    addCode(start: boolean) {
      if (this.files.filter((p) => p.path != "").length == 0) {
        showError("请选择输入文件");
        return;
      }
      let args = (this.$refs.args as any).getArgs();
      if (this.inputArgs.enableClip) {
        args.input = {};
        args.input.from =
          this.inputArgs.timeFromH * 3600 +
          this.inputArgs.timeFromM * 60 +
          this.inputArgs.timeFromS;
        args.input.to =
          this.inputArgs.timeToH * 3600 +
          this.inputArgs.timeToM * 60 +
          this.inputArgs.timeToS;
        if (args.input.to <= args.input.from) {
          showError("结束时间需要小于开始时间");
          return;
        }
      }
      net
        .postAddCodeTask({
          input: this.files.filter((p) => p.path != "").map((p) => p.path),
          output: this.output,
          argument: args,
          start: start,
        })
        .then((response) => {
          this.files=[];
          this.files.push({ index: 0, path: "" });
          (this.$refs.files as any)[0].file = "";
          this.output="";
          showSuccess("已加入队列");
        })
        .catch(showError);
    },
  },
  components: { CodeArguments },
  mounted: function () {
    this.$nextTick(function () {
      this.fillPresets();
    });
  },
});
</script>
<style scoped>
.with-slider {
  margin-bottom: 24px;
}

.time {
  width: 108px;
  margin-left: 12px;
  margin-right: 12px;
}
.bottom-div {
  display: inline-block;
  margin-top: 36px;
  margin-right: 24px;
}
</style>
