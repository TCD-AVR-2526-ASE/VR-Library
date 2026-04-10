using TMPro;
using UnityEngine;
using XRMultiplayer;

public class EnterSubmitToMessageBoard : MonoBehaviour
{
    [Header("Assign in Inspector (recommended)")]
    [SerializeField] private TMP_InputField inputField;          // XR InputField (TMP)
    [SerializeField] private NetworkMessageBoard messageBoard;   // Message Board UI 上的脚本

    void Awake()
    {
        if (!inputField) inputField = GetComponent<TMP_InputField>();
        // 如果你没拖引用，也自动找一个（场景里一般就一个）
        if (!messageBoard) messageBoard = FindObjectOfType<NetworkMessageBoard>(true);
    }

    void OnEnable()
    {
        if (inputField != null)
            inputField.onSubmit.AddListener(OnSubmit);
    }

    void OnDisable()
    {
        if (inputField != null)
            inputField.onSubmit.RemoveListener(OnSubmit);
    }

    private void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            inputField.ActivateInputField();
            return;
        }

        // 关键：走原有网络消息流程（会创建 Message Prefab(Clone) 并同步）
        messageBoard.SubmitTextLocal(text);

        // 清空 + 重新聚焦（VR 必须）
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }
}
