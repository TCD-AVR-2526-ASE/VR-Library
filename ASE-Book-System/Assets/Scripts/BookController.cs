using echo17.EndlessBook;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookController : MonoBehaviour
{
    // the active book
    private Book book;

    // the boss
    public BookSystem bookSystem;

    // read the keyboard input for controling the active book per frame 
    // there is no way to change the active book yet (the latest book is activated)
    // send render request to the bookSystem if the active book is opened, closed or turned
    void Update()
    {
        if (book == null) return;
        if (Keyboard.current.spaceKey.isPressed) {
            book.ToggleBookOpening();
            bookSystem.AddRenderRequest(book);
        }
        else if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            book.TurnPage(false);
            bookSystem.AddRenderRequest(book);
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            book.TurnPage(true);
            bookSystem.AddRenderRequest(book);
        }
    }

    // Set the active book for this controller
    public void SetBook(Book book)
    {
        this.book = book;
    }
}
