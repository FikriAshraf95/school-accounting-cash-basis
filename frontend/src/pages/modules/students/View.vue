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
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

const sidebar = useSidebarStore()
const route = useRoute()
const router = useRouter()

const studentId = Number(route.params.id)

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
  address: string | null
  balance: number
  isActive: boolean
  transactionHistory: Transaction[]
  createdAt: string
  updatedAt: string
}

interface Class {
  id: number
  name: string
  gradeName: string
}

const student = ref<Student | null>(null)
const availableClasses = ref<Class[]>([])
const selectedClassId = ref<string>('')
const isLoading = ref(true)
const isDeleting = ref(false)
const isAssigning = ref(false)
const error = ref<string | null>(null)
const showAssignDialog = ref(false)

onMounted(async () => {
  sidebar.setPageName('View Student')
  await fetchStudent()
})

async function fetchStudent() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getStudent(studentId) as any
    student.value = response
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load student'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function fetchAvailableClasses() {
  try {
    const response = await api.getClasses({ perPage: 1000 }) as any
    availableClasses.value = (response ?? []).map((c: any) => ({
      id: c.id,
      name: c.name,
      gradeName: c.gradeName,
    }))
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load available classes' })
  }
}

async function openAssignDialog() {
  await fetchAvailableClasses()
  selectedClassId.value = student.value?.classId ? String(student.value.classId) : 'none'
  showAssignDialog.value = true
}

async function assignToClass() {
  try {
    isAssigning.value = true
    const classId = selectedClassId.value && selectedClassId.value !== 'none' ? Number(selectedClassId.value) : null
    await api.assignStudentToClass(studentId, { classId })
    toast.success('Success', { description: 'Student assigned to class successfully' })
    showAssignDialog.value = false
    await fetchStudent()
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to assign student to class'
    toast.error('Error', { description: message || undefined })
  } finally {
    isAssigning.value = false
  }
}

async function deleteStudent() {
  try {
    isDeleting.value = true
    await api.deleteStudent(studentId)
    toast.success('Success', { description: 'Student deleted successfully' })
    router.push({ name: 'students_list' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to delete student'
    toast.error('Error', { description: message || undefined })
    isDeleting.value = false
  }
}

function goToEdit() {
  router.push({ name: 'student_edit', params: { id: studentId } })
}

function goBack() {
  router.push({ name: 'students_list' })
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

function formatAmount(amount: number, type: string): string {
  const formatted = new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(amount)
  return formatted
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Student Details</h2>
        <p class="text-muted-foreground">View student information and transaction history</p>
      </div>
      <div class="flex gap-2">
        <Button variant="outline" @click="goBack">
          <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
          Back
        </Button>
        <Button v-if="!isLoading && !error" variant="default" @click="router.push({ name: 'quick_payment', params: { id: studentId } })">
          <iconify-icon icon="lucide:credit-card" class="mr-2 h-4 w-4" />
          Quick Payment
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
              <AlertDialogTitle>Delete Student</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to delete this student? This action cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction @click="deleteStudent" :disabled="isDeleting">
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
    <template v-else-if="student">
      <!-- Student Info Card -->
      <Card>
        <CardHeader>
          <div class="flex items-start justify-between">
            <div>
              <CardTitle class="text-2xl">{{ student.name }}</CardTitle>
              <CardDescription>Student ID: {{ student.studentId }}</CardDescription>
            </div>
            <Badge :variant="student.isActive ? 'default' : 'secondary'">
              {{ student.isActive ? 'Active' : 'Inactive' }}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Grade</dt>
              <dd class="mt-1 text-base">{{ student.gradeName || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Class</dt>
              <dd class="mt-1 text-base">{{ student.className || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Email</dt>
              <dd class="mt-1 text-base">{{ student.email || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Phone</dt>
              <dd class="mt-1 text-base">{{ student.phone || '-' }}</dd>
            </div>
            <div class="sm:col-span-2">
              <dt class="text-sm font-medium text-muted-foreground">Address</dt>
              <dd class="mt-1 text-base">{{ student.address || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Balance</dt>
              <dd class="mt-1 text-base" :class="student.balance < 0 ? 'text-destructive font-semibold' : ''">
                {{ formatAmount(student.balance, 'balance') }}
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Student ID</dt>
              <dd class="mt-1 text-base">#{{ student.id }}</dd>
            </div>
          </dl>

          <!-- Assign to Class Button -->
          <div class="mt-6 flex gap-2">
            <Dialog v-model:open="showAssignDialog">
              <DialogTrigger as-child>
                <Button variant="outline">
                  <iconify-icon icon="lucide:users" class="mr-2 h-4 w-4" />
                  {{ student.classId ? 'Change Class' : 'Assign to Class' }}
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Assign Student to Class</DialogTitle>
                  <DialogDescription>
                    Select a class for {{ student.name }}
                  </DialogDescription>
                </DialogHeader>
                <div class="py-4">
                  <Select v-model="selectedClassId">
                    <SelectTrigger>
                      <SelectValue placeholder="Select a class" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">No Class</SelectItem>
                      <SelectItem 
                        v-for="cls in availableClasses" 
                        :key="cls.id" 
                        :value="String(cls.id)"
                      >
                        {{ cls.gradeName }} - {{ cls.name }}
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <DialogFooter>
                  <Button variant="outline" @click="showAssignDialog = false">Cancel</Button>
                  <Button @click="assignToClass" :disabled="isAssigning">
                    <iconify-icon v-if="isAssigning" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
                    {{ isAssigning ? 'Assigning...' : 'Assign' }}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        </CardContent>
      </Card>

      <!-- Transaction History Card -->
      <Card>
        <CardHeader>
          <CardTitle>Transaction History</CardTitle>
          <CardDescription>Recent financial transactions for this student</CardDescription>
        </CardHeader>
        <CardContent>
          <!-- Empty State -->
          <div v-if="student.transactionHistory.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
            <iconify-icon icon="lucide:receipt" class="h-12 w-12 text-muted-foreground mb-4" />
            <h3 class="text-lg font-semibold">No transactions</h3>
            <p class="text-sm text-muted-foreground">This student has no recorded transactions yet.</p>
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
                <TableRow v-for="txn in student.transactionHistory" :key="txn.id">
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
                    {{ formatAmount(txn.amount, txn.type) }}
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
              <dd class="mt-1 text-sm">{{ formatDateTime(student.createdAt) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Last Updated</dt>
              <dd class="mt-1 text-sm">{{ formatDateTime(student.updatedAt) }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </template>
  </div>
</template>
