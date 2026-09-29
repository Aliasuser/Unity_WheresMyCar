using UnityEngine;

public class RandomPositionScript : MonoBehaviour
{

    public GameObject[] cars; // UZMANĪBU: cars[0..11] ir stāvvietu (parking place) marķieri ar DropPlaceScript,
                               // cars[12..] ir īstās velkamās mašīnas.
    public Transform[] spawnPoints;
    public GameObjectsScript gameObjectsScript;

    [Header("Indekss, no kura sākas īstās mašīnas (parking places ir pirms tā)")]
    public int firstDraggableCarIndex = 12;

    [Header("Parking Spot Rotation")]
    [Tooltip("Iespējamie pagrieziena leņķi (grādos ap Z asi), no kuriem katrai stāvvietai izvēlas nejaušu katru spēles sākumu.")]
    public float[] possibleRotations = { 0f, 90f, 180f, 270f };

    [Tooltip("Varbūtība (0-1), ka stāvvieta tiks arī spoguļota (localScale.x apgriezts), papildus pagriezienam.")]
    [Range(0f, 1f)]
    public float mirrorChance = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            //garbageTruck.transform.localPosition = spawnPointsCoord[UnityEngine.Random.Range(1, 12)];
            int randIndex = UnityEngine.Random.Range(0, spawnPoints.Length);

            Transform t = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randIndex];
            spawnPoints[randIndex] = t;
        }

        RandomizeParkingPlaceRotations(); // Piešķir nejaušu pagriezienu tikai stāvvietu marķieriem (cars[0..firstDraggableCarIndex-1])

        // Izvieto TIKAI īstās mašīnas uz sajauktajiem spawn points - stāvvietu marķieri paliek savā vietā
        int spawnIndex = 0;
        for (int i = firstDraggableCarIndex; i < cars.Length; i++)
        {
            if (spawnIndex >= spawnPoints.Length) break;
            cars[i].GetComponent<RectTransform>().localPosition = spawnPoints[spawnIndex].localPosition;
            spawnIndex++;
        }

        gameObjectsScript.positions();
    }

    void RandomizeParkingPlaceRotations()
    {
        if (possibleRotations == null || possibleRotations.Length == 0)
            return;

        for (int i = 0; i < firstDraggableCarIndex && i < cars.Length; i++)
        {
            if (cars[i] == null) continue;
            RectTransform rt = cars[i].GetComponent<RectTransform>();

            float angle = possibleRotations[UnityEngine.Random.Range(0, possibleRotations.Length)];
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);

            // Nejauši nospoguļo stāvvietu (tāpat kā spēlētājs to dara ar atstarpes taustiņu -
            // apgriežot localScale.x zīmi), saglabājot esošo izmēra apjomu.
            bool shouldMirror = UnityEngine.Random.value < mirrorChance;
            float magnitude = Mathf.Abs(rt.localScale.x);
            float sign = shouldMirror ? -1f : 1f;
            rt.localScale = new Vector3(sign * magnitude, rt.localScale.y, rt.localScale.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
