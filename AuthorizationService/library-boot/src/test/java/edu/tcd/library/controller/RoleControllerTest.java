package edu.tcd.library.controller;

import edu.tcd.library.admin.entity.UmsRole;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.http.MediaType;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultHandlers.print;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;
@DisplayName("Role Management Module Test (Requires Login)")
class RoleControllerTest extends BaseIntegrationTest {

    @Test
    @DisplayName("Create Role - Authorized")
    void createRole_Success() throws Exception {
        UmsRole role = new UmsRole();
        role.setName("VR_Admin");
        role.setCode("vr_admin");
        role.setStatus(1);
        role.setDescription("Administrator for VR lab");
        role.setSort(0);

        mockMvc.perform(withAuth(post("/ums/role/create"))
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(role)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("List Roles (Pagination)")
    void listRoles_Success() throws Exception {
        mockMvc.perform(withAuth(get("/ums/role/list"))
                        .param("pageNum", "1")
                        .param("pageSize", "10")
                        .param("keyword", "Admin")) // Optional param from docs
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data.list").isArray());
    }

    @Test
    @DisplayName("List All Roles (No Pagination)")
    void listAll_Success() throws Exception {
        // Corresponds to /ums/role/listAll
        mockMvc.perform(withAuth(get("/ums/role/listAll")))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data").isArray());
    }

    @Test
    @DisplayName("Search Users by Role ID")
    void qryUserAuthedById_Success() throws Exception {
        // Corresponds to /ums/role/qryUserAuthedById
        mockMvc.perform(withAuth(get("/ums/role/qryUserAuthedById"))
                        .param("roleId", "1"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data").isArray());
    }

    @Test
    @DisplayName("Update Role")
    void updateRole_Success() throws Exception {
        // Corresponds to /ums/role/update/{id}
        Long roleIdToUpdate = 2L;
        UmsRole roleUpdate = new UmsRole();
        roleUpdate.setName("VR_Admin_V2");
        roleUpdate.setStatus(1);

        mockMvc.perform(withAuth(post("/ums/role/update/{id}", roleIdToUpdate))
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(roleUpdate)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("Batch Delete Roles")
    void deleteRole_Success() throws Exception {
        // Corresponds to /ums/role/delete
        // Parameter is 'ids' (List<Long>), passed as query parameters for POST in this API spec
        mockMvc.perform(withAuth(post("/ums/role/delete"))
                        .param("ids", "101", "102"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("Grant Role to User")
    void userAuth_Success() throws Exception {
        // Corresponds to /ums/role/userAuth
        mockMvc.perform(withAuth(post("/ums/role/userAuth"))
                        .param("roleId", "1")
                        .param("adminIds", "1001", "1002"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }
}