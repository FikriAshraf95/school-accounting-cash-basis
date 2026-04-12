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
import { Badge } from '@/components/ui/badge'
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

interface OpeningEntry {
  ledgerCode: string
  ledgerName: string
  type: string
  entryType: 'debit' | 'credit'
  amount: number
}

interface YearBeginningOpenResult {
  year: number
  totalOpeningDebits: number
  totalOpeningCredits: number
  isBalanced: boolean
  openingEntries: OpeningEntry[]
  openedAt: string
}

const yearToOpen = ref<number>(new Date().getFullYear())
const isOpening = ref(false)
const openResult = ref<YearBeginningOpenResult | null>(null)
const error = ref<string | null>(null)

const canOpen = computed(() => {
  return yearToOpen.value >= new Date().getFullYear() && yearToOpen.value <= new Date().getFullYear() + 1
})

onMounted(() => {
  sidebar.setPageName('open_ledger')
})

async function performYearBeginningOpen() {
  try {
    isOpening.value = true
    error.value = null

    const response = await api.yearBeginningOpen({ year: yearToOpen.value }) as any
    openResult.value = response.data
    toast.success('Year-Beginning Open Completed', {
      description: `Fiscal year ${yearToOpen.value} has been successfully opened.`,
    })
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to perform year-beginning open'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isOpening.value = false
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
        <h2 class="text-3xl font-bold tracking-tight">Year-Beginning Open</h2>
        <p class="text-muted-foreground">
          Open a new fiscal year by carrying forward opening balances from the previous year.
        </p>
      </div>
      <Button variant="outline" @click="router.push({ name: 'ledger' })">
        <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
        Back to Ledgers
      </Button>
    </div>

    <!-- Info Alert -->
    <Alert class="border-blue-500 bg-blue-50 dark:bg-blue-950/20">
      <iconify-icon icon="lucide:info" class="h-4 w-4" />
      <AlertTitle>Information</AlertTitle>
      <AlertDescription>
        Opening a new fiscal year will create opening balance entries for all asset, liability, and equity accounts
        based on their closing balances from the previous year. Make sure the previous year has been closed before opening a new one.
      </AlertDescription>
    </Alert>

    <!-- Error Alert -->
    <Alert v-if="error" variant="destructive">
      <iconify-icon icon="lucide:alert-circle" class="h-4 w-4" />
      <AlertTitle>Error</AlertTitle>
      <AlertDescription>{{ error }}</AlertDescription>
    </Alert>

    <!-- Year Selection Card -->
    <Card v-if="!openResult">
      <CardHeader>
        <CardTitle>Select Fiscal Year to Open</CardTitle>
        <CardDescription>
          Enter the new fiscal year you want to open. This should be the next year after your last closed year.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div class="flex flex-col gap-6 sm:flex-row sm:items-end">
          <div class="flex-1">
            <label class="text-sm font-medium mb-2 block">Fiscal Year</label>
            <Input
              v-model="yearToOpen"
              type="number"
              :min="new Date().getFullYear()"
              :max="new Date().getFullYear() + 1"
              class="w-full sm:w-48"
              placeholder="Enter year (e.g., 2026)"
            />
          </div>
          
          <AlertDialog>
            <AlertDialogTrigger as-child>
              <Button 
                :disabled="!canOpen || isOpening"
                class="w-full sm:w-auto"
              >
                <iconify-icon icon="lucide:calendar-plus" class="mr-2 h-4 w-4" />
                <span v-if="isOpening">Opening...</span>
                <span v-else>Open Year</span>
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Confirm Year Opening</AlertDialogTitle>
                <AlertDialogDescription class="space-y-2">
                  <p>
                    This action will open fiscal year <strong>{{ yearToOpen }}</strong>.
                  </p>
                  <p>
                    Opening balances will be created for all asset, liability, and equity accounts based on their closing balances from the previous year.
                  </p>
                  <p class="text-blue-600 font-medium">
                    Ensure that the previous fiscal year has been properly closed before proceeding.
                  </p>
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction @click="performYearBeginningOpen">
                  Yes, Open Year {{ yearToOpen }}
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </CardContent>
    </Card>

    <!-- Success Result -->
    <div v-if="openResult" class="space-y-6">
      <!-- Success Alert -->
      <Alert 
        :class="openResult.isBalanced 
          ? 'border-green-500 bg-green-50 dark:bg-green-950/20' 
          : 'border-amber-500 bg-amber-50 dark:bg-amber-950/20'"
      >
        <iconify-icon 
          :icon="openResult.isBalanced ? 'lucide:check-circle' : 'lucide:alert-triangle'" 
          :class="openResult.isBalanced ? 'text-green-600' : 'text-amber-600'"
          class="h-4 w-4" 
        />
        <AlertTitle :class="openResult.isBalanced ? 'text-green-800 dark:text-green-400' : 'text-amber-800 dark:text-amber-400'">
          Year-Beginning Open {{ openResult.isBalanced ? 'Successful' : 'Completed with Warning' }}
        </AlertTitle>
        <AlertDescription :class="openResult.isBalanced ? 'text-green-700 dark:text-green-500' : 'text-amber-700 dark:text-amber-500'">
          Fiscal year {{ openResult.year }} has been opened on {{ formatDate(openResult.openedAt) }}.
          <span v-if="!openResult.isBalanced">
            Warning: Opening entries are not balanced. Please review the entries below.
          </span>
        </AlertDescription>
      </Alert>

      <!-- Summary Cards -->
      <div class="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">Total Opening Debits</CardTitle>
            <iconify-icon icon="lucide:arrow-down-left" class="h-4 w-4 text-red-500" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold text-red-600">{{ formatAmount(openResult.totalOpeningDebits) }}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">Total Opening Credits</CardTitle>
            <iconify-icon icon="lucide:arrow-up-right" class="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div class="text-2xl font-bold text-green-600">{{ formatAmount(openResult.totalOpeningCredits) }}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle class="text-sm font-medium">Balance Status</CardTitle>
            <iconify-icon icon="lucide:scale" class="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            <div class="flex items-center gap-2">
              <Badge :variant="openResult.isBalanced ? 'default' : 'destructive'">
                <iconify-icon 
                  :icon="openResult.isBalanced ? 'lucide:check' : 'lucide:x'" 
                  class="mr-1 h-3 w-3" 
                />
                {{ openResult.isBalanced ? 'Balanced' : 'Unbalanced' }}
              </Badge>
            </div>
            <p class="text-xs text-muted-foreground mt-1">
              Difference: {{ formatAmount(Math.abs(openResult.totalOpeningDebits - openResult.totalOpeningCredits)) }}
            </p>
          </CardContent>
        </Card>
      </div>

      <!-- Opening Entries Table -->
      <Card>
        <CardHeader>
          <CardTitle>Opening Entries</CardTitle>
          <CardDescription>
            Opening balance entries created for the new fiscal year
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Ledger Code</TableHead>
                  <TableHead>Account Name</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Entry Type</TableHead>
                  <TableHead class="text-right">Amount</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="(entry, index) in openResult.openingEntries" :key="index">
                  <TableCell class="font-medium">{{ entry.ledgerCode }}</TableCell>
                  <TableCell>{{ entry.ledgerName }}</TableCell>
                  <TableCell>
                    <Badge :variant="getTypeBadgeColor(entry.type)">
                      {{ entry.type.charAt(0).toUpperCase() + entry.type.slice(1) }}
                    </Badge>
                  </TableCell>
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
        <Button @click="openResult = null; error = null">
          <iconify-icon icon="lucide:rotate-ccw" class="mr-2 h-4 w-4" />
          Open Another Year
        </Button>
        <Button variant="outline" @click="router.push({ name: 'ledger' })">
          <iconify-icon icon="lucide:list-tree" class="mr-2 h-4 w-4" />
          View Ledgers
        </Button>
      </div>
    </div>
  </div>
</template>
