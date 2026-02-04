package edu.tcd.library;

import cn.hutool.core.lang.Assert;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.config.UnityConfig;
import edu.tcd.library.admin.dto.UnityCustomInfo;
import edu.tcd.library.admin.service.UnityService;
import okhttp3.*;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;

import java.io.IOException;

@SpringBootTest
public class UnityCustomerIDTest {

    private final OkHttpClient client = new OkHttpClient();

    @Autowired
    private UnityConfig config;

    @Autowired
    private UnityService unityService;

    @Test
    void testGetUnityConfig() {
        System.out.println(config.getCustomIdUri());
    }

    @Test
    void testGetStatelessToken() {
        String url = String.format(config.getExchangeUri(), config.getProjectId());
        RequestBody body = RequestBody.create("{}", MediaType.parse("application/json"));
        String basicAuth = Credentials.basic(config.getSaId(), config.getSaSecret());
        Request request = new Request.Builder()
                .url(url)
                .post(body)
                .header("Authorization", basicAuth)
                .header("Content-Type", "application/json")
                .build();

        try (Response response = client.newCall(request).execute()) {
            if (!response.isSuccessful()) {
                System.err.println("request failed: " + response.code());
                if (response.body() != null) {
                    System.err.println("error msg: " + response.body().string());
                }
                return;
            }

            Assert.notNull(response.body(), "response body is null");
            if (response.body() != null) {
                String responseData = response.body().string();
                System.out.println("success, Token: " + responseData);
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    String token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6InVuaXR5LWtleXM6MzU0OWFkNDMtN2RjYS00YTdkLTg2MWMtYjJmM2ZjZmMyZTAyIiwiamt1IjoiaHR0cHM6Ly9rZXlzLnNlcnZpY2VzLnVuaXR5LmNvbS8ifQ.eyJleHAiOjE3NzAxMTg5MzcsImlhdCI6MTc3MDExNTMzNywibmJmIjoxNzcwMTE1MzM3LCJqdGkiOiJiYTNhMjVlNi0yY2MzLTQ0YjgtYmU4NS0zNzZhODk2NGVmZGQiLCJzdWIiOiI5MjI1ZjEyYi1mMWU5LTQxZWYtOTNkZS1kMTVjZGM4YjVlZWUiLCJ2ZXJzaW9uIjoxLCJpc3MiOiJodHRwczovL3NlcnZpY2VzLnVuaXR5LmNvbSIsImF1ZCI6WyJ1cGlkOjQ4MmMzNDUzLWFiNGQtNDRkMy1iMjBhLTZiODU4YTJkYjJjMiJdLCJzY29wZXMiOlsicGxheWVyX2F1dGguc2VydmVyLmN1c3RvbV9pZF9hdXRoIiwidW5pdHkuZW52aXJvbm1lbnRzLmNyZWF0ZSIsInVuaXR5LmVudmlyb25tZW50cy5kZWxldGUiLCJ1bml0eS5lbnZpcm9ubWVudHMuZ2V0IiwidW5pdHkuZW52aXJvbm1lbnRzLmxpc3QiXSwiYXBpS2V5UHVibGljSWRlbnRpZmllciI6ImVhOTBjOTQ4LWUxNzEtNGE2Mi1hZWUxLTc4ZDcwYmJlOGRlOSJ9.Mq8ayzMn2oSvFYve5yVQ94W2g6M1YjOROYfGlrhLd0VlwhYqnqP-rKDqItYF5mIPkPDM2HvmTu4BhYkIX14Y_ii4UeSOxnFI-8QeYphT8w19y3XQtDSqtD7QEceFS6_78RDKRx1rwJo5Kl-1zSqD4LzjChqT_Yl83gT38j4_9DaZYTf3DZFGP4LXCVE6KEzeMgKu6LdgJmf-3pBTBQo9QJoAHMr13zjaICR36jURKOeLYTJZUijEuoH_Sk1vDJor3dRD1vZQTp6mXpkBprz_FCA1tgqau5Oj_lU5Q1s0vHBcnCeHtK-c6-gdQabSB7ChAIqtVHUgRaCPmkvumGtHCA";

    @Test
    void testGetCustomerID() {
        String customPlayerId = "123";
        String url = String.format(config.getCustomIdUri(), config.getProjectId());
        MediaType mediaType = MediaType.get("application/json; charset=utf-8");
        String json = "{\"externalId\": \"" + customPlayerId + "\"}";
        RequestBody body = RequestBody.create(json, mediaType);

        Request request = new Request.Builder()
                .url(url)
                .post(body)
                .addHeader("UnityEnvironment", config.getEnvironment())
                .addHeader("Authorization", "Bearer " + token)
                .build();

        try (Response response = client.newCall(request).execute()) {
            Assert.isTrue(response.isSuccessful(), "response code is " + response.code());
            if (response.isSuccessful()) {
                String responseBody = response.body().string();
                Assert.notBlank(responseBody, "response body is null");
                System.out.println(responseBody);
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    @Test
    void testSerializeCustomeInfo() {
        String json = """
                {
                    "expiresIn": 3599,
                    "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6InB1YmxpYzo2NzQ2QjA5NC0zODNCLTRFMDYtQjA0OS04OUU4MTU1NjdBOUQiLCJ0eXAiOiJKV1QifQ.eyJhdWQiOlsiaWRkOmY1YjRjNjkzLTdhNzctNDk4Yi05N2VlLTY5MWU2ZmRhZWQxZSIsImVudk5hbWU6cHJvZHVjdGlvbiIsImVudklkOjk5NDJjMTIwLTQxZmItNGFlNC1iNjIyLWNlMDU1ZjE4YTk3NyIsInVwaWQ6NDgyYzM0NTMtYWI0ZC00NGQzLWIyMGEtNmI4NThhMmRiMmMyIl0sImV4cCI6MTc3MDExOTQ0MywiaWF0IjoxNzcwMTE1ODQzLCJpZGQiOiJmNWI0YzY5My03YTc3LTQ5OGItOTdlZS02OTFlNmZkYWVkMWUiLCJpc3MiOiJodHRwczovL3BsYXllci1hdXRoLnNlcnZpY2VzLmFwaS51bml0eS5jb20iLCJqdGkiOiI5ZTg5YmYyZS0xYTE2LTRjYzMtYWM3Zi1iNDBlMGVjMTI5YjkiLCJuYmYiOjE3NzAxMTU4NDMsInByb2plY3RfaWQiOiI0ODJjMzQ1My1hYjRkLTQ0ZDMtYjIwYS02Yjg1OGEyZGIyYzIiLCJzaWduX2luX3Byb3ZpZGVyIjoiY3VzdG9tIiwic3ViIjoiMjAzU0c1MTM0QkNnclF3TkNEbWlNNTlGdjEyMCIsInRva2VuX3R5cGUiOiJhdXRoZW50aWNhdGlvbiIsInZlcnNpb24iOiIxIn0.ZI4Y3deizk7zcQ-RcZ9M8WnEzk8WSGmj2DxvClDkfBZgBNVmCYbLvg9jPu9emOlmzVApRZVimS4HcLnaDWYqRZKzcxaZYtaAFaoPnxVuUBBN1_H_bZU1q_-55bv5v1nboAvxDIrFUOcCRyL04z9-d_EVfXI3E6YGE_OkTi-w3ivEZ5VrtxsXOEJwhgalsSY6fYTWKjAsBjPOK7Z_prcsC75MfnVJ8vKC7_KYQi0VtVo0atFIcEbBxorXuTvi1J7Z4E2WHVCooLtSaAQtxG1s9QRlhVIDcjryffQnht5xWzTZe3N2Eu1QozgcXJQqrJ9Dlzre2vEcFbeJjFXRf-eVIA",
                    "sessionToken": "Qo3SBbowprLO7IaZKjQjxTs5y2x0OVaMhq-SumZu_m0WKtEKWTrvEE-9IvYRDJlxAk8HovENT9RkWU2FgXuVuPX22sCe0kRfmRL0j_b0pMaeQ4cVV2NwWqMK8tIPHMktpL36MW_t3Ao5ictadK8COVqUwoXXCjR40qdtvTDaOyA.FZkwIfOAb1sAsrpiBi9h4zl8QzxOV55jvx1CMO3_j6Q",
                    "user": {
                        "disabled": false,
                        "externalIds": [
                            {
                                "externalId": "123",
                                "providerId": "custom"
                            }
                        ],
                        "id": "203SG5134BCgrQwNCDmiM59Fv120"
                    },
                    "userId": "203SG5134BCgrQwNCDmiM59Fv120"
                }
                """;
        UnityCustomInfo obj = JSONUtil.toBean(json, UnityCustomInfo.class);
        Assert.notNull(obj, "cannot get the object");
        Assert.notBlank(obj.getUserId(), "cannot get the userId");
        Assert.notBlank(obj.getIdToken(), "cannot get the idToken");
    }

    @Test
    void testGetExchangeFromService(){
        String accessToken = unityService.exchange();
        Assert.notBlank(accessToken, "cannot get the accessToken");
    }

    @Test
    void testGetCustomInfo(){
        String accessToken = unityService.exchange();
        Assert.notBlank(accessToken, "cannot get the accessToken");
        UnityCustomInfo customInfo = unityService.customInfo(accessToken, "123");
        Assert.notNull(customInfo, "cannot get the customInfo");
        Assert.notBlank(customInfo.getUserId(), "cannot get the userId");
        Assert.notBlank(customInfo.getIdToken(), "cannot get the idToken");
    }
}
