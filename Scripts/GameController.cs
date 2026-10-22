using System;
using System.Collections.Generic;
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

    [Serializable]
    public class SpeedIncrease
    {
        [Tooltip("Distância total necessária para aplicar este aumento.")]
        [Min(0f)]
        public float afterMeters = 100f;

        [Tooltip("Valor adicionado à velocidade ao atingir essa distância.")]
        [Min(0f)]
        public float increaseBy = 0.5f;
    }

    [Header("Referências")]
    [SerializeField] private PlayerLaneMovement playerMovement;

    [Header("Velocidades iniciais — unidades por segundo")]
    [SerializeField, Min(0f)] private float roadSpeed = 3f;
    [SerializeField, Min(0f)] private float roadsideSpeed = 2f;
    [SerializeField, Min(0f)] private float obstacleSpeed = 3f;
    [SerializeField, Min(0f)] private float proposalSpeed = 3f;

    [Header("Distância")]
    [Tooltip("Quantos metros correspondem a uma unidade de movimento da rua.")]
    [SerializeField, Min(0.01f)]
    private float metersPerWorldUnit = 1f;

    [Header("Progressão — rua")]
    [SerializeField]
    private List<SpeedIncrease> roadProgression =
        new List<SpeedIncrease>();

    [Header("Progressão — laterais")]
    [SerializeField]
    private List<SpeedIncrease> roadsideProgression =
        new List<SpeedIncrease>();

    [Header("Progressão — obstáculos")]
    [SerializeField]
    private List<SpeedIncrease> obstacleProgression =
        new List<SpeedIncrease>();

    [Header("Progressão — propostas")]
    [SerializeField]
    private List<SpeedIncrease> proposalProgression =
        new List<SpeedIncrease>();

    [Header("Contadores — acompanhe durante o Play")]
    [SerializeField] private float distanceMeters;
    [SerializeField] private int leftPoints;
    [SerializeField] private int rightPoints;
    [SerializeField] private int collectedProposals;

    [Header("Estado — acompanhe durante o Play")]
    [SerializeField] private bool isRunning;
    [SerializeField] private RunResult result;

    public bool IsRunning => isRunning;

    public float DistanceMeters => distanceMeters;
    public int LeftPoints => leftPoints;
    public int RightPoints => rightPoints;
    public int CollectedProposals => collectedProposals;

    // Valor que exibiremos como "Propostas" no HUD.
    public int TotalProposalPoints => leftPoints + rightPoints;

    public RunResult Result => result;

    public float RoadSpeed => isRunning
        ? CalculateSpeed(roadSpeed, roadProgression)
        : 0f;

    public float RoadsideSpeed => isRunning
        ? CalculateSpeed(roadsideSpeed, roadsideProgression)
        : 0f;

    public float ObstacleSpeed => isRunning
        ? CalculateSpeed(obstacleSpeed, obstacleProgression)
        : 0f;

    public float ProposalSpeed => isRunning
        ? CalculateSpeed(proposalSpeed, proposalProgression)
        : 0f;

    private void Awake()
    {
        distanceMeters = 0f;
        leftPoints = 0;
        rightPoints = 0;
        collectedProposals = 0;

        result = RunResult.None;
        isRunning = true;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        distanceMeters += RoadSpeed
            * metersPerWorldUnit
            * Time.deltaTime;
    }

    private float CalculateSpeed(
        float initialSpeed,
        List<SpeedIncrease> progression)
    {
        float speed = Mathf.Max(0f, initialSpeed);

        if (progression == null)
            return speed;

        foreach (SpeedIncrease step in progression)
        {
            if (step == null)
                continue;

            if (distanceMeters >= Mathf.Max(0f, step.afterMeters))
            {
                speed += Mathf.Max(0f, step.increaseBy);
            }
        }

        return speed;
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
            $"Propostas: {TotalProposalPoints} | " +
            $"Esquerda: {leftPoints} | Direita: {rightPoints}"
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

        Debug.Log(
            $"GAME OVER — {result} | " +
            $"Propostas: {TotalProposalPoints} | " +
            $"Distância: {Mathf.FloorToInt(distanceMeters)}m"
        );
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}


