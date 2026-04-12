import { useAuthStore } from "@/stores/auth";

// Authentication guard
const requireAuth = async (to: any, from: any, next: any) => {
  const authStore = useAuthStore();
  const isAuth = authStore.isAuthenticated || await authStore.checkAuth();

  if (isAuth) {
    next();
  } else {
    next("/login");
  }
};

const redirectIfAuthenticated = async (to: any, from: any, next: any) => {
  const authStore = useAuthStore();
  const isAuth = authStore.isAuthenticated || await authStore.checkAuth();
  if (isAuth) {
    next("/dashboard");
  } else {
    next();
  }
};

const routes = [
  {
    path: "/",
    redirect: "/login",
  },
  
  // Authentication
  {
    path: "/login",
    name: "login",
    component: () => import("@/pages/auth/Login.vue"),
    beforeEnter: redirectIfAuthenticated,
  },
  {
    path: "/register",
    name: "register",
    component: () => import("@/pages/auth/Register.vue"),
    beforeEnter: redirectIfAuthenticated,
  },
  
  // Public routes
  {
    path: "/public",
    component: () => import("@/layouts/PublicLayout.vue"),
    children: [],
  },
  
  // Dashboard
  {
    path: "/",
    component: () => import("@/layouts/AuthLayout.vue"),
    beforeEnter: requireAuth,
    children: [
      {
        name: "dashboard",
        path: "/dashboard",
        component: () => import("@/pages/dashboard/Index.vue"),
      },
    ],
  },
  
  // Students Module - Week 3
  // {
  //   path: "/students",
  //   component: () => import("@/layouts/AuthLayout.vue"),
  //   beforeEnter: requireAuth,
  //   children: [
  //     {
  //       name: "students_list",
  //       path: "",
  //       component: () => import("@/pages/modules/students/Index.vue"),
  //     },
  //     {
  //       name: "student_create",
  //       path: "create",
  //       component: () => import("@/pages/modules/students/Detail.vue"),
  //     },
  //     {
  //       name: "student_edit",
  //       path: ":id/edit",
  //       component: () => import("@/pages/modules/students/Detail.vue"),
  //     },
  //     {
  //       name: "student_view",
  //       path: ":id",
  //       component: () => import("@/pages/modules/students/View.vue"),
  //     },
  //     // Reports
  //     {
  //       name: "students_reports",
  //       path: "reports",
  //       component: () => import("@/pages/modules/student_reports/Index.vue"),
  //     },
  //   ],
  // },

  // Payer Module - Week 3
  // {
  //   path: "/payer",
  //   component: () => import("@/layouts/AuthLayout.vue"),
  //   beforeEnter: requireAuth,
  //   children: [
  //     {
  //       name: "payers_list",
  //       path: "",
  //       component: () => import("@/pages/modules/payer/Index.vue"),
  //     },
  //     {
  //       name: "payer_create",
  //       path: "create",
  //       component: () => import("@/pages/modules/payer/Detail.vue"),
  //     },
  //     {
  //       name: "payer_edit",
  //       path: ":id/edit",
  //       component: () => import("@/pages/modules/payer/Detail.vue"),
  //     },
  //     {
  //       name: "payer_view",
  //       path: ":id",
  //       component: () => import("@/pages/modules/payer/View.vue"),
  //     },
  //   ]
  // },
  
  // Classes Module
  {
    path: "/classes",
    component: () => import("@/layouts/AuthLayout.vue"),
    beforeEnter: requireAuth,
    children: [
      {
        name: "classes_list",
        path: "",
        component: () => import("@/pages/modules/classes/Index.vue"),
      },
      {
        name: "class_create",
        path: "create",
        component: () => import("@/pages/modules/classes/Detail.vue"),
      },
      {
        name: "class_edit",
        path: ":id/edit",
        component: () => import("@/pages/modules/classes/Detail.vue"),
      },
      {
        name: "class_view",
        path: ":id",
        component: () => import("@/pages/modules/classes/View.vue"),
      },
    ],
  },
  
  // Accounting Module
  {
    path: "/accounting",
    component: () => import("@/layouts/AuthLayout.vue"),
    beforeEnter: requireAuth,
    children: [
      // Main/Business
      {
        name: "view_main",
        path: "main/view",
        component: () => import("@/pages/modules/accounting/main/View.vue"),
      },
      {
        name: "edit_main",
        path: "main/edit/:id",
        component: () => import("@/pages/modules/accounting/main/Detail.vue"),
      },

      // Income/Deposits - Week 4
      // {
      //   name: "deposit",
      //   path: "income",
      //   component: () => import("@/pages/modules/accounting/deposit/Index.vue"),
      // },
      // {
      //   name: "create_deposit",
      //   path: "income/create",
      //   component: () => import("@/pages/modules/accounting/deposit/Detail.vue"),
      // },
      // {
      //   name: "edit_deposit",
      //   path: "income/edit/:id",
      //   component: () => import("@/pages/modules/accounting/deposit/Detail.vue"),
      // },
      // {
      //   name: "view_deposit",
      //   path: "income/view/:id",
      //   component: () => import("@/pages/modules/accounting/deposit/View.vue"),
      // },

      // Expenses/Payments - Week 4
      // {
      //   name: "payment",
      //   path: "expenses",
      //   component: () => import("@/pages/modules/accounting/payment/Index.vue"),
      // },
      // {
      //   name: "create_payment",
      //   path: "expenses/create",
      //   component: () => import("@/pages/modules/accounting/payment/Detail.vue"),
      // },
      // {
      //   name: "edit_payment",
      //   path: "expenses/edit/:id",
      //   component: () => import("@/pages/modules/accounting/payment/Detail.vue"),
      // },
      // {
      //   name: "view_payment",
      //   path: "expenses/view/:id",
      //   component: () => import("@/pages/modules/accounting/payment/View.vue"),
      // },
      // {
      //   name: "quick_payment",
      //   path: "expenses/quick/:id",
      //   component: () => import("@/pages/modules/accounting/payment/QuickPayment.vue"),
      // },

      // All Transactions (Optional - for viewing only) - Week 4
      // {
      //   name: "transactions_all",
      //   path: "transactions",
      //   component: () => import("@/pages/modules/accounting/transactions/Index.vue"),
      // },

      // Reports - Week 5
      // {
      //   name: "index_reports",
      //   path: "reports",
      //   component: () => import("@/pages/modules/accounting/reports/Index.vue"),
      // },
      // {
      //   name: "balance_sheet_report",
      //   path: "reports/balance-sheet",
      //   component: () => import("@/pages/modules/accounting/reports/balance-sheet/Index.vue"),
      // },
      // {
      //   name: "profit_loss_report",
      //   path: "reports/profit-loss",
      //   component: () => import("@/pages/modules/accounting/reports/profit-loss/Index.vue"),
      // },
      // {
      //   name: "trial_balance_report",
      //   path: "reports/trial-balance",
      //   component: () => import("@/pages/modules/accounting/reports/trial-balance/Index.vue"),
      // },
      // Ledgers / Chart of Accounts
      {
        name: "ledger",
        path: "ledgers/list",
        component: () => import("@/pages/modules/accounting/ledger/Index.vue"),
      },
      {
        name: "create_ledger",
        path: "ledgers/create",
        component: () => import("@/pages/modules/accounting/ledger/Detail.vue"),
      },
      {
        name: "edit_ledger",
        path: "ledgers/edit/:id",
        component: () => import("@/pages/modules/accounting/ledger/Detail.vue"),
      },
      {
        name: "view_ledger",
        path: "ledgers/view/:id",
        component: () => import("@/pages/modules/accounting/ledger/View.vue"),
      },
      {
        name: "create_category",
        path: "category/create",
        component: () => import("@/pages/modules/accounting/category/Detail.vue"),
      },
      {
        name: "edit_category",
        path: "category/:id/edit",
        component: () => import("@/pages/modules/accounting/category/Detail.vue"),
      },
      {
        name: "view_category",
        path: "category/view/:id",
        component: () => import("@/pages/modules/accounting/category/View.vue"),
      },
      // Year-end operations - Week 5
      // {
      //   name: "close_ledger",
      //   path: "close-ledger",
      //   component: () => import("@/pages/modules/accounting/ledger/Close.vue"),
      // },
      // {
      //   name: "open_ledger",
      //   path: "open-ledger",
      //   component: () => import("@/pages/modules/accounting/ledger/Open.vue"),
      // },
    ],
  },
  
  // 404
  {
    name: "NotFound",
    path: "/:pathMatch(.*)*",
    component: () => import("@/pages/NotFound.vue"),
  },
];

export default routes;