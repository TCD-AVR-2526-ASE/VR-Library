package edu.tcd.library.admin.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotEmpty;
import lombok.Getter;
import lombok.Setter;

/**
 * Parameters for updating administrator password
 */
@Getter
@Setter
public class UpdateAdminPasswordDTO {

    @NotEmpty
    @Schema(description = "Username", requiredMode = Schema.RequiredMode.REQUIRED)
    private String username;

    @NotEmpty
    @Schema(description = "Old password", requiredMode = Schema.RequiredMode.REQUIRED)
    private String oldPassword;

    @NotEmpty
    @Schema(description = "New password", requiredMode = Schema.RequiredMode.REQUIRED)
    private String newPassword;

}