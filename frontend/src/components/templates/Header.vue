<script setup lang="ts">
import { Icon } from "@iconify/vue";
import { ref, computed } from "vue";
import { Menu, MenuButton, MenuItems, MenuItem } from "@headlessui/vue";
import { useSidebarStore } from "@/stores/sidebar";
import { useAuthStore } from "@/stores/auth";
import { useRoute } from "vue-router";
import router from "@/router";
const route = useRoute();

const sidebar = useSidebarStore();
const auth = useAuthStore();

const userNavigation: any[] = [
  // { name: "Your profile", href: "#" },
  // { name: "Sign out", href: "#" },
];

async function handleLogout() {
  await auth.logout();
  router.push({ name: "login" });
}
</script>

<template>
  <div :class="[
      'transition-all duration-300',
      sidebar.collapse ? 'lg:pl-20' : 'lg:pl-72',
    ]">
    <div class="
    sticky top-0 z-40 flex h-16 shrink-0 items-center gap-x-4 
    border-b border-gray-200 bg-white px-4 
    shadow-sm sm:gap-x-6 sm:px-6 lg:px-8
    ">
      <button type="button" class="-m-2.5 p-2.5 text-gray-700 lg:hidden" @click="sidebar.open">
        <span class="sr-only">Open sidebar</span>
        <Icon icon="heroicons:bars-3" class="h-6 w-6" />
      </button>

      <div class="h-6 w-px bg-gray-900/10 lg:hidden" aria-hidden="true" />

      <!-- Use mr-auto here -->
      <div class="ml-auto ">
        <div class="flex items-center gap-x-4 lg:gap-x-6 mr-auto">
          <button type="button" class="-m-2.5 p-2.5 text-gray-400 hover:text-gray-500">
            <span class="sr-only">View notifications</span>
            <Icon icon="heroicons:bell" class="h-6 w-6" />
          </button>

          <div class="hidden lg:block lg:h-6 lg:w-px lg:bg-gray-900/10" aria-hidden="true" />

          <Menu as="div" class="relative">
            <MenuButton class="-m-1.5 flex items-center p-1.5">
              <span class="sr-only">Open user menu</span>
              <!-- <img class="h-8 w-8 rounded-full bg-gray-50" src="@/assets/256x256.gif" alt="" /> -->
              {{ auth.user?.name || 'User' }}
              <span class="hidden lg:flex lg:items-center">
                <Icon icon="heroicons:chevron-down" class="ml-2 h-5 w-5 text-gray-400" />
              </span>
            </MenuButton>

            <transition enter-active-class="transition ease-out duration-100"
              enter-from-class="transform opacity-0 scale-95" enter-to-class="transform opacity-100 scale-100"
              leave-active-class="transition ease-in duration-75" leave-from-class="transform opacity-100 scale-100"
              leave-to-class="transform opacity-0 scale-95">
              <MenuItems
                class="absolute right-0 z-10 mt-2.5 w-32 origin-top-right rounded-md bg-white py-2 shadow-lg ring-1 ring-gray-900/5 focus:outline-none">
                <MenuItem v-for="item in userNavigation" :key="item.name" v-slot="{ active }">
                <a :href="item.href" :class="[
                  active ? 'bg-gray-50' : '',
                  'block px-3 py-1 text-sm leading-6 text-gray-900',
                ]">{{ item.name }}</a>
                </MenuItem>

                <!-- Remove 'active' check from the button class -->
                <button class="block px-3 py-1 text-sm leading-6 text-gray-900" @click="handleLogout()">
                  Sign Out
                </button>
              </MenuItems>

            </transition>
          </Menu>
        </div>
      </div>
    </div>
  </div>
</template>
