<template>
    <Icon :icon="icon" v-bind="props" />
  </template>
  
  <script setup>
  import { Icon } from '@iconify/vue';
  import { defineProps, toRefs } from 'vue';
  
  // Define props to accept all attributes
  const props = defineProps({
    icon: {
      type: String,
      required: true
    },
    width: {
      type: [String, Number],
      default: null
    },
    height: {
      type: [String, Number],
      default: null
    },
    rotate: {
      type: [String, Number],
      default: null
    },
    // Add other attributes as needed
  });
  
  // Spread the props to pass them all to the Icon component
  const { icon, ...restProps } = toRefs(props);
  </script>
  