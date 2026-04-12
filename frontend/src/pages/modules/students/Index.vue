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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import Pagination from '@/components/templates/Pagination.vue'

const sidebar = useSidebarStore()
const router = useRouter()
const route = useRoute()

interface Grade {
  id: number
  name: string
  code: string
}

interface Class {
  id: number
  name: string
  gradeId: number
}

interface Student {
  id: number
  studentId: string
  name: string
  classId: number | null
  className: string | null
  gradeId: number | null
  gradeName: string | null
  email: string | null
  phone: string | null
  isActive: boolean
  balance: number
  createdAt: string
  updatedAt: string
}

interface PaginationMeta {
  total: number
  page: number
  perPage: number
  lastPage: number
}

const students = ref<Student[]>([])
const grades = ref<Grade[]>([])
const classes = ref<Class[]>([])
const pagination = ref<PaginationMeta>({
  total: 0,
  page: 1,
  perPage: 15,
  lastPage: 1,
})
const isLoading = ref(true)
const isLoadingFilters = ref(false)
const error = ref<string | null>(null)
const showImportDialog = ref(false)
const isImporting = ref(false)
const importFile = ref<File | null>(null)
const fileInputRef = ref<HTMLInputElement | null>(null)

// Filters
const searchQuery = ref('')
const selectedGradeId = ref<string>('all')
const selectedClassId = ref<string>('all')

onMounted(async () => {
  sidebar.setPageName('Students')
  // Get initial values from query params
  const page = parseInt(route.query.page as string) || 1
  const perPage = parseInt(route.query.perPage as string) || 15
  pagination.value.page = page
  pagination.value.perPage = perPage
  
  if (route.query.search) searchQuery.value = route.query.search as string
  if (route.query.gradeId) selectedGradeId.value = route.query.gradeId as string
  if (route.query.classId) selectedClassId.value = route.query.classId as string
  
  await fetchGrades()
  await fetchClasses()
  await fetchStudents()
})

async function fetchGrades() {
  try {
    const response = await api.getGrades({ perPage: 100 }) as any
    grades.value = response.data ?? []
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load grades' })
  }
}

async function fetchClasses() {
  try {
    const params: any = { perPage: 100 }
    if (selectedGradeId.value && selectedGradeId.value !== 'all') {
      params.gradeId = selectedGradeId.value
    }
    const response = await api.getClasses(params) as any
    classes.value = response.data ?? []
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load classes' })
  }
}

async function fetchStudents() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {
      page: pagination.value.page,
      perPage: pagination.value.perPage,
    }

    if (searchQuery.value) params.search = searchQuery.value
    if (selectedGradeId.value && selectedGradeId.value !== 'all') params.gradeId = selectedGradeId.value
    if (selectedClassId.value && selectedClassId.value !== 'all') params.classId = selectedClassId.value

    const response = await api.getStudents(params) as any
    students.value = response.data ?? []
    pagination.value = response.meta ?? pagination.value
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load students'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

function handlePageChange(newPage: number) {
  pagination.value.page = newPage
  updateQueryParams()
  fetchStudents()
}

function handlePageSizeChange(newPageSize: number) {
  pagination.value.perPage = newPageSize
  pagination.value.page = 1
  updateQueryParams()
  fetchStudents()
}

function updateQueryParams() {
  const query: any = {
    page: pagination.value.page.toString(),
    perPage: pagination.value.perPage.toString(),
  }
  if (searchQuery.value) query.search = searchQuery.value
  if (selectedGradeId.value && selectedGradeId.value !== 'all') query.gradeId = selectedGradeId.value
  if (selectedClassId.value && selectedClassId.value !== 'all') query.classId = selectedClassId.value
  
  router.replace({ query })
}

function applyFilters() {
  pagination.value.page = 1
  updateQueryParams()
  fetchStudents()
}

function resetFilters() {
  searchQuery.value = ''
  selectedGradeId.value = 'all'
  selectedClassId.value = 'all'
  pagination.value.page = 1
  updateQueryParams()
  fetchStudents()
}

// Watch for grade change to reload classes
watch(selectedGradeId, async () => {
  selectedClassId.value = 'all'
  await fetchClasses()
})

function goToCreate() {
  router.push({ name: 'student_create' })
}

function goToView(id: number) {
  router.push({ name: 'student_view', params: { id } })
}

function goToEdit(id: number) {
  router.push({ name: 'student_edit', params: { id } })
}

function handleFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    importFile.value = target.files[0]
  }
}

