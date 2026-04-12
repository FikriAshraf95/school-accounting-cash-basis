<script setup lang="ts">
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="space-y-6">
    <Card>
      <CardHeader>
        <CardTitle class="text-2xl">404 - Page Not Found</CardTitle>
        <CardDescription>
          The page you are looking for doesn't exist or has been moved.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div class="flex flex-col items-center py-10 gap-6">
          <div class="p-6 bg-muted rounded-full">
            <iconify-icon icon="lucide:file-question" class="h-16 w-16 text-muted-foreground" />
          </div>
          <div class="text-center space-y-2">
            <p class="text-muted-foreground">
              You may have followed a broken link or entered an address that doesn't exist.
            </p>
          </div>
          <div class="flex gap-3">
            <Button @click="router.back()">
              <iconify-icon icon="lucide:arrow-left" class="mr-2 h-4 w-4" />
              Go Back
            </Button>
            <Button variant="outline" @click="router.push({ name: 'dashboard' })">
              <iconify-icon icon="lucide:home" class="mr-2 h-4 w-4" />
              Go to Dashboard
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
