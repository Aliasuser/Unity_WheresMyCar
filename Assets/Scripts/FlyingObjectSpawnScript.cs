using UnityEngine;

public class FlyingObjectSpawnScript : MonoBehaviour
{

    ScreenBoundaryScript screenBoundaryScript;
    [Header("Cloud Prefabs")]
    public GameObject[] cloudPrefabs;
    [Header("Plain Prefabs")]
    public GameObject[] plainPrefabs;
    [Header("Spawn Point")]
    public Transform spawnPoint;
    [Tooltip("Canvas transform, lai lidojošie objekti tiktu novietoti Canvas hierarhijas priekšplānā (virs mašīnām), nevis paslēpti aiz spawnPoint sākotnējās vietas hierarhijā.")]
    public RectTransform canvasRoot;

    [Header("Spawn Interval")]
    public float cloudSpawnInterval = 2f;
    public float plainSpawnInterval = 2f;

    private float minY, maxY;

    [Header("Cloud Speed")]
    public float cloudMinSpeed = 1.5f;
    public float cloudMaxSpeed = 150f;

    [Header("Plain Speed")]
    public float plainMinSpeed = 2f;
    public float plainMaxSpeed = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenBoundaryScript = Object.FindFirstObjectByType<ScreenBoundaryScript>();
        minY = screenBoundaryScript.worldBounds.yMin;
        maxY = screenBoundaryScript.worldBounds.xMax;
        InvokeRepeating(nameof(SpawnCloud), 0f, cloudSpawnInterval);
        InvokeRepeating(nameof(SpawnPlain), 0f, plainSpawnInterval);
    }

    void SpawnCloud()
    {
        if (GameObjectsScript.isGameEnded) return; // Spēle beigusies - vairs nespawno jaunus objektus, kas varētu pārklāt uzvaras ekrānu
        if (cloudPrefabs.Length == 0) return;

        GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        float y = Random.Range(minY, maxY);

        Vector3 pos = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);
        Transform parent = canvasRoot != null ? (Transform)canvasRoot : spawnPoint;
        GameObject cloud = Instantiate(prefab, pos, Quaternion.identity, parent);
        cloud.transform.SetAsLastSibling(); // Renderē mākoni virs mašīnām (Canvas: pēdējais bērns = priekšplānā)
        float speed = Random.Range(cloudMinSpeed, cloudMaxSpeed);

        FlyingObjectControllerScript controller = cloud.GetComponent<FlyingObjectControllerScript>();

        if(controller != null)
        {
            controller.speed = speed;
        }
    }

    void SpawnPlain()
    {
        if (GameObjectsScript.isGameEnded) return; // Spēle beigusies - vairs nespawno jaunus objektus, kas varētu pārklāt uzvaras ekrānu
        if (plainPrefabs.Length == 0) return;

        GameObject prefab = plainPrefabs[Random.Range(0, plainPrefabs.Length)];
        float y = Random.Range(minY, maxY);

        Vector3 pos = new Vector3(-spawnPoint.position.x, y, spawnPoint.position.z);
        Transform parent = canvasRoot != null ? (Transform)canvasRoot : spawnPoint;
        GameObject plain = Instantiate(prefab, pos, Quaternion.identity, parent);
        plain.transform.SetAsLastSibling(); // Renderē lidmašīnu virs mašīnām
        float speed = Random.Range(plainMinSpeed, plainMaxSpeed);

        FlyingObjectControllerScript controller = plain.GetComponent<FlyingObjectControllerScript>();

        if (controller != null)
        {
            controller.speed = -speed;
        }
    }
}
