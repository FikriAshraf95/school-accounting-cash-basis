<script setup>
import { ref, computed } from "vue";
import { useSidebarStore } from "@/stores/sidebar";

const sidebar = useSidebarStore();

const miniSidebar = computed(() => sidebar.collapse);
</script>

<template>
  <div :class="[
    'transition-all duration-300',
    sidebar.collapse ? 'lg:pl-20' : 'lg:pl-72',
  ]">
    <main class="py-7 px-4 sm:px-6 lg:px-8 bg-neutral-200 overflow-auto h-full min-h-[calc(100vh-65px)]">
      <slot />
    </main>
  </div>
</template>
