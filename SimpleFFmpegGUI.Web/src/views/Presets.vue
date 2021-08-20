<template>
  <div>
    <el-table ref="table" :data="list">
      <el-table-column type="expand">
        <template slot-scope="props">
          <code-arguments-description :args="props.row.arguments" :type="props.row.type"></code-arguments-description>
        </template>
      </el-table-column>
      <el-table-column prop="name" label="预设名" min-width="80" />
      <el-table-column prop="typeText" label="类型" width="80" />

      <el-table-column label="操作" width="165">
        <template slot-scope="scope">
          <el-button slot="reference" type="text" @click="remake(scope.row)"
            >新建任务</el-button
          >
          <el-button type="text" slot="reference" @click="edit(scope.row)"
            >编辑</el-button
          >
          <el-popconfirm
            title="真的要删除预设吗？"
            style="margin-left: 8px"
            @onConfirm="deletePreset(scope.row)"
          >
            <el-button slot="reference" type="text"
              >删除</el-button
            ></el-popconfirm
          >
        </template>
      </el-table-column>

      <el-table-column align="right">
        <template slot="header">
          <el-button type="text" @click="fillData()">刷新</el-button>
        </template>
      </el-table-column>
    </el-table>
    <el-dialog title="编辑预设" :visible.sync="dialogVisible" width="80%" >
      <code-arguments    ref="args" :type="type" :showPresets="false" ></code-arguments>
      <span slot="footer" class="dialog-footer">
        <el-button
       
          type="primary"
          @click="savePreset"
          :loading="saving"
          >保存</el-button
        >
      </span>
    </el-dialog>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import {
  showError,
  showSuccess,
  formatDateTime,
  jump,
  getTaskTypeDescription,
  showLoading,
  closeLoading,
  jumpByArgs,
  stringType2Number,
} from "../common";

import * as net from "../net";
import { Notification, Table } from "element-ui";
import { ElTable } from "element-ui/types/table";
import CodeArguments from "@/components/CodeArguments.vue";
import CodeArgumentsDescription from "@/components/CodeArgumentsDescription.vue";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      list: [],
      dialogVisible: false,
      editingPreset: null,
      type: "",
      saving: false,
    };
  },
  props: [],
  watch: {},
  methods: {
    remake(item: any) {
      jumpByArgs(item.arguments, item.inputs, item.output, item.type);
    },
    savePreset() {
      this.saving = true;
      const item = this.editingPreset as any;
      net
        .postAddOrUpdatePreset(
          item.name,
          item.type,
          (this.$refs.args as any).getArgs()
        )
        .then((r) => {
          showSuccess("保存成功");
          this.dialogVisible = false;
          this.fillData();
        })
        .catch(showError)
        .finally(() => {
          this.saving = false;
        });
    },
    deletePreset(item: any) {
      net
        .postDeletePreset(item.id)
        .then((r) => {
          this.fillData();
        })
        .catch(showError);
    },
    edit(item: any) {
      this.editingPreset = item;
      this.type = item.type;
      this.dialogVisible = true;
    },
    fillData() {
      showLoading();
      return net
        .getPresets()
        .then((response) => {
          response.data.forEach((element: any) => {
            element.typeText = getTaskTypeDescription(element.type);
          });
          this.list = response.data;
        })
        .catch(showError)
        .finally(closeLoading);
    },
  },
  computed: {},
  mounted: function () {
    showLoading();
    this.$nextTick(function () {
      this.fillData();
    });
  },
  components: { CodeArguments, CodeArgumentsDescription },
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
