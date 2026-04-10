<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSidebarStore } from '@/stores/sidebar'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

const sidebar = useSidebarStore()

// Stats placeholders
const stats = ref({
  totalStudents: 0,
  totalPayers: 0,
  totalTransactions: 0,
  recentDeposits: 0,
})

const isLoading = ref(true)

onMounted(async () => {
  sidebar.setPageName('dashboard')
  
  // Fetch initial stats (placeholder data for now)
  // In Week 5, we'll fetch real data here
  await new Promise(resolve => setTimeout(resolve, 500))
  isLoading.value = false
})
</script>

<template>
  <div class="space-y-6">
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

    <!-- Stat Placeholders Grid -->
    <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      <!-- Total Students -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Students</CardTitle>
          <iconify-icon icon="lucide:users" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <span v-if="isLoading">...</span>
            <span v-else>{{ stats.totalStudents }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Enrolled students</p>
        </CardContent>
      </Card>

      <!-- Total Payers -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Payers</CardTitle>
          <iconify-icon icon="lucide:users-round" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <span v-if="isLoading">...</span>
            <span v-else>{{ stats.totalPayers }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Active payers</p>
        </CardContent>
      </Card>

      <!-- Total Transactions -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Transactions</CardTitle>
          <iconify-icon icon="lucide:arrow-left-right" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <span v-if="isLoading">...</span>
            <span v-else>{{ stats.totalTransactions }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Total records</p>
        </CardContent>
      </Card>

      <!-- Recent Deposits -->
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Recent Deposits</CardTitle>
          <iconify-icon icon="lucide:arrow-down-to-line" class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">
            <span v-if="isLoading">...</span>
            <span v-else>{{ stats.recentDeposits }}</span>
          </div>
          <p class="text-xs text-muted-foreground">Income this month</p>
        </CardContent>
      </Card>
    </div>

    <!-- Quick Actions -->
    <Card>
      <CardHeader>
        <CardTitle>Quick Actions</CardTitle>
        <CardDescription>Common tasks to get started</CardDescription>
      </CardHeader>
      <CardContent>
        <div class="flex flex-wrap gap-4">
          <router-link :to="{ name: 'student_create' }">
            <button class="inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2">
              <iconify-icon icon="lucide:user-plus" class="mr-2 h-4 w-4" />
              Add Student
            </button>
          </router-link>
          
          <router-link :to="{ name: 'create_deposit' }">
            <button class="inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2">
              <iconify-icon icon="lucide:arrow-down-to-line" class="mr-2 h-4 w-4" />
              Record Income
            </button>
          </router-link>
          
          <router-link :to="{ name: 'create_payment' }">
            <button class="inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 bg-primary text-primary-foreground hover:bg-primary/90 h-10 px-4 py-2">
              <iconify-icon icon="lucide:arrow-up-from-line" class="mr-2 h-4 w-4" />
              Record Expense
            </button>
          </router-link>
          
          <router-link :to="{ name: 'ledger' }">
            <button class="inline-flex items-center justify-center rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 border border-input bg-background hover:bg-accent hover:text-accent-foreground h-10 px-4 py-2">
              <iconify-icon icon="lucide:list-tree" class="mr-2 h-4 w-4" />
              Chart of Accounts
            </button>
          </router-link>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
