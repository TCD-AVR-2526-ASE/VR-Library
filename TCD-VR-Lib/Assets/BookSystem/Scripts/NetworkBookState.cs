using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Replicated shared state for a spawned network book.
/// Keeps only the networked identity/state and RPC entry points.
/// Local loading/binding is handled by NetworkBookSync.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class NetworkBookState : NetworkBehaviour
{
    [SerializeField] private string initialTitle = string.Empty;
    [SerializeField] private int initialBookId = -1;
    [SerializeField] private int initialPage;
    [SerializeField] private bool initialOpen;
    [SerializeField] private int initialTableIndex = -1;

    private readonly NetworkVariable<FixedString512Bytes> networkTitle =
        new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<int> networkBookId =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<int> networkPage =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> networkOpen =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<int> networkTableIndex =
        new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkBookSync sync;

    public string SharedTitle => networkTitle.Value.ToString();
    public int SharedBookId => networkBookId.Value;
    public int SharedPage => networkPage.Value;
    public bool SharedOpen => networkOpen.Value;
    public int SharedTableIndex => networkTableIndex.Value;
    public bool HasUsableIdentity => !string.IsNullOrWhiteSpace(SharedTitle) || SharedBookId >= 0;

    public void SetInitialState(string title, int bookId, int page, bool open, int tableIndex = -1)
    {
        initialTitle = title ?? string.Empty;
        initialBookId = bookId;
        initialPage = page;
        initialOpen = open;
        initialTableIndex = tableIndex;
    }

    public void SetPreloadedBook(Book book)
    {
        EnsureSync();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[NetworkBookState] OnNetworkSpawn name={name} localClient={NetworkManager.LocalClientId} server={IsServer} ownerLocal={IsOwner} owner={OwnerClientId} initialTitle='{initialTitle}' initialBookId={initialBookId}");

        EnsureSync();

        networkTitle.OnValueChanged += OnIdentityChanged;
        networkBookId.OnValueChanged += OnIdentityChanged;
        networkPage.OnValueChanged += OnPageChanged;
        networkOpen.OnValueChanged += OnOpenChanged;

        if (IsOwner)
        {
            networkTitle.Value = new FixedString512Bytes(initialTitle ?? string.Empty);
            networkBookId.Value = initialBookId;
            networkPage.Value = initialPage;
            networkOpen.Value = initialOpen;
            networkTableIndex.Value = initialTableIndex;
            Debug.Log($"[NetworkBookState] Seeded owner state title='{initialTitle}' id={initialBookId} page={initialPage} open={initialOpen} table={initialTableIndex}");
        }

        sync.TryLoadAndBindBook();
    }

    public override void OnNetworkDespawn()
    {
        networkTitle.OnValueChanged -= OnIdentityChanged;
        networkBookId.OnValueChanged -= OnIdentityChanged;
        networkPage.OnValueChanged -= OnPageChanged;
        networkOpen.OnValueChanged -= OnOpenChanged;
    }

    private void EnsureSync()
    {
        if (sync == null)
        {
            sync = GetComponent<NetworkBookSync>();
            if (sync == null)
                sync = gameObject.AddComponent<NetworkBookSync>();
            sync.Initialize(this);
        }
    }

    public void PushReplicatedState(int page, bool open)
    {
        if (IsOwner)
        {
            networkPage.Value = page;
            networkOpen.Value = open;
        }
        else
        {
            SubmitStateOwnerRpc(page, open);
        }
    }

    [Rpc(SendTo.Owner)]
    private void SubmitStateOwnerRpc(int page, bool open)
    {
        networkPage.Value = page;
        networkOpen.Value = open;
    }

    private void OnIdentityChanged(FixedString512Bytes previous, FixedString512Bytes current)
    {
        sync?.TryLoadAndBindBook();
    }

    private void OnIdentityChanged(int previous, int current)
    {
        sync?.TryLoadAndBindBook();
    }

    private void OnPageChanged(int previous, int current)
    {
        sync?.ApplyNetworkStateImmediately();
    }

    private void OnOpenChanged(bool previous, bool current)
    {
        sync?.ApplyNetworkStateImmediately();
    }

    public void OnLocalGrabStarted()
    {
    }

    public void OnLocalGrabReleased()
    {
    }

    public void BroadcastHighlightLine(int pageIndex, Vector2Int from, Vector2Int to, int halfWidth, int halfHeight, Color color)
    {
        if (IsOwner)
        {
            ApplyHighlightLine(pageIndex, from, to, halfWidth, halfHeight, color);
            BroadcastHighlightLineClientRpc(pageIndex, from.x, from.y, to.x, to.y, halfWidth, halfHeight, color);
            return;
        }

        BroadcastHighlightLineOwnerRpc(pageIndex, from.x, from.y, to.x, to.y, halfWidth, halfHeight, color);
    }

    [Rpc(SendTo.Owner)]
    private void BroadcastHighlightLineOwnerRpc(int pageIndex, int fromX, int fromY, int toX, int toY, int halfWidth, int halfHeight, Color color)
    {
        ApplyHighlightLine(pageIndex, new Vector2Int(fromX, fromY), new Vector2Int(toX, toY), halfWidth, halfHeight, color);
        BroadcastHighlightLineClientRpc(pageIndex, fromX, fromY, toX, toY, halfWidth, halfHeight, color);
    }

    [Rpc(SendTo.NotMe)]
    private void BroadcastHighlightLineClientRpc(int pageIndex, int fromX, int fromY, int toX, int toY, int halfWidth, int halfHeight, Color color)
    {
        ApplyHighlightLine(pageIndex, new Vector2Int(fromX, fromY), new Vector2Int(toX, toY), halfWidth, halfHeight, color);
    }

    public void BroadcastClearPage(int pageIndex)
    {
        if (IsOwner)
        {
            ApplyClearPage(pageIndex);
            BroadcastClearPageClientRpc(pageIndex);
            return;
        }

        BroadcastClearPageOwnerRpc(pageIndex);
    }

    [Rpc(SendTo.Owner)]
    private void BroadcastClearPageOwnerRpc(int pageIndex)
    {
        ApplyClearPage(pageIndex);
        BroadcastClearPageClientRpc(pageIndex);
    }

    [Rpc(SendTo.NotMe)]
    private void BroadcastClearPageClientRpc(int pageIndex)
    {
        ApplyClearPage(pageIndex);
    }

    public void BroadcastNote(int pageIndex, string noteText)
    {
        if (IsOwner)
        {
            ApplyNote(pageIndex, noteText);
            BroadcastNoteClientRpc(pageIndex, noteText ?? string.Empty);
            return;
        }

        BroadcastNoteOwnerRpc(pageIndex, noteText ?? string.Empty);
    }

    [Rpc(SendTo.Owner)]
    private void BroadcastNoteOwnerRpc(int pageIndex, string noteText)
    {
        ApplyNote(pageIndex, noteText);
        BroadcastNoteClientRpc(pageIndex, noteText ?? string.Empty);
    }

    [Rpc(SendTo.NotMe)]
    private void BroadcastNoteClientRpc(int pageIndex, string noteText)
    {
        ApplyNote(pageIndex, noteText);
    }

    private void ApplyHighlightLine(int pageIndex, Vector2Int from, Vector2Int to, int halfWidth, int halfHeight, Color color)
    {
        string title = sync != null ? sync.GetBookTitleForAnnotations() : SharedTitle;
        if (string.IsNullOrWhiteSpace(title))
            return;

        Texture2D tex = BookAnnotationStore.GetTexture(title, pageIndex);
        BookAnnotationStore.DrawHighlighterLine(tex, from, to, halfWidth, halfHeight, color);
        sync?.RequestRenderRefresh();
    }

    private void ApplyClearPage(int pageIndex)
    {
        string title = sync != null ? sync.GetBookTitleForAnnotations() : SharedTitle;
        if (string.IsNullOrWhiteSpace(title))
            return;

        BookAnnotationStore.Clear(title, pageIndex);
        sync?.RequestRenderRefresh();
    }

    private void ApplyNote(int pageIndex, string noteText)
    {
        string title = sync != null ? sync.GetBookTitleForAnnotations() : SharedTitle;
        if (string.IsNullOrWhiteSpace(title))
            return;

        BookAnnotationStore.SaveNote(title, pageIndex, noteText);
    }
}
