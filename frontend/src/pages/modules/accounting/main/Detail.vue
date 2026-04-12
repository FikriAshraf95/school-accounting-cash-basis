<script setup lang="ts">
import { ref, onMounted } from 'vue'
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

const sidebar = useSidebarStore()
const route = useRoute()
const router = useRouter()

interface BusinessInfo {
  id: number
  schoolName: string
  registrationNumber: string | null
  address: string | null
  phone: string | null
  email: string | null
  financialYearStart: string
  financialYearEnd: string
  currency: string
  timezone: string
  bankName: string | null
  bankAccountName: string | null
  bankAccountNumber: string | null
  taxRegistration: string | null
  taxRate: number
  createdAt: string
  updatedAt: string
}

interface BusinessInfoForm {
  id?: number
  schoolName: string
  registrationNumber: string
  address: string
  phone: string
  email: string
  financialYearStart: string
  financialYearEnd: string
  currency: string
  timezone: string
  bankName: string
  bankAccountName: string
  bankAccountNumber: string
  taxRegistration: string
  taxRate: number
}

const businessInfo = ref<BusinessInfo | null>(null)
const formData = ref<BusinessInfoForm>({
  schoolName: '',
  registrationNumber: '',
  address: '',
  phone: '',
  email: '',
  financialYearStart: '',
  financialYearEnd: '',
  currency: '',
  timezone: '',
  bankName: '',
  bankAccountName: '',
  bankAccountNumber: '',
  taxRegistration: '',
  taxRate: 0,
})
const isLoading = ref(true)
const isSaving = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName('Edit Business Information')
  await fetchBusinessInfo()
})

async function fetchBusinessInfo() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getBusinessInfo() as any
    businessInfo.value = response.data
    // Initialize form data with current values, converting nulls to empty strings
    const data = response.data
    formData.value = {
      ...data,
      registrationNumber: data.registrationNumber ?? '',
      address: data.address ?? '',
      phone: data.phone ?? '',
      email: data.email ?? '',
      currency: data.currency ?? '',
      timezone: data.timezone ?? '',
      bankName: data.bankName ?? '',
      bankAccountName: data.bankAccountName ?? '',
      bankAccountNumber: data.bankAccountNumber ?? '',
      taxRegistration: data.taxRegistration ?? '',
    }
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load business information'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function saveBusinessInfo() {
  if (!formData.value.schoolName) {
    toast.error('Validation Error', { description: 'School name is required' })
    return
  }

  try {
    isSaving.value = true
    await api.updateBusinessInfo(formData.value)
    toast.success('Success', { description: 'Business information updated successfully' })
    router.push({ name: 'view_main' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to update business information'
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'view_main' })
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Edit Business Information</h2>
        <p class="text-muted-foreground">Update your school's business details and settings.</p>
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
    <form v-else @submit.prevent="saveBusinessInfo" class="space-y-6">
      <!-- School Details Card -->
      <Card>
        <CardHeader>
          <CardTitle>School Details</CardTitle>
          <CardDescription>Basic information about your school</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="schoolName">School Name <span class="text-red-500">*</span></Label>
              <Input
                id="schoolName"
                v-model="formData.schoolName"
                placeholder="Enter school name"
                required
              />
            </div>
            <div class="space-y-2">
              <Label for="registrationNumber">Registration Number</Label>
              <Input
                id="registrationNumber"
                v-model="formData.registrationNumber"
                placeholder="Enter registration number"
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
              <Label for="phone">Phone</Label>
              <Input
                id="phone"
                v-model="formData.phone"
                placeholder="Enter phone number"
              />
            </div>
            <div class="space-y-2">
              <Label for="email">Email</Label>
              <Input
                id="email"
                type="email"
                v-model="formData.email"
                placeholder="Enter email address"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Financial Settings Card -->
      <Card>
        <CardHeader>
          <CardTitle>Financial Settings</CardTitle>
          <CardDescription>Accounting and financial year configuration</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="financialYearStart">Financial Year Start</Label>
              <Input
                id="financialYearStart"
                type="date"
                v-model="formData.financialYearStart"
              />
            </div>
            <div class="space-y-2">
              <Label for="financialYearEnd">Financial Year End</Label>
              <Input
                id="financialYearEnd"
                type="date"
                v-model="formData.financialYearEnd"
              />
            </div>
          </div>
          <div class="grid gap-4 sm:grid-cols-3">
            <div class="space-y-2">
              <Label for="currency">Currency</Label>
              <Input
                id="currency"
                v-model="formData.currency"
                placeholder="e.g., MYR"
              />
            </div>
            <div class="space-y-2">
              <Label for="timezone">Timezone</Label>
              <Input
                id="timezone"
                v-model="formData.timezone"
                placeholder="e.g., Asia/Kuala_Lumpur"
              />
            </div>
            <div class="space-y-2">
              <Label for="taxRate">Tax Rate (%)</Label>
              <Input
                id="taxRate"
                type="number"
                step="0.01"
                v-model="formData.taxRate"
                placeholder="0.00"
              />
            </div>
          </div>
          <div class="space-y-2">
            <Label for="taxRegistration">Tax Registration</Label>
            <Input
              id="taxRegistration"
              v-model="formData.taxRegistration"
              placeholder="Enter tax registration number"
            />
          </div>
        </CardContent>
      </Card>

      <!-- Bank Details Card -->
      <Card>
        <CardHeader>
          <CardTitle>Bank Details</CardTitle>
          <CardDescription>Bank account information for transactions</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="bankName">Bank Name</Label>
              <Input
                id="bankName"
                v-model="formData.bankName"
                placeholder="Enter bank name"
              />
            </div>
            <div class="space-y-2">
              <Label for="bankAccountName">Account Name</Label>
              <Input
                id="bankAccountName"
                v-model="formData.bankAccountName"
                placeholder="Enter account name"
              />
            </div>
          </div>
          <div class="space-y-2">
            <Label for="bankAccountNumber">Account Number</Label>
            <Input
              id="bankAccountNumber"
              v-model="formData.bankAccountNumber"
              placeholder="Enter account number"
            />
          </div>
        </CardContent>
      </Card>

      <!-- Actions -->
      <div class="flex gap-4">
        <Button type="submit" :disabled="isSaving">
          <iconify-icon v-if="isSaving" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
          <iconify-icon v-else icon="lucide:save" class="mr-2 h-4 w-4" />
          {{ isSaving ? 'Saving...' : 'Save Changes' }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
