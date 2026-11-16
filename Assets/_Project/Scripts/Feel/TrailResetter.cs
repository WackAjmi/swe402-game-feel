using System.Collections;
using UnityEngine;

/// The ball teleports back to its start position when lost, which would draw a long
/// streak across the screen. This hides + clears the trail around that teleport.
[RequireComponent(typeof(TrailRenderer))]
public class TrailResetter : MonoBehaviour
{
    [Tooltip("How long the trail stays hidden after the ball is lost.")]
    [SerializeField] private float hideTime = 0.06f;

    private TrailRenderer trail;
    private Coroutine routine;

    private void Awake()     => trail = GetComponent<TrailRenderer>();
    private void OnEnable()  => GameEvents.BallLost += OnBallLost;

    private void OnDisable()
    {
        GameEvents.BallLost -= OnBallLost;
        if (routine != null) StopCoroutine(routine);
        routine = null;
        trail.emitting = true;
    }

    private void OnBallLost()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ResetTrail());
    }

    private IEnumerator ResetTrail()
    {
        trail.emitting = false;
        trail.Clear();
        yield return new WaitForSeconds(hideTime);   // let the teleport finish
        trail.Clear();
        trail.emitting = true;
        routine = null;
    }
}