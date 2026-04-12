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

const studentId = Number(route.params.id)

interface Student {
  id: number
  studentId: string
  name: string
  className: string | null
}

interface Ledger {
  id: number
  code: string
  name: string
}

interface Category {
  id: number
  name: string
}

const student = ref<Student | null>(null)
const cashLedgers = ref<Ledger[]>([])
const incomeCategories = ref<Category[]>([])

const formData = ref({
  transactionDate: new Date().toISOString().split('T')[0],
  type: 'income' as 'income' | 'expense',
  paymentMethod: 'cash',
  referenceNumber: '',
  description: '',
  cashLedgerId: '',
  categoryId: '',
  amount: 0,
  itemDescription: '',
})

const isLoading = ref(true)
const isSaving = ref(false)
const error = ref<string | null>(null)
const isLoadingOptions = ref(true)

const paymentMethods = [
  { value: 'cash', label: 'Cash' },
  { value: 'bank_transfer', label: 'Bank Transfer' },
  { value: 'cheque', label: 'Cheque' },
  { value: 'credit_card', label: 'Credit Card' },
  { value: 'debit_card', label: 'Debit Card' },
  { value: 'online', label: 'Online Payment' },
]

onMounted(async () => {
  sidebar.setPageName('Quick Payment')
  await Promise.all([fetchStudent(), loadOptions()])
})

async function fetchStudent() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getStudent(studentId) as any
    student.value = response.data
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load student'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

async function loadOptions() {
  try {
    isLoadingOptions.value = true
    // Load cash ledgers (asset type)
    const ledgersResponse = await api.getLedgers({ perPage: 1000, type: 'asset' }) as any
    cashLedgers.value = ledgersResponse.data.data

    // Load income categories
    const categoriesResponse = await api.getCategories({ perPage: 1000, type: 'income' }) as any
    incomeCategories.value = categoriesResponse.data.data
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load options' })
  } finally {
    isLoadingOptions.value = false
  }
}

async function savePayment() {
  // Validation
  if (!formData.value.transactionDate) {
    toast.error('Validation Error', { description: 'Transaction date is required' })
    return
  }
  if (!formData.value.cashLedgerId) {
    toast.error('Validation Error', { description: 'Please select a cash ledger' })
    return
  }
  if (!formData.value.categoryId) {
    toast.error('Validation Error', { description: 'Please select a category' })
    return
  }
  if (!formData.value.amount || formData.value.amount <= 0) {
    toast.error('Validation Error', { description: 'Please enter a valid amount' })
    return
  }

  try {
    isSaving.value = true
    const payload = {
      transactionDate: formData.value.transactionDate,
      type: 'income',
      transactableType: 'Student',
      studentId: studentId,
      payerId: null,
      paymentMethod: formData.value.paymentMethod,
      referenceNumber: formData.value.referenceNumber || null,
      description: formData.value.description || null,
      cashLedgerId: parseInt(formData.value.cashLedgerId),
      items: [
        {
          categoryId: parseInt(formData.value.categoryId),
          amount: formData.value.amount,
          description: formData.value.itemDescription,
          quantity: 1,
          unitPrice: formData.value.amount,
        }
      ],
    }

    await api.createTransaction(payload)
    toast.success('Success', { description: 'Payment recorded successfully' })
    
    // Redirect to student view
    router.push({ name: 'student_view', params: { id: studentId } })
  } catch (err: any) {
    const message = err?.response?.data?.detail || 'Failed to record payment'
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'student_view', params: { id: studentId } })
}

function formatAmount(amount: number): string {
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
        <h2 class="text-3xl font-bold tracking-tight">Quick Payment</h2>
        <p class="text-muted-foreground">Record a quick payment from {{ student?.name || 'student' }}</p>
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
    <div v-if="isLoading || isLoadingOptions" class="space-y-4">
      <Skeleton class="h-8 w-1/3" />
      <Skeleton class="h-64 w-full" />
    </div>

    <!-- Form -->
    <form v-else @submit.prevent="savePayment" class="space-y-6">
      <!-- Student Info Card -->
      <Card>
        <CardHeader>
          <CardTitle>Student Information</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="flex items-center gap-4">
            <div class="h-12 w-12 rounded-full bg-primary/10 flex items-center justify-center">
              <iconify-icon icon="lucide:user" class="h-6 w-6 text-primary" />
            </div>
            <div>
              <p class="text-lg font-semibold">{{ student?.name }}</p>
              <p class="text-sm text-muted-foreground">
                {{ student?.studentId }} 
                <span v-if="student?.className">• {{ student.className }}</span>
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Payment Details -->
      <Card>
        <CardHeader>
          <CardTitle>Payment Details</CardTitle>
          <CardDescription>Enter the payment information</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <div class="space-y-2">
              <Label for="transactionDate">Date <span class="text-red-500">*</span></Label>
              <Input
                id="transactionDate"
                v-model="formData.transactionDate"
                type="date"
                required
              />
            </div>
            <div class="space-y-2">
              <Label for="paymentMethod">Payment Method <span class="text-red-500">*</span></Label>
              <Select v-model="formData.paymentMethod">
                <SelectTrigger>
                  <SelectValue placeholder="Select method" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="method in paymentMethods" :key="method.value" :value="method.value">
                    {{ method.label }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div class="space-y-2">
              <Label for="referenceNumber">Reference Number</Label>
              <Input
                id="referenceNumber"
                v-model="formData.referenceNumber"
                placeholder="Cheque no, ref no, etc."
              />
            </div>
          </div>

          <div class="space-y-2">
            <Label for="cashLedger">Cash Ledger <span class="text-red-500">*</span></Label>
            <Select v-model="formData.cashLedgerId">
              <SelectTrigger>
                <SelectValue placeholder="Select ledger" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="ledger in cashLedgers" :key="ledger.id" :value="String(ledger.id)">
                  {{ ledger.code }} - {{ ledger.name }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div class="space-y-2">
            <Label for="description">Description</Label>
            <Textarea
              id="description"
              v-model="formData.description"
              placeholder="Enter transaction description..."
              rows="2"
            />
          </div>
        </CardContent>
      </Card>

      <!-- Line Item -->
      <Card>
        <CardHeader>
          <CardTitle>Payment Item</CardTitle>
          <CardDescription>Enter payment category and amount</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label for="category">Category <span class="text-red-500">*</span></Label>
              <Select v-model="formData.categoryId">
                <SelectTrigger>
                  <SelectValue placeholder="Select category" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="cat in incomeCategories" :key="cat.id" :value="String(cat.id)">
                    {{ cat.name }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div class="space-y-2">
              <Label for="amount">Amount <span class="text-red-500">*</span></Label>
              <Input
                id="amount"
                v-model.number="formData.amount"
                type="number"
                min="0"
                step="0.01"
                placeholder="0.00"
                required
              />
            </div>
          </div>

          <div class="space-y-2">
            <Label for="itemDescription">Item Description</Label>
            <Input
              id="itemDescription"
              v-model="formData.itemDescription"
              placeholder="Description for this payment item..."
            />
          </div>

          <!-- Total -->
          <div class="mt-4 flex justify-end border-t pt-4">
            <div class="text-right">
              <p class="text-sm text-muted-foreground">Total Amount</p>
              <p class="text-2xl font-bold text-green-600">{{ formatAmount(formData.amount) }}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Actions -->
      <div class="flex gap-4">
        <Button type="submit" :disabled="isSaving">
          <iconify-icon v-if="isSaving" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
          <iconify-icon v-else icon="lucide:save" class="mr-2 h-4 w-4" />
          {{ isSaving ? 'Saving...' : 'Record Payment' }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
