using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

    [Header("Ritmo — segundos")]
    [FormerlySerializedAs("spawnInterval")]
    [SerializeField, Min(0.1f)]
    private float minSpawnInterval = 1.2f;

    [SerializeField, Min(0.1f)]
    private float maxSpawnInterval = 1.8f;

    [Tooltip("Máxima mudança do intervalo entre duas aparições.")]
    [SerializeField, Min(0.01f)]
    private float maxIntervalChange = 0.25f;

    [Header("Espaçamento")]
    [SerializeField, Min(0f)]
    private float minimumEntryGap = 2f;

    [SerializeField, Min(0.05f)]
    private float blockedRetryInterval = 0.12f;

    [Header("Chance relativa de cada categoria")]
    [SerializeField, Min(0f)]
    private float obstacleWeight = 1f;

    [SerializeField, Min(0f)]
    private float proposalWeight = 1f;

    [Header("Variedade")]
    [Tooltip("Quantidade de propostas recentes que serão evitadas.")]
    [SerializeField, Min(0)]
    private int recentProposalsToAvoid = 2;

    [Tooltip("Quantidade de obstáculos recentes que serão evitados.")]
    [SerializeField, Min(0)]
    private int recentObstaclesToAvoid = 1;

    [Tooltip("Limite seguido de obstáculos ou propostas, se ambos disponíveis.")]
    [SerializeField, Min(1)]
    private int maxCategoryStreak = 2;

    [Header("Obstáculos")]
    [SerializeField] private SpawnOption[] obstacles;

    [Header("Propostas")]
    [SerializeField] private SpawnOption[] proposals;

    private readonly List<RoadItem> activeItems =
        new List<RoadItem>();

    private readonly List<Transform> validLanes =
        new List<Transform>();

    private readonly List<Transform> laneBag =
        new List<Transform>();

    private readonly List<Transform> availablePoints =
        new List<Transform>();

    private readonly List<SpawnOption> candidates =
        new List<SpawnOption>();

    private readonly List<RoadItem> recentProposals =
        new List<RoadItem>();

    private readonly List<RoadItem> recentObstacles =
        new List<RoadItem>();

    private Transform lastPoint;
    private RoadItem.ItemKind lastKind;
    private int categoryStreak;

    private float remainingTime;
    private float previousInterval;

    private void OnEnable()
    {
        lastPoint = null;
        categoryStreak = 0;

        laneBag.Clear();
        recentProposals.Clear();
        recentObstacles.Clear();

        previousInterval = 0f;
        remainingTime = NextInterval();
    }

    private void Update()
    {
        if (gameController == null || !gameController.IsRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime > 0f)
            return;

        remainingTime = TrySpawn()
            ? NextInterval()
            : Mathf.Max(0.05f, blockedRetryInterval);
    }

    private float NextInterval()
    {
        float minimum = Mathf.Max(0.1f, minSpawnInterval);
        float maximum = Mathf.Max(minimum, maxSpawnInterval);

        if (previousInterval > 0f)
        {
            float previous = Mathf.Clamp(
                previousInterval,
                minimum,
                maximum
            );

            float variation = Mathf.Max(0.01f, maxIntervalChange);

            minimum = Mathf.Max(minimum, previous - variation);
            maximum = Mathf.Min(maximum, previous + variation);
        }

        previousInterval = UnityEngine.Random.Range(
            minimum,
            maximum
        );

        return previousInterval;
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

    private bool HasValidOption(
        SpawnOption[] options,
        RoadItem.ItemKind kind)
    {
        if (options == null)
            return false;

        foreach (SpawnOption option in options)
        {
            if (IsValid(option, kind))
                return true;
        }

        return false;
    }

    private bool WasRecent(
        RoadItem prefab,
        List<RoadItem> history,
        int count)
    {
        int start = Mathf.Max(0, history.Count - count);

        for (int index = start; index < history.Count; index++)
        {
            if (history[index] == prefab)
                return true;
        }

        return false;
    }

    private RoadItem PickPrefab(
        SpawnOption[] options,
        RoadItem.ItemKind kind,
        List<RoadItem> history,
        int avoidCount)
    {
        int blockedCount = Mathf.Min(
            Mathf.Max(0, avoidCount),
            history.Count
        );

        // Se faltarem alternativas, libera primeiro
        // o item mais antigo do histórico.
        for (; blockedCount >= 0; blockedCount--)
        {
            candidates.Clear();
            double totalWeight = 0d;

            foreach (SpawnOption option in options)
            {
                if (!IsValid(option, kind))
                    continue;

                if (WasRecent(option.prefab, history, blockedCount))
                    continue;

                candidates.Add(option);
                totalWeight += option.weight;
            }

            if (candidates.Count == 0)
                continue;

            double draw = UnityEngine.Random.value * totalWeight;

            foreach (SpawnOption option in candidates)
            {
                if (draw < option.weight)
                    return option.prefab;

                draw -= option.weight;
            }

            return candidates[candidates.Count - 1].prefab;
        }

        return null;
    }

    private void Remember(
        List<RoadItem> history,
        RoadItem prefab,
        int limit)
    {
        history.Add(prefab);

        while (history.Count > Mathf.Max(0, limit))
            history.RemoveAt(0);
    }

    private bool HasEntrySpace(Transform point)
    {
        foreach (RoadItem item in activeItems)
        {
            if (item == null)
                continue;

            float distance = Mathf.Abs(
                item.transform.position.y - point.position.y
            );

            if (distance < minimumEntryGap)
                return false;
        }

        return true;
    }

    private void PrepareAvailableLanes()
    {
        validLanes.Clear();
        availablePoints.Clear();

        if (spawnPoints == null)
            return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null ||
                !spawnPoint.gameObject.activeInHierarchy ||
                validLanes.Contains(spawnPoint))
            {
                continue;
            }

            validLanes.Add(spawnPoint);
        }

        laneBag.RemoveAll(
            point => point == null || !validLanes.Contains(point)
        );

        if (laneBag.Count == 0)
            laneBag.AddRange(validLanes);

        foreach (Transform candidatePoint in laneBag)
        {
            if (validLanes.Count > 1 && candidatePoint == lastPoint)
                continue;

            if (HasEntrySpace(candidatePoint))
                availablePoints.Add(candidatePoint);
        }
    }

    private bool TrySpawn()
    {
        activeItems.RemoveAll(item => item == null);

        PrepareAvailableLanes();

        if (availablePoints.Count == 0)
            return false;

        float obstacleChance =
            HasValidOption(obstacles, RoadItem.ItemKind.Obstacle)
            && gameController.ObstacleSpeed > 0f
                ? SafeWeight(obstacleWeight)
                : 0f;

        float proposalChance =
            HasValidOption(proposals, RoadItem.ItemKind.Proposal)
            && gameController.ProposalSpeed > 0f
                ? SafeWeight(proposalWeight)
                : 0f;

        if (obstacleChance > 0f &&
            proposalChance > 0f &&
            categoryStreak >= Mathf.Max(1, maxCategoryStreak))
        {
            if (lastKind == RoadItem.ItemKind.Obstacle)
                obstacleChance = 0f;
            else
                proposalChance = 0f;
        }

        double totalChance =
            (double)obstacleChance + proposalChance;

        if (totalChance <= 0d)
            return false;

        bool spawnObstacle =
            UnityEngine.Random.value * totalChance < obstacleChance;

        RoadItem.ItemKind selectedKind = spawnObstacle
            ? RoadItem.ItemKind.Obstacle
            : RoadItem.ItemKind.Proposal;

        List<RoadItem> history = spawnObstacle
            ? recentObstacles
            : recentProposals;

        int historyLimit = spawnObstacle
            ? recentObstaclesToAvoid
            : recentProposalsToAvoid;

        RoadItem prefab = PickPrefab(
            spawnObstacle ? obstacles : proposals,
            selectedKind,
            history,
            historyLimit
        );

        if (prefab == null)
            return false;

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
        Remember(history, prefab, historyLimit);

        laneBag.Remove(selectedPoint);
        lastPoint = selectedPoint;

        categoryStreak =
            categoryStreak > 0 && lastKind == selectedKind
                ? categoryStreak + 1
                : 1;

        lastKind = selectedKind;

        return true;
    }

    private float SafeWeight(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            return 0f;

        return Mathf.Max(0f, value);
    }
}

