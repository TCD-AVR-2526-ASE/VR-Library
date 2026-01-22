package com.geoscene.topo.admin.service;

import com.baomidou.mybatisplus.extension.service.IService;
import com.geoscene.topo.admin.entity.UmsAdmin;
import com.geoscene.topo.admin.entity.UmsMenu;
import com.geoscene.topo.admin.entity.UmsRole;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

public interface UmsRoleService extends IService<UmsRole> {

    /**
     * 获取用户对应角色
     *
     * @param adminId 用户id
     * @return
     */
    List<UmsRole> getRoleList(Long adminId);

    /**
     * 获取角色对应的菜单列表
     *
     * @param roleId 角色id
     * @return 菜单列表
     */
    List<UmsMenu> listMenu(Long roleId);

    /**
     * 给角色分配菜单
     *
     * @param roleId  角色id
     * @param menuIds 菜单ids
     * @return
     */
    @Transactional
    int allocMenu(Long roleId, List<Long> menuIds);

    /**
     * 将角色授权给多个用户
     *
     * @param roleId   角色id
     * @param adminIds 用户id集合
     * @return
     */
    Boolean userAuth(Long roleId, List<Long> adminIds);

    /**
     * 根据角色查询已授权用户
     *
     * @param roleId 角色id
     * @return 已授权用户列表
     */
    List<UmsAdmin> qryUserAuthedById(Long roleId);
}
