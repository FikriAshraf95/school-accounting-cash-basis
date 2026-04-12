<script setup>
import { ref } from "vue";
import {
  Dialog,
  DialogPanel,
  TransitionChild,
  TransitionRoot,
  Disclosure,
  DisclosureButton,
  DisclosurePanel,
} from "@headlessui/vue";

import { useSidebarStore } from "@/stores/sidebar";

const sidebar = useSidebarStore();

const navigation = ref([
  {
    name: "dashboard",
    displayName: "Dashboard",
    icon: "lucide:home",
    href: "#",
    current: sidebar.pageName == "dashboard",
  },
  
  // Payer Management
  {
    current: false,
    name: "payers_list",
    displayName: "Payers",
    icon: "lucide:users-round",
    href: "#",
  },

  // Students Management
  {
    displayName: "Students",
    current: false,
    icon: "lucide:users",
    children: [
      {
        name: "students_list",
        displayName: "All Students",
        icon: "lucide:list",
        href: "#",
      },
      {
        name: "classes_list",
        displayName: "Classes",
        icon: "lucide:school",
        href: "#",
      },
      {
        name: "students_reports",
        displayName: "Student Reports",
        icon: "lucide:file-text",
        href: "#",
      },
    ],
  },
  
  // Accounting
  {
    displayName: "Accounting",
    current: false,
    icon: "lucide:calculator",
    children: [
      {
        name: "view_main",
        displayName: "Business Info",
        icon: "lucide:building-2",
        href: "#",
      },
      {
        name: "deposit",
        displayName: "Income",
        icon: "lucide:arrow-down-to-line",
        href: "#",
      },
      {
        name: "payment",
        displayName: "Expenses",
        icon: "lucide:arrow-up-from-line",
        href: "#",
      },
      {
        name: "transactions_all",
        displayName: "All Transactions",
        icon: "lucide:list",
        href: "/accounting/transactions",
      },
      {
        name: "index_reports",
        displayName: "Reports",
        icon: "lucide:file-text",
        href: "#",
      },
      {
        name: "ledger",
        displayName: "Chart of Accounts",
        icon: "lucide:list-tree",
        href: "#",
      },
      {
        name: "close_ledger",
        displayName: "Year-End Closing",
        icon: "lucide:calendar-check",
        href: "#",
      },
    ],
  },
  
  // // Settings
  // {
  //   name: "accounting_settings",
  //   displayName: "Settings",
  //   icon: "lucide:settings",
  //   href: "#",
  //   current: sidebar.pageName == "accounting_settings",
  // },
]);
</script>

