package com.geoscene.topo.admin.service.impl;

import cn.hutool.core.collection.CollUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import com.geoscene.topo.admin.entity.UmsMenu;
import com.geoscene.topo.admin.entity.UmsMenuNode;
import com.geoscene.topo.admin.entity.UmsRole;
import com.geoscene.topo.admin.entity.UmsRoleMenuRelation;
import com.geoscene.topo.admin.mapper.UmsMenuMapper;
import com.geoscene.topo.admin.service.UmsMenuService;
import com.geoscene.topo.admin.service.UmsRoleMenuRelationService;
import org.springframework.beans.BeanUtils;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

@Service
public class UmsMenuServiceImpl extends ServiceImpl<UmsMenuMapper, UmsMenu> implements UmsMenuService {

    private final UmsRoleMenuRelationService roleMenuRelationService;

    public UmsMenuServiceImpl(UmsRoleMenuRelationService roleMenuRelationService) {
        this.roleMenuRelationService = roleMenuRelationService;
    }

    @Override
    public List<UmsMenuNode> treeList() {
        List<UmsMenu> menuList = this.baseMapper.selectList(null);
        return menuList.stream()
                .filter(menu -> menu.getParentId().equals(0L))
                .map(menu -> covertMenuNode(menu, menuList)).collect(Collectors.toList());
    }

    @Override
    public List<UmsMenu> getMenuListByRoleList(List<UmsRole> roleList) {
        List<Long> roleIds = roleList.stream().map(UmsRole::getId).collect(Collectors.toList());
        return this.getMenuListByRoleIds(roleIds);
    }


    public List<UmsMenu> getMenuListByRoleIds(List<Long> roleIds) {
        LambdaQueryWrapper<UmsRoleMenuRelation> lambdaRelation = new LambdaQueryWrapper<>();
        lambdaRelation.in(UmsRoleMenuRelation::getRoleId, roleIds);
        List<UmsRoleMenuRelation> menuRelations = roleMenuRelationService.list(lambdaRelation);
        List<Long> menuIds = menuRelations.stream().map(UmsRoleMenuRelation::getMenuId).collect(Collectors.toList());
        if (CollUtil.isNotEmpty(menuIds)) {
            LambdaQueryWrapper<UmsMenu> lambdaMenu = new LambdaQueryWrapper<>();
            lambdaMenu.in(UmsMenu::getId, menuIds);
            return this.baseMapper.selectList(lambdaMenu);
        } else {
            return new ArrayList<>();
        }
    }


    /**
     * 将UmsMenu转化为UmsMenuNode并设置children属性
     */
    private UmsMenuNode covertMenuNode(UmsMenu menu, List<UmsMenu> menuList) {
        UmsMenuNode node = new UmsMenuNode();
        BeanUtils.copyProperties(menu, node);
        List<UmsMenuNode> children = menuList.stream()
                .filter(subMenu -> subMenu.getParentId().equals(menu.getId()))
                .map(subMenu -> covertMenuNode(subMenu, menuList)).collect(Collectors.toList());
        node.setChildren(children);
        return node;
    }
}
