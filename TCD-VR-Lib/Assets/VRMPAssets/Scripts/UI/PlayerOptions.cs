using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using TMPro;
using System;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.Android;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

namespace XRMultiplayer
{
    [DefaultExecutionOrder(100)]
    public class PlayerOptions : MonoBehaviour
    {
        [SerializeField] InputActionReference m_ToggleMenuAction;
        [SerializeField] AudioMixer m_Mixer;

        [Header("Panels")]
        [SerializeField] GameObject m_HostRoomPanel;
        [SerializeField] GameObject m_ClientRoomPanel;
        [SerializeField] GameObject[] m_OfflineWarningPanels;
        [SerializeField] GameObject[] m_OnlinePanels;
        [SerializeField] GameObject[] m_Panels;
        [SerializeField] Toggle[] m_PanelToggles;

        [Header("Text Components")]
        [SerializeField] TMP_Text m_SnapTurnText;
        [SerializeField] TMP_Text m_RoomCodeText;
        [SerializeField] TMP_Text m_TimeText;
        [SerializeField] TMP_Text[] m_RoomNameText;
        [SerializeField] TMP_InputField m_RoomNameInputField;
        [SerializeField] TMP_Text[] m_PlayerCountText;

        [Header("Voice Chat")]
        [SerializeField] Button m_MicPermsButton;
        [SerializeField] Slider m_InputVolumeSlider;
        [SerializeField] Slider m_OutputVolumeSlider;
        [SerializeField] TMP_InputField m_MainVolumeInputField;
        [SerializeField] TMP_InputField m_InputVolumeInputField;
        [SerializeField] TMP_InputField m_OutputVolumeInputField;
        [SerializeField] Image m_LocalPlayerAudioVolume;
        [SerializeField] Image m_MutedIcon;
        [SerializeField] Image m_MicOnIcon;
        [SerializeField] TMP_Text m_VoiceChatStatus;

        [Header("Player Options")]
        [SerializeField] Vector2 m_MinMaxMoveSpeed = new Vector2(2.0f, 10.0f);
        [SerializeField] Vector2 m_MinMaxTurnAmount = new Vector2(15.0f, 180.0f);
        [SerializeField] float m_SnapTurnUpdateAmount = 15.0f;
        [SerializeField] Slider m_MoveSpeedSlider;
        [SerializeField] TMP_InputField m_MoveSpeedInputField;

        VoiceChatManager m_VoiceChatManager;
        DynamicMoveProvider m_MoveProvider;
        SnapTurnProvider m_TurnProvider;
        UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController m_TunnelingVignetteController;

        PermissionCallbacks permCallbacks;

        RoomData room;

        private void Awake()
        {
            m_VoiceChatManager = FindFirstObjectByType<VoiceChatManager>();
            ResolveLocomotionProviders();

            XRINetworkGameManager.Connected.Subscribe(ConnectOnline);
            XRINetworkGameManager.ConnectedRoomName.Subscribe(UpdateRoomName);
            XRINetworkGameManager.Instance.OnSessionOwnerPromoted += UpdateHostVisuals;

            ResolveFallbackSliders();
            m_VoiceChatManager.selfMuted.Subscribe(MutedChanged);
            m_VoiceChatManager.connectionStatus.Subscribe(UpdateVoiceChatStatus);
            m_InputVolumeSlider?.onValueChanged.AddListener(SetInputVolume);
            m_OutputVolumeSlider?.onValueChanged.AddListener(SetOutputVolume);
            m_MoveSpeedSlider?.onValueChanged.AddListener(SetMoveSpeed);
            ResolveFallbackInputFields();
            BindFallbackInputFields();

            ConnectOnline(false);

            if (m_ToggleMenuAction != null)
                m_ToggleMenuAction.action.performed += ctx => ToggleMenu();
            else
                Utils.Log("No toggle menu action assigned to OptionsPanel", 1);

            permCallbacks = new PermissionCallbacks();
            permCallbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            permCallbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        }

        private void UpdateHostVisuals(ulong newHostId)
        {
            m_HostRoomPanel.SetActive(NetworkManager.Singleton.LocalClientId == newHostId);
            m_ClientRoomPanel.SetActive(NetworkManager.Singleton.LocalClientId != newHostId);
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Utils.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
            m_MicPermsButton.gameObject.SetActive(false);
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            Utils.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
        }

