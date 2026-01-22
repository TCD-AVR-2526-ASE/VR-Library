package com.geoscene.topo.admin.mapper;

import com.baomidou.mybatisplus.core.mapper.BaseMapper;
import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.geoscene.topo.admin.entity.UmsAdmin;
import com.geoscene.topo.admin.entity.UmsAdminExtend;
import org.apache.ibatis.annotations.Param;

public interface UmsAdminMapper extends BaseMapper<UmsAdmin> {

    Page<UmsAdminExtend> selectAdminPage(Page<UmsAdminExtend> page, @Param("deptId") Long deptId,
                                         @Param("keyword") String keyword,
                                         @Param("nickName") String nickName,
                                         @Param("userName") String userName);
}
