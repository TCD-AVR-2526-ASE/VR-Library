using echo17.EndlessBook;
using System.Collections.Generic;
using UnityEngine;

public class BookSystem : MonoBehaviour
{
    // Queues
    private Queue<string> requestQueue;
    private Queue<Book> renderQueue;

    /// <summary>
    /// the max number of requests that can be processed per frame
    /// </summary>
    private readonly int MAX_REQUEST = 30;

    // the book subsystems
    private BookRepository bookRepo;
    private BookController bookController;
    private BookRenderer bookRenderer;
    private Camera targetCamera;

    /// <summary>
    /// the prefab used to instantiate book objects in the scene.
    /// </summary>
    private GameObject endlessBookPrefab;

    // Instantiate the bookSystem
    // instantiate the render queue and loading queue
    private void Awake()
    {
        bookRepo = gameObject.AddComponent<BookRepository>();
        endlessBookPrefab = Resources.Load<GameObject>("BookSystem/Prefabs/Book");

        requestQueue = new Queue<string>();
        renderQueue = new Queue<Book>();
    }

    /// <summary>
    /// Initializes the bookController and bookRenderer for Book Processing
    /// </summary>
    private void Start()
    {
        bookController = FindFirstObjectByType<BookController>();
        bookRenderer = FindFirstObjectByType<BookRenderer>();
    }

    /// <summary>
    /// the main to handle the book request for asking book<br></br>
    /// or the render request for rendering the new book pages
    /// </summary>
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
    /// the bookRequest exact/partial match the title of the book.
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

    /// <summary>
    /// Instantiate the book prefab<br></br>
    /// set the book prefab to the book controller<br></br>
    /// display the book prefab
    /// </summary>
    /// <param name="book"></param>
    public void ProcessBookRequest(Book book)
    {
        float spawnDepth = 3.5f;
        targetCamera = Camera.main;
        Vector3 viewportCenter = new Vector3(0.5f, 0.4f, spawnDepth);
        Debug.Log(viewportCenter);
        Vector3 spawnPosition = targetCamera.ViewportToWorldPoint(viewportCenter);
        spawnPosition.y += 1.0f; // adjust height above ground
        EndlessBook endlessBook = Instantiate(endlessBookPrefab, spawnPosition, Quaternion.identity).GetComponent<EndlessBook>();
        endlessBook.transform.localScale = Vector3.one * 4.5f;

        book.SetBookInstance(endlessBook);
        bookController.SetBook(book);
        bookRenderer.DisplayCurrent(book);
    }
}
