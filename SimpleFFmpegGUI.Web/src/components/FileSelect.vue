
<template>
  <el-select v-model="file" @change="selectChanged" placeholder="请选择文件">
    <el-option v-for="item in files" :key="item" :label="item" :value="item">
    </el-option>
  </el-select>
</template>
<script >
import Vue from "vue";
import Cookies from "js-cookie";
import * as net from "../net";
import { withToken, showError, jump, formatDateTime } from "../common";
export default Vue.component("file-select", {
  data() {
    return {
      files: null,
      file:null
    };
  },
  props: {
   
  },
  computed: {},
  methods: {
    selectChanged(e) {
      this.$emit("select", e);
    },
  },
  components: {},
  mounted: function () {
    this.$nextTick(function () {
    net.getMediaNames()
        .then((response) => {
          this.files = response.data;
        })
        .catch(showError);
    });
  },
});
</script>
<style scoped>
</style>