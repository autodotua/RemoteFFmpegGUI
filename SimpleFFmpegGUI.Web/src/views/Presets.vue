<template>
  <div>
    <el-table
      ref="table"
      :data="list"
    >
      <el-table-column type="expand">
        <template slot-scope="props">
          <div style="white-space: pre-wrap">
            {{ JSON.stringify(props.row.arguments, null, 4) }}
          </div>
        </template>
      </el-table-column>
      <el-table-column prop="name" label="预设名" min-width="80" />
      <el-table-column prop="typeText" label="类型" width="80" />

      <el-table-column label="操作" width="140">
        <template slot-scope="scope">
          <el-popconfirm
            title="真的要删除预设吗？"
            style="margin-left: 10px"
            @onConfirm="deletePreset(scope.row)"
          >
            <el-button slot="reference" type="text" size="small"
              >删除</el-button
            ></el-popconfirm
          >

          <el-button
            slot="reference"
            type="text"
            size="small"
            @click="remake(scope.row)"
            >新建任务</el-button
          >
        </template>
      </el-table-column>

      <el-table-column align="right">
        <template slot="header">
          <el-button type="text" @click="fillData()">刷新</el-button>
        </template>
      </el-table-column>
    </el-table>
    <div></div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import {
  withToken,
  showError,
  showSuccess,
  formatDateTime,
  jump,
  getTaskTypeDescription,
} from "../common";

import * as net from "../net";
import { Notification, Table } from "element-ui";
import { ElTable } from "element-ui/types/table";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      list: [],
    };
  },
  props: [],
  watch: {},
  methods: {
    remake(item: any) {
      localStorage.setItem("codeArgs", JSON.stringify(item.arguments));
      jump("code");
    },
    deletePreset(item: any) {
      net
        .postDeletePreset(item.id)
        .then((r) => {
          this.fillData();
        })
        .catch(showError);
    },

    fillData() {
      net
        .getPresets()
        .then((response) => {
          response.data.forEach((element: any) => {
            element.typeText = getTaskTypeDescription(element.type);
          });
          this.list = response.data;
        })
        .catch(showError);
    },
  },
  computed: {},
  mounted: function () {
    this.$nextTick(function () {
      this.fillData();
    });
  },
  components: {},
  // mounted: function() {
  //   this.$nextTick(function() {

  //   });
  // }
});
</script>

<style scoped>
.el-table .cell {
  white-space: pre-line;
  word-wrap: break-word;
}

.cell .el-button {
  margin-right: 6px;
}
</style>
