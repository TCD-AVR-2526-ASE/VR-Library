package com.geoscene.topo.admin.entity;

import com.baomidou.mybatisplus.annotation.IdType;
import com.baomidou.mybatisplus.annotation.TableId;
import com.baomidou.mybatisplus.annotation.TableName;
import com.fasterxml.jackson.annotation.JsonFormat;
import com.geoscene.topo.common.mybatis.entity.BaseEntity;
import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;

import jakarta.validation.constraints.NotEmpty;
import jakarta.validation.constraints.NotNull;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;
import java.util.Date;

@EqualsAndHashCode(callSuper = true)
@Data
@TableName("ums_dict")
public class UmsDict extends BaseEntity implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    @TableId(type = IdType.AUTO)
    private Long id;

    @Schema(description = "字典名称")
    @NotEmpty(message = "字典名称不能为空！")
    private String name;

    @Schema(description = "字典编码")
    @NotEmpty(message = "字典编码不能为空！")
    private String code;

    @Schema(description = "字典说明")
    private String description;

    @Schema(description = "父id")
    @NotNull(message = "父id不能为空！")
    private Long parentId;

}
