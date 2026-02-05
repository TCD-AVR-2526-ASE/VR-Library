using echo17.EndlessBook;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Book", menuName = "Scriptable Objects/Book")]
public class Book : ScriptableObject
{
    public string path { get; private set; } = null;
    public int id { get; private set; } = -1;
    public float fontSize { get; private set; } = 10f;
    public string title { get; private set; } = "";
    public int activePage {get; protected set;} = 0;
    public int totalPages { get; protected set; } = 0;
    private EndlessBook endlessbBookInstance = null;
    // [0] cover [1] leftPage [2] rightPage
    public List<Material> materials;
    public List<RenderTexture> renderTextures;

    private List<string> pages;

    public void Init(string path, string name, float fontSize)
    {
        this.path = path;
        this.fontSize = fontSize;
        title = name;
        //pageTextures = new List<RenderTexture>();
        //pageTextures.Add(new RenderTexture(1024, 1024, 1)); // cover
        //pageTextures.Add(new RenderTexture(1024, 1024, 1)); // left page
        //pageTextures.Add(new RenderTexture(1024, 1024, 1)); // right page

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

    public void Paginate(int pageCount, List<string> paginatedText)
    {
        totalPages = pageCount;
        pages = paginatedText;
    }

    public void TurnPage(bool forward = true)
    {
        activePage = forward ? 
            activePage < totalPages - 2 ? activePage + 2 : totalPages :
            activePage > 0 ? activePage - 2 : activePage;
    }

    public Tuple<string, string> GetPageText()
    {
        string left = pages[activePage];
        string right = activePage == totalPages ? null : pages[activePage + 1];
        return new(left, right);
    }

    public void SetBookInstance(EndlessBook endlessBook)
    {
        this.endlessbBookInstance = endlessBook;
        endlessbBookInstance.SetMaterial(EndlessBook.MaterialEnum.BookCover, materials[0]);
        endlessbBookInstance.SetMaterial(EndlessBook.MaterialEnum.BookPageLeft, materials[1]);
        endlessbBookInstance.SetMaterial(EndlessBook.MaterialEnum.BookPageRight, materials[2]);
    }

    public void ToggleBookOpening()
    {
        if (this.endlessbBookInstance.CurrentState == EndlessBook.StateEnum.ClosedFront)
        {
            this.endlessbBookInstance.SetState(EndlessBook.StateEnum.OpenMiddle);
            Debug.Log("OpenBook!");
        }
        else
        {
            this.endlessbBookInstance.SetState(EndlessBook.StateEnum.ClosedFront);
            Debug.Log("Closebook!");
        }
    }
}
