using UnityEngine;

/// Colored particle burst where a brick is destroyed.
/// Reuses ONE ParticleSystem via Emit(), so bursts never pile up as new GameObjects.
[RequireComponent(typeof(ParticleSystem))]
public class BrickBurstFX : MonoBehaviour
{
    [SerializeField] private int particleCount = 14;
    [SerializeField, Range(0f, 1f)] private float brighten = 0.25f;

    private ParticleSystem ps;

    private void Awake()     => ps = GetComponent<ParticleSystem>();
    private void OnEnable()  => GameEvents.BrickDestroyed += OnBrickDestroyed;
    private void OnDisable() => GameEvents.BrickDestroyed -= OnBrickDestroyed;

    private void OnBrickDestroyed(ImpactInfo info)
    {
        var emit = new ParticleSystem.EmitParams
        {
            position = info.Center,            // brick center (copied before Destroy)
            applyShapeToPosition = true,       // still spread over the Shape area
            startColor = Color.Lerp(info.Color, Color.white, brighten)
        };
        ps.Emit(emit, particleCount);
    }
}