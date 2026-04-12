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

const sidebar = useSidebarStore()
const route = useRoute()
const router = useRouter()

const ledgerId = Number(route.params.id)

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

const ledger = ref<Ledger | null>(null)
const isLoading = ref(true)
const isDeleting = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName('View Ledger')
  await fetchLedger()
})

async function fetchLedger() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getLedger(ledgerId) as any
    ledger.value = response
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load ledger'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function deleteLedger() {
  try {
    isDeleting.value = true
    await api.deleteLedger(ledgerId)
    toast.success('Success', { description: 'Ledger deleted successfully' })
    router.push({ name: 'ledger' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to delete ledger'
    toast.error('Error', { description: message || undefined })
    isDeleting.value = false
  }
}

function goToEdit() {
  router.push({ name: 'edit_ledger', params: { id: ledgerId } })
}

function goBack() {
  router.push({ name: 'ledger' })
}

function getTypeBadgeColor(type: string): any {
  const typeMap: Record<string, any> = {
    asset: 'default',
    liability: 'secondary',
    equity: 'outline',
    revenue: 'default',
    expense: 'destructive',
  }
  return typeMap[type] || 'default'
}

function formatBalance(balance: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
  }).format(balance)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Ledger Details</h2>
        <p class="text-muted-foreground">View ledger account information</p>
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
              <AlertDialogTitle>Delete Ledger</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to delete this ledger? This action cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction @click="deleteLedger" :disabled="isDeleting">
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
    </div>

    <!-- Content -->
    <template v-else-if="ledger">
      <!-- Ledger Info Card -->
      <Card>
        <CardHeader>
          <div class="flex items-start justify-between">
            <div>
              <CardTitle class="text-2xl">{{ ledger.name }}</CardTitle>
              <CardDescription>Code: {{ ledger.code }}</CardDescription>
            </div>
            <Badge :variant="getTypeBadgeColor(ledger.type)" class="text-sm">
              {{ ledger.type.charAt(0).toUpperCase() + ledger.type.slice(1) }}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Current Balance</dt>
              <dd class="mt-1 text-2xl font-bold">{{ formatBalance(ledger.balance) }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Status</dt>
              <dd class="mt-1">
                <Badge :variant="ledger.isActive ? 'default' : 'secondary'">
                  {{ ledger.isActive ? 'Active' : 'Inactive' }}
                </Badge>
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Category</dt>
              <dd class="mt-1 text-base">{{ ledger.category || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Ledger ID</dt>
              <dd class="mt-1 text-base">#{{ ledger.id }}</dd>
            </div>
          </dl>
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
              <dd class="mt-1 text-sm">{{ new Date(ledger.createdAt).toLocaleString() }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Last Updated</dt>
              <dd class="mt-1 text-sm">{{ new Date(ledger.updatedAt).toLocaleString() }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </template>
  </div>
</template>
