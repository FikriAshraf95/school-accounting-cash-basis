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

const categoryId = Number(route.params.id)

interface Category {
  id: number
  name: string
  type: 'income' | 'expense'
  ledgerId: number
  ledgerName: string
  ledgerCode: string
  description: string | null
  requiresStudent: boolean
  isActive: boolean
  createdAt: string
  updatedAt: string
}

const category = ref<Category | null>(null)
const isLoading = ref(true)
const isDeleting = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  sidebar.setPageName('View Category')
  await fetchCategory()
})

async function fetchCategory() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getCategory(categoryId) as any
    category.value = response
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load category'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function deleteCategory() {
  try {
    isDeleting.value = true
    await api.deleteCategory(categoryId)
    toast.success('Success', { description: 'Category deleted successfully' })
    router.push({ name: 'ledger' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to delete category'
    toast.error('Error', { description: message || undefined })
    isDeleting.value = false
  }
}

function goToEdit() {
  router.push({ name: 'edit_category', params: { id: categoryId } })
}

function goBack() {
  router.push({ name: 'ledger' })
}

function getTypeBadgeColor(type: string): any {
  return type === 'income' ? 'default' : 'destructive'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-3xl font-bold tracking-tight">Category Details</h2>
        <p class="text-muted-foreground">View transaction category information</p>
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
              <AlertDialogTitle>Delete Category</AlertDialogTitle>
              <AlertDialogDescription>
                Are you sure you want to delete this category? This action cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction @click="deleteCategory" :disabled="isDeleting">
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
    <template v-else-if="category">
      <!-- Category Info Card -->
      <Card>
        <CardHeader>
          <div class="flex items-start justify-between">
            <div>
              <CardTitle class="text-2xl">{{ category.name }}</CardTitle>
              <CardDescription>
                Linked to Ledger: {{ category.ledgerCode }} - {{ category.ledgerName }}
              </CardDescription>
            </div>
            <Badge :variant="getTypeBadgeColor(category.type)" class="text-sm">
              {{ category.type.charAt(0).toUpperCase() + category.type.slice(1) }}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <dl class="grid gap-4 sm:grid-cols-2">
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Description</dt>
              <dd class="mt-1 text-base">{{ category.description || '-' }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Requires Student</dt>
              <dd class="mt-1">
                <Badge :variant="category.requiresStudent ? 'default' : 'secondary'">
                  {{ category.requiresStudent ? 'Yes' : 'No' }}
                </Badge>
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Status</dt>
              <dd class="mt-1">
                <Badge :variant="category.isActive ? 'default' : 'secondary'">
                  {{ category.isActive ? 'Active' : 'Inactive' }}
                </Badge>
              </dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Category ID</dt>
              <dd class="mt-1 text-base">#{{ category.id }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      <!-- Ledger Link Card -->
      <Card>
        <CardHeader>
          <CardTitle>Linked Ledger</CardTitle>
          <CardDescription>This category is associated with the following ledger account</CardDescription>
        </CardHeader>
        <CardContent>
          <div class="flex items-center justify-between rounded-lg border p-4">
            <div>
              <p class="font-medium">{{ category.ledgerName }}</p>
              <p class="text-sm text-muted-foreground">Code: {{ category.ledgerCode }}</p>
            </div>
            <Button variant="outline" size="sm" @click="router.push({ name: 'view_ledger', params: { id: category.ledgerId } })">
              <iconify-icon icon="lucide:external-link" class="mr-2 h-4 w-4" />
              View Ledger
            </Button>
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
              <dd class="mt-1 text-sm">{{ new Date(category.createdAt).toLocaleString() }}</dd>
            </div>
            <div>
              <dt class="text-sm font-medium text-muted-foreground">Last Updated</dt>
              <dd class="mt-1 text-sm">{{ new Date(category.updatedAt).toLocaleString() }}</dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </template>
  </div>
</template>
