using System;
using UnityEngine;

public static class AuthSession
{
    public static TokenVo CurrentToken { get; private set; }

    public static UserVo CurrentUserInfo { get; private set; }

    public static bool IsLoggedIn => CurrentToken != null;

    public static event Action<TokenVo> LoggedIn;
    public static event Action LoggedOut;

    public static void SetToken(TokenVo token)
    {
        CurrentToken = token;
        LoggedIn?.Invoke(token);
    }

    public static void SetUserInfo(UserVo userInfo)
    {
        if (CurrentToken == null)
        {
            Debug.LogWarning("[AuthSession] Attempted to set user info without a valid token.");
            return;
        }
        CurrentUserInfo = userInfo;
    }

    public static void Clear()
    {
        CurrentToken = null;
        LoggedOut?.Invoke();
    }

    public static string GetAuthorizationHeader()
    {
        if (CurrentToken == null) return null;
        Debug.Log($"[AuthSession] Current Token: {CurrentToken.tokenHead}{CurrentToken.token}");
        return $"{CurrentToken.tokenHead}{CurrentToken.token}";
    }
}
