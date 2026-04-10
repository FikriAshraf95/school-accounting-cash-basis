<script setup lang="ts">
import { ref, computed } from "vue";
import { useToast } from "@/components/ui/toast/use-toast";
import { useAPI } from "@/services/api";

const api = useAPI();
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import { Icon } from "@iconify/vue";
import { Separator } from "@/components/ui/separator";

const { toast } = useToast();

// State
const showDialog = ref(false);
const showSummaryDialog = ref(false);
const yearToClose = ref(new Date().getFullYear() - 1); // Default to last year
const closing = ref(false);
const summary = ref<any>(null);

// Available years (last 5 years)
const availableYears = computed(() => {
  const currentYear = new Date().getFullYear();
  return Array.from({ length: 5 }, (_, i) => currentYear - i);
});

// Open confirmation dialog
function openCloseDialog() {
  showDialog.value = true;
}

// Fetch year-end summary before closing
async function fetchYearSummary() {
  try {
    const response = await api.get<any>(`/ledgers/summary/${yearToClose.value}`);
    summary.value = response;
    showDialog.value = false;
    showSummaryDialog.value = true;
  } catch (error: any) {
    toast({
      title: "Error",
      description: error.response?.data?.message || error.message || "Failed to load year-end summary",
      variant: "destructive",
    });
  }
}

// Close the year
async function closeYear() {
  closing.value = true;

  try {
    const result = await api.post<any>('/ledgers/year-end-close', {
      year: yearToClose.value,
    });

    toast({
      title: "Success",
      description: `Year ${yearToClose.value} closed successfully. Net ${result.data.net_profit_loss >= 0 ? 'Profit' : 'Loss'}: RM ${Math.abs(result.data.net_profit_loss).toFixed(2)}`,
    });

    showSummaryDialog.value = false;

    // Emit event to refresh parent if needed
    emit('yearClosed', result);

  } catch (error: any) {
    toast({
      title: "Error",
      description: error.response?.data?.message || error.message || "Failed to close year",
      variant: "destructive",
    });
  } finally {
    closing.value = false;
  }
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-MY', {
    style: 'currency',
    currency: 'MYR',
    minimumFractionDigits: 2,
  }).format(amount);
}

const emit = defineEmits(['yearClosed']);
</script>

