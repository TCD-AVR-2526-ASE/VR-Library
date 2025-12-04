package com.geoscene.topo.admin.controller;

import cn.hutool.core.util.StrUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.geoscene.topo.admin.entity.UmsAdmin;
import com.geoscene.topo.admin.entity.UmsMenu;
import com.geoscene.topo.admin.entity.UmsRole;
import com.geoscene.topo.admin.service.UmsRoleService;
import com.geoscene.topo.common.core.api.CommonPage;
import com.geoscene.topo.common.core.api.CommonResult;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.web.bind.annotation.*;

import java.util.Date;
import java.util.List;

//@RestController
@Tag(name = "role management")
@RequestMapping("/ums/role")
public class UmsRoleController {

    private final UmsRoleService roleService;

    public UmsRoleController(UmsRoleService roleService) {
        this.roleService = roleService;
    }

    @Operation(summary ="list all roles")
    @RequestMapping(value = "/listAll", method = RequestMethod.GET)
    public CommonResult<List<UmsRole>> listAll() {
        List<UmsRole> roleList = roleService.list();
        return CommonResult.success(roleList);
    }

    @Operation(summary ="add role")
    @RequestMapping(value = "/create", method = RequestMethod.POST)
    public CommonResult<Boolean> create(@RequestBody UmsRole role) {
        boolean saved = roleService.save(role);
        return CommonResult.judge(saved);
    }

    @Operation(summary ="update role")
    @RequestMapping(value = "/update/{id}", method = RequestMethod.POST)
    public CommonResult<Boolean> update(@PathVariable Long id,
                                        @RequestBody UmsRole role) {
        role.setId(id);
        boolean updated = roleService.updateById(role);
        return CommonResult.judge(updated);
    }

    @Operation(summary ="batch delete role")
    @RequestMapping(value = "/delete", method = RequestMethod.POST)
    public CommonResult<Boolean> delete(@RequestParam("ids") List<Long> ids) {
        boolean deleted = roleService.removeByIds(ids);
        return CommonResult.judge(deleted);
    }

    @Operation(summary ="search roles")
    @RequestMapping(value = "/list", method = RequestMethod.GET)
    public CommonResult<CommonPage<UmsRole>> list(@RequestParam(value = "keyword", required = false) String keyword,
                                                  @RequestParam(value = "roleName", required = false) String roleName,
                                                  @RequestParam(value = "pageSize", defaultValue = "5") Integer pageSize,
                                                  @RequestParam(value = "pageNum", defaultValue = "1") Integer pageNum) {
        LambdaQueryWrapper<UmsRole> lambda = new LambdaQueryWrapper<>();
        lambda.like(StrUtil.isNotEmpty(keyword), UmsRole::getName, keyword);
        lambda.like(StrUtil.isNotEmpty(roleName), UmsRole::getName, roleName);
        lambda.orderByAsc(UmsRole::getSort);
        Page<UmsRole> page = new Page<>(pageNum, pageSize);
        return CommonResult.success(CommonPage.restPage(roleService.page(page, lambda)));
    }

    @Operation(summary ="search users by role")
    @RequestMapping(value = "/qryUserAuthedById", method = RequestMethod.GET)
    public CommonResult<List<UmsAdmin>> qryUserAuthedById(@RequestParam Long roleId) {
        List<UmsAdmin> adminList = roleService.qryUserAuthedById(roleId);
        return CommonResult.success(adminList);
    }

    @Operation(summary ="grant role to user")
    @RequestMapping(value = "/userAuth", method = RequestMethod.POST)
    public CommonResult<Boolean> userAuth(@RequestParam Long roleId,
                                          @RequestParam(required = false) List<Long> adminIds) {
        Boolean flag = roleService.userAuth(roleId, adminIds);
        return CommonResult.judge(flag);
    }
}
