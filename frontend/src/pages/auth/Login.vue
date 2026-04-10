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
const test = import.meta.env.VITE_TEST_FROM_ENV;

// Reactive references for form data and states
let data = ref<any>({});
let validationErr = ref<boolean>(false);
let loader = ref<boolean>(false);
let showPassword = ref<boolean>(false); // New ref for password visibility toggle

// Login request function
async function loginRequest() {
  loader.value = true;

  const result = await auth.login({
    email: data.value.email,
    password: data.value.password,
  });

  if (result) {
    loader.value = false;
    router.push({ name: "dashboard" });
  } else {
    toast({
      variant: "destructive",
      title: "Login Failed",
      description: result.message || "Invalid email or password. Please try again.",
    });
    document.getElementById("password")?.focus();
    validationErr.value = true;
    loader.value = false;
  }
}

// Function to handle input value changes
async function handleInputValue() {
  validationErr.value = false;
}

// Function to toggle password visibility
function togglePasswordVisibility() {
  showPassword.value = !showPassword.value;
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
  <div class="w-full ">
    <div class="h-screen flex items-center justify-center ">
      <div class="flex flex-col md:flex-row w-full overflow-hidden bg-custom-blue">
        <!-- Left Column: Static Image (Hidden on Small Screens) -->
        <div class="hidden md:block md:w-1/2">
          <img src="@/assets/600x580.png" alt="Login Image" class="w-full h-full object-cover">
        </div>

        <!-- Right Column: Login Form -->
        <div class="w-full md:w-1/2 p-8 flex flex-col justify-center mx-auto lg:w-96">
          <form>
            <div class="text-center">
              <span class="text-4xl font-bold text-black">Welcome</span>
              <p class="text-sm font-semibold text-black my-3">Sign in to your account.</p>
            </div>
            <div class="grid gap-4 mt-5">
              <Label for="email" :class="cn('', validationErr ? TextError : '')" class="text-black">Username</Label>
              <Input v-model="data.email" type="text" required @keyup.enter="loginRequest()" 
                :class="cn('', validationErr ? BorderError : '')" v-on:keyup="handleInputValue"
                v-on:focus="handleInputValue" />
            </div>
            <div class="grid gap-4 mt-3">
              <Label for="password" :class="cn('', validationErr ? TextError : '')" class="text-black">Password</Label>
              <div class="relative">
                <Input v-model="data.password" :type="showPassword ? 'text' : 'password'" id="password" required  autocomplete="current-password"
                  @keyup.enter="loginRequest()" :class="cn('', validationErr ? BorderError : '')"
                  v-on:keyup="handleInputValue" v-on:focus="handleInputValue"/>
                <button type="button" @click="togglePasswordVisibility"
                  class="absolute inset-y-0 right-0 pr-3 flex items-center text-sm leading-5">
                  <iconify-icon :icon="showPassword ? 'mdi:eye-off' : 'mdi:eye'"
                    class="w-5 h-5 text-gray-500"></iconify-icon>
                </button>
              </div>
            </div>

            <div class="grid gap-4 mt-9">
              <Button type="button" class="w-full" disabled v-if="loader == true">
                <iconify-icon icon="lucide:loader-circle" class="w-4 h-4 mr-2 animate-spin" />
                Please wait...
              </Button>
              <Button type="button" class="w-full" @click="loginRequest()" v-else>Login</Button>
            </div>

            <!-- Link to Register -->
            <div class="text-center mt-4">
              <p class="text-sm text-black">
                Don't have an account?
                <router-link :to="{ name: 'register' }" class="font-semibold text-blue-600 hover:underline">
                  Sign up
                </router-link>
              </p>
            </div>

          </form>
        </div>
      </div>
    </div>
  </div>
</template>