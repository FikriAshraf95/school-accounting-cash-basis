<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
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

const sidebar = useSidebarStore()
const route = useRoute()
const router = useRouter()

const transactionId = Number(route.params.id)

interface LineItem {
  id: number
  categoryId: number
  categoryName: string
  amount: number
  description: string | null
  quantity: number
  unitPrice: number
}

interface Transaction {
  id: number
  transactionNumber: string
  transactionDate: string
  type: 'income' | 'expense'
  transactableType: string
  studentId: number | null
  studentName: string | null
  payerId: number | null
  payerName: string | null
  amount: number
  paymentMethod: string | null
  referenceNumber: string | null
  description: string | null
  receiptNumber: string | null
  cashLedgerId: number
  cashLedgerName: string | null
  isReversed: boolean
  reversalTransactionId: number | null
  createdByName: string | null
  createdAt: string
  updatedAt: string
  items: LineItem[]
}

const transaction = ref<Transaction | null>(null)
const isLoading = ref(true)
const isReversing = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName('View Income')
  await fetchTransaction()
})

async function fetchTransaction() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getTransaction(transactionId) as any
    transaction.value = response
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load transaction'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function reverseTransaction() {
  try {
    isReversing.value = true
    await api.reverseTransaction(transactionId)
    toast.success('Success', { description: 'Transaction reversed successfully' })
    await fetchTransaction()
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to reverse transaction'
    toast.error('Error', { description: message || undefined })
  } finally {
    isReversing.value = false
  }
}

function goToEdit() {
  router.push({ name: 'edit_deposit', params: { id: transactionId } })
}

function goBack() {
  router.push({ name: 'deposit' })
}

function formatAmount(amount: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(amount)
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-MY')
}

function formatDateTime(dateString: string): string {
  return new Date(dateString).toLocaleString('en-MY')
}

const paymentMethodIcon = computed(() => {
  if (!transaction.value?.paymentMethod) return 'lucide:help-circle'
  const icons: Record<string, string> = {
    cash: 'lucide:banknote',
    bank_transfer: 'lucide:landmark',
    cheque: 'lucide:file-text',
    credit_card: 'lucide:credit-card',
    debit_card: 'lucide:credit-card',
    online: 'lucide:globe',
  }
  return icons[transaction.value.paymentMethod] || 'lucide:help-circle'
})

