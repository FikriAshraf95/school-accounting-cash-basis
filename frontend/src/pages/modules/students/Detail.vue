<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Switch } from '@/components/ui/switch'
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

const isEditing = computed(() => !!route.params.id)
const studentId = computed(() => Number(route.params.id))

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

const formData = ref({
  studentId: '',
  name: '',
  email: '',
  phone: '',
  address: '',
  gradeId: 'none',
  classId: 'none',
  isActive: true,
})

const grades = ref<Grade[]>([])
const classes = ref<Class[]>([])
const isLoading = ref(false)
const isSaving = ref(false)
const isLoadingGrades = ref(false)
const isLoadingClasses = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName(isEditing.value ? 'Edit Student' : 'Create Student')
  await fetchGrades()
  if (isEditing.value) {
    await fetchStudent()
  }
})

async function fetchGrades() {
  try {
    isLoadingGrades.value = true
    const response = await api.getGrades({ perPage: 100 }) as any
    grades.value = response.data ?? []
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load grades' })
  } finally {
    isLoadingGrades.value = false
  }
}

async function fetchClasses() {
  if (!formData.value.gradeId || formData.value.gradeId === 'none') {
    classes.value = []
    return
  }
  try {
    isLoadingClasses.value = true
    const response = await api.getClasses({
      perPage: 100,
      gradeId: Number(formData.value.gradeId)
    }) as any
    classes.value = response.data ?? []
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load classes' })
  } finally {
    isLoadingClasses.value = false
  }
}

async function fetchStudent() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getStudent(studentId.value) as any
    const student = response
    formData.value = {
      studentId: student.studentId,
      name: student.name,
      email: student.email || '',
      phone: student.phone || '',
      address: student.address || '',
      gradeId: student.gradeId ? String(student.gradeId) : 'none',
      classId: student.classId ? String(student.classId) : 'none',
      isActive: student.isActive,
    }
    // Fetch classes for the selected grade
    await fetchClasses()
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load student'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

// Watch for grade change to reload classes and clear selected class
watch(() => formData.value.gradeId, async (newGradeId, oldGradeId) => {
  if (newGradeId !== oldGradeId) {
    formData.value.classId = 'none'
    await fetchClasses()
  }
})

async function saveStudent() {
  // Validation
  if (!formData.value.studentId) {
    toast.error('Validation Error', { description: 'Student ID is required' })
    return
  }
  if (!formData.value.name) {
    toast.error('Validation Error', { description: 'Name is required' })
    return
  }

  try {
    isSaving.value = true
    const payload = {
      ...formData.value,
      gradeId: formData.value.gradeId && formData.value.gradeId !== 'none' ? Number(formData.value.gradeId) : null,
      classId: formData.value.classId && formData.value.classId !== 'none' ? Number(formData.value.classId) : null,
      email: formData.value.email || null,
      phone: formData.value.phone || null,
      address: formData.value.address || null,
    }

    if (isEditing.value) {
      await api.updateStudent(studentId.value, payload)
      toast.success('Success', { description: 'Student updated successfully' })
    } else {
      await api.createStudent(payload)
      toast.success('Success', { description: 'Student created successfully' })
    }
    router.push({ name: 'students_list' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || `Failed to ${isEditing.value ? 'update' : 'create'} student`
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'students_list' })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">
          {{ isEditing ? 'Edit Student' : 'Create Student' }}
        </h2>
        <p class="text-muted-foreground">
          {{ isEditing ? 'Update the student details.' : 'Add a new student to the school.' }}
        </p>
      </div>
      <Button variant="outline" @click="goBack">
        <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
        Back
      </Button>
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
      <Skeleton class="h-96 w-full" />
    </div>

    <!-- Form -->
    <form v-else @submit.prevent="saveStudent" class="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Student Information</CardTitle>
          <CardDescription>Enter the student details</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="studentId">Student ID <span class="text-red-500">*</span></Label>
              <Input
                id="studentId"
                v-model="formData.studentId"
                placeholder="e.g., STU001"
                required
              />
            </div>
            <div class="space-y-2">
              <Label for="name">Name <span class="text-red-500">*</span></Label>
              <Input
                id="name"
                v-model="formData.name"
                placeholder="e.g., John Doe"
                required
              />
            </div>
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="email">Email</Label>
              <Input
                id="email"
                type="email"
                v-model="formData.email"
                placeholder="student@example.com"
              />
            </div>
            <div class="space-y-2">
              <Label for="phone">Phone</Label>
              <Input
                id="phone"
                v-model="formData.phone"
                placeholder="e.g., +60123456789"
              />
            </div>
          </div>

          <div class="space-y-2">
            <Label for="address">Address</Label>
            <Input
              id="address"
              v-model="formData.address"
              placeholder="Enter address"
            />
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="grade">Grade</Label>
              <Select v-model="formData.gradeId" :disabled="isLoadingGrades">
                <SelectTrigger>
                  <SelectValue :placeholder="isLoadingGrades ? 'Loading...' : 'Select grade'" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">No Grade</SelectItem>
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
            <div class="space-y-2">
              <Label for="class">Class</Label>
              <Select v-model="formData.classId" :disabled="formData.gradeId === 'none' || isLoadingClasses">
                <SelectTrigger>
                  <SelectValue :placeholder="isLoadingClasses ? 'Loading...' : (formData.gradeId !== 'none' ? 'Select class' : 'Select grade first')" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">No Class</SelectItem>
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
          </div>

          <div class="flex items-center gap-2 pt-4">
            <Switch
              id="isActive"
              v-model:checked="formData.isActive"
            />
            <Label for="isActive" class="cursor-pointer">Active</Label>
          </div>
        </CardContent>
      </Card>

      <!-- Actions -->
      <div class="flex gap-4">
        <Button type="submit" :disabled="isSaving">
          <iconify-icon v-if="isSaving" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
          <iconify-icon v-else icon="lucide:save" class="mr-2 h-4 w-4" />
          {{ isSaving ? 'Saving...' : (isEditing ? 'Update Student' : 'Create Student') }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
