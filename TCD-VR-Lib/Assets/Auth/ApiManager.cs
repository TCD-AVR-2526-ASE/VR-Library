using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// ==========================================
// Core Response and DTO Definitions
// ==========================================

/// <summary>
/// Standard API response wrapper.
/// </summary>
[Serializable]
public class ApiResponse<T>
{
    public long code;
    public string message;
    public T data;
}

/// <summary>
/// Data Transfer Object for login requests.
/// </summary>
[Serializable]
public class LoginDto
{
    public string username;
    public string password;
}

/// <summary>
/// Data Transfer Object for user registration.
/// </summary>
[Serializable]
public class RegisterDto
{
    public string username;
    public string password;
}

/// <summary>
/// Value Object containing authentication tokens.
/// </summary>
[Serializable]
public class TokenVo
{
    public string token;
    public string tokenHead;
    public long expiresIn;
    public string unityCustomIdToken;
    public string unityCustomIdSessionToken;
}

/// <summary>
/// Value Object representing user information.
/// </summary>
[Serializable]
public class UserVo
{
    public long userId;
    public string username;
    public string icon;
    public List<long> roles;
}

/// <summary>
/// Data Transfer Object for role management.
/// </summary>
[Serializable]
public class RoleDto
{
    public long id;
    public string name;
    public string description;
    public int status;
}

/// <summary>
/// Generic wrapper for paginated results.
/// </summary>
[Serializable]
public class PageResult<T>
{
    public int pageNum;
    public int pageSize;
    public int totalPage;
    public long total;
    public List<T> list;
}

/// <summary>
/// Data Transfer Object for book entities.
/// </summary>
[Serializable]
public class BookDto
{
    public int id;
    public string title;
    public string authors;
    public string subjects;
    public string bookshelves;
}

/// <summary>
/// Data Transfer Object for updating user password.
/// </summary>
[Serializable]
public class UpdateUserPasswordDto
{
    public string username;
    public string oldPassword;
    public string newPassword;
}

// ==========================================
// Base Service 
// ==========================================

/// <summary>
/// Abstract base class for all domain services.
/// </summary>
public abstract class BaseService
{
    protected RequestUtils Network => RequestUtils.Instance;

    /// <summary>
    /// Helper to append query parameters to the URL.
    /// </summary>
    protected string BuildUrl(string endpoint, Dictionary<string, string> queryParams)
    {
        if (queryParams == null || queryParams.Count == 0) return endpoint;

        var sb = new StringBuilder(endpoint);
        sb.Append("?");
        bool first = true;
        foreach (var kvp in queryParams)
        {
            if (!first) sb.Append("&");
            sb.Append($"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}");
            first = false;
        }
        return sb.ToString();
    }
}

// ==========================================
// Auth Domain
// ==========================================

/// <summary>
/// Handles authentication and user registration logic.
/// </summary>
public class AuthService : BaseService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    public async Task<TokenVo> Login(string username, string password)
    {
        var body = new LoginDto { username = username, password = password };
        var response = await Network.SendRequestAsync<ApiResponse<TokenVo>, LoginDto>(
            "auth/login",
            RequestUtils.RequestType.POST,
            body
        );

        if (response.code == 200) return response.data;
        throw new Exception($"Login failed: {response.message}");
    }

    /// <summary>
    /// Fetches the profile of the currently authenticated user.
    /// </summary>
    public async Task<UserVo> GetUserInfo()
    {
        var response = await Network.SendRequestAsync<ApiResponse<UserVo>>(
            "auth/info",
            RequestUtils.RequestType.GET
        );

        return response.code == 200 ? response.data : null;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public async Task<UserVo> Register(string username, string password)
    {
        var body = new RegisterDto { username = username, password = password };
        var response = await Network.SendRequestAsync<ApiResponse<UserVo>, RegisterDto>(
            "auth/register",
            RequestUtils.RequestType.POST,
            body
        );

        return response.code == 200 ? response.data : null;
    }

    /// <summary>
    /// Updates the password for a user.
    /// </summary>
    public async Task<bool> UpdateMyPassword(string username, string oldPassword, string newPassword)
    {
        var body = new UpdateUserPasswordDto
        {
            username = username,
            oldPassword = oldPassword,
            newPassword = newPassword
        };

        var response = await Network.SendRequestAsync<ApiResponse<bool>, UpdateUserPasswordDto>(
            "auth/updateMyPassword",
            RequestUtils.RequestType.POST,
            body
        );
        
        return response != null && response.code == 200 && response.data;
    }
}

// ==========================================
// User Domain
// ==========================================

/// <summary>
/// Manages user lists and administrative user status.
/// </summary>
public class UserService : BaseService
{
    /// <summary>
    /// Retrieves a paginated list of users, optionally filtered by keyword.
    /// </summary>
    public async Task<PageResult<UserVo>> GetUserList(int pageNum, int pageSize, string keyword = "")
    {
        var queryParams = new Dictionary<string, string>
        {
            { "pageNum", pageNum.ToString() },
            { "pageSize", pageSize.ToString() }
        };
        if (!string.IsNullOrEmpty(keyword)) queryParams.Add("keyword", keyword);

        string url = BuildUrl("ums/admin/list", queryParams);

        var response = await Network.SendRequestAsync<ApiResponse<PageResult<UserVo>>>(
            url,
            RequestUtils.RequestType.GET
        );

        return response.code == 200 ? response.data : null;
    }

