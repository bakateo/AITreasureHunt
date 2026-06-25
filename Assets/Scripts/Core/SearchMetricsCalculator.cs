using UnityEngine;

public static class SearchMetricsCalculator
{
    public static SearchContext Calculate(
        Vector3 playerPosition,
        Vector3 playerForward,
        Vector3 targetPosition,
        bool isTargetVisible,
        SearchGameState state,
        float lookAngleThreshold = 12f)
    {
        Vector3 safeForward = playerForward.sqrMagnitude > 0.001f
            ? playerForward.normalized
            : Vector3.forward;

        Vector3 toTarget = targetPosition - playerPosition;

        float distance = toTarget.magnitude;

        Vector3 directionToTarget = distance > 0.001f
            ? toTarget.normalized
            : safeForward;

        float angle = Vector3.Angle(safeForward, directionToTarget);

        Vector3 flatForward = Vector3.ProjectOnPlane(safeForward, Vector3.up);
        Vector3 flatDirectionToTarget = Vector3.ProjectOnPlane(directionToTarget, Vector3.up);

        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        if (flatDirectionToTarget.sqrMagnitude < 0.001f)
        {
            flatDirectionToTarget = flatForward;
        }

        float signedHorizontalAngle = Vector3.SignedAngle(
            flatForward.normalized,
            flatDirectionToTarget.normalized,
            Vector3.up
        );

        string directionText = GetDirectionText(signedHorizontalAngle);

        return new SearchContext
        {
            playerPosition = playerPosition,
            playerForward = safeForward,
            targetPosition = targetPosition,
            distanceToTarget = distance,
            angleToTarget = angle,
            signedHorizontalAngle = signedHorizontalAngle,
            directionText = directionText,
            isTargetVisible = isTargetVisible,
            isLookingCloseToTarget = angle <= lookAngleThreshold,
            state = state
        };
    }

    public static string GetDirectionText(float signedHorizontalAngle)
    {
        if (signedHorizontalAngle > 10f)
        {
            return "rechts";
        }

        if (signedHorizontalAngle < -10f)
        {
            return "links";
        }

        return "geradeaus";
    }
}