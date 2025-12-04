package com.geoscene.topo.common.security.utils;

import cn.dev33.satoken.context.SaHolder;
import cn.dev33.satoken.stp.StpUtil;
import com.geoscene.topo.common.core.domain.UserDto;
import lombok.extern.slf4j.Slf4j;

import static com.geoscene.topo.common.security.constants.TokenConstant.TOKEN_INFO_CACHE_KEY;

@Slf4j
public class SecurityUtils {

    /**
     * 设置用户数据缓存
     *
     * @param user UserDto
     */
    public static void setUserCache(UserDto user) {
        StpUtil.getTokenSession().set(TOKEN_INFO_CACHE_KEY, user);
    }

    /**
     * 清除用户数据缓存
     */
    public static void removeUserCache() {
        StpUtil.getTokenSession().delete(TOKEN_INFO_CACHE_KEY);
    }


    /**
     * 获取用户数据 多级缓存
     *
     * @return UserDto
     */
    public static UserDto getUserCache() {
        UserDto userDto = (UserDto) SaHolder.getStorage().get(TOKEN_INFO_CACHE_KEY);
        if (userDto != null) {
            return userDto;
        }
        try {
            userDto = (UserDto) StpUtil.getTokenSession().get(TOKEN_INFO_CACHE_KEY);
            SaHolder.getStorage().set(TOKEN_INFO_CACHE_KEY, userDto);
        } catch (Exception ex) {
            log.error("获取用户数据缓存信息失败:{}", ex.getMessage());
        }
        return userDto;
    }
}
