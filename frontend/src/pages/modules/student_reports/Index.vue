<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { isCancel } from '@/services/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
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
import DatePicker from '@/components/templates/DatePicker.vue'

const sidebar = useSidebarStore()

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

interface StudentReport {
  id: number
  studentId: string
  name: string
  className: string | null
  gradeName: string | null
  balance: number
  totalIncome: number
  totalExpense: number
  transactionCount: number
}

const reportData = ref<StudentReport[]>([])
const grades = ref<Grade[]>([])
const classes = ref<Class[]>([])
const isLoading = ref(true)
const isLoadingFilters = ref(false)
const error = ref<string | null>(null)

// Filters
const selectedGradeId = ref<string>('all')
const selectedClassId = ref<string>('all')
const dateFrom = ref<Date | null>(null)
const dateTo = ref<Date | null>(null)

onMounted(async () => {
  sidebar.setPageName('Student Reports')
  await fetchGrades()
  await fetchReport()
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
  if (!selectedGradeId.value || selectedGradeId.value === 'all') {
    classes.value = []
    return
  }
  try {
    isLoadingFilters.value = true
    const response = await api.getClasses({
      perPage: 100,
      gradeId: Number(selectedGradeId.value)
    }) as any
    classes.value = response.data ?? []
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load classes' })
  } finally {
    isLoadingFilters.value = false
  }
}

async function fetchReport() {
  try {
    isLoading.value = true
    error.value = null

    const params: any = {}

    if (selectedGradeId.value && selectedGradeId.value !== 'all') params.gradeId = selectedGradeId.value
    if (selectedClassId.value && selectedClassId.value !== 'all') params.classId = selectedClassId.value
    if (dateFrom.value) params.dateFrom = dateFrom.value.toISOString()
    if (dateTo.value) params.dateTo = dateTo.value.toISOString()

    const response = await api.getStudentsReport(params) as any
    reportData.value = response ?? []
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load student report'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

// Watch for grade change to reload classes and clear selected class
watch(selectedGradeId, async () => {
  selectedClassId.value = 'all'
  await fetchClasses()
})

function applyFilters() {
  fetchReport()
}

function resetFilters() {
  selectedGradeId.value = 'all'
  selectedClassId.value = 'all'
  dateFrom.value = null
  dateTo.value = null
  fetchReport()
}

function formatAmount(amount: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(amount)
}

// Calculate totals
const totals = computed(() => {
  return reportData.value.reduce(
    (acc, student) => ({
      count: acc.count + 1,
      totalIncome: acc.totalIncome + student.totalIncome,
      totalExpense: acc.totalExpense + student.totalExpense,
      totalBalance: acc.totalBalance + student.balance,
      totalTransactions: acc.totalTransactions + student.transactionCount,
    }),
    { count: 0, totalIncome: 0, totalExpense: 0, totalBalance: 0, totalTransactions: 0 }
  )
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Student Reports</h2>
        <p class="text-muted-foreground">View financial summary reports for students.</p>
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
            <Select v-model="selectedClassId" :disabled="selectedGradeId === 'all' || isLoadingFilters">
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
          <div class="w-full md:w-48">
            <Label class="mb-2 block text-sm font-medium">Date From</Label>
            <DatePicker v-model="dateFrom" placeholder="Start date" />
          </div>
          <div class="w-full md:w-48">
            <Label class="mb-2 block text-sm font-medium">Date To</Label>
            <DatePicker v-model="dateTo" placeholder="End date" />
          </div>
          <div class="flex gap-2">
            <Button variant="secondary" @click="applyFilters">
              <iconify-icon icon="lucide:filter" class="mr-2 h-4 w-4" />
              Generate
            </Button>
            <Button variant="outline" @click="resetFilters">
              <iconify-icon icon="lucide:rotate-ccw" class="mr-2 h-4 w-4" />
              Reset
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>

    <!-- Summary Cards -->
    <div v-if="!isLoading && reportData.length > 0" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
      <Card>
        <CardHeader class="pb-2">
          <CardTitle class="text-sm font-medium text-muted-foreground">Total Students</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ totals.count }}</div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="pb-2">
          <CardTitle class="text-sm font-medium text-muted-foreground">Total Income</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-green-600">{{ formatAmount(totals.totalIncome) }}</div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="pb-2">
          <CardTitle class="text-sm font-medium text-muted-foreground">Total Expense</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-red-600">{{ formatAmount(totals.totalExpense) }}</div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="pb-2">
          <CardTitle class="text-sm font-medium text-muted-foreground">Net Balance</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold" :class="totals.totalBalance >= 0 ? 'text-green-600' : 'text-red-600'">
            {{ formatAmount(totals.totalBalance) }}
          </div>
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="pb-2">
          <CardTitle class="text-sm font-medium text-muted-foreground">Transactions</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ totals.totalTransactions }}</div>
        </CardContent>
      </Card>
    </div>

    <!-- Table -->
    <Card>
      <CardHeader>
        <CardTitle>Student Financial Report</CardTitle>
      </CardHeader>
      <CardContent>
        <!-- Loading State -->
        <div v-if="isLoading" class="space-y-4">
          <Skeleton v-for="i in 5" :key="i" class="h-12 w-full" />
        </div>

        <!-- Empty State -->
        <div v-else-if="reportData.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
          <iconify-icon icon="lucide:file-text" class="h-12 w-12 text-muted-foreground mb-4" />
          <h3 class="text-lg font-semibold">No data found</h3>
          <p class="text-sm text-muted-foreground">Try adjusting your filters to see more results.</p>
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
                <TableHead class="text-right">Total Income</TableHead>
                <TableHead class="text-right">Total Expense</TableHead>
                <TableHead class="text-right">Balance</TableHead>
                <TableHead class="text-right">Transactions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="student in reportData" :key="student.id">
                <TableCell class="font-medium">{{ student.studentId }}</TableCell>
                <TableCell>{{ student.name }}</TableCell>
                <TableCell>{{ student.gradeName || '-' }}</TableCell>
                <TableCell>{{ student.className || '-' }}</TableCell>
                <TableCell class="text-right text-green-600">
                  {{ formatAmount(student.totalIncome) }}
                </TableCell>
                <TableCell class="text-right text-red-600">
                  {{ formatAmount(student.totalExpense) }}
                </TableCell>
                <TableCell class="text-right" :class="student.balance < 0 ? 'text-red-600 font-semibold' : 'text-green-600'">
                  {{ formatAmount(student.balance) }}
                </TableCell>
                <TableCell class="text-right">
                  <Badge variant="outline">{{ student.transactionCount }}</Badge>
                </TableCell>
              </TableRow>
              <!-- Totals Row -->
              <TableRow class="bg-muted/50 font-semibold">
                <TableCell colspan="4" class="text-right">Total:</TableCell>
                <TableCell class="text-right text-green-600">{{ formatAmount(totals.totalIncome) }}</TableCell>
                <TableCell class="text-right text-red-600">{{ formatAmount(totals.totalExpense) }}</TableCell>
                <TableCell class="text-right" :class="totals.totalBalance < 0 ? 'text-red-600' : 'text-green-600'">
                  {{ formatAmount(totals.totalBalance) }}
                </TableCell>
                <TableCell class="text-right">{{ totals.totalTransactions }}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
