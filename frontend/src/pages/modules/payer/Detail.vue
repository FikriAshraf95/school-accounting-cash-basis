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
import { Textarea } from '@/components/ui/textarea'
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
const payerId = computed(() => Number(route.params.id))

const payerTypes = [
  { value: 'donor', label: 'Donor' },
  { value: 'sponsor', label: 'Sponsor' },
  { value: 'vendor', label: 'Vendor' },
  { value: 'supplier', label: 'Supplier' },
  { value: 'general', label: 'General' },
  { value: 'government', label: 'Government' },
]

const payerCategories = [
  { value: 'individual', label: 'Individual' },
  { value: 'corporate', label: 'Corporate' },
  { value: 'government', label: 'Government' },
  { value: 'ngo', label: 'NGO' },
]

const formData = ref({
  payerCode: '',
  name: '',
  type: 'general',
  category: 'individual',
  email: '',
  phone: '',
  address: '',
  isRecurring: false,
  notes: '',
  isActive: true,
})

const isLoading = ref(false)
const isSaving = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName(isEditing.value ? 'Edit Payer' : 'Create Payer')
  if (isEditing.value) {
    await fetchPayer()
  }
})

async function fetchPayer() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getPayer(payerId.value) as any
    const payer = response
    formData.value = {
      payerCode: payer.payerCode,
      name: payer.name,
      type: payer.type,
      category: payer.category,
      email: payer.email || '',
      phone: payer.phone || '',
      address: payer.address || '',
      isRecurring: payer.isRecurring,
      notes: payer.notes || '',
      isActive: payer.isActive,
    }
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load payer'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function savePayer() {
  // Validation
  if (!formData.value.payerCode) {
    toast.error('Validation Error', { description: 'Payer Code is required' })
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
      email: formData.value.email || null,
      phone: formData.value.phone || null,
      address: formData.value.address || null,
      notes: formData.value.notes || null,
    }

    if (isEditing.value) {
      await api.updatePayer(payerId.value, payload)
      toast.success('Success', { description: 'Payer updated successfully' })
    } else {
      await api.createPayer(payload)
      toast.success('Success', { description: 'Payer created successfully' })
    }
    router.push({ name: 'payers_list' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || `Failed to ${isEditing.value ? 'update' : 'create'} payer`
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'payers_list' })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">
          {{ isEditing ? 'Edit Payer' : 'Create Payer' }}
        </h2>
        <p class="text-muted-foreground">
          {{ isEditing ? 'Update the payer details.' : 'Add a new payer to the system.' }}
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
    <form v-else @submit.prevent="savePayer" class="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Payer Information</CardTitle>
          <CardDescription>Enter the payer details</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="payerCode">Payer Code <span class="text-red-500">*</span></Label>
              <Input
                id="payerCode"
                v-model="formData.payerCode"
                placeholder="e.g., PAY001"
                required
              />
            </div>
            <div class="space-y-2">
              <Label for="name">Name <span class="text-red-500">*</span></Label>
              <Input
                id="name"
                v-model="formData.name"
                placeholder="e.g., ABC Corporation"
                required
              />
            </div>
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="type">Type <span class="text-red-500">*</span></Label>
              <Select v-model="formData.type">
                <SelectTrigger>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem 
                    v-for="type in payerTypes" 
                    :key="type.value" 
                    :value="type.value"
                  >
                    {{ type.label }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div class="space-y-2">
              <Label for="category">Category <span class="text-red-500">*</span></Label>
              <Select v-model="formData.category">
                <SelectTrigger>
                  <SelectValue placeholder="Select category" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem 
                    v-for="cat in payerCategories" 
                    :key="cat.value" 
                    :value="cat.value"
                  >
                    {{ cat.label }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="email">Email</Label>
              <Input
                id="email"
                type="email"
                v-model="formData.email"
                placeholder="contact@example.com"
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
            <Textarea
              id="address"
              v-model="formData.address"
              placeholder="Enter address"
              rows="3"
            />
          </div>

          <div class="space-y-2">
            <Label for="notes">Notes</Label>
            <Textarea
              id="notes"
              v-model="formData.notes"
              placeholder="Additional notes about this payer"
              rows="2"
            />
          </div>

          <div class="flex flex-col gap-4 pt-4">
            <div class="flex items-center gap-2">
              <Switch
                id="isRecurring"
                v-model:checked="formData.isRecurring"
              />
              <Label for="isRecurring" class="cursor-pointer">Recurring Payer</Label>
            </div>
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
          {{ isSaving ? 'Saving...' : (isEditing ? 'Update Payer' : 'Create Payer') }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
