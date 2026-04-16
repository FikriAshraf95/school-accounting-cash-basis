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
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
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

const activeTab = ref<string>((route.query.tab as string) || 'ledger')

// ---- Interfaces ----
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

interface Category {
  id: number
  name: string
  type: 'income' | 'expense'
  ledgerId: number
  ledgerName: string
  ledgerCode: string
  description: string | null
  requiresStudent: boolean
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

// ---- Ledger State ----
const ledgers = ref<Ledger[]>([])
const ledgerPagination = ref<PaginationMeta>({ total: 0, page: 1, perPage: 10, lastPage: 1 })
const ledgerLoading = ref(true)
const ledgerError = ref<string | null>(null)
const ledgerTypeFilter = ref<string>('all')
const ledgerSearch = ref('')

const ledgerTypes = [
  { value: 'asset', label: 'Asset' },
  { value: 'liability', label: 'Liability' },
  { value: 'equity', label: 'Equity' },
  { value: 'revenue', label: 'Revenue' },
  { value: 'expense', label: 'Expense' },
]

async function fetchLedgers() {
  try {
    ledgerLoading.value = true
    ledgerError.value = null
    const params: any = {
      page: ledgerPagination.value.page,
      perPage: ledgerPagination.value.perPage,
    }
    if (ledgerTypeFilter.value !== 'all') params.type = ledgerTypeFilter.value
    if (ledgerSearch.value) params.search = ledgerSearch.value
    const response = await api.getLedgers(params) as any
    ledgers.value = response.data ?? []
    ledgerPagination.value = response.meta ?? ledgerPagination.value
  } catch (err: any) {
    if (isCancel(err)) return
    ledgerError.value = err?.response?.data?.detail || 'Failed to load ledgers'
    toast.error('Error', { description: ledgerError.value || undefined })
  } finally {
    ledgerLoading.value = false
  }
}

function handleLedgerPageChange(newPage: number) {
  ledgerPagination.value.page = newPage
  fetchLedgers()
}

function handleLedgerPageSizeChange(newPageSize: number) {
  ledgerPagination.value.perPage = newPageSize
  ledgerPagination.value.page = 1
  fetchLedgers()
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
  return new Intl.NumberFormat('en-MY', { style: 'currency', currency: 'MYR' }).format(balance)
}

watch([ledgerTypeFilter, ledgerSearch], () => {
  ledgerPagination.value.page = 1
  fetchLedgers()
})

// ---- Category State ----
const categories = ref<Category[]>([])
const catPagination = ref<PaginationMeta>({ total: 0, page: 1, perPage: 10, lastPage: 1 })
const catLoading = ref(true)
const catError = ref<string | null>(null)
const catTypeFilter = ref<string>('all')
const catSearch = ref('')

async function fetchCategories() {
  try {
    catLoading.value = true
    catError.value = null
    const params: any = {
      page: catPagination.value.page,
      perPage: catPagination.value.perPage,
    }
    if (catTypeFilter.value !== 'all') params.type = catTypeFilter.value
    if (catSearch.value) params.search = catSearch.value
    const response = await api.getCategories(params) as any
    categories.value = response.data ?? []
    catPagination.value = response.meta ?? catPagination.value
  } catch (err: any) {
    if (isCancel(err)) return
    catError.value = err?.response?.data?.detail || 'Failed to load categories'
    toast.error('Error', { description: catError.value || undefined })
  } finally {
    catLoading.value = false
  }
}

function handleCatPageChange(newPage: number) {
  catPagination.value.page = newPage
  fetchCategories()
}

function handleCatPageSizeChange(newPageSize: number) {
  catPagination.value.perPage = newPageSize
  catPagination.value.page = 1
  fetchCategories()
}

function getTypeBadgeVariant(type: string) {
  return type === 'income' ? 'default' : 'destructive'
}

watch([catTypeFilter, catSearch], () => {
  catPagination.value.page = 1
  fetchCategories()
})

// ---- Shared ----
function onTabChange(tab: string | number) {
  activeTab.value = String(tab)
  router.replace({ query: { ...route.query, tab: String(tab) } })
}

// Navigation - Ledger
function goToCreateLedger() { router.push({ name: 'create_ledger' }) }
function goToViewLedger(id: number) { router.push({ name: 'view_ledger', params: { id } }) }
function goToEditLedger(id: number) { router.push({ name: 'edit_ledger', params: { id } }) }

// Navigation - Category
function goToCreateCategory() { router.push({ name: 'create_category' }) }
function goToViewCategory(id: number) { router.push({ name: 'view_category', params: { id } }) }
function goToEditCategory(id: number) { router.push({ name: 'edit_category', params: { id } }) }

onMounted(() => {
  sidebar.setPageName('Chart of Accounts')
  fetchLedgers()
  fetchCategories()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Chart of Accounts</h2>
        <p class="text-muted-foreground">Manage your ledger accounts and categories.</p>
      </div>
      <Button v-if="activeTab === 'ledger'" @click="goToCreateLedger">
        <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
        Add Ledger
      </Button>
      <Button v-else @click="goToCreateCategory">
        <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
        Add Category
      </Button>
    </div>

    <Tabs :model-value="activeTab" @update:model-value="onTabChange">
      <TabsList>
        <TabsTrigger value="ledger">Ledger Accounts</TabsTrigger>
        <TabsTrigger value="category">Categories</TabsTrigger>
      </TabsList>

      <!-- Ledger Tab -->
      <TabsContent value="ledger" class="space-y-4 mt-4">
        <!-- Error Alert -->
        <Alert v-if="ledgerError" variant="destructive">
          <iconify-icon icon="lucide:alert-circle" class="h-4 w-4" />
          <AlertTitle>Error</AlertTitle>
          <AlertDescription>{{ ledgerError }}</AlertDescription>
        </Alert>

        <!-- Filters -->
        <Card>
          <CardContent class="pt-6">
            <div class="flex flex-col gap-4 sm:flex-row">
              <div class="flex-1">
                <Input
                  v-model="ledgerSearch"
                  placeholder="Search by code or name..."
                  class="max-w-sm"
                >
                  <template #prefix>
                    <iconify-icon icon="lucide:search" class="h-4 w-4 text-muted-foreground" />
                  </template>
                </Input>
              </div>
              <div class="w-full sm:w-48">
                <Select v-model="ledgerTypeFilter">
                  <SelectTrigger>
                    <SelectValue placeholder="Filter by type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Types</SelectItem>
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
            <div v-if="ledgerLoading" class="space-y-4">
              <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
            </div>

            <div v-else-if="ledgers.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
              <iconify-icon icon="lucide:book-open" class="h-12 w-12 text-muted-foreground mb-4" />
              <h3 class="text-lg font-semibold">No ledgers found</h3>
              <p class="text-sm text-muted-foreground">Get started by creating a new ledger account.</p>
              <Button class="mt-4" @click="goToCreateLedger">
                <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
                Add Ledger
              </Button>
            </div>

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
                        <Button variant="ghost" size="icon" @click="goToViewLedger(ledger.id)" title="View">
                          <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" @click="goToEditLedger(ledger.id)" title="Edit">
                          <iconify-icon icon="lucide:pencil" class="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </div>

            <div v-if="!ledgerLoading && ledgers.length > 0" class="mt-4 flex justify-end">
              <Pagination
                :total-count="ledgerPagination.total"
                :total-pages="ledgerPagination.lastPage"
                :page-number="ledgerPagination.page"
                :page-size="ledgerPagination.perPage"
                @update-page-number="handleLedgerPageChange"
                @update-page-size="handleLedgerPageSizeChange"
              />
            </div>
          </CardContent>
        </Card>
      </TabsContent>

      <!-- Category Tab -->
      <TabsContent value="category" class="space-y-4 mt-4">
        <!-- Error Alert -->
        <Alert v-if="catError" variant="destructive">
          <iconify-icon icon="lucide:alert-circle" class="h-4 w-4" />
          <AlertTitle>Error</AlertTitle>
          <AlertDescription>{{ catError }}</AlertDescription>
        </Alert>

        <!-- Filters -->
        <Card>
          <CardContent class="pt-6">
            <div class="flex flex-col gap-4 sm:flex-row">
              <div class="flex-1">
                <Input
                  v-model="catSearch"
                  placeholder="Search by name..."
                  class="max-w-sm"
                >
                  <template #prefix>
                    <iconify-icon icon="lucide:search" class="h-4 w-4 text-muted-foreground" />
                  </template>
                </Input>
              </div>
              <div class="w-full sm:w-48">
                <Select v-model="catTypeFilter">
                  <SelectTrigger>
                    <SelectValue placeholder="Filter by type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Types</SelectItem>
                    <SelectItem value="income">Income</SelectItem>
                    <SelectItem value="expense">Expense</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </CardContent>
        </Card>

        <!-- Table -->
        <Card>
          <CardHeader>
            <CardTitle>Category List</CardTitle>
          </CardHeader>
          <CardContent>
            <div v-if="catLoading" class="space-y-4">
              <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
            </div>

            <div v-else-if="categories.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
              <iconify-icon icon="lucide:tag" class="h-12 w-12 text-muted-foreground mb-4" />
              <h3 class="text-lg font-semibold">No categories found</h3>
              <p class="text-sm text-muted-foreground">Get started by creating a new category.</p>
              <Button class="mt-4" @click="goToCreateCategory">
                <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
                Add Category
              </Button>
            </div>

            <div v-else class="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Type</TableHead>
                    <TableHead>Ledger</TableHead>
                    <TableHead>Requires Student</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead class="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableRow v-for="category in categories" :key="category.id">
                    <TableCell class="font-medium">{{ category.name }}</TableCell>
                    <TableCell>
                      <Badge :variant="getTypeBadgeVariant(category.type)">
                        {{ category.type.charAt(0).toUpperCase() + category.type.slice(1) }}
                      </Badge>
                    </TableCell>
                    <TableCell class="text-sm text-muted-foreground">
                      {{ category.ledgerCode }} — {{ category.ledgerName }}
                    </TableCell>
                    <TableCell>
                      <Badge :variant="category.requiresStudent ? 'default' : 'secondary'">
                        {{ category.requiresStudent ? 'Yes' : 'No' }}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge :variant="category.isActive ? 'default' : 'secondary'">
                        {{ category.isActive ? 'Active' : 'Inactive' }}
                      </Badge>
                    </TableCell>
                    <TableCell class="text-right">
                      <div class="flex justify-end gap-2">
                        <Button variant="ghost" size="icon" @click="goToViewCategory(category.id)" title="View">
                          <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" @click="goToEditCategory(category.id)" title="Edit">
                          <iconify-icon icon="lucide:pencil" class="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </div>

            <div v-if="!catLoading && categories.length > 0" class="mt-4 flex justify-end">
              <Pagination
                :total-count="catPagination.total"
                :total-pages="catPagination.lastPage"
                :page-number="catPagination.page"
                :page-size="catPagination.perPage"
                @update-page-number="handleCatPageChange"
                @update-page-size="handleCatPageSizeChange"
              />
            </div>
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>
  </div>
</template>
