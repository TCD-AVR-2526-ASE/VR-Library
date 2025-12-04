package com.geoscene.topo.admin.entity;

import com.baomidou.mybatisplus.annotation.IdType;
import com.baomidou.mybatisplus.annotation.TableId;
import com.baomidou.mybatisplus.annotation.TableName;
import com.fasterxml.jackson.annotation.JsonIgnore;
import com.geoscene.topo.common.mybatis.entity.BaseEntity;
import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;
import java.util.Date;

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

    @Schema(description = "头像")
    private String icon;

    @Schema(description = "邮箱")
    private String email;

    @Schema(description = "手机号")
    private String phone;

    @Schema(description = "职位")
    private String position;

    @Schema(description = "联系方式")
    private String contact;

    @Schema(description = "昵称")
    private String nickName;

    @Schema(description = "身份证号")
    private String idCard;

    @Schema(description = "备注信息")
    private String note;

    @Schema(description = "帐号启用状态：0->禁用；1->启用")
    private Integer status;

    @Schema(description = "排序编号")
    private Integer sort;

}