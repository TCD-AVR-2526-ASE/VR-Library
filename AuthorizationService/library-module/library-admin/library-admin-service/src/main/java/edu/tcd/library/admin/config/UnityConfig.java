package edu.tcd.library.admin.config;

import lombok.Data;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

@Configuration
@ConfigurationProperties(prefix = "unity")
@Data
public class UnityConfig {

    private String exchangeUri;

    private String customIdUri;

    private String environment;

    private String saId;

    private String saSecret;

    private String projectId;

}
