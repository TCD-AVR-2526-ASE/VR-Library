using echo17.EndlessBook;
using UnityEngine;

/// <summary>
/// Local-side helper for a spawned network book.
/// Resolves the shared book identity into local book data, binds it,
/// observes local changes, and applies replicated state.
/// </summary>
public class NetworkBookSync : MonoBehaviour
{
    private NetworkBookState state;
    private BookSystem bookSystem;
    private BookRepository bookRepository;
    private Book localBook;
    private bool isLoadingBook;
    private bool suppressLocalSync;
    private bool waitingForIdentity;
    private int lastObservedPage = -1;
    private bool lastObservedOpen;

    public Book LocalBook => localBook;

    public void Initialize(NetworkBookState ownerState)
    {
        state = ownerState;
        bookSystem = FindFirstObjectByType<BookSystem>();
        bookRepository = FindFirstObjectByType<BookRepository>();
    }

    private void Update()
    {
        if (state == null)
            return;

        if (waitingForIdentity && localBook == null && !isLoadingBook && state.HasUsableIdentity)
        {
            waitingForIdentity = false;
            Debug.Log($"[NetworkBookSync] Identity arrived later for {name}: title='{state.SharedTitle}' id={state.SharedBookId}");
            TryLoadAndBindBook();
        }

        if (localBook == null || localBook.BookInstance == null || suppressLocalSync)
            return;

        int currentPage = localBook.activePage;
        bool currentOpen = IsBookOpen();

        if (currentPage == lastObservedPage && currentOpen == lastObservedOpen)
            return;

        lastObservedPage = currentPage;
        lastObservedOpen = currentOpen;

        state.PushReplicatedState(currentPage, currentOpen);
    }

    public void TryLoadAndBindBook()
    {
        if (localBook != null || isLoadingBook || bookSystem == null || bookRepository == null)
        {
            if (bookSystem == null || bookRepository == null)
                Debug.LogWarning($"[NetworkBookSync] Missing refs on {name}. bookSystem={bookSystem != null} repo={bookRepository != null}");
            return;
        }

        string title = state.SharedTitle;
        int bookId = state.SharedBookId;
        Debug.Log($"[NetworkBookSync] TryLoadAndBindBook name={name} title='{title}' id={bookId} localClient={state.NetworkManager.LocalClientId} ownerLocal={state.IsOwner} owner={state.OwnerClientId}");

        if (!state.HasUsableIdentity)
        {
            waitingForIdentity = true;
            Debug.LogWarning($"[NetworkBookSync] No usable identity yet for {name}; waiting for replicated values.");
            return;
        }

        if (bookId >= 0)
        {
            Book cachedById = bookSystem.FindBookByID(bookId);
            if (cachedById != null)
            {
                Debug.Log($"[NetworkBookSync] Found cached book by id={bookId} for {name}.");
                BindLoadedBook(cachedById);
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            isLoadingBook = true;
            Debug.Log($"[NetworkBookSync] Loading '{title}' for {name}.");
            bookRepository.LoadBookData(title, book =>
            {
                isLoadingBook = false;
                if (book == null)
                {
                    Debug.LogWarning($"[NetworkBookSync] Failed to load '{title}' for {name}.");
                    return;
                }

                BindLoadedBook(book);
            });
        }
    }

    private void BindLoadedBook(Book book)
    {
        if (book == null || bookSystem == null)
            return;

        Debug.Log($"[NetworkBookSync] Binding '{book.title}' (id={book.id}) to {name}. pages={book.totalPages}");
        localBook = book;
        bookSystem.BindBookToSpawnedObject(book, gameObject);
        Debug.Log($"[NetworkBookSync] Bound '{book.title}' to {name}. EndlessBook={localBook.BookInstance != null}");
        ApplyNetworkStateImmediately();
    }

    public void ApplyNetworkStateImmediately()
    {
        if (localBook == null || localBook.BookInstance == null)
        {
            Debug.LogWarning($"[NetworkBookSync] Cannot apply network state on {name}; localBook={localBook != null} instance={(localBook != null && localBook.BookInstance != null)}");
            return;
        }

        suppressLocalSync = true;

        localBook.SetPageIndexDirect(state.SharedPage);

        int targetPageNumber = Mathf.Clamp(state.SharedPage + 1, 1, Mathf.Max(1, localBook.totalPages));
        localBook.BookInstance.SetPageNumber(targetPageNumber);

        EndlessBook.StateEnum targetState = state.SharedOpen
            ? EndlessBook.StateEnum.OpenMiddle
            : EndlessBook.StateEnum.ClosedFront;

        if (localBook.BookInstance.CurrentState != targetState)
            localBook.BookInstance.SetState(targetState, 0f);

        lastObservedPage = state.SharedPage;
        lastObservedOpen = state.SharedOpen;

        RequestRenderRefresh();
        Debug.Log($"[NetworkBookSync] Applied state on {name}: page={state.SharedPage} open={state.SharedOpen} totalPages={localBook.totalPages}");

        suppressLocalSync = false;
    }

    public string GetBookTitleForAnnotations()
    {
        if (localBook != null && !string.IsNullOrWhiteSpace(localBook.title))
            return localBook.title;

        return state != null ? state.SharedTitle : string.Empty;
    }

    public void RequestRenderRefresh()
    {
        if (bookSystem != null && localBook != null)
            bookSystem.AddRenderRequest(localBook);
    }

    private bool IsBookOpen()
    {
        return localBook != null &&
               localBook.BookInstance != null &&
               localBook.BookInstance.CurrentState == EndlessBook.StateEnum.OpenMiddle;
    }
}
