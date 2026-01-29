using UnityEngine;
using TMPro;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System;
using echo17.EndlessBook;

public class BookRenderer : MonoBehaviour
{
    public TMP_Text textAreaLeft;
    public TMP_Text textAreaRight;
    public TMP_Text textAreaCover;
    public int maxCharPerPage = 500;
    public BookSystem bookSystem;
    public float titleFontSize = 20;

    string content;

    public void DisplayCurrent(Book book)
    {
        Tuple<string, string> pages = book.GetPageText();
        if (pages == null) return;
        textAreaLeft.text = pages.Item1;
        bool hasPage = pages.Item2 != null;
        textAreaRight.text = pages.Item2 ?? "";

        textAreaLeft.fontSize = book.fontSize;
        textAreaLeft.enableAutoSizing = false;
        textAreaRight.fontSize = book.fontSize;
        textAreaRight.enableAutoSizing = false;
        textAreaCover.fontSize = titleFontSize;
        textAreaCover.enableAutoSizing = false;
        textAreaCover.text = book.title;
    }

    //content = "Loading...";
    //Paginate();
    //textAreaLeft.fontSize = 10f;
    //textAreaLeft.enableAutoSizing = false;
    //textAreaRight.fontSize = 10f;
    //textAreaRight.enableAutoSizing = false;
    //ShowPage();

    // move to book system

    // duplicate pagination because test of local DB as well
    // remove one set & throw the other into the book data struct.
    //content = "Paginating...";
    //textAreaLeft.fontSize = 10f;
    //textAreaLeft.enableAutoSizing = false;
    //textAreaRight.fontSize = 10f;
    //textAreaRight.enableAutoSizing = false;

    //LoadText(book.path);
    //textAreaLeft.fontSize = 10f;
    //textAreaLeft.enableAutoSizing = false;
    //textAreaRight.fontSize = 10f;
    //textAreaRight.enableAutoSizing = false;
    //textAreaCover.fontSize = 20f;
    //textAreaCover.enableAutoSizing = false;
    //textAreaCover.text = book.title;
    //ShowPage();
}
