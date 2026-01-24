package edu.tcd.library.admin.entity;

import com.baomidou.mybatisplus.annotation.IdType;
import com.baomidou.mybatisplus.annotation.TableId;
import com.baomidou.mybatisplus.annotation.TableName;
import edu.tcd.library.common.mybatis.entity.BaseEntity;
import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;

/**
 * System Role Entity
 */
@Data
@EqualsAndHashCode(callSuper = true)
@TableName("ums_role")
public class UmsRole extends BaseEntity implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    @TableId(type = IdType.AUTO)
    private Long id;

    @Schema(description = "Role name")
    private String name;

    @Schema(description = "Role code")
    private String code;

    @Schema(description = "Role description")
    private String description;

    @Schema(description = "Activation status: 0 -> Disabled; 1 -> Enabled")
    private Integer status;

    @Schema(description = "Sort order index")
    private Integer sort;

}