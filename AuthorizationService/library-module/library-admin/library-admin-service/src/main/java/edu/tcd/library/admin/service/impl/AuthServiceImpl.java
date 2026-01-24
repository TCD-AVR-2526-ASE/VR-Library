package edu.tcd.library.admin.service.impl;


import cn.dev33.satoken.stp.SaTokenInfo;
import cn.dev33.satoken.stp.StpUtil;
import cn.dev33.satoken.temp.SaTempUtil;
import cn.hutool.crypto.digest.BCrypt;
import edu.tcd.library.admin.service.UmsAdminService;
import edu.tcd.library.admin.vo.Oauth2TokenVO;
import edu.tcd.library.admin.service.AuthService;
import edu.tcd.library.common.core.api.CommonResult;
import edu.tcd.library.common.core.domain.UserDto;
import edu.tcd.library.common.security.utils.SecurityUtils;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import static edu.tcd.library.common.security.constants.TokenConstant.REFRESH_TOKEN_EXPIRE;
import static edu.tcd.library.common.security.constants.TokenConstant.TOKEN_PREFIX;

@Service
public class AuthServiceImpl implements AuthService {

    @Value("${sa-token.timeout}")
    private String expireTime;

    private final UmsAdminService adminService;

    public AuthServiceImpl(UmsAdminService adminService) {
        this.adminService = adminService;
    }


    @Override
    public CommonResult<Oauth2TokenVO> login(String username, String password) {
        UserDto dto = adminService.loadUserByUsername(username);
        if (BCrypt.checkpw(password, dto.getPassword())) {
            StpUtil.login(dto.getId());
            SecurityUtils.setUserCache(dto);
        }
        SaTokenInfo tokenInfo = StpUtil.getTokenInfo();
        String refreshToken = SaTempUtil.createToken(dto.getId(), REFRESH_TOKEN_EXPIRE);

        Oauth2TokenVO tokenVO = Oauth2TokenVO.builder()
                .token(tokenInfo.tokenValue)
                .expiresIn(Long.valueOf(expireTime))
                .refreshToken(refreshToken)
                .tokenHead(TOKEN_PREFIX)
                .build();

        return CommonResult.success(tokenVO);
    }

    @Override
    public CommonResult<Oauth2TokenVO> refresh(String refreshToken) {
        Object userId = SaTempUtil.parseToken(refreshToken);
        String accessToken = "";
        String newRefreshToken = SaTempUtil.createToken(userId, REFRESH_TOKEN_EXPIRE);

        UserDto userCache = SecurityUtils.getUserCache();
        if (userCache == null) {
            UserDto userDto = adminService.loadUserByUserId(Long.valueOf(userId.toString()));
            StpUtil.login(userDto.getId());
            SaTokenInfo tokenInfo = StpUtil.getTokenInfo();
            accessToken = tokenInfo.tokenValue;
            SecurityUtils.setUserCache(userDto);
        } else {
            accessToken = StpUtil.createLoginSession(userId);
        }

        Oauth2TokenVO tokenVO = Oauth2TokenVO.builder()
                .token(accessToken)
                .expiresIn(Long.valueOf(expireTime))
                .refreshToken(newRefreshToken)
                .tokenHead(TOKEN_PREFIX)
                .build();

        SaTempUtil.deleteToken(refreshToken);
        return CommonResult.success(tokenVO);
    }

    @Override
    public CommonResult<Boolean> logout() {
        SecurityUtils.removeUserCache();
        StpUtil.logout();
        return CommonResult.success();
    }
}
