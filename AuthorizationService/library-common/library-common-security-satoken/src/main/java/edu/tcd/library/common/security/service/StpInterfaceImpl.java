package edu.tcd.library.common.security.service;

import cn.dev33.satoken.stp.StpInterface;
import edu.tcd.library.common.core.domain.UserDto;
import edu.tcd.library.common.security.utils.SecurityUtils;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
public class StpInterfaceImpl implements StpInterface {

    // todo permission revise
    @Override
    public List<String> getPermissionList(Object loginId, String loginType) {
        return List.of();
    }


    @Override
    public List<String> getRoleList(Object loginId, String loginType) {
        UserDto userCache = SecurityUtils.getUserCache();
        return userCache.getRoles();
    }
}
