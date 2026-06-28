using System.Collections;
using UnityEngine;

public class SearchIntroAnimation : MonoBehaviour
{
    [Header("Objects")]
    public Transform spoon;
    public Transform target;

    [Header("Placement In Front Of User")]
    public bool placeInFrontOfCamera = true;
    public float distanceInFront = 1.5f;
    public float heightOffset = -0.15f;
    public Vector3 targetLocalOffset = Vector3.zero;
    public Vector3 spoonLocalOffset = new Vector3(0.35f, 0.25f, 0f);

    [Header("Animation")]
    public int hitCount = 3;
    public float hitDistance = 0.12f;
    public float hitSpeed = 0.12f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;

    private Vector3 spoonStartPos;
    private Quaternion spoonStartRot;

    public IEnumerator PlayIntro()
    {
        Debug.Log("[INTRO] PlayIntro wurde gestartet");

        if (spoon == null)
        {
            Debug.LogError("[INTRO] Spoon ist nicht gesetzt.");
            yield break;
        }

        if (target == null)
        {
            Debug.LogError("[INTRO] Target ist nicht gesetzt.");
            yield break;
        }

        if (placeInFrontOfCamera)
        {
            PlaceObjectsInFrontOfCamera();
        }
        spoon.gameObject.SetActive(true);
        spoonStartPos = spoon.position;
        spoonStartRot = spoon.rotation;

        spoon.gameObject.SetActive(true);
        target.gameObject.SetActive(true);

        for (int i = 0; i < hitCount; i++)
        {
            yield return MoveToTarget();

            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            yield return MoveBack();
            yield return new WaitForSeconds(0.08f);
        }

        spoon.gameObject.SetActive(false);
        target.gameObject.SetActive(false);
    }

    private void PlaceObjectsInFrontOfCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("[INTRO] Keine Main Camera gefunden.");
            return;
        }

        Vector3 cameraPosition = cam.transform.position;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = cam.transform.forward.normalized;
        }

        Vector3 right = cam.transform.right;
        Vector3 up = Vector3.up;

        Vector3 basePosition =
            cameraPosition +
            forward * distanceInFront +
            up * heightOffset;

        target.position =
            basePosition +
            right * targetLocalOffset.x +
            up * targetLocalOffset.y +
            forward * targetLocalOffset.z;

        spoon.position =
            basePosition +
            right * spoonLocalOffset.x +
            up * spoonLocalOffset.y +
            forward * spoonLocalOffset.z;

        target.rotation = Quaternion.LookRotation(forward, Vector3.up);
        spoon.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private IEnumerator MoveToTarget()
    {
        Vector3 directionFromTargetToSpoon =
            (spoon.position - target.position).normalized;

        if (directionFromTargetToSpoon.sqrMagnitude < 0.001f)
        {
            directionFromTargetToSpoon = Vector3.up;
        }

        Vector3 hitPosition =
            target.position +
            directionFromTargetToSpoon * hitDistance;

        yield return Move(spoon.position, hitPosition);
    }

    private IEnumerator MoveBack()
    {
        yield return Move(spoon.position, spoonStartPos);
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / hitSpeed;
            spoon.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        spoon.position = to;
    }
}