package com.geoscene.topo.admin.mapper;

import com.baomidou.mybatisplus.core.mapper.BaseMapper;
import com.geoscene.topo.admin.entity.UmsRole;
import org.apache.ibatis.annotations.Param;

import java.util.List;

public interface UmsRoleMapper extends BaseMapper<UmsRole> {

    /**
     * 获取用于所有角色
     */
    List<UmsRole> getRoleList(@Param("adminId") Long adminId);
}
