using echo17.EndlessBook;
using UnityEngine;
using UnityEngine.InputSystem;

public class BookController : MonoBehaviour
{
    private Book book;

    public BookRenderer bookRenderer;
    public BookSystem bookSystem;

    // Update is called once per frame
    void Update()
    {
        if (book == null) return;
        if (Keyboard.current.spaceKey.isPressed) {
            book.ToggleBookOpening();
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

    public void SetBook(Book book)
    {
        Debug.Log(book == null);
        this.book = book;
    }
}