        void OnEnable()
        {
            TogglePanel(0);
            SyncFallbackInputFields();

            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                m_MicPermsButton.gameObject.SetActive(true);
            }
            else
            {
                m_MicPermsButton.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            XRINetworkGameManager.Connected.Unsubscribe(ConnectOnline);
            XRINetworkGameManager.ConnectedRoomName.Unsubscribe(UpdateRoomName);
            XRINetworkGameManager.Instance.OnSessionOwnerPromoted += UpdateHostVisuals;
            m_VoiceChatManager.selfMuted.Unsubscribe(MutedChanged);

            m_VoiceChatManager.connectionStatus.Unsubscribe(UpdateVoiceChatStatus);
            m_InputVolumeSlider?.onValueChanged.RemoveListener(SetInputVolume);
            m_OutputVolumeSlider?.onValueChanged.RemoveListener(SetOutputVolume);
            m_MoveSpeedSlider?.onValueChanged.RemoveListener(SetMoveSpeed);
            UnbindFallbackInputFields();
        }

        private void Update()
        {
            //m_TimeText.text = $"{DateTime.Now:h:mm}<size=4><voffset=1em>{DateTime.Now:tt}</size></voffset>";
            float t = WorldTime.Instance.GetTime();
            int h = Mathf.FloorToInt(t);
            int m = Mathf.FloorToInt((t - h) * 60);
            m_TimeText.text = $"{h:00}:{m:00}";
            if (XRINetworkGameManager.Connected.Value)
            {
                m_LocalPlayerAudioVolume.fillAmount = XRINetworkPlayer.LocalPlayer.playerVoiceAmp;
            }
            else
            {
                m_LocalPlayerAudioVolume.fillAmount = OfflinePlayerAvatar.voiceAmp.Value;
            }
        }

        void ConnectOnline(bool connected)
        {
            foreach (var go in m_OfflineWarningPanels)
            {
                go.SetActive(!connected);
            }

            foreach (var go in m_OnlinePanels)
            {
                go.SetActive(connected);
            }

            if (connected)
            {
                m_HostRoomPanel.SetActive(XRINetworkPlayer.LocalPlayer.IsSessionOwner);
                m_ClientRoomPanel.SetActive(!XRINetworkPlayer.LocalPlayer.IsSessionOwner);
                UpdateRoomName(XRINetworkGameManager.ConnectedRoomName.Value);
                m_MutedIcon.enabled = false;
                m_MicOnIcon.enabled = true;
                m_LocalPlayerAudioVolume.enabled = true;
            }
            else
            {
                ToggleMenu(false);
            }
        }

        public void TogglePanel(int panelID)
        {
            for (int i = 0; i < m_Panels.Length; i++)
            {
                m_PanelToggles[i].SetIsOnWithoutNotify(panelID == i);
                m_Panels[i].SetActive(i == panelID);
            }
        }

        /// <summary>
        /// Toggles the menu on or off.
        /// </summary>
        /// <param name="overrideToggle"></param>
        /// <param name="overrideValue"></param>
        public void ToggleMenu(bool overrideToggle = false, bool overrideValue = false)
        {
            if (overrideToggle)
            {
                gameObject.SetActive(overrideValue);
            }
            else
            {
                ToggleMenu();
            }
            TogglePanel(0);
        }

        public void ToggleMenu()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void LogOut()
        {
            XRINetworkGameManager.Instance.Disconnect();
        }

        public void DeleteCachedRoom()
        {
            if (XRINetworkGameManager.ConnectedRoomCode == null)
            {
                Debug.LogWarning("No room code. Nothing to delete.");
                return;
            }

            if (RoomCacheProvider.CacheService == null)
            {
                Debug.LogError("CacheService is NULL. Did it initialize?");
                return;
            }

            _ = RoomCacheProvider.CacheService.Upsert(new RoomData
            {
                GUID = XRINetworkGameManager.ConnectedRoomCode,
                RoomName = XRINetworkGameManager.ConnectedRoomName?.Value ?? "Unknown Room",
                SceneName = "",
                MaxPlayers = 0,
                StatusEnum = RoomStatus.Closed,
                Endpoint = "",
                SessionID = "",
                JoinCode = XRINetworkGameManager.ConnectedRoomCode
            });

            Debug.Log($"Deleted cached room {XRINetworkGameManager.ConnectedRoomCode}");
            LogOut();
        }

