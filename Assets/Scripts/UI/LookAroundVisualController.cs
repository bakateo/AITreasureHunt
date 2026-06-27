using System.Collections.Generic;
using UnityEngine;

public class LookAroundVisualController : MonoBehaviour
{
    [Header("References")]
    public PlayerTrackingProvider playerTracking;
    public LookAroundManager lookAroundManager;

    [Header("Targets")]
    public List<LookAroundTargetVisual> targets = new List<LookAroundTargetVisual>();

    [Header("Placement")]
    public float radius = 2.0f;
    public float heightOffset = 0.0f;
    public bool followPlayerPosition = true;

    [Header("Billboard")]
    public bool facePlayer = true;

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
    }

    private void Update()
    {
        if (playerTracking == null || lookAroundManager == null)
        {
            return;
        }

        PositionTargetsAroundPlayer();
        UpdateTargetStates();
    }

    private void PositionTargetsAroundPlayer()
    {
        Vector3 center = playerTracking.Position + Vector3.up * heightOffset;

        Vector3 forward = Vector3.ProjectOnPlane(lookAroundManager.InitialForward, Vector3.up).normalized;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        foreach (LookAroundTargetVisual target in targets)
        {
            if (target == null)
            {
                continue;
            }

            Vector3 direction = GetWorldDirection(target.direction, forward, right);
            Vector3 targetPosition = center + direction * radius;

            targetPosition.y = center.y;
            target.transform.position = targetPosition;

            if (facePlayer)
            {
                Vector3 toPlayer = playerTracking.Position - target.transform.position;

                if (toPlayer.sqrMagnitude > 0.001f)
                {
                    target.transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                }
            }
        }
    }

    private void UpdateTargetStates()
    {
        foreach (LookAroundTargetVisual target in targets)
        {
            if (target == null)
            {
                continue;
            }

            bool completed = lookAroundManager.IsDirectionCompleted(target.direction);
            bool active = lookAroundManager.IsActive &&
                        !completed &&
                        target.direction == lookAroundManager.CurrentRequiredDirection;

            if (!lookAroundManager.IsActive && !lookAroundManager.IsComplete)
            {
                target.gameObject.SetActive(false);
                continue;
            }

            if (completed && target.IsReadyToHide())
            {
                target.gameObject.SetActive(false);
                continue;
            }

            if (!target.gameObject.activeSelf)
            {
                target.gameObject.SetActive(true);
            }

            target.SetState(active, completed);
        }
    }

    private Vector3 GetWorldDirection(
        LookAroundDirection direction,
        Vector3 forward,
        Vector3 right)
    {
        switch (direction)
        {
            case LookAroundDirection.Front:
                return forward;

            case LookAroundDirection.Right:
                return right;

            case LookAroundDirection.Left:
                return -right;

            case LookAroundDirection.Back:
                return -forward;

            default:
                return forward;
        }
    }
}