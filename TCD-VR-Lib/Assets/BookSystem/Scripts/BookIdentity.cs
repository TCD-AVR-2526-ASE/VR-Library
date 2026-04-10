using UnityEngine;

/// <summary>
/// Attached to each spawned book GameObject to link it back to its Book data.
/// Used by BookController to identify which book was clicked.
/// </summary>
public class BookIdentity : MonoBehaviour
{
    public Book Book { get; private set; }

    public void Init(Book book)
    {
        Book = book;
    }
}
