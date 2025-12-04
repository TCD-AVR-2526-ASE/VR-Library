package com.geoscene.topo.admin.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Getter;
import lombok.Setter;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotEmpty;

/**
 * 用户登录参数
 */
@Getter
@Setter
public class UmsAdminDTO {

    @Schema(description = "用户名", requiredMode = Schema.RequiredMode.REQUIRED)
    @NotEmpty
    private String username;

    @Schema(description = "密码", requiredMode = Schema.RequiredMode.REQUIRED)
    @NotEmpty
    private String password;

    @Schema(description = "邮箱")
    @Email
    private String email;

    @Schema(description = "用户昵称")
    private String nickName;

    @Schema(description = "备注")
    private String note;

    @Schema(description = "身份证号")
    private String idCard;

    @Schema(description = "排序编号")
    private Integer sort;
}
