package com.geoscene.topo.admin.entity;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

/**
 * 部门节点封装
 */
@Getter
@Setter
public class UmsDeptNode extends UmsDept {

    @Schema(description = "子级部门")
    private List<UmsDeptNode> children;

}
