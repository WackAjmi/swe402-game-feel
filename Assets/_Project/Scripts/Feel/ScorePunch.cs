using TMPro;
using UnityEngine;

/// Pops and color-flashes the score text when points are ADDED.
/// Only scales/recolors the text object — never writes the text —
/// so HUD keeps ownership of the score value (no ScoreHandledExternally needed).
[RequireComponent(typeof(TMP_Text))]
public class ScorePunch : MonoBehaviour
{
    [Header("Pop")]
    [Tooltip("Extra scale at the peak (0.35 = 135% size).")]
    [SerializeField, Range(0f, 1f)] private float punchScale = 0.35f;
    [Tooltip("Extra pop per brick in the current combo streak.")]
    [SerializeField] private float comboBonus = 0.05f;
    [SerializeField] private int maxComboSteps = 5;
    [Tooltip("Seconds until the text is fully back to normal.")]
    [SerializeField] private float duration = 0.3f;
    [Tooltip("How many wobbles while settling (0 = plain shrink back).")]
    [SerializeField] private float wobbles = 1f;

    [Header("Flash")]
    [SerializeField] private Color flashColor = new Color(1f, 0.85f, 0.2f); // gold

    private TMP_Text text;
    private Vector3 baseScale;
    private Color baseColor;
    private int lastScore;
    private float t = 1f;          // 0 = just triggered, 1 = finished
    private float currentPunch;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        baseScale = transform.localScale;
        baseColor = text.color;
    }

    private void OnEnable()
    {
        lastScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        GameEvents.ScoreChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        GameEvents.ScoreChanged -= OnScoreChanged;
        t = 1f;
        transform.localScale = baseScale;   // never leave it enlarged
        text.color = baseColor;
    }

    private void OnScoreChanged(int score)
    {
        // Only celebrate gains; the reset to 0 after the last life shouldn't "pop".
        if (score > lastScore)
        {
            int combo = GameManager.Instance != null ? GameManager.Instance.Combo : 0;
            currentPunch = punchScale + comboBonus * Mathf.Min(combo, maxComboSteps);
            t = 0f;   // restart (not add) -> rapid hits can't stack the scale
        }
        lastScore = score;
    }

    private void Update()
    {
        if (t >= 1f) return;

        t = Mathf.Min(t + Time.deltaTime / duration, 1f);
        float e = 1f - t;                                        // 1 -> 0
        float wobble = Mathf.Cos(t * Mathf.PI * 2f * wobbles);   // small overshoot
        float s = 1f + currentPunch * e * e * wobble;

        transform.localScale = baseScale * s;
        text.color = Color.Lerp(baseColor, flashColor, e);

        if (t >= 1f)
        {
            transform.localScale = baseScale;
            text.color = baseColor;
        }
    }
}