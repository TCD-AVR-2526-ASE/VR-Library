package edu.tcd.library.admin.vo;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;
import java.util.List;
import java.util.Map;

/**
 * Information of the current authenticated user
 */
@Data
@EqualsAndHashCode(callSuper = false)
public class CurrentUserVO implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    @Schema(description = "User ID")
    private Long userId;

    @Schema(description = "Username")
    private String username;

    @Schema(description = "User avatar URL")
    private String icon;

    @Schema(description = "List of assigned role IDs")
    private List<Long> roles;

}