using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "Book", menuName = "Scriptable Objects/Book")]
public class Book : ScriptableObject
{
    public string path { get; private set; } = null;
    public string id { get; private set; } = null;
    public float fontSize { get; private set; } = .1f;
    public string title { get; private set; } = "";
    public int activePage {get; protected set;} = 0;
    public int totalPages { get; protected set; } = 0;

    private List<string> pages;

    public void Init(string path, string name, float fontSize, int pageCount, List<string> paginatedText)
    {
        this.path = path;
        this.fontSize = fontSize;
        title = name;
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
}
