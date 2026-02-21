package edu.tcd.library.admin.service.impl;

import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.service.UmsAdminCacheService;
import edu.tcd.library.admin.vo.CurrentUserVO;
import edu.tcd.library.common.core.utils.RedisUtils;
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
