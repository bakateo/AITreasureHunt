using UnityEngine;

public class SearchDebugOverlay : MonoBehaviour
{
    public SearchGameManager gameManager;
    public AIHintServiceMock aiHintService;
    public HudHotColdFeedback hudFeedback;

    public bool showOverlay = false;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = GetComponent<SearchGameManager>();
        }

        if (aiHintService == null)
        {
            aiHintService = GetComponent<AIHintServiceMock>();
        }

        if (hudFeedback == null)
        {
            hudFeedback = GetComponent<HudHotColdFeedback>();
        }
    }

    private void OnGUI()
    {
        if (!showOverlay || gameManager == null)
        {
            return;
        }

        SearchContext context = gameManager.CurrentContext;

        GUILayout.BeginArea(new Rect(10, 10, 560, 280), GUI.skin.box);

        GUILayout.Label("HoloLens Search Prototype - Debug");
        GUILayout.Space(8);

        GUILayout.Label("State: " + context.state);
        GUILayout.Label("Distanz: " + context.distanceToTarget.ToString("0.00") + " m");
        GUILayout.Label("Winkel: " + context.angleToTarget.ToString("0") + " Grad");
        GUILayout.Label("Horizontale Abweichung: " + context.signedHorizontalAngle.ToString("0") + " Grad");
        GUILayout.Label("Richtung: " + context.directionText);
        GUILayout.Label("Objekt sichtbar: " + context.isTargetVisible);
        GUILayout.Label("Blick nah am Ziel: " + context.isLookingCloseToTarget);

        if (hudFeedback != null)
        {
            GUILayout.Space(8);
            GUILayout.Label("Hot Intensity: " + hudFeedback.HotIntensity.ToString("0.00"));
            GUILayout.Label("Cold Intensity: " + hudFeedback.ColdIntensity.ToString("0.00"));
        }

        if (aiHintService != null)
        {
            GUILayout.Space(8);
            GUILayout.Label("Letzter Mock-Hinweis:");
            GUILayout.Label(aiHintService.lastMockHint);
        }

        GUILayout.Space(8);
        GUILayout.Label("Controls: MRTK Simulator bewegen | F Fund | R Reset");

        GUILayout.EndArea();
    }
}