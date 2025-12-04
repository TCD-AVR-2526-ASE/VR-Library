package com.geoscene.topo.admin.service;

import com.baomidou.mybatisplus.extension.service.IService;
import com.geoscene.topo.admin.entity.UmsMenu;
import com.geoscene.topo.admin.entity.UmsMenuNode;
import com.geoscene.topo.admin.entity.UmsRole;

import java.util.List;

public interface UmsMenuService extends IService<UmsMenu> {

    /**
     * 树形结构返回所有菜单列表
     */
    List<UmsMenuNode> treeList();

    /**
     * 根据角色列表获取对应的菜单权限列表
     *
     * @param roleList 角色列表
     * @return 角色对应菜单权限列表
     */
    List<UmsMenu> getMenuListByRoleList(List<UmsRole> roleList);

    /**
     * 根据角色id列表获取对应的菜单权限列表
     *
     * @param roleIds 角色id列表
     * @return 角色对应菜单权限列表
     */
    List<UmsMenu> getMenuListByRoleIds(List<Long> roleIds);
}
