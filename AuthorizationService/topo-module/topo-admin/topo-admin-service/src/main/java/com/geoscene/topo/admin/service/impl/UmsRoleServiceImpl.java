package com.geoscene.topo.admin.service.impl;

import cn.hutool.core.collection.CollUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import com.geoscene.topo.admin.entity.*;
import com.geoscene.topo.admin.mapper.UmsRoleMapper;
import com.geoscene.topo.admin.service.UmsRoleService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Lazy;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;

import com.geoscene.topo.admin.service.UmsMenuService;
import com.geoscene.topo.admin.service.UmsRoleMenuRelationService;
import com.geoscene.topo.admin.service.UmsAdminRoleRelationService;
import com.geoscene.topo.admin.mapper.UmsAdminMapper;


@Service
public class UmsRoleServiceImpl extends ServiceImpl<UmsRoleMapper, UmsRole> implements UmsRoleService {

    @Lazy
    private final UmsMenuService menuService;

    private final UmsRoleMenuRelationService menuRelationService;

    private final UmsAdminRoleRelationService adminRoleRelationService;

    private final UmsAdminMapper adminMapper;

    public UmsRoleServiceImpl(@Lazy UmsMenuService menuService, UmsRoleMenuRelationService menuRelationService, UmsAdminRoleRelationService adminRoleRelationService, UmsAdminMapper adminMapper) {
        this.menuService = menuService;
        this.menuRelationService = menuRelationService;
        this.adminRoleRelationService = adminRoleRelationService;
        this.adminMapper = adminMapper;
    }

    @Override
    public List<UmsRole> getRoleList(Long adminId) {
        return this.baseMapper.getRoleList(adminId);
    }

    @Override
    public List<UmsMenu> listMenu(Long roleId) {
        return menuService.getMenuListByRoleIds(Collections.singletonList(roleId));
    }

    @Override
    public int allocMenu(Long roleId, List<Long> menuIds) {
        //先删除原有关系
        LambdaQueryWrapper<UmsRoleMenuRelation> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsRoleMenuRelation::getRoleId, roleId);
        menuRelationService.remove(lambda);
        //批量插入新关系
        List<UmsRoleMenuRelation> relationList = new ArrayList<>();
        for (Long menuId : menuIds) {
            UmsRoleMenuRelation relation = new UmsRoleMenuRelation();
            relation.setRoleId(roleId);
            relation.setMenuId(menuId);
            relationList.add(relation);
        }
        menuRelationService.saveBatch(relationList);
        return menuIds.size();
    }

    @Override
    @Transactional
    public Boolean userAuth(Long roleId, List<Long> adminIds) {
        //根据roleId 删除原角色-用户关联关系
        LambdaQueryWrapper<UmsAdminRoleRelation> wrapper = new LambdaQueryWrapper<>();
        wrapper.eq(UmsAdminRoleRelation::getRoleId, roleId);
        adminRoleRelationService.remove(wrapper);
        //创建新的关联关系
        if (CollUtil.isNotEmpty(adminIds)) {
            List<UmsAdminRoleRelation> insertList = new ArrayList<>();
            for (Long adminId : adminIds) {
                UmsAdminRoleRelation adminRoleRelation = new UmsAdminRoleRelation();
                adminRoleRelation.setRoleId(roleId);
                adminRoleRelation.setAdminId(adminId);
                insertList.add(adminRoleRelation);
            }
            return adminRoleRelationService.saveBatch(insertList);
        } else {
            return true;
        }
    }

    @Override
    public List<UmsAdmin> qryUserAuthedById(Long roleId) {
        //根据roleId查询关联adminId
        LambdaQueryWrapper<UmsAdminRoleRelation> wrapper = new LambdaQueryWrapper<>();
        wrapper.eq(UmsAdminRoleRelation::getRoleId, roleId);
        List<UmsAdminRoleRelation> adminRoleRelations = adminRoleRelationService.list(wrapper);
        if (CollUtil.isNotEmpty(adminRoleRelations)) {
            //根据adminId集合查询用户信息
            List<Long> adminIds = adminRoleRelations.stream()
                    .map(UmsAdminRoleRelation::getAdminId)
                    .collect(Collectors.toList());
            return adminMapper.selectBatchIds(adminIds);
        }
        return new ArrayList<UmsAdmin>();
    }
}
