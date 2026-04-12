<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

const sidebar = useSidebarStore()
const router = useRouter()

interface ClosingEntry {
  ledgerCode: string
  ledgerName: string
  entryType: 'debit' | 'credit'
  amount: number
}

interface YearEndCloseResult {
  year: number
  netRevenue: number
  netExpense: number
  netProfit: number
  closingEntries: ClosingEntry[]
  closedAt: string
}

const yearToClose = ref<number>(new Date().getFullYear())
const isClosing = ref(false)
const closeResult = ref<YearEndCloseResult | null>(null)
const error = ref<string | null>(null)

const canClose = computed(() => {
  return yearToClose.value > 2000 && yearToClose.value <= new Date().getFullYear()
})

onMounted(() => {
  sidebar.setPageName('close_ledger')
  // Default to previous year if we're early in the current year
  const currentMonth = new Date().getMonth()
  if (currentMonth < 3) {
    yearToClose.value = new Date().getFullYear() - 1
  }
})

async function performYearEndClose() {
  try {
    isClosing.value = true
    error.value = null

    const response = await api.yearEndClose({ year: yearToClose.value }) as any
    closeResult.value = response
    toast.success('Year-End Close Completed', {
      description: `Fiscal year ${yearToClose.value} has been successfully closed.`,
    })
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to perform year-end close'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isClosing.value = false
  }
}

function formatAmount(amount: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(amount)
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleString('en-MY', {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Year-End Close</h2>
        <p class="text-muted-foreground">
          Close the fiscal year and transfer balances to retained earnings.
        </p>
      </div>
      <Button variant="outline" @click="router.push({ name: 'ledger' })">
        <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
        Back to Ledgers
      </Button>
    </div>

    <!-- Warning Alert -->
    <Alert variant="destructive" class="border-amber-500 bg-amber-50 dark:bg-amber-950/20">
      <iconify-icon icon="lucide:alert-triangle" class="h-4 w-4" />
      <AlertTitle>Important Notice</AlertTitle>
      <AlertDescription>
        Year-end closing is a critical operation that cannot be undone. It will close all revenue and expense accounts,
        transferring their balances to retained earnings. Please ensure all transactions for the year are complete before proceeding.
      </AlertDescription>
    </Alert>

    <!-- Error Alert -->
    <Alert v-if="error" variant="destructive">
      <iconify-icon icon="lucide:alert-circle" class="h-4 w-4" />
      <AlertTitle>Error</AlertTitle>
      <AlertDescription>{{ error }}</AlertDescription>
    </Alert>

    <!-- Year Selection Card -->
    <Card v-if="!closeResult">
      <CardHeader>
        <CardTitle>Select Fiscal Year to Close</CardTitle>
        <CardDescription>
          Enter the fiscal year you want to close. This should be a completed fiscal year.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div class="flex flex-col gap-6 sm:flex-row sm:items-end">
          <div class="flex-1">
            <label class="text-sm font-medium mb-2 block">Fiscal Year</label>
            <Input
              v-model="yearToClose"
              type="number"
              :min="2000"
              :max="new Date().getFullYear()"
              class="w-full sm:w-48"
              placeholder="Enter year (e.g., 2025)"
            />
          </div>
          
          <AlertDialog>
            <AlertDialogTrigger as-child>
              <Button 
                :disabled="!canClose || isClosing"
                variant="destructive"
                class="w-full sm:w-auto"
              >
                <iconify-icon icon="lucide:lock" class="mr-2 h-4 w-4" />
                <span v-if="isClosing">Closing...</span>
                <span v-else>Close Year</span>
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                <AlertDialogDescription class="space-y-2">
                  <p>
                    This action will close fiscal year <strong>{{ yearToClose }}</strong> and cannot be undone.
                  </p>
                  <p>
                    All revenue and expense accounts will be reset to zero, and their net balance will be transferred to retained earnings.
                  </p>
                  <p class="text-amber-600 font-medium">
                    Please confirm that all transactions for {{ yearToClose }} have been recorded.
                  </p>
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction 
                  @click="performYearEndClose"
                  class="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                >
                  Yes, Close Year {{ yearToClose }}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </CardContent>
    </Card>

    <!-- Success Result -->
    <div v-if="closeResult" class="space-y-6">
      <!-- Success Alert -->
      <Alert class="border-green-500 bg-green-50 dark:bg-green-950/20">
        <iconify-icon icon="lucide:check-circle" class="h-4 w-4 text-green-600" />
        <AlertTitle class="text-green-800 dark:text-green-400">Year-End Close Successful</AlertTitle>
        <AlertDescription class="text-green-700 dark:text-green-500">
          Fiscal year {{ closeResult.year }} has been successfully closed on {{ formatDate(closeResult.closedAt) }}.
        </AlertDescription>
      </Alert>

      <!-- Summary Cards -->
      <div class="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">Total Revenue</CardTitle>
            <iconify-icon icon="lucide:trending-up" class="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold text-green-600">{{ formatAmount(closeResult.netRevenue) }}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">Total Expenses</CardTitle>
            <iconify-icon icon="lucide:trending-down" class="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold text-red-600">{{ formatAmount(closeResult.netExpense) }}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">Net Profit</CardTitle>
            <iconify-icon icon="lucide:coins" class="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold text-blue-600">{{ formatAmount(closeResult.netProfit) }}</div>
            <p class="text-xs text-muted-foreground">Transferred to retained earnings</p>
          </CardContent>
        </Card>
      </div>

      <!-- Closing Entries Table -->
      <Card>
        <CardHeader>
          <CardTitle>Closing Entries</CardTitle>
          <CardDescription>
            Journal entries created during the year-end closing process
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Ledger Code</TableHead>
                  <TableHead>Account Name</TableHead>
                  <TableHead>Entry Type</TableHead>
                  <TableHead class="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="(entry, index) in closeResult.closingEntries" :key="index">
                  <TableCell class="font-medium">{{ entry.ledgerCode }}</TableCell>
                  <TableCell>{{ entry.ledgerName }}</TableCell>
                  <TableCell>
                    <span 
                      :class="entry.entryType === 'debit' ? 'text-red-600' : 'text-green-600'"
                      class="font-medium capitalize"
                    >
                      {{ entry.entryType }}
                    </span>
                  </TableCell>
                  <TableCell class="text-right">{{ formatAmount(entry.amount) }}</TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <!-- Actions -->
      <div class="flex gap-4">
        <Button @click="closeResult = null; error = null">
          <iconify-icon icon="lucide:rotate-ccw" class="mr-2 h-4 w-4" />
          Close Another Year
        </Button>
        <Button variant="outline" @click="router.push({ name: 'open_ledger' })">
          <iconify-icon icon="lucide:calendar-plus" class="mr-2 h-4 w-4" />
          Open New Year
        </Button>
      </div>
    </div>
  </div>
</template>
