using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.Events;

public class LoginUI : MonoBehaviour
{
    [Header("Tab System")]
    public GameObject loginPanel;
    public GameObject signupPanel;
    public Button loginTabButton;
    public Button signupTabButton;
    public Image loginTabUnderline;
    public Image signupTabUnderline;

    [Header("Login Input Fields")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TextMeshProUGUI loginMessageText;
    public Button loginButton;
    public Button showPasswordLogin;
    private bool passwordLoginIsShow = false;
    public Toggle rememberMeToggle;

    [Header("Signup Input Fields")]
    public TMP_InputField signupUsernameInput;
    public TMP_InputField signupPasswordInput;
    public TMP_InputField signupConfirmPasswordInput;
    public TextMeshProUGUI signupMessageText;
    public Button signupButton;
    public Button showPasswordSignup;
    private bool passwordSignupIsShow = false;
    public Button showPasswordConfirm;
    private bool passwordConfirmIsShow = false;

    [Header("Icons")]
    public Sprite showImage;
    public Sprite hideImage;

    [Header("Player / Movement")]
    public GameObject playerRoot;
    public MonoBehaviour[] movementScripts;

    [Header("Post-Login GameObjects")]
    [Tooltip("GameObjects to enable after successful login")]
    public GameObject[] gameObjectsToEnableOnLogin;
    [SerializeField] private TMP_InputField playerNameTag;
    public UnityEvent<string> onPlayerNameUpdate = new();

    [Header("Authentication Canvas")]
    public GameObject authenticationCanvas;

    // PlayerPrefs keys
    private const string PREF_USERNAME = "SavedUsername";
    private const string PREF_PASSWORD = "SavedPassword";
    private const string PREF_REMEMBER = "RememberMe";

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");
        }
        catch (Exception e)
        {
            ShowMessage(loginMessageText, $"Unity Services Init Failed: {e.Message}", Color.red);
        }

        // Setup tab buttons
        if (loginTabButton != null)
            loginTabButton.onClick.AddListener(() => ShowTab(true));

        if (signupTabButton != null)
            signupTabButton.onClick.AddListener(() => ShowTab(false));

