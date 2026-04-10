<script setup>
import Header from "@/components/templates/Header.vue";
import Sidebar from "@/components/templates/SideBar/SideBar.vue";
import MainContent from "@/components/templates/MainContent.vue";
</script>
<template>
  <div class="flex">
    <Sidebar />
    <div class="w-full relative h-full font-montserrat">
      <Header />
      <MainContent>
        <router-view />
      </MainContent>
    </div>
  </div>
</template>

<style>
/* Chrome, Safari, Edge */
.no-spinner::-webkit-outer-spin-button,
.no-spinner::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

/* Firefox */
.no-spinner[type='number'] {
  -moz-appearance: textfield;
}
</style>