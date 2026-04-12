<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
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

interface Ledger {
  id: number
  code: string
  name: string
  type: 'asset' | 'liability' | 'equity' | 'revenue' | 'expense'
  category: string | null
  balance: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}

interface PaginationMeta {
  total: number
  page: number
  perPage: number
  lastPage: number
}

const ledgers = ref<Ledger[]>([])
const pagination = ref<PaginationMeta>({
  total: 0,
  page: 1,
  perPage: 15,
  lastPage: 1,
})
const isLoading = ref(true)
const error = ref<string | null>(null)

// Filters
const typeFilter = ref<string>('')
const searchQuery = ref('')

const ledgerTypes = [
  { value: 'asset', label: 'Asset', color: 'default' },
  { value: 'liability', label: 'Liability', color: 'secondary' },
  { value: 'equity', label: 'Equity', color: 'outline' },
  { value: 'revenue', label: 'Revenue', color: 'default' },
  { value: 'expense', label: 'Expense', color: 'destructive' },
]

onMounted(() => {
  sidebar.setPageName('Chart of Accounts')
  // Get initial page from query params
  const page = parseInt(route.query.page as string) || 1
  const perPage = parseInt(route.query.perPage as string) || 15
  pagination.value.page = page
  pagination.value.perPage = perPage
  fetchLedgers()
})

async function fetchLedgers() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {
      page: pagination.value.page,
      perPage: pagination.value.perPage,
    }

    if (typeFilter.value) {
      params.type = typeFilter.value
    }

    const response = await api.getLedgers(params) as any
    ledgers.value = response.data.data
    pagination.value = response.data.meta
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load ledgers'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

function handlePageChange(newPage: number) {
  pagination.value.page = newPage
  updateQueryParams()
  fetchLedgers()
}

function handlePageSizeChange(newPageSize: number) {
  pagination.value.perPage = newPageSize
  pagination.value.page = 1
  updateQueryParams()
  fetchLedgers()
}

function updateQueryParams() {
  router.replace({
    query: {
      ...route.query,
      page: pagination.value.page.toString(),
      perPage: pagination.value.perPage.toString(),
    },
  })
}

function goToCreate() {
  router.push({ name: 'create_ledger' })
}

function goToView(id: number) {
  router.push({ name: 'view_ledger', params: { id } })
}

function goToEdit(id: number) {
  router.push({ name: 'edit_ledger', params: { id } })
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

function formatBalance(balance: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(balance)
}

// Watch for filter changes
watch(typeFilter, () => {
  pagination.value.page = 1
  fetchLedgers()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Chart of Accounts</h2>
        <p class="text-muted-foreground">Manage your ledger accounts and chart of accounts.</p>
      </div>
      <Button @click="goToCreate">
        <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
        Add Ledger
      </Button>
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
        <div class="flex flex-col gap-4 sm:flex-row">
          <div class="flex-1">
            <Input
              v-model="searchQuery"
              placeholder="Search by code or name..."
              class="max-w-sm"
            >
              <template #prefix>
                <iconify-icon icon="lucide:search" class="h-4 w-4 text-muted-foreground" />
              </template>
            </Input>
          </div>
          <div class="w-full sm:w-48">
            <Select v-model="typeFilter">
              <SelectTrigger>
                <SelectValue placeholder="Filter by type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="">All Types</SelectItem>
                <SelectItem v-for="type in ledgerTypes" :key="type.value" :value="type.value">
                  {{ type.label }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </CardContent>
    </Card>

    <!-- Table -->
    <Card>
      <CardHeader>
        <CardTitle>Ledger Accounts</CardTitle>
      </CardHeader>
      <CardContent>
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="ledgers.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:book-open" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No ledgers found</h3>
          <p class="text-sm text-muted-foreground">Get started by creating a new ledger account.</p>
          <Button class="mt-4" @click="goToCreate">
            <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
            Add Ledger
          </Button>
        </div>

        <!-- Data Table -->
        <div v-else class="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Code</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead class="text-right">Balance</TableHead>
                <TableHead>Status</TableHead>
                <TableHead class="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="ledger in ledgers" :key="ledger.id">
                <TableCell class="font-medium">{{ ledger.code }}</TableCell>
                <TableCell>{{ ledger.name }}</TableCell>
                <TableCell>
                  <Badge :variant="getTypeBadgeColor(ledger.type)">
                    {{ ledger.type.charAt(0).toUpperCase() + ledger.type.slice(1) }}
                  </Badge>
                </TableCell>
                <TableCell class="text-right">{{ formatBalance(ledger.balance) }}</TableCell>
                <TableCell>
                  <Badge :variant="ledger.isActive ? 'default' : 'secondary'">
                    {{ ledger.isActive ? 'Active' : 'Inactive' }}
                  </Badge>
                </TableCell>
                <TableCell class="text-right">
                  <div class="flex justify-end gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToView(ledger.id)"
                      title="View"
                    >
                      <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToEdit(ledger.id)"
                      title="Edit"
                    >
                      <iconify-icon icon="lucide:pencil" class="h-4 w-4" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </div>

        <!-- Pagination -->
        <div v-if="!isLoading && ledgers.length > 0" class="mt-4 flex justify-end">
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
