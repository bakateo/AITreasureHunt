using System.Globalization;
using UnityEngine;

public class AIHintServiceMock : MonoBehaviour
{
    [Header("Hint Timing")]
    public float hintCooldown = 3.0f;

    [Header("Debug Output")]
    [TextArea(8, 20)]
    public string lastPrompt;

    [TextArea(2, 5)]
    public string lastMockHint;

    private float nextHintTime = 0f;

    public bool TryCreateHint(SearchContext context, out string prompt, out string hint)
    {
        prompt = "";
        hint = "";

        if (Time.time < nextHintTime)
        {
            return false;
        }

        nextHintTime = Time.time + hintCooldown;

        prompt = BuildPrompt(context);
        hint = GenerateMockHint(context);

        lastPrompt = prompt;
        lastMockHint = hint;

        return true;
    }

    public string BuildPrompt(SearchContext context)
    {
        string distance = context.distanceToTarget.ToString("0.00", CultureInfo.InvariantCulture);
        string angle = context.angleToTarget.ToString("0", CultureInfo.InvariantCulture);
        string horizontalAngle = context.signedHorizontalAngle.ToString("0", CultureInfo.InvariantCulture);
        string visible = context.isTargetVisible ? "ja" : "nein";

        return
            $@"Du bist ein freundlicher KI-Assistent in einem HoloLens-Suchspiel.
            Der Spieler sucht ein virtuelles Objekt im Raum.

            Aktuelle Spieldaten:
            - Entfernung zum Ziel: {distance} Meter
            - Winkelabweichung zur Blickrichtung: {angle} Grad
            - Horizontale Abweichung: {horizontalAngle} Grad
            - Zielrichtung aus Spielersicht: {context.directionText}
            - Objekt sichtbar: {visible}
            - Spielzustand: {context.state}

            Aufgabe:
            Gib genau einen kurzen, natürlichen Hinweis auf Deutsch.
            Sprich nicht technisch.
            Verwende keine Koordinaten und keine Gradzahl.
            Maximal ein Satz.";
    }

    public string GenerateMockHint(SearchContext context)
    {
        if (context.state == SearchGameState.Found)
        {
            return "Sehr gut, du hast das Objekt gefunden.";
        }

        if (context.isTargetVisible &&
            context.distanceToTarget < 1.2f &&
            context.isLookingCloseToTarget)
        {
            return "Du bist direkt davor, bestätige jetzt den Fund.";
        }

        if (context.distanceToTarget > 5.0f)
        {
            return "Du bist noch weit entfernt, bewege dich weiter durch den Raum.";
        }

        if (Mathf.Abs(context.signedHorizontalAngle) > 45f)
        {
            return "Dreh dich deutlich nach " + context.directionText + ".";
        }

        if (Mathf.Abs(context.signedHorizontalAngle) > 15f)
        {
            return "Schau etwas weiter nach " + context.directionText + ".";
        }

        if (context.distanceToTarget > 2.0f)
        {
            return "Die Richtung stimmt, geh weiter geradeaus.";
        }

        if (!context.isTargetVisible)
        {
            return "Du bist schon näher dran, such in deiner aktuellen Umgebung.";
        }

        return "Sehr nah, das Objekt sollte jetzt sichtbar sein.";
    }
}
