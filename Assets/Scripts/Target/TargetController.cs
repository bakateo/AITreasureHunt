using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [Header("Target")]
    public GameObject targetObject;

    [Header("Placement")]
    public Transform fixedSpawnPoint;
    public bool useRandomSpawnPoint = false;
    public List<Transform> randomSpawnPoints = new List<Transform>();

    [Header("Visibility")]
    public bool hideOnStart = true;
    public float visibleDistance = 2.0f;

    public bool IsVisible { get; private set; }

    public Vector3 TargetPosition
    {
        get
        {
            if (targetObject != null)
            {
                return targetObject.transform.position;
            }

            return transform.position;
        }
    }

    public void InitializeTarget()
    {
        PlaceTarget();
        SetVisible(!hideOnStart);
    }

    public void PlaceTarget()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("TargetController: Kein targetObject zugewiesen.");
            return;
        }

        Transform chosenSpawn = null;

        if (useRandomSpawnPoint && randomSpawnPoints != null && randomSpawnPoints.Count > 0)
        {
            List<Transform> validSpawnPoints = randomSpawnPoints.FindAll(point => point != null);

            if (validSpawnPoints.Count > 0)
            {
                int randomIndex = Random.Range(0, validSpawnPoints.Count);
                chosenSpawn = validSpawnPoints[randomIndex];
            }
        }

        if (chosenSpawn == null && fixedSpawnPoint != null)
        {
            chosenSpawn = fixedSpawnPoint;
        }

        if (chosenSpawn != null)
        {
            targetObject.transform.SetPositionAndRotation(
                chosenSpawn.position,
                chosenSpawn.rotation
            );
        }
    }

    public void UpdateVisibility(float distanceToPlayer)
    {
        bool shouldBeVisible = distanceToPlayer <= visibleDistance;
        SetVisible(shouldBeVisible);
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;

        if (targetObject != null && targetObject.activeSelf != visible)
        {
            targetObject.SetActive(visible);
        }
    }
}