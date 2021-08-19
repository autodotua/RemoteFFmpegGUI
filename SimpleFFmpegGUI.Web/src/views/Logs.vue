<template>
  <div>
    <a class="right24">时间范围：</a>
    <el-date-picker
      @change="fillData"
      v-model="timeRange"
      type="datetimerange"
      range-separator="至"
      start-placeholder="开始日期"
      end-placeholder="结束日期"
      align="right"
    >
    </el-date-picker>
    <el-table ref="table" :data="list" size="small">
      <el-table-column type="expand">
        <template slot-scope="props">
          <div class="pre-wrap">
            {{ props.row.message }}
          </div>
        </template>
      </el-table-column>

      <el-table-column prop="timeText" label="时间" width="180" />
      <el-table-column label="状态" width="80">
        <template slot-scope="scope">
          <span v-if="scope.row.type == 'E'" style="color: red">错误</span>
          <span style="color: orange" v-if="scope.row.type == 'W'">警告</span>
          <span v-if="scope.row.type == 'I'">信息</span>
          <span style="color: gray" v-if="scope.row.type == 'O'">输出</span>
        </template></el-table-column
      >
      <el-table-column label="信息" min-width="180">
        <template slot-scope="scope">
          <div class="single-line">{{ scope.row.message }}</div>
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
        <el-pagination
          style="float: left"
          @size-change="fillData"
          @current-change="fillData"
          layout="sizes,prev, pager, next"
          :page-sizes="[10, 20, 50, 100, 200, 500, 1000]"
          :page-size.sync="countPerPage"
          :current-page.sync="page"
          :total="totalCount"
        >
        </el-pagination>
        <el-radio-group
          v-model="typeFilter"
          size="mini"
          @change="fillData"
          style="float: right"
        >
          <el-radio-button :label="null"><b>全部</b></el-radio-button>
          <el-radio-button label="E">错误</el-radio-button>
          <el-radio-button label="W">警告</el-radio-button>
          <el-radio-button label="I">信息</el-radio-button>
          <el-radio-button label="O">输出</el-radio-button>
        </el-radio-group>
      </div>
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
  getTaskTypeDescription,
  showLoading,
  closeLoading,
} from "../common";

import * as net from "../net";
import { Notification, Table } from "element-ui";
import { ElTable } from "element-ui/types/table";
export default Vue.extend({
  name: "Home",
  data() {
    return {
      list: [],
      totalCount: 0,
      pageCount: 1,
      page: 1,
      countPerPage: 10,
      typeFilter: null,
      timeRange: [],
    };
  },
  methods: {
    fillData() {
      showLoading();
      const from =
        this.timeRange && this.timeRange.length == 2
          ? (this.timeRange[0] as Date).toJSON()
          : null;
      const to =
        this.timeRange && this.timeRange.length == 2
          ? (this.timeRange[1] as Date).toJSON()
          : null;

      return net
        .getLogs(
          this.typeFilter,
          from,
          to,
          (this.page - 1) * this.countPerPage,
          this.countPerPage
        )
        .then((response) => {
          this.totalCount = response.data.totalCount;
          this.pageCount = Math.ceil(this.totalCount / this.countPerPage);
          response.data.list.forEach((element: any) => {
            element.timeText = formatDateTime(new Date(element.time));
          });
          this.list = response.data.list;
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
  components: {},
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
