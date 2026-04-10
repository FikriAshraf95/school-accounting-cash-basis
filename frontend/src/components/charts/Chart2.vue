<script lang="ts" setup>
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from "chart.js";
import { ChevronRight } from "lucide-vue-next";
import { Doughnut } from "vue-chartjs";

const data = {
  labels: ["Product 1", "Product 2", "Product 3", "Product 4"],
  datasets: [
    {
      borderWidth: 0,
      weight: 50,
      backgroundColor: ["#CBCBCB", "#979797", "#5A5A5A", "#202020"],
      data: [50, 25, 15, 10],
    },
  ],
};

const options = {
  responsive: true,
  maintainAspectRatio: false,
  radius: 100,
  cutout: 105,
};

ChartJS.register(ArcElement, Tooltip, Legend);
</script>

<template>
  <div
    class="overflow-hidden rounded-lg bg-white shadow px-4 py-5 sm:p-6 flex flex-col"
  >
    <p class="font-bold">Report Type 1</p>
    <div class="flex-1">
      <Doughnut :data="data" :options="options" />
    </div>
    <hr class="border-t border-gray-300 mb-4" />
    
      More Insights <ChevronRight class="inline w-4" />

  </div>
</template>
