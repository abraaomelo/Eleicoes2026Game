using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Velocidades — unidades por segundo")]

    [SerializeField, Min(0f)]
    private float roadSpeed = 3f;

    [SerializeField, Min(0f)]
    private float roadsideSpeed = 2f;

    [SerializeField, Min(0f)]
    private float obstacleSpeed = 3f;

    [SerializeField, Min(0f)]
    private float proposalSpeed = 3f;

    public float RoadSpeed => roadSpeed;
    public float RoadsideSpeed => roadsideSpeed;
    public float ObstacleSpeed => obstacleSpeed;
    public float ProposalSpeed => proposalSpeed;

    // Temporário: mantém os scripts antigos funcionando
    // enquanto substituímos o sistema de inimigos.
    public float ScrollSpeed => obstacleSpeed;
}


