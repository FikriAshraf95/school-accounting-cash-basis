<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
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
const classId = computed(() => Number(route.params.id))

interface Grade {
  id: number
  name: string
  code: string
}

const formData = ref({
  name: '',
  code: '',
  gradeId: '',
  section: '',
  description: '',
  capacity: 30,
  feeAmount: 0,
  isActive: true,
})

const grades = ref<Grade[]>([])
const isLoading = ref(false)
const isSaving = ref(false)
const isLoadingGrades = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName(isEditing.value ? 'Edit Class' : 'Create Class')
  await fetchGrades()
  if (isEditing.value) {
    await fetchClass()
  }
})

async function fetchGrades() {
  try {
    isLoadingGrades.value = true
    const response = await api.getGrades({ perPage: 100 }) as any
    grades.value = response.data.data
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load grades' })
  } finally {
    isLoadingGrades.value = false
  }
}

async function fetchClass() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getClass(classId.value) as any
    const cls = response.data
    formData.value = {
      name: cls.name,
      code: cls.code,
      gradeId: String(cls.gradeId),
      section: cls.section || '',
      description: cls.description || '',
      capacity: cls.capacity,
      feeAmount: cls.feeAmount,
      isActive: cls.isActive,
    }
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load class'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function saveClass() {
  // Validation
  if (!formData.value.name) {
    toast.error('Validation Error', { description: 'Name is required' })
    return
  }
  if (!formData.value.code) {
    toast.error('Validation Error', { description: 'Code is required' })
    return
  }
  if (!formData.value.gradeId) {
    toast.error('Validation Error', { description: 'Grade is required' })
    return
  }

  try {
    isSaving.value = true
    const payload = {
      ...formData.value,
      gradeId: Number(formData.value.gradeId),
      section: formData.value.section || null,
      description: formData.value.description || null,
    }

    if (isEditing.value) {
      await api.updateClass(classId.value, payload)
      toast.success('Success', { description: 'Class updated successfully' })
    } else {
      await api.createClass(payload)
      toast.success('Success', { description: 'Class created successfully' })
    }
    router.push({ name: 'classes_list' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || `Failed to ${isEditing.value ? 'update' : 'create'} class`
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'classes_list' })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">
          {{ isEditing ? 'Edit Class' : 'Create Class' }}
        </h2>
        <p class="text-muted-foreground">
          {{ isEditing ? 'Update the class details.' : 'Add a new class to the school.' }}
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
    <form v-else @submit.prevent="saveClass" class="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Class Details</CardTitle>
          <CardDescription>Enter the class information</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="name">Name <span class="text-red-500">*</span></Label>
              <Input
                id="name"
                v-model="formData.name"
                placeholder="e.g., Class 1A"
                required
              />
            </div>
            <div class="space-y-2">
              <Label for="code">Code <span class="text-red-500">*</span></Label>
              <Input
                id="code"
                v-model="formData.code"
                placeholder="e.g., C1A"
                required
              />
            </div>
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="grade">Grade <span class="text-red-500">*</span></Label>
              <Select v-model="formData.gradeId" :disabled="isLoadingGrades">
                <SelectTrigger>
                  <SelectValue :placeholder="isLoadingGrades ? 'Loading...' : 'Select grade'" />
                </SelectTrigger>
                <SelectContent>
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
              <Label for="section">Section</Label>
              <Input
                id="section"
                v-model="formData.section"
                placeholder="e.g., A"
              />
            </div>
          </div>

          <div class="space-y-2">
            <Label for="description">Description</Label>
            <Input
              id="description"
              v-model="formData.description"
              placeholder="Enter description"
            />
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="capacity">Capacity</Label>
              <Input
                id="capacity"
                type="number"
                v-model="formData.capacity"
                placeholder="30"
                min="1"
              />
            </div>
            <div class="space-y-2">
              <Label for="feeAmount">Fee Amount (MYR)</Label>
              <Input
                id="feeAmount"
                type="number"
                step="0.01"
                v-model="formData.feeAmount"
                placeholder="0.00"
                min="0"
              />
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
          {{ isSaving ? 'Saving...' : (isEditing ? 'Update Class' : 'Create Class') }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
