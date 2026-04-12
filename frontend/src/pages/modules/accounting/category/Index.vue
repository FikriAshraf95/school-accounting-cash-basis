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

const categories = ref<Category[]>([])
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

onMounted(() => {
  sidebar.setPageName('Categories')
  const page = parseInt(route.query.page as string) || 1
  const perPage = parseInt(route.query.perPage as string) || 15
  pagination.value.page = page
  pagination.value.perPage = perPage
  fetchCategories()
})

async function fetchCategories() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {
      page: pagination.value.page,
      perPage: pagination.value.perPage,
    }

    if (typeFilter.value) params.type = typeFilter.value
    if (searchQuery.value) params.search = searchQuery.value

    const response = await api.getCategories(params) as any
    categories.value = response.data.data
    pagination.value = response.data.meta
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load categories'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

function handlePageChange(newPage: number) {
  pagination.value.page = newPage
  updateQueryParams()
  fetchCategories()
}

function handlePageSizeChange(newPageSize: number) {
  pagination.value.perPage = newPageSize
  pagination.value.page = 1
  updateQueryParams()
  fetchCategories()
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
  router.push({ name: 'create_category' })
}

function goToView(id: number) {
  router.push({ name: 'view_category', params: { id } })
}

function goToEdit(id: number) {
  router.push({ name: 'edit_category', params: { id } })
}

function getTypeBadgeVariant(type: string) {
  return type === 'income' ? 'default' : 'destructive'
}

watch([typeFilter, searchQuery], () => {
  pagination.value.page = 1
  fetchCategories()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Categories</h2>
        <p class="text-muted-foreground">Manage income and expense categories.</p>
      </div>
      <Button @click="goToCreate">
        <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
        Add Category
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
              placeholder="Search by name..."
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
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="categories.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:tag" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No categories found</h3>
          <p class="text-sm text-muted-foreground">Get started by creating a new category.</p>
          <Button class="mt-4" @click="goToCreate">
            <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
            Add Category
          </Button>
        </div>

        <!-- Data Table -->
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
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToView(category.id)"
                      title="View"
                    >
                      <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToEdit(category.id)"
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
        <div v-if="!isLoading && categories.length > 0" class="mt-4 flex justify-end">
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
