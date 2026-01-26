using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SurvivalWaveSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    // ================= TIMING =================

    [Header("Timing")]
    public float initialSpawnDelay = 2f;

    public float baseTimeBetweenWaves = 4f;
    public float baseTimeBetweenEnemySpawns = 0.25f;

    [Tooltip("Multiplier applied per wave (e.g. 0.99 = 1% faster per wave)")]
    [Range(0.95f, 1f)]
    public float waveTimeMultiplier = 0.985f;

    [Range(0.95f, 1f)]
    public float enemySpawnMultiplier = 0.99f;

    public float minTimeBetweenWaves = 1.5f;
    public float minTimeBetweenEnemySpawns = 0.08f;

    float timeBetweenWaves;
    float timeBetweenEnemySpawns;

    // ================= SPAWN =================

    [Header("Spawn Distance")]
    public float spawnDistance = 15f;

    [Header("Level Bounds")]
    public float levelLeftBound = -50f;
    public float levelRightBound = 50f;

    // ================= ENEMY TIERS =================

    [System.Serializable]
    public class EnemyTier
    {
        public GameObject prefab;

        [Tooltip("Wave this enemy becomes available")]
        public int unlockWave = 1;

        [Tooltip("Wave this enemy becomes unavailable (0 = never locks)")]
        public int lockWave = 0;
    }

    [Header("Basic Enemies")]
    public EnemyTier[] basicEnemies;

    [Header("Elite Enemies")]
    public EnemyTier[] eliteEnemies;

    [Header("Elite Chance")]
    [Range(0f, 1f)]
    public float baseEliteChance = 0.15f;

    public float eliteChancePerWave = 0.005f;

    [Range(0f, 1f)]
    public float maxEliteChance = 0.45f;

    // ================= BOSSES =================

    [Header("Bosses")]
    public GameObject[] bossEnemies;

    [Tooltip("Spawn a boss every X waves")]
    public int bossEveryWaves = 25;

    public GameObject finalBoss;
    public int finalBossWave = 0; // 0 = disabled

    // ================= WAVES =================

    [Header("Enemy Counts")]
    public int baseEnemiesPerWave = 3;
    public int enemiesPerWaveGrowth = 1;
    public int minEnemiesPerWave = 3;
    public int maxEnemiesPerWave = 14;

    int globalWaveIndex = 0;

    // ================= UNITY =================

    void Start()
    {
        StartCoroutine(SurvivalLoop());
    }

    // ================= MAIN LOOP =================

    IEnumerator SurvivalLoop()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            globalWaveIndex++;
            MainController.Instance.currentWaves = globalWaveIndex;

            SetupScaling(globalWaveIndex);

            // -------- BOSS LOGIC --------

            if (finalBoss != null &&
                finalBossWave > 0 &&
                globalWaveIndex == finalBossWave)
            {
                SpawnEnemy(finalBoss);
            }
            else if (
                bossEnemies.Length > 0 &&
                globalWaveIndex % bossEveryWaves == 0
            )
            {
                SpawnEnemy(
                    bossEnemies[Random.Range(0, bossEnemies.Length)]
                );
            }

            // -------- WAVE --------

            yield return StartCoroutine(
                SpawnWaveRoutine(globalWaveIndex)
            );

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    // ================= SCALING =================

    void SetupScaling(int wave)
    {
        timeBetweenWaves = Mathf.Max(
            minTimeBetweenWaves,
            baseTimeBetweenWaves *
            Mathf.Pow(waveTimeMultiplier, wave - 1)
        );

        timeBetweenEnemySpawns = Mathf.Max(
            minTimeBetweenEnemySpawns,
            baseTimeBetweenEnemySpawns *
            Mathf.Pow(enemySpawnMultiplier, wave - 1)
        );
    }

    // ================= WAVE SPAWN =================

    IEnumerator SpawnWaveRoutine(int wave)
    {
        int enemyCount =
            baseEnemiesPerWave +
            wave * enemiesPerWaveGrowth;

        enemyCount = Mathf.Clamp(
            enemyCount,
            minEnemiesPerWave,
            maxEnemiesPerWave
        );

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(GetEnemyForWave(wave));
            yield return new WaitForSeconds(timeBetweenEnemySpawns);
        }
    }

    // ================= ENEMY SELECTION =================

    GameObject GetEnemyForWave(int wave)
    {
        float eliteChance = Mathf.Min(
            maxEliteChance,
            baseEliteChance + eliteChancePerWave * wave
        );

        if (eliteEnemies.Length > 0 &&
            Random.value < eliteChance)
        {
            var elites = GetAvailable(eliteEnemies, wave);
            if (elites.Count > 0)
                return elites[Random.Range(0, elites.Count)];
        }

        var basics = GetAvailable(basicEnemies, wave);
        return basics[Random.Range(0, basics.Count)];
    }

    List<GameObject> GetAvailable(
        EnemyTier[] tiers,
        int wave
    )
    {
        List<GameObject> list = new();

        foreach (var tier in tiers)
        {
            if (tier.prefab == null)
                continue;

            if (wave < tier.unlockWave)
                continue;

            if (tier.lockWave > 0 && wave > tier.lockWave)
                continue;

            list.Add(tier.prefab);
        }

        return list;
    }

    // ================= SPAWNING =================

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || player == null)
            return;

        Camera cam = Camera.main;
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        float camLeft  = cam.transform.position.x - camHalfWidth;
        float camRight = cam.transform.position.x + camHalfWidth;

        bool spawnLeft = Random.value < 0.5f;

        float leftX = Mathf.Clamp(
            player.position.x - spawnDistance,
            levelLeftBound,
            levelRightBound
        );

        float rightX = Mathf.Clamp(
            player.position.x + spawnDistance,
            levelLeftBound,
            levelRightBound
        );

        bool leftVisible  = leftX  > camLeft && leftX  < camRight;
        bool rightVisible = rightX > camLeft && rightX < camRight;

        float spawnX =
            spawnLeft
                ? (leftVisible && !rightVisible ? rightX : leftX)
                : (rightVisible && !leftVisible ? leftX : rightX);

        Vector3 pos = new Vector3(spawnX, -3f, 0f);
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
