using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Stores the replicated identity and runtime state for a spawned shared book.
/// Acts as the RPC entry point for state changes while local loading and binding remain in <see cref="NetworkBookSync"/>.
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
    private BookSystem bookSystem;
    private int appliedTableIndex = -1;

    public string SharedTitle => networkTitle.Value.ToString();
    public int SharedBookId => networkBookId.Value;
    public int SharedPage => networkPage.Value;
    public bool SharedOpen => networkOpen.Value;
    public int SharedTableIndex => networkTableIndex.Value;
    public bool HasUsableIdentity => !string.IsNullOrWhiteSpace(SharedTitle) || SharedBookId >= 0;

    /// <summary>
    /// Seeds the initial replicated identity and placement values before the network object spawns.
    /// </summary>
    /// <param name="title">The shared book title.</param>
    /// <param name="bookId">The cached repository id when known, otherwise <c>-1</c>.</param>
    /// <param name="page">The initial logical page index.</param>
    /// <param name="open">Whether the book starts open.</param>
    /// <param name="tableIndex">The assigned table index, or <c>-1</c> for fallback placement.</param>
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
        bookSystem = FindFirstObjectByType<BookSystem>();

        networkTitle.OnValueChanged += OnIdentityChanged;
        networkBookId.OnValueChanged += OnIdentityChanged;
        networkPage.OnValueChanged += OnPageChanged;
        networkOpen.OnValueChanged += OnOpenChanged;
        networkTableIndex.OnValueChanged += OnTableIndexChanged;

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
        ApplyTableOccupancy(networkTableIndex.Value);
    }

    public override void OnNetworkDespawn()
    {
        networkTitle.OnValueChanged -= OnIdentityChanged;
        networkBookId.OnValueChanged -= OnIdentityChanged;
        networkPage.OnValueChanged -= OnPageChanged;
        networkOpen.OnValueChanged -= OnOpenChanged;
        networkTableIndex.OnValueChanged -= OnTableIndexChanged;

        if (bookSystem != null && appliedTableIndex >= 0)
            bookSystem.ClearTableOccupancy(appliedTableIndex);

        appliedTableIndex = -1;
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

    /// <summary>
    /// Pushes the latest local page/open state into the replicated network variables.
    /// Non-owners forward the request to the owner first.
    /// </summary>
    /// <param name="page">The logical page index to replicate.</param>
    /// <param name="open">Whether the book is open.</param>
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

    private void OnTableIndexChanged(int previous, int current)
    {
        ApplyTableOccupancy(current, previous);
    }

    public void OnLocalGrabStarted()
    {
    }

    public void OnLocalGrabReleased()
    {
    }

    /// <summary>
    /// Broadcasts a page-turn animation event so remote clients can play the same turn locally.
    /// </summary>
    /// <param name="forward"><c>true</c> for a forward turn; <c>false</c> for a backward turn.</param>
    /// <param name="turnSpeed">The animation duration used by <see cref="echo17.EndlessBook.EndlessBook"/>.</param>
    public void BroadcastPageTurn(bool forward, float turnSpeed)
    {
        ulong initiatorClientId = NetworkManager != null ? NetworkManager.LocalClientId : ulong.MaxValue;

        if (IsOwner)
        {
            sync?.PlayRemotePageTurn(forward, turnSpeed);
            BroadcastPageTurnClientRpc(forward, turnSpeed, initiatorClientId);
            return;
        }

        BroadcastPageTurnOwnerRpc(forward, turnSpeed, initiatorClientId);
    }

    [Rpc(SendTo.Owner)]
    private void BroadcastPageTurnOwnerRpc(bool forward, float turnSpeed, ulong initiatorClientId)
    {
        sync?.PlayRemotePageTurn(forward, turnSpeed);
        BroadcastPageTurnClientRpc(forward, turnSpeed, initiatorClientId);
    }

    [Rpc(SendTo.NotMe)]
    private void BroadcastPageTurnClientRpc(bool forward, float turnSpeed, ulong initiatorClientId)
    {
        if (NetworkManager != null && NetworkManager.LocalClientId == initiatorClientId)
            return;

        sync?.PlayRemotePageTurn(forward, turnSpeed);
    }

    /// <summary>
    /// Broadcasts a highlight stroke to all clients and applies it immediately on the authoritative side.
    /// </summary>
    /// <param name="pageIndex">The page being annotated.</param>
    /// <param name="from">The start point in annotation texture space.</param>
    /// <param name="to">The end point in annotation texture space.</param>
    /// <param name="halfWidth">Half-width of the highlighter stamp.</param>
    /// <param name="halfHeight">Half-height of the highlighter stamp.</param>
    /// <param name="color">The highlight color to apply.</param>
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

    /// <summary>
    /// Clears the annotation texture for a specific page and synchronizes that change to peers.
    /// </summary>
    /// <param name="pageIndex">The page whose annotations should be removed.</param>
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

    /// <summary>
    /// Saves a text note for a page and propagates that note to all peers.
    /// </summary>
    /// <param name="pageIndex">The page associated with the note.</param>
    /// <param name="noteText">The note content to save.</param>
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

    private void ApplyTableOccupancy(int currentTableIndex, int previousTableIndex = -1)
    {
        if (bookSystem == null)
            bookSystem = FindFirstObjectByType<BookSystem>();

        if (bookSystem == null)
            return;

        if (previousTableIndex >= 0)
            bookSystem.ClearTableOccupancy(previousTableIndex);

        if (currentTableIndex >= 0)
            bookSystem.MarkTableOccupied(currentTableIndex);

        appliedTableIndex = currentTableIndex;
    }
}
