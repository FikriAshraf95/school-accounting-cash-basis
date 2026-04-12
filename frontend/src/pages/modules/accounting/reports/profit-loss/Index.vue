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

interface LedgerSummary {
  ledgerId: number
  ledgerCode: string
  ledgerName: string
  type: string
  openingBalance: number
  totalDebits: number
  totalCredits: number
  netChange: number
  closingBalance: number
}

interface LedgerSummaryData {
  year: number
  ledgers: LedgerSummary[]
  totalAssets: number
  totalLiabilities: number
  totalEquity: number
  totalRevenue: number
  totalExpenses: number
}

const summaryData = ref<LedgerSummaryData | null>(null)
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

// Computed properties for P&L
const revenueLedgers = computed(() => {
  if (!summaryData.value) return []
  return summaryData.value.ledgers.filter(l => l.type === 'revenue')
})

const expenseLedgers = computed(() => {
  if (!summaryData.value) return []
  return summaryData.value.ledgers.filter(l => l.type === 'expense')
})

const netProfit = computed(() => {
  if (!summaryData.value) return 0
  return summaryData.value.totalRevenue - summaryData.value.totalExpenses
})

const isProfit = computed(() => netProfit.value >= 0)

onMounted(() => {
  sidebar.setPageName('profit_loss_report')
  const currentYear = new Date().getFullYear()
  selectedYear.value = currentYear.toString()
  fetchLedgerSummary()
})

async function fetchLedgerSummary() {
  try {
    isLoading.value = true
    error.value = null

    const year = parseInt(selectedYear.value)
    const response = await api.getLedgerSummary(year) as any
    summaryData.value = response.data
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load profit and loss report'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

watch(selectedYear, () => {
  fetchLedgerSummary()
})

function formatAmount(amount: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(amount)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Profit & Loss Statement</h2>
        <p class="text-muted-foreground">
          Income and expenses summary showing net profit or loss for the period.
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
      </CardContent>
    </Card>

    <!-- Summary Cards -->
    <div v-if="summaryData && !isLoading" class="grid gap-4 md:grid-cols-4">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Revenue</CardTitle>
          <iconify-icon icon="lucide:trending-up" class="h-4 w-4 text-green-500" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-green-600">{{ formatAmount(summaryData.totalRevenue) }}</div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Expenses</CardTitle>
          <iconify-icon icon="lucide:trending-down" class="h-4 w-4 text-red-500" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-red-600">{{ formatAmount(summaryData.totalExpenses) }}</div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Net Result</CardTitle>
          <iconify-icon 
            :icon="isProfit ? 'lucide:plus-circle' : 'lucide:minus-circle'" 
            :class="isProfit ? 'text-green-500' : 'text-red-500'"
            class="h-4 w-4" 
          />
        </CardHeader>
        <CardContent>
          <div 
            class="text-2xl font-bold"
            :class="isProfit ? 'text-green-600' : 'text-red-600'"
          >
            {{ formatAmount(Math.abs(netProfit)) }}
          </div>
          <p class="text-xs text-muted-foreground">
            {{ isProfit ? 'Net Profit' : 'Net Loss' }}
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Profit Margin</CardTitle>
          <iconify-icon icon="lucide:percent" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            {{ summaryData.totalRevenue > 0 
              ? ((netProfit / summaryData.totalRevenue) * 100).toFixed(1) + '%' 
              : '0%' }}
          </div>
          <p class="text-xs text-muted-foreground">Of total revenue</p>
        </CardContent>
      </Card>
    </div>

    <!-- Revenue & Expense Tables -->
    <div class="grid gap-6 lg:grid-cols-2">
      <!-- Revenue Section -->
      <Card>
        <CardHeader>
          <CardTitle class="flex items-center gap-2">
            <iconify-icon icon="lucide:arrow-down-to-line" class="h-5 w-5 text-green-500" />
            Revenue
          </CardTitle>
          <CardDescription>Income accounts</CardDescription>
        </CardHeader>
        <CardContent>
          <!-- Loading State -->
          <div v-if="isLoading" class="space-y-4">
            <Skeleton v-for="i in 4" :key="i" class="h-10 w-full" />
          </div>

          <!-- Empty State -->
          <div v-else-if="revenueLedgers.length === 0" class="text-center py-8">
            <iconify-icon icon="lucide:inbox" class="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p class="text-sm text-muted-foreground">No revenue accounts found</p>
          </div>

          <!-- Data Table -->
          <div v-else class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Account</TableHead>
                  <TableHead class="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="ledger in revenueLedgers" :key="ledger.ledgerId">
                  <TableCell>
                    <div class="font-medium">{{ ledger.ledgerName }}</div>
                    <div class="text-xs text-muted-foreground">{{ ledger.ledgerCode }}</div>
                  </TableCell>
                  <TableCell class="text-right text-green-600">
                    {{ formatAmount(Math.abs(ledger.netChange)) }}
                  </TableCell>
                </TableRow>
                <TableRow class="border-t-2 font-bold bg-muted/50">
                  <TableCell>Total Revenue</TableCell>
                  <TableCell class="text-right text-green-600">
                    {{ formatAmount(summaryData?.totalRevenue || 0) }}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <!-- Expense Section -->
      <Card>
        <CardHeader>
          <CardTitle class="flex items-center gap-2">
            <iconify-icon icon="lucide:arrow-up-from-line" class="h-5 w-5 text-red-500" />
            Expenses
          </CardTitle>
          <CardDescription>Expense accounts</CardDescription>
        </CardHeader>
        <CardContent>
          <!-- Loading State -->
          <div v-if="isLoading" class="space-y-4">
            <Skeleton v-for="i in 4" :key="i" class="h-10 w-full" />
          </div>

          <!-- Empty State -->
          <div v-else-if="expenseLedgers.length === 0" class="text-center py-8">
            <iconify-icon icon="lucide:inbox" class="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p class="text-sm text-muted-foreground">No expense accounts found</p>
          </div>

          <!-- Data Table -->
          <div v-else class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Account</TableHead>
                  <TableHead class="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="ledger in expenseLedgers" :key="ledger.ledgerId">
                  <TableCell>
                    <div class="font-medium">{{ ledger.ledgerName }}</div>
                    <div class="text-xs text-muted-foreground">{{ ledger.ledgerCode }}</div>
                  </TableCell>
                  <TableCell class="text-right text-red-600">
                    {{ formatAmount(Math.abs(ledger.netChange)) }}
                  </TableCell>
                </TableRow>
                <TableRow class="border-t-2 font-bold bg-muted/50">
                  <TableCell>Total Expenses</TableCell>
                  <TableCell class="text-right text-red-600">
                    {{ formatAmount(summaryData?.totalExpenses || 0) }}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>

    <!-- Net Result Summary -->
    <Card v-if="summaryData && !isLoading">
      <CardContent class="pt-6">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-lg font-semibold">
              Net {{ isProfit ? 'Profit' : 'Loss' }}
            </h3>
            <p class="text-sm text-muted-foreground">
              For fiscal year {{ selectedYear }}
            </p>
          </div>
          <div 
            class="text-3xl font-bold"
            :class="isProfit ? 'text-green-600' : 'text-red-600'"
          >
            {{ isProfit ? '+' : '-' }}{{ formatAmount(Math.abs(netProfit)) }}
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
