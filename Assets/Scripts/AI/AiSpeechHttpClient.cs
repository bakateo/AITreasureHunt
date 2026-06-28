using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AiSpeechHttpClient : MonoBehaviour
{
    [Header("Server")]
    public string serverUrl = "http://192.168.178.42:5000/speech";

    [Header("Debug")]
    public bool logDebug = true;

    [System.Serializable]
    public class AiSpeechResponse
    {
        public string answer;
        public string error;
    }

    public IEnumerator SendAudio(
        byte[] wavBytes,
        string gameStateJson,
        System.Action<string> onAnswerReceived
    )
    {
        if (wavBytes == null || wavBytes.Length == 0)
        {
            Debug.LogError("[AI HTTP] No audio data to send.");
            yield break;
        }

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(wavBytes);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "audio/wav");

        if (!string.IsNullOrWhiteSpace(gameStateJson))
        {
            request.SetRequestHeader("X-Game-State", gameStateJson);
        }

        if (logDebug)
        {
            Debug.Log("[AI HTTP] Sending audio to: " + serverUrl);
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[AI HTTP] Request failed: " + request.error);
            Debug.LogError("[AI HTTP] Response: " + request.downloadHandler.text);
            yield break;
        }

        string json = request.downloadHandler.text;

        if (logDebug)
        {
            Debug.Log("[AI HTTP] Server response: " + json);
        }

        AiSpeechResponse response = JsonUtility.FromJson<AiSpeechResponse>(json);

        if (response == null)
        {
            Debug.LogError("[AI HTTP] Could not parse server response.");
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(response.error))
        {
            Debug.LogError("[AI HTTP] Server error: " + response.error);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(response.answer))
        {
            Debug.LogWarning("[AI HTTP] No answer in server response.");
            yield break;
        }

        onAnswerReceived?.Invoke(response.answer);
    }
}