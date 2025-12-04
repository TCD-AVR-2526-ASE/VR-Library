package com.geoscene.topo.admin.controller;


import com.geoscene.topo.admin.dto.UmsAdminLoginDTO;
import com.geoscene.topo.admin.service.UmsAdminService;
import com.geoscene.topo.admin.vo.CurrentUserVO;
import com.geoscene.topo.admin.vo.Oauth2TokenVO;
import com.geoscene.topo.admin.service.AuthService;
import com.geoscene.topo.common.core.api.CommonResult;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.*;

@RestController
@Tag(name = "auth management")
@RequestMapping("/auth")
public class AuthController {

    @Autowired
    private AuthService authService;

    @Autowired
    private UmsAdminService adminService;


    @Operation(summary = "login")
    @RequestMapping(value = "/login", method = RequestMethod.POST)
    public CommonResult<Oauth2TokenVO> login(@Validated @RequestBody UmsAdminLoginDTO umsAdminLoginParam) {
        return authService.login(umsAdminLoginParam.getUsername(), umsAdminLoginParam.getPassword());
    }

    @Operation(summary = "refresh token")
    @RequestMapping(value = "/refresh", method = RequestMethod.POST)
    public CommonResult<Oauth2TokenVO> refresh(@RequestParam(required = true) String refreshToken) {
        return authService.refresh(refreshToken);
    }

    @Operation(summary = "get user info")
    @RequestMapping(value = "/info", method = RequestMethod.GET)
    public CommonResult<CurrentUserVO> getAdminInfo() {
        return CommonResult.success(adminService.getAdminInfo());
    }

}
