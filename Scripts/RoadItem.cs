using UnityEngine;

public class RoadItem : MonoBehaviour
{
    public enum ItemKind
    {
        Obstacle,
        Proposal
    }

    public enum ProposalSide
    {
        Left,
        Right
    }

    [Header("Identificação")]
    [SerializeField] private ItemKind kind;

    [Header("Dados da proposta")]
    [SerializeField] private string proposalName;

    [TextArea]
    [SerializeField] private string proposalDescription;

    [SerializeField] private ProposalSide side;
    [SerializeField, Min(1)] private int points = 1;

    [Header("Som exclusivo desta proposta")]
    [Tooltip("Vazio: usa o som padrão configurado no GameAudio.")]
    [SerializeField] private AudioClip collectionSoundOverride;

    [SerializeField, Range(0f, 1f)]
    private float collectionOverrideVolume = 0.5f;

    [Header("Movimento")]
    [SerializeField] private GameController gameController;
    [SerializeField] private float despawnY = -6f;

    private bool collected;

    public ItemKind Kind => kind;
    public ProposalSide Side => side;
    public int Points => points;
    public string ProposalName => proposalName;
    public string ProposalDescription => proposalDescription;

    public void Initialize(GameController controller)
    {
        gameController = controller;
    }

    private void Update()
    {
        if (gameController == null || !gameController.IsRunning)
            return;

        float speed = kind == ItemKind.Obstacle
            ? gameController.ObstacleSpeed
            : gameController.ProposalSpeed;

        transform.position += Vector3.down
            * speed
            * Time.deltaTime;

        if (transform.position.y < despawnY)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected ||
            gameController == null ||
            !gameController.IsRunning)
        {
            return;
        }

        PlayerLaneMovement player =
            other.GetComponentInParent<PlayerLaneMovement>();

        if (player == null)
            return;

        if (kind == ItemKind.Obstacle)
        {
            gameController.EndRun();
            return;
        }

        if (gameController.CollectProposal(this))
        {
            collected = true;

            if (GameAudio.Instance != null)
            {
                GameAudio.Instance.PlayProposal(
                    side,
                    collectionSoundOverride,
                    collectionOverrideVolume
                );
            }

            Destroy(gameObject);
        }
    }
}