        public void QuickJoin()
        {
            XRINetworkGameManager.Instance.QuickJoinLobby();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void UpdateVoiceChatStatus(string statusMessage)
        {
            m_VoiceChatStatus.text = $"<b>Voice Chat:</b> {statusMessage}";
        }
        public void SetVolumeLevel(float sliderValue)
        {
            m_Mixer.SetFloat("MainVolume", Mathf.Log10(sliderValue) * 20);
        }
        public void SetInputVolume(float volume)
        {
            float perc = Mathf.Lerp(-10, 10, volume);
            m_VoiceChatManager.SetInputVolume(perc);
        }

        public void SetOutputVolume(float volume)
        {
            float perc = Mathf.Lerp(-10, 10, volume);
            m_VoiceChatManager.SetOutputVolume(perc);
        }

        public void ToggleMute()
        {
            m_VoiceChatManager.ToggleSelfMute();
        }

        void MutedChanged(bool muted)
        {
            m_MutedIcon.enabled = muted;
            m_MicOnIcon.enabled = !muted;
            m_LocalPlayerAudioVolume.enabled = !muted;
            PlayerHudNotification.Instance.ShowText($"<b>Microphone: {(muted ? "OFF" : "ON")}</b>");
        }

        // Room Options
        public void UpdateRoomPrivacy(bool toggle)
        {
            XRINetworkGameManager.Instance.sessionManager.UpdateRoomPrivacy(toggle);
        }

        public void SubmitNewRoomName(string text)
        {
            XRINetworkGameManager.Instance.sessionManager.UpdateLobbyName(text);
        }

        void UpdateRoomName(string newValue)
        {
            m_RoomCodeText.text = $"Room Code: {XRINetworkGameManager.ConnectedRoomCode}";
            foreach (var t in m_RoomNameText)
            {
                t.text = XRINetworkGameManager.ConnectedRoomName.Value;
            }
            m_RoomNameInputField.text = XRINetworkGameManager.ConnectedRoomName.Value;
        }

        // Player Options
        public void SetHandOrientation(bool toggle)
        {
            if (toggle)
            {
                m_MoveProvider.leftHandMovementDirection = DynamicMoveProvider.MovementDirection.HandRelative;
            }
        }
        public void SetHeadOrientation(bool toggle)
        {
            if (toggle)
            {
                m_MoveProvider.leftHandMovementDirection = DynamicMoveProvider.MovementDirection.HeadRelative;
            }
        }
        public void SetMoveSpeed(float speedPercent)
        {
            if (m_MoveProvider == null)
                ResolveLocomotionProviders();

            if (m_MoveProvider == null)
                return;

            m_MoveProvider.moveSpeed = Mathf.Lerp(m_MinMaxMoveSpeed.x, m_MinMaxMoveSpeed.y, speedPercent);

            if (m_MoveSpeedInputField != null)
                m_MoveSpeedInputField.SetTextWithoutNotify(m_MoveProvider.moveSpeed.ToString("0.0"));
        }

        public void UpdateSnapTurn(int dir)
        {
            float newTurnAmount = Mathf.Clamp(m_TurnProvider.turnAmount + (m_SnapTurnUpdateAmount * dir), m_MinMaxTurnAmount.x, m_MinMaxTurnAmount.y);
            m_TurnProvider.turnAmount = newTurnAmount;
            m_SnapTurnText.text = $"{newTurnAmount}°";
        }

        public void ToggleTunnelingVignette(bool toggle)
        {
            m_TunnelingVignetteController.gameObject.SetActive(toggle);
        }

        public void ToggleFlight(bool toggle)
        {
            var gravityProvider = m_MoveProvider.GetComponent<GravityProvider>();
            if (gravityProvider != null)
            {
                gravityProvider.enabled = !toggle;
            }
            m_MoveProvider.enableFly = toggle;
        }

        void ResolveFallbackInputFields()
        {
            m_MainVolumeInputField ??= FindInputFieldForRow("Main Volume (Slider)");
            m_InputVolumeInputField ??= FindInputFieldForRow("Voice Input (Slider)");
            m_OutputVolumeInputField ??= FindInputFieldForRow("Voice Output (Slider)");
            m_MoveSpeedInputField ??= FindInputFieldForRow("Movement Speed (Slider)");
        }

        void ResolveFallbackSliders()
        {
            m_InputVolumeSlider ??= FindSliderForRow("Voice Input (Slider)");
            m_OutputVolumeSlider ??= FindSliderForRow("Voice Output (Slider)");
            m_MoveSpeedSlider ??= FindSliderForRow("Movement Speed (Slider)");
        }

        void BindFallbackInputFields()
        {
            m_MainVolumeInputField?.onEndEdit.AddListener(SetMainVolumeFromInput);
            m_InputVolumeInputField?.onEndEdit.AddListener(SetInputVolumeFromInput);
            m_OutputVolumeInputField?.onEndEdit.AddListener(SetOutputVolumeFromInput);
            m_MoveSpeedInputField?.onEndEdit.AddListener(SetMoveSpeedFromInput);
        }

        void UnbindFallbackInputFields()
        {
            m_MainVolumeInputField?.onEndEdit.RemoveListener(SetMainVolumeFromInput);
            m_InputVolumeInputField?.onEndEdit.RemoveListener(SetInputVolumeFromInput);
            m_OutputVolumeInputField?.onEndEdit.RemoveListener(SetOutputVolumeFromInput);
            m_MoveSpeedInputField?.onEndEdit.RemoveListener(SetMoveSpeedFromInput);
        }

        void SyncFallbackInputFields()
        {
            if (m_MainVolumeInputField != null && m_Mixer != null && m_Mixer.GetFloat("MainVolume", out float volumeDb))
            {
                float normalizedVolume = volumeDb <= -80f ? 0f : Mathf.Clamp01(Mathf.Pow(10f, volumeDb / 20f));
                m_MainVolumeInputField.SetTextWithoutNotify(Mathf.RoundToInt(normalizedVolume * 100f).ToString());
            }

            if (m_MoveSpeedInputField != null && m_MoveProvider != null)
            {
                m_MoveSpeedInputField.SetTextWithoutNotify(m_MoveProvider.moveSpeed.ToString("0.0"));
            }

            if (m_MoveSpeedSlider != null && m_MoveProvider != null)
            {
                float speedPercent = Mathf.InverseLerp(m_MinMaxMoveSpeed.x, m_MinMaxMoveSpeed.y, m_MoveProvider.moveSpeed);
                m_MoveSpeedSlider.SetValueWithoutNotify(speedPercent);
            }

            m_InputVolumeInputField?.SetTextWithoutNotify("50");
            m_OutputVolumeInputField?.SetTextWithoutNotify("50");
        }

        TMP_InputField FindInputFieldForRow(string rowName)
        {
            Transform row = FindChildRecursive(transform, rowName);
            return row != null ? row.GetComponentInChildren<TMP_InputField>(true) : null;
        }

        Slider FindSliderForRow(string rowName)
        {
            Transform row = FindChildRecursive(transform, rowName);
            return row != null ? row.GetComponentInChildren<Slider>(true) : null;
        }

        Transform FindChildRecursive(Transform parent, string targetName)
        {
            if (parent == null)
                return null;

            if (parent.name == targetName)
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindChildRecursive(parent.GetChild(i), targetName);
                if (result != null)
                    return result;
            }

            return null;
        }

