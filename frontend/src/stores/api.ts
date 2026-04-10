import { useAPI } from '@/services/api';

export const api = {
  // ============================================
  // Auth
  // ============================================
  async register(data: any) {
    const http = useAPI();
    return await http.post('/register', data);
  },

  async login(data: any) {
    const http = useAPI();
    return await http.post('/login', data);
  },

  async logout() {
    const http = useAPI();
    return await http.post('/logout', {});
  },

  async getCurrentUser() {
    const http = useAPI();
    return await http.get('/user');
  },

  // ============================================
  // Users
  // ============================================
  async getUsers(params = {}) {
    const http = useAPI();
    return await http.get('/users', params);
  },

  async getUser(id: number) {
    const http = useAPI();
    return await http.get(`/users/${id}`);
  },

  async createUser(data: any) {
    const http = useAPI();
    return await http.post('/users', data);
  },

  async updateUser(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/users/${id}`, data);
  },

  async deleteUser(id: number) {
    const http = useAPI();
    return await http.delete(`/users/${id}`);
  },

  async assignUserRole(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/users/${id}/role`, data);
  },

  async changeUserPassword(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/users/${id}/password`, data);
  },

  // ============================================
  // Students
  // ============================================
  async getStudents(params = {}) {
    const http = useAPI();
    return await http.get('/students', params);
  },

  async getStudent(id: number) {
    const http = useAPI();
    return await http.get(`/students/${id}`);
  },

  async createStudent(data: any) {
    const http = useAPI();
    return await http.post('/students', data);
  },

  async updateStudent(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/students/${id}`, data);
  },

  async deleteStudent(id: number) {
    const http = useAPI();
    return await http.delete(`/students/${id}`);
  },

  async assignStudentToClass(id: number, data: any) {
    const http = useAPI();
    return await http.post(`/students/${id}/assign-class`, data);
  },

  async getStudentsReport(params = {}) {
    const http = useAPI();
    return await http.get('/students/report', params);
  },

  async importStudents(data: FormData) {
    const http = useAPI();
    return await http.post('/students/import', data, undefined, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  },

  // ============================================
  // Grades
  // ============================================
  async getGrades(params = {}) {
    const http = useAPI();
    return await http.get('/grades', params);
  },

  async getGrade(id: number) {
    const http = useAPI();
    return await http.get(`/grades/${id}`);
  },

  async createGrade(data: any) {
    const http = useAPI();
    return await http.post('/grades', data);
  },

  async updateGrade(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/grades/${id}`, data);
  },

  async deleteGrade(id: number) {
    const http = useAPI();
    return await http.delete(`/grades/${id}`);
  },

  // ============================================
  // Classes
  // ============================================
  async getClasses(params = {}) {
    const http = useAPI();
    return await http.get('/classes', params);
  },

  async getClass(id: number) {
    const http = useAPI();
    return await http.get(`/classes/${id}`);
  },

  async createClass(data: any) {
    const http = useAPI();
    return await http.post('/classes', data);
  },

  async updateClass(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/classes/${id}`, data);
  },

  async deleteClass(id: number) {
    const http = useAPI();
    return await http.delete(`/classes/${id}`);
  },

  async addStudentToClass(classId: number, data: any) {
    const http = useAPI();
    return await http.post(`/classes/${classId}/students`, data);
  },

  async removeStudentFromClass(classId: number, studentId: number) {
    const http = useAPI();
    return await http.delete(`/classes/${classId}/students/${studentId}`);
  },

  // ============================================
  // Transactions
  // ============================================
  async getTransactions(params = {}) {
    const http = useAPI();
    return await http.get('/transactions', params);
  },

  async getTransaction(id: number) {
    const http = useAPI();
    return await http.get(`/transactions/${id}`);
  },

  async createTransaction(data: any) {
    const http = useAPI();
    return await http.post('/transactions', data);
  },

  async updateTransaction(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/transactions/${id}`, data);
  },

  async deleteTransaction(id: number) {
    const http = useAPI();
    return await http.delete(`/transactions/${id}`);
  },

  async reverseTransaction(id: number) {
    const http = useAPI();
    return await http.post(`/transactions/${id}/reverse`, {});
  },

  // ============================================
  // Categories
  // ============================================
  async getCategories(params = {}) {
    const http = useAPI();
    return await http.get('/categories', params);
  },

  async getCategory(id: number) {
    const http = useAPI();
    return await http.get(`/categories/${id}`);
  },

  async createCategory(data: any) {
    const http = useAPI();
    return await http.post('/categories', data);
  },

  async updateCategory(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/categories/${id}`, data);
  },

  async deleteCategory(id: number) {
    const http = useAPI();
    return await http.delete(`/categories/${id}`);
  },

  // ============================================
  // Ledgers
  // ============================================
  async getLedgers(params = {}) {
    const http = useAPI();
    return await http.get('/ledgers', params);
  },

  async getLedger(id: number) {
    const http = useAPI();
    return await http.get(`/ledgers/${id}`);
  },

  async createLedger(data: any) {
    const http = useAPI();
    return await http.post('/ledgers', data);
  },

  async updateLedger(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/ledgers/${id}`, data);
  },

  async deleteLedger(id: number) {
    const http = useAPI();
    return await http.delete(`/ledgers/${id}`);
  },

  async getTrialBalance(params = {}) {
    const http = useAPI();
    return await http.get('/ledgers/reports/trial-balance', params);
  },

  async getLedgerSummary(year: number) {
    const http = useAPI();
    return await http.get(`/ledgers/summary/${year}`);
  },

  async yearEndClose(data: any) {
    const http = useAPI();
    return await http.post('/ledgers/year-end-close', data);
  },

  async yearBeginningOpen(data: any) {
    const http = useAPI();
    return await http.post('/ledgers/year-beginning-open', data);
  },

  // ============================================
  // Payers
  // ============================================
  async getPayers(params = {}) {
    const http = useAPI();
    return await http.get('/payers', params);
  },

  async getPayer(id: number) {
    const http = useAPI();
    return await http.get(`/payers/${id}`);
  },

  async createPayer(data: any) {
    const http = useAPI();
    return await http.post('/payers', data);
  },

  async updatePayer(id: number, data: any) {
    const http = useAPI();
    return await http.put(`/payers/${id}`, data);
  },

  async deletePayer(id: number) {
    const http = useAPI();
    return await http.delete(`/payers/${id}`);
  },

  // ============================================
  // Journal Entries
  // ============================================
  async getJournalEntries(params = {}) {
    const http = useAPI();
    return await http.get('/journal-entries', params);
  },

  // ============================================
  // Business Info
  // ============================================
  async getBusinessInfo() {
    const http = useAPI();
    return await http.get('/business-info');
  },

  async updateBusinessInfo(data: any) {
    const http = useAPI();
    return await http.put('/business-info', data);
  },
};

// Export useAPI for direct use if needed
export { useAPI } from '@/services/api';
