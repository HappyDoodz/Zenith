using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    // ================= TIMING =================

    [Header("Timing")]
    public float initialSpawnDelay = 2f;

    public float baseTimeBetweenWaves = 4f;
    public float baseTimeBetweenEnemySpawns = 0.25f;

    [Tooltip("Multiplier applied per floor (e.g. 0.98 = 2% faster per floor)")]
    [Range(0.90f, 1f)]
    public float waveTimeMultiplierPerFloor = 0.985f;

    [Range(0.90f, 1f)]
    public float enemySpawnMultiplierPerFloor = 0.99f;

    [Tooltip("Minimum cap so things never get insane")]
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

        [Tooltip("Floor this enemy becomes available")]
        public int unlockFloor = 1;

        [Tooltip("Floor this enemy becomes unavailable (0 = never locks)")]
        public int lockFloor = 0;
    }

    [Header("Basic Enemies")]
    public EnemyTier[] basicEnemies;

    [Header("Elite Enemies")]
    public EnemyTier[] eliteEnemies;

    [Header("Elite Chance")]
    [Range(0f, 1f)]
    public float baseEliteChance = 0.2f;

    [Tooltip("Added per floor")]
    public float eliteChancePerFloor = 0.01f;

    [Tooltip("Hard cap")]
    [Range(0f, 1f)]
    public float maxEliteChance = 0.45f;

    // ================= BOSSES =================

    [Header("Bosses")]
    public GameObject[] bossEnemies;
    public GameObject finalBoss;
    public int bossEveryFloors = 5;
    public int finalBossFloor = 20;

    // ================= WAVES =================

    [Header("Wave Counts")]
    public int baseWaves = 3;
    public int wavesPerFloor = 1;

    [Header("Enemy Counts")]
    public int baseEnemiesPerWave = 2;
    public int enemiesPerFloor = 1;
    public int minEnemiesPerWave = 2;
    public int maxEnemiesPerWave = 10;

    int wavesRemaining;

    // ================= UNITY =================

    void Start()
    {
        StartCoroutine(WaveFlow());
    }

    // ================= FLOW =================

    IEnumerator WaveFlow()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        int floor = MainController.Instance.currentFloor;

        SetupScaling(floor);
        SetupWaves(floor);

        // -------- BOSS LOGIC --------

        if (floor == finalBossFloor && finalBoss != null)
        {
            SpawnEnemy(finalBoss);
        }
        else if (floor % bossEveryFloors == 0 && bossEnemies.Length > 0)
        {
            SpawnEnemy(
                bossEnemies[Random.Range(0, bossEnemies.Length)]
            );
            wavesRemaining = Mathf.Max(1, wavesRemaining - 2);
        }

        // -------- WAVES --------

        while (wavesRemaining > 0)
        {
            yield return StartCoroutine(SpawnWaveRoutine(floor));
            wavesRemaining--;
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        // -------- CLEANUP --------

        yield return new WaitUntil(() =>
            FindObjectsOfType<EnemyHealth>().Length == 0
        );

        FindFirstObjectByType<ElevatorController>()
            ?.SetActive(true);
    }

    // ================= SETUP =================

    void SetupScaling(int floor)
    {
        timeBetweenWaves = Mathf.Max(
            minTimeBetweenWaves,
            baseTimeBetweenWaves * Mathf.Pow(waveTimeMultiplierPerFloor, floor - 1)
        );

        timeBetweenEnemySpawns = Mathf.Max(
            minTimeBetweenEnemySpawns,
            baseTimeBetweenEnemySpawns * Mathf.Pow(enemySpawnMultiplierPerFloor, floor - 1)
        );
    }

    void SetupWaves(int floor)
    {
        wavesRemaining = baseWaves + (floor / wavesPerFloor);
    }

    // ================= WAVE SPAWN =================

    IEnumerator SpawnWaveRoutine(int floor)
    {
        int enemyCount =
            baseEnemiesPerWave + floor * enemiesPerFloor;

        enemyCount = Mathf.Clamp(
            enemyCount,
            minEnemiesPerWave,
            maxEnemiesPerWave
        );

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(GetEnemyForFloor(floor));
            yield return new WaitForSeconds(timeBetweenEnemySpawns);
        }
    }

    // ================= ENEMY SELECTION =================

    GameObject GetEnemyForFloor(int floor)
    {
        float eliteChance = Mathf.Min(
            maxEliteChance,
            baseEliteChance + eliteChancePerFloor * floor
        );

        if (eliteEnemies.Length > 0 &&
            Random.value < eliteChance)
        {
            var elites = GetAvailable(eliteEnemies, floor);
            if (elites.Count > 0)
                return elites[Random.Range(0, elites.Count)];
        }

        var basics = GetAvailable(basicEnemies, floor);
        return basics[Random.Range(0, basics.Count)];
    }

    List<GameObject> GetAvailable(
        EnemyTier[] tiers,
        int floor
    )
    {
        List<GameObject> list = new();

        foreach (var tier in tiers)
        {
            if (tier.prefab == null)
                continue;

            if (floor < tier.unlockFloor)
                continue;

            if (tier.lockFloor > 0 && floor > tier.lockFloor)
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
