using UnityEngine;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;
using XRMultiplayer;

public class BoardInteractionZone : MonoBehaviour
{
    [SerializeField] private BoardInteraction board;

    void Awake()
    {
        if (board == null)
            board = GetComponentInParent<BoardInteraction>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[BoardInteractionZone] Trigger enter by '{other.name}'.", this);
        if (IsLocalDesktopPlayer(other))
        {
            Debug.Log("[BoardInteractionZone] Local desktop player entered zone.", this);
            board?.SetDesktopInteractionEnabled(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[BoardInteractionZone] Trigger exit by '{other.name}'.", this);
        if (IsLocalDesktopPlayer(other))
        {
            Debug.Log("[BoardInteractionZone] Local desktop player exited zone.", this);
            board?.SetDesktopInteractionEnabled(false);
        }
    }

    bool IsLocalDesktopPlayer(Collider other)
    {
        var player = other.GetComponentInParent<XRINetworkPlayer>();
        var xrOrigin = other.GetComponentInParent<XROrigin>();
        bool isDesktop = IsDesktop();
        bool isLocalPlayer = player != null && player.IsLocalPlayer;
        bool isDesktopRig = xrOrigin != null;

        return isDesktop && (isLocalPlayer || isDesktopRig);
    }

    bool IsDesktop()
    {
        return !(XRGeneralSettings.Instance != null
            && XRGeneralSettings.Instance.Manager != null
            && XRGeneralSettings.Instance.Manager.activeLoader != null);
    }
}