<template>
  <div>
    <!-- Mobile menu -->
    <TransitionRoot as="template" :show="sidebar.show">
      <Dialog class="relative z-50 lg:hidden" @close="sidebar.close">
        <TransitionChild 
          as="template" 
          enter="transition-opacity ease-linear duration-300" 
          enter-from="opacity-0"
          enter-to="opacity-100" 
          leave="transition-opacity ease-linear duration-300" 
          leave-from="opacity-100"
          leave-to="opacity-0"
        >
          <div class="fixed inset-0 bg-gray-900/80" />
        </TransitionChild>

        <div class="fixed inset-0 flex">
          <TransitionChild 
            as="template" 
            enter="transition ease-in-out duration-300 transform"
            enter-from="-translate-x-full" 
            enter-to="translate-x-0"
            leave="transition ease-in-out duration-300 transform" 
            leave-from="translate-x-0"
            leave-to="-translate-x-full"
          >
            <DialogPanel class="relative mr-16 flex w-full max-w-xs flex-1">
              <TransitionChild 
                as="template" 
                enter="ease-in-out duration-300" 
                enter-from="opacity-0"
                enter-to="opacity-100" 
                leave="ease-in-out duration-300" 
                leave-from="opacity-100" 
                leave-to="opacity-0"
              >
                <div class="absolute left-full top-0 flex w-16 justify-center pt-5">
                  <button type="button" class="-m-2.5 p-2.5" @click="sidebar.close">
                    <span class="sr-only">Close sidebar</span>
                    <iconify-icon icon="heroicons:x-mark" class="h-6 w-6 text-white" aria-hidden="true" />
                  </button>
                </div>
              </TransitionChild>
              
              <!-- Mobile Sidebar -->
              <div class="flex grow flex-col gap-y-5 overflow-y-auto bg-gray-900 px-6 pb-2 ring-1 ring-white/10">
                <div class="flex h-16 shrink-0 items-center">
                  <span class="text-white text-lg font-semibold">Ezi Account</span>
                </div>
                
                <nav class="flex flex-1 flex-col">
                  <ul role="list" class="flex flex-1 flex-col gap-y-7">
                    <li>
                      <ul role="list" class="-mx-2 space-y-1">
                        <li v-for="item in navigation" :key="item.displayName">
                          <!-- Single menu item -->
                          <router-link 
                            v-if="!item.children" 
                            :to="{ name: item.name }" 
                            :class="[
                              sidebar.pageName == item.name
                                ? 'bg-gray-800 text-white'
                                : 'text-gray-400 hover:bg-gray-800 hover:text-white',
                              'group flex gap-x-3 rounded-md p-2 text-sm font-semibold leading-6',
                            ]"
                          >
                            <iconify-icon :icon="item.icon" class="h-6 w-6 shrink-0" aria-hidden="true" />
                            {{ item.displayName }}
                          </router-link>
                          
                          <!-- Menu with children -->
                          <Disclosure as="div" v-else v-slot="{ open }">
                            <DisclosureButton 
                              :class="[
                                'text-gray-400 hover:bg-gray-800 hover:text-white',
                                'group flex w-full items-center gap-x-3 rounded-md p-2 text-sm font-semibold leading-6',
                              ]"
                            >
                              <iconify-icon 
                                icon="heroicons:chevron-right" 
                                :class="[
                                  open ? 'rotate-90 text-gray-500' : 'text-gray-400',
                                  'h-5 w-5 shrink-0',
                                ]" 
                                aria-hidden="true" 
                              />
                              <iconify-icon 
                                v-if="item.icon" 
                                :icon="item.icon" 
                                class="h-5 w-5 shrink-0" 
                                aria-hidden="true" 
                              />
                              {{ item.displayName }}
                            </DisclosureButton>
                            
                            <DisclosurePanel as="ul" class="mt-1 px-2">
                              <li v-for="subItem in item.children" :key="subItem.displayName">
                                <router-link :to="{ name: subItem.name }">
                                  <DisclosureButton 
                                    as="div" 
                                    :class="[
                                      sidebar.pageName == subItem.name
                                        ? 'bg-gray-800 text-white'
                                        : 'text-gray-400 hover:bg-gray-800 hover:text-white',
                                      'group flex gap-x-3 rounded-md p-2 pl-9 text-sm font-semibold leading-6',
                                    ]"
                                  >
                                    <iconify-icon 
                                      v-if="subItem.icon" 
                                      :icon="subItem.icon" 
                                      class="h-5 w-5 shrink-0" 
                                      aria-hidden="true" 
                                    />
                                    {{ subItem.displayName }}
                                  </DisclosureButton>
                                </router-link>
                              </li>
                            </DisclosurePanel>
                          </Disclosure>
                        </li>
                      </ul>
                    </li>
                  </ul>
                </nav>
              </div>
            </DialogPanel>
          </TransitionChild>
        </div>
      </Dialog>
    </TransitionRoot>

    <!-- Desktop sidebar -->
    <div
      :class="[
        'transition-all duration-300',
        sidebar.collapse ? 'lg:w-20' : 'lg:w-72',
        'hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:flex-col',
      ]"
    >
      <div class="flex grow flex-col gap-y-5 overflow-y-auto bg-gray-900 px-6 pb-4">
        <div class="flex h-16 shrink-0 items-center">
          <span class="text-white text-lg font-semibold mx-3" v-if="!sidebar.collapse">
            An&E Accounting
          </span>
          <span class="text-white text-lg font-semibold" v-else>
            EA
          </span>
        </div>
        
        <nav class="flex flex-1 flex-col">
          <ul role="list" class="flex flex-1 flex-col gap-y-7">
            <li>
              <ul role="list" class="-mx-1 space-y-1">
                <li v-for="item in navigation" :key="item.displayName">
                  <!-- Single menu item -->
                  <router-link 
                    v-if="!item.children" 
                    :to="{ name: item.name }" 
                    :class="[
                      sidebar.pageName == item.name
                        ? 'bg-gray-800 text-white'
                        : 'text-gray-400 hover:bg-gray-800 hover:text-white',
                      'group flex gap-x-3 rounded-md p-2 text-sm font-semibold leading-6',
                    ]"
                  >
                    <iconify-icon :icon="item.icon" class="h-6 w-6 shrink-0" aria-hidden="true" />
                    <span v-if="!sidebar.collapse">{{ item.displayName }}</span>
                  </router-link>
                  
                  <!-- Menu with children -->
                  <Disclosure as="div" v-else v-slot="{ open }">
                    <DisclosureButton 
                      :class="[
                        'text-gray-400 hover:bg-gray-800 hover:text-white',
                        'group flex w-full items-center gap-x-3 rounded-md p-2 text-sm font-semibold leading-6',
                      ]"
                    >
                      <iconify-icon 
                        icon="heroicons:chevron-right" 
                        :class="[
                          open ? 'rotate-90 text-gray-500' : 'text-gray-400',
                          'h-5 w-5 shrink-0',
                        ]" 
                        aria-hidden="true" 
                      />
                      <iconify-icon 
                        v-if="item.icon" 
                        :icon="item.icon" 
                        class="h-5 w-5 shrink-0" 
                        aria-hidden="true" 
                      />
                      <span v-if="!sidebar.collapse">{{ item.displayName }}</span>
                    </DisclosureButton>
                    
                    <DisclosurePanel as="ul" class="mt-1">
                      <li v-for="subItem in item.children" :key="subItem.displayName" class="py-1">
                        <router-link :to="{ name: subItem.name }">
                          <DisclosureButton 
                            as="div" 
                            :class="[
                              sidebar.pageName == subItem.name
                                ? 'bg-gray-800 text-white'
                                : 'text-gray-400 hover:bg-gray-800 hover:text-white',
                              'group flex gap-x-3 rounded-md p-2 pl-9 text-sm font-semibold leading-6',
                            ]"
                          >
                            <iconify-icon 
                              v-if="subItem.icon" 
                              :icon="subItem.icon" 
                              class="h-5 w-5 shrink-0" 
                              aria-hidden="true" 
                            />
                            <span v-if="!sidebar.collapse">{{ subItem.displayName }}</span>
                          </DisclosureButton>
                        </router-link>
                      </li>
                    </DisclosurePanel>
                  </Disclosure>
                </li>
              </ul>
            </li>
            
            <!-- Collapse toggle button -->
            <li class="mt-auto">
              <button
                @click="sidebar.toggleCollapse"
                class="group -mx-1 flex w-full gap-x-3 rounded-md p-2 text-sm font-semibold leading-6 text-gray-400 hover:bg-gray-800 hover:text-white"
              >
                <iconify-icon 
                  :icon="sidebar.collapse ? 'lucide:chevrons-right' : 'lucide:chevrons-left'" 
                  class="h-6 w-6 shrink-0" 
                  aria-hidden="true" 
                />
                <span v-if="!sidebar.collapse">Collapse</span>
              </button>
            </li>
          </ul>
        </nav>
      </div>
    </div>
  </div>
</template>

<style scoped>
::-webkit-scrollbar {
  width: 5px;
  height: 5px;
}

::-webkit-scrollbar-track {
  background: #1f2937;
}

::-webkit-scrollbar-thumb {
  background: #4b5563;
}

::-webkit-scrollbar-thumb:hover {
  background: #6b7280;
}
</style>