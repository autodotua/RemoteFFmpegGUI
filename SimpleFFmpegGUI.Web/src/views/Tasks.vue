<template>
  <div>
    <div>
      <el-button v-if="isProcessing == false" type="primary" @click="start"
        >开始队列</el-button
      >
      <el-button v-if="isProcessing" @click="cancel" type="danger"
        >停止队列</el-button
      >
      <el-button v-if="selection.length > 0" type="warn">删除</el-button>
      <el-button v-if="selection.length > 0" @click="resetTasks" type="warn"
        >重置</el-button
      >
      <el-button v-if="selection.length > 0" type="warn">取消</el-button>
    </div>
    <el-table
      ref="table"
      :data="list"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="expand">
        <template slot-scope="props">
          <el-form label-position="left" class="demo-table-expand">
            <el-form-item label="输入"
              >{{ props.row.inputs.join("\n") }}
            </el-form-item>
            <el-form-item label="输出">{{ props.row.output }} </el-form-item>
            <el-form-item label="创建时间"
              >{{ props.row.createTime }}
            </el-form-item>
            <el-form-item label="开始时间"
              >{{ props.row.startTime }}
            </el-form-item>
            <el-form-item label="结束时间"
              >{{ props.row.finishTime }}
            </el-form-item>
            <el-form-item label="错误信息"
              >{{ props.row.message }}
            </el-form-item>
          </el-form>
        </template>
      </el-table-column>
      <el-table-column type="selection" width="55" />
      <el-table-column prop="type" label="类型" width="60" />
      <el-table-column label="状态" width="80">
        <template slot-scope="scope">
          <span style="" v-if="scope.row.status == 1">待处理</span>
          <span
            style="color: orange; font-weight: bold"
            v-if="scope.row.status == 2"
            >进行中</span
          >
          <span style="color: green" v-if="scope.row.status == 3">完成</span>
          <span style="color: red" v-if="scope.row.status == 4">错误</span>
          <span style="color: gray" v-if="scope.row.status == 5">取消</span>
        </template></el-table-column
      >
      <el-table-column prop="input" label="输入" width="180" />
      <el-table-column prop="output" label="输出" width="180" />

      <el-table-column label="操作" width="100">
        <template slot-scope="scope">
          <el-button
            @click="resetTask(scope.row)"
            type="text"
            size="small"
            :disabled="scope.row.status == 1 || scope.row.status == 2"
            >重置</el-button
          >
          <el-button
            type="text"
            size="small"
            @click="cancelTask(scope.row)"
            :disabled="
              scope.row.status == 3 ||
              scope.row.status == 4 ||
              scope.row.status == 5
            "
            >取消</el-button
          >
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { withToken, showError, showSuccess, formatDateTime } from "../common";

import * as net from "../net";
import { Notification, Table } from "element-ui";
import { ElTable } from "element-ui/types/table";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      list: [],
      isProcessing: false,
      totalCount: 0,
      selection: [],
    };
  },
  props: ["status"],
  watch: {
    status(value) {
      this.isProcessing = value.isProcessing;
    },
  },
  methods: {
    getSelectionIds(): number[] {
      return this.toIdList(this.selection as []);
    },
    toIdList(items: []): number[] {
      let list: number[] = [];
      items.forEach((item: any) => {
        list.push(item.id);
      });
      return list;
    },
    handleSelectionChange(val: any) {
      this.selection = val;
    },
    resetTask(item: any) {
      net
        .postResetTask(item.id)
        .then((r) => {
          this.fillData();
        })
        .catch(showError);
    },
    resetTasks() {
      net
        .postResetTasks(this.getSelectionIds())
        .then((r) => {
          this.fillData();
        })
        .catch(showError);
    },
    cancelTask(item: any) {
      net
        .postCancelTask(item.id)
        .then((r) => {
          this.fillData();
          console.log(r);
        })
        .catch(showError);
    },
    start() {
      net
        .postStartQueue()
        .then((r) => {
          this.isProcessing = true;
          this.fillData();
        })
        .catch(showError);
    },
    cancel() {
      this.$confirm("是否终止队列？", "提示", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning",
      }).then(() => {
        net
          .postCancelQueue()
          .then((r) => {
            this.isProcessing = false;
            this.fillData();
          })
          .catch(showError);
      });
    },
    fillData() {
      let selection = this.selection;

      net
        .getTaskList()
        .then((response) => {
          this.totalCount = response.data.totalCount;
          response.data.forEach((element: any) => {
            switch (element.type) {
              case 0:
                element.type = "转码";
                break;
            }
            element.input = element.inputs.join("、");
          });
          let ids = this.getSelectionIds();
          this.list = response.data;
          let table: any = this.$refs.table;
          // this.list.forEach((element: any) => {
          //   if (ids.indexOf((element as any).id) >= 0) {
          //     table.toggleRowSelection(element);
          //   }
          // });
        })
        .catch(showError);
    },
  },
  computed: {},
  mounted: function () {
    this.$nextTick(function () {
      this.fillData();
      setInterval(() => {
        if (this.selection.length == 0) {
          this.fillData();
        }
      }, 5000);
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
