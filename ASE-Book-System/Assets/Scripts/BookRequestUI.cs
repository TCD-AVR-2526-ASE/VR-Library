using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BookRequestUI : MonoBehaviour
{
    public TMP_InputField inputName;
    public BookSystem bookSystem;

    public void SendBookRequest(bool online)
    {
        string bookName = inputName.text;
        // send request to book system with bookName & online
        Tuple<string, bool> bookRequest = new (bookName, online);
        bookSystem.AddBookRequest(bookRequest);
    }
}
