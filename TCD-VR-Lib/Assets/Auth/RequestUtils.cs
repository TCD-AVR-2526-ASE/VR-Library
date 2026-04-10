using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Network Request Utility - Enhanced with root array serialization support.
/// </summary>
public class RequestUtils : MonoBehaviour
{
    public static RequestUtils Instance { get; private set; }

    [Header("Core Configuration")]
    [SerializeField] public static string _baseIP = "172.20.10.4";
    [SerializeField] private string _baseUrl = "http://" + _baseIP + ":6201/";
    [SerializeField] private int _timeout = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum RequestType { GET, POST, PUT, DELETE }

    /// <summary>
    /// Primary entry point for sending asynchronous network requests.
    /// </summary>
    public async Task<TResponse> SendRequestAsync<TResponse, TBody>(string endpoint, RequestType method, TBody bodyData)
    {
        string fullUrl = CombineUrl(_baseUrl, endpoint);
        
        // Special handling for array/list to overcome JsonUtility limitations.
        string jsonBody = null;
        if (bodyData != null)
        {
            jsonBody = JsonHelper.ToJson(bodyData);
        }

        using (UnityWebRequest request = CreateRequest(fullUrl, method, jsonBody))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[RequestUtils] Request Failed: {request.error} | URL: {fullUrl}");
                throw new Exception(request.error);
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[RequestUtils] Response Received: {responseText}");

            try
            {
                // Note: JsonHelper could be extended to handle array responses as well if needed.
                return JsonUtility.FromJson<TResponse>(responseText);
            }
            catch (Exception e)
            {
                Debug.LogError($"[RequestUtils] Parsing Failed: {e.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Overload for requests without a request body.
    /// </summary>
    public async Task<TResponse> SendRequestAsync<TResponse>(string endpoint, RequestType method)
    {
        return await SendRequestAsync<TResponse, object>(endpoint, method, null);
    }

    private UnityWebRequest CreateRequest(string url, RequestType method, string jsonBody)
    {
        UnityWebRequest request = new UnityWebRequest(url, method.ToString());
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = _timeout;

        if (!string.IsNullOrEmpty(jsonBody) && (method == RequestType.POST || method == RequestType.PUT))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Content-Type", "application/json");
        }

        // Attach Authorization header if logged in
        string authHeader = AuthSession.GetAuthorizationHeader();
        if (!string.IsNullOrEmpty(authHeader))
        {
            request.SetRequestHeader("Authorization", authHeader);
        }

        return request;
    }

    private string CombineUrl(string baseUri, string endpoint)
    {
        if (string.IsNullOrEmpty(baseUri)) return endpoint;
        return baseUri.TrimEnd('/') + "/" + endpoint.TrimStart('/');
    }

    /// <summary>
    /// Internal Helper: Addresses JsonUtility's inability to serialize Lists/Arrays as root elements.
    /// </summary>
    public static class JsonHelper
    {
        public static string ToJson<T>(T obj)
        {
            // If the object is a List, wrap it before serialization.
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                return ToJsonArray(obj);
            }
            return JsonUtility.ToJson(obj);
        }

        private static string ToJsonArray<T>(T list)
        {
            // Wrap the list into a temporary object and then extract the raw array string.
            string wrapperJson = JsonUtility.ToJson(new Wrapper<T> { items = list });
            // Convert {"items":[...]} -> [...]
            int startIndex = wrapperJson.IndexOf('[');
            int endIndex = wrapperJson.LastIndexOf(']');
            return wrapperJson.Substring(startIndex, endIndex - startIndex + 1);
        }

        [Serializable]
        private class Wrapper<T> { public T items; }
    }
}