using UnityEngine;
using UnityEngine.InputSystem;

public class BookController : MonoBehaviour
{
    /// <summary>
    /// A reference to the active book.
    /// </summary>
    private Book book;

    /// <summary>
    /// the boss.
    /// </summary>
    private BookSystem bookSystem;

    private void Start()
    {
        bookSystem = FindFirstObjectByType<BookSystem>();
    }

    // reads the keyboard input for controlling the active book per frame.
    // there is no way to change the active book yet (the latest book is activated).
    // sends render request to the bookSystem if the active book is opened, closed or has its pages turned.
    void Update()
    {
        if (book == null) return;
        if (Mouse.current.middleButton.isPressed) {
            book.ToggleBookOpening();
            bookSystem.AddRenderRequest(book);
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            book.TurnPage(false);
            bookSystem.AddRenderRequest(book);
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            book.TurnPage(true);
            bookSystem.AddRenderRequest(book);
        }
    }

    /// <summary>
    /// Sets the active book for this controller.
    /// </summary>
    /// <param name="book"></param>
    public void SetBook(Book book)
    {
        this.book = book;
    }
}
