import { defineStore } from "pinia";

export const useSidebarStore = defineStore("sidebarStore", {
  state: () => ({
    show: false,
    collapse: false,
    pageName: "",
  }),
  persist: true,
  actions: {
    setPageName(pageName: any) {
      this.pageName = pageName;
    },
    toggle() {
      this.show = !this.show;
    },
    open() {
      this.show = true;
    },
    close() {
      this.show = false;
    },
    toggleCollapse() {
      this.collapse = !this.collapse;
    },
    openMini() {
      this.collapse = true;
    },
    closeMini() {
      this.collapse = false;
    },
  },
});
