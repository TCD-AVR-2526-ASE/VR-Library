package com.geoscene.topo.admin.service;

import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.baomidou.mybatisplus.extension.service.IService;
import com.geoscene.topo.admin.dto.UmsDeptPageDTO;
import com.geoscene.topo.admin.entity.UmsDept;
import com.geoscene.topo.admin.entity.UmsDeptNode;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

public interface UmsDeptService extends IService<UmsDept> {
    /**
     * 获取部门树
     * TODO 可能需要控制展示节点权限
     *
     * @return
     */
    List<UmsDeptNode> treeList();

    /**
     * 条件分页查询部门列表
     *
     * @param umsDeptPageParam 条件
     * @return 分页数据
     */
    Page<UmsDept> pageListByCondition(UmsDeptPageDTO umsDeptPageParam);

    /**
     * 删除部门
     *
     * @param id 部门id
     * @return
     */
    @Transactional
    Boolean deleteDept(Long id);
}
