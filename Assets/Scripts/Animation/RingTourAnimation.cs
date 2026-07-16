using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingTourAnimation : MonoBehaviour
{
    [Header("Flying Object")]
    public Transform flyingObject;

    [Header("Ring Targets")]
    public List<LookAroundTargetVisual> ringTargets = new List<LookAroundTargetVisual>();

    [Header("Movement")]
    public float moveDuration = 0.45f;
    public float arcHeight = 0.25f;

    [Header("Offset")]
    [Tooltip("Kleiner Offset, damit das Objekt nicht exakt im Ring steckt.")]
    public Vector3 localOffsetAtRing = new Vector3(0f, 0f, -0.15f);

    [Header("Visibility")]
    public bool showObjectOnStart = true;
    public bool hideObjectWhenDone = true;

    [Header("Debug")]
    public bool logDebug = true;

    private Coroutine runningMove;

    public void ShowAtDirection(LookAroundDirection direction)
    {
        if (flyingObject == null)
        {
            Debug.LogWarning("[RingTour] Flying Object fehlt.");
            return;
        }

        LookAroundTargetVisual target = FindRingTarget(direction);

        if (target == null)
        {
            Debug.LogWarning("[RingTour] Kein RingTarget für Richtung gefunden: " + direction);
            return;
        }

        flyingObject.gameObject.SetActive(true);
        flyingObject.position = GetTargetPosition(target.transform);
        FaceCamera();

        if (logDebug)
        {
            Debug.Log("[RingTour] Objekt bei Richtung platziert: " + direction);
        }
    }
    
    public void ShowAtDirectionDelayed(LookAroundDirection direction)
    {
        if (runningMove != null)
        {
            StopCoroutine(runningMove);
        }

        runningMove = StartCoroutine(ShowAtDirectionDelayedRoutine(direction));
    }

    private IEnumerator ShowAtDirectionDelayedRoutine(LookAroundDirection direction)
    {
        // Warten, bis LookAroundVisualController die Ringe positioniert hat
        yield return null;
        yield return new WaitForEndOfFrame();

        ShowAtDirection(direction);

        runningMove = null;
    }

    public void MoveToDirection(LookAroundDirection direction)
    {
        if (flyingObject == null)
        {
            Debug.LogWarning("[RingTour] Flying Object fehlt.");
            return;
        }

        LookAroundTargetVisual target = FindRingTarget(direction);

        if (target == null)
        {
            Debug.LogWarning("[RingTour] Kein RingTarget für Richtung gefunden: " + direction);
            return;
        }

        flyingObject.gameObject.SetActive(true);

        Vector3 targetPosition = GetTargetPosition(target.transform);

        if (runningMove != null)
        {
            StopCoroutine(runningMove);
        }

        runningMove = StartCoroutine(MoveObjectRoutine(flyingObject.position, targetPosition, direction));
    }

    public void HideObject()
    {
        if (flyingObject != null && hideObjectWhenDone)
        {
            flyingObject.gameObject.SetActive(false);
        }
    }

    private LookAroundTargetVisual FindRingTarget(LookAroundDirection direction)
    {
        foreach (LookAroundTargetVisual target in ringTargets)
        {
            if (target == null)
            {
                continue;
            }

            if (target.direction == direction)
            {
                return target;
            }
        }

        return null;
    }

    private Vector3 GetTargetPosition(Transform ringTransform)
    {
        Vector3 position = ringTransform.position;

        position += ringTransform.right * localOffsetAtRing.x;
        position += ringTransform.up * localOffsetAtRing.y;
        position += ringTransform.forward * localOffsetAtRing.z;

        return position;
    }

    private IEnumerator MoveObjectRoutine(Vector3 from, Vector3 to, LookAroundDirection direction)
    {
        if (logDebug)
        {
            Debug.Log("[RingTour] Bewege Objekt zu Richtung: " + direction);
        }

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / moveDuration);
            float smoothT = t * t * (3f - 2f * t);

            Vector3 position = Vector3.Lerp(from, to, smoothT);

            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
            position += Vector3.up * arc;

            flyingObject.position = position;
            FaceCamera();

            yield return null;
        }

        flyingObject.position = to;
        FaceCamera();

        runningMove = null;
    }

    private void FaceCamera()
    {
        if (flyingObject == null || Camera.main == null)
        {
            return;
        }

        Vector3 directionToCamera = Camera.main.transform.position - flyingObject.position;

        if (directionToCamera.sqrMagnitude < 0.001f)
        {
            return;
        }

        flyingObject.rotation = Quaternion.LookRotation(-directionToCamera.normalized, Vector3.up);
    }
}