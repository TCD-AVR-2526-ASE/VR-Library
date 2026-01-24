package edu.tcd.library.admin.entity;

import com.baomidou.mybatisplus.annotation.IdType;
import com.baomidou.mybatisplus.annotation.TableId;
import com.baomidou.mybatisplus.annotation.TableName;
import com.fasterxml.jackson.annotation.JsonIgnore;
import edu.tcd.library.common.mybatis.entity.BaseEntity;
import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;

/**
 * System Administrator Entity
 */
@EqualsAndHashCode(callSuper = true)
@Data
@TableName("ums_admin")
public class UmsAdmin extends BaseEntity implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    @TableId(type = IdType.ASSIGN_ID)
    private Long id;

    private String username;

    @JsonIgnore
    private String password;

    @Schema(description = "Avatar URL")
    private String icon;

    @Schema(description = "Email address")
    private String email;

    @Schema(description = "Mobile phone number")
    private String phone;

    @Schema(description = "Job position")
    private String position;

    @Schema(description = "Contact information")
    private String contact;

    @Schema(description = "Nickname")
    private String nickName;

    @Schema(description = "Identity card number")
    private String idCard;

    @Schema(description = "Remarks/Notes")
    private String note;

    @Schema(description = "Account status: 0 -> Disabled; 1 -> Enabled")
    private Integer status;

    @Schema(description = "Sort order index")
    private Integer sort;

}