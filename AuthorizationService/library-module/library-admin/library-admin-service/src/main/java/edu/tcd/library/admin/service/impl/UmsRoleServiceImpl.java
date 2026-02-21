package edu.tcd.library.admin.service.impl;

import cn.hutool.core.collection.CollUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import edu.tcd.library.admin.entity.*;
import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.entity.UmsAdminRoleRelation;
import edu.tcd.library.admin.entity.UmsRole;
import edu.tcd.library.admin.mapper.UmsRoleMapper;
import edu.tcd.library.admin.service.UmsRoleService;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.Collectors;

import edu.tcd.library.admin.service.UmsAdminRoleRelationService;
import edu.tcd.library.admin.mapper.UmsAdminMapper;


@Service
public class UmsRoleServiceImpl extends ServiceImpl<UmsRoleMapper, UmsRole> implements UmsRoleService {

    private final UmsAdminRoleRelationService adminRoleRelationService;

    private final UmsAdminMapper adminMapper;

    public UmsRoleServiceImpl(UmsAdminRoleRelationService adminRoleRelationService,
                              UmsAdminMapper adminMapper) {
        this.adminRoleRelationService = adminRoleRelationService;
        this.adminMapper = adminMapper;
    }

    @Override
    public List<UmsRole> getRoleList(Long adminId) {
        return this.baseMapper.getRoleList(adminId);
    }

    @Override
    @Transactional
    public Boolean userAuth(Long roleId, List<Long> adminIds) {
        // Delete original role-user associations based on roleId
        LambdaQueryWrapper<UmsAdminRoleRelation> wrapper = new LambdaQueryWrapper<>();
        wrapper.eq(UmsAdminRoleRelation::getRoleId, roleId);
        adminRoleRelationService.remove(wrapper);
        // Create new associations
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
        // Query associated adminIds based on roleId
        LambdaQueryWrapper<UmsAdminRoleRelation> wrapper = new LambdaQueryWrapper<>();
        wrapper.eq(UmsAdminRoleRelation::getRoleId, roleId);
        List<UmsAdminRoleRelation> adminRoleRelations = adminRoleRelationService.list(wrapper);
        if (CollUtil.isNotEmpty(adminRoleRelations)) {
            // Query user information based on adminId list
            List<Long> adminIds = adminRoleRelations.stream()
                    .map(UmsAdminRoleRelation::getAdminId)
                    .collect(Collectors.toList());
            return adminMapper.selectBatchIds(adminIds);
        }
        return new ArrayList<UmsAdmin>();
    }
}