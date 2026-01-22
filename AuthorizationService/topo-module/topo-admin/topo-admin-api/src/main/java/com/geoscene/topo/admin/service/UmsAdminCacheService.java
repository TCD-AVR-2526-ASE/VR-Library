package com.geoscene.topo.admin.service;


import com.geoscene.topo.admin.entity.UmsAdmin;
import com.geoscene.topo.admin.vo.CurrentUserVO;

/**
 * 后台用户缓存操作类
 */
public interface UmsAdminCacheService {
    /**
     * 删除后台用户缓存 用户详细信息缓存
     */
    void delAdmin(Long adminId);

    /**
     * 获取缓存后台用户信息
     */
    UmsAdmin getAdmin(Long adminId);

    /**
     * 设置缓存后台用户信息
     *
     * @param admin 后台用户信息
     */
    void setAdmin(UmsAdmin admin);

    /**
     * 获取用户详细信息
     *
     * @param adminId 用户id
     * @return 缓存中的用户详细信息
     */
    CurrentUserVO getAdminDto(Long adminId);

    /**
     * 存储用户详细信息到缓存
     *
     * @param dto 用户详细信息
     */
    void setAdminDto(CurrentUserVO dto);
}
