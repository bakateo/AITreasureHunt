using UnityEngine;
using UnityEngine.UI;

public class LookAroundArrowHud : MonoBehaviour
{
    [Header("References")]
    public PlayerTrackingProvider playerTracking;
    public LookAroundManager lookAroundManager;

    [Header("UI")]
    public RectTransform arrowRect;
    public CanvasGroup arrowCanvasGroup;
    public Image arrowImage;

    [Header("Settings")]
    public float screenEdgePadding = 120f;
    public float fadeInAngle = 8f;
    public float maxAlpha = 0.85f;
    public float rotationOffset = -90f;

    private void Awake()
    {
        if (playerTracking == null)
        {
            playerTracking = FindObjectOfType<PlayerTrackingProvider>();
        }

        if (lookAroundManager == null)
        {
            lookAroundManager = FindObjectOfType<LookAroundManager>();
        }

        if (arrowRect == null)
        {
            arrowRect = GetComponent<RectTransform>();
        }

        if (arrowCanvasGroup == null)
        {
            arrowCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (arrowImage == null)
        {
            arrowImage = GetComponent<Image>();
        }
    }

    private void Update()
    {
        if (playerTracking == null ||
            lookAroundManager == null ||
            arrowRect == null ||
            arrowCanvasGroup == null)
        {
            return;
        }

        if (!lookAroundManager.IsActive)
        {
            arrowCanvasGroup.alpha = Mathf.Lerp(
                arrowCanvasGroup.alpha,
                0f,
                Time.deltaTime * 8f
            );
            return;
        }

        Vector3 targetDirection = GetCurrentTargetDirection();
        Vector3 playerForward = Vector3.ProjectOnPlane(playerTracking.Forward, Vector3.up).normalized;
        Vector3 playerRight = Vector3.Cross(Vector3.up, playerForward).normalized;

        Vector3 flatTargetDirection = Vector3.ProjectOnPlane(targetDirection, Vector3.up).normalized;

        float signedAngle = Vector3.SignedAngle(
            playerForward,
            flatTargetDirection,
            Vector3.up
        );

        bool lookingAtTarget = Mathf.Abs(signedAngle) <= fadeInAngle;

        float targetAlpha = lookingAtTarget ? 0f : maxAlpha;

        arrowCanvasGroup.alpha = Mathf.Lerp(
            arrowCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * 6f
        );

        PositionArrowOnScreenEdge(signedAngle);
        RotateArrow(signedAngle);
    }

    private Vector3 GetCurrentTargetDirection()
    {
        Vector3 initialForward = Vector3.ProjectOnPlane(
            lookAroundManager.InitialForward,
            Vector3.up
        ).normalized;

        if (initialForward.sqrMagnitude < 0.001f)
        {
            initialForward = Vector3.forward;
        }

        Vector3 right = Vector3.Cross(Vector3.up, initialForward).normalized;

        switch (lookAroundManager.CurrentRequiredDirection)
        {
            case LookAroundDirection.Front:
                return initialForward;

            case LookAroundDirection.Left:
                return -right;

            case LookAroundDirection.Right:
                return right;

            case LookAroundDirection.Back:
                return -initialForward;

            default:
                return initialForward;
        }
    }

    private void PositionArrowOnScreenEdge(float signedAngle)
    {
        float x = 0f;
        float y = 0f;

        if (Mathf.Abs(signedAngle) > 120f)
        {
            y = -Screen.height / 2f + screenEdgePadding;
            x = Mathf.Sign(signedAngle) * (Screen.width / 4f);
        }
        else
        {
            x = Mathf.Sign(signedAngle) * (Screen.width / 2f - screenEdgePadding);
            y = 0f;
        }

        Vector2 targetPosition = new Vector2(x, y);

        arrowRect.anchoredPosition = Vector2.Lerp(
            arrowRect.anchoredPosition,
            targetPosition,
            Time.deltaTime * 8f
        );
    }

    private void RotateArrow(float signedAngle)
    {
        float zRotation;

        if (Mathf.Abs(signedAngle) > 120f)
        {
            zRotation = 180f;
        }
        else if (signedAngle > 0f)
        {
            zRotation = -90f;
        }
        else
        {
            zRotation = 90f;
        }

        arrowRect.localRotation = Quaternion.Lerp(
            arrowRect.localRotation,
            Quaternion.Euler(0f, 0f, zRotation + rotationOffset),
            Time.deltaTime * 8f
        );
    }
}