        void SetMainVolumeFromInput(string value)
        {
            if (!TryParseFloat(value, out float parsedValue))
                return;

            float normalizedValue = parsedValue > 1f ? parsedValue / 100f : parsedValue;
            SetVolumeLevel(Mathf.Clamp(normalizedValue, 0.0001f, 1f));
        }

        void SetInputVolumeFromInput(string value)
        {
            if (!TryParseFloat(value, out float parsedValue))
                return;

            if (parsedValue < -10f || parsedValue > 10f)
                parsedValue = Mathf.Lerp(-10f, 10f, Mathf.Clamp01(parsedValue / 100f));

            m_VoiceChatManager.SetInputVolume(Mathf.Clamp(parsedValue, -10f, 10f));
        }

        void SetOutputVolumeFromInput(string value)
        {
            if (!TryParseFloat(value, out float parsedValue))
                return;

            if (parsedValue < -10f || parsedValue > 10f)
                parsedValue = Mathf.Lerp(-10f, 10f, Mathf.Clamp01(parsedValue / 100f));

            m_VoiceChatManager.SetOutputVolume(Mathf.Clamp(parsedValue, -10f, 10f));
        }

        void SetMoveSpeedFromInput(string value)
        {
            if (!TryParseFloat(value, out float parsedValue))
                return;

            float speedPercent = parsedValue <= 1f
                ? Mathf.Clamp01(parsedValue)
                : Mathf.InverseLerp(m_MinMaxMoveSpeed.x, m_MinMaxMoveSpeed.y, parsedValue);

            SetMoveSpeed(speedPercent);
        }

        bool TryParseFloat(string value, out float parsedValue)
        {
            return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out parsedValue)
                || float.TryParse(value, out parsedValue);
        }

        void ResolveLocomotionProviders()
        {
            Transform root = transform.root;
            m_MoveProvider = root != null
                ? root.GetComponentInChildren<DynamicMoveProvider>(true)
                : FindFirstObjectByType<DynamicMoveProvider>();
            m_TurnProvider = root != null
                ? root.GetComponentInChildren<SnapTurnProvider>(true)
                : FindFirstObjectByType<SnapTurnProvider>();
            m_TunnelingVignetteController = root != null
                ? root.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController>(true)
                : FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort.TunnelingVignetteController>();
        }
    }
}
