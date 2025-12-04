package com.geoscene.topo.admin.service.impl;

import com.geoscene.topo.admin.entity.UmsAdmin;
import com.geoscene.topo.admin.service.UmsAdminCacheService;
import com.geoscene.topo.admin.vo.CurrentUserVO;
import com.geoscene.topo.common.core.utils.RedisUtils;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

@Service
public class UmsAdminCacheServiceImpl implements UmsAdminCacheService {

    private final RedisUtils redisService = new RedisUtils();
    
    private final static String REDIS_DATABASE = "topo";

    private final static Long REDIS_EXPIRE = 86400L;

    private final static String REDIS_KEY_ADMIN = "ums:admin";

    private final static Long REDIS_EXPIRE_DTO = 300L;

    private final static String REDIS_KEY_ADMIN_DTO = "ums:dto";

    @Override
    public synchronized void delAdmin(Long adminId) {
        String key = REDIS_DATABASE + ":" + REDIS_KEY_ADMIN + ":" + adminId;
        redisService.del(key);
        String dtoKey = REDIS_DATABASE + ":" + REDIS_KEY_ADMIN_DTO + ":" + adminId;
        redisService.del(dtoKey);
    }

    @Override
    public UmsAdmin getAdmin(Long adminId) {
        String key = REDIS_DATABASE + ":" + REDIS_KEY_ADMIN + ":" + adminId;
        return (UmsAdmin) redisService.get(key);
    }

    @Override
    public void setAdmin(UmsAdmin admin) {
        String key = REDIS_DATABASE + ":" + REDIS_KEY_ADMIN + ":" + admin.getId();
        redisService.set(key, admin, REDIS_EXPIRE);
    }

    @Override
    public CurrentUserVO getAdminDto(Long adminId) {
        String key = REDIS_DATABASE + ":" + REDIS_KEY_ADMIN_DTO + ":" + adminId;
        return (CurrentUserVO) redisService.get(key);
    }

    @Override
    public void setAdminDto(CurrentUserVO dto) {
        String key = REDIS_DATABASE + ":" + REDIS_KEY_ADMIN_DTO + ":" + dto.getUserId();
        redisService.set(key, dto, REDIS_EXPIRE_DTO);
    }
}
