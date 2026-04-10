<script setup>
import MainContent from "@/components/templates/MainContent.vue";
</script>
<template>
  <div class="flex">
    <div class="flex-1 bg-neutral-200 h-screen">
      <MainContent>
        <router-view />
      </MainContent>
    </div>
  </div>
</template>