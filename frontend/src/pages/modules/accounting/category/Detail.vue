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
const categoryId = computed(() => Number(route.params.id))

interface Ledger {
  id: number
  code: string
  name: string
  type: string
}

const formData = ref({
  name: '',
  type: '' as 'income' | 'expense' | '',
  ledgerId: '',
  description: '',
  requiresStudent: false,
  isActive: true,
})

const ledgers = ref<Ledger[]>([])
const filteredLedgers = computed(() => {
  if (!formData.value.type) return ledgers.value
  return ledgers.value.filter(l => {
    // Map category type to ledger type
    if (formData.value.type === 'income') return l.type === 'revenue'
    if (formData.value.type === 'expense') return l.type === 'expense'
    return true
  })
})

const isLoading = ref(false)
const isSaving = ref(false)
const isLoadingLedgers = ref(false)
const error = ref<string | null>(null)

const categoryTypes = [
  { value: 'income', label: 'Income' },
  { value: 'expense', label: 'Expense' },
]

onMounted(async () => {
  sidebar.setPageName(isEditing.value ? 'Edit Category' : 'Create Category')
  await fetchLedgers()
  if (isEditing.value) {
    await fetchCategory()
  }
})

async function fetchLedgers() {
  try {
    isLoadingLedgers.value = true
    const response = await api.getLedgers({ perPage: 100 }) as any
    ledgers.value = response.data ?? []
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load ledgers' })
  } finally {
    isLoadingLedgers.value = false
  }
}

async function fetchCategory() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getCategory(categoryId.value) as any
    const category = response
    formData.value = {
      name: category.name,
      type: category.type,
      ledgerId: String(category.ledgerId),
      description: category.description || '',
      requiresStudent: category.requiresStudent,
      isActive: category.isActive,
    }
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load category'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function saveCategory() {
  // Validation
  if (!formData.value.name) {
    toast.error('Validation Error', { description: 'Name is required' })
    return
  }
  if (!formData.value.type) {
    toast.error('Validation Error', { description: 'Type is required' })
    return
  }
  if (!formData.value.ledgerId) {
    toast.error('Validation Error', { description: 'Ledger is required' })
    return
  }

  try {
    isSaving.value = true
    const payload = {
      ...formData.value,
      ledgerId: Number(formData.value.ledgerId),
      description: formData.value.description || null,
    }

    if (isEditing.value) {
      await api.updateCategory(categoryId.value, payload)
      toast.success('Success', { description: 'Category updated successfully' })
    } else {
      await api.createCategory(payload)
      toast.success('Success', { description: 'Category created successfully' })
    }
    router.push({ name: 'ledger' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || `Failed to ${isEditing.value ? 'update' : 'create'} category`
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'ledger' })
}

// When type changes, clear ledger selection if current ledger doesn't match
function onTypeChange() {
  formData.value.ledgerId = ''
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">
          {{ isEditing ? 'Edit Category' : 'Create Category' }}
        </h2>
        <p class="text-muted-foreground">
          {{ isEditing ? 'Update the transaction category details.' : 'Add a new transaction category.' }}
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
    <form v-else @submit.prevent="saveCategory" class="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Category Details</CardTitle>
          <CardDescription>Enter the transaction category information</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="space-y-2">
            <Label for="name">Name <span class="text-red-500">*</span></Label>
            <Input
              id="name"
              v-model="formData.name"
              placeholder="e.g., Tuition Fee"
              required
            />
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="type">Type <span class="text-red-500">*</span></Label>
              <Select v-model="formData.type" @update:model-value="onTypeChange" :disabled="isEditing">
                <SelectTrigger>
                  <SelectValue placeholder="Select category type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="type in categoryTypes" :key="type.value" :value="type.value">
                    {{ type.label }}
                  </SelectItem>
                </SelectContent>
              </Select>
              <p v-if="isEditing" class="text-xs text-muted-foreground">Type cannot be changed after creation</p>
            </div>
            <div class="space-y-2">
              <Label for="ledger">Ledger <span class="text-red-500">*</span></Label>
              <Select v-model="formData.ledgerId" :disabled="!formData.type || isLoadingLedgers">
                <SelectTrigger>
                  <SelectValue :placeholder="isLoadingLedgers ? 'Loading...' : 'Select ledger'" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem 
                    v-for="ledger in filteredLedgers" 
                    :key="ledger.id" 
                    :value="String(ledger.id)"
                  >
                    {{ ledger.code }} - {{ ledger.name }}
                  </SelectItem>
                </SelectContent>
              </Select>
              <p v-if="formData.type" class="text-xs text-muted-foreground">
                Showing {{ formData.type === 'income' ? 'revenue' : 'expense' }} ledgers
              </p>
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

          <div class="flex flex-col gap-4 pt-4">
            <div class="flex items-center gap-2">
              <Switch
                id="requiresStudent"
                v-model:checked="formData.requiresStudent"
              />
              <Label for="requiresStudent" class="cursor-pointer">Requires Student</Label>
            </div>
            <p class="text-xs text-muted-foreground">If enabled, transactions in this category must be linked to a student</p>

            <div class="flex items-center gap-2">
              <Switch
                id="isActive"
                v-model:checked="formData.isActive"
              />
              <Label for="isActive" class="cursor-pointer">Active</Label>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Actions -->
      <div class="flex gap-4">
        <Button type="submit" :disabled="isSaving">
          <iconify-icon v-if="isSaving" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
          <iconify-icon v-else icon="lucide:save" class="mr-2 h-4 w-4" />
          {{ isSaving ? 'Saving...' : (isEditing ? 'Update Category' : 'Create Category') }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
