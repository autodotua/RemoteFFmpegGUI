import Vue from 'vue'
import VueRouter, { RouteConfig } from 'vue-router'
import Home from '../views/Home.vue'
import Password from '../views/Tasks.vue'
import Welcome from '../views/Welcome.vue'
import MediaInfo from '../views/MediaInfo.vue'
import Code from '../views/Code.vue'
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
    path: '/code',
    name: 'Code',
    component: Code
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
