import './assets/index.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPersistedstate from 'pinia-plugin-persistedstate';

import globalComponents from '@/plugins/globalComponents';

import App from './App.vue'
import router from './router';

const pinia = createPinia();

createApp(App)
    .use(router)
    .use(pinia.use(piniaPersistedstate))
    .use(globalComponents)
    .mount('#app')

// // Register global components
// globalComponents(app);

// // Use the plugin
// pinia.use(piniaPersistedstate);

// app.use(router);
// app.use(pinia);
// app.mount('#app')
