import { defineStore } from 'pinia';
import { useAPI } from '@/services/api';

interface User {
  id: number;
  name: string;
  email: string;
  role?: string;
  permissions?: string[];
}

interface AuthState {
  user: User | null;
  token: string | null;
  isLoading: boolean;
}

export const useAuthStore = defineStore('auth', {
  persist: true,
  state: (): AuthState => ({
    user: null,
    token: null,
    isLoading: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token && !!state.user,
    userRole: (state) => state.user?.role || null,
    permissions: (state) => state.user?.permissions || [],
  },

  actions: {
    // Register new user
    async register(payload: { name: string; username:string; email: string; password: string; password_confirmation: string }) {
      this.isLoading = true;
      const api = useAPI();

      try {

        const response = await api.post<any>('/register', payload);

        if (response.token) {
          this.token = response.token;
        }

        if (response.user) {
          this.user = response.user;
        }

        return { success: true };
      } catch (error: any) {
        const message = error.response?.data?.message || 'Registration failed';
        const errors = error.response?.data?.errors || {};
        return { success: false, message, errors };
      } finally {
        this.isLoading = false;
      }
    },

    // Login with Sanctum
    async login(payload: { email: string; password: string }) {
      this.isLoading = true;
      const api = useAPI();

      try {

        const response = await api.post<any>('/login', payload);

        if (response.token) {
          this.token = response.token;
        }

        if (response.user) {
          this.user = response.user;
        } else {
          await this.fetchUser();
        }

        return { success: true };
      } catch (error: any) {
        const message = error.response?.data?.message || 'Login failed';
        return { success: false, message };
      } finally {
        this.isLoading = false;
      }
    },

    // Fetch current authenticated user
    async fetchUser() {
      const api = useAPI();

      try {
        const response = await api.get<User>('/user');
        this.user = response;
        return this.user;
      } catch (error) {
        this.user = null;
        this.token = null;
        return null;
      }
    },

    // Logout
    async logout() {
      const api = useAPI();

      try {
        await api.post('/logout', {});
      } catch (error) {
        // Ignore logout errors
      } finally {
        this.user = null;
        this.token = null;
      }
    },

    // Check if user has specific permission
    hasPermission(permissionName: string) {
      return this.permissions.includes(permissionName);
    },

    // Check authentication status on app load
    async checkAuth() {
      if (this.token || this.user) {
        try {
          await this.fetchUser();
          return true;
        } catch {
          this.user = null;
          this.token = null;
          return false;
        }
      }
      return false;
    },
  },
});
