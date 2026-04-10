import http from "@/services/api";

const pageNumber = 1;
const pageSize = 10;

export default {

    // Manage List Project
    // Index
    projectList() {
        return http.get(`/project?page=${pageNumber}&pageSize=${pageSize}`);
    },
    // Project List By Id
    projectListById(id: any) {
        return http.get(`/project?page=${pageNumber}&pageSize=${pageSize}`);
    },


};