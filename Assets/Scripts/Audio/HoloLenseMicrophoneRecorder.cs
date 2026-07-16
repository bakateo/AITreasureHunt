using System.Collections;
using UnityEngine;

public class HoloLensMicrophoneRecorder : MonoBehaviour
{
    [Header("Recording")]
    public int recordingLengthSeconds = 4;
    public int sampleRate = 16000;

    [Header("Debug")]
    public bool logDebug = true;

    private AudioClip recordedClip;
    private string microphoneDevice;
    private bool isRecording;

    public bool IsRecording
    {
        get { return isRecording; }
    }

    private void Awake()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];

            if (logDebug)
            {
                Debug.Log("[MIC] Using microphone: " + microphoneDevice);
            }
        }
        else
        {
            Debug.LogWarning("[MIC] No microphone device found.");
        }
    }

    public IEnumerator RecordAudio(System.Action<byte[]> onRecordingFinished)
    {
        if (isRecording)
        {
            yield break;
        }

        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogError("[MIC] Cannot record, no microphone available.");
            onRecordingFinished?.Invoke(null);
            yield break;
        }

        isRecording = true;

        if (logDebug)
        {
            Debug.Log("[MIC] Recording started.");
        }

        recordedClip = Microphone.Start(
            microphoneDevice,
            false,
            recordingLengthSeconds,
            sampleRate
        );

        yield return new WaitForSeconds(recordingLengthSeconds);

        Microphone.End(microphoneDevice);

        if (logDebug)
        {
            Debug.Log("[MIC] Recording finished.");
        }

        isRecording = false;

        byte[] wavBytes = WavUtility.FromAudioClip(recordedClip);
        onRecordingFinished?.Invoke(wavBytes);
    }
}