package edu.tcd.library.admin.service;

import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.vo.CurrentUserVO;

/**
 * Service interface for administrative user cache operations
 */
public interface UmsAdminCacheService {

    /**
     * Deletes administrative user cache and detailed user info cache
     * * @param adminId The unique ID of the administrator
     */
    void delAdmin(Long adminId);

    /**
     * Retrieves cached administrative user information
     * * @param adminId The unique ID of the administrator
     * @return The cached UmsAdmin entity
     */
    UmsAdmin getAdmin(Long adminId);

    /**
     * Caches administrative user information
     *
     * @param admin Administrative user information entity
     */
    void setAdmin(UmsAdmin admin);

    /**
     * Retrieves detailed user information from the cache
     *
     * @param adminId The unique ID of the user
     * @return Detailed user information (VO) from the cache
     */
    CurrentUserVO getAdminDto(Long adminId);

    /**
     * Stores detailed user information in the cache
     *
     * @param dto Detailed user information (VO)
     */
    void setAdminDto(CurrentUserVO dto);
}