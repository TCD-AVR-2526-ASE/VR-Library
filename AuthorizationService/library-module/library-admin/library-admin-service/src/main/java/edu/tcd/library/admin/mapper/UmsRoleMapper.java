package edu.tcd.library.admin.mapper;

import com.baomidou.mybatisplus.core.mapper.BaseMapper;
import edu.tcd.library.admin.entity.UmsRole;
import org.apache.ibatis.annotations.Param;

import java.util.List;

public interface UmsRoleMapper extends BaseMapper<UmsRole> {

    List<UmsRole> getRoleList(@Param("adminId") Long adminId);
}
