package edu.tcd.library.admin.vo;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Builder;
import lombok.Data;
import lombok.EqualsAndHashCode;

/**
 * Wrapper for OAuth2 token response information
 */
@Data
@EqualsAndHashCode(callSuper = false)
@Builder
public class Oauth2TokenVO {

    @Schema(description = "Access token")
    private String token;

    @Schema(description = "Refresh token")
    private String refreshToken;

    @Schema(description = "Access token header prefix")
    private String tokenHead;

    @Schema(description = "Expiration time in seconds")
    private Long expiresIn;
}