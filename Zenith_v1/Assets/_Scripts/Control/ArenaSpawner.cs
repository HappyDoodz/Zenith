using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
    [Header("Spawn Point")]
    [Tooltip("Where the arena prefab will be spawned")]
    public Transform spawnPoint;

    [Header("Arena Pools")]
    public GameObject[] arenas_Floor1_5;
    public GameObject[] arenas_Floor6_10;
    public GameObject[] arenas_Floor11_15;
    public GameObject[] arenas_Floor16_19;

    [Header("Final Arena")]
    public GameObject finalBossArena; // floor 20

    GameObject currentArenaInstance;

    void Start()
    {
        SpawnArenaForCurrentFloor();
    }

    // ================= MAIN LOGIC =================

    void SpawnArenaForCurrentFloor()
    {
        int floor = MainController.Instance.currentFloor;

        // Clean up previous arena if scene reloads
        if (currentArenaInstance != null)
            Destroy(currentArenaInstance);

        GameObject arenaPrefab = GetArenaForFloor(floor);

        if (arenaPrefab == null)
        {
            Debug.LogError($"No arena prefab available for floor {floor}");
            return;
        }

        currentArenaInstance = Instantiate(
            arenaPrefab,
            spawnPoint != null ? spawnPoint.position : Vector3.zero,
            Quaternion.identity,
            transform // parent to spawner
        );
    }

    // ================= FLOOR LOGIC =================

    GameObject GetArenaForFloor(int floor)
    {
        // FINAL BOSS
        if (floor >= 20)
            return finalBossArena;

        if (floor >= 16)
            return GetRandomFromArray(arenas_Floor16_19);

        if (floor >= 11)
            return GetRandomFromArray(arenas_Floor11_15);

        if (floor >= 6)
            return GetRandomFromArray(arenas_Floor6_10);

        // floor 1–5
        return GetRandomFromArray(arenas_Floor1_5);
    }

    GameObject GetRandomFromArray(GameObject[] array)
    {
        if (array == null || array.Length == 0)
            return null;

        return array[Random.Range(0, array.Length)];
    }
}
