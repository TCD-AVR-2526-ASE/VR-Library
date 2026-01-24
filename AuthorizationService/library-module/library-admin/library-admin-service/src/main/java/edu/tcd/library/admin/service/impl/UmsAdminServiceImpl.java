package edu.tcd.library.admin.service.impl;

import cn.hutool.core.bean.BeanUtil;
import cn.hutool.core.bean.copier.CopyOptions;
import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.date.StopWatch;
import cn.hutool.core.util.StrUtil;
import cn.hutool.crypto.digest.BCrypt;
import cn.hutool.extra.spring.SpringUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import edu.tcd.library.admin.dto.UmsAdminDTO;
import edu.tcd.library.admin.dto.UpdateAdminPasswordDTO;
import edu.tcd.library.admin.entity.*;
import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.entity.UmsAdminExtend;
import edu.tcd.library.admin.entity.UmsAdminRoleRelation;
import edu.tcd.library.admin.entity.UmsRole;
import edu.tcd.library.admin.mapper.UmsAdminMapper;
import edu.tcd.library.admin.service.*;
import edu.tcd.library.admin.service.UmsAdminCacheService;
import edu.tcd.library.admin.service.UmsAdminRoleRelationService;
import edu.tcd.library.admin.service.UmsAdminService;
import edu.tcd.library.admin.service.UmsRoleService;
import edu.tcd.library.admin.vo.CurrentUserVO;
import edu.tcd.library.common.core.api.CommonResult;
import edu.tcd.library.common.core.domain.UserDto;
import edu.tcd.library.common.minio.enums.BucketEnums;
import edu.tcd.library.common.minio.service.MinioService;
import edu.tcd.library.common.security.utils.SecurityUtils;
import org.springframework.beans.BeanUtils;
import org.springframework.stereotype.Service;
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

    //todo To be rewritten
    public UmsAdminCacheService getCacheService() {
        return SpringUtil.getBean(UmsAdminCacheService.class);
    }

    @Override
    public UserDto loadUserByUsername(String username) {
        // Get user info
        UmsAdmin admin = getAdminByUsername(username);
        return getUserDto(admin);
    }

    @Override
    public UserDto loadUserByUserId(Long userid) {
        // Get user info
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
        // Check if a user with the same username exists
        LambdaQueryWrapper<UmsAdmin> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdmin::getUsername, umsAdmin.getUsername());
        Long iCount = this.baseMapper.selectCount(lambda);
        if (iCount > 0) {
            throw new RuntimeException("Username already exists!");
        }
        // Save avatar to MinIO
        if (icon != null && !icon.isEmpty()) {
            String minioFilePath = minioService.upload(icon, BucketEnums.ADMIN.getName());
            umsAdmin.setIcon(minioFilePath);
        }
        // Encrypt the password
        String encodePassword = BCrypt.hashpw(umsAdmin.getPassword());
        umsAdmin.setPassword(encodePassword);
        this.baseMapper.insert(umsAdmin);
        return umsAdmin;
    }

    /**
     * todo Add login record
     *
     * @param username  Username
     * @param stopWatch StopWatch
     */
    private void addLoginLog(String username, StopWatch stopWatch) {

    }


    @Override
    public boolean update(Long id, MultipartFile icon, UmsAdminDTO param) {
        UmsAdmin rawAdmin = this.baseMapper.selectById(id);
        BeanUtil.copyProperties(param, rawAdmin, CopyOptions.create().ignoreNullValue());
        if (StrUtil.isNotEmpty(param.getPassword()) &&
                !BCrypt.checkpw(rawAdmin.getPassword(), param.getPassword())) {
            // Encrypt and modify if different from the original encrypted password
            rawAdmin.setPassword(BCrypt.hashpw(param.getPassword()));
        }
        if (icon != null && !icon.isEmpty()) {
            minioService.remove(rawAdmin.getIcon(), BucketEnums.ADMIN.getName());
            // Save avatar to MinIO
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
            return CommonResult.failed("Invalid submission parameters");
        }
        LambdaQueryWrapper<UmsAdmin> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdmin::getUsername, param.getUsername());

        List<UmsAdmin> adminList = this.baseMapper.selectList(lambda);
        if (CollUtil.isEmpty(adminList)) {
            return CommonResult.failed("User not found");
        }
        UmsAdmin umsAdmin = adminList.get(0);
        if (!BCrypt.checkpw(param.getOldPassword(), umsAdmin.getPassword())) {
            return CommonResult.failed("Incorrect old password");
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
            return CommonResult.failed("You can only modify your own password!");
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
     * Get current logged-in user info from Redis cache
     *
     * @return UmsAdmin
     */
    private UmsAdmin getCurrentAdmin(Long userid) {
        UmsAdmin admin = getCacheService().getAdmin(userid);
        if (admin == null) {
            admin = this.baseMapper.selectById(userid);
            // Get user avatar from MinIO
            if (StrUtil.isNotEmpty(admin.getIcon())) {
                admin.setIcon(minioService.preview(admin.getIcon(), BucketEnums.ADMIN.getName()));
            }
            getCacheService().setAdmin(admin);
        }
        return admin;
    }

    /**
     * Organize DTO info
     *
     * @param umsAdmin Admin info
     * @return CurrentUserVO
     */
    private CurrentUserVO getCurrentUserVO(UmsAdmin umsAdmin) {
        CurrentUserVO vo = getCacheService().getAdminDto(umsAdmin.getId());
        if (vo == null) {
            vo = new CurrentUserVO();
            vo.setUserId(umsAdmin.getId());
            vo.setUsername(umsAdmin.getUsername());
            vo.setIcon(umsAdmin.getIcon());
            // Role list
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
        // Delete original relationships first
        LambdaQueryWrapper<UmsAdminRoleRelation> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsAdminRoleRelation::getAdminId, adminId);
        roleRelationService.remove(lambda);
        // Establish new relationships
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