using echo17.EndlessBook;
using System.Collections.Generic;
using UnityEngine;

public class BookSystem : MonoBehaviour
{
    // Queues
    private Queue<string> requestQueue;
    private Queue<Book> renderQueue;

    /// <summary>
    /// the max number of requests processed per frame
    /// </summary>
    private readonly int MAX_REQUEST = 30;

    // the book subsystems
    private BookRepository bookRepo;
    private BookController bookController;
    private BookRenderer bookRenderer;

    /// <summary>
    /// the prefab used to instantiate book objects in the scene.
    /// </summary>
    private GameObject endlessBookPrefab;

    // Instantiate the bookSystem
    // instantiate the render queue and loading queue
    private void Awake()
    {
        bookRepo = gameObject.AddComponent<BookRepository>();
        requestQueue = new Queue<string>();
        renderQueue = new Queue<Book>();
    }

    /// <summary>
    /// This contains a test for "stress-testing" the book request system by asking for a gibberish book.\n
    /// Contingent on all BookSystem components being instantiated.
    /// </summary>
    private void Start()
    {
        bookController = FindFirstObjectByType<BookController>();
        bookRenderer = FindFirstObjectByType<BookRenderer>();
    }

    // the main to handle the book request for asking book
    // or the render request for rendering the new book pages
    void Update()
    {
        if (requestQueue.Count != 0) {
            int i = MAX_REQUEST;
            while (i > 0 && requestQueue.Count > 0)
            {
                string bookRequest = requestQueue.Dequeue();
                GetBookFromRepositry(bookRequest);
                i--;
            }
        }

        if(renderQueue != null && renderQueue.Count > 0)
        {
            int i = MAX_REQUEST;
            while (i > 0 && renderQueue.Count > 0)
            {
                Book bookToRender = renderQueue.Dequeue();
                bookRenderer.DisplayCurrent(bookToRender);
                i--;
            }
        }

    }

    /// <summary>
    /// Add one book rendering request to the back of the render queue.
    /// </summary>
    /// <param name="book"></param>
    public void AddRenderRequest(Book book)
    {
        renderQueue.Enqueue(book);
    }

    /// <summary>
    /// Add one book loading request to the tail of the loading queue<br></br>
    /// the bookRequest must exactly match the title of the book.
    /// </summary>
    /// <param name="bookRequest"></param>
    public void AddBookRequest(string bookRequest)
    {
        requestQueue.Enqueue(bookRequest);
    }

    // Get one book from the local repositry
    // the bookName must matches the title of the book
    public void GetBookFromRepositry(string bookName)
    {
        bookRepo.RequestBook(bookName);
    }

    public void ProcessBookRequest(Book book)
    {
        EndlessBook endlessBook = Instantiate(endlessBookPrefab).GetComponent<EndlessBook>();
        book.SetBookInstance(endlessBook);
        bookController.SetBook(book);
        bookRenderer.DisplayCurrent(book);
    }
}
