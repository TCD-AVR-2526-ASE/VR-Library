package com.geoscene.topo.common.minio.config;


import com.geoscene.topo.common.minio.MinioClientExtend;
import com.geoscene.topo.common.minio.service.MinioService;
import com.geoscene.topo.common.core.utils.WebUtils;
import lombok.Data;
import okhttp3.OkHttpClient;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.security.KeyManagementException;

@Configuration
@ConfigurationProperties(prefix = "minio")
@Data
public class MinioConfig {

    private String endpoint;
    private String accessKey;
    private String secretKey;
    private String volumePath;


    @Bean
    public MinioClientExtend minioClientExtend() throws KeyManagementException {
        OkHttpClient okHttpClient = WebUtils.getUnsafeOkHttpClient();
        assert okHttpClient != null;
        return MinioClientExtend.builder()
                .endpoint(endpoint)
                .httpClient(okHttpClient)
                .credentials(accessKey, secretKey)
                .build();
    }

    @Bean
    public MinioService minioService() {
        return new MinioService();
    }
}
