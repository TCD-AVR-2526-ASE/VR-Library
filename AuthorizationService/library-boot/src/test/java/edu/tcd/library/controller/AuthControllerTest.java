package edu.tcd.library.controller;


import edu.tcd.library.admin.dto.UmsAdminDTO;
import edu.tcd.library.admin.dto.UmsAdminLoginDTO;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.http.MediaType;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultHandlers.print;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

/**
 * Authentication module does not need to inherit the Token injection logic from BaseIntegrationTest,
 * or rather, its login interface is designed to obtain the Token.
 */
@DisplayName("Authentication Management Module Test")
class AuthControllerTest extends BaseIntegrationTest {

    @Test
    @DisplayName("Login API - Success")
    void login_Success() throws Exception {
        // Prepare data
        UmsAdminLoginDTO loginDto = new UmsAdminLoginDTO();
        loginDto.setUsername("admin");
        loginDto.setPassword("macro123");

        // Perform request
        mockMvc.perform(post("/auth/login")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(loginDto)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200))
                .andExpect(jsonPath("$.data.token").exists()); // Verify Token existence
    }

    @Test
    @DisplayName("User Register - Success")
    void register_Success() throws Exception {
        UmsAdminDTO registerDto = new UmsAdminDTO();
        registerDto.setUsername("new_user");
        registerDto.setPassword("password123");

        // Does the register API need a token? Based on docs under /auth, assumed no token needed.
        // If needed, wrap with withAuth().
        mockMvc.perform(post("/auth/register")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(registerDto)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }
}
