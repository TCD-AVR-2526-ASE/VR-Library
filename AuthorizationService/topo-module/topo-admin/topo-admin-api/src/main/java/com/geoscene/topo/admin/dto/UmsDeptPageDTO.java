package com.geoscene.topo.admin.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;

/***
 * 部门管理-条件查询 请求参数
 * @author zhj
 * @create 2022/11/10
 **/
@Data
public class UmsDeptPageDTO {

    @Schema(description = "部门名称")
    private String deptName;

    @Schema(description = "父id")
    private Long parentId;

    @Schema(description = "分页大小")
    private Integer pageSize = 5;

    @Schema(description = "当前分页")
    private Integer pageNum = 1;
}