async function importStudents() {
  if (!importFile.value) {
    toast.error('Validation Error', { description: 'Please select a CSV file' })
    return
  }

  try {
    isImporting.value = true
    const formData = new FormData()
    formData.append('file', importFile.value)

    const response = await api.importStudents(formData) as any
    const { imported, skipped, errors } = response
    
    toast.success('Import Complete', {
      description: `Imported: ${imported}, Skipped: ${skipped}`,
    })
    
    if (errors && errors.length > 0) {
      errors.forEach((err: string) => toast.error('Import Error', { description: err }))
    }
    
    showImportDialog.value = false
    importFile.value = null
    if (fileInputRef.value) fileInputRef.value.value = ''
    await fetchStudents()
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to import students'
    toast.error('Error', { description: message || undefined })
  } finally {
    isImporting.value = false
  }
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
        <h2 class="text-3xl font-bold tracking-tight">Students</h2>
        <p class="text-muted-foreground">Manage student records and class assignments.</p>
      </div>
      <div class="flex gap-2">
        <Dialog v-model:open="showImportDialog">
          <DialogTrigger as-child>
            <Button variant="outline">
              <iconify-icon icon="lucide:upload" class="mr-2 h-4 w-4" />
              Import CSV
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Import Students</DialogTitle>
              <DialogDescription>
                Upload a CSV file with student data. Required columns: StudentId, Name
              </DialogDescription>
            </DialogHeader>
            <div class="py-4">
              <Input
                ref="fileInputRef"
                type="file"
                accept=".csv"
                @change="handleFileChange"
              />
              <p class="mt-2 text-sm text-muted-foreground">
                Optional columns: ClassId, GradeId, Email, Phone, Address, IsActive
              </p>
            </div>
            <DialogFooter>
              <Button variant="outline" @click="showImportDialog = false">Cancel</Button>
              <Button @click="importStudents" :disabled="!importFile || isImporting">
                <iconify-icon v-if="isImporting" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
                {{ isImporting ? 'Importing...' : 'Import' }}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
        <Button @click="goToCreate">
          <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
          Add Student
        </Button>
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
              placeholder="Search by name or student ID..."
              @keyup.enter="applyFilters"
            >
              <template #prefix>
                <iconify-icon icon="lucide:search" class="h-4 w-4 text-muted-foreground" />
              </template>
            </Input>
          </div>
          <div class="w-full md:w-48">
            <Label class="mb-2 block text-sm font-medium">Grade</Label>
            <Select v-model="selectedGradeId">
              <SelectTrigger>
                <SelectValue placeholder="All Grades" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Grades</SelectItem>
                <SelectItem
                  v-for="grade in grades"
                  :key="grade.id"
                  :value="String(grade.id)"
                >
                  {{ grade.name }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="w-full md:w-48">
            <Label class="mb-2 block text-sm font-medium">Class</Label>
            <Select v-model="selectedClassId" :disabled="selectedGradeId === 'all'">
              <SelectTrigger>
                <SelectValue :placeholder="selectedGradeId !== 'all' ? 'All Classes' : 'Select grade first'" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Classes</SelectItem>
                <SelectItem 
                  v-for="cls in classes" 
                  :key="cls.id" 
                  :value="String(cls.id)"
                >
                  {{ cls.name }}
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
        <CardTitle>Student List</CardTitle>
      </CardHeader>
      <CardContent>
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="students.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:graduation-cap" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No students found</h3>
          <p class="text-sm text-muted-foreground">Get started by creating a new student or importing from CSV.</p>
          <div class="flex gap-2 mt-4">
            <Button variant="outline" @click="showImportDialog = true">
              <iconify-icon icon="lucide:upload" class="mr-2 h-4 w-4" />
              Import CSV
            </Button>
            <Button @click="goToCreate">
              <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
              Add Student
            </Button>
          </div>
        </div>

        <!-- Data Table -->
        <div v-else class="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Student ID</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Grade</TableHead>
                <TableHead>Class</TableHead>
                <TableHead>Email</TableHead>
                <TableHead class="text-right">Balance</TableHead>
                <TableHead>Status</TableHead>
                <TableHead class="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="student in students" :key="student.id">
                <TableCell class="font-medium">{{ student.studentId }}</TableCell>
                <TableCell>{{ student.name }}</TableCell>
                <TableCell>{{ student.gradeName || '-' }}</TableCell>
                <TableCell>{{ student.className || '-' }}</TableCell>
                <TableCell>{{ student.email || '-' }}</TableCell>
                <TableCell class="text-right" :class="student.balance < 0 ? 'text-destructive' : ''">
                  {{ formatBalance(student.balance) }}
                </TableCell>
                <TableCell>
                  <Badge :variant="student.isActive ? 'default' : 'secondary'">
                    {{ student.isActive ? 'Active' : 'Inactive' }}
                  </Badge>
                </TableCell>
                <TableCell class="text-right">
                  <div class="flex justify-end gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToView(student.id)"
                      title="View"
                    >
                      <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      @click="goToEdit(student.id)"
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
        <div v-if="!isLoading && students.length > 0" class="mt-4 flex justify-end">
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
