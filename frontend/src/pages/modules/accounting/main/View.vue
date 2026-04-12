<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'

const sidebar = useSidebarStore()
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

const businessInfo = ref<BusinessInfo | null>(null)
const isLoading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName('Business Information')
  await fetchBusinessInfo()
})

async function fetchBusinessInfo() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getBusinessInfo() as any
    businessInfo.value = response
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load business information'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

function goToEdit() {
  if (businessInfo.value) {
    router.push({ name: 'edit_main', params: { id: businessInfo.value.id } })
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Business Information</h2>
        <p class="text-muted-foreground">View your school's business details and settings.</p>
      </div>
      <Button v-if="!isLoading && !error" @click="goToEdit">
        <iconify-icon icon="lucide:edit" class="mr-2 h-4 w-4" />
        Edit
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
      <Skeleton class="h-32 w-full" />
      <Skeleton class="h-32 w-full" />
    </div>

    <!-- Content -->
    <template v-else-if="businessInfo">
      <!-- School Details Card -->
      <Card>
        <CardHeader>
          <CardTitle>School Details</CardTitle>
          <CardDescription>Basic information about your school</CardDescription>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">School Name</dt>
              <dd class="mt-1 text-lg font-semibold">{{ businessInfo.schoolName }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Registration Number</dt>
              <dd class="mt-1 text-base">{{ businessInfo.registrationNumber || '-' }}</dd>
            </div>
            <div class="sm:col-span-2">
              <dt class="text-sm font-medium text-muted-foreground">Address</dt>
              <dd class="mt-1 text-base">{{ businessInfo.address || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Phone</dt>
              <dd class="mt-1 text-base">{{ businessInfo.phone || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Email</dt>
              <dd class="mt-1 text-base">{{ businessInfo.email || '-' }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <!-- Financial Settings Card -->
      <Card>
        <CardHeader>
          <CardTitle>Financial Settings</CardTitle>
          <CardDescription>Accounting and financial year configuration</CardDescription>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Financial Year Start</dt>
              <dd class="mt-1 text-base">{{ businessInfo.financialYearStart }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Financial Year End</dt>
              <dd class="mt-1 text-base">{{ businessInfo.financialYearEnd }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Currency</dt>
              <dd class="mt-1 text-base">{{ businessInfo.currency }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Timezone</dt>
              <dd class="mt-1 text-base">{{ businessInfo.timezone }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Tax Rate</dt>
              <dd class="mt-1 text-base">{{ businessInfo.taxRate }}%</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Tax Registration</dt>
              <dd class="mt-1 text-base">{{ businessInfo.taxRegistration || '-' }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <!-- Bank Details Card -->
      <Card>
        <CardHeader>
          <CardTitle>Bank Details</CardTitle>
          <CardDescription>Bank account information for transactions</CardDescription>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Bank Name</dt>
              <dd class="mt-1 text-base">{{ businessInfo.bankName || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Account Name</dt>
              <dd class="mt-1 text-base">{{ businessInfo.bankAccountName || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Account Number</dt>
              <dd class="mt-1 text-base">{{ businessInfo.bankAccountNumber || '-' }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <!-- Metadata -->
      <div class="text-xs text-muted-foreground">
        <p>Created: {{ new Date(businessInfo.createdAt).toLocaleString() }}</p>
        <p>Last Updated: {{ new Date(businessInfo.updatedAt).toLocaleString() }}</p>
      </div>
    </template>
  </div>
</template>
