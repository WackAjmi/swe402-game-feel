using UnityEngine;

/// Plays a distinct sound per game event. Each sound gets its own AudioSource
/// (so pitch changes don't bleed into each other) plus a cooldown, so rapid repeats stay balanced.
public class SfxPlayer : MonoBehaviour
{
    [System.Serializable]
    public class Sfx
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.7f;
        [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
        [Tooltip("Minimum seconds between two plays of this sound.")]
        public float cooldown = 0.04f;
        [System.NonSerialized] public AudioSource source;
        [System.NonSerialized] public float lastTime = -1f;
    }

    [SerializeField] private Sfx paddleHit;
    [SerializeField] private Sfx brickBreak;
    [SerializeField] private Sfx ballLost;
    [SerializeField] private Sfx wallHit;   // optional, keep it quiet

    [Header("Brick combo pitch (rises until the ball touches the paddle)")]
    [SerializeField] private float pitchPerCombo = 0.06f;
    [SerializeField] private float maxComboPitch = 1.5f;

    private void Awake()
    {
        foreach (var s in new[] { paddleHit, brickBreak, ballLost, wallHit })
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        GameEvents.BallHitPaddle  += OnPaddle;
        GameEvents.BallHitWall    += OnWall;
        GameEvents.BrickDestroyed += OnBrick;
        GameEvents.BallLost       += OnLost;
    }

    private void OnDisable()
    {
        GameEvents.BallHitPaddle  -= OnPaddle;
        GameEvents.BallHitWall    -= OnWall;
        GameEvents.BrickDestroyed -= OnBrick;
        GameEvents.BallLost       -= OnLost;
    }

    private void OnPaddle(ImpactInfo _) => Play(paddleHit, 1f);
    private void OnWall(ImpactInfo _)   => Play(wallHit, 1f);
    private void OnLost()               => Play(ballLost, 1f);
    private void OnBrick(ImpactInfo info) =>
        Play(brickBreak, Mathf.Min(1f + (info.Combo - 1) * pitchPerCombo, maxComboPitch));

    private void Play(Sfx s, float basePitch)
    {
        if (s == null || s.clip == null || s.source == null) return;
        if (Time.time - s.lastTime < s.cooldown) return;
        s.lastTime = Time.time;
        s.source.pitch = basePitch + Random.Range(-s.pitchJitter, s.pitchJitter);
        s.source.PlayOneShot(s.clip, s.volume);
    }
}