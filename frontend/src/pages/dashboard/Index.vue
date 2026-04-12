<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useSidebarStore } from '@/stores/sidebar'
import { api } from '@/stores/api'
import { isCancel } from '@/services/api'
import { toast } from 'vue-sonner'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  LineElement,
  CategoryScale,
  LinearScale,
  PointElement,
  ArcElement,
} from 'chart.js'
import { Bar, Line, Doughnut } from 'vue-chartjs'

// Register Chart.js components
ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  LineElement,
  CategoryScale,
  LinearScale,
  PointElement,
  ArcElement
)

const sidebar = useSidebarStore()

interface DashboardStats {
  totalStudents: number
  totalPayers: number
  totalTransactions: number
  totalIncome: number
  totalExpenses: number
  netBalance: number
  recentTransactions: any[]
}

const stats = ref<DashboardStats>({
  totalStudents: 0,
  totalPayers: 0,
  totalTransactions: 0,
  totalIncome: 0,
  totalExpenses: 0,
  netBalance: 0,
  recentTransactions: [],
})

const isLoading = ref(true)
const error = ref<string | null>(null)
const monthlyData = ref({
  labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
  income: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
  expenses: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
})

onMounted(async () => {
  sidebar.setPageName('dashboard')
  await fetchDashboardData()
})

async function fetchDashboardData() {
  try {
    isLoading.value = true
    error.value = null

    // Fetch students count
    const studentsResponse = await api.getStudents({ perPage: 1 }) as any
    const totalStudents = studentsResponse.data.meta?.total || 0

    // Fetch payers count
    const payersResponse = await api.getPayers({ perPage: 1 }) as any
    const totalPayers = payersResponse.data.meta?.total || 0

    // Fetch transactions with date range (current year)
    const currentYear = new Date().getFullYear()
    const transactionsResponse = await api.getTransactions({ 
      perPage: 100,
      dateFrom: `${currentYear}-01-01`,
      dateTo: `${currentYear}-12-31`,
    }) as any

    const transactions = transactionsResponse.data.data || []
    const totalTransactions = transactionsResponse.data.meta?.total || 0

    // Calculate totals
    let totalIncome = 0
    let totalExpenses = 0

    // Initialize monthly data
    const monthlyIncome = new Array(12).fill(0)
    const monthlyExpenses = new Array(12).fill(0)

    transactions.forEach((txn: any) => {
      const amount = parseFloat(txn.amount) || 0
      const txnDate = new Date(txn.transactionDate)
      const month = txnDate.getMonth()

      if (txn.type === 'income') {
        totalIncome += amount
        monthlyIncome[month] += amount
      } else if (txn.type === 'expense') {
        totalExpenses += amount
        monthlyExpenses[month] += amount
      }
    })

    // Get recent transactions (last 5)
    const recentResponse = await api.getTransactions({ perPage: 5 }) as any
    const recentTransactions = recentResponse.data.data || []

    stats.value = {
      totalStudents,
      totalPayers,
      totalTransactions,
      totalIncome,
      totalExpenses,
      netBalance: totalIncome - totalExpenses,
      recentTransactions,
    }

    monthlyData.value = {
      labels: monthlyData.value.labels,
      income: monthlyIncome,
      expenses: monthlyExpenses,
    }
  } catch (err: any) {
    if (isCancel(err)) return
    error.value = err?.response?.data?.detail || 'Failed to load dashboard data'
    toast.error('Error', { description: error.value || undefined })
  } finally {
    isLoading.value = false
  }
}

// Chart configurations
const barChartData = computed(() => ({
  labels: monthlyData.value.labels,
  datasets: [
    {
      label: 'Income',
      backgroundColor: '#22c55e',
      data: monthlyData.value.income,
      borderRadius: 4,
    },
    {
      label: 'Expenses',
      backgroundColor: '#ef4444',
      data: monthlyData.value.expenses,
      borderRadius: 4,
    },
  ],
}))

const barChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom' as const,
    },
    title: {
      display: true,
      text: 'Monthly Income vs Expenses',
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        callback: function(value: any) {
          return 'RM ' + value.toLocaleString()
        },
      },
    },
  },
}

const doughnutChartData = computed(() => ({
  labels: ['Income', 'Expenses'],
  datasets: [
    {
      backgroundColor: ['#22c55e', '#ef4444'],
      data: [stats.value.totalIncome, stats.value.totalExpenses],
    },
  ],
}))

const doughnutChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom' as const,
    },
    title: {
      display: true,
      text: 'Income vs Expenses Distribution',
    },
  },
}

function formatAmount(amount: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
    maximumFractionDigits: 0,
  }).format(amount)
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-MY', {
    month: 'short',
    day: 'numeric',
  })
}

function getTransactionIcon(type: string): string {
  return type === 'income' ? 'lucide:arrow-down-to-line' : 'lucide:arrow-up-from-line'
}

function getTransactionColor(type: string): string {
  return type === 'income' ? 'text-green-600' : 'text-red-600'
}

function getTransactionBgColor(type: string): string {
  return type === 'income' ? 'bg-green-100 dark:bg-green-900' : 'bg-red-100 dark:bg-red-900'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Error Alert -->
    <Alert v-if="error" variant="destructive">
      <iconify-icon icon="lucide:alert-circle" class="h-4 w-4" />
      <AlertTitle>Error</AlertTitle>
      <AlertDescription>{{ error }}</AlertDescription>
    </Alert>

    <!-- Welcome Card -->
    <Card>
      <CardHeader>
        <CardTitle class="text-2xl">Welcome to School Accounting</CardTitle>
        <CardDescription>
          Manage your school's finances, students, and accounting records from one place.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <p class="text-muted-foreground">
          Use the sidebar to navigate between different modules. Start by setting up your 
          <router-link :to="{ name: 'view_main' }" class="text-primary hover:underline">
            Business Information
          </router-link> 
          or manage your 
          <router-link :to="{ name: 'students_list' }" class="text-primary hover:underline">
            Students
          </router-link>.
        </p>
      </CardContent>
    </Card>

    <!-- Stats Grid -->
    <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      <!-- Total Students -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Students</CardTitle>
          <div class="p-2 bg-blue-100 rounded-full dark:bg-blue-900">
            <iconify-icon icon="lucide:users" class="h-4 w-4 text-blue-600 dark:text-blue-400" />
          </div>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <Skeleton v-if="isLoading" class="h-8 w-20" />
            <span v-else>{{ stats.totalStudents.toLocaleString() }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Enrolled students</p>
        </CardContent>
      </Card>

      <!-- Total Payers -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Payers</CardTitle>
          <div class="p-2 bg-purple-100 rounded-full dark:bg-purple-900">
            <iconify-icon icon="lucide:users-round" class="h-4 w-4 text-purple-600 dark:text-purple-400" />
          </div>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <Skeleton v-if="isLoading" class="h-8 w-20" />
            <span v-else>{{ stats.totalPayers.toLocaleString() }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Active payers</p>
        </CardContent>
      </Card>

      <!-- Total Transactions -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Transactions</CardTitle>
          <div class="p-2 bg-amber-100 rounded-full dark:bg-amber-900">
            <iconify-icon icon="lucide:arrow-left-right" class="h-4 w-4 text-amber-600 dark:text-amber-400" />
          </div>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <Skeleton v-if="isLoading" class="h-8 w-20" />
            <span v-else>{{ stats.totalTransactions.toLocaleString() }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Total records this year</p>
        </CardContent>
      </Card>

      <!-- Net Balance -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Net Balance</CardTitle>
          <div class="p-2 rounded-full" :class="stats.netBalance >= 0 ? 'bg-green-100 dark:bg-green-900' : 'bg-red-100 dark:bg-red-900'">
            <iconify-icon 
              icon="lucide:scale" 
              class="h-4 w-4" 
              :class="stats.netBalance >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'"
            />
          </div>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <Skeleton v-if="isLoading" class="h-8 w-28" />
            <span v-else :class="stats.netBalance >= 0 ? 'text-green-600' : 'text-red-600'">
              {{ formatAmount(stats.netBalance) }}
            </span>
          </div>
          <p class="text-xs text-muted-foreground">Income minus expenses</p>
        </CardContent>
      </Card>
    </div>

    <!-- Financial Summary Cards -->
    <div class="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Income</CardTitle>
          <div class="p-2 bg-green-100 rounded-full dark:bg-green-900">
            <iconify-icon icon="lucide:arrow-down-to-line" class="h-4 w-4 text-green-600 dark:text-green-400" />
          </div>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-green-600">
            <Skeleton v-if="isLoading" class="h-8 w-32" />
            <span v-else>{{ formatAmount(stats.totalIncome) }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Year to date</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Expenses</CardTitle>
          <div class="p-2 bg-red-100 rounded-full dark:bg-red-900">
            <iconify-icon icon="lucide:arrow-up-from-line" class="h-4 w-4 text-red-600 dark:text-red-400" />
          </div>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-red-600">
            <Skeleton v-if="isLoading" class="h-8 w-32" />
            <span v-else>{{ formatAmount(stats.totalExpenses) }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Year to date</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Profit Margin</CardTitle>
          <div class="p-2 bg-blue-100 rounded-full dark:bg-blue-900">
            <iconify-icon icon="lucide:trending-up" class="h-4 w-4 text-blue-600 dark:text-blue-400" />
          </div>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <Skeleton v-if="isLoading" class="h-8 w-20" />
            <span v-else>
              {{ stats.totalIncome > 0 
                ? ((stats.netBalance / stats.totalIncome) * 100).toFixed(1) + '%' 
                : '0%' }}
            </span>
          </div>
          <p class="text-xs text-muted-foreground">Net profit percentage</p>
        </CardContent>
      </Card>
    </div>

    <!-- Charts Row -->
    <div class="grid gap-6 lg:grid-cols-2">
      <!-- Monthly Trends Chart -->
      <Card>
        <CardHeader>
          <CardTitle>Monthly Trends</CardTitle>
          <CardDescription>Income and expenses by month</CardDescription>
        </CardHeader>
        <CardContent>
          <div v-if="isLoading" class="h-80 flex items-center justify-center">
            <Skeleton class="h-full w-full" />
          </div>
          <div v-else class="h-80">
            <Bar :data="barChartData" :options="barChartOptions" />
          </div>
        </CardContent>
      </Card>

      <!-- Distribution Chart -->
      <Card>
        <CardHeader>
          <CardTitle>Financial Distribution</CardTitle>
          <CardDescription>Income vs Expenses breakdown</CardDescription>
        </CardHeader>
        <CardContent>
          <div v-if="isLoading" class="h-80 flex items-center justify-center">
            <Skeleton class="h-full w-full" />
          </div>
          <div v-else class="h-80">
            <Doughnut :data="doughnutChartData" :options="doughnutChartOptions" />
          </div>
        </CardContent>
      </Card>
    </div>

    <!-- Recent Transactions & Quick Actions -->
    <div class="grid gap-6 lg:grid-cols-2">
      <!-- Recent Transactions -->
      <Card>
        <CardHeader>
          <CardTitle>Recent Transactions</CardTitle>
          <CardDescription>Latest financial activities</CardDescription>
        </CardHeader>
        <CardContent>
          <div v-if="isLoading" class="space-y-3">
            <Skeleton v-for="i in 5" :key="i" class="h-14 w-full" />
          </div>
          <div v-else-if="stats.recentTransactions.length === 0" class="text-center py-8">
            <iconify-icon icon="lucide:inbox" class="h-8 w-8 text-muted-foreground mx-auto mb-2" />
            <p class="text-sm text-muted-foreground">No transactions yet</p>
          </div>
          <div v-else class="space-y-3">
            <div 
              v-for="txn in stats.recentTransactions" 
              :key="txn.id"
              class="flex items-center justify-between p-3 rounded-lg border hover:bg-muted/50 transition-colors cursor-pointer"
              @click="$router.push({ name: txn.type === 'income' ? 'view_deposit' : 'view_payment', params: { id: txn.id } })"
            >
              <div class="flex items-center gap-3">
                <div :class="['p-2 rounded-full', getTransactionBgColor(txn.type)]">
                  <iconify-icon 
                    :icon="getTransactionIcon(txn.type)" 
                    :class="['h-4 w-4', getTransactionColor(txn.type)]"
                  />
                </div>
                <div>
                  <p class="font-medium text-sm">{{ txn.transactionNumber || 'N/A' }}</p>
                  <p class="text-xs text-muted-foreground">
                    {{ txn.transactableName || 'General' }} · {{ formatDate(txn.transactionDate) }}
                  </p>
                </div>
              </div>
              <div :class="['font-semibold', getTransactionColor(txn.type)]">
                {{ txn.type === 'income' ? '+' : '-' }}{{ formatAmount(parseFloat(txn.amount) || 0) }}
              </div>
            </div>
          </div>
          <div class="mt-4 text-center">
            <Button variant="ghost" size="sm" @click="$router.push({ name: 'transactions_all' })">
              View All Transactions
              <iconify-icon icon="lucide:arrow-right" class="ml-2 h-4 w-4" />
            </Button>
          </div>
        </CardContent>
      </Card>

      <!-- Quick Actions -->
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
          <CardDescription>Common tasks to get started</CardDescription>
        </CardHeader>
        <CardContent>
          <div class="grid grid-cols-2 gap-3">
            <Button variant="outline" class="justify-start h-auto py-4" @click="$router.push({ name: 'student_create' })">
              <div class="p-2 bg-blue-100 rounded-full dark:bg-blue-900 mr-3">
                <iconify-icon icon="lucide:user-plus" class="h-4 w-4 text-blue-600 dark:text-blue-400" />
              </div>
              <div class="text-left">
                <p class="font-medium">Add Student</p>
                <p class="text-xs text-muted-foreground">Register new student</p>
              </div>
            </Button>
            
            <Button variant="outline" class="justify-start h-auto py-4" @click="$router.push({ name: 'create_deposit' })">
              <div class="p-2 bg-green-100 rounded-full dark:bg-green-900 mr-3">
                <iconify-icon icon="lucide:arrow-down-to-line" class="h-4 w-4 text-green-600 dark:text-green-400" />
              </div>
              <div class="text-left">
                <p class="font-medium">Record Income</p>
                <p class="text-xs text-muted-foreground">Add new deposit</p>
              </div>
            </Button>
            
            <Button variant="outline" class="justify-start h-auto py-4" @click="$router.push({ name: 'create_payment' })">
              <div class="p-2 bg-red-100 rounded-full dark:bg-red-900 mr-3">
                <iconify-icon icon="lucide:arrow-up-from-line" class="h-4 w-4 text-red-600 dark:text-red-400" />
              </div>
              <div class="text-left">
                <p class="font-medium">Record Expense</p>
                <p class="text-xs text-muted-foreground">Add new payment</p>
              </div>
            </Button>
            
            <Button variant="outline" class="justify-start h-auto py-4" @click="$router.push({ name: 'index_reports' })">
              <div class="p-2 bg-purple-100 rounded-full dark:bg-purple-900 mr-3">
                <iconify-icon icon="lucide:file-text" class="h-4 w-4 text-purple-600 dark:text-purple-400" />
              </div>
              <div class="text-left">
                <p class="font-medium">View Reports</p>
                <p class="text-xs text-muted-foreground">Financial reports</p>
              </div>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