    /// <summary>
    /// Updates the operational status of an administrative user.
    /// </summary>
    public async Task<bool> UpdateStatus(long adminId, int status)
    {
        string url = $"ums/admin/updateStatus/{adminId}?status={status}";
        var response = await Network.SendRequestAsync<ApiResponse<bool>>(
            url,
            RequestUtils.RequestType.POST
        );

        return response.code == 200;
    }


}

// ==========================================
// Role Domain
// ==========================================

/// <summary>
/// Manages user roles and permissions.
/// </summary>
public class RoleService : BaseService
{
    /// <summary>
    /// Creates a new role in the system.
    /// </summary>
    public async Task<bool> CreateRole(string name, string description)
    {
        var body = new RoleDto
        {
            name = name,
            description = description,
            status = 1
        };

        var response = await Network.SendRequestAsync<ApiResponse<bool>, RoleDto>(
            "ums/role/create",
            RequestUtils.RequestType.POST,
            body
        );

        return response.code == 200;
    }

    /// <summary>
    /// Retrieves all available roles.
    /// </summary>
    public async Task<List<RoleDto>> GetAllRoles()
    {
        var response = await Network.SendRequestAsync<ApiResponse<List<RoleDto>>>(
            "ums/role/listAll",
            RequestUtils.RequestType.GET
        );

        return response.code == 200 ? response.data : new List<RoleDto>();
    }
}

// ==========================================
// Room Domain
// ==========================================

/// <summary>
/// Manages virtual room sessions and metadata.
/// </summary>
public class RoomService : BaseService
{
    /// <summary>
    /// Adds multiple rooms in a single batch request.
    /// </summary>
    public async Task<bool> AddRooms(List<RoomData> rooms)
    {
        var response = await Network.SendRequestAsync<ApiResponse<bool>, List<RoomData>>(
            "room/addRooms",
            RequestUtils.RequestType.POST,
            rooms
        );
        Debug.Log($"[RoomService] AddRooms response: code={response?.code}, message={response?.message}");

        return response != null && response.code == 200 && response.data;
    }

    /// <summary>
    /// Lists all rooms from the server.
    /// </summary>
    public async Task<List<RoomData>> GetAllRooms()
    {
        var response = await Network.SendRequestAsync<ApiResponse<List<RoomData>>>(
            "room/listAll",
            RequestUtils.RequestType.GET
        );

        return response != null && response.code == 200 ? response.data : new List<RoomData>();
    }
}

// ==========================================
// Book Domain
// ==========================================

/// <summary>
/// Manages book library and search operations.
/// </summary>
public class BookService : BaseService
{
    /// <summary>
    /// Searches for books using various optional filters.
    /// </summary>
    public async Task<PageResult<BookDto>> SearchBooks(
        string keyword = null,
        string author = null,
        string subject = null,
        string bookshelve = null,
        int pageNum = 1,
        int pageSize = 10)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "pageNum", pageNum.ToString() },
            { "pageSize", pageSize.ToString() }
        };

        if (!string.IsNullOrEmpty(keyword)) queryParams.Add("keyword", keyword);
        if (!string.IsNullOrEmpty(author)) queryParams.Add("author", author);
        if (!string.IsNullOrEmpty(subject)) queryParams.Add("subject", subject);
        if (!string.IsNullOrEmpty(bookshelve)) queryParams.Add("bookshelve", bookshelve);

        string url = BuildUrl("book/page", queryParams);

        var response = await Network.SendRequestAsync<ApiResponse<PageResult<BookDto>>>(
            url,
            RequestUtils.RequestType.GET
        );

        return response != null && response.code == 200 ? response.data : null;
    }
}

// ==========================================
// API Manager Central Hub
// ==========================================

/// <summary>
/// Singleton manager that acts as the primary access point for all backend services.
/// </summary>
public class APIManager : MonoBehaviour
{
    private static APIManager _instance;

    /// <summary>
    /// Static instance for global access. Auto-generates if missing.
    /// </summary>
    public static APIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<APIManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("_GameSystem_Auto");
                    _instance = go.AddComponent<APIManager>();
                    Debug.Log("[APIManager] Auto-generating instance.");
                }
            }
            return _instance;
        }
    }

    public AuthService Auth { get; private set; }
    public UserService User { get; private set; }
    public RoleService Role { get; private set; }
    public RoomService Room { get; private set; }

    public BookService Book { get; private set; }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitServices();
    }

    /// <summary>
    /// Initializes all sub-services and ensures network utilities are present.
    /// </summary>
    private void InitServices()
    {
        if (Auth == null) Auth = new AuthService();
        if (User == null) User = new UserService();
        if (Role == null) Role = new RoleService();
        if (Room == null) Room = new RoomService();
        if (Book == null) Book = new BookService();

        if (GetComponent<RequestUtils>() == null)
        {
            gameObject.AddComponent<RequestUtils>();
        }
    }
}