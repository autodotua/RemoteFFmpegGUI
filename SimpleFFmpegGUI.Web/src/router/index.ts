import Vue from 'vue'
import VueRouter, { RouteConfig } from 'vue-router'
import Home from '../views/Home.vue'
import {TaskType} from '../common'
import Password from '../views/Tasks.vue'
import Welcome from '../views/Welcome.vue'
import MediaInfo from '../views/MediaInfo.vue'
import Code from '../views/Add/Code.vue'
import Combine from '../views/Add/Combine.vue'
import Compare from '../views/Add/Compare.vue'
import Custom from '../views/Add/Custom.vue'
import Tasks from '../views/Tasks.vue'
import Files from '../views/Files.vue'
import Presets from '../views/Presets.vue'
import Logs from '../views/Logs.vue'

Vue.use(VueRouter)

  const routes: Array<RouteConfig> = [
  {
    path: '/',
    name: 'welcome',
    component: Welcome
  },
  {
    path: '/info',
    name: 'MediaInfo',
    component: MediaInfo
  },
  {
    path: '/preset',
    name: 'Preset',
    component: Presets
  },
  {
    path: '/log',
    name: 'Logs',
    component: Logs
  },
  {
    path: '/file',
    name: 'File',
    component: Files
  },
  {
    path: '/tasks',
    name: 'Tasks',
    component: Tasks
  },
  {
    path: '/add/'+TaskType.GetByID(0).Route,
    name:TaskType.GetByID(0).Name,
    component: Code
  },
  {
    path: '/add/'+TaskType.GetByID(1).Route,
    name:TaskType.GetByID(1).Name,
    component: Combine
  },
  {
    path: '/add/'+TaskType.GetByID(2).Route,
    name: TaskType.GetByID(2).Name,
    component: Compare
  },
  {
    path: '/add/'+TaskType.GetByID(3).Route,
    name: TaskType.GetByID(3).Name,
    component: Custom
  },
  {
    path: '/password',
    name: 'Password',
    component: Password
  },
  {
    path: '/about',
    name: 'About',
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import(/* webpackChunkName: "about" */ '../views/About.vue')
  },
  {
    path: '/login',
    name: 'Login',
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import(/* webpackChunkName: "about" */ '../views/Login.vue')
  }
]

const router = new VueRouter({
  mode: 'hash',
  base: process.env.BASE_URL,
  routes
})

export default router
