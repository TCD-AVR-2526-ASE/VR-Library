package edu.tcd.library.admin.service;

import edu.tcd.library.admin.vo.Oauth2TokenVO;
import edu.tcd.library.common.core.api.CommonResult;

public interface AuthService {

    /**
     * User login
     *
     * @param username Username
     * @param password Password
     * @return login result
     */
    CommonResult<Oauth2TokenVO> login(String username, String password);

    /**
     * Refresh token
     *
     * @param refreshToken Refresh token
     * @return login result
     */
    CommonResult<Oauth2TokenVO> refresh(String refreshToken);

    /**
     * Logout
     */
    CommonResult<Boolean> logout();
}