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

    [Header("Movimento")]
    [SerializeField] private GameController gameController;
    [SerializeField] private float despawnY = -6f;

    public ItemKind Kind => kind;
    public ProposalSide Side => side;
    public int Points => points;
    public string ProposalName => proposalName;
    public string ProposalDescription => proposalDescription;

    // O gerador fornecerá esta referência ao criar o objeto.
    public void Initialize(GameController controller)
    {
        gameController = controller;
    }

    private void Update()
    {
        if (gameController == null)
            return;

        float speed = kind == ItemKind.Obstacle
            ? gameController.ObstacleSpeed
            : gameController.ProposalSpeed;

        transform.position += Vector3.down
            * speed
            * Time.deltaTime;

        if (transform.position.y < despawnY)
        {
            Destroy(gameObject);
        }
    }
}


