using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class SpawnOption
    {
        public Enemy prefab;

        [Min(0f)]
        public float weight = 1f;
    }

    [Header("Controle geral")]
    [SerializeField] private GameController gameController;

    [Header("Inimigos e pesos")]
    [SerializeField] private SpawnOption[] enemies;

    [Header("Pontos de aparição")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Intervalo entre aparições")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;

    private float timer;
    private Enemy.EnemyType? lastType;

    private void Update()
    {
        if (gameController == null ||
            gameController.ScrollSpeed <= 0f)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= Mathf.Max(0.1f, spawnInterval))
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    private bool CanSpawn(SpawnOption option)
    {
        return option != null
            && option.prefab != null
            && option.weight > 0f
            && !float.IsNaN(option.weight)
            && !float.IsInfinity(option.weight)
            && (!lastType.HasValue
                || option.prefab.Type != lastType.Value);
    }

    private void SpawnEnemy()
    {
        if (enemies == null ||
            spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return;
        }

        int index = UnityEngine.Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        if (spawnPoint == null)
            return;

        float totalWeight = 0f;

        foreach (SpawnOption option in enemies)
        {
            if (CanSpawn(option))
                totalWeight += option.weight;
        }

        // Sem outro tipo disponível: não gera um inimigo repetido.
        if (totalWeight <= 0f)
            return;

        float draw = UnityEngine.Random.value * totalWeight;
        SpawnOption selected = null;

        foreach (SpawnOption option in enemies)
        {
            if (!CanSpawn(option))
                continue;

            selected = option;

            if (draw < option.weight)
                break;

            draw -= option.weight;
        }

        if (selected == null)
            return;

        Enemy enemy = Instantiate(
            selected.prefab,
            spawnPoint.position,
            Quaternion.identity
        );

        enemy.Initialize(gameController);
        lastType = selected.prefab.Type;
    }
}


