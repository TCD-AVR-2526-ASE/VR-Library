using UnityEngine;
using echo17.EndlessBook;
using UnityEngine.InputSystem;

public class BookController : MonoBehaviour
{
    private EndlessBook book;
    public Book bookData;

    public BookRenderer bookRenderer;

    // Update is called once per frame
    void Update()
    {
        if (bookData == null || book == null) return;
        if (Keyboard.current.spaceKey.isPressed) {
            if (book.CurrentState == EndlessBook.StateEnum.ClosedFront)
            {
                book.SetState(EndlessBook.StateEnum.OpenMiddle);
                Debug.Log("OpenBook!");
            }
            else
            {
                book.SetState(EndlessBook.StateEnum.ClosedFront);
                Debug.Log("Closebook!");
            }
        }
        else if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            bookData.TurnPage(false);
            bookRenderer.DisplayCurrent(bookData);
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            bookData.TurnPage(true);
            bookRenderer.DisplayCurrent(bookData);
        }
    }
}
