<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { isCancel } from '@/services/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Label } from '@/components/ui/label'
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
import Pagination from '@/components/templates/Pagination.vue'

const sidebar = useSidebarStore()
const router = useRouter()
const route = useRoute()

interface Transaction {
  id: number
  transactionNumber: string
  transactionDate: string
  type: 'income' | 'expense'
  transactableName: string | null
  amount: number
  paymentMethod: string | null
  description: string | null
  isReversed: boolean
  createdAt: string
}

interface PaginationMeta {
  total: number
  page: number
  perPage: number
  lastPage: number
}

const transactions = ref<Transaction[]>([])
const pagination = ref<PaginationMeta>({
  total: 0,
  page: 1,
  perPage: 15,
  lastPage: 1,
})
const isLoading = ref(true)
const error = ref<string | null>(null)

// Filters
const dateFrom = ref('')
const dateTo = ref('')
const typeFilter = ref<string>('')
const searchQuery = ref('')

const typeOptions = [
  { value: 'income', label: 'Income', color: 'default' },
  { value: 'expense', label: 'Expense', color: 'destructive' },
]

onMounted(() => {
  sidebar.setPageName('All Transactions')
  // Get initial values from query params
  const page = parseInt(route.query.page as string) || 1
  const perPage = parseInt(route.query.perPage as string) || 15
  pagination.value.page = page
  pagination.value.perPage = perPage

  if (route.query.dateFrom) dateFrom.value = route.query.dateFrom as string
  if (route.query.dateTo) dateTo.value = route.query.dateTo as string
  if (route.query.type) typeFilter.value = route.query.type as string
  if (route.query.search) searchQuery.value = route.query.search as string

  fetchTransactions()
})

async function fetchTransactions() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {
      page: pagination.value.page,
      perPage: pagination.value.perPage,
    }

    if (dateFrom.value) params.dateFrom = dateFrom.value
    if (dateTo.value) params.dateTo = dateTo.value
    if (typeFilter.value) params.type = typeFilter.value
    if (searchQuery.value) params.search = searchQuery.value

    const response = await api.getTransactions(params) as any
    transactions.value = response.data.data
    pagination.value = response.data.meta
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load transactions'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

function handlePageChange(newPage: number) {
  pagination.value.page = newPage
  updateQueryParams()
  fetchTransactions()
}

function handlePageSizeChange(newPageSize: number) {
  pagination.value.perPage = newPageSize
  pagination.value.page = 1
  updateQueryParams()
  fetchTransactions()
}

function updateQueryParams() {
  const query: any = {
    page: pagination.value.page.toString(),
    perPage: pagination.value.perPage.toString(),
  }
  if (dateFrom.value) query.dateFrom = dateFrom.value
  if (dateTo.value) query.dateTo = dateTo.value
  if (typeFilter.value) query.type = typeFilter.value
  if (searchQuery.value) query.search = searchQuery.value

  router.replace({ query })
}

function applyFilters() {
  pagination.value.page = 1
  updateQueryParams()
  fetchTransactions()
}

function resetFilters() {
  dateFrom.value = ''
  dateTo.value = ''
  typeFilter.value = ''
  searchQuery.value = ''
  pagination.value.page = 1
  updateQueryParams()
  fetchTransactions()
}

function goToView(transaction: Transaction) {
  if (transaction.type === 'income') {
    router.push({ name: 'view_deposit', params: { id: transaction.id } })
  } else {
    router.push({ name: 'view_payment', params: { id: transaction.id } })
  }
}

function formatAmount(amount: number, type: string): string {
  const formatted = new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(amount)
  return formatted
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-MY')
}

function getPaymentMethodIcon(method: string | null): string {
  if (!method) return 'lucide:help-circle'
  const icons: Record<string, string> = {
    cash: 'lucide:banknote',
    bank_transfer: 'lucide:landmark',
    cheque: 'lucide:file-text',
    credit_card: 'lucide:credit-card',
    debit_card: 'lucide:credit-card',
    online: 'lucide:globe',
  }
  return icons[method] || 'lucide:help-circle'
}

