package com.geoscene.topo.admin.service;

import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.baomidou.mybatisplus.extension.service.IService;
import com.geoscene.topo.admin.dto.UmsAdminDTO;
import com.geoscene.topo.admin.dto.UpdateAdminPasswordDTO;
import com.geoscene.topo.admin.entity.UmsAdmin;
import com.geoscene.topo.admin.entity.UmsAdminExtend;
import com.geoscene.topo.admin.entity.UmsDept;
import com.geoscene.topo.admin.entity.UmsRole;
import com.geoscene.topo.admin.vo.CurrentUserVO;
import com.geoscene.topo.common.core.api.CommonResult;
import com.geoscene.topo.common.core.domain.UserDto;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.multipart.MultipartFile;

import java.util.List;

public interface UmsAdminService extends IService<UmsAdmin> {

    /**
     * 根据用户名获取后台管理员
     */
    UmsAdmin getAdminByUsername(String username);

    /**
     * 获取用户信息
     */
    UserDto loadUserByUsername(String username);

    /**
     * 获取用户信息
     */
    UserDto loadUserByUserId(Long userid);

    /**
     * 用户注册
     *
     * @param icon  头像文件流
     * @param param 用户信息
     */
    UmsAdmin register(MultipartFile icon, UmsAdminDTO param);

    /**
     * 修改用户信息
     *
     * @param id    用户id
     * @param icon  头像文件流
     * @param param 用户信息
     * @return boolean
     */
    boolean update(Long id, MultipartFile icon, UmsAdminDTO param);

    /**
     * 更新用户密码
     *
     * @param param 密码参数
     * @return
     */
    CommonResult<Boolean> updatePassword(UpdateAdminPasswordDTO param);

    /**
     * 更新自己密码
     *
     * @param param 密码参数
     * @return
     */
    CommonResult<Boolean> updateMyPassword(UpdateAdminPasswordDTO param);

    /**
     * 获取当前登录用户信息
     *
     * @return
     */
    CurrentUserVO getAdminInfo();

    /**
     * 根据id获取当前登录用户信息
     *
     * @param id 用户id
     * @return
     */
    CurrentUserVO getAdminInfoById(Long id);

    /**
     * 查询用户列表
     *
     * @param deptId  部门id
     * @param keyword 关键词
     * @param page    分页条件
     * @return
     */
    Page<UmsAdminExtend> selectPage(Long deptId, String keyword, String nickName,
                                    String userName, Page<UmsAdminExtend> page);

    /**
     * 修改用户角色关系
     */
    @Transactional
    boolean updateRole(Long adminId, List<Long> roleIds);

    /**
     * 获取用户对应角色
     */
    List<UmsRole> getRoleList(Long adminId);

}
