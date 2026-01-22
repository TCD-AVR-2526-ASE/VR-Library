package com.geoscene.topo.admin.entity;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Getter;
import lombok.Setter;

import java.util.List;

/**
 * 字典节点封装
 */
@Getter
@Setter
public class UmsDictNode extends UmsDict {

    @Schema(description = "子级字典")
    private List<UmsDictNode> children;

}
