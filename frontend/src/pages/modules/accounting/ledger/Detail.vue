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
const ledgerId = computed(() => Number(route.params.id))

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

const formData = ref({
  code: '',
  name: '',
  type: '' as 'asset' | 'liability' | 'equity' | 'revenue' | 'expense' | '',
  category: '',
  isActive: true,
})

const isLoading = ref(false)
const isSaving = ref(false)
const error = ref<string | null>(null)

const ledgerTypes = [
  { value: 'asset', label: 'Asset' },
  { value: 'liability', label: 'Liability' },
  { value: 'equity', label: 'Equity' },
  { value: 'revenue', label: 'Revenue' },
  { value: 'expense', label: 'Expense' },
]

onMounted(async () => {
  sidebar.setPageName(isEditing.value ? 'Edit Ledger' : 'Create Ledger')
  if (isEditing.value) {
    await fetchLedger()
  }
})

async function fetchLedger() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getLedger(ledgerId.value) as any
    const ledger = response
    formData.value = {
      code: ledger.code,
      name: ledger.name,
      type: ledger.type,
      category: ledger.category || '',
      isActive: ledger.isActive,
    }
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load ledger'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function saveLedger() {
  // Validation
  if (!formData.value.code) {
    toast.error('Validation Error', { description: 'Code is required' })
    return
  }
  if (!formData.value.name) {
    toast.error('Validation Error', { description: 'Name is required' })
    return
  }
  if (!formData.value.type) {
    toast.error('Validation Error', { description: 'Type is required' })
    return
  }

  try {
    isSaving.value = true
    const payload = {
      ...formData.value,
      category: formData.value.category || null,
    }

    if (isEditing.value) {
      await api.updateLedger(ledgerId.value, payload)
      toast.success('Success', { description: 'Ledger updated successfully' })
    } else {
      await api.createLedger(payload)
      toast.success('Success', { description: 'Ledger created successfully' })
    }
    router.push({ name: 'ledger' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || `Failed to ${isEditing.value ? 'update' : 'create'} ledger`
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'ledger' })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">
          {{ isEditing ? 'Edit Ledger' : 'Create Ledger' }}
        </h2>
        <p class="text-muted-foreground">
          {{ isEditing ? 'Update the ledger account details.' : 'Add a new ledger account to the chart of accounts.' }}
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
    <form v-else @submit.prevent="saveLedger" class="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Ledger Details</CardTitle>
          <CardDescription>Enter the ledger account information</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="code">Code <span class="text-red-500">*</span></Label>
              <Input
                id="code"
                v-model="formData.code"
                placeholder="e.g., 1001"
                required
              />
              <p class="text-xs text-muted-foreground">Unique identifier for this ledger account</p>
            </div>
            <div class="space-y-2">
              <Label for="name">Name <span class="text-red-500">*</span></Label>
              <Input
                id="name"
                v-model="formData.name"
                placeholder="e.g., Cash in Bank"
                required
              />
            </div>
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="type">Type <span class="text-red-500">*</span></Label>
              <Select v-model="formData.type" :disabled="isEditing">
                <SelectTrigger>
                  <SelectValue placeholder="Select ledger type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="type in ledgerTypes" :key="type.value" :value="type.value">
                    {{ type.label }}
                  </SelectItem>
                </SelectContent>
              </Select>
              <p v-if="isEditing" class="text-xs text-muted-foreground">Type cannot be changed after creation</p>
            </div>
            <div class="space-y-2">
              <Label for="category">Category</Label>
              <Input
                id="category"
                v-model="formData.category"
                placeholder="e.g., Current Assets"
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
          {{ isSaving ? 'Saving...' : (isEditing ? 'Update Ledger' : 'Create Ledger') }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
