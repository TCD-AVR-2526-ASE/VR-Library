using echo17.EndlessBook;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Book", menuName = "Scriptable Objects/Book")]
public class Book : ScriptableObject
{
    // Book Metadata
    /// <summary>
    /// Book Path relative to the Unity Project's directory.
    /// Should ALWAYS (until it's on a remote cache) be in Assets/Resources!
    /// </summary>
    public string path { get; private set; } = null;
    /// <summary>
    /// Unique Book ID
    /// </summary>
    public int id { get; private set; } = -1;
    /// <summary>
    /// The font size to the current book. Maybe could be moved to the renderer? Idk.
    /// </summary>
    public float fontSize { get; private set; } = 10f;
    public string title { get; private set; } = "";
    public int activePage {get; protected set;} = 0;
    public int totalPages => pages.Count;

    /// <summary>
    /// The book object attached to this book's data
    /// </summary>
    private EndlessBook endlessbBookInstance = null;

    // materials and textures for rendering the book's contents
    // [0] cover [1] leftPage [2] rightPage
    /// <summary>
    /// The list of materials associated with this book, in order:
    /// 0: Book cover,
    /// 1: Left page,
    /// 2: Right page.
    /// </summary>
    public List<Material> materials;
    /// <summary>
    /// The list of textures associated with this book, in order:
    /// 0: Book cover,
    /// 1: Left page,
    /// 2: Right page.
    /// </summary>
    public List<RenderTexture> renderTextures;
    /// <summary>
    /// The contents of the book segmented by page.
    /// </summary>
    private List<string> pages;

    /// <summary>
    /// Intializes the core data of the book.
    /// Also initializes materials & textures used to render the book's contents.<br></br>
    /// the path should be the path in local disk, relative to the unity project root;
    /// 'name' corresponds to the book title as it is displayed.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="fontSize"></param>
    public void Init(string path, string name, float fontSize)
    {
        this.path = path;
        this.fontSize = fontSize;
        title = name;

        materials = new List<Material>();
        renderTextures = new List<RenderTexture>();

        for (int i = 0; i < 3 ; i++)
        {
            materials.Add(new Material(Shader.Find("Universal Render Pipeline/Simple Lit")));
            renderTextures.Add(new RenderTexture(1024, 1024, 1));
            renderTextures[i].Create();
            materials[i].SetTexture("_BaseMap", renderTextures[i]);
        }
    }

    /// <summary>
    /// Passes the paginated text to the book.<br></br>
    /// paginatedText should be a list of strings, each representing a page's content.
    /// </summary>
    /// <param name="paginatedText"></param>
    public void Paginate(List<string> paginatedText)
    {
        pages = paginatedText;
    }

    /// <summary>
    /// Updates the current page index<br></br>
    /// +2 if forward, otherwise -2 within the range [0, pageCount].
    /// </summary>
    /// <param name="forward"></param>
    public void TurnPage(bool forward = true)
    {
        var mod = forward ? 2 : -2;
        activePage = Math.Clamp(activePage + mod, 0, totalPages);
    }

    /// <summary>
    /// Returns a tuple containing the contents of the current and next page respectively (Left, Right).
    /// </summary>
    /// <returns></returns>
    public Tuple<string, string> GetPageText()
    {
        string left = pages[activePage];
        string right = activePage == totalPages ? null : pages[activePage + 1];
        return new(left, right);
    }

    /// <summary>
    /// Assigns a book mesh to a book data object.<br></br>
    /// Also applies the materials associated with a book to the mesh.
    /// </summary>
    /// <param name="endlessBook"></param>
    public void SetBookInstance(EndlessBook endlessBook)
    {
        this.endlessbBookInstance = endlessBook;
        endlessbBookInstance.SetMaterial(EndlessBook.MaterialEnum.BookCover, materials[0]);
        endlessbBookInstance.SetMaterial(EndlessBook.MaterialEnum.BookPageLeft, materials[1]);
        endlessbBookInstance.SetMaterial(EndlessBook.MaterialEnum.BookPageRight, materials[2]);
    }


    /// <summary>
    /// Opens the book object if closed, or closes it if not.
    /// </summary>
    public void ToggleBookOpening()
    {
        if (this.endlessbBookInstance.CurrentState == EndlessBook.StateEnum.ClosedFront)
        {
            this.endlessbBookInstance.SetState(EndlessBook.StateEnum.OpenMiddle);
        }
        else
        {
            this.endlessbBookInstance.SetState(EndlessBook.StateEnum.ClosedFront);
        }
    }
}
