package com.geoscene.topo.admin.service.impl;

import cn.hutool.core.bean.BeanUtil;
import cn.hutool.core.bean.copier.CopyOptions;
import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.date.StopWatch;
import cn.hutool.core.util.StrUtil;
import cn.hutool.crypto.digest.BCrypt;
import cn.hutool.extra.spring.SpringUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.core.toolkit.Wrappers;
import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import com.geoscene.topo.admin.dto.UmsAdminDTO;
import com.geoscene.topo.admin.dto.UpdateAdminPasswordDTO;
import com.geoscene.topo.admin.entity.*;
import com.geoscene.topo.admin.mapper.UmsAdminMapper;
import com.geoscene.topo.admin.service.*;
import com.geoscene.topo.admin.vo.CurrentUserVO;
import com.geoscene.topo.common.core.api.CommonResult;
import com.geoscene.topo.common.core.domain.UserDto;
import com.geoscene.topo.common.minio.enums.BucketEnums;
import com.geoscene.topo.common.minio.service.MinioService;
import com.geoscene.topo.common.security.utils.SecurityUtils;
import org.springframework.beans.BeanUtils;
import org.springframework.stereotype.Service;
import org.springframework.util.Assert;
import org.springframework.util.CollectionUtils;
import org.springframework.web.multipart.MultipartFile;

import java.util.*;
import java.util.stream.Collectors;

@Service
public class UmsAdminServiceImpl extends ServiceImpl<UmsAdminMapper, UmsAdmin> implements UmsAdminService {

    private final UmsRoleService roleService;


    private final UmsAdminRoleRelationService roleRelationService;

    private final MinioService minioService;


    public UmsAdminServiceImpl(UmsRoleService roleService,
                               UmsAdminRoleRelationService roleRelationService,
                               MinioService minioService) {
        this.roleService = roleService;
        this.roleRelationService = roleRelationService;
        this.minioService = minioService;
    }

    //todo 待改写
    public UmsAdminCacheService getCacheService() {
        return SpringUtil.getBean(UmsAdminCacheService.class);
    }

    @Override
    public UserDto loadUserByUsername(String username) {
        //获取用户信息
        UmsAdmin admin = getAdminByUsername(username);
        return getUserDto(admin);
    }

    @Override
    public UserDto loadUserByUserId(Long userid) {
        //获取用户信息
        UmsAdmin admin = getById(userid);
        return getUserDto(admin);
    }

    private UserDto getUserDto(UmsAdmin admin) {
        if (admin != null) {
            UserDto userDTO = new UserDto();
            BeanUtils.copyProperties(admin, userDTO);
            List<UmsRole> roleList = roleService.getRoleList(admin.getId());
            if (CollUtil.isNotEmpty(roleList)) {
                List<String> roleStrList =
                        roleList.stream().map(UmsRole::getCode).collect(Collectors.toList());
                userDTO.setRoles(roleStrList);
            }
            return userDTO;
        }
        return null;
    }

    @Override
    public UmsAdmin getAdminByUsername(String username) {
        LambdaQueryWrapper<UmsAdmin> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdmin::getUsername, username);
        return this.baseMapper.selectOne(lambda);
    }

    @Override
    public UmsAdmin register(MultipartFile icon, UmsAdminDTO umsAdminParam) {
        UmsAdmin umsAdmin = new UmsAdmin();
        BeanUtils.copyProperties(umsAdminParam, umsAdmin);
        umsAdmin.setStatus(1);
        //查询是否有相同用户名的用户
        LambdaQueryWrapper<UmsAdmin> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdmin::getUsername, umsAdmin.getUsername());
        Long iCount = this.baseMapper.selectCount(lambda);
        if (iCount > 0) {
            throw new RuntimeException("用户名重复！");
        }
        //头像存储到minio
        if (icon != null && !icon.isEmpty()) {
            String minioFilePath = minioService.upload(icon, BucketEnums.ADMIN.getName());
            umsAdmin.setIcon(minioFilePath);
        }
        //将密码进行加密操作
        String encodePassword = BCrypt.hashpw(umsAdmin.getPassword());
        umsAdmin.setPassword(encodePassword);
        this.baseMapper.insert(umsAdmin);
        return umsAdmin;
    }

    /**
     * todo 添加登录记录
     *
     * @param username  用户名
     * @param stopWatch
     */
    private void addLoginLog(String username, StopWatch stopWatch) {

    }


    @Override
    public boolean update(Long id, MultipartFile icon, UmsAdminDTO param) {
        UmsAdmin rawAdmin = this.baseMapper.selectById(id);
        BeanUtil.copyProperties(param, rawAdmin, CopyOptions.create().ignoreNullValue());
        if (StrUtil.isNotEmpty(param.getPassword()) &&
                !BCrypt.checkpw(rawAdmin.getPassword(), param.getPassword())) {
            //与原加密密码不同的需要加密修改
            rawAdmin.setPassword(BCrypt.hashpw(param.getPassword()));
        }
        if (icon != null && !icon.isEmpty()) {
            minioService.remove(rawAdmin.getIcon(), BucketEnums.ADMIN.getName());
            //头像存储到minio
            String minioFilePath = minioService.upload(icon, BucketEnums.ADMIN.getName());
            rawAdmin.setIcon(minioFilePath);
        }
        int count = this.baseMapper.updateById(rawAdmin);
        getCacheService().delAdmin(id);
        return count > 0;
    }

    @Override
    public CommonResult<Boolean> updatePassword(UpdateAdminPasswordDTO param) {
        if (StrUtil.isEmpty(param.getUsername())
                || StrUtil.isEmpty(param.getOldPassword())
                || StrUtil.isEmpty(param.getNewPassword())) {
            return CommonResult.failed("提交参数不合法");
        }
        LambdaQueryWrapper<UmsAdmin> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdmin::getUsername, param.getUsername());

        List<UmsAdmin> adminList = this.baseMapper.selectList(lambda);
        if (CollUtil.isEmpty(adminList)) {
            return CommonResult.failed("找不到该用户");
        }
        UmsAdmin umsAdmin = adminList.get(0);
        if (!BCrypt.checkpw(param.getOldPassword(), umsAdmin.getPassword())) {
            return CommonResult.failed("旧密码错误");
        }
        umsAdmin.setPassword(BCrypt.hashpw(param.getNewPassword()));
        this.baseMapper.update(umsAdmin, lambda);
