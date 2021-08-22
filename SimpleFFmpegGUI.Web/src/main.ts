import Vue from 'vue'
import App from './App.vue'
import './registerServiceWorker'
import router from './router'
import ElementUI from 'element-ui';
import 'element-ui/lib/theme-chalk/index.css';
import axios from 'axios'
import VueAxios from 'vue-axios'
import './assets/global.css'

Vue.use(VueAxios, axios)
Vue.config.productionTip = false
Vue.use(ElementUI,{
  size:'small'
});
new Vue({
  router,
  render: h => h(App),
}).$mount('#app')
