using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Timing")]
    public float initialSpawnDelay = 2f;
    public float timeBetweenWaves = 4f;
    public float timeBetweenEnemySpawns = 0.25f;

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
        public int unlockFloor;
    }

    [Header("Basic Enemies")]
    public EnemyTier[] basicEnemies;

    [Header("Elite Enemies")]
    public EnemyTier[] eliteEnemies;
    [Range(0f, 1f)]
    public float eliteSpawnChance = 0.25f;

    [Header("Bosses")]
    public GameObject[] bossEnemies;
    public GameObject finalBoss;
    public int bossEveryFloors = 5;
    public int finalBossFloor = 20;

    // ================= WAVE SETTINGS =================

    [Header("Wave Counts")]
    public int baseWaves = 3;
    public int wavesPerFloor = 1;

    [Header("Enemy Counts")]
    public int baseEnemiesPerWave = 2;
    public int enemiesPerFloor = 1;
    public int minEnemiesPerWave = 2;
    public int maxEnemiesPerWave = 10;

    int wavesRemaining;
    bool spawning;

    void Start()
    {
        StartCoroutine(WaveFlow());
    }

    // ================= FLOW =================

    IEnumerator WaveFlow()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        SetupWaves();

        // Boss logic
        int floor = MainController.Instance.currentFloor;

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

        spawning = true;

        while (wavesRemaining > 0)
        {
            yield return StartCoroutine(SpawnWaveRoutine());
            wavesRemaining--;
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        spawning = false;

        // Wait for all enemies to die
        yield return new WaitUntil(() =>
            FindObjectsOfType<EnemyHealth>().Length == 0
        );

        FindFirstObjectByType<ElevatorController>()
            ?.SetActive(true);
    }

    // ================= WAVE SETUP =================

    void SetupWaves()
    {
        int floor = MainController.Instance.currentFloor;
        wavesRemaining = baseWaves + floor / wavesPerFloor;
    }

    // ================= WAVE SPAWN =================

    IEnumerator SpawnWaveRoutine()
    {
        int floor = MainController.Instance.currentFloor;

        int enemyCount = baseEnemiesPerWave +
                         floor * enemiesPerFloor;

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
        // Try elite
        if (eliteEnemies.Length > 0 &&
            Random.value < eliteSpawnChance)
        {
            var availableElites = GetUnlocked(eliteEnemies, floor);
            if (availableElites.Count > 0)
                return availableElites[Random.Range(0, availableElites.Count)];
        }

        // Fallback to basic
        var availableBasics = GetUnlocked(basicEnemies, floor);
        return availableBasics[Random.Range(0, availableBasics.Count)];
    }

    List<GameObject> GetUnlocked(
        EnemyTier[] tiers,
        int floor
    )
    {
        List<GameObject> list = new();

        foreach (var tier in tiers)
        {
            if (tier.prefab != null &&
                floor >= tier.unlockFloor)
            {
                list.Add(tier.prefab);
            }
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

        float spawnX;

        if (spawnLeft)
            spawnX = leftVisible && !rightVisible ? rightX : leftX;
        else
            spawnX = rightVisible && !leftVisible ? leftX : rightX;

        Vector3 pos = new Vector3(spawnX, 0f, 0f);
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
