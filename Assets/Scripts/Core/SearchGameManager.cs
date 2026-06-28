using UnityEngine;
using System.Collections;

public class SearchGameManager : MonoBehaviour
{
    [Header("References")]
    public PlayerTrackingProvider playerTracking;
    public TargetController targetController;
    public AIHintServiceMock aiHintService;
    public HudHotColdFeedback hudFeedback;
    public DebugKeyboardInputProvider debugInput;

    [SerializeField]
    private SearchIntroAnimation introAnimation;

    [Header("Game Settings")]
    public bool autoStartOnPlay = false;
    public float lookAngleThreshold = 12.0f;
    public float foundDistance = 1.5f;

    [Header("Debug")]
    public bool logPromptsToConsole = true;

    public SearchGameState currentState = SearchGameState.Setup;

    public bool IsRoundActive { get; private set; }

    public SearchContext CurrentContext { get; private set; }

    private void Awake()
    {
        if (playerTracking == null)
        {
            playerTracking = GetComponent<PlayerTrackingProvider>();
        }

        if (targetController == null)
        {
            targetController = GetComponent<TargetController>();
        }

        if (aiHintService == null)
        {
            aiHintService = GetComponent<AIHintServiceMock>();
        }

        if (hudFeedback == null)
        {
            hudFeedback = GetComponent<HudHotColdFeedback>();
        }

        if (debugInput == null)
        {
            debugInput = GetComponent<DebugKeyboardInputProvider>();
        }
    }

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartCoroutine(StartSearchSequence());
        }
        else
        {
            currentState = SearchGameState.Setup;
            IsRoundActive = false;

            if (targetController != null)
            {
                targetController.SetVisible(false);
            }

            Debug.Log("SearchGameManager wartet auf Startsignal.");
        }
    }

    private void Update()
    {
        if (debugInput != null && debugInput.WasResetPressedThisFrame())
        {
            StartCoroutine(StartSearchSequence());
        }

        if (!IsRoundActive)
        {
            return;
        }

        if (targetController == null || playerTracking == null)
        {
            return;
        }

        if (currentState == SearchGameState.Found)
        {
            return;
        }

        UpdateContextAndFeedback();

        if (debugInput != null && debugInput.WasFoundPressedThisFrame())
        {
            TryConfirmFound();
        }
    }

    private bool isStartingSequence = false;

    public void StartNewRound()
    {
        if (isStartingSequence)
        {
            return;
        }

        StartCoroutine(StartSearchSequence());
    }

    private IEnumerator StartSearchSequence()
    {
        isStartingSequence = true;

        Debug.Log("[FLOW] Search sequence started");

        if (introAnimation != null)
        {
            Debug.Log("[FLOW] Playing intro animation");
            yield return introAnimation.PlayIntro();
        }
        else
        {
            Debug.LogWarning("[FLOW] Keine Intro Animation im SearchGameManager zugewiesen.");
        }

        StartActualSearchRound();

        isStartingSequence = false;
    }

    public void StartActualSearchRound()
    {
        IsRoundActive = true;
        currentState = SearchGameState.Searching;

        if (targetController != null)
        {
            targetController.InitializeTarget();
        }

        UpdateContextAndFeedback();

        Debug.Log("Neue Runde gestartet. Suche das versteckte Objekt.");
    }

    private void UpdateContextAndFeedback()
    {
        Vector3 playerPosition = playerTracking.Position;
        Vector3 playerForward = playerTracking.Forward;
        Vector3 targetPosition = targetController.TargetPosition;

        float distance = Vector3.Distance(playerPosition, targetPosition);

        targetController.UpdateVisibility(distance);

        currentState = targetController.IsVisible
            ? SearchGameState.TargetVisible
            : SearchGameState.Searching;

        CurrentContext = SearchMetricsCalculator.Calculate(
            playerPosition,
            playerForward,
            targetPosition,
            targetController.IsVisible,
            currentState,
            lookAngleThreshold
        );

        if (hudFeedback != null)
        {
            hudFeedback.UpdateFeedback(CurrentContext);
        }

        if (aiHintService != null)
        {
            bool createdHint = aiHintService.TryCreateHint(
                CurrentContext,
                out string prompt,
                out string hint
            );

            if (createdHint && logPromptsToConsole)
            {
                Debug.Log("KI-Prompt:\n" + prompt);
                Debug.Log("Mock-Hinweis:\n" + hint);
            }
        }
    }

    private void TryConfirmFound()
    {
        bool canFind =
            CurrentContext.isTargetVisible &&
            CurrentContext.isLookingCloseToTarget &&
            CurrentContext.distanceToTarget <= foundDistance;

        if (canFind)
        {
            currentState = SearchGameState.Found;

            if (targetController != null)
            {
                targetController.SetVisible(true);
            }

            SearchContext foundContext = CurrentContext;
            foundContext.state = SearchGameState.Found;
            CurrentContext = foundContext;

            Debug.Log("Gefunden! Das Suchobjekt wurde erfolgreich bestätigt.");
        }
        else
        {
            Debug.Log(
                "Noch nicht gefunden. Distanz: " +
                CurrentContext.distanceToTarget.ToString("0.00") +
                " m, Winkel: " +
                CurrentContext.angleToTarget.ToString("0") +
                " Grad."
            );
        }
    }
}