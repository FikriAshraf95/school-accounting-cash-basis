<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { isCancel } from '@/services/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

const sidebar = useSidebarStore()

interface TrialBalanceItem {
  ledgerId: number
  ledgerCode: string
  ledgerName: string
  type: string
  debitAmount: number
  creditAmount: number
}

interface TrialBalanceData {
  items: TrialBalanceItem[]
  totalDebits: number
  totalCredits: number
  isBalanced: boolean
}

const trialBalance = ref<TrialBalanceData | null>(null)
const isLoading = ref(true)
const error = ref<string | null>(null)
const selectedYear = ref<string>('')

// Generate year options (current year and 5 years back)
const yearOptions = computed(() => {
  const currentYear = new Date().getFullYear()
  const years = []
  for (let i = 0; i < 6; i++) {
    years.push((currentYear - i).toString())
  }
  return years
})

onMounted(() => {
  sidebar.setPageName('trial_balance_report')
  const currentYear = new Date().getFullYear()
  selectedYear.value = currentYear.toString()
  fetchTrialBalance()
})

async function fetchTrialBalance() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {}
    if (selectedYear.value) {
      params.year = parseInt(selectedYear.value)
    }

    const response = await api.getTrialBalance(params) as any
    trialBalance.value = response
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load trial balance'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

watch(selectedYear, () => {
  fetchTrialBalance()
})

function formatAmount(amount: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(amount)
}

function getTypeBadgeColor(type: string): any {
  const typeMap: Record<string, any> = {
    asset: 'default',
    liability: 'secondary',
    equity: 'outline',
    revenue: 'default',
    expense: 'destructive',
  }
  return typeMap[type] || 'default'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Trial Balance</h2>
        <p class="text-muted-foreground">
          Verify that total debits equal total credits across all ledger accounts.
        </p>
      </div>
      <div class="flex gap-2">
        <Button variant="outline" @click="$router.push({ name: 'index_reports' })">
          <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
          Back to Reports
        </Button>
      </div>
    </div>

    <!-- Error Alert -->
    <Alert v-if="error" variant="destructive">
      <iconify-icon icon="lucide:alert-circle" class="h-4 w-4" />
      <AlertTitle>Error</AlertTitle>
      <AlertDescription>{{ error }}</AlertDescription>
    </Alert>

    <!-- Filters -->
    <Card>
      <CardContent class="pt-6">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-center">
          <div class="flex items-center gap-4">
            <label class="text-sm font-medium">Fiscal Year:</label>
            <Select v-model="selectedYear">
              <SelectTrigger class="w-40">
                <SelectValue placeholder="Select year" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="year in yearOptions" :key="year" :value="year">
                  {{ year }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
          
          <!-- Balance Status -->
          <div v-if="trialBalance" class="flex items-center gap-2 ml-auto">
            <span class="text-sm text-muted-foreground">Status:</span>
            <Badge :variant="trialBalance.isBalanced ? 'default' : 'destructive'">
              <iconify-icon 
                :icon="trialBalance.isBalanced ? 'lucide:check-circle' : 'lucide:x-circle'" 
                class="mr-1 h-3 w-3" 
              />
              {{ trialBalance.isBalanced ? 'Balanced' : 'Unbalanced' }}
            </Badge>
          </div>
        </div>
      </CardContent>
    </Card>

    <!-- Summary Cards -->
    <div v-if="trialBalance && !isLoading" class="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Debits</CardTitle>
          <iconify-icon icon="lucide:arrow-down-left" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ formatAmount(trialBalance.totalDebits) }}</div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Credits</CardTitle>
          <iconify-icon icon="lucide:arrow-up-right" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ formatAmount(trialBalance.totalCredits) }}</div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Difference</CardTitle>
          <iconify-icon icon="lucide:equal" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div 
            class="text-2xl font-bold"
            :class="trialBalance.isBalanced ? 'text-green-600' : 'text-red-600'"
          >
            {{ formatAmount(Math.abs(trialBalance.totalDebits - trialBalance.totalCredits)) }}
          </div>
        </CardContent>
      </Card>
    </div>

    <!-- Trial Balance Table -->
    <Card>
      <CardHeader>
        <CardTitle>Account Balances</CardTitle>
        <CardDescription>
          All ledger accounts with their respective debit and credit balances
        </CardDescription>
      </CardHeader>
      <CardContent>
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 8" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="!trialBalance?.items?.length" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:scale" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No data found</h3>
          <p class="text-sm text-muted-foreground">No ledger accounts found for the selected year.</p>
        </div>

        <!-- Data Table -->
        <div v-else class="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Code</TableHead>
                <TableHead>Account Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead class="text-right">Debit</TableHead>
                <TableHead class="text-right">Credit</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="item in trialBalance.items" :key="item.ledgerId">
                <TableCell class="font-medium">{{ item.ledgerCode }}</TableCell>
                <TableCell>{{ item.ledgerName }}</TableCell>
                <TableCell>
                  <Badge :variant="getTypeBadgeColor(item.type)">
                    {{ item.type.charAt(0).toUpperCase() + item.type.slice(1) }}
                  </Badge>
                </TableCell>
                <TableCell class="text-right">
                  <span v-if="item.debitAmount > 0" class="text-red-600">
                    {{ formatAmount(item.debitAmount) }}
                  </span>
                  <span v-else class="text-muted-foreground">-</span>
                </TableCell>
                <TableCell class="text-right">
                  <span v-if="item.creditAmount > 0" class="text-green-600">
                    {{ formatAmount(item.creditAmount) }}
                  </span>
                  <span v-else class="text-muted-foreground">-</span>
                </TableCell>
              </TableRow>
              
              <!-- Total Row -->
              <TableRow class="border-t-2 font-bold bg-muted/50">
                <TableCell colspan="3" class="text-right">Total</TableCell>
                <TableCell class="text-right text-red-600">
                  {{ formatAmount(trialBalance.totalDebits) }}
                </TableCell>
                <TableCell class="text-right text-green-600">
                  {{ formatAmount(trialBalance.totalCredits) }}
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
