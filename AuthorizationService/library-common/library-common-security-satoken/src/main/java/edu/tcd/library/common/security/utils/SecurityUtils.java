package edu.tcd.library.common.security.utils;

import cn.dev33.satoken.context.SaHolder;
import cn.dev33.satoken.stp.StpUtil;
import edu.tcd.library.common.core.domain.UserDto;
import lombok.extern.slf4j.Slf4j;

import static edu.tcd.library.common.security.constants.TokenConstant.TOKEN_INFO_CACHE_KEY;

@Slf4j
public class SecurityUtils {

    public static void setUserCache(UserDto user) {
        StpUtil.getTokenSession().set(TOKEN_INFO_CACHE_KEY, user);
    }

    public static void removeUserCache() {
        StpUtil.getTokenSession().delete(TOKEN_INFO_CACHE_KEY);
    }

    public static UserDto getUserCache() {
        UserDto userDto = (UserDto) SaHolder.getStorage().get(TOKEN_INFO_CACHE_KEY);
        if (userDto != null) {
            return userDto;
        }
        try {
            userDto = (UserDto) StpUtil.getTokenSession().get(TOKEN_INFO_CACHE_KEY);
            SaHolder.getStorage().set(TOKEN_INFO_CACHE_KEY, userDto);
        } catch (Exception ex) {
            log.error("Failed to retrieve user data cache:{}", ex.getMessage());
        }
        return userDto;
    }
}
