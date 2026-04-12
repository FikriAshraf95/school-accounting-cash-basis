<script setup lang="ts">
import { ref, onMounted } from 'vue'
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

const payerId = Number(route.params.id)

interface Transaction {
  id: number
  transactionNumber: string
  transactionDate: string
  type: 'income' | 'expense'
  amount: number
  description: string | null
  receiptNumber: string | null
  isReversed: boolean
  createdAt: string
}

interface Payer {
  id: number
  payerCode: string
  name: string
  type: 'donor' | 'sponsor' | 'vendor' | 'supplier' | 'general' | 'government'
  category: 'individual' | 'corporate' | 'government' | 'ngo'
  email: string | null
  phone: string | null
  address: string | null
  balance: number
  isRecurring: boolean
  notes: string | null
  isActive: boolean
  transactionHistory: Transaction[]
  createdAt: string
  updatedAt: string
}

const payer = ref<Payer | null>(null)
const isLoading = ref(true)
const isDeleting = ref(false)
const error = ref<string | null>(null)

const payerTypes = [
  { value: 'donor', label: 'Donor' },
  { value: 'sponsor', label: 'Sponsor' },
  { value: 'vendor', label: 'Vendor' },
  { value: 'supplier', label: 'Supplier' },
  { value: 'general', label: 'General' },
  { value: 'government', label: 'Government' },
]

onMounted(async () => {
  sidebar.setPageName('View Payer')
  await fetchPayer()
})

async function fetchPayer() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getPayer(payerId) as any
    payer.value = response.data
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load payer'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function deletePayer() {
  try {
    isDeleting.value = true
    await api.deletePayer(payerId)
    toast.success('Success', { description: 'Payer deleted successfully' })
    router.push({ name: 'payers_list' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to delete payer'
    toast.error('Error', { description: message || undefined })
    isDeleting.value = false
  }
}

function goToEdit() {
  router.push({ name: 'payer_edit', params: { id: payerId } })
}

function goBack() {
  router.push({ name: 'payers_list' })
}

function getTypeLabel(type: string): string {
  const found = payerTypes.find(t => t.value === type)
  return found ? found.label : type
}

function formatDate(date: string): string {
  return new Date(date).toLocaleDateString('en-MY', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

function formatDateTime(date: string): string {
  return new Date(date).toLocaleString('en-MY')
}

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
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Payer Details</h2>
        <p class="text-muted-foreground">View payer information and transaction history</p>
      </div>
      <div class="flex gap-2">
        <Button variant="outline" @click="goBack">
          <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
          Back
        </Button>
        <Button v-if="!isLoading && !error" @click="goToEdit">
          <iconify-icon icon="lucide:edit" class="mr-2 h-4 w-4" />
          Edit
        </Button>
        <AlertDialog v-if="!isLoading && !error">
          <AlertDialogTrigger as-child>
            <Button variant="destructive">
              <iconify-icon icon="lucide:trash-2" class="mr-2 h-4 w-4" />
              Delete
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Delete Payer</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to delete this payer? This action cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction @click="deletePayer" :disabled="isDeleting">
                <iconify-icon v-if="isDeleting" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
                {{ isDeleting ? 'Deleting...' : 'Delete' }}
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
      <Skeleton class="h-64 w-full" />
    </div>

    <!-- Content -->
    <template v-else-if="payer">
      <!-- Payer Info Card -->
      <Card>
        <CardHeader>
          <div class="flex items-start justify-between">
            <div>
              <CardTitle class="text-2xl">{{ payer.name }}</CardTitle>
              <CardDescription>Payer Code: {{ payer.payerCode }}</CardDescription>
            </div>
            <div class="flex gap-2">
              <Badge :variant="payer.isActive ? 'default' : 'secondary'">
                {{ payer.isActive ? 'Active' : 'Inactive' }}
              </Badge>
              <Badge v-if="payer.isRecurring" variant="outline">Recurring</Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Type</dt>
              <dd class="mt-1 text-base">
                <Badge variant="outline">{{ getTypeLabel(payer.type) }}</Badge>
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Category</dt>
              <dd class="mt-1 text-base capitalize">{{ payer.category }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Email</dt>
              <dd class="mt-1 text-base">{{ payer.email || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Phone</dt>
              <dd class="mt-1 text-base">{{ payer.phone || '-' }}</dd>
            </div>
            <div class="sm:col-span-2">
              <dt class="text-sm font-medium text-muted-foreground">Address</dt>
              <dd class="mt-1 text-base">{{ payer.address || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Balance</dt>
              <dd class="mt-1 text-base" :class="payer.balance < 0 ? 'text-destructive font-semibold' : ''">
                {{ formatAmount(payer.balance) }}
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Payer ID</dt>
              <dd class="mt-1 text-base">#{{ payer.id }}</dd>
            </div>
          </dl>

          <div v-if="payer.notes" class="mt-6 pt-4 border-t">
            <dt class="text-sm font-medium text-muted-foreground">Notes</dt>
            <dd class="mt-1 text-base">{{ payer.notes }}</dd>
          </div>
        </CardContent>
      </Card>

      <!-- Transaction History Card -->
      <Card>
        <CardHeader>
          <CardTitle>Transaction History</CardTitle>
          <CardDescription>Recent financial transactions for this payer</CardDescription>
        </CardHeader>
        <CardContent>
          <!-- Empty State -->
          <div v-if="payer.transactionHistory.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
            <iconify-icon icon="lucide:receipt" class="h-12 w-12 text-muted-foreground mb-4" />
            <h3 class="text-lg font-semibold">No transactions</h3>
            <p class="text-sm text-muted-foreground">This payer has no recorded transactions yet.</p>
          </div>

          <!-- Transactions Table -->
          <div v-else class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Transaction #</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Receipt #</TableHead>
                  <TableHead class="text-right">Amount</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="txn in payer.transactionHistory" :key="txn.id">
                  <TableCell class="font-medium">{{ txn.transactionNumber }}</TableCell>
                  <TableCell>{{ formatDate(txn.transactionDate) }}</TableCell>
                  <TableCell>
                    <Badge :variant="txn.type === 'income' ? 'default' : 'secondary'">
                      {{ txn.type === 'income' ? 'Income' : 'Expense' }}
                    </Badge>
                  </TableCell>
                  <TableCell>{{ txn.description || '-' }}</TableCell>
                  <TableCell>{{ txn.receiptNumber || '-' }}</TableCell>
                  <TableCell class="text-right">
                    {{ formatAmount(txn.amount) }}
                  </TableCell>
                  <TableCell>
                    <Badge v-if="txn.isReversed" variant="destructive">Reversed</Badge>
                    <Badge v-else variant="outline">Completed</Badge>
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>

      <!-- Metadata Card -->
      <Card>
        <CardHeader>
          <CardTitle>Metadata</CardTitle>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Created At</dt>
              <dd class="mt-1 text-sm">{{ formatDateTime(payer.createdAt) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Last Updated</dt>
              <dd class="mt-1 text-sm">{{ formatDateTime(payer.updatedAt) }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </template>
  </div>
</template>
