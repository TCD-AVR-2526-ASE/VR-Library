using UnityEngine;
using echo17.EndlessBook;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Drives books using their IBookInteractor (VR: per-book, Desktop: shared scene-level).
/// Supports multiple books — automatically responds to whichever book
/// the user is currently interacting with.
/// </summary>
public class BookController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BookSystem bookSystem;

    [Header("Page Turn Settings")]
    [Tooltip("Time in seconds for a single animated page turn.")]
    [SerializeField] private float pageTurnSpeed = 0.4f;

    private List<Book> trackedBooks = new List<Book>();
    private DesktopBookInteractor desktopInteractor;

    public UnityEvent OnEnterInteract;
    public UnityEvent OnExitInteract;
    public UnityEvent OnEnterFocus;
    public UnityEvent OnExitFocus;



    private void Start()
    {
        if (bookSystem == null)
            bookSystem = FindFirstObjectByType<BookSystem>();

        desktopInteractor = FindFirstObjectByType<DesktopBookInteractor>();
    }

    /// <summary>
    /// Register a book so the controller responds to its interactor.
    /// </summary>
    public void SetBook(Book book)
    {
        if (book != null && !trackedBooks.Contains(book))
            trackedBooks.Add(book);
    }

    private void Update()
    {
        trackedBooks.RemoveAll(b => b == null || b.BookInstance == null);

        // Lazy-find in case it was added after Start
        if (desktopInteractor == null)
            desktopInteractor = FindFirstObjectByType<DesktopBookInteractor>();

        // Desktop mode: single shared interactor tells us which book is active
        if (desktopInteractor != null && desktopInteractor.ActiveBook != null)
        {
            var activeBook = desktopInteractor.ActiveBook;
            if (trackedBooks.Contains(activeBook))
                ProcessBook(activeBook, desktopInteractor);
            return;
        }

        // VR mode: each book has its own interactor
        for (int i = 0; i < trackedBooks.Count; i++)
        {
            var book = trackedBooks[i];
            var interactor = book.BookInstance.GetComponent<IBookInteractor>();
            if (interactor != null)
                ProcessBook(book, interactor);
        }
    }

    private void ProcessBook(Book book, IBookInteractor interactor)
    {
        var endlessBook = book.BookInstance;
        if (endlessBook == null) return;

        // Open / Close
        if (interactor.ToggleOpen)
        {
            book.ToggleBookOpening();
            if (bookSystem != null) bookSystem.AddRenderRequest(book);
            return;
        }

        // Only allow page turns when book is open and not mid-animation
        if (endlessBook.CurrentState != EndlessBook.StateEnum.OpenMiddle) return;
        if (endlessBook.IsTurningPages || endlessBook.IsDraggingPage) return;

        if (interactor.TurnPageForward && !endlessBook.IsLastPageGroup)
        {
            book.TurnPage(true);
            endlessBook.TurnForward(pageTurnSpeed);
            endlessBook.GetComponent<NetworkBookState>()?.BroadcastPageTurn(true, pageTurnSpeed);
            if (bookSystem != null) bookSystem.AddRenderRequest(book);
        }
        else if (interactor.TurnPageBackward && !endlessBook.IsFirstPageGroup)
        {
            book.TurnPage(false);
            endlessBook.TurnBackward(pageTurnSpeed);
            endlessBook.GetComponent<NetworkBookState>()?.BroadcastPageTurn(false, pageTurnSpeed);
            if (bookSystem != null) bookSystem.AddRenderRequest(book);
        }
    }
}
