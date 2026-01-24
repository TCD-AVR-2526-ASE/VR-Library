package edu.tcd.library.admin.controller;

import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import edu.tcd.library.admin.dto.UmsAdminDTO;
import edu.tcd.library.admin.dto.UpdateAdminPasswordDTO;
import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.entity.UmsAdminExtend;
import edu.tcd.library.admin.entity.UmsRole;
import edu.tcd.library.admin.service.UmsAdminService;
import edu.tcd.library.admin.vo.CurrentUserVO;
import edu.tcd.library.common.core.api.CommonPage;
import edu.tcd.library.common.core.api.CommonResult;
import edu.tcd.library.common.core.domain.UserDto;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.util.List;


@RestController
@Tag(name = "user management")
@RequestMapping("/ums/admin")
public class UmsAdminController {

    private final UmsAdminService adminService;

    public UmsAdminController(UmsAdminService adminService) {
        this.adminService = adminService;
    }


    @Operation(summary = "user register")
    @RequestMapping(value = "/register", method = RequestMethod.POST)
    public CommonResult<UmsAdmin> register(@RequestPart(value = "icon", required = false) MultipartFile icon,
                                           @Validated UmsAdminDTO umsAdminParam) {
        UmsAdmin umsAdmin = adminService.register(icon, umsAdminParam);
        return CommonResult.judge(umsAdmin != null, umsAdmin, "用户注册失败！");
    }

    @Operation(summary = "delete user")
    @RequestMapping(value = "/delete/{id}", method = RequestMethod.POST)
    public CommonResult<Boolean> delete(@PathVariable Long id) {
        boolean deleted = adminService.removeById(id);
        return CommonResult.judge(deleted);
    }

    @Operation(summary = "modify user")
    @RequestMapping(value = "/update/{id}", method = RequestMethod.POST)
    public CommonResult<Boolean> update(@PathVariable Long id,
                                        @RequestPart(value = "icon", required = false) MultipartFile icon,
                                        UmsAdminDTO umsAdminParam
    ) {
        boolean updated = adminService.update(id, icon, umsAdminParam);
        return CommonResult.judge(updated);
    }

    @Operation(summary = "change user status")
    @RequestMapping(value = "/updateStatus/{id}", method = RequestMethod.POST)
    public CommonResult<Boolean> updateStatus(@PathVariable Long id,
                                              @RequestParam(value = "status") Integer status) {
        UmsAdmin umsAdmin = adminService.getById(id);
        umsAdmin.setStatus(status);
        boolean updated = adminService.save(umsAdmin);
        return CommonResult.judge(updated);
    }

    @Operation(summary = "change password")
    @RequestMapping(value = "/updatePassword", method = RequestMethod.POST)
    public CommonResult<Boolean> updatePassword(@RequestBody UpdateAdminPasswordDTO param) {
        return adminService.updatePassword(param);
    }

    @Operation(summary = "change password")
    @RequestMapping(value = "/updateMyPassword", method = RequestMethod.POST)
    public CommonResult<Boolean> updateMyPassword(@RequestBody UpdateAdminPasswordDTO param) {
        return adminService.updateMyPassword(param);
    }

    @Operation(summary = "get information")
    @RequestMapping(value = "/info/{id}", method = RequestMethod.GET)
    public CommonResult<CurrentUserVO> getAdminInfoById(@PathVariable Long id) {
        return CommonResult.success(adminService.getAdminInfoById(id));
    }

    @Operation(summary = "get user list")
    @RequestMapping(value = "/list", method = RequestMethod.GET)
    public CommonResult<CommonPage<UmsAdminExtend>> list(@RequestParam(value = "deptId", required = false) Long deptId,
                                                         @RequestParam(value = "keyword", required = false) String keyword,
                                                         @RequestParam(value = "nickName", required = false) String nickName,
                                                         @RequestParam(value = "userName", required = false) String userName,
                                                         @RequestParam(value = "pageSize", defaultValue = "5") Integer pageSize,
                                                         @RequestParam(value = "pageNum", defaultValue = "1") Integer pageNum) {
        Page<UmsAdminExtend> page = new Page<>(pageNum, pageSize);
        return CommonResult.success(CommonPage.restPage(adminService.selectPage(deptId, keyword, nickName, userName, page)));
    }

    @Operation(summary = "get information by username")
    @RequestMapping(value = "/loadByUsername", method = RequestMethod.GET)
    public UserDto loadUserByUsername(@RequestParam String username) {
        return adminService.loadUserByUsername(username);
    }

    @Operation(summary = "distribute role")
    @RequestMapping(value = "/role/update", method = RequestMethod.POST)
    public CommonResult<Boolean> updateRole(@RequestParam("adminId") Long adminId,
                                            @RequestParam("roleIds") List<Long> roleIds) {
        boolean updated = adminService.updateRole(adminId, roleIds);
        return CommonResult.judge(updated);
    }

    @Operation(summary = "get user roles")
    @RequestMapping(value = "/role/{adminId}", method = RequestMethod.GET)
    public CommonResult<List<UmsRole>> getRoleList(@PathVariable Long adminId) {
        List<UmsRole> roleList = adminService.getRoleList(adminId);
        return CommonResult.success(roleList);
    }

}
