using UnityEngine;
using echo17.EndlessBook;
using UnityEngine.InputSystem;

public class BookController : MonoBehaviour
{
    public EndlessBook book;
    public BookLoader bookLoader;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.enterKey.isPressed) {
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
            bookLoader.PrevPage();
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            bookLoader.NextPage();
        }
    }
}
