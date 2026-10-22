using System;
using System.Collections.Generic;
using UnityEngine;

public class RunSpawner : MonoBehaviour
{
    [Serializable]
    public class SpawnOption
    {
        public RoadItem prefab;

        [Min(0f)]
        public float weight = 1f;
    }

    [Header("Referências")]
    [SerializeField] private GameController gameController;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Ritmo")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 1.5f;
    [SerializeField, Min(0f)] private float minimumEntryGap = 2f;

    [Header("Chance relativa de cada categoria")]
    [SerializeField, Min(0f)] private float obstacleWeight = 1f;
    [SerializeField, Min(0f)] private float proposalWeight = 1f;

    [Header("Obstáculos")]
    [SerializeField] private SpawnOption[] obstacles;

    [Header("Propostas")]
    [SerializeField] private SpawnOption[] proposals;

    private readonly List<RoadItem> activeItems =
        new List<RoadItem>();

    private readonly List<Transform> availablePoints =
        new List<Transform>();

    private Transform lastPoint;
    private float timer;

    private void OnEnable()
    {
        timer = 0f;
        lastPoint = null;
    }

    private void Update()
    {
        if (gameController == null || !gameController.IsRunning)
            return;

        timer += Time.deltaTime;

        if (timer < Mathf.Max(0.1f, spawnInterval))
            return;

        // Uma tentativa por intervalo, sem acumular uma fila de spawns.
        timer = 0f;
        TrySpawn();
    }

    private bool IsValid(
        SpawnOption option,
        RoadItem.ItemKind expectedKind)
    {
        return option != null
            && option.prefab != null
            && option.prefab.Kind == expectedKind
            && option.weight > 0f
            && !float.IsNaN(option.weight)
            && !float.IsInfinity(option.weight);
    }

    private float GetTotalWeight(
        SpawnOption[] options,
        RoadItem.ItemKind expectedKind)
    {
        float total = 0f;

        if (options == null)
            return total;

        foreach (SpawnOption option in options)
        {
            if (IsValid(option, expectedKind))
                total += option.weight;
        }

        return total;
    }

    private RoadItem PickPrefab(
        SpawnOption[] options,
        RoadItem.ItemKind expectedKind,
        float totalWeight)
    {
        float draw = UnityEngine.Random.value * totalWeight;
        RoadItem selected = null;

        foreach (SpawnOption option in options)
        {
            if (!IsValid(option, expectedKind))
                continue;

            selected = option.prefab;

            if (draw < option.weight)
                break;

            draw -= option.weight;
        }

        return selected;
    }

    private bool HasEntrySpace(Transform point)
    {
        foreach (RoadItem item in activeItems)
        {
            // Mantém um intervalo vertical na entrada,
            // mesmo entre objetos de lanes diferentes.
            if (Mathf.Abs(item.transform.position.y - point.position.y)
                < minimumEntryGap)
            {
                return false;
            }
        }

        return true;
    }

    private void TrySpawn()
    {
        activeItems.RemoveAll(item => item == null);

        availablePoints.Clear();

        if (spawnPoints == null)
            return;

        foreach (Transform point in spawnPoints)
        {
            if (point == null || point == lastPoint)
                continue;

            if (HasEntrySpace(point))
                availablePoints.Add(point);
        }

        if (availablePoints.Count == 0)
            return;

        float obstacleTotal = GetTotalWeight(
            obstacles,
            RoadItem.ItemKind.Obstacle
        );

        float proposalTotal = GetTotalWeight(
            proposals,
            RoadItem.ItemKind.Proposal
        );

        float obstacleChance =
            obstacleTotal > 0f && gameController.ObstacleSpeed > 0f
                ? Mathf.Max(0f, obstacleWeight)
                : 0f;

        float proposalChance =
            proposalTotal > 0f && gameController.ProposalSpeed > 0f
                ? Mathf.Max(0f, proposalWeight)
                : 0f;

        float categoryTotal = obstacleChance + proposalChance;

        if (categoryTotal <= 0f)
            return;

        bool spawnObstacle =
            UnityEngine.Random.value * categoryTotal < obstacleChance;

        RoadItem prefab = spawnObstacle
            ? PickPrefab(
                obstacles,
                RoadItem.ItemKind.Obstacle,
                obstacleTotal)
            : PickPrefab(
                proposals,
                RoadItem.ItemKind.Proposal,
                proposalTotal);

        if (prefab == null)
            return;

        Transform selectedPoint = availablePoints[
    UnityEngine.Random.Range(0, availablePoints.Count)
];

        RoadItem item = Instantiate(
            prefab,
            selectedPoint.position,
            Quaternion.identity
        );

        item.Initialize(gameController);

        activeItems.Add(item);
        lastPoint = selectedPoint;



    }
}

