
<template>
  <div>
    <el-row :gutter="12">
      <el-col :span="1">
        <el-checkbox style="margin-top: 15px" v-model="isEnabled"></el-checkbox>
      </el-col>
      <el-col :sm="22" :md="10" class="top12">
        <el-input
          :disabled="!isEnabled"
          maxlength="13"
          placeholder="时间格式：12:34:56.123"
          v-model="str"
          class="time-text"
        >
          <template slot="prepend"> {{ label }}</template>
        </el-input>
      </el-col>
      <el-col :sm="24" :md="11" class="top12">
        <el-input-number
          :disabled="!isEnabled"
          v-model="h"
          :min="0"
          :max="100"
          size="small"
          :controls="false"
          class="time"
        ></el-input-number>
        <a class="time-colon">:</a>
        <el-input-number
          :disabled="!isEnabled"
          v-model="m"
          :min="0"
          :controls="false"
          :max="59"
          size="small"
          class="time"
        ></el-input-number>
        <a class="time-colon"> :</a>
        <el-input-number
          :disabled="!isEnabled"
          v-model="s"
          :min="0"
          :controls="false"
          :precision="3"
          :max="59.999"
          size="small"
          class="time"
        ></el-input-number>
      </el-col>
    </el-row>
    <div v-if="error != null" style="color: red">
      {{ error }}
    </div>
  </div>
</template>
<script lang="ts">
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";
import { showError, jump, formatDateTime } from "../common";
export default Vue.component("time-input", {
  data() {
    return {
      h: 0,
      m: 0,
      s: 0.0,
      str: "",
      isEnabled: this.enabled,
      error: "",
    };
  },
  props: ["enabled", "label", "time"],
  computed: {},
  watch: {
    isEnabled() {
      this.$emit("update:enabled", this.isEnabled);
    },
    str() {
      this.parseTime();
    },
    h() {
      this.updateTime();
    },
    m() {
      this.updateTime();
    },
    s() {
      this.updateTime();
    },
  },
  created() {
    if (this.time) {
      this.h = Math.floor(this.time / 3600);
      this.m = Math.floor((this.time / 60) % 60);
      this.s = this.time - this.m * 60 - this.h * 3600;
    }
  },
  methods: {
    updateTime() {
      this.$emit("update:time", this.h * 3600 + this.m * 60 + this.s);
    },
    parseTime() {
      let parts: string[];
      let h: number;
      let m: number;
      let s: number;
      parts = this.str.replace("：", ":").split(":");

      if (parts.length == 1 || parts.length > 3) {
        this.error = "解析失败，无法识别时间部分";
        return;
      }
      const strS = parts[parts.length - 1];
      const strM = parts[parts.length - 2];
      const strH = parts.length == 3 ? parts[parts.length - 3] : "0";

      h = Number.parseInt(strH);
      m = Number.parseInt(strM);
      s = Number.parseFloat(strS);
      if (Number.isNaN(h) || Number.isNaN(m) || Number.isNaN(s)) {
        this.error = "解析失败，无法转为数字";
        return;
      }
      this.error = "";
      this.h = h;
      this.m = m;
      this.s = s;
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

.time-label {
  margin-left: 6px;
}
</style>