using echo17.EndlessBook;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Book", menuName = "Scriptable Objects/Book")]
public class Book : ScriptableObject
{
    // Metadata
    public string path { get; private set; }
    public int id { get; private set; } = -1;
    public float fontSize { get; private set; } = 10f;
    public string title { get; private set; } = "";
    public int activePage { get; protected set; }
    public int totalPages => pages != null ? pages.Count : 0;

    /// <summary>
    /// The EndlessBook 3D instance linked to this book's data.
    /// </summary>
    public EndlessBook BookInstance { get; private set; }

    // Materials and textures: [0] cover, [1] left page, [2] right page
    public List<Material> materials;
    public List<RenderTexture> renderTextures;
    private List<string> pages;

    /// <summary>
    /// Initialize book data, materials, and render textures.
    /// </summary>
    public void Init(string path, string name, float fontSize, int bookId = -1)
    {
        this.path = path;
        this.fontSize = fontSize;
        this.id = bookId;
        title = name;

        materials = new List<Material>();
        renderTextures = new List<RenderTexture>();

        for (int i = 0; i < 3; i++)
        {
            materials.Add(new Material(Shader.Find("Universal Render Pipeline/Simple Lit")));
            renderTextures.Add(new RenderTexture(1024, 1024, 1));
            renderTextures[i].Create();
            materials[i].SetTexture("_BaseMap", renderTextures[i]);
        }
    }

    /// <summary>
    /// Store paginated text content.
    /// </summary>
    public void Paginate(List<string> paginatedText)
    {
        pages = paginatedText;
    }

    /// <summary>
    /// Advance or retreat by 2 pages (one leaf = left + right).
    /// </summary>
    public void TurnPage(bool forward = true)
    {
        int mod = forward ? 2 : -2;
        activePage = Math.Clamp(activePage + mod, 0, Math.Max(0, totalPages - 1));
    }

    /// <summary>
    /// Sync the page index from the EndlessBook's actual page number after a drag turn.
    /// </summary>
    public void SyncPageIndex(int leftPageNumber)
    {
        // EndlessBook page numbers are 1-based; our page list is 0-based
        int index = Math.Max(0, leftPageNumber - 1);
        activePage = Math.Clamp(index, 0, Math.Max(0, totalPages - 1));
    }

    /// <summary>
    /// Returns (left page text, right page text) for the current spread.
    /// </summary>
    public Tuple<string, string> GetPageText()
    {
        if (pages == null || pages.Count == 0) return null;

        string left = activePage < pages.Count ? pages[activePage] : "";
        string right = (activePage + 1) < pages.Count ? pages[activePage + 1] : "";
        return new Tuple<string, string>(left, right);
    }

    /// <summary>
    /// Directly sets the active page index without animation.
    /// Useful when a networked book needs to snap to shared state.
    /// </summary>
    public void SetPageIndexDirect(int index)
    {
        activePage = Math.Clamp(index, 0, Math.Max(0, totalPages - 1));
    }

    /// <summary>
    /// Link a 3D EndlessBook instance to this data and apply materials.
    /// Populates EndlessBook's internal page data so page turning works.
    /// </summary>
    public void SetBookInstance(EndlessBook endlessBook)
    {
        BookInstance = endlessBook;
        BookInstance.SetMaterial(EndlessBook.MaterialEnum.BookCover, materials[0]);
        BookInstance.SetMaterial(EndlessBook.MaterialEnum.BookPageLeft, materials[1]);
        BookInstance.SetMaterial(EndlessBook.MaterialEnum.BookPageRight, materials[2]);

        // EndlessBook needs page data entries to know how many pages exist.
        // Without this, IsFirstPageGroup/IsLastPageGroup are both true and
        // TurnForward/TurnBackward silently do nothing.
        // Odd pages (1,3,5...) = left side material, even pages (2,4,6...) = right side material.
        if (pages != null)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                Material pageMat = (i % 2 == 0) ? materials[1] : materials[2];
                BookInstance.AddPageData(pageMat);
            }
        }
    }

    /// <summary>
    /// Toggle between open and closed states.
    /// </summary>
    public void ToggleBookOpening()
    {
        if (BookInstance == null) return;

        if (BookInstance.CurrentState == EndlessBook.StateEnum.ClosedFront)
            BookInstance.SetState(EndlessBook.StateEnum.OpenMiddle);
        else
            BookInstance.SetState(EndlessBook.StateEnum.ClosedFront);
    }
}
