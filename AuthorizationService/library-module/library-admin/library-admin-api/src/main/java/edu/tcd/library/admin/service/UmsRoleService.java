package edu.tcd.library.admin.service;

import com.baomidou.mybatisplus.extension.service.IService;
import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.entity.UmsRole;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

/**
 * Service interface for role management and authorization
 */
public interface UmsRoleService extends IService<UmsRole> {

    /**
     * Get roles associated with a specific user
     *
     * @param adminId Unique ID of the administrator
     * @return List of UmsRole entities assigned to the user
     */
    List<UmsRole> getRoleList(Long adminId);

    /**
     * Authorize a specific role to multiple users
     *
     * @param roleId   Unique ID of the role
     * @param adminIds List of administrator IDs to be authorized
     * @return Boolean indicating if the authorization was successful
     */
    @Transactional
    Boolean userAuth(Long roleId, List<Long> adminIds);

    /**
     * Query users who have been authorized for a specific role
     *
     * @param roleId Unique ID of the role
     * @return List of authorized UmsAdmin entities
     */
    List<UmsAdmin> qryUserAuthedById(Long roleId);
}