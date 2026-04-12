import { ref, computed, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAPI } from "@/services/api";

export interface TableConfig {
  page: number;
  perPage: number;
  sortBy: string;
  sortDesc: boolean;
  paginate: boolean;
  keyword: string;
}

export interface PaginationState {
  totalItems: number;
  currentPage: number;
  lastPage: number;
  from: number;
  to: number;
}

export interface UsePaginationOptions {
  endpoint: string;
  defaultSortBy?: string;
  defaultSortDesc?: boolean;
  defaultPerPage?: number;
  additionalParams?: () => Record<string, any>;
  onSuccess?: (data: any) => void;
  onError?: (error: string) => void;
}

export function usePagination(options: UsePaginationOptions) {
  const router = useRouter();
  const route = useRoute();

  // Table configuration
  const tableConfig = ref<TableConfig>({
    page: 1,
    perPage: options.defaultPerPage || 10,
    sortBy: options.defaultSortBy || "created_at",
    sortDesc: options.defaultSortDesc !== undefined ? options.defaultSortDesc : false,
    paginate: true,
    keyword: "",
  });

  // State
  const items = ref<any[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Pagination state
  const paginationState = ref<PaginationState>({
    totalItems: 0,
    currentPage: 1,
    lastPage: 1,
    from: 0,
    to: 0,
  });

  // Search debounce timer
  let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;

  // Computed
  const queryParams = computed(() => {
    const params: any = {
      page: tableConfig.value.page,
      per_page: tableConfig.value.perPage,
      sortBy: tableConfig.value.sortBy,
      sortDesc: tableConfig.value.sortDesc ? "true" : "false",
      paginate: tableConfig.value.paginate ? "true" : "false",
    };

    if (tableConfig.value.keyword) {
      params.keyword = tableConfig.value.keyword;
    }

    // Add additional params if provided
    if (options.additionalParams) {
      Object.assign(params, options.additionalParams());
    }

    return params;
  });

  const perPageOptions = [10, 25, 50, 100];

  const paginationInfo = computed(() => {
    if (!tableConfig.value.paginate || items.value.length === 0) {
      return `Total of ${items.value.length} item(s)`;
    }
    return `Showing ${paginationState.value.from} to ${paginationState.value.to} of ${paginationState.value.totalItems} item(s)`;
  });

  // Methods
  async function fetchData() {
    loading.value = true;
    error.value = null;
    const api = useAPI();

    try {
      const response = await api.get<any>(options.endpoint, queryParams.value);

      if (response.success) {
        if (tableConfig.value.paginate) {
          items.value = response.data.payload.data;
          paginationState.value = {
            totalItems: response.payload.total,
            currentPage: response.payload.current_page,
            lastPage: response.payload.last_page,
            from: response.payload.from || 0,
            to: response.payload.to || 0,
          };
        } else {
          items.value = response.data.payload;
          paginationState.value.totalItems = items.value.length;
        }

        if (options.onSuccess) {
          options.onSuccess(response);
        }
      } else {
        const errorMsg = response.data.error || "Failed to load data";
        error.value = errorMsg;
        if (options.onError) {
          options.onError(errorMsg);
        }
      }
    } catch (err: any) {
      const errorMsg = err.response?.data?.error || err.message || "Failed to load data";
      error.value = errorMsg;
      if (options.onError) {
        options.onError(errorMsg);
      }
    } finally {
      loading.value = false;
    }
  }

  function handleSort(column: string) {
    if (tableConfig.value.sortBy === column) {
      tableConfig.value.sortDesc = !tableConfig.value.sortDesc;
    } else {
      tableConfig.value.sortBy = column;
      tableConfig.value.sortDesc = false;
    }
    tableConfig.value.page = 1;
  }

  function handlePageChange(page: number) {
    tableConfig.value.page = page;
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function handlePerPageChange(value: string) {
    tableConfig.value.perPage = parseInt(value);
    tableConfig.value.page = 1;
  }

  function handleSearch(value: string | number) {
    if (searchDebounceTimer) {
      clearTimeout(searchDebounceTimer);
    }

    searchDebounceTimer = setTimeout(() => {
      tableConfig.value.keyword = String(value);
      tableConfig.value.page = 1;
    }, 300);
  }

  function resetFilters(additionalResets?: () => void) {
    tableConfig.value.keyword = "";
    tableConfig.value.page = 1;
    if (additionalResets) {
      additionalResets();
    }
  }

  function getPaginationPages() {
    const pages: (number | string)[] = [];
    const totalPages = paginationState.value.lastPage;
    const current = paginationState.value.currentPage;

    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);
      if (current > 3) pages.push("...");
      const start = Math.max(2, current - 1);
      const end = Math.min(totalPages - 1, current + 1);
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      if (current < totalPages - 2) pages.push("...");
      pages.push(totalPages);
    }

    return pages;
  }

  function initFromUrl() {
    const urlParams = route.query;
    if (urlParams.page) tableConfig.value.page = parseInt(urlParams.page as string);
    if (urlParams.per_page) tableConfig.value.perPage = parseInt(urlParams.per_page as string);
    if (urlParams.sortBy) tableConfig.value.sortBy = urlParams.sortBy as string;
    if (urlParams.sortDesc) tableConfig.value.sortDesc = urlParams.sortDesc === "true";
    if (urlParams.keyword) tableConfig.value.keyword = urlParams.keyword as string;
  }

  // Watch for changes and update URL
  watch(
    () => queryParams.value,
    (newParams) => {
      router.replace({ query: newParams }).catch(() => {});
      fetchData();
    },
    { deep: true }
  );

  return {
    // State
    tableConfig,
    items,
    loading,
    error,
    paginationState,

    // Computed
    queryParams,
    perPageOptions,
    paginationInfo,

    // Methods
    fetchData,
    handleSort,
    handlePageChange,
    handlePerPageChange,
    handleSearch,
    resetFilters,
    getPaginationPages,
    initFromUrl,
  };
}
