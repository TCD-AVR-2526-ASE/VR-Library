package edu.tcd.library.admin.service;

import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import com.baomidou.mybatisplus.extension.service.IService;
import edu.tcd.library.admin.dto.UmsAdminDTO;
import edu.tcd.library.admin.dto.UpdateAdminPasswordDTO;
import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.entity.UmsAdminExtend;
import edu.tcd.library.admin.entity.UmsRole;
import edu.tcd.library.admin.vo.CurrentUserVO;
import edu.tcd.library.common.core.api.CommonResult;
import edu.tcd.library.common.core.domain.UserDto;
import org.springframework.transaction.annotation.Transactional;
import org.springframework.web.multipart.MultipartFile;

import java.util.List;

/**
 * Service interface for administrative user management
 */
public interface UmsAdminService extends IService<UmsAdmin> {

    /**
     * Get administrative user by username
     *
     * @param username The username to search for
     * @return The UmsAdmin entity
     */
    UmsAdmin getAdminByUsername(String username);

    /**
     * Load user data for authentication by username
     *
     * @param username The username to load
     * @return UserDto containing security information
     */
    UserDto loadUserByUsername(String username);

    /**
     * Load user data for authentication by user ID
     *
     * @param userid The unique user ID
     * @return UserDto containing security information
     */
    UserDto loadUserByUserId(Long userid);

    /**
     * User registration
     *
     * @param icon  Avatar file stream
     * @param param User registration information
     * @return The registered UmsAdmin entity
     */
    UmsAdmin register(MultipartFile icon, UmsAdminDTO param);

    /**
     * Update user information
     *
     * @param id    User ID
     * @param icon  Avatar file stream
     * @param param Updated user information
     * @return Boolean indicating success or failure
     */
    boolean update(Long id, MultipartFile icon, UmsAdminDTO param);

    /**
     * Update a specific user's password
     *
     * @param param Password update parameters
     * @return CommonResult indicating the operation status
     */
    CommonResult<Boolean> updatePassword(UpdateAdminPasswordDTO param);

    /**
     * Update the currently logged-in user's own password
     *
     * @param param Password update parameters
     * @return CommonResult indicating the operation status
     */
    CommonResult<Boolean> updateMyPassword(UpdateAdminPasswordDTO param);

    /**
     * Get information for the currently logged-in user
     *
     * @return CurrentUserVO containing user details
     */
    CurrentUserVO getAdminInfo();

    /**
     * Get user information by ID
     *
     * @param id User ID
     * @return CurrentUserVO containing user details
     */
    CurrentUserVO getAdminInfoById(Long id);

    /**
     * Query user list with pagination and filters
     *
     * @param deptId   Department ID
     * @param keyword  General keyword for search
     * @param nickName Search by nickname
     * @param userName Search by username
     * @param page     Pagination criteria
     * @return A paged list of extended admin information
     */
    Page<UmsAdminExtend> selectPage(Long deptId, String keyword, String nickName,
                                    String userName, Page<UmsAdminExtend> page);

    /**
     * Update user-role relationships
     *
     * @param adminId The ID of the administrator
     * @param roleIds List of role IDs to assign
     * @return Boolean indicating success
     */
    @Transactional
    boolean updateRole(Long adminId, List<Long> roleIds);

    /**
     * Get roles associated with a specific user
     *
     * @param adminId The ID of the administrator
     * @return List of assigned UmsRole entities
     */
    List<UmsRole> getRoleList(Long adminId);

}