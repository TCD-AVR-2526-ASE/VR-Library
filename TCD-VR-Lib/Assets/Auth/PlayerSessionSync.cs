using Unity.Netcode;
using UnityEngine;

namespace XRMultiplayer
{
    public class PlayerSessionSync : NetworkBehaviour
    {
        public NetworkVariable<long> networkUserId = new NetworkVariable<long>(
            0, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Owner
        );

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                TrySyncUserId();
                
                // bind session events to keep networkUserId in sync with AuthSession state
                AuthSession.LoggedIn += OnUserLoggedIn;
                AuthSession.LoggedOut += OnUserLoggedOut;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                // should unbind events when despawning to prevent potential memory leaks or unintended callbacks
                AuthSession.LoggedIn -= OnUserLoggedIn;
                AuthSession.LoggedOut -= OnUserLoggedOut;
            }
            base.OnNetworkDespawn();
        }


        private void OnUserLoggedIn(TokenVo token)
        {
            // triggered when the local player logs in successfully
            TrySyncUserId();
        }

        private void OnUserLoggedOut()
        {
            // triggered when the local player logs out
            if (IsSpawned && IsOwner)
            {
                networkUserId.Value = 0;
                Debug.Log("[PlayerSessionSync] User logged out, cleared network userId.");
            }
        }


        private void TrySyncUserId()
        {
            // should only attempt to sync if 
            // 1. this object is spawned, 
            // 2. we are the owner
            // 3. we have valid user info from the AuthSession
            if (IsSpawned && IsOwner && AuthSession.CurrentUserInfo != null)
            {
                networkUserId.Value = AuthSession.CurrentUserInfo.userId;
                Debug.Log($"[PlayerSessionSync] Successfully synced userId ({networkUserId.Value}) to network.");
            }
        }
    }
}