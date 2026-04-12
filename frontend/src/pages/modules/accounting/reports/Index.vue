<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSidebarStore } from '@/stores/sidebar'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'

const sidebar = useSidebarStore()

interface ReportCard {
  title: string
  description: string
  icon: string
  routeName: string
  color: string
}

const reports = ref<ReportCard[]>([
  {
    title: 'Trial Balance',
    description: 'View all ledger accounts with debit and credit balances to verify accounting equation.',
    icon: 'lucide:scale',
    routeName: 'trial_balance_report',
    color: 'bg-blue-500/10 text-blue-500',
  },
  {
    title: 'Profit & Loss',
    description: 'Income and expense summary showing net profit or loss for a specific period.',
    icon: 'lucide:trending-up',
    routeName: 'profit_loss_report',
    color: 'bg-green-500/10 text-green-500',
  },
  {
    title: 'Balance Sheet',
    description: 'Financial position showing assets, liabilities, and equity at a point in time.',
    icon: 'lucide:sheet',
    routeName: 'balance_sheet_report',
    color: 'bg-purple-500/10 text-purple-500',
  },
  {
    title: 'Student Report',
    description: 'View student transaction summaries and balances by grade or class.',
    icon: 'lucide:users',
    routeName: 'students_reports',
    color: 'bg-orange-500/10 text-orange-500',
  },
])

onMounted(() => {
  sidebar.setPageName('index_reports')
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div>
      <h2 class="text-3xl font-bold tracking-tight">Financial Reports</h2>
      <p class="text-muted-foreground">
        Generate and view standard accounting reports for your school.
      </p>
    </div>

    <!-- Reports Grid -->
    <div class="grid gap-6 md:grid-cols-2">
      <Card
        v-for="report in reports"
        :key="report.routeName"
        class="transition-all hover:shadow-md cursor-pointer"
        @click="$router.push({ name: report.routeName })"
      >
        <CardHeader>
          <div class="flex items-start justify-between">
            <div class="flex items-center gap-4">
              <div :class="['p-3 rounded-lg', report.color]">
                <iconify-icon :icon="report.icon" class="h-6 w-6" />
              </div>
              <div>
                <CardTitle>{{ report.title }}</CardTitle>
                <CardDescription class="mt-1">
                  {{ report.description }}
                </CardDescription>
              </div>
            </div>
            <iconify-icon 
              icon="lucide:chevron-right" 
              class="h-5 w-5 text-muted-foreground" 
            />
          </div>
        </CardHeader>
        <CardContent>
          <Button variant="ghost" class="w-full" @click.stop="$router.push({ name: report.routeName })">
            View Report
            <iconify-icon icon="lucide:arrow-right" class="ml-2 h-4 w-4" />
          </Button>
        </CardContent>
      </Card>
    </div>

    <!-- Quick Info -->
    <Card class="bg-muted/50">
      <CardHeader>
        <CardTitle class="text-lg">About Financial Reports</CardTitle>
      </CardHeader>
      <CardContent class="space-y-4 text-sm text-muted-foreground">
        <p>
          <strong class="text-foreground">Trial Balance:</strong> Lists all general ledger accounts and their balances at a specific point in time. The total debits must equal total credits.
        </p>
        <p>
          <strong class="text-foreground">Profit & Loss:</strong> Also known as Income Statement, shows revenues, expenses, and net income/loss over a period of time.
        </p>
        <p>
          <strong class="text-foreground">Balance Sheet:</strong> Provides a snapshot of the school's financial position, showing what is owned (assets), what is owed (liabilities), and the net worth (equity).
        </p>
      </CardContent>
    </Card>
  </div>
</template>
