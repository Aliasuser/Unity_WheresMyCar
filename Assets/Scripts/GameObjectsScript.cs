using Unity.VisualScripting;
using UnityEngine;

public class GameObjectsScript : MonoBehaviour
{

    public GameObject garbageTruck;
    public GameObject medicine;
    public GameObject schoolBuss;

    [HideInInspector]
    public Vector2 garbageTruckCoord;
    [HideInInspector]
    public Vector2 medicineCord;
    [HideInInspector]
    public Vector2 schoolBussCord;

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;

    [HideInInspector]
    public bool inRightPlace = false;
    public static GameObjectsScript lastDragged = null;
    public static bool isDragging = false;


   
    void Awake()
    {
        garbageTruckCoord = garbageTruck.GetComponent<RectTransform>().localPosition;
        medicineCord = medicine.GetComponent<RectTransform>().localPosition;
        schoolBussCord = schoolBuss.GetComponent<RectTransform>().localPosition;
    }

}
