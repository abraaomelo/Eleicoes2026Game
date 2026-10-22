using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public enum RunResult
    {
        None,
        Left,
        Right,
        Tie,
        NoProposals
    }

    [Header("Referências")]
    [SerializeField] private PlayerLaneMovement playerMovement;

    [Header("Velocidades")]
    [SerializeField, Min(0f)] private float roadSpeed = 3f;
    [SerializeField, Min(0f)] private float roadsideSpeed = 2f;
    [SerializeField, Min(0f)] private float obstacleSpeed = 3f;
    [SerializeField, Min(0f)] private float proposalSpeed = 3f;

    [Header("Contadores — acompanhe durante o Play")]
    [SerializeField] private int leftPoints;
    [SerializeField] private int rightPoints;
    [SerializeField] private int collectedProposals;

    [Header("Estado — acompanhe durante o Play")]
    [SerializeField] private bool isRunning;
    [SerializeField] private RunResult result;

    public bool IsRunning => isRunning;
    public int LeftPoints => leftPoints;
    public int RightPoints => rightPoints;
    public int CollectedProposals => collectedProposals;
    public RunResult Result => result;

    public float RoadSpeed => isRunning ? roadSpeed : 0f;
    public float RoadsideSpeed => isRunning ? roadsideSpeed : 0f;
    public float ObstacleSpeed => isRunning ? obstacleSpeed : 0f;
    public float ProposalSpeed => isRunning ? proposalSpeed : 0f;

    private void Awake()
    {
        leftPoints = 0;
        rightPoints = 0;
        collectedProposals = 0;
        result = RunResult.None;

        // Por enquanto, começa automaticamente.
        // Depois conectaremos a tela inicial.
        isRunning = true;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    public bool CollectProposal(RoadItem proposal)
    {
        if (!isRunning ||
            proposal == null ||
            proposal.Kind != RoadItem.ItemKind.Proposal)
        {
            return false;
        }

        int points = Mathf.Max(1, proposal.Points);

        if (proposal.Side == RoadItem.ProposalSide.Left)
            leftPoints += points;
        else
            rightPoints += points;

        collectedProposals++;

        Debug.Log(
            $"Coletou: {proposal.ProposalName} | " +
            $"Esquerda: {leftPoints} | Direita: {rightPoints} | " +
            $"Propostas: {collectedProposals}"
        );

        return true;
    }

    public void EndRun()
    {
        if (!isRunning)
            return;

        isRunning = false;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (collectedProposals == 0)
            result = RunResult.NoProposals;
        else if (leftPoints > rightPoints)
            result = RunResult.Left;
        else if (rightPoints > leftPoints)
            result = RunResult.Right;
        else
            result = RunResult.Tie;

        switch (result)
        {
            case RunResult.Left:
                Debug.Log("GAME OVER — Você é de esquerda!");
                break;

            case RunResult.Right:
                Debug.Log("GAME OVER — Você é de direita!");
                break;

            case RunResult.Tie:
                Debug.Log("GAME OVER — Isentão!");
                break;

            case RunResult.NoProposals:
                Debug.Log(
                    "GAME OVER — Nenhuma proposta! " +
                    "Easter egg da Campopiano."
                );
                break;
        }
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}