        // Setup action buttons
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginClicked);
        else
            Debug.LogError("Login Button missing in Inspector");

        if (signupButton != null)
            signupButton.onClick.AddListener(OnSignupClicked);
        else
            Debug.LogError("Signup Button missing in Inspector");


        // Setup listeners for password display
        if (showPasswordLogin != null)
            showPasswordLogin.onClick.AddListener(() =>
            {
                passwordLoginIsShow = !passwordLoginIsShow;
                ShowOrHidePassword(loginPasswordInput, passwordLoginIsShow);
                showPasswordLogin.GetComponent<Image>().sprite =
                    passwordLoginIsShow ? showImage : hideImage;
            });

        if (showPasswordSignup != null)
            showPasswordSignup.onClick.AddListener(() =>
            {
                passwordSignupIsShow = !passwordSignupIsShow;
                ShowOrHidePassword(signupPasswordInput, passwordSignupIsShow);
                showPasswordSignup.GetComponent<Image>().sprite =
                    passwordSignupIsShow ? showImage : hideImage;
            });


        if (showPasswordConfirm != null)
            showPasswordConfirm.onClick.AddListener(() =>
            {
                passwordConfirmIsShow = !passwordConfirmIsShow;
                ShowOrHidePassword(signupConfirmPasswordInput, passwordConfirmIsShow);
                showPasswordConfirm.GetComponent<Image>().sprite =
                    passwordConfirmIsShow ? showImage : hideImage;
            });

        // Show login tab by default
        ShowTab(true);

        // Disable post-login GameObjects initially
        SetPostLoginGameObjectsEnabled(false);
        SetPlayerControlEnabled(false);

        // Load saved credentials
        LoadSavedCredentials();
    }

    private void ShowTab(bool showLogin)
    {
        if (loginPanel != null)
            loginPanel.SetActive(showLogin);

        if (signupPanel != null)
            signupPanel.SetActive(!showLogin);

        // Update tab underlines
        if (loginTabUnderline != null)
            loginTabUnderline.enabled = showLogin;

        if (signupTabUnderline != null)
            signupTabUnderline.enabled = !showLogin;

        // Clear messages
        if (loginMessageText != null)
            loginMessageText.text = showLogin ? "Please enter your credentials" : "";

        if (signupMessageText != null)
            signupMessageText.text = "";
    }


    #region Login Flow

    public async void OnLoginClicked()
    {
        // Validate input fields
        if (!ValidateLoginInputs())
            return;

        string username = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text;

        if (loginButton != null)
            loginButton.interactable = false;

        ShowMessage(loginMessageText, $"Logging in as {username}...", Color.yellow);
        Debug.Log($"Attempting login for user: {username}");

        try
        {
            TokenVo tokenData = await APIManager.Instance.Auth.Login(username, password);
            ShowMessage(loginMessageText, "Backend Auth Success. Connecting to UGS...", Color.cyan);

            // Process Unity Custom ID tokens for UGS login
            await SignInToUGSAsync(tokenData);
            ShowMessage(loginMessageText, "Success: Player Authenticated", Color.green);

            // Store token in session
            AuthSession.SetToken(tokenData);
            ShowMessage(loginMessageText, "Success: Set AuthSession Token", Color.green);

            UserVo userInfo = await APIManager.Instance.Auth.GetUserInfo();
            AuthSession.SetUserInfo(userInfo);


            await WebSocketManager.Instance.Connect(tokenData);
            ShowMessage(loginMessageText, "Success: Socket Connected", Color.green);

            // Update player display name in Unity Gaming Services
            if(playerNameTag)
                playerNameTag.text = username;
            ShowMessage(loginMessageText, "Success: Updated player name tag", Color.gray);
            onPlayerNameUpdate.Invoke(username);

            ShowMessage(loginMessageText, $"Authentication successful!", Color.cyan);

            // Remember Me handling
            if (rememberMeToggle != null && rememberMeToggle.isOn)
                SaveCredentials(username, password);
            else
                ClearSavedCredentials();

            // Enable player + gameplay objects
            SetPlayerControlEnabled(true);
            SetPostLoginGameObjectsEnabled(true);

            // Hide login canvas after short delay
            await Task.Delay(1500);
            if (authenticationCanvas != null)
                authenticationCanvas.SetActive(false);

            // Clear password field for security
            if (loginPasswordInput != null)
                loginPasswordInput.text = "";
        }
        catch (Exception ex)
        {
            Debug.LogError($"Login Process Failed: {ex}");
            ShowMessage(loginMessageText, $"Login Failed: {ex.Message}", Color.red);

            // Keep everything disabled on failure
            SetPlayerControlEnabled(false);
            SetPostLoginGameObjectsEnabled(false);
        }
        finally
        {
            if (loginButton != null)
                loginButton.interactable = true;
        }
    }

    private bool ValidateLoginInputs()
    {
        if (loginUsernameInput == null || loginPasswordInput == null)
        {
            ShowMessage(loginMessageText, "Error: Input fields not assigned!", Color.red);
            Debug.LogError("Username or Password input field is not assigned in Inspector");
            return false;
        }

        string username = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage(loginMessageText, "Please enter a username", Color.red);
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage(loginMessageText, "Please enter a password", Color.red);
            return false;
        }

        return true;
    }

    private void ShowOrHidePassword(TMP_InputField passwordInputField, bool isOn)
    {
        passwordInputField.contentType = isOn
            ? TMP_InputField.ContentType.Alphanumeric
            : TMP_InputField.ContentType.Password;

        passwordInputField.ForceLabelUpdate();
    }

    #endregion

    #region Signup Flow

    public async void OnSignupClicked()
    {
        if (!ValidateSignupInputs())
            return;

        string username = signupUsernameInput.text.Trim();
        string password = signupPasswordInput.text;

        if (signupButton != null)
            signupButton.interactable = false;

        ShowMessage(signupMessageText, $"Creating account for {username}...", Color.yellow);
        Debug.Log($"Attempting signup for user: {username}");

        try
        {
            // Returns UserVo if successful
            UserVo newUser = await APIManager.Instance.Auth.Register(username, password);

            // Gate failed registration
            if (newUser == null)
                throw new Exception("Registration failed.");

            ShowMessage(signupMessageText, "Account created successfully!", Color.green);

            // Clear signup inputs
            signupUsernameInput.text = "";
            signupPasswordInput.text = "";
            signupConfirmPasswordInput.text = "";

            // Short delay before switching tabs
            await Task.Delay(1500);

            // Pre-fill login username
            if (loginUsernameInput != null)
                loginUsernameInput.text = username;

            ShowTab(true);
            ShowMessage(loginMessageText, "Please login with your new account", Color.cyan);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Signup Process Failed: {ex}");
            ShowMessage(signupMessageText, $"Signup Failed: {ex.Message}", Color.red);
        }
        finally
        {
            if (signupButton != null)
                signupButton.interactable = true;
        }
    }

    private bool ValidateSignupInputs()
    {
        if (signupUsernameInput == null || signupPasswordInput == null || signupConfirmPasswordInput == null)
        {
            ShowMessage(signupMessageText, "Error: Input fields not assigned!", Color.red);
            Debug.LogError("Signup input fields are not assigned in Inspector");
            return false;
        }

        string username = signupUsernameInput.text.Trim();
        string password = signupPasswordInput.text;
        string confirmPassword = signupConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage(signupMessageText, "Please enter a username", Color.red);
            return false;
        }

        if (username.Length < 3)
        {
            ShowMessage(signupMessageText, "Username must be at least 3 characters", Color.red);
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage(signupMessageText, "Please enter a password", Color.red);
            return false;
        }

        if (password.Length < 6)
        {
            ShowMessage(signupMessageText, "Password must be at least 6 characters", Color.red);
            return false;
        }

        if (password != confirmPassword)
        {
            ShowMessage(signupMessageText, "Passwords do not match", Color.red);
            return false;
        }

        return true;
    }

    #endregion

    #region Unity Gaming Services

    private async Task SignInToUGSAsync(TokenVo tokenData)
    {
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Already Signed In to UGS.");
                return;
            }

            if (tokenData == null)
                throw new Exception("Missing token response from backend.");

            if (string.IsNullOrEmpty(tokenData.unityCustomIdToken))
                throw new Exception("Missing unityCustomIdToken in response.");

            // Process Unity Custom ID tokens
            AuthenticationService.Instance.ProcessAuthenticationTokens(
                tokenData.unityCustomIdToken,
                tokenData.unityCustomIdSessionToken
            );

            Debug.Log($"UGS PlayerID: {AuthenticationService.Instance.PlayerId}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception($"UGS Auth Failed: {ex.Message}");
        }
    }

    #endregion

    #region Remember Me / Credentials

    private void SaveCredentials(string username, string password)
    {
        PlayerPrefs.SetString(PREF_USERNAME, username);
        PlayerPrefs.SetString(PREF_PASSWORD, password);
        PlayerPrefs.SetInt(PREF_REMEMBER, 1);
        PlayerPrefs.Save();
        Debug.Log("Credentials saved");
    }

    private void LoadSavedCredentials()
    {
        if (PlayerPrefs.GetInt(PREF_REMEMBER, 0) == 1)
        {
            string savedUsername = PlayerPrefs.GetString(PREF_USERNAME, "");
            string savedPassword = PlayerPrefs.GetString(PREF_PASSWORD, "");

            if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
            {
                if (loginUsernameInput != null)
                    loginUsernameInput.text = savedUsername;

                if (loginPasswordInput != null)
                    loginPasswordInput.text = savedPassword;

                if (rememberMeToggle != null)
                    rememberMeToggle.isOn = true;

                Debug.Log("Loaded saved credentials");
            }
        }
    }

    private void ClearSavedCredentials()
    {
        PlayerPrefs.DeleteKey(PREF_USERNAME);
        PlayerPrefs.DeleteKey(PREF_PASSWORD);
        PlayerPrefs.DeleteKey(PREF_REMEMBER);
        PlayerPrefs.Save();
        Debug.Log("Cleared saved credentials");
    }

    #endregion

    #region Helper Methods

    private void SetPlayerControlEnabled(bool enabled)
    {
        if (movementScripts != null)
        {
            foreach (var script in movementScripts)
            {
                if (script != null)
                    script.enabled = enabled;
            }
        }
    }

    private void SetPostLoginGameObjectsEnabled(bool enabled)
    {
        if (gameObjectsToEnableOnLogin != null)
        {
            foreach (var go in gameObjectsToEnableOnLogin)
            {
                if (go != null)
                {
                    go.SetActive(enabled);
                    Debug.Log($"{(enabled ? "Enabled" : "Disabled")} GameObject: {go.name}");
                }
            }
        }
    }

    private void ShowMessage(TextMeshProUGUI textField, string msg, Color color)
    {
        Debug.Log($"[UI Message] {msg}");
        if (textField == null) return;
        textField.text = msg;
        textField.color = color;
    }

    #endregion
}

// Extension method for async UnityWebRequest
public static class UnityWebRequestExtension
{
    public static System.Runtime.CompilerServices.TaskAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
    {
        var tcs = new System.Threading.Tasks.TaskCompletionSource<UnityWebRequest>();
        reqOp.completed += asyncOp => tcs.SetResult(reqOp.webRequest);
        return tcs.Task.GetAwaiter();
    }
}

[Serializable]
public class User
{
    public string username;
    public string password;
}