<template>
  <Card>
    <CardHeader>
      <div class="flex items-center justify-between">
        <div>
          <CardTitle class="flex items-center gap-2">
            <Icon icon="lucide:calendar-check" class="h-5 w-5 text-orange-600" />
            Year-End Closing
          </CardTitle>
          <CardDescription>
            Close accounting year and transfer profit/loss to retained earnings
          </CardDescription>
        </div>
        <Button @click="openCloseDialog" variant="outline" class="gap-2">
          <Icon icon="lucide:lock" class="h-4 w-4" />
          Close Year
        </Button>
      </div>
    </CardHeader>
    <CardContent>
      <div class="p-4 bg-orange-50 rounded-lg border border-orange-200">
        <div class="flex items-start gap-3">
          <Icon icon="lucide:alert-triangle" class="h-5 w-5 text-orange-600 mt-0.5" />
          <div class="text-sm text-orange-900">
            <p class="font-medium mb-1">Important: Year-End Closing Process</p>
            <ul class="list-disc list-inside space-y-1">
              <li>All revenue and expense accounts will be reset to zero</li>
              <li>Net profit or loss will be transferred to Retained Earnings</li>
              <li>Asset, liability, and equity balances will carry forward</li>
              <li>This action creates permanent journal entries</li>
              <li>Make sure all transactions for the year are recorded first</li>
            </ul>
          </div>
        </div>
      </div>
    </CardContent>
  </Card>

  <!-- Initial Confirmation Dialog -->
  <AlertDialog v-model:open="showDialog">
    <AlertDialogContent class="max-w-[80vw] max-h-[85vh] overflow-y-auto">
      <AlertDialogHeader>
        <AlertDialogTitle class="flex items-center gap-2">
          <Icon icon="lucide:alert-circle" class="h-5 w-5 text-orange-600" />
          Close Accounting Year
        </AlertDialogTitle>
        <AlertDialogDescription class="space-y-3">
          <p>You are about to close the accounting year. This will:</p>
          
          <div class="p-3 bg-blue-50 rounded-lg border border-blue-200 text-blue-900 text-sm">
            <p class="font-medium mb-2">✓ What will happen:</p>
            <ul class="list-disc list-inside space-y-1">
              <li>Close all revenue accounts to zero</li>
              <li>Close all expense accounts to zero</li>
              <li>Calculate net profit or loss</li>
              <li>Transfer result to Retained Earnings</li>
              <li>Create closing journal entries</li>
            </ul>
          </div>

          <div class="p-3 bg-green-50 rounded-lg border border-green-200 text-green-900 text-sm">
            <p class="font-medium mb-2">✓ What will NOT change:</p>
            <ul class="list-disc list-inside space-y-1">
              <li>Asset balances (Cash, Bank, etc.)</li>
              <li>Liability balances (Loans, Payables, etc.)</li>
              <li>Student balances</li>
              <li>Vendor/Payer balances</li>
            </ul>
          </div>

          <div class="space-y-2 pt-3">
            <label class="text-sm font-medium text-gray-700">Select Year to Close:</label>
            <Select :model-value="`${yearToClose}`">
              <SelectTrigger>
                <SelectValue placeholder="Select year" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem 
                  v-for="year in availableYears" 
                  :key="year" 
                  :value="`${year}`"
                >
                  {{ year }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div class="p-3 bg-red-50 rounded-lg border border-red-200 text-red-900 text-sm">
            <div class="flex items-start gap-2">
              <Icon icon="lucide:alert-triangle" class="h-4 w-4 mt-0.5" />
              <div>
                <p class="font-medium">Warning: This action cannot be undone!</p>
                <p class="mt-1">Make sure all transactions for year {{ yearToClose }} are recorded.</p>
              </div>
            </div>
          </div>
        </AlertDialogDescription>
      </AlertDialogHeader>
      <AlertDialogFooter>
        <AlertDialogCancel>Cancel</AlertDialogCancel>
        <AlertDialogAction 
          @click="fetchYearSummary"
          class="bg-orange-600 hover:bg-orange-700"
        >
          Continue to Summary
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>

  <!-- Summary & Final Confirmation Dialog -->
  <AlertDialog v-model:open="showSummaryDialog">
    <AlertDialogContent class="max-w-[80vw] max-h-[85vh] overflow-y-auto">
      <AlertDialogHeader>
        <AlertDialogTitle class="flex items-center gap-2">
          <Icon icon="lucide:file-text" class="h-5 w-5 text-blue-600" />
          Year-End Summary for {{ yearToClose }}
        </AlertDialogTitle>
        <AlertDialogDescription>
          Review the summary before proceeding with year-end closing
        </AlertDialogDescription>
      </AlertDialogHeader>

      <div v-if="summary" class="space-y-4 py-4">
        <!-- Revenue Summary -->
        <div>
          <h4 class="font-semibold text-sm text-gray-700 mb-2 flex items-center gap-2">
            <Icon icon="lucide:trending-up" class="h-4 w-4 text-green-600" />
            Revenue Accounts
          </h4>
          <div class="bg-green-50 rounded-lg p-3 border border-green-200">
            <div v-if="summary.revenue_accounts?.length > 0" class="space-y-1">
              <div 
                v-for="account in summary.revenue_accounts" 
                :key="account.id"
                class="flex justify-between text-sm"
              >
                <span class="text-gray-700">{{ account.name }}</span>
                <span class="font-medium text-green-700">{{ formatCurrency(account.balance) }}</span>
              </div>
              <Separator class="my-2" />
              <div class="flex justify-between text-sm font-bold">
                <span>Total Revenue:</span>
                <span class="text-green-700">{{ formatCurrency(summary.total_revenue) }}</span>
              </div>
            </div>
            <p v-else class="text-sm text-gray-500">No revenue accounts with balance</p>
          </div>
        </div>

        <!-- Expense Summary -->
        <div>
          <h4 class="font-semibold text-sm text-gray-700 mb-2 flex items-center gap-2">
            <Icon icon="lucide:trending-down" class="h-4 w-4 text-red-600" />
            Expense Accounts
          </h4>
          <div class="bg-red-50 rounded-lg p-3 border border-red-200">
            <div v-if="summary.expense_accounts?.length > 0" class="space-y-1">
              <div 
                v-for="account in summary.expense_accounts" 
                :key="account.id"
                class="flex justify-between text-sm"
              >
                <span class="text-gray-700">{{ account.name }}</span>
                <span class="font-medium text-red-700">{{ formatCurrency(account.balance) }}</span>
              </div>
              <Separator class="my-2" />
              <div class="flex justify-between text-sm font-bold">
                <span>Total Expenses:</span>
                <span class="text-red-700">{{ formatCurrency(summary.total_expenses) }}</span>
              </div>
            </div>
            <p v-else class="text-sm text-gray-500">No expense accounts with balance</p>
          </div>
        </div>

        <!-- Net Result -->
        <div class="p-4 rounded-lg border-2" :class="[
          summary.net_profit_loss >= 0 
            ? 'bg-green-100 border-green-400' 
            : 'bg-red-100 border-red-400'
        ]">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-gray-700 mb-1">
                Net {{ summary.net_profit_loss >= 0 ? 'Profit' : 'Loss' }} for {{ yearToClose }}
              </p>
              <p class="text-2xl font-bold" :class="[
                summary.net_profit_loss >= 0 ? 'text-green-700' : 'text-red-700'
              ]">
                {{ formatCurrency(Math.abs(summary.net_profit_loss)) }}
              </p>
            </div>
            <Icon 
              :icon="summary.net_profit_loss >= 0 ? 'lucide:trending-up' : 'lucide:trending-down'" 
              class="h-12 w-12 opacity-50"
              :class="summary.net_profit_loss >= 0 ? 'text-green-600' : 'text-red-600'"
            />
          </div>
        </div>

        <!-- What Will Happen -->
        <div class="p-3 bg-blue-50 rounded-lg border border-blue-200 text-sm text-blue-900">
          <p class="font-medium mb-2">After closing:</p>
          <ul class="list-disc list-inside space-y-1">
            <li>All revenue accounts will become RM 0.00</li>
            <li>All expense accounts will become RM 0.00</li>
            <li>Retained Earnings will {{ summary.net_profit_loss >= 0 ? 'increase' : 'decrease' }} by {{ formatCurrency(Math.abs(summary.net_profit_loss)) }}</li>
            <li>Closing journal entries will be created on Dec 31, {{ yearToClose }}</li>
          </ul>
        </div>

        <!-- Final Warning -->
        <div class="p-3 bg-red-50 rounded-lg border border-red-200">
          <div class="flex items-start gap-2 text-sm text-red-900">
            <Icon icon="lucide:shield-alert" class="h-5 w-5 mt-0.5" />
            <div>
              <p class="font-bold">Final Warning</p>
              <p class="mt-1">This action is permanent and cannot be reversed. Please ensure all data is correct before proceeding.</p>
            </div>
          </div>
        </div>
      </div>

      <AlertDialogFooter>
        <AlertDialogCancel :disabled="closing">Cancel</AlertDialogCancel>
        <AlertDialogAction 
          @click="closeYear"
          :disabled="closing"
          class="bg-red-600 hover:bg-red-700"
        >
          <Icon 
            :icon="closing ? 'lucide:loader-2' : 'lucide:lock'" 
            class="mr-2 h-4 w-4"
            :class="closing ? 'animate-spin' : ''"
          />
          {{ closing ? 'Closing...' : 'Close Year ' + yearToClose }}
        </AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>