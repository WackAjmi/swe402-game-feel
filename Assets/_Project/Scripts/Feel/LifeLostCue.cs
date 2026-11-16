using TMPro;
using UnityEngine;

/// Shakes + red-flashes the LIVES text when the ball is lost, so ball loss
/// is readable even with audio muted. Uses BallLost (not LivesChanged) because
/// losing the last life resets lives upward to the starting value.
[RequireComponent(typeof(TMP_Text))]
public class LifeLostCue : MonoBehaviour
{
    [SerializeField] private float duration = 0.45f;
    [Tooltip("Shake distance in UI pixels.")]
    [SerializeField] private float shakeDistance = 12f;
    [SerializeField] private float shakeSpeed = 50f;
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f);

    private TMP_Text text;
    private RectTransform rt;
    private Vector2 basePos;
    private Color baseColor;
    private float t = 1f;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        rt = (RectTransform)transform;
        basePos = rt.anchoredPosition;
        baseColor = text.color;
    }

    private void OnEnable()  => GameEvents.BallLost += OnBallLost;

    private void OnDisable()
    {
        GameEvents.BallLost -= OnBallLost;
        t = 1f;
        rt.anchoredPosition = basePos;
        text.color = baseColor;
    }

    private void OnBallLost() => t = 0f;   // restart, never stack

    private void Update()
    {
        if (t >= 1f) return;

        t = Mathf.Min(t + Time.deltaTime / duration, 1f);
        float e = 1f - t;
        float x = Mathf.Sin(t * duration * shakeSpeed) * shakeDistance * e;

        rt.anchoredPosition = basePos + new Vector2(x, 0f);
        text.color = Color.Lerp(baseColor, flashColor, e);

        if (t >= 1f)
        {
            rt.anchoredPosition = basePos;
            text.color = baseColor;
        }
    }
}