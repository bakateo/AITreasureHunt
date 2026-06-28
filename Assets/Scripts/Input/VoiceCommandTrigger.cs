using System.Collections.Generic;
using UnityEngine;

#if UNITY_WSA || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
using System.Linq;
#endif

public class VoiceCommandTrigger : MonoBehaviour
{
    [Header("References")]
    public VoiceAiInteractionController voiceAiInteractionController;

#if UNITY_WSA || UNITY_EDITOR_WIN
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> commands;
#endif

    private void Awake()
    {
        if (voiceAiInteractionController == null)
        {
            voiceAiInteractionController = FindObjectOfType<VoiceAiInteractionController>();
        }
    }

    private void Start()
    {
#if UNITY_WSA || UNITY_EDITOR_WIN
        commands = new Dictionary<string, System.Action>
        {
            { "hilfe", TriggerVoiceInteraction },
            { "hey computer", TriggerVoiceInteraction },
            { "wo ist es", TriggerVoiceInteraction },
            { "nochmal", TriggerVoiceInteraction }
        };

        keywordRecognizer = new KeywordRecognizer(commands.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("[VOICE COMMAND] Keyword recognizer started.");
#else
        Debug.Log("[VOICE COMMAND] Not supported on this platform.");
#endif
    }

#if UNITY_WSA || UNITY_EDITOR_WIN
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        string command = args.text.ToLower();

        Debug.Log("[VOICE COMMAND] Recognized: " + command);

        if (commands.ContainsKey(command))
        {
            commands[command].Invoke();
        }
    }
#endif

    private void TriggerVoiceInteraction()
    {
        if (voiceAiInteractionController == null)
        {
            Debug.LogError("[VOICE COMMAND] VoiceAiInteractionController missing.");
            return;
        }

        voiceAiInteractionController.StartVoiceInteraction();
    }

    private void OnDestroy()
    {
#if UNITY_WSA || UNITY_EDITOR_WIN
        if (keywordRecognizer != null)
        {
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;

            if (keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }

            keywordRecognizer.Dispose();
        }
#endif
    }
}