const fromName = computed(() => {
  if (!transaction.value) return '-'
  if (transaction.value.transactableType === 'Student') {
    return transaction.value.studentName || 'Unknown Student'
  }
  if (transaction.value.transactableType === 'Payer') {
    return transaction.value.payerName || 'Unknown Payer'
  }
  return 'General'
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Income Transaction</h2>
        <p class="text-muted-foreground">View income transaction details</p>
      </div>
      <div class="flex gap-2">
        <Button variant="outline" @click="goBack">
          <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
          Back
        </Button>
        <Button v-if="!isLoading && !error && transaction && !transaction.isReversed" @click="goToEdit">
          <iconify-icon icon="lucide:edit" class="mr-2 h-4 w-4" />
          Edit
        </Button>
        <AlertDialog v-if="!isLoading && !error && transaction && !transaction.isReversed">
          <AlertDialogTrigger as-child>
            <Button variant="destructive">
              <iconify-icon icon="lucide:rotate-ccw" class="mr-2 h-4 w-4" />
              Reverse
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Reverse Transaction</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to reverse this transaction? This will create a reversing entry and cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction @click="reverseTransaction" :disabled="isReversing">
                <iconify-icon v-if="isReversing" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
                {{ isReversing ? 'Reversing...' : 'Reverse' }}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>

    <!-- Error Alert -->
    <Alert v-if="error" variant="destructive">
      <iconify-icon icon="lucide:alert-circle" class="h-4 w-4" />
      <AlertTitle>Error</AlertTitle>
      <AlertDescription>{{ error }}</AlertDescription>
    </Alert>

    <!-- Loading State -->
    <div v-if="isLoading" class="space-y-4">
      <Skeleton class="h-8 w-1/3" />
      <Skeleton class="h-64 w-full" />
      <Skeleton class="h-48 w-full" />
    </div>

    <!-- Content -->
    <template v-else-if="transaction">
      <!-- Status Banner -->
      <Alert v-if="transaction.isReversed" variant="destructive">
        <iconify-icon icon="lucide:alert-triangle" class="h-4 w-4" />
        <AlertTitle>Reversed Transaction</AlertTitle>
        <AlertDescription>
          This transaction has been reversed.
          <span v-if="transaction.reversalTransactionId">
            Reversal ID: #{{ transaction.reversalTransactionId }}
          </span>
        </AlertDescription>
      </Alert>

      <!-- Main Transaction Card -->
      <Card>
        <CardHeader>
          <div class="flex items-start justify-between">
            <div>
              <CardTitle class="text-2xl">{{ transaction.transactionNumber }}</CardTitle>
              <CardDescription>Income Transaction</CardDescription>
            </div>
            <Badge :variant="transaction.isReversed ? 'destructive' : 'default'" class="text-sm">
              {{ transaction.isReversed ? 'Reversed' : 'Active' }}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Transaction Date</dt>
              <dd class="mt-1 text-lg">{{ formatDate(transaction.transactionDate) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Amount</dt>
              <dd class="mt-1 text-2xl font-bold text-green-600">{{ formatAmount(transaction.amount) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Payment Method</dt>
              <dd class="mt-1 flex items-center gap-2">
                <iconify-icon :icon="paymentMethodIcon" class="h-4 w-4 text-muted-foreground" />
                <span class="capitalize">{{ transaction.paymentMethod?.replace('_', ' ') || 'Unknown' }}</span>
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">From</dt>
              <dd class="mt-1 text-base">{{ fromName }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Cash Ledger</dt>
              <dd class="mt-1 text-base">{{ transaction.cashLedgerName || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Reference Number</dt>
              <dd class="mt-1 text-base">{{ transaction.referenceNumber || '-' }}</dd>
            </div>
            <div class="sm:col-span-2 lg:col-span-3" v-if="transaction.description">
              <dt class="text-sm font-medium text-muted-foreground">Description</dt>
              <dd class="mt-1 text-base">{{ transaction.description }}</dd>
            </div>
            <div v-if="transaction.receiptNumber">
              <dt class="text-sm font-medium text-muted-foreground">Receipt Number</dt>
              <dd class="mt-1 text-base">{{ transaction.receiptNumber }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <!-- Line Items Card -->
      <Card>
        <CardHeader>
          <CardTitle>Line Items</CardTitle>
          <CardDescription>Breakdown of income categories</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Category</TableHead>
                <TableHead>Description</TableHead>
                <TableHead class="text-right">Qty</TableHead>
                <TableHead class="text-right">Unit Price</TableHead>
                <TableHead class="text-right">Amount</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="item in transaction.items" :key="item.id">
                <TableCell class="font-medium">{{ item.categoryName || 'Unknown' }}</TableCell>
                <TableCell>{{ item.description || '-' }}</TableCell>
                <TableCell class="text-right">{{ item.quantity }}</TableCell>
                <TableCell class="text-right">{{ formatAmount(item.unitPrice) }}</TableCell>
                <TableCell class="text-right font-medium">{{ formatAmount(item.amount) }}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell colspan="4" class="text-right font-medium">Total</TableCell>
                <TableCell class="text-right font-bold text-green-600">{{ formatAmount(transaction.amount) }}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <!-- Metadata Card -->
      <Card>
        <CardHeader>
          <CardTitle>Metadata</CardTitle>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-3">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Created By</dt>
              <dd class="mt-1 text-sm">{{ transaction.createdByName || 'System' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Created At</dt>
              <dd class="mt-1 text-sm">{{ formatDateTime(transaction.createdAt) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Last Updated</dt>
              <dd class="mt-1 text-sm">{{ formatDateTime(transaction.updatedAt) }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </template>
  </div>
</template>
