using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BookRequestUI : MonoBehaviour
{
    public TMP_InputField inputName;
    public BookSystem bookSystem;

    public void SendBookRequest()
    {
        string bookName = inputName.text.ToLower();
        // send request to book system with bookName & online
        bookSystem.AddBookRequest(bookName);
    }
}
