<script setup lang="ts">
import { ref, onMounted } from 'vue'
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

const payers = ref<Payer[]>([])
const pagination = ref<PaginationMeta>({
  total: 0,
  page: 1,
  perPage: 10,
  lastPage: 1,
})
const isLoading = ref(true)
const error = ref<string | null>(null)

// Filters
const searchQuery = ref('')
const selectedType = ref<string>('all')

const payerTypes = [
  { value: 'donor', label: 'Donor' },
  { value: 'sponsor', label: 'Sponsor' },
  { value: 'vendor', label: 'Vendor' },
  { value: 'supplier', label: 'Supplier' },
  { value: 'general', label: 'General' },
  { value: 'government', label: 'Government' },
]

onMounted(() => {
  sidebar.setPageName('Payers')
  // Get initial values from query params
  const page = parseInt(route.query.page as string) || 1
  const perPage = parseInt(route.query.perPage as string) || 10
  pagination.value.page = page
  pagination.value.perPage = perPage
  
  if (route.query.search) searchQuery.value = route.query.search as string
  if (route.query.type) selectedType.value = route.query.type as string || 'all'
  
  fetchPayers()
})

async function fetchPayers() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {
      page: pagination.value.page,
      perPage: pagination.value.perPage,
    }

    if (searchQuery.value) params.search = searchQuery.value
    if (selectedType.value && selectedType.value !== 'all') params.type = selectedType.value

    const response = await api.getPayers(params) as any
    payers.value = response.data ?? []
    pagination.value = response.meta ?? pagination.value
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load payers'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

function handlePageChange(newPage: number) {
  pagination.value.page = newPage
  updateQueryParams()
  fetchPayers()
}

function handlePageSizeChange(newPageSize: number) {
  pagination.value.perPage = newPageSize
  pagination.value.page = 1
  updateQueryParams()
  fetchPayers()
}

function updateQueryParams() {
  const query: any = {
    page: pagination.value.page.toString(),
    perPage: pagination.value.perPage.toString(),
  }
  if (searchQuery.value) query.search = searchQuery.value
  if (selectedType.value && selectedType.value !== 'all') query.type = selectedType.value
  
  router.replace({ query })
}

function applyFilters() {
  pagination.value.page = 1
  updateQueryParams()
  fetchPayers()
}

function resetFilters() {
  searchQuery.value = ''
  selectedType.value = 'all'
  pagination.value.page = 1
  updateQueryParams()
  fetchPayers()
}

function goToCreate() {
  router.push({ name: 'payer_create' })
}

function goToView(id: number) {
  router.push({ name: 'payer_view', params: { id } })
}

function goToEdit(id: number) {
  router.push({ name: 'payer_edit', params: { id } })
}

function getTypeLabel(type: string): string {
  const found = payerTypes.find(t => t.value === type)
  return found ? found.label : type
}

function formatBalance(amount: number): string {
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
        <h2 class="text-3xl font-bold tracking-tight">Payers</h2>
        <p class="text-muted-foreground">Manage payers, donors, vendors, and sponsors.</p>
      </div>
      <Button @click="goToCreate">
        <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
        Add Payer
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
        <div class="flex flex-col gap-4 md:flex-row md:items-end">
          <div class="flex-1">
            <Label class="mb-2 block text-sm font-medium">Search</Label>
            <Input
              v-model="searchQuery"
              placeholder="Search by name or payer code..."
              @keyup.enter="applyFilters"
            >
              <template #prefix>
                <iconify-icon icon="lucide:search" class="h-4 w-4 text-muted-foreground" />
              </template>
            </Input>
          </div>
          <div class="w-full md:w-48">
            <Label class="mb-2 block text-sm font-medium">Type</Label>
            <Select v-model="selectedType">
              <SelectTrigger>
                <SelectValue placeholder="All Types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                <SelectItem 
                  v-for="type in payerTypes" 
                  :key="type.value" 
                  :value="type.value"
                >
                  {{ type.label }}
                </SelectItem>
              </SelectContent>
            </Select>
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
        <CardTitle>Payer List</CardTitle>
      </CardHeader>
      <CardContent>
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="payers.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:building-2" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No payers found</h3>
          <p class="text-sm text-muted-foreground">Get started by creating a new payer.</p>
          <Button class="mt-4" @click="goToCreate">
            <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
            Add Payer
          </Button>
        </div>

        <!-- Data Table -->
        <div v-else class="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Payer Code</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Category</TableHead>
                <TableHead>Contact</TableHead>
                <TableHead class="text-right">Balance</TableHead>
                <TableHead>Status</TableHead>
                <TableHead class="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="payer in payers" :key="payer.id">
                <TableCell class="font-medium">{{ payer.payerCode }}</TableCell>
                <TableCell>{{ payer.name }}</TableCell>
                <TableCell>
                  <Badge variant="outline">{{ getTypeLabel(payer.type) }}</Badge>
                </TableCell>
                <TableCell>
                  <span class="capitalize">{{ payer.category }}</span>
                </TableCell>
                <TableCell>
                  <div class="text-sm">
                    <div v-if="payer.email">{{ payer.email }}</div>
                    <div v-if="payer.phone" class="text-muted-foreground">{{ payer.phone }}</div>
                    <div v-if="!payer.email && !payer.phone">-</div>
                  </div>
                </TableCell>
                <TableCell class="text-right" :class="payer.balance < 0 ? 'text-destructive' : ''">
                  {{ formatBalance(payer.balance) }}
                </TableCell>
                <TableCell>
                  <Badge :variant="payer.isActive ? 'default' : 'secondary'">
                    {{ payer.isActive ? 'Active' : 'Inactive' }}
                  </Badge>
                </TableCell>
                <TableCell class="text-right">
                  <div class="flex justify-end gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToView(payer.id)"
                      title="View"
                    >
                      <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToEdit(payer.id)"
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
        <div v-if="!isLoading && payers.length > 0" class="mt-4 flex justify-end">
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
