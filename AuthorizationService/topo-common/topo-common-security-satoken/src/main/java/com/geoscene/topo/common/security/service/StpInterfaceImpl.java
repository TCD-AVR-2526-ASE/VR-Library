package com.geoscene.topo.common.security.service;

import cn.dev33.satoken.stp.StpInterface;
import com.geoscene.topo.common.core.domain.UserDto;
import com.geoscene.topo.common.security.utils.SecurityUtils;
import org.springframework.stereotype.Component;

import java.util.List;

@Component
public class StpInterfaceImpl implements StpInterface {

    // todo permission改造
    @Override
    public List<String> getPermissionList(Object loginId, String loginType) {
        return List.of();
    }

    /**
     * 获取角色信息
     *
     * @param loginId   账号id
     * @param loginType 账号类型
     * @return List<String>
     */
    @Override
    public List<String> getRoleList(Object loginId, String loginType) {
        UserDto userCache = SecurityUtils.getUserCache();
        return userCache.getRoles();
    }
}
