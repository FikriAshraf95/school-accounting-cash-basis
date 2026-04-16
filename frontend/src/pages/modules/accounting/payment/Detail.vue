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
import { Textarea } from '@/components/ui/textarea'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

const sidebar = useSidebarStore()
const route = useRoute()
const router = useRouter()

const isEditing = computed(() => !!route.params.id)
const transactionId = computed(() => Number(route.params.id))

interface LineItem {
  id?: number
  categoryId: string
  amount: number
  description: string
  quantity: number
  unitPrice: number
}

interface Student {
  id: number
  studentId: string
  name: string
}

interface Payer {
  id: number
  payerCode: string
  name: string
}

interface Ledger {
  id: number
  code: string
  name: string
}

interface Category {
  id: number
  name: string
  type: string
}

const formData = ref({
  transactionDate: new Date().toISOString().split('T')[0],
  type: 'expense' as 'income' | 'expense',
  transactableType: 'none' as 'Student' | 'Payer' | 'none',
  studentId: '',
  payerId: '',
  paymentMethod: 'cash',
  referenceNumber: '',
  description: '',
  cashLedgerId: '',
})

const lineItems = ref<LineItem[]>([
  { categoryId: '', amount: 0, description: '', quantity: 1, unitPrice: 0 }
])

const students = ref<Student[]>([])
const payers = ref<Payer[]>([])
const cashLedgers = ref<Ledger[]>([])
const expenseCategories = ref<Category[]>([])

const isLoading = ref(false)
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

const totalAmount = computed(() => {
  return lineItems.value.reduce((sum, item) => sum + (item.amount || 0), 0)
})

onMounted(async () => {
  sidebar.setPageName(isEditing.value ? 'Edit Expense' : 'Record Expense')
  await loadOptions()
  if (isEditing.value) {
    await fetchTransaction()
  }
})

async function loadOptions() {
  try {
    isLoadingOptions.value = true
    // Load students
    const studentsResponse = await api.getStudents({ perPage: 1000, isActive: true }) as any
    students.value = studentsResponse.data

    // Load payers
    const payersResponse = await api.getPayers({ perPage: 1000, isActive: true }) as any
    payers.value = payersResponse.data

    // Load cash ledgers (asset type)
    const ledgersResponse = await api.getLedgers({ perPage: 1000, type: 'asset' }) as any
    cashLedgers.value = ledgersResponse.data

    // Load expense categories
    const categoriesResponse = await api.getCategories({ perPage: 1000, type: 'expense' }) as any
    expenseCategories.value = categoriesResponse.data
  } catch (err: any) {
    toast.error('Error', { description: 'Failed to load options' })
  } finally {
    isLoadingOptions.value = false
  }
}

