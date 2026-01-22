package com.geoscene.topo.admin.service.impl;

import cn.hutool.core.util.StrUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import com.geoscene.topo.admin.dto.UmsDeptPageDTO;
import com.geoscene.topo.admin.entity.UmsAdminDeptRelation;
import com.geoscene.topo.admin.entity.UmsDept;
import com.geoscene.topo.admin.entity.UmsDeptNode;
import com.geoscene.topo.admin.mapper.UmsAdminDeptRelationMapper;
import com.geoscene.topo.admin.mapper.UmsDeptMapper;
import com.geoscene.topo.admin.service.UmsDeptService;
import org.springframework.beans.BeanUtils;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Objects;
import java.util.stream.Collectors;

@Service
public class UmsDeptServiceImpl extends ServiceImpl<UmsDeptMapper, UmsDept> implements UmsDeptService {

    private final UmsDeptMapper deptMapper;

    private final UmsAdminDeptRelationMapper relationMapper;

    public UmsDeptServiceImpl(UmsDeptMapper deptMapper, UmsAdminDeptRelationMapper relationMapper) {
        this.deptMapper = deptMapper;
        this.relationMapper = relationMapper;
    }

    @Override
    public List<UmsDeptNode> treeList() {
        LambdaQueryWrapper<UmsDept> lambda = new LambdaQueryWrapper<>();
        lambda.orderByAsc(UmsDept::getSort);
        List<UmsDept> deptList = this.baseMapper.selectList(lambda);
        return deptList.stream()
                .filter(menu -> menu.getParentId().equals(0L))
                .map(menu -> covertMenuNode(menu, deptList)).collect(Collectors.toList());
    }

    @Override
    public Page<UmsDept> pageListByCondition(UmsDeptPageDTO umsDeptPageParam) {
        Page<UmsDept> page = new Page<>(umsDeptPageParam.getPageNum(), umsDeptPageParam.getPageSize());
        LambdaQueryWrapper<UmsDept> queryWrapper = new LambdaQueryWrapper<>();
        queryWrapper.like(StrUtil.isNotEmpty(umsDeptPageParam.getDeptName()), UmsDept::getName,
                umsDeptPageParam.getDeptName());
        queryWrapper.eq(Objects.nonNull(umsDeptPageParam.getParentId()), UmsDept::getParentId,
                umsDeptPageParam.getParentId());
        queryWrapper.orderByAsc(UmsDept::getSort);
        return deptMapper.selectPage(page, queryWrapper);
    }

    @Override
    public Boolean deleteDept(Long id) {
        boolean deleted = this.removeById(id);
        if (deleted) {
            LambdaQueryWrapper<UmsAdminDeptRelation> lambda = new LambdaQueryWrapper<>();
            lambda.eq(UmsAdminDeptRelation::getDeptId, id);
            int delete = relationMapper.delete(lambda);
            if (delete < 0) {
                throw new RuntimeException("删除部门关联信息失败！");
            }
        } else {
            throw new RuntimeException("删除部门失败！");
        }
        return true;
    }

    /**
     * 将UmsDept转化为UmsDeptNode并设置children属性
     */
    private UmsDeptNode covertMenuNode(UmsDept dept, List<UmsDept> deptList) {
        UmsDeptNode node = new UmsDeptNode();
        BeanUtils.copyProperties(dept, node);
        List<UmsDeptNode> children = deptList.stream()
                .filter(subDept -> subDept.getParentId().equals(dept.getId()))
                .map(subDept -> covertMenuNode(subDept, deptList)).collect(Collectors.toList());
        node.setChildren(children);
        return node;
    }
}
