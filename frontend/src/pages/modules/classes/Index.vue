<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { isCancel } from '@/services/api'
import { toast } from 'vue-sonner'
import { useDebounceFn } from '@vueuse/core'
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
import Pagination from '@/components/templates/Pagination.vue'

const sidebar = useSidebarStore()
const router = useRouter()
const route = useRoute()

interface Class {
  id: number
  name: string
  code: string
  gradeId: number
  gradeName: string
  section: string | null
  description: string | null
  capacity: number
  feeAmount: number
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

const classes = ref<Class[]>([])
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

onMounted(() => {
  sidebar.setPageName('Classes')
  // Get initial page from query params
  const page = parseInt(route.query.page as string) || 1
  const perPage = parseInt(route.query.perPage as string) || 10
  pagination.value.page = page
  pagination.value.perPage = perPage
  fetchClasses()
})


async function fetchClasses() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {
      page: pagination.value.page,
      perPage: pagination.value.perPage,
    }

    if (searchQuery.value) params.search = searchQuery.value

    const response = await api.getClasses(params) as any
    classes.value = response.data ?? []
    pagination.value = response.meta ?? pagination.value
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load classes'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

const debouncedFetchClasses = useDebounceFn(async () => {
  pagination.value.page = 1
  await fetchClasses()
}, 1000)

watch(searchQuery, () => {
  debouncedFetchClasses()
}, { immediate: false })

function handlePageChange(newPage: number) {
  pagination.value.page = newPage
  updateQueryParams()
  fetchClasses()
}

function handlePageSizeChange(newPageSize: number) {
  pagination.value.perPage = newPageSize
  pagination.value.page = 1
  updateQueryParams()
  fetchClasses()
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
  router.push({ name: 'class_create' })
}

function goToView(id: number) {
  router.push({ name: 'class_view', params: { id } })
}

function goToEdit(id: number) {
  router.push({ name: 'class_edit', params: { id } })
}

function formatFee(amount: number): string {
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
        <h2 class="text-3xl font-bold tracking-tight">Classes</h2>
        <p class="text-muted-foreground">Manage school classes and grade levels.</p>
      </div>
      <Button @click="goToCreate">
        <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
        Add Class
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
              placeholder="Search by name or code..."
              class="max-w-sm"
            >
              <template #prefix>
                <iconify-icon icon="lucide:search" class="h-4 w-4 text-muted-foreground" />
              </template>
            </Input>
          </div>
        </div>
      </CardContent>
    </Card>

    <!-- Table -->
    <Card>
      <CardHeader>
        <CardTitle>Class List</CardTitle>
      </CardHeader>
      <CardContent>
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="classes.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:users" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No classes found</h3>
          <p class="text-sm text-muted-foreground">Get started by creating a new class.</p>
          <Button class="mt-4" @click="goToCreate">
            <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
            Add Class
          </Button>
        </div>

        <!-- Data Table -->
        <div v-else class="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Code</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Grade</TableHead>
                <TableHead>Section</TableHead>
                <TableHead class="text-right">Capacity</TableHead>
                <TableHead class="text-right">Fee</TableHead>
                <TableHead>Status</TableHead>
                <TableHead class="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="cls in classes" :key="cls.id">
                <TableCell class="font-medium">{{ cls.code }}</TableCell>
                <TableCell>{{ cls.name }}</TableCell>
                <TableCell>{{ cls.gradeName }}</TableCell>
                <TableCell>{{ cls.section || '-' }}</TableCell>
                <TableCell class="text-right">{{ cls.capacity }}</TableCell>
                <TableCell class="text-right">{{ formatFee(cls.feeAmount) }}</TableCell>
                <TableCell>
                  <Badge :variant="cls.isActive ? 'default' : 'secondary'">
                    {{ cls.isActive ? 'Active' : 'Inactive' }}
                  </Badge>
                </TableCell>
                <TableCell class="text-right">
                  <div class="flex justify-end gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToView(cls.id)"
                      title="View"
                    >
                      <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToEdit(cls.id)"
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
        <div v-if="!isLoading && classes.length > 0" class="mt-4 flex justify-end">
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
