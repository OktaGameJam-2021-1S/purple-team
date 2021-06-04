using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HandshakeTest : MonoBehaviour
{
    public string URL = "https://okta-team-purple.herokuapp.com/";

    public void TestHandShake()
    {
        StartCoroutine(SendWebRequest());
    }

    private IEnumerator SendWebRequest()
    {
        UnityWebRequestAsyncOperation operation = UnityWebRequest.Get(URL).SendWebRequest();
        yield return operation;

        Debug.Log("HandShake!");
        Debug.Log(operation.webRequest.downloadHandler.text);
    }
}