function getTypeBadgeColor(type: string): any {
  return type === 'income' ? 'default' : 'destructive'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">All Transactions</h2>
        <p class="text-muted-foreground">View all income and expense transactions. This is a read-only view.</p>
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
        <div class="flex flex-col gap-4 md:flex-row md:items-end">
          <div class="flex-1">
            <Label class="mb-2 block text-sm font-medium">Search</Label>
            <Input
              v-model="searchQuery"
              placeholder="Search by ref no or name..."
              @keyup.enter="applyFilters"
            >
              <template #prefix>
                <iconify-icon icon="lucide:search" class="h-4 w-4 text-muted-foreground" />
              </template>
            </Input>
          </div>
          <div class="w-full md:w-44">
            <Label class="mb-2 block text-sm font-medium">Type</Label>
            <Select v-model="typeFilter">
              <SelectTrigger>
                <SelectValue placeholder="All types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">All Types</SelectItem>
                <SelectItem v-for="opt in typeOptions" :key="opt.value" :value="opt.value">
                  {{ opt.label }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="w-full md:w-44">
            <Label class="mb-2 block text-sm font-medium">Date From</Label>
            <Input
              v-model="dateFrom"
              type="date"
              placeholder="From date"
            />
          </div>
          <div class="w-full md:w-44">
            <Label class="mb-2 block text-sm font-medium">Date To</Label>
            <Input
              v-model="dateTo"
              type="date"
              placeholder="To date"
            />
          </div>
          <div class="flex gap-2">
            <Button variant="secondary" @click="applyFilters">
              <iconify-icon icon="lucide:filter" class="mr-2 h-4 w-4" />
              Filter
            </Button>
            <Button variant="outline" @click="resetFilters">
              <iconify-icon icon="lucide:rotate-ccw" class="mr-2 h-4 w-4" />
              Reset
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>

    <!-- Table -->
    <Card>
      <CardHeader>
        <CardTitle>Transaction History</CardTitle>
      </CardHeader>
      <CardContent>
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="transactions.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:receipt" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No transactions found</h3>
          <p class="text-sm text-muted-foreground">Try adjusting your filters or date range.</p>
        </div>

        <!-- Data Table -->
        <div v-else class="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Date</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Ref No</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Method</TableHead>
                <TableHead class="text-right">Amount</TableHead>
                <TableHead>Status</TableHead>
                <TableHead class="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow 
                v-for="transaction in transactions" 
                :key="transaction.id" 
                :class="transaction.isReversed ? 'opacity-50' : ''"
              >
                <TableCell>{{ formatDate(transaction.transactionDate) }}</TableCell>
                <TableCell>
                  <Badge :variant="getTypeBadgeColor(transaction.type)">
                    {{ transaction.type.charAt(0).toUpperCase() + transaction.type.slice(1) }}
                  </Badge>
                </TableCell>
                <TableCell class="font-medium">{{ transaction.transactionNumber }}</TableCell>
                <TableCell>{{ transaction.transactableName || 'General' }}</TableCell>
                <TableCell>
                  <div class="flex items-center gap-2">
                    <iconify-icon :icon="getPaymentMethodIcon(transaction.paymentMethod)" class="h-4 w-4 text-muted-foreground" />
                    <span class="capitalize">{{ transaction.paymentMethod?.replace('_', ' ') || 'Unknown' }}</span>
                  </div>
                </TableCell>
                <TableCell 
                  class="text-right font-medium"
                  :class="transaction.type === 'income' ? 'text-green-600' : 'text-destructive'"
                >
                  {{ formatAmount(transaction.amount, transaction.type) }}
                </TableCell>
                <TableCell>
                  <Badge v-if="transaction.isReversed" variant="destructive">
                    Reversed
                  </Badge>
                  <Badge v-else variant="outline">
                    Active
                  </Badge>
                </TableCell>
                <TableCell class="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    @click="goToView(transaction)"
                    title="View"
                  >
                    <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                  </Button>
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </div>

        <!-- Pagination -->
        <div v-if="!isLoading && transactions.length > 0" class="mt-4 flex justify-end">
          <Pagination
            :total-count="pagination.total"
            :total-pages="pagination.lastPage"
            :page-number="pagination.page"
            :page-size="pagination.perPage"
            @update-page-number="handlePageChange"
            @update-page-size="handlePageSizeChange"
          />
        </div>
      </CardContent>
    </Card>
  </div>
</template>
