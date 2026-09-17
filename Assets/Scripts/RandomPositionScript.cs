using UnityEngine;

public class RandomPositionScript : MonoBehaviour
{

    public GameObject[] cars;
    public Transform[] spawnPoints;
    public GameObjectsScript gameObjectsScript;

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
        for (int i = 0; i < cars.Length; i++)
        {
            cars[i].GetComponent<RectTransform>().localPosition = spawnPoints[i].localPosition;
        }
        gameObjectsScript.positions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