//        getCacheService().delAdmin(umsAdmin.getId());
        return CommonResult.success(true);
    }

    @Override
    public CommonResult<Boolean> updateMyPassword(UpdateAdminPasswordDTO param) {
        UserDto userDto = SecurityUtils.getUserCache();
        UmsAdmin umsAdmin = getCurrentAdmin(userDto.getId());
        if (!umsAdmin.getUsername().equals(param.getUsername())) {
            return CommonResult.failed("只能修改自己的密码！");
        }
        return updatePassword(param);
    }

    @Override
    public CurrentUserVO getAdminInfo() {
        UserDto userDto = SecurityUtils.getUserCache();
        UmsAdmin umsAdmin = getCurrentAdmin(userDto.getId());
        return getCurrentUserVO(umsAdmin);
    }


    @Override
    public CurrentUserVO getAdminInfoById(Long id) {
        UmsAdmin umsAdmin = getCurrentAdmin(id);
        return getCurrentUserVO(umsAdmin);
    }

    /**
     * 获取redis缓存中的当前登录用户信息
     *
     * @return
     */
    private UmsAdmin getCurrentAdmin(Long userid) {
        UmsAdmin admin = getCacheService().getAdmin(userid);
        if (admin == null) {
            admin = this.baseMapper.selectById(userid);
            //用户头像从minio获取
            if (StrUtil.isNotEmpty(admin.getIcon())) {
                admin.setIcon(minioService.preview(admin.getIcon(), BucketEnums.ADMIN.getName()));
            }
            getCacheService().setAdmin(admin);
        }
        return admin;
    }

    /**
     * 组织dto信息
     *
     * @param umsAdmin admin信息
     * @return
     */
    private CurrentUserVO getCurrentUserVO(UmsAdmin umsAdmin) {
        CurrentUserVO vo = getCacheService().getAdminDto(umsAdmin.getId());
        if (vo == null) {
            vo = new CurrentUserVO();
            vo.setUserId(umsAdmin.getId());
            vo.setUsername(umsAdmin.getUsername());
            vo.setIcon(umsAdmin.getIcon());
            //角色list
            List<UmsRole> roleList = roleService.getRoleList(umsAdmin.getId());
            if (CollUtil.isNotEmpty(roleList)) {
                List<Long> roles = roleList.stream().map(UmsRole::getId).collect(Collectors.toList());
                vo.setRoles(roles);
            }
            getCacheService().setAdminDto(vo);
        }
        return vo;
    }


    @Override
    public Page<UmsAdminExtend> selectPage(Long deptId, String keyword, String nickName, String userName, Page<UmsAdminExtend> page) {
        Page<UmsAdminExtend> umsAdminPage = this.baseMapper.selectAdminPage(page, deptId, nickName, userName, keyword);
        List<UmsAdminExtend> records = umsAdminPage.getRecords();
        for (UmsAdminExtend admin : records) {
            if (StrUtil.isNotEmpty(admin.getIcon())) {
                admin.setIcon(minioService.preview(admin.getIcon(), BucketEnums.ADMIN.getName()));
            }
        }
        return umsAdminPage;
    }

    @Override
    public boolean updateRole(Long adminId, List<Long> roleIds) {
        //先删除原来的关系
        LambdaQueryWrapper<UmsAdminRoleRelation> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdminRoleRelation::getAdminId, adminId);
        roleRelationService.remove(lambda);
        //建立新关系
        if (!CollectionUtils.isEmpty(roleIds)) {
            List<UmsAdminRoleRelation> list = new ArrayList<>();
            for (Long roleId : roleIds) {
                UmsAdminRoleRelation roleRelation = new UmsAdminRoleRelation();
                roleRelation.setAdminId(adminId);
                roleRelation.setRoleId(roleId);
                list.add(roleRelation);
            }
            roleRelationService.saveBatch(list);
        }
        return true;
    }

    @Override
    public List<UmsRole> getRoleList(Long adminId) {
        LambdaQueryWrapper<UmsAdminRoleRelation> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdminRoleRelation::getAdminId, adminId);
        List<UmsAdminRoleRelation> list = roleRelationService.list(lambda);
        List<Long> roleIds = list.stream().map(UmsAdminRoleRelation::getRoleId).collect(Collectors.toList());
        return roleService.listByIds(roleIds);
    }

}
