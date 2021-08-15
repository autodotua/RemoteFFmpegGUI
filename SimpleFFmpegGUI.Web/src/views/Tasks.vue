<template>
  <div>
    <div>
      <el-button v-if="isProcessing == false" type="primary" @click="start"
        >开始队列</el-button
      >
      <el-popconfirm
        v-if="isProcessing"
        title="真的要取消任务吗？"
        @onConfirm="cancel"
      >
        <el-button type="danger" slot="reference"
          >停止队列</el-button
        ></el-popconfirm
      >
      <el-button v-if="selection.length > 0" type="warn" @click="deleteTasks"
        >删除</el-button
      >
      <el-button v-if="selection.length > 0" @click="resetTasks" type="warn"
        >重置</el-button
      >
      <el-popconfirm
        title="真的要取消所选任务吗？正在执行的任务会被终止"
        @onConfirm="cancelTask(scope.row)"
      >
        <el-button
          v-if="selection.length > 0"
          type="warn"
          @click="cancelTasks"
          slot="reference"
          >取消</el-button
        ></el-popconfirm
      >
    </div>
    <el-table
      ref="table"
      :data="list"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="expand">
        <template slot-scope="props">
          <el-form label-position="left" label-width="120px">
            <el-form-item label="输入">
              <div v-for="file in props.row.inputs" :key="file">
                {{ file }}
              </div>
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
            <el-form-item label="参数"
              ><div style="white-space: pre-wrap">
                {{ JSON.stringify(props.row.arguments, null, 4) }}
              </div></el-form-item
            >
          </el-form>
        </template>
      </el-table-column>
      <el-table-column type="selection" width="55" />
      <el-table-column prop="typeText" label="类型" width="60" />
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
      <el-table-column prop="inputText" label="输入" min-width="360" />
      <!-- <el-table-column prop="output" label="输出" width="180" /> -->

      <el-table-column label="操作" width="140">
        <template slot-scope="scope">
          <el-button
            @click="resetTask(scope.row)"
            type="text"
            size="small"
            :disabled="scope.row.status == 1 || scope.row.status == 2"
            >重置</el-button
          >
          <el-popconfirm
            v-if="scope.row.status == 2"
            title="真的要取消任务吗？任务会终止"
            style="margin-left: 10px"
            @onConfirm="cancelTask(scope.row)"
          >
            <el-button
              slot="reference"
              type="text"
              size="small"
              :disabled="
                scope.row.status == 3 ||
                scope.row.status == 4 ||
                scope.row.status == 5
              "
              >取消</el-button
            ></el-popconfirm
          >
          <el-button
            v-else
            slot="reference"
            type="text"
            size="small"
            :disabled="
              scope.row.status == 3 ||
              scope.row.status == 4 ||
              scope.row.status == 5
            "
            @click="cancelTask(scope.row)"
            >取消</el-button
          >

          <el-button
            slot="reference"
            type="text"
            size="small"
            @click="remake(scope.row)"
            >重制</el-button
          >
        </template>
      </el-table-column>

      <el-table-column align="right">
        <template slot="header">
          <el-button type="text" @click="fillData()">刷新</el-button>
        </template>
      </el-table-column>
    </el-table>
    <div>
      <div class="top12">
      <el-pagination style="float:left" 
        @size-change="fillData"
        @current-change="fillData"
        layout="sizes,prev, pager, next"
        :page-sizes="[10, 20, 50, 100]"
        :page-size.sync="countPerPage"
        :current-page.sync="page"
        :total="totalCount"
      >
      </el-pagination>
        <el-radio-group v-model="statusFilter" size="small" @change="fillData" style="float:right">
      <el-radio-button :label="null"><b>全部</b></el-radio-button>
      <el-radio-button :label="1">排队中</el-radio-button>
      <el-radio-button :label="2">进行中</el-radio-button>
      <el-radio-button :label="3">已完成</el-radio-button>
      <el-radio-button :label="4">错误</el-radio-button>
      <el-radio-button :label="5">取消</el-radio-button>
    </el-radio-group></div>
    </div>
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
} from "../common";

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
      taskID: 0,
      pageCount: 1,
      page: 1,
      countPerPage: 10,
      statusFilter:null
    };
  },
  props: ["status"],
  watch: {
    status(value) {
      if (
        this.isProcessing != value.isProcessing ||
        this.taskID != (value.task == null ? 0 : value.task.id)
      ) {
        this.isProcessing = value.isProcessing;
        this.taskID = value.task == null ? 0 : value.task.id;
        this.fillData();
      }
    },
  },
  methods: {
    remake(item: any) {
      localStorage.setItem("codeArgs", JSON.stringify(item.arguments));
      jump("code");
    },
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
    deleteTasks() {
      net
        .postDeleteTasks(this.getSelectionIds())
        .then((r) => {
          this.fillData();
        })
        .catch(showError);
    },
    cancelTasks() {
      net
        .postCancelTasks(this.getSelectionIds())
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
      net
        .postCancelQueue()
        .then((r) => {
          this.isProcessing = false;
          this.fillData();
        })
        .catch(showError);
    },
    fillData() {
      let selection = this.selection;

      net
        .getTaskList(
          this.statusFilter,
          (this.page - 1) * this.countPerPage,
          this.countPerPage
        )
        .then((response) => {
          this.totalCount = response.data.totalCount;
          this.pageCount = Math.ceil(this.totalCount / this.countPerPage);
          response.data.list.forEach((element: any) => {
            switch (element.type) {
              case 0:
                element.typeText = "转码";
                break;
            }
            element.inputText =
              element.inputs.length == 1
                ? element.inputs[0]
                : element.inputs[0] + " 等";
          });
          this.list = response.data.list;
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
      }, 20000);
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
