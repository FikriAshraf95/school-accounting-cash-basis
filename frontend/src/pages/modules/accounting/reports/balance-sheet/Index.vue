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

// Computed properties for Balance Sheet
const assetLedgers = computed(() => {
  if (!summaryData.value) return []
  return summaryData.value.ledgers.filter(l => l.type === 'asset')
})

const liabilityLedgers = computed(() => {
  if (!summaryData.value) return []
  return summaryData.value.ledgers.filter(l => l.type === 'liability')
})

const equityLedgers = computed(() => {
  if (!summaryData.value) return []
  return summaryData.value.ledgers.filter(l => l.type === 'equity')
})

const totalLiabilitiesAndEquity = computed(() => {
  if (!summaryData.value) return 0
  return summaryData.value.totalLiabilities + summaryData.value.totalEquity
})

const isBalanced = computed(() => {
  if (!summaryData.value) return true
  return Math.abs(summaryData.value.totalAssets - totalLiabilitiesAndEquity.value) < 0.01
})

onMounted(() => {
  sidebar.setPageName('balance_sheet_report')
  const currentYear = new Date().getFullYear()
  selectedYear.value = currentYear.toString()
  // fetchLedgerSummary()
})

async function fetchLedgerSummary() {
  try {
    isLoading.value = true
    error.value = null

    const year = parseInt(selectedYear.value)
    const response = await api.getLedgerSummary(year) as any
    summaryData.value = response
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load balance sheet'
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
        <h2 class="text-3xl font-bold tracking-tight">Balance Sheet</h2>
        <p class="text-muted-foreground">
          Financial position showing assets, liabilities, and equity at a point in time.
        </p>
      </div>
      <div class="flex items-center gap-2">
        <div v-if="!isLoading && summaryData" class="flex items-center gap-2">
          <span class="text-sm text-muted-foreground">Status:</span>
          <span 
            :class="isBalanced ? 'text-green-600' : 'text-red-600'"
            class="flex items-center gap-1 text-sm font-medium"
          >
            <iconify-icon 
              :icon="isBalanced ? 'lucide:check-circle' : 'lucide:alert-circle'" 
              class="h-4 w-4" 
            />
            {{ isBalanced ? 'Balanced' : 'Unbalanced' }}
          </span>
        </div>
        <Button variant="outline" @click="$router.push({ name: 'index_reports' })">
          <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
          Back
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
          <label class="text-sm font-medium">As of Year:</label>
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
    <div v-if="summaryData && !isLoading" class="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Assets</CardTitle>
          <iconify-icon icon="lucide:building-2" class="h-4 w-4 text-blue-500" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-blue-600">{{ formatAmount(summaryData.totalAssets) }}</div>
          <p class="text-xs text-muted-foreground">What we own</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Liabilities</CardTitle>
          <iconify-icon icon="lucide:credit-card" class="h-4 w-4 text-red-500" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-red-600">{{ formatAmount(summaryData.totalLiabilities) }}</div>
          <p class="text-xs text-muted-foreground">What we owe</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Equity</CardTitle>
          <iconify-icon icon="lucide:shield" class="h-4 w-4 text-green-500" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-green-600">{{ formatAmount(summaryData.totalEquity) }}</div>
          <p class="text-xs text-muted-foreground">Net worth</p>
        </CardContent>
      </Card>
    </div>

    <!-- Balance Sheet Tables -->
    <div class="grid gap-6">
      <!-- Assets Section -->
      <Card>
        <CardHeader class="bg-blue-50/50 dark:bg-blue-950/20">
          <CardTitle class="flex items-center gap-2 text-blue-700 dark:text-blue-400">
            <iconify-icon icon="lucide:building-2" class="h-5 w-5" />
            Assets
          </CardTitle>
          <CardDescription>Resources owned by the school</CardDescription>
        </CardHeader>
        <CardContent class="pt-6">
          <!-- Loading State -->
          <div v-if="isLoading" class="space-y-4">
            <Skeleton v-for="i in 4" :key="i" class="h-10 w-full" />
          </div>

          <!-- Empty State -->
          <div v-else-if="assetLedgers.length === 0" class="text-center py-8">
            <iconify-icon icon="lucide:inbox" class="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p class="text-sm text-muted-foreground">No asset accounts found</p>
          </div>

          <!-- Data Table -->
          <div v-else class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Account Name</TableHead>
                  <TableHead class="text-right">Closing Balance</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="ledger in assetLedgers" :key="ledger.ledgerId">
                  <TableCell class="font-medium">{{ ledger.ledgerCode }}</TableCell>
                  <TableCell>{{ ledger.ledgerName }}</TableCell>
                  <TableCell class="text-right">
                    {{ formatAmount(ledger.closingBalance) }}
                  </TableCell>
                </TableRow>
                <TableRow class="border-t-2 font-bold bg-muted/50">
                  <TableCell colspan="2">Total Assets</TableCell>
                  <TableCell class="text-right text-blue-600">
                    {{ formatAmount(summaryData?.totalAssets || 0) }}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <!-- Liabilities Section -->
      <Card>
        <CardHeader class="bg-red-50/50 dark:bg-red-950/20">
          <CardTitle class="flex items-center gap-2 text-red-700 dark:text-red-400">
            <iconify-icon icon="lucide:credit-card" class="h-5 w-5" />
            Liabilities
          </CardTitle>
          <CardDescription>Financial obligations and debts</CardDescription>
        </CardHeader>
        <CardContent class="pt-6">
          <!-- Loading State -->
          <div v-if="isLoading" class="space-y-4">
            <Skeleton v-for="i in 4" :key="i" class="h-10 w-full" />
          </div>

          <!-- Empty State -->
          <div v-else-if="liabilityLedgers.length === 0" class="text-center py-8">
            <iconify-icon icon="lucide:inbox" class="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p class="text-sm text-muted-foreground">No liability accounts found</p>
          </div>

          <!-- Data Table -->
          <div v-else class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Account Name</TableHead>
                  <TableHead class="text-right">Closing Balance</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="ledger in liabilityLedgers" :key="ledger.ledgerId">
                  <TableCell class="font-medium">{{ ledger.ledgerCode }}</TableCell>
                  <TableCell>{{ ledger.ledgerName }}</TableCell>
                  <TableCell class="text-right">
                    {{ formatAmount(ledger.closingBalance) }}
                  </TableCell>
                </TableRow>
                <TableRow class="border-t-2 font-bold bg-muted/50">
                  <TableCell colspan="2">Total Liabilities</TableCell>
                  <TableCell class="text-right text-red-600">
                    {{ formatAmount(summaryData?.totalLiabilities || 0) }}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <!-- Equity Section -->
      <Card>
        <CardHeader class="bg-green-50/50 dark:bg-green-950/20">
          <CardTitle class="flex items-center gap-2 text-green-700 dark:text-green-400">
            <iconify-icon icon="lucide:shield" class="h-5 w-5" />
            Equity
          </CardTitle>
          <CardDescription>Owner's equity and retained earnings</CardDescription>
        </CardHeader>
        <CardContent class="pt-6">
          <!-- Loading State -->
          <div v-if="isLoading" class="space-y-4">
            <Skeleton v-for="i in 4" :key="i" class="h-10 w-full" />
          </div>

          <!-- Empty State -->
          <div v-else-if="equityLedgers.length === 0" class="text-center py-8">
            <iconify-icon icon="lucide:inbox" class="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p class="text-sm text-muted-foreground">No equity accounts found</p>
          </div>

          <!-- Data Table -->
          <div v-else class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Code</TableHead>
                  <TableHead>Account Name</TableHead>
                  <TableHead class="text-right">Closing Balance</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="ledger in equityLedgers" :key="ledger.ledgerId">
                  <TableCell class="font-medium">{{ ledger.ledgerCode }}</TableCell>
                  <TableCell>{{ ledger.ledgerName }}</TableCell>
                  <TableCell class="text-right">
                    {{ formatAmount(ledger.closingBalance) }}
                  </TableCell>
                </TableRow>
                <TableRow class="border-t-2 font-bold bg-muted/50">
                  <TableCell colspan="2">Total Equity</TableCell>
                  <TableCell class="text-right text-green-600">
                    {{ formatAmount(summaryData?.totalEquity || 0) }}
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>

    <!-- Accounting Equation Summary -->
    <Card v-if="summaryData && !isLoading" :class="isBalanced ? 'border-green-500' : 'border-red-500'">
      <CardContent class="pt-6">
        <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <h3 class="text-lg font-semibold">Accounting Equation</h3>
            <p class="text-sm text-muted-foreground">
              Assets = Liabilities + Equity
            </p>
          </div>
          <div class="flex items-center gap-4 text-lg">
            <div class="text-center">
              <div class="font-bold text-blue-600">{{ formatAmount(summaryData.totalAssets) }}</div>
              <div class="text-xs text-muted-foreground">Assets</div>
            </div>
            <div class="text-muted-foreground">=</div>
            <div class="text-center">
              <div class="font-bold text-red-600">{{ formatAmount(summaryData.totalLiabilities) }}</div>
              <div class="text-xs text-muted-foreground">Liabilities</div>
            </div>
            <div class="text-muted-foreground">+</div>
            <div class="text-center">
              <div class="font-bold text-green-600">{{ formatAmount(summaryData.totalEquity) }}</div>
              <div class="text-xs text-muted-foreground">Equity</div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
