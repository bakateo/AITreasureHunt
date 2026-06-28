using UnityEngine;
using System;

public class LookAroundManager : MonoBehaviour
{
    [Header("References")]
    public PlayerTrackingProvider playerTracking;
    public SearchGameManager searchGameManager;
    public TextToSpeechManager textToSpeech;

    [Header("Look Around Settings")]
    public bool startOnPlay = true;
    public float directionTolerance = 12f;
    public float startDelayAfterComplete = 1.0f;

    [Header("Startup Delay")]
    public float activationDelay = 0.75f;

    [Header("Debug UI")]
    public bool showDebugOverlay = false;

    public bool HasLookedForward { get; private set; }
    public bool HasLookedLeft { get; private set; }
    public bool HasLookedRight { get; private set; }
    public bool HasLookedBack { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsComplete { get; private set; }

    public LookAroundDirection CurrentRequiredDirection { get; private set; }
    public event Action<LookAroundDirection> OnDirectionCompleted;

    public Vector3 InitialForward
    {
        get { return initialForward; }
    }

    public int CompletedTargetCount
    {
        get { return completedTargetCount; }
    }

    private Vector3 initialForward;
    private float phaseStartTime;
    private float completeTime = -1f;
    private bool hasStartedSearchGame = false;
    private int completedTargetCount = 0;

    private void Awake()
    {
        if (playerTracking == null)
        {
            playerTracking = GetComponent<PlayerTrackingProvider>();
        }

        if (searchGameManager == null)
        {
            searchGameManager = GetComponent<SearchGameManager>();
        }

        if(textToSpeech == null)
        {
            textToSpeech = FindObjectOfType<TextToSpeechManager>();
        }
    }

    private void Start()
    {
        if (startOnPlay)
        {
            BeginLookAroundPhase();
        }
    }

    private void Update()
    {
        if (IsComplete && !hasStartedSearchGame)
        {
            TryStartSearchGameAfterDelay();
            return;
        }

        if (!IsActive)
        {
            return;
        }

        // Wichtig: verhindert, dass der erste Kreis direkt im ersten Frame abgeschlossen wird.
        if (Time.time < phaseStartTime + activationDelay)
        {
            return;
        }

        UpdateLookProgress();
        CheckCompletion();
    }

    public void BeginLookAroundPhase()
    {
        if (playerTracking == null)
        {
            Debug.LogError("LookAroundManager: PlayerTrackingProvider fehlt.");
            return;
        }

        if (textToSpeech != null)
        {
            textToSpeech.Speak("Hey User! Schau dich langsam im Raum um.");
        }
        Vector3 forward = Vector3.ProjectOnPlane(playerTracking.Forward, Vector3.up);

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        initialForward = forward.normalized;

        HasLookedForward = false;
        HasLookedLeft = false;
        HasLookedRight = false;
        HasLookedBack = false;

        completedTargetCount = 0;

        CurrentRequiredDirection = LookAroundDirection.Front;

        IsActive = true;
        IsComplete = false;
        hasStartedSearchGame = false;
        completeTime = -1f;
        phaseStartTime = Time.time;

        Debug.Log("Umschau-Phase gestartet: Folge den leuchtenden Kreisen.");
    }

    private void UpdateLookProgress()
    {
        Vector3 currentForward = Vector3.ProjectOnPlane(playerTracking.Forward, Vector3.up);

        if (currentForward.sqrMagnitude < 0.001f)
        {
            return;
        }

        currentForward.Normalize();

        float angleFromStart = Vector3.SignedAngle(
            initialForward,
            currentForward,
            Vector3.up
        );

        // Reihenfolge egal: Nutzer kann vorne, links, rechts oder hinten starten.
        if (!HasLookedForward && IsAngleForDirection(angleFromStart, LookAroundDirection.Front))
        {
            CompleteDirection(LookAroundDirection.Front);
        }

        if (!HasLookedLeft && IsAngleForDirection(angleFromStart, LookAroundDirection.Left))
        {
            CompleteDirection(LookAroundDirection.Left);
        }

        if (!HasLookedRight && IsAngleForDirection(angleFromStart, LookAroundDirection.Right))
        {
            CompleteDirection(LookAroundDirection.Right);
        }

        if (!HasLookedBack && IsAngleForDirection(angleFromStart, LookAroundDirection.Back))
        {
            CompleteDirection(LookAroundDirection.Back);
        }

        UpdateNextRequiredDirection();
    }

    private void CompleteDirection(LookAroundDirection direction)
    {
        switch (direction)
        {
            case LookAroundDirection.Front:
                HasLookedForward = true;
                break;

            case LookAroundDirection.Left:
                HasLookedLeft = true;
                break;

            case LookAroundDirection.Right:
                HasLookedRight = true;
                break;

            case LookAroundDirection.Back:
                HasLookedBack = true;
                break;
        }

        completedTargetCount++;
        OnDirectionCompleted?.Invoke(direction);

        Debug.Log("Richtung abgeschlossen: " + direction + " (" + completedTargetCount + "/4)");
    }

    private void UpdateNextRequiredDirection()
    {
        if (!HasLookedForward)
        {
            CurrentRequiredDirection = LookAroundDirection.Front;
            return;
        }

        if (!HasLookedLeft)
        {
            CurrentRequiredDirection = LookAroundDirection.Left;
            return;
        }

        if (!HasLookedBack)
        {
            CurrentRequiredDirection = LookAroundDirection.Back;
            return;
        }

        if (!HasLookedRight)
        {
            CurrentRequiredDirection = LookAroundDirection.Right;
            return;
        }
    }

    private bool IsAngleForDirection(float angle, LookAroundDirection direction)
    {
        switch (direction)
        {
            case LookAroundDirection.Front:
                return Mathf.Abs(angle) <= directionTolerance;

            case LookAroundDirection.Left:
                return angle <= -90f + directionTolerance &&
                       angle >= -90f - directionTolerance;

            case LookAroundDirection.Right:
                return angle >= 90f - directionTolerance &&
                       angle <= 90f + directionTolerance;

            case LookAroundDirection.Back:
                return Mathf.Abs(angle) >= 180f - directionTolerance;

            default:
                return false;
        }
    }

    private void CheckCompletion()
    {
        if (completedTargetCount >= 4)
        {
            IsComplete = true;
            IsActive = false;
            completeTime = Time.time;

            Debug.Log("Umschau-Phase abgeschlossen. Alle 4 Targets wurden angesehen.");
        }
    }

    private void TryStartSearchGameAfterDelay()
    {
        if (completeTime < 0f)
        {
            return;
        }

        if (Time.time < completeTime + startDelayAfterComplete)
        {
            return;
        }

        hasStartedSearchGame = true;

        if (searchGameManager != null)
        {
            searchGameManager.StartNewRound();
        }

        Debug.Log("Suchspiel wurde nach Umschau-Phase gestartet.");
    }

    public bool IsDirectionCompleted(LookAroundDirection direction)
    {
        switch (direction)
        {
            case LookAroundDirection.Front:
                return HasLookedForward;

            case LookAroundDirection.Left:
                return HasLookedLeft;

            case LookAroundDirection.Right:
                return HasLookedRight;

            case LookAroundDirection.Back:
                return HasLookedBack;

            default:
                return false;
        }
    }

    private void OnGUI()
    {
        if (!showDebugOverlay)
        {
            return;
        }

        if (!IsActive && !IsComplete)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(10, 310, 480, 250), GUI.skin.box);

        GUILayout.Label("Raum-Orientierung");
        GUILayout.Space(8);

        GUILayout.Label("Angesehene Targets: " + completedTargetCount + " / 4");

        if (IsActive)
        {
            GUILayout.Label("KI: Schau dich langsam im Raum um.");
            GUILayout.Label("Folge den leuchtenden Kreisen.");
        }
        else if (IsComplete && !hasStartedSearchGame)
        {
            GUILayout.Label("KI: Perfekt, ich habe genug gesehen.");
            GUILayout.Label("Das Suchspiel startet gleich.");
        }

        GUILayout.Space(8);

        GUILayout.Label(GetCheckText(HasLookedForward) + " Vorne angesehen");
        GUILayout.Label(GetCheckText(HasLookedLeft) + " Links angesehen");
        GUILayout.Label(GetCheckText(HasLookedRight) + " Rechts angesehen");
        GUILayout.Label(GetCheckText(HasLookedBack) + " Hinten angesehen");

        GUILayout.EndArea();
    }

    private string GetCheckText(bool value)
    {
        return value ? "[x]" : "[ ]";
    }
}