using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single search result row. Displays title + author.
/// Request button sends the book title to BookSystem, which auto-picks a free table.
/// </summary>
public class BookRowUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text authorText;
    public Button requestButton;

    private int bookId;
    private string bookTitle;
    private FetchBooks fetchBooks;

    public void Setup(int id, string title, string author, FetchBooks parent = null)
    {
        bookId = id;
        bookTitle = title;
        fetchBooks = parent;

        if (titleText != null) titleText.text = title;
        if (authorText != null) authorText.text = author;

        if (requestButton != null)
        {
            requestButton.onClick.RemoveAllListeners();
            requestButton.onClick.AddListener(OnRequestButtonClicked);
        }
        else
        {
            Debug.LogWarning($"[BookRowUI] requestButton is null for '{title}'");
        }
    }

    private void OnRequestButtonClicked()
    {
        if (string.IsNullOrEmpty(bookTitle))
        {
            Debug.LogWarning("[BookRowUI] No title to request.");
            return;
        }

        Debug.Log($"[BookRowUI] Request button clicked for: {bookTitle}");

        // Find BookSystem by type name (it's in a separate assembly)
        MonoBehaviour bookSystem = null;
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb.GetType().Name == "BookSystem")
            {
                bookSystem = mb;
                break;
            }
        }

        if (bookSystem != null)
        {
            string request = bookTitle.ToLower();
            string assignedTableLabel = fetchBooks != null ? fetchBooks.GetNextSpawnLabel() : "Unavailable";
            bookSystem.SendMessage("AddBookRequest", request, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[BookRowUI] Sent request to BookSystem: {request}");

            if (fetchBooks != null)
            {
                fetchBooks.ShowLastRequestedTable(assignedTableLabel);
                fetchBooks.UpdateRequestStatus("Request sent");
            }

            // Visual feedback
            requestButton.interactable = false;
            var btnText = requestButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = "Requested";
        }
        else
        {
            Debug.LogError("[BookRowUI] BookSystem not found in scene. Is the BookManager prefab present?");
        }
    }
}
