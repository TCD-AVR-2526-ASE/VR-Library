package com.geoscene.topo.admin.vo;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;
import java.util.List;
import java.util.Map;

/**
 * 当前用户相关信息
 */
@Data
@EqualsAndHashCode(callSuper = false)
public class CurrentUserVO implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    @Schema(description = "用户id")
    private Long userId;

    @Schema(description = "用户名称")
    private String username;

    @Schema(description = "用户头像")
    private String icon;

    @Schema(description = "所属角色ids")
    List<Long> roles;

    @Schema(description = "所属部门ids")
    List<Long> depts;

    @Schema(description = "所属部门名称")
    List<String> deptNames;

    @Schema(description = "权限菜单ids+权限名称")
    List<Map<String, Object>> menus;

}
