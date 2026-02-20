package edu.tcd.library.controller;

import edu.tcd.library.admin.dto.UpdatePasswordByAdminDTO;
import edu.tcd.library.admin.dto.UpdateUserPasswordDTO;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.http.MediaType;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultHandlers.print;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@DisplayName("User Management Module Test (Requires Login)")
class UserControllerTest extends BaseIntegrationTest {

    // --- Read Operations ---

    @Test
    @DisplayName("List Users (Pagination)")
    void listUsers_Success() throws Exception {
        // API: /ums/admin/list
        mockMvc.perform(withAuth(get("/ums/admin/list"))
                        .param("keyword", "admin")
                        .param("pageSize", "10")
                        .param("pageNum", "1"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data.list").isArray());
    }

    @Test
    @DisplayName("Get User Info by ID")
    void getUserInfo_Success() throws Exception {
        // API: /ums/admin/info/{id}
        Long userId = 1L;
        mockMvc.perform(withAuth(get("/ums/admin/info/{id}", userId)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data.username").exists());
    }

    @Test
    @DisplayName("Load User by Username")
    void loadUserByUsername_Success() throws Exception {
        // API: /ums/admin/loadByUsername
        mockMvc.perform(withAuth(get("/ums/admin/loadByUsername"))
                        .param("username", "admin"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.username").value("admin"));
    }

    @Test
    @DisplayName("Get User Roles")
    void getUserRoles_Success() throws Exception {
        // API: /ums/admin/role/{adminId}
        Long adminId = 1L;
        mockMvc.perform(withAuth(get("/ums/admin/role/{adminId}", adminId)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data").isArray());
    }

    // --- Update Operations ---

    @Test
    @DisplayName("Update User Status (Enable/Disable)")
    void updateUserStatus_Success() throws Exception {
        // API: /ums/admin/updateStatus/{id}
        Long userId = 1L;
        // status: 0->Disabled, 1->Enabled
        mockMvc.perform(withAuth(post("/ums/admin/updateStatus/{id}", userId))
                        .param("status", "1"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("Distribute Roles to User")
    void distributeRole_Success() throws Exception {
        // API: /ums/admin/role/update
        // Note: adminId is single, roleIds is array
        mockMvc.perform(withAuth(post("/ums/admin/role/update"))
                        .param("adminId", "1")
                        .param("roleIds", "1"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("User Updates Own Password")
    void userUpdateMyPassword_Success() throws Exception {
        // API: /ums/admin/updateMyPassword
        UpdateUserPasswordDTO dto = new UpdateUserPasswordDTO();
        dto.setUsername("admin");
        dto.setOldPassword("macro123");
        dto.setNewPassword("macro123");

        mockMvc.perform(withAuth(post("/ums/admin/updateMyPassword"))
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(dto)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("Update User Profile (Multipart + Query Params)")
    void updateUserProfile_Success() throws Exception {
        // API: /ums/admin/update/{id}
        // This endpoint is tricky in docs: it has query params (umsAdminParam) AND body (icon)
        Long userId = 13L;

        // According to OpenAPI, 'umsAdminParam' is a ref in 'query'.
        // So we pass fields as params, NOT as JSON body.
        mockMvc.perform(withAuth(multipart("/ums/admin/update/{id}", userId))
                        .param("username", "updated_name")
                        .param("nickName", "New Nickname")
                        .param("email", "test@tcd.ie")
                        .param("status", "1"))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("Admin Updates User Password")
    void adminUpdatePassword_Success() throws Exception {
        // API: /ums/admin/updatePassword
        UpdatePasswordByAdminDTO dto = new UpdatePasswordByAdminDTO();
        dto.setUsername("updated_name");
        dto.setNewPassword("new_secure_pass");

        mockMvc.perform(withAuth(post("/ums/admin/updatePassword"))
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(dto)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    // --- Delete Operations ---

    @Test
    @DisplayName("Delete User")
    void deleteUser_Success() throws Exception {
        // API: /ums/admin/delete/{id}
        Long userIdToDelete = 99L;
        mockMvc.perform(withAuth(post("/ums/admin/delete/{id}", userIdToDelete)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(500));
    }
}
