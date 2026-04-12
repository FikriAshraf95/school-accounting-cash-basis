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

const classId = Number(route.params.id)

interface Student {
  id: number
  studentId: string
  name: string
  email: string | null
  phone: string | null
}

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
  students: Student[]
  createdAt: string
  updatedAt: string
}

interface AvailableStudent {
  id: number
  studentId: string
  name: string
}

const cls = ref<Class | null>(null)
const availableStudents = ref<AvailableStudent[]>([])
const selectedStudentId = ref<string>('')
const isLoading = ref(true)
const isDeleting = ref(false)
const isAssigning = ref(false)
const isRemoving = ref(false)
const error = ref<string | null>(null)
const showAssignDialog = ref(false)
const studentToRemove = ref<number | null>(null)

onMounted(async () => {
  sidebar.setPageName('View Class')
  await fetchClass()
})

async function fetchClass() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getClass(classId) as any
    cls.value = response
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load class'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function fetchAvailableStudents() {
  try {
    const response = await api.getStudents({ perPage: 1000 }) as any
    // Filter out students already in this class
    const enrolledIds = cls.value?.students.map(s => s.id) || []
    availableStudents.value = (response.data ?? []).filter(
      (s: any) => !enrolledIds.includes(s.id)
    )
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load available students' })
  }
}

async function openAssignDialog() {
  await fetchAvailableStudents()
  selectedStudentId.value = ''
  showAssignDialog.value = true
}

async function assignStudent() {
  if (!selectedStudentId.value) {
    toast.error('Validation Error', { description: 'Please select a student' })
    return
  }

  try {
    isAssigning.value = true
    await api.addStudentToClass(classId, { studentId: Number(selectedStudentId.value) })
    toast.success('Success', { description: 'Student assigned to class successfully' })
    showAssignDialog.value = false
    await fetchClass()
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to assign student'
    toast.error('Error', { description: message || undefined })
  } finally {
    isAssigning.value = false
  }
}

async function removeStudent() {
  if (!studentToRemove.value) return

  try {
    isRemoving.value = true
    await api.removeStudentFromClass(classId, studentToRemove.value)
    toast.success('Success', { description: 'Student removed from class successfully' })
    studentToRemove.value = null
    await fetchClass()
  } catch (err: any) {
    const message = err.response?.data?.detail || 'Failed to remove student'
    toast.error('Error', { description: message })
  } finally {
    isRemoving.value = false
  }
}

