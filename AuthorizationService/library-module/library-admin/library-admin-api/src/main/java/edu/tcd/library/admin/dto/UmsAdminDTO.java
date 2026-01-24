package edu.tcd.library.admin.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Getter;
import lombok.Setter;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotEmpty;

/**
 * User login and registration parameters
 */
@Getter
@Setter
public class UmsAdminDTO {

    @Schema(description = "Username", requiredMode = Schema.RequiredMode.REQUIRED)
    @NotEmpty
    private String username;

    @Schema(description = "Password", requiredMode = Schema.RequiredMode.REQUIRED)
    @NotEmpty
    private String password;

    @Schema(description = "Email address")
    @Email
    private String email;

    @Schema(description = "User nickname")
    private String nickName;

    @Schema(description = "Remarks or notes")
    private String note;

    @Schema(description = "Identity card number")
    private String idCard;

    @Schema(description = "Sort order index")
    private Integer sort;
}