using UnityEngine;

[System.Serializable]
public struct SearchContext
{
    public Vector3 playerPosition;
    public Vector3 playerForward;
    public Vector3 targetPosition;

    public float distanceToTarget;
    public float angleToTarget;
    public float signedHorizontalAngle;

    public string directionText;

    public bool isTargetVisible;
    public bool isLookingCloseToTarget;

    public SearchGameState state;
}