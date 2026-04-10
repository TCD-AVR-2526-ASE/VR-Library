using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Searches the Gutenberg library directly via the Gutendex API (gutendex.com).
/// No external API server required — queries are made straight from Unity.
/// Book requests are auto-routed to the next free table, so manual table selection is no longer needed.
/// </summary>
public class FetchBooks : MonoBehaviour
{
    [Header("Search Fields")]
    public Button searchButton;
    public TMP_InputField titleInput;
    public TMP_InputField authorInput;
    public TMP_InputField categoryInput;
    public TMP_InputField bookshelfInput;

    [Header("UI List")]
    public Transform content;      // ScrollView -> Viewport -> Content
    public GameObject bookRowPrefab; // Optional — if null, rows are built at runtime

    [Header("Legacy Table Selection")]
    [Tooltip("Old table button count. Kept only so the legacy buttons can be hidden.")]
    public int tableCount = 5;

    [Header("Settings")]
    [Tooltip("Max number of results to display.")]
    public int maxResults = 15;

    private bool isSearching;
    private Button[] tableButtons;

    private void Start()
    {
        searchButton.onClick.AddListener(() => StartCoroutine(SearchGutenberg()));
        HideLegacyTableButtons();
    }

    /// <summary>
    /// Finds the old table buttons by name ("1", "2", "3", etc.) and hides them,
    /// since the request flow now auto-assigns a free table.
    /// </summary>
    private void HideLegacyTableButtons()
    {
        tableButtons = new Button[tableCount];
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < tableCount; i++)
        {
            string btnName = (i + 1).ToString();
            foreach (var btn in allButtons)
            {
                if (btn.gameObject.name == btnName && btn.transform.parent != null && btn.transform.parent.name == "Display")
                {
                    tableButtons[i] = btn;
                    btn.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private IEnumerator SearchGutenberg()
    {
        if (isSearching) yield break;
        isSearching = true;

        string title = titleInput != null ? titleInput.text.Trim() : "";
        string author = authorInput != null ? authorInput.text.Trim() : "";
        string category = categoryInput != null ? categoryInput.text.Trim() : "";
        string bookshelf = bookshelfInput != null ? bookshelfInput.text.Trim() : "";

        List<string> queryParts = new List<string>();

        string searchTerm = "";
        if (!string.IsNullOrEmpty(title))
            searchTerm += title;
        if (!string.IsNullOrEmpty(author))
            searchTerm += (searchTerm.Length > 0 ? " " : "") + author;
        if (!string.IsNullOrEmpty(searchTerm))
            queryParts.Add("search=" + UnityWebRequest.EscapeURL(searchTerm));

        if (!string.IsNullOrEmpty(category) || !string.IsNullOrEmpty(bookshelf))
        {
            string topic = category;
            if (!string.IsNullOrEmpty(bookshelf))
                topic += (topic.Length > 0 ? " " : "") + bookshelf;
            queryParts.Add("topic=" + UnityWebRequest.EscapeURL(topic));
        }

        if (queryParts.Count == 0)
        {
            Debug.LogWarning("[FetchBooks] Enter at least one search term.");
            isSearching = false;
            yield break;
        }

        string url = "https://gutendex.com/books?" + string.Join("&", queryParts);
        Debug.Log($"[FetchBooks] Searching: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("User-Agent", "Mozilla/5.0");
            request.timeout = 30;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FetchBooks] Search failed: {request.error}");
                isSearching = false;
                yield break;
            }

            string json = request.downloadHandler.text;
            GutendexResponse response = JsonUtility.FromJson<GutendexResponse>(json);

            // Clear old results
            foreach (Transform child in content)
                Destroy(child.gameObject);

            if (response == null || response.results == null || response.results.Length == 0)
            {
                Debug.Log("[FetchBooks] No books found.");
                isSearching = false;
                yield break;
            }

            int count = Mathf.Min(response.results.Length, maxResults);
            for (int i = 0; i < count; i++)
            {
                var book = response.results[i];
                string authorName = (book.authors != null && book.authors.Length > 0)
                    ? book.authors[0].name
                    : "Unknown";

                if (bookRowPrefab != null)
                {
                    GameObject row = Instantiate(bookRowPrefab, content);
                    BookRowUI ui = row.GetComponent<BookRowUI>();
                    if (ui != null)
                        ui.Setup(book.id, book.title, authorName, this);
                    else
                        Debug.LogWarning($"[FetchBooks] Prefab missing BookRowUI component!");
                }
                else
                {
                    GameObject row = CreateBookRow(book.id, book.title, authorName);
                    row.transform.SetParent(content, false);
                }
            }

            Debug.Log($"[FetchBooks] Found {count} books.");
        }

        isSearching = false;
    }

    /// <summary>
    /// Builds a book row UI element entirely from code (no prefab needed).
    /// </summary>
    private GameObject CreateBookRow(int id, string title, string authorName)
    {
        // Row container
        GameObject row = new GameObject($"BookRow_{id}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 50);
        var hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(5, 5, 2, 2);
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.minHeight = 50;
        rowLE.preferredHeight = 50;

        var bg = row.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.6f);
        bg.raycastTarget = false;

        // Title text
        GameObject titleGO = new GameObject("Title", typeof(RectTransform));
        titleGO.transform.SetParent(row.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = title;
        titleTMP.fontSize = 16;
        titleTMP.color = Color.white;
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
        titleTMP.overflowMode = TextOverflowModes.Ellipsis;
        titleTMP.raycastTarget = false;
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.flexibleWidth = 3;
        titleLE.minWidth = 100;

        // Author text
        GameObject authorGO = new GameObject("Author", typeof(RectTransform));
        authorGO.transform.SetParent(row.transform, false);
        var authorTMP = authorGO.AddComponent<TextMeshProUGUI>();
        authorTMP.text = authorName;
        authorTMP.fontSize = 14;
        authorTMP.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        authorTMP.alignment = TextAlignmentOptions.MidlineLeft;
        authorTMP.overflowMode = TextOverflowModes.Ellipsis;
        authorTMP.raycastTarget = false;
        var authorLE = authorGO.AddComponent<LayoutElement>();
        authorLE.flexibleWidth = 2;
        authorLE.minWidth = 80;

        // Request button
        GameObject btnGO = new GameObject("RequestButton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(row.transform, false);
        var btnImage = btnGO.GetComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 0.3f, 1f);
        btnImage.raycastTarget = true;
        var btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = btnImage;
        var btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.minWidth = 90;
        btnLE.preferredWidth = 90;

        // Button label
        GameObject btnLabelGO = new GameObject("Label", typeof(RectTransform));
        btnLabelGO.transform.SetParent(btnGO.transform, false);
        var btnLabelRT = btnLabelGO.GetComponent<RectTransform>();
        btnLabelRT.anchorMin = Vector2.zero;
        btnLabelRT.anchorMax = Vector2.one;
        btnLabelRT.offsetMin = Vector2.zero;
        btnLabelRT.offsetMax = Vector2.zero;
        var btnLabel = btnLabelGO.AddComponent<TextMeshProUGUI>();
        btnLabel.text = "Request";
        btnLabel.fontSize = 14;
        btnLabel.color = Color.white;
        btnLabel.alignment = TextAlignmentOptions.Center;
        btnLabel.raycastTarget = false;

        // Wire up BookRowUI
        var bookRowUI = row.AddComponent<BookRowUI>();
        bookRowUI.titleText = titleTMP;
        bookRowUI.authorText = authorTMP;
        bookRowUI.requestButton = btn;
        bookRowUI.Setup(id, title, authorName, this);

        return row;
    }

    // --- Gutendex JSON response classes ---

    [System.Serializable]
    public class GutendexResponse
    {
        public int count;
        public GutendexBook[] results;
    }

    [System.Serializable]
    public class GutendexBook
    {
        public int id;
        public string title;
        public GutendexAuthor[] authors;
        public string[] subjects;
        public string[] bookshelves;
    }

    [System.Serializable]
    public class GutendexAuthor
    {
        public string name;
        public int birth_year;
        public int death_year;
    }
}
