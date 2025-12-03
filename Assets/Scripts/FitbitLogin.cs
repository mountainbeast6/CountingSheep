using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Firebase.Auth;
using System.Linq;
public class FitbitLogin : MonoBehaviour
{
    private const string BackendStartFitbitUrl = "https://7bjmgv2xc2isaz7glkz7mzcibm0pbguh.lambda-url.us-east-2.on.aws/";

    [Serializable]
    private class StartFitbitResponse
    {
        public string authorizationUrl;
    }

    public void OnConnectFitbitClicked()
    {
        StartCoroutine(StartFitbitFlow());
    }

    private IEnumerator StartFitbitFlow()
    {
        var auth = FirebaseAuth.DefaultInstance;
        var user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No Firebase user signed in. Make sure Firebase Auth login runs first.");
            yield break;
        }

        // Firebase UID
        string firebaseUid = user.UserId;

        // Get Firebase ID token (JWT)
        var tokenTask = user.TokenAsync(true);
        while (!tokenTask.IsCompleted)
            yield return null;

        if (tokenTask.Exception != null)
        {
            Debug.LogError("Failed to get Firebase ID token: " + tokenTask.Exception);
            yield break;
        }

        string idToken = tokenTask.Result;
        Debug.Log("Got Firebase ID token, length: " + idToken.Length);

        // Now call backend and include UID
        yield return StartCoroutine(CallBackendStartFitbit(idToken, firebaseUid));
    }

    private IEnumerator CallBackendStartFitbit(string idToken, string firebaseUid)
    {
        // Include UID in JSON body
        var jsonBody = "{\"firebaseUid\": \"" + firebaseUid + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (var request = new UnityWebRequest(BackendStartFitbitUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            // Send identity proof
            request.SetRequestHeader("Authorization", "Bearer " + idToken);

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("Error calling backend /fitbit/start: " + request.error +
                               "\n" + request.downloadHandler.text);
                yield break;
            }

            string responseJson = request.downloadHandler.text;
            Debug.Log("Backend /fitbit/start response: " + responseJson);

            StartFitbitResponse resp;
            try
            {
                resp = JsonUtility.FromJson<StartFitbitResponse>(responseJson);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse backend response: " + e);
                yield break;
            }

            if (resp == null || string.IsNullOrEmpty(resp.authorizationUrl))
            {
                Debug.LogError("Backend response missing authorizationUrl");
                yield break;
            }

            Application.OpenURL(resp.authorizationUrl);
        }
    }
    [Serializable]
public class FitbitSleepWeekResponse
{
    public string startDate;
    public string endDate;
    public string raw; // if you keep raw as nested JSON string, or make a class for it
}

private const string BackendSleepWeekUrl =
    "https://twu7y3dkzh4h7y5fr6wiym7wxe0iaqvz.lambda-url.us-east-2.on.aws/";

public void OnGetWeekSleepClicked()
{
    StartCoroutine(GetWeekSleep());
}

private IEnumerator GetWeekSleep()
{
    var auth = FirebaseAuth.DefaultInstance;
    var user = auth.CurrentUser;

    if (user == null)
    {
        Debug.LogError("No Firebase user signed in.");
        yield break;
    }

    var tokenTask = user.TokenAsync(true);
    while (!tokenTask.IsCompleted)
        yield return null;

    if (tokenTask.Exception != null)
    {
        Debug.LogError("Failed to get Firebase ID token: " + tokenTask.Exception);
        yield break;
    }

    string idToken = tokenTask.Result;

    using (var request = new UnityWebRequest(BackendSleepWeekUrl, "POST"))
    {
        request.uploadHandler = new UploadHandlerRaw(new byte[0]);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + idToken);

        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.LogError("Error calling backend week sleep: " + request.error +
                           "\n" + request.downloadHandler.text);
            yield break;
        }

        string json = request.downloadHandler.text;
        Debug.Log("Week sleep response: " + json);
        var firebase = FindObjectOfType<FirebaseController>();
        if (firebase != null)
            {
                firebase.SyncFitbitSleepWeek(json);
        }
        else
            {
                Debug.LogError("FirebaseController not found in scene.");
        }
        // Optional: deserialize into a C# class if you define the structure
        // var resp = JsonUtility.FromJson<FitbitSleepWeekResponse>(json);
    }
}

}