package edu.tcd.library.controller;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.HttpHeaders;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.request.MockHttpServletRequestBuilder;

/**
 * Base class for all integration tests requiring authentication.
 * It injects the Authorization Token into the request header by default.
 */
@SpringBootTest
@AutoConfigureMockMvc
public abstract class BaseIntegrationTest {

    @Autowired
    protected MockMvc mockMvc;

    @Autowired
    protected ObjectMapper objectMapper;

    protected String validToken;

    @BeforeEach
    public void setup() {
        // Two options here:
        // 1. Simulate real login to get Token (Recommended for Integration Test)
        // 2. Mock SecurityContext to forge identity (Recommended for Unit Test)
        // Here we demonstrate option 1. If the system is not running, use @MockBean for AuthService.
        this.validToken = "Bearer " + obtainAccessToken();
    }

    /**
     * Logic to obtain access token.
     * In a real environment, call /auth/login to get the token.
     * In a Mock environment, return a JWT-compliant string directly.
     */
    protected String obtainAccessToken() {
        // Mock: Assume this token is obtained via the login flow.
        // If using JWT, ensure this token can pass your Filter validation.
        return "jgh7fiWbgvglTFTLZpBxribXphb7yLaAYx7sgRGDzlra92M5YvmsVB7Wy84xkjERWGYTJCVohvke8KpEXC8UXqsmRnF6zRZ3qu4TCjKq3yMvBWIkbLsByefhneRs7MWe";
    }

    /**
     * Wraps the request builder to automatically add the authentication header.
     */
    protected MockHttpServletRequestBuilder withAuth(MockHttpServletRequestBuilder builder) {
        return builder.header(HttpHeaders.AUTHORIZATION, this.validToken);
    }
}