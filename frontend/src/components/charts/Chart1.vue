<script lang="ts" setup>
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
} from "chart.js";
import { Bar } from "vue-chartjs";
import { Button } from "@/components/ui/button";
import { FileUp } from "lucide-vue-next";

const data = {
  labels: [
    "Jan 20",
    "Feb 20",
    "Mar 20",
    "Apr 20",
    "Jun 20",
    "Jun 20",
    "Jul 20",
    "Aug 20",
    "Sep 20",
    "Oct 20",
    "Nov 20",
    "Dec 20",
  ],
  datasets: [
    {
      label: "Invoice Requests",
      backgroundColor: "#878787",
      data: [40, 20, 12, 39, 10, 40, 39, 80, 40, 20, 12, 11],
    },
    {
      label: "Invoices Issued",
      backgroundColor: "#1E1E1E",
      data: [40, 20, 12, 39, 10, 40, 39, 80, 40, 20, 12, 11],
    },
  ],
};

const options = {
  responsive: true,
  maintainAspectRatio: false,
  borderRadius: 15,
  scales: {
    x: {
      grid: {
        offset: true,
      },
    },
  },
};

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);
</script>

<template>
  <div
    class="overflow-hidden rounded-lg bg-white shadow px-4 py-5 sm:p-6 min-w-[600px] flex flex-col"
  >
    <div class="flex justify-between">
      <p class="font-bold">Report Type 1</p>
      <Button variant="outline"><FileUp class="w-4 h-auto" />Export</Button>
    </div>
    <div class="flex-1">
      <Bar :data="data" :options="options" />
    </div>
  </div>
</template>
