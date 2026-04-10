using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace XRMultiplayer
{
    public class LobbyListSlotUI : MonoBehaviour
    {
        [SerializeField] TMP_Text m_RoomNameText;
        [SerializeField] TMP_Text m_PlayerCountText;
        [SerializeField] Button m_JoinButton;
        [SerializeField] GameObject m_FullImage;
        [SerializeField] TMP_Text m_StatusText;

        LobbyUI m_LobbyListUI;
        ISessionInfo m_Session;

        bool m_NonJoinable = false;

        RoomData room;

        public void CreateSessionUI(ISessionInfo session, LobbyUI lobbyListUI)
        {
            m_NonJoinable = false;
            m_Session = session;
            m_LobbyListUI = lobbyListUI;
            m_JoinButton.onClick.RemoveAllListeners();
            m_JoinButton.onClick.AddListener(JoinRoom);
            m_RoomNameText.text = session.Name;
            m_PlayerCountText.text = $"{session.MaxPlayers - session.AvailableSlots}/{session.MaxPlayers}";

            m_FullImage.SetActive(false);
        }

        public void CreateNonJoinableLobbyUI(ISessionInfo session, LobbyUI lobbyListUI, string statusText)
        {
            m_NonJoinable = true;
            m_JoinButton.interactable = false;
            m_Session = session;
            m_LobbyListUI = lobbyListUI;
            m_RoomNameText.text = session.Name;
            m_StatusText.text = statusText;
            m_FullImage.SetActive(true);
        }

        public void ToggleHover(bool toggle)
        {
            if (m_NonJoinable) return;
            if (m_Session == null) return;

            if (toggle)
            {
                if (m_Session.AvailableSlots <= 0)
                {
                    m_FullImage.SetActive(true);
                    m_JoinButton.interactable = false;
                }
                else
                {
                    m_FullImage.SetActive(false);
                    m_JoinButton.interactable = true;
                }
            }
            else
            {
                m_FullImage.SetActive(false);
            }
        }



        private void OnDestroy()
        {
            if (m_Session != null) {
                m_JoinButton.onClick.RemoveListener(JoinRoom);
            }
        }

        public void JoinRoom()
        {
            m_LobbyListUI.JoinLobby(m_Session);
        }

        // ================ For Cache logic ================

        /// <summary>
        /// Configures a lobby UI slot to represent a cached (non-live) room.
        /// Allows reconnection or deletion depending on room state.
        /// </summary>
        /// <param name="room">The cached room metadata.</param>
        /// <param name="lobbyListUI">The parent lobby UI controller.</param>
        public void CreateCachedRoomUI(RoomData room, LobbyUI lobbyListUI)
        {
            m_NonJoinable = false;
            m_Session = null; // important: this is NOT a live session
            m_LobbyListUI = lobbyListUI;

            m_RoomNameText.text = room.RoomName;
            m_PlayerCountText.text = $"0/{room.MaxPlayers}";
            m_StatusText.text = "Dormant";

            m_FullImage.SetActive(false);

            m_JoinButton.interactable = true;
            m_JoinButton.onClick.RemoveAllListeners();
            m_JoinButton.onClick.AddListener(() =>
            {
                lobbyListUI.RecreateCachedRoom(room);
            });
        }
    }
}
