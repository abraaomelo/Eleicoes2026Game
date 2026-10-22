using System;
using UnityEngine;

public class RoadItemSpawner : MonoBehaviour
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

    [Header("Categoria deste gerador")]
    [SerializeField] private RoadItem.ItemKind kind;

    [Header("Objetos e pesos")]
    [SerializeField] private SpawnOption[] options;

    [Header("Intervalo entre aparições — segundos")]
    [SerializeField, Min(0.1f)] private float minInterval = 2f;
    [SerializeField, Min(0.1f)] private float maxInterval = 4f;

    [Header("Repetição")]
    [SerializeField] private bool preventImmediateRepeat;

    private RoadItem lastPrefab;
    private float remainingTime;

    private void OnEnable()
    {
        lastPrefab = null;
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (gameController == null || !gameController.IsRunning)
            return;

        float speed = kind == RoadItem.ItemKind.Obstacle
            ? gameController.ObstacleSpeed
            : gameController.ProposalSpeed;

        if (speed <= 0f)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            Spawn();
            ScheduleNextSpawn();
        }
    }

    private void ScheduleNextSpawn()
    {
        float minimum = Mathf.Max(0.1f, minInterval);
        float maximum = Mathf.Max(minimum, maxInterval);

        remainingTime = UnityEngine.Random.Range(minimum, maximum);
    }

    private bool IsEligible(SpawnOption option)
    {
        return option != null
            && option.prefab != null
            && option.prefab.Kind == kind
            && option.weight > 0f
            && !float.IsNaN(option.weight)
            && !float.IsInfinity(option.weight)
            && (!preventImmediateRepeat || option.prefab != lastPrefab);
    }

    private void Spawn()
    {
        if (options == null || spawnPoints == null)
            return;

        Transform selectedPoint = null;
        int validPoints = 0;

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;

            validPoints++;

            if (UnityEngine.Random.Range(0, validPoints) == 0)
                selectedPoint = point;
        }

        if (selectedPoint == null)
            return;

        float totalWeight = 0f;

        foreach (SpawnOption option in options)
        {
            if (IsEligible(option))
                totalWeight += option.weight;
        }

        if (totalWeight <= 0f)
            return;

        float draw = UnityEngine.Random.value * totalWeight;
        RoadItem selectedPrefab = null;

        foreach (SpawnOption option in options)
        {
            if (!IsEligible(option))
                continue;

            selectedPrefab = option.prefab;

            if (draw < option.weight)
                break;

            draw -= option.weight;
        }

        if (selectedPrefab == null)
            return;

        RoadItem item = Instantiate(
            selectedPrefab,
            selectedPoint.position,
            Quaternion.identity
        );

        item.Initialize(gameController);
        lastPrefab = selectedPrefab;
    }
}