async function fetchTransaction() {
  try {
    isLoading.value = true
    error.value = null
    const response = await api.getTransaction(transactionId.value) as any
    const transaction = response

    formData.value = {
      transactionDate: transaction.transactionDate.split('T')[0],
      type: transaction.type,
      transactableType: transaction.transactableType || 'none',
      studentId: transaction.studentId ? String(transaction.studentId) : '',
      payerId: transaction.payerId ? String(transaction.payerId) : '',
      paymentMethod: transaction.paymentMethod || 'cash',
      referenceNumber: transaction.referenceNumber || '',
      description: transaction.description || '',
      cashLedgerId: transaction.cashLedgerId ? String(transaction.cashLedgerId) : '',
    }

    if (transaction.items && transaction.items.length > 0) {
      lineItems.value = transaction.items.map((item: any) => ({
        id: item.id,
        categoryId: String(item.categoryId),
        amount: item.amount,
        description: item.description || '',
        quantity: item.quantity || 1,
        unitPrice: item.unitPrice || item.amount,
      }))
    }
  } catch (err: any) {
    error.value = err?.response?.data?.detail || 'Failed to load transaction'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

function addLineItem() {
  lineItems.value.push({
    categoryId: '',
    amount: 0,
    description: '',
    quantity: 1,
    unitPrice: 0,
  })
}

function removeLineItem(index: number) {
  if (lineItems.value.length > 1) {
    lineItems.value.splice(index, 1)
  }
}

function updateLineItemAmount(index: number) {
  const item = lineItems.value[index]
  item.amount = (item.quantity || 0) * (item.unitPrice || 0)
}

async function saveTransaction() {
  // Validation
  if (!formData.value.transactionDate) {
    toast.error('Validation Error', { description: 'Transaction date is required' })
    return
  }
  if (!formData.value.cashLedgerId) {
    toast.error('Validation Error', { description: 'Please select a cash ledger' })
    return
  }
  if (!formData.value.transactableType || formData.value.transactableType === 'none') {
    toast.error('Validation Error', { description: 'Please select a Pay To type (Student or Payer)' })
    return
  }
  if (formData.value.transactableType === 'Student' && !formData.value.studentId) {
    toast.error('Validation Error', { description: 'Please select a student' })
    return
  }
  if (formData.value.transactableType === 'Payer' && !formData.value.payerId) {
    toast.error('Validation Error', { description: 'Please select a payer' })
    return
  }

  // Validate line items
  const validItems = lineItems.value.filter(item => item.categoryId && item.amount > 0)
  if (validItems.length === 0) {
    toast.error('Validation Error', { description: 'At least one valid line item is required' })
    return
  }

  try {
    isSaving.value = true
    const payload = {
      ...formData.value,
      transactableType: formData.value.transactableType,
      studentId: formData.value.transactableType === 'Student' && formData.value.studentId
        ? parseInt(formData.value.studentId)
        : null,
      payerId: formData.value.transactableType === 'Payer' && formData.value.payerId
        ? parseInt(formData.value.payerId)
        : null,
      cashLedgerId: parseInt(formData.value.cashLedgerId),
      items: validItems.map(item => ({
        categoryId: parseInt(item.categoryId),
        amount: item.amount,
        description: item.description,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
      })),
    }

    if (isEditing.value) {
      await api.updateTransaction(transactionId.value, payload)
      toast.success('Success', { description: 'Expense transaction updated successfully' })
    } else {
      await api.createTransaction(payload)
      toast.success('Success', { description: 'Expense transaction recorded successfully' })
    }
    router.push({ name: 'payment' })
  } catch (err: any) {
    const message = err?.response?.data?.detail || `Failed to ${isEditing.value ? 'update' : 'record'} expense`
    toast.error('Error', { description: message || undefined })
  } finally {
    isSaving.value = false
  }
}

function goBack() {
  router.push({ name: 'payment' })
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
        <h2 class="text-3xl font-bold tracking-tight">
          {{ isEditing ? 'Edit Expense' : 'Record Expense' }}
        </h2>
        <p class="text-muted-foreground">
          {{ isEditing ? 'Update the expense transaction details.' : 'Record a new expense or payment transaction.' }}
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
    <div v-if="isLoading || isLoadingOptions" class="space-y-4">
      <Skeleton class="h-8 w-1/3" />
      <Skeleton class="h-96 w-full" />
    </div>

    <!-- Form -->
    <form v-else @submit.prevent="saveTransaction" class="space-y-6">
      <!-- Transaction Details -->
      <Card>
        <CardHeader>
          <CardTitle>Transaction Details</CardTitle>
          <CardDescription>Enter the main transaction information</CardDescription>
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

          <div class="grid gap-4 sm:grid-cols-2">
            <div class="space-y-2">
              <Label>Pay To <span class="text-red-500">*</span></Label>
              <div class="flex gap-2">
                <Select v-model="formData.transactableType" class="w-32">
                  <SelectTrigger>
                    <SelectValue placeholder="Type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">None</SelectItem>
                    <SelectItem value="Student">Student</SelectItem>
                    <SelectItem value="Payer">Payer</SelectItem>
                  </SelectContent>
                </Select>
                <Select v-if="formData.transactableType === 'Student'" v-model="formData.studentId" class="flex-1">
                  <SelectTrigger>
                    <SelectValue placeholder="Select student" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="student in students" :key="student.id" :value="String(student.id)">
                      {{ student.name }} ({{ student.studentId }})
                    </SelectItem>
                  </SelectContent>
                </Select>
                <Select v-else-if="formData.transactableType === 'Payer'" v-model="formData.payerId" class="flex-1">
                  <SelectTrigger>
                    <SelectValue placeholder="Select payer" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="payer in payers" :key="payer.id" :value="String(payer.id)">
                      {{ payer.name }} ({{ payer.payerCode }})
                    </SelectItem>
                  </SelectContent>
                </Select>
                <div v-else class="flex-1 flex items-center text-muted-foreground text-sm">
                  General expense
                </div>
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

      <!-- Line Items -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Line Items</CardTitle>
            <CardDescription>Add expense categories and amounts</CardDescription>
          </div>
          <Button type="button" variant="outline" @click="addLineItem">
            <iconify-icon icon="lucide:plus" class="mr-2 h-4 w-4" />
            Add Item
          </Button>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead class="w-[200px]">Category <span class="text-red-500">*</span></TableHead>
                <TableHead class="w-[100px]">Qty</TableHead>
                <TableHead class="w-[150px]">Unit Price</TableHead>
                <TableHead class="w-[150px]">Amount</TableHead>
                <TableHead>Description</TableHead>
                <TableHead class="w-[50px]"></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              <TableRow v-for="(item, index) in lineItems" :key="index">
                <TableCell>
                  <Select v-model="item.categoryId">
                    <SelectTrigger>
                      <SelectValue placeholder="Select category" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem v-for="cat in expenseCategories" :key="cat.id" :value="String(cat.id)">
                        {{ cat.name }}
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </TableCell>
                <TableCell>
                  <Input
                    v-model.number="item.quantity"
                    type="number"
                    min="1"
                    @input="updateLineItemAmount(index)"
                  />
                </TableCell>
                <TableCell>
                  <Input
                    v-model.number="item.unitPrice"
                    type="number"
                    min="0"
                    step="0.01"
                    @input="updateLineItemAmount(index)"
                  />
                </TableCell>
                <TableCell>
                  <Input
                    v-model.number="item.amount"
                    type="number"
                    min="0"
                    step="0.01"
                    readonly
                    class="bg-muted"
                  />
                </TableCell>
                <TableCell>
                  <Input
                    v-model="item.description"
                    placeholder="Item description..."
                  />
                </TableCell>
                <TableCell>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    @click="removeLineItem(index)"
                    :disabled="lineItems.length <= 1"
                  >
                    <iconify-icon icon="lucide:trash-2" class="h-4 w-4 text-destructive" />
                  </Button>
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>

          <!-- Total -->
          <div class="mt-4 flex justify-end border-t pt-4">
            <div class="text-right">
              <p class="text-sm text-muted-foreground">Total Amount</p>
              <p class="text-2xl font-bold text-destructive">{{ formatAmount(totalAmount) }}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <!-- Actions -->
      <div class="flex gap-4">
        <Button type="submit" :disabled="isSaving">
          <iconify-icon v-if="isSaving" icon="lucide:loader-2" class="mr-2 h-4 w-4 animate-spin" />
          <iconify-icon v-else icon="lucide:save" class="mr-2 h-4 w-4" />
          {{ isSaving ? 'Saving...' : (isEditing ? 'Update Expense' : 'Record Expense') }}
        </Button>
        <Button type="button" variant="outline" @click="goBack" :disabled="isSaving">
          Cancel
        </Button>
      </div>
    </form>
  </div>
</template>
