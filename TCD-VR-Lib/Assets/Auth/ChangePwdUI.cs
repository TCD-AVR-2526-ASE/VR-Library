using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;


public class ChangePwdUI : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] public TMP_InputField oldPasswordInput;
    [SerializeField] public TMP_InputField newPasswordInput;
    [SerializeField] public Button confirmButton;
    [SerializeField] public TextMeshProUGUI changePwdMessageText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        if (changePwdMessageText != null)
            changePwdMessageText.text = "";

    }

    private async void OnConfirmButtonClicked()
    {
        string oldPassword = oldPasswordInput.text;
        string newPassword = newPasswordInput.text;
        Debug.Log($"[ChangePwdUI] Change Password Requested | Old: {oldPassword} | New: {newPassword}");
        if (oldPassword != null && newPassword != null)
        {

            UserVo userVo = AuthSession.CurrentUserInfo;
            if (userVo == null)
            {
                ShowMessage(changePwdMessageText, "Error: User is not logged in yet!", Color.red);
            }
            else
            {
                bool changed = await APIManager.Instance.Auth.UpdateMyPassword(userVo.username, oldPassword, newPassword);
                if (changed)
                {
                    ShowMessage(changePwdMessageText, "Password Changed!", Color.green);
                }
                else
                {
                    ShowMessage(changePwdMessageText, "Change Password Failed!", Color.red);
                }
            }

        }
        else
        {
            ShowMessage(changePwdMessageText, "Error: Input fields not assigned!", Color.red);
        }
    }

    private void ShowMessage(TextMeshProUGUI textField, string msg, Color color)
    {
        Debug.Log($"[UI Message] {msg}");
        if (textField == null) return;
        textField.text = msg;
        textField.color = color;
    }

}
