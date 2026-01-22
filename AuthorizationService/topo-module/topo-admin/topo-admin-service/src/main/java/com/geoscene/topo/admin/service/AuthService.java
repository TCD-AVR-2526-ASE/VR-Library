package com.geoscene.topo.admin.service;

import com.geoscene.topo.admin.vo.Oauth2TokenVO;
import com.geoscene.topo.common.core.api.CommonResult;

public interface AuthService {

    /**
     * 用户登录
     *
     * @param username 用户名
     * @param password 密码
     * @return login
     */
    CommonResult<Oauth2TokenVO> login(String username, String password);

    /**
     * 刷新token
     *
     * @param refreshToken 刷新token
     * @return login
     */
    CommonResult<Oauth2TokenVO> refresh(String refreshToken);

    /**
     * 登出
     */
    CommonResult<Boolean> logout();
}
