using UnityEngine;

public class PlayerRunMirror : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameController gameController;

    [Header("Ritmo da animação")]
    [Tooltip("Velocidade usada como referência para o intervalo.")]
    [SerializeField, Min(0.01f)]
    private float referenceSpeed = 3f;

    [Tooltip("Segundos entre inversões na velocidade de referência.")]
    [SerializeField, Min(0.01f)]
    private float flipInterval = 0.25f;

    [Tooltip("Menor intervalo permitido, mesmo em velocidades altas.")]
    [SerializeField, Min(0.01f)]
    private float minimumInterval = 0.08f;

    private float elapsed;
    private bool originalFlipX;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalFlipX = spriteRenderer.flipX;
    }

    private void Update()
    {
        if (spriteRenderer == null ||
            gameController == null ||
            !gameController.IsRunning)
        {
            return;
        }

        float speed = gameController.RoadSpeed;

        if (speed <= 0f)
            return;

        float interval = Mathf.Max(
            minimumInterval,
            flipInterval * referenceSpeed / speed
        );

        elapsed += Time.deltaTime;

        if (elapsed < interval)
            return;

        // Mantém o ritmo mesmo se um frame demorar mais.
        int flips = Mathf.FloorToInt(elapsed / interval);
        elapsed %= interval;

        if (flips % 2 != 0)
            spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    private void OnDisable()
    {
        elapsed = 0f;

        if (spriteRenderer != null)
            spriteRenderer.flipX = originalFlipX;
    }
}

