using System.Collections;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class VoiceAiInteractionController : MonoBehaviour
{
    [Header("References")]
    public HoloLensMicrophoneRecorder microphoneRecorder;
    public AiSpeechHttpClient aiSpeechClient;
    public TextToSpeechManager textToSpeechManager;
    public SearchGameManager searchGameManager;

    [Header("Test Input")]
    public bool enableKeyboardTest = true;

    [Header("State")]
    public bool isBusy;

    private void Awake()
    {
        if (microphoneRecorder == null)
        {
            microphoneRecorder = FindObjectOfType<HoloLensMicrophoneRecorder>();
        }

        if (aiSpeechClient == null)
        {
            aiSpeechClient = FindObjectOfType<AiSpeechHttpClient>();
        }

        if (textToSpeechManager == null)
        {
            textToSpeechManager = FindObjectOfType<TextToSpeechManager>();
        }

        if (searchGameManager == null)
        {
            searchGameManager = FindObjectOfType<SearchGameManager>();
        }
    }

    private void Update()
    {
        if (!enableKeyboardTest)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            StartVoiceInteraction();
        }
#else
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartVoiceInteraction();
        }
#endif
    }

    public void StartVoiceInteraction()
    {
        if (isBusy)
        {
            Debug.Log("[VOICE AI] Already busy.");
            return;
        }

        if (textToSpeechManager != null && textToSpeechManager.IsSpeaking)
        {
            Debug.Log("[VOICE AI] TTS is currently speaking.");
            return;
        }

        StartCoroutine(VoiceInteractionRoutine());
    }

    private IEnumerator VoiceInteractionRoutine()
    {
        isBusy = true;

        Debug.Log("[VOICE AI] Start recording.");

        byte[] recordedAudio = null;

        yield return microphoneRecorder.RecordAudio(audioBytes =>
        {
            recordedAudio = audioBytes;
        });

        if (recordedAudio == null || recordedAudio.Length == 0)
        {
            Debug.LogError("[VOICE AI] Recording failed.");
            isBusy = false;
            yield break;
        }

        string gameStateJson = BuildGameStateJson();

        string aiAnswer = null;

        yield return aiSpeechClient.SendAudio(
            recordedAudio,
            gameStateJson,
            answer =>
            {
                aiAnswer = answer;
            }
        );

        if (!string.IsNullOrWhiteSpace(aiAnswer))
        {
            Debug.Log("[VOICE AI] Speaking answer: " + aiAnswer);

            if (textToSpeechManager != null)
            {
                textToSpeechManager.Speak(aiAnswer);
            }
        }

        isBusy = false;
    }

    private string BuildGameStateJson()
    {
        if (searchGameManager == null)
        {
            return "";
        }

        SearchContext context = searchGameManager.CurrentContext;

        return JsonUtility.ToJson(new SerializableSearchState
        {
            distance = context.distanceToTarget,
            angle = context.angleToTarget,
            horizontalAngle = context.signedHorizontalAngle,
            directionText = context.directionText,
            visible = context.isTargetVisible,
            state = context.state.ToString()
        });
    }

    [System.Serializable]
    private class SerializableSearchState
    {
        public float distance;
        public float angle;
        public float horizontalAngle;
        public string directionText;
        public bool visible;
        public string state;
    }
}