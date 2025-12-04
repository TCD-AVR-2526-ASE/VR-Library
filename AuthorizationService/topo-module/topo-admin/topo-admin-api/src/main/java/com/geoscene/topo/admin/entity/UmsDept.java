package com.geoscene.topo.admin.entity;


import com.baomidou.mybatisplus.annotation.IdType;
import com.baomidou.mybatisplus.annotation.TableId;
import com.baomidou.mybatisplus.annotation.TableName;
import com.geoscene.topo.common.mybatis.entity.BaseEntity;
import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotEmpty;
import jakarta.validation.constraints.NotNull;
import lombok.Data;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;

@EqualsAndHashCode(callSuper = true)
@Data
@TableName("ums_dept")
public class UmsDept extends BaseEntity implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    @TableId(type = IdType.AUTO)
    private Long id;

    @Schema(description = "部门名称")
    @NotEmpty(message = "部门名称不能为空！")
    private String name;

    @Schema(description = "部门编码")
    private String code;

    @Schema(description = "父id")
    @NotNull(message = "父id不能为空！")
    private Long parentId;

    @Schema(description = "部门级别")
    private String level;

    @Schema(description = "备注")
    private String description;

    @Schema(description = "排序编号")
    private Integer sort;
}