async function deleteClass() {
  try {
    isDeleting.value = true
    await api.deleteClass(classId)
    toast.success('Success', { description: 'Class deleted successfully' })
    router.push({ name: 'classes_list' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to delete class'
    toast.error('Error', { description: message || undefined })
    isDeleting.value = false
  }
}

function goToEdit() {
  router.push({ name: 'class_edit', params: { id: classId } })
}

function goBack() {
  router.push({ name: 'classes_list' })
}

function goToStudent(studentId: number) {
  router.push({ name: 'student_view', params: { id: studentId } })
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
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Class Details</h2>
        <p class="text-muted-foreground">View class information and enrolled students</p>
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
              <AlertDialogTitle>Delete Class</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to delete this class? This action cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction @click="deleteClass" :disabled="isDeleting">
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
    <template v-else-if="cls">
      <!-- Class Info Card -->
      <Card>
        <CardHeader>
          <div class="flex items-start justify-between">
            <div>
              <CardTitle class="text-2xl">{{ cls.name }}</CardTitle>
              <CardDescription>Code: {{ cls.code }} | Grade: {{ cls.gradeName }}</CardDescription>
            </div>
            <Badge :variant="cls.isActive ? 'default' : 'secondary'">
              {{ cls.isActive ? 'Active' : 'Inactive' }}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Section</dt>
              <dd class="mt-1 text-base">{{ cls.section || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Capacity</dt>
              <dd class="mt-1 text-base">{{ cls.students.length }} / {{ cls.capacity }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Fee Amount</dt>
              <dd class="mt-1 text-base">{{ formatFee(cls.feeAmount) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Class ID</dt>
              <dd class="mt-1 text-base">#{{ cls.id }}</dd>
            </div>
            <div class="sm:col-span-2 lg:col-span-4">
              <dt class="text-sm font-medium text-muted-foreground">Description</dt>
              <dd class="mt-1 text-base">{{ cls.description || '-' }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <!-- Enrolled Students Card -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Enrolled Students</CardTitle>
            <CardDescription>Students currently enrolled in this class</CardDescription>
          </div>
          <Dialog v-model:open="showAssignDialog">
            <DialogTrigger as-child>
              <Button @click="openAssignDialog">
                <iconify-icon icon="lucide:user-plus" class="mr-2 h-4 w-4" />
                Assign Student
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Assign Student to Class</DialogTitle>
                <DialogDescription>
                  Select a student to add to {{ cls.name }}
                </DialogDescription>
              </DialogHeader>
              <div class="py-4">
                <Select v-model="selectedStudentId">
                  <SelectTrigger>
                    <SelectValue placeholder="Select a student" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem 
                      v-for="student in availableStudents" 
                      :key="student.id" 
                      :value="String(student.id)"
                    >
                      {{ student.studentId }} - {{ student.name }}
                    </SelectItem>
                  </SelectContent>
                </Select>
                <p v-if="availableStudents.length === 0" class="mt-2 text-sm text-muted-foreground">
                  No available students to assign.
                </p>
              </div>
              <DialogFooter>
                <Button variant="outline" @click="showAssignDialog = false">Cancel</Button>
                <Button @click="assignStudent" :disabled="!selectedStudentId || isAssigning">
                  <iconify-icon v-if="isAssigning" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
                  {{ isAssigning ? 'Assigning...' : 'Assign' }}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </CardHeader>
        <CardContent>
          <!-- Empty State -->
          <div v-if="cls.students.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
            <iconify-icon icon="lucide:users" class="h-12 w-12 text-muted-foreground mb-4" />
            <h3 class="text-lg font-semibold">No students enrolled</h3>
            <p class="text-sm text-muted-foreground">Assign students to this class to get started.</p>
          </div>

          <!-- Students Table -->
          <div v-else class="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Student ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Phone</TableHead>
                  <TableHead class="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="student in cls.students" :key="student.id">
                  <TableCell class="font-medium">{{ student.studentId }}</TableCell>
                  <TableCell>{{ student.name }}</TableCell>
                  <TableCell>{{ student.email || '-' }}</TableCell>
                  <TableCell>{{ student.phone || '-' }}</TableCell>
                  <TableCell class="text-right">
                    <div class="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="icon"
                        @click="goToStudent(student.id)"
                        title="View Student"
                      >
                        <iconify-icon icon="lucide:eye" class="h-4 w-4" />
                      </Button>
                      <AlertDialog>
                        <AlertDialogTrigger as-child>
                          <Button
                            variant="ghost"
                            size="icon"
                            @click="studentToRemove = student.id"
                            title="Remove from Class"
                          >
                            <iconify-icon icon="lucide:user-minus" class="h-4 w-4 text-destructive" />
                          </Button>
                        </AlertDialogTrigger>
                        <AlertDialogContent>
                          <AlertDialogHeader>
                            <AlertDialogTitle>Remove Student</AlertDialogTitle>
                            <AlertDialogDescription>
                              Are you sure you want to remove {{ student.name }} from this class?
                            </AlertDialogDescription>
                          </AlertDialogHeader>
                          <AlertDialogFooter>
                            <AlertDialogCancel @click="studentToRemove = null">Cancel</AlertDialogCancel>
                            <AlertDialogAction @click="removeStudent" :disabled="isRemoving">
                              <iconify-icon v-if="isRemoving" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
                              {{ isRemoving ? 'Removing...' : 'Remove' }}
                            </AlertDialogAction>
                          </AlertDialogFooter>
                        </AlertDialogContent>
                      </AlertDialog>
                    </div>
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
              <dd class="mt-1 text-sm">{{ new Date(cls.createdAt).toLocaleString() }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Last Updated</dt>
              <dd class="mt-1 text-sm">{{ new Date(cls.updatedAt).toLocaleString() }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </template>
  </div>
</template>
