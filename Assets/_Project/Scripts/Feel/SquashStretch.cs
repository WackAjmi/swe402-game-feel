using UnityEngine;

/// Squash & stretch (+ paddle recoil) on a VISUAL child only.
/// The collider and Rigidbody live on the parent, so physics never changes.
/// Spring-based: every hit SETS the deformation (never adds to it), so rapid
/// repeated hits can't accumulate, and the spring always settles back to the original scale.
public class SquashStretch : MonoBehaviour
{
    public enum Mode { Paddle, Ball }

    [SerializeField] private Mode mode = Mode.Paddle;

    [Header("Deformation")]
    [Tooltip("How much the visual squashes on impact (0.3 = 30%).")]
    [SerializeField, Range(0f, 0.6f)] private float squashAmount = 0.3f;
    [Tooltip("Paddle only: how far the visual dips down on impact, in world units.")]
    [SerializeField] private float recoilDistance = 0.12f;

    [Header("Spring (how it settles back)")]
    [Tooltip("Higher = snaps back faster.")]
    [SerializeField] private float stiffness = 350f;
    [Tooltip("Lower = more wobble/overshoot (bouncier). Higher = settles calmly.")]
    [SerializeField] private float damping = 14f;

    [Header("Ball only: which hits deform it")]
    [SerializeField] private bool reactToWalls = true;
    [SerializeField] private bool reactToBricks = true;

    private Vector3 baseScale, basePos;
    private Vector2 scaleOffset, scaleVel;   // deformation relative to base scale
    private float posOffset, posVel;         // recoil in world units

    private void Awake()
    {
        baseScale = transform.localScale;
        basePos = transform.localPosition;
    }

    private void OnEnable()
    {
        GameEvents.BallHitPaddle  += OnPaddleHit;
        GameEvents.BallHitWall    += OnWallHit;
        GameEvents.BrickDestroyed += OnBrickHit;
    }

    private void OnDisable()
    {
        GameEvents.BallHitPaddle  -= OnPaddleHit;
        GameEvents.BallHitWall    -= OnWallHit;
        GameEvents.BrickDestroyed -= OnBrickHit;

        // Never leave the visual deformed.
        scaleOffset = scaleVel = Vector2.zero;
        posOffset = posVel = 0f;
        transform.localScale = baseScale;
        transform.localPosition = basePos;
    }

    private void OnPaddleHit(ImpactInfo info)
    {
        if (mode == Mode.Paddle)
        {
            // Wider + flatter, and dip down like a trampoline.
            scaleOffset = new Vector2(squashAmount * 0.6f, -squashAmount);
            scaleVel = Vector2.zero;
            posOffset = -recoilDistance;
            posVel = 0f;
        }
        else SquashAlong(info.Normal);
    }

    private void OnWallHit(ImpactInfo info)  { if (mode == Mode.Ball && reactToWalls)  SquashAlong(info.Normal); }
    private void OnBrickHit(ImpactInfo info) { if (mode == Mode.Ball && reactToBricks) SquashAlong(info.Normal); }

    /// Flatten along the impact direction, bulge sideways.
    private void SquashAlong(Vector2 normal)
    {
        bool verticalHit = Mathf.Abs(normal.y) >= Mathf.Abs(normal.x);
        scaleOffset = verticalHit
            ? new Vector2( squashAmount, -squashAmount)   // hit floor/ceiling: wide & flat
            : new Vector2(-squashAmount,  squashAmount);  // hit side wall: tall & thin
        scaleVel = Vector2.zero;
    }

    private void Update()
    {
        // Clamp dt so a frame hitch can't make the spring explode.
        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);

        // Damped spring pulling the offsets back to zero (semi-implicit Euler).
        scaleVel   += (-stiffness * scaleOffset - damping * scaleVel) * dt;
        scaleOffset += scaleVel * dt;
        posVel     += (-stiffness * posOffset - damping * posVel) * dt;
        posOffset  += posVel * dt;

        scaleOffset.x = Mathf.Clamp(scaleOffset.x, -0.8f, 0.8f);
        scaleOffset.y = Mathf.Clamp(scaleOffset.y, -0.8f, 0.8f);

        transform.localScale = new Vector3(
            baseScale.x * (1f + scaleOffset.x),
            baseScale.y * (1f + scaleOffset.y),
            baseScale.z);

        // Parent (Paddle) is scaled, so convert world-unit recoil to local units.
        float parentScaleY = transform.parent ? transform.parent.lossyScale.y : 1f;
        transform.localPosition = basePos + Vector3.up * (posOffset / parentScaleY);
    }
}