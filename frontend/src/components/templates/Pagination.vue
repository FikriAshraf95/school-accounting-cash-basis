<script setup lang="ts">
// import { defineProps, defineEmits } from "vue";
import {
  Pagination,
  PaginationEllipsis,
  PaginationFirst,
  PaginationLast,
  PaginationList,
  PaginationListItem,
  PaginationNext,
  PaginationPrev,
} from "@/components/ui/pagination";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";

const props = defineProps({
  pageSizeOptions: {
    type: Array as () => number[],
    default: () => [10, 20, 30, 50],
  },
  pageSize: {
    type: Number,
    default: 10,
  },
  totalCount: {
    type: Number,
    default: 0,
  },
  totalPages: {
    type: Number,
    default: 0,
  },
  pageNumber: {
    type: Number,
    default: 1,
  },
  hidePageSizeSelector: {
    type: Boolean,
    default: false,
  },
});

const emits = defineEmits(["updatePageNumber", "updatePageSize"]);

function handlePageChange(newPageNumber: any) {
  emits("updatePageNumber", newPageNumber);
}

function handlePageSizeChange(newPageSize: string) {
  emits("updatePageSize", Number(newPageSize));
}
</script>

<template>
  <div class="flex items-center gap-4">
    <span v-if="!props.hidePageSizeSelector" class="flex items-center gap-2">
      <p class="text-sm text-gray-600">Show</p>
      <Select :model-value="`${props.pageSize}`" @update:model-value="handlePageSizeChange">
        <SelectTrigger class="h-8 w-[70px]">
          <SelectValue :placeholder="`${props.pageSize}`" />
        </SelectTrigger>
        <SelectContent side="top">
          <SelectItem v-for="size in props.pageSizeOptions" :key="size" :value="`${size}`">
            {{ size }}
          </SelectItem>
        </SelectContent>
      </Select>
    </span>
    <Pagination
      v-slot="{ page }"
      :total="totalCount"
      :itemsPerPage="pageSize"
      :sibling-count="1"
      show-edges
      :default-page="pageNumber"
      @update:page="handlePageChange"
    >
      <PaginationList v-slot="{ items }" class="flex items-center gap-1">
        <PaginationFirst />
        <PaginationPrev />
        <template v-for="(item, index) in items">
          <PaginationListItem
            v-if="item.type === 'page'"
            :key="index"
            :value="item.value"
            as-child
          >
            <Button
              class="w-10 h-10 p-0"
              :variant="item.value === page ? 'default' : 'outline'"
            >
              {{ item.value }}
            </Button>
          </PaginationListItem>
          <PaginationEllipsis v-else :key="item.type" :index="index" />
        </template>

        <PaginationNext />
        <PaginationLast />
      </PaginationList>
    </Pagination>
  </div>
</template>