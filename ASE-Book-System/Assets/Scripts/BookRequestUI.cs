using TMPro;
using UnityEngine;

public class BookRequestUI : MonoBehaviour
{
    public TMP_InputField inputName;
    public BookSystem bookSystem;

    public async void SendBookRequest(bool online)
    {
        string bookName = inputName.text;
        // send request to book system with bookName & online
    }
}
