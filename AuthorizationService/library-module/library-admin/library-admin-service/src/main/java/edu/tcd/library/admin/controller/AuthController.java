package edu.tcd.library.admin.controller;


import edu.tcd.library.admin.dto.UmsAdminLoginDTO;
import edu.tcd.library.admin.service.UmsAdminService;
import edu.tcd.library.admin.vo.CurrentUserVO;
import edu.tcd.library.admin.vo.Oauth2TokenVO;
import edu.tcd.library.admin.service.AuthService;
import edu.tcd.library.common.core.api.CommonResult;
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
