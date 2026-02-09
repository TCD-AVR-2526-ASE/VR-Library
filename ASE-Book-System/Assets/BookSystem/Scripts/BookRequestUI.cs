using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BookRequestUI : MonoBehaviour
{
    public TMP_InputField inputName;
    public BookSystem bookSystem;

    // takes an input from a text input field and sends a request to the book system
    // Name should match a case-insensitive exact or partial title from the Gutenberg library.
    public void SendBookRequest()
    {
        string bookName = inputName.text.ToLower();
        // send request to book system with bookName & online
        bookSystem.AddBookRequest(bookName);
    }
}
