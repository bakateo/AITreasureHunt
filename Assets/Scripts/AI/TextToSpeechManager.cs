using UnityEngine;

#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
#endif

public class TextToSpeechManager : MonoBehaviour
{
    [Header("General")]
    public bool enableSpeech = true;

    [Tooltip("Wenn true, wird ein aktueller Satz durch den neuen ersetzt.")]
    public bool interruptCurrentSpeech = true;

    [Header("Fallback")]
    public bool logSpeechIfUnsupported = true;

    [Header("Spam Protection")]
    public float minTimeBetweenSpeech = 2.0f;

    public bool IsSpeaking { get; private set; }

    private string lastSpokenText = "";
    private float lastSpeakTime = -999f;

#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
    private Windows.Media.SpeechSynthesis.SpeechSynthesizer uwpSynthesizer;
    private MediaPlayer mediaPlayer;
#endif

    private void Awake()
    {
#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
        uwpSynthesizer = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();

        VoiceInformation germanVoice =
            Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
                .FirstOrDefault(voice => voice.Language.StartsWith("de"));

        if (germanVoice != null)
        {
            uwpSynthesizer.Voice = germanVoice;
        }

        mediaPlayer = new MediaPlayer();
#endif
    }

    public void Speak(string text)
    {
        if (!enableSpeech)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        text = text.Trim();

        if (Time.time < lastSpeakTime + minTimeBetweenSpeech)
        {
            return;
        }

        if (text == lastSpokenText)
        {
            return;
        }

        lastSpokenText = text;
        lastSpeakTime = Time.time;
        IsSpeaking = true;

#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
        _ = SpeakOnUwpAsync(text);
#else
        if (logSpeechIfUnsupported)
        {
            Debug.Log("[TTS unsupported in this environment] " + text);
        }

        StartCoroutine(ResetSpeakingAfterDelay(text));
#endif
    }

#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
    private async Task SpeakOnUwpAsync(string text)
    {
        if (uwpSynthesizer == null || mediaPlayer == null)
        {
            IsSpeaking = false;
            return;
        }

        if (interruptCurrentSpeech)
        {
            mediaPlayer.Pause();
            mediaPlayer.Source = null;
        }

        SpeechSynthesisStream stream =
            await uwpSynthesizer.SynthesizeTextToStreamAsync(text).AsTask();

        mediaPlayer.Source = MediaSource.CreateFromStream(
            stream,
            stream.ContentType
        );

        mediaPlayer.Play();

        float estimatedDuration = Mathf.Clamp(text.Length * 0.06f, 1.0f, 10.0f);
        await Task.Delay((int)(estimatedDuration * 1000f));

        IsSpeaking = false;
    }
#endif

    private System.Collections.IEnumerator ResetSpeakingAfterDelay(string text)
    {
        float estimatedDuration = Mathf.Clamp(text.Length * 0.06f, 1.0f, 10.0f);
        yield return new WaitForSeconds(estimatedDuration);
        IsSpeaking = false;
    }
}