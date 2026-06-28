using UnityEngine;

#if UNITY_EDITOR_WIN
using System.Diagnostics;
#endif

#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
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

    [Header("Editor Windows TTS")]
    public bool speakInWindowsEditor = true;
    public int editorVolume = 100;
    public int editorRate = 0;

    [Header("Fallback")]
    public bool logSpeechIfUnsupported = true;

    [Header("Spam Protection")]
    public float minTimeBetweenSpeech = 2.0f;

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

        VoiceInformation germanVoice = Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices
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

#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
        _ = SpeakOnUwpAsync(text);
#elif UNITY_EDITOR_WIN
        SpeakInEditorWithPowerShell(text);
#else
        if (logSpeechIfUnsupported)
        {
            UnityEngine.Debug.Log("[TTS] " + text);
        }
#endif
    }

#if UNITY_EDITOR_WIN
    private void SpeakInEditorWithPowerShell(string text)
    {
        if (!speakInWindowsEditor)
        {
            UnityEngine.Debug.Log("[TTS Preview] " + text);
            return;
        }

        string safeText = text.Replace("'", "''");

        string command =
            "Add-Type -AssemblyName System.Speech; " +
            "$speak = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
            "$speak.Volume = " + editorVolume + "; " +
            "$speak.Rate = " + editorRate + "; " +
            "$speak.Speak('" + safeText + "');";

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "powershell";
        startInfo.Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"";
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;

        Process.Start(startInfo);
    }
#endif

#if ENABLE_WINMD_SUPPORT && !UNITY_EDITOR
    private async Task SpeakOnUwpAsync(string text)
    {
        if (uwpSynthesizer == null || mediaPlayer == null)
        {
            return;
        }

        if (interruptCurrentSpeech)
        {
            mediaPlayer.Pause();
            mediaPlayer.Source = null;
        }

        SpeechSynthesisStream stream = await uwpSynthesizer.SynthesizeTextToStreamAsync(text);

        mediaPlayer.Source = MediaSource.CreateFromStream(
            stream,
            stream.ContentType
        );

        mediaPlayer.Play();
    }
#endif
}