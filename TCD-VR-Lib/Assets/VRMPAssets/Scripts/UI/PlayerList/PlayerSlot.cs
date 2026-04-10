using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using System.Collections.Generic;

namespace XRMultiplayer
{
    public class PlayerSlot : MonoBehaviour
    {
        public TMP_Text playerSlotName;
        public TMP_Text playerInitial;
        public Image playerIconImage;
        public Button kickButton;

        [Header("Mic Button")]
        public Image voiceChatFillImage;
        [SerializeField] Button m_MicButton;
        [SerializeField] Image m_PlayerVoiceIcon;
        [SerializeField] Image m_SquelchedIcon;
        [SerializeField] Sprite[] micIcons;
        XRINetworkPlayer m_Player;
        internal ulong playerID = 0;
        internal long userId = 0;

        PlayerSessionSync m_SessionSync;


        public void Setup(XRINetworkPlayer player)
        {

            //check user role is admin or librarian (who has access to the kickoff button)
            List<long> currentUserRoles = AuthSession.CurrentUserInfo.roles;
            if (currentUserRoles.Contains(1L) || currentUserRoles.Contains(3L))
            {
                kickButton.gameObject.SetActive(true);
            }
            else
            {
                kickButton.gameObject.SetActive(false);
            }


            m_Player = player;
            m_Player.onColorUpdated += UpdateColor;
            m_Player.onNameUpdated += UpdateName;
            m_Player.selfMuted.OnValueChanged += UpdateSelfMutedState;
            m_MicButton.onClick.AddListener(Squelch);
            m_Player.squelched.Subscribe(UpdateSquelchedState);
            m_SquelchedIcon.enabled = false;
            if (m_Player.IsLocalPlayer)
            {
                m_MicButton.interactable = false;
            }

            if (m_Player.selfMuted.Value)
            {
                m_PlayerVoiceIcon.sprite = micIcons[1];
            }

            if (m_Player.TryGetComponent<PlayerSessionSync>(out m_SessionSync))
            {
                userId = m_SessionSync.networkUserId.Value;
                m_SessionSync.networkUserId.OnValueChanged += OnNetworkUserIdChanged;
            }

            kickButton.onClick.AddListener(KickPlayer);
        }

        void OnNetworkUserIdChanged(long previousValue, long newValue)
        {
            userId = newValue;
        }

        async void KickPlayer()
        {
            await WebSocketManager.Instance.SendKickoffMessage(userId);
            Debug.Log($"Kicking player with ID {userId}");
        }

        void OnDestroy()
        {
            m_Player.onColorUpdated -= UpdateColor;
            m_Player.onNameUpdated -= UpdateName;
            m_Player.selfMuted.OnValueChanged -= UpdateSelfMutedState;
            m_MicButton.onClick.RemoveListener(Squelch);
            m_Player.squelched.Unsubscribe(UpdateSquelchedState);
        }

        void UpdateColor(Color newColor)
        {
            playerIconImage.color = newColor;
        }

        void UpdateName(string newName)
        {
            if (!newName.IsNullOrEmpty())
            {
                string playerName = newName;
                if (m_Player.IsLocalPlayer)
                {
                    playerName += " (You)";
                    kickButton.gameObject.SetActive(false); // you cannot kickoff yourself
                }
                else if (m_Player.IsOwnedByServer)
                {
                    playerName += " (Host)";
                }
                playerSlotName.text = playerName;
                playerInitial.text = newName.Substring(0, 1);
            }
        }

        #region Muting
        public void Squelch()
        {
            m_Player.ToggleSquelch();
        }

        void UpdateSelfMutedState(bool old, bool current)
        {
            m_PlayerVoiceIcon.sprite = micIcons[current ? 1 : 0];
        }

        void UpdateSquelchedState(bool squelched)
        {
            m_SquelchedIcon.enabled = squelched;
        }
        #endregion
    }
}
