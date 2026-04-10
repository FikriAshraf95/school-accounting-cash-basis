<script setup lang="ts">
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "@/components/ui/toast/use-toast";
import { ref } from "vue";
import { cn } from "@/lib/utils";
import router from "@/router";
import { useAuthStore } from '@/stores/auth';
import { useSidebarStore } from "@/stores/sidebar";
import { useRoute } from "vue-router";

// Initialize stores and route
const route = useRoute();
const sidebar = useSidebarStore();
sidebar.setPageName(route.name);
const auth = useAuthStore();

// Reactive references for form data and states
let data = ref<any>({});
let validationErrors = ref<Record<string, string[]>>({});
let loader = ref<boolean>(false);
let showPassword = ref<boolean>(false);
let showPasswordConfirm = ref<boolean>(false);

// Register request function
async function registerRequest() {
  loader.value = true;
  validationErrors.value = {};

  const result = await auth.register({
    name: data.value.name,
    username: data.value.username,
    email: data.value.email,
    password: data.value.password,
    password_confirmation: data.value.password_confirmation,
  });

  if (result.success) {
    toast({
      title: "Registration Successful",
      description: "Welcome! Your account has been created.",
    });
    loader.value = false;
    router.push({ name: "login" });
  } else {
    if (result.errors) {
      validationErrors.value = result.errors;
    }
    toast({
      variant: "destructive",
      title: "Registration Failed",
      description: result.message || "Please check your information and try again.",
    });
    loader.value = false;
  }
}

// Function to handle input value changes
function handleInputValue(field: string) {
  if (validationErrors.value[field]) {
    delete validationErrors.value[field];
  }
}

// Function to toggle password visibility
function togglePasswordVisibility() {
  showPassword.value = !showPassword.value;
}

function togglePasswordConfirmVisibility() {
  showPasswordConfirm.value = !showPasswordConfirm.value;
}

// Helper to check if field has error
function hasError(field: string): boolean {
  return !!validationErrors.value[field];
}

// Helper to get error message
function getError(field: string): string {
  return validationErrors.value[field]?.[0] || '';
}

// Error classes for validation
let TextError = "text-rose-500";
let BorderError = "border-rose-500";
</script>

<style>
input::-ms-reveal,
input::-ms-clear {
  display: none;
}
</style>

<template>
  <div class="w-full">
    <div class="h-screen flex items-center justify-center">
      <div class="flex flex-col md:flex-row w-full overflow-hidden bg-custom-blue">
        <!-- Left Column: Static Image (Hidden on Small Screens) -->
        <div class="hidden md:block md:w-1/2">
          <img src="@/assets/600x580.png" alt="Register Image" class="w-full h-full object-cover">
        </div>

        <!-- Right Column: Register Form -->
        <div class="w-full md:w-1/2 p-8 flex flex-col justify-center mx-auto lg:w-96">
          <form @submit.prevent="registerRequest">
            <div class="text-center">
              <span class="text-4xl font-bold text-black">Create Account</span>
              <p class="text-sm font-semibold text-black my-3">Sign up for a new account.</p>
            </div>

            <!-- Name Field -->
            <div class="grid gap-2 mt-5">
              <Label for="name" :class="cn('', hasError('name') ? TextError : '')" class="text-black">Name</Label>
              <Input
                v-model="data.name"
                type="text"
                id="name"
                required
                autocomplete="name"
                :class="cn('', hasError('name') ? BorderError : '')"
                @keyup="handleInputValue('name')"
                @focus="handleInputValue('name')"
              />
              <span v-if="hasError('name')" class="text-xs text-rose-500">{{ getError('name') }}</span>
            </div>

            <!-- Username Field -->
            <div class="grid gap-2 mt-3">
              <Label for="username" :class="cn('', hasError('username') ? TextError : '')" class="text-black">Username</Label>
              <Input
                v-model="data.username"
                type="text"
                id="username"
                required
                autocomplete="username"
                :class="cn('', hasError('username') ? BorderError : '')"
                @keyup="handleInputValue('username')"
                @focus="handleInputValue('username')"
              />
              <span v-if="hasError('username')" class="text-xs text-rose-500">{{ getError('username') }}</span>
            </div>

            <!-- Email Field -->
            <div class="grid gap-2 mt-3">
              <Label for="email" :class="cn('', hasError('email') ? TextError : '')" class="text-black">Email</Label>
              <Input
                v-model="data.email"
                type="email"
                id="email"
                required
                autocomplete="email"
                :class="cn('', hasError('email') ? BorderError : '')"
                @keyup="handleInputValue('email')"
                @focus="handleInputValue('email')"
              />
              <span v-if="hasError('email')" class="text-xs text-rose-500">{{ getError('email') }}</span>
            </div>

            <!-- Password Field -->
            <div class="grid gap-2 mt-3">
              <Label for="password" :class="cn('', hasError('password') ? TextError : '')" class="text-black">Password</Label>
              <div class="relative">
                <Input
                  v-model="data.password"
                  :type="showPassword ? 'text' : 'password'"
                  id="password"
                  required
                  autocomplete="new-password"
                  :class="cn('', hasError('password') ? BorderError : '')"
                  @keyup="handleInputValue('password')"
                  @focus="handleInputValue('password')"
                />
                <button
                  type="button"
                  @click="togglePasswordVisibility"
                  class="absolute inset-y-0 right-0 pr-3 flex items-center text-sm leading-5"
                >
                  <iconify-icon :icon="showPassword ? 'mdi:eye-off' : 'mdi:eye'" class="w-5 h-5 text-gray-500"></iconify-icon>
                </button>
              </div>
              <span v-if="hasError('password')" class="text-xs text-rose-500">{{ getError('password') }}</span>
            </div>

            <!-- Confirm Password Field -->
            <div class="grid gap-2 mt-3">
              <Label for="password_confirmation" :class="cn('', hasError('password') ? TextError : '')" class="text-black">Confirm Password</Label>
              <div class="relative">
                <Input
                  v-model="data.password_confirmation"
                  :type="showPasswordConfirm ? 'text' : 'password'"
                  id="password_confirmation"
                  required
                  autocomplete="new-password"
                  :class="cn('', hasError('password') ? BorderError : '')"
                  @keyup.enter="registerRequest()"
                />
                <button
                  type="button"
                  @click="togglePasswordConfirmVisibility"
                  class="absolute inset-y-0 right-0 pr-3 flex items-center text-sm leading-5"
                >
                  <iconify-icon :icon="showPasswordConfirm ? 'mdi:eye-off' : 'mdi:eye'" class="w-5 h-5 text-gray-500"></iconify-icon>
                </button>
              </div>
            </div>

            <!-- Submit Button -->
            <div class="grid gap-4 mt-9">
              <Button type="submit" class="w-full" disabled v-if="loader">
                <iconify-icon icon="lucide:loader-circle" class="w-4 h-4 mr-2 animate-spin" />
                Please wait...
              </Button>
              <Button type="submit" class="w-full" v-else>Register</Button>
            </div>

            <!-- Link to Login -->
            <div class="text-center mt-4">
              <p class="text-sm text-black">
                Already have an account?
                <router-link :to="{ name: 'login' }" class="font-semibold text-blue-600 hover:underline">
                  Sign in
                </router-link>
              </p>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
