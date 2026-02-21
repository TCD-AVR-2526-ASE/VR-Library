package edu.tcd.library.admin.service.impl;

import cn.hutool.core.lang.Assert;
import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.config.UnityConfig;
import edu.tcd.library.admin.dto.UnityCustomInfo;
import edu.tcd.library.admin.service.UnityService;
import okhttp3.*;
import org.springframework.stereotype.Service;

import java.io.IOException;

@Service
public class UnityServiceImpl implements UnityService {

    private final UnityConfig config;

    private final OkHttpClient okHttpClient;

    public UnityServiceImpl(UnityConfig unityConfig) {
        this.config = unityConfig;
        this.okHttpClient = new OkHttpClient();
    }

    @Override
    public String exchange() {
        String url = String.format(config.getExchangeUri(), config.getProjectId());
        RequestBody body = RequestBody.create("{}", MediaType.parse("application/json"));
        String basicAuth = Credentials.basic(config.getSaId(), config.getSaSecret());
        Request request = new Request.Builder()
                .url(url)
                .post(body)
                .header("Authorization", basicAuth)
                .header("Content-Type", "application/json")
                .build();

        try (Response response = okHttpClient.newCall(request).execute()) {
            if (!response.isSuccessful()) {
                System.err.println("request failed: " + response.code());
                if (response.body() != null) {
                    System.err.println("error msg: " + response.body().string());
                }
            }

            Assert.notNull(response.body(), "response body is null");
            if (response.body() != null) {
                String responseData = response.body().string();
                JSONObject entries = JSONUtil.parseObj(responseData);
                return entries.getStr("accessToken");
            }
        } catch (IOException e) {
            throw new RuntimeException("request failed: " + e.getMessage());
        }
        return null;
    }

    @Override
    public UnityCustomInfo customInfo(String accessToken, String customPlayerId) {
        String url = String.format(config.getCustomIdUri(), config.getProjectId());
        MediaType mediaType = MediaType.get("application/json; charset=utf-8");
        String json = "{\"externalId\": \"" + customPlayerId + "\"}";
        RequestBody body = RequestBody.create(json, mediaType);
        Request request = new Request.Builder()
                .url(url)
                .post(body)
                .addHeader("UnityEnvironment", config.getEnvironment())
                .addHeader("Authorization", "Bearer " + accessToken)
                .build();

        try (Response response = okHttpClient.newCall(request).execute()) {
            Assert.isTrue(response.isSuccessful(), "response code is " + response.code());
            if (response.isSuccessful()) {
                String responseBody = response.body().string();
                UnityCustomInfo obj = JSONUtil.toBean(responseBody, UnityCustomInfo.class);
                return obj;
            }
        } catch (IOException e) {
            throw new RuntimeException("request failed: " + e.getMessage());
        }
        return null;
    }

    @Override
    public UnityCustomInfo customToken(String customPlayerId) {
        String accessToken = exchange();
        Assert.notBlank(accessToken, "cannot get the accessToken");
        UnityCustomInfo customInfo = customInfo(accessToken, "123");
        Assert.notNull(customInfo, "cannot get the customInfo");
        return customInfo;
    }

}
