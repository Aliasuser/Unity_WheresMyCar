using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameObjectsScript : MonoBehaviour
{

    public GameObject garbageTruck;
    public GameObject medicine;
    public GameObject schoolBuss;
    public GameObject b2;
    public GameObject CementTruck;
    public GameObject e46;
    public GameObject e61;
    public GameObject Excavator;
    public GameObject Police;
    public GameObject Tractor1;
    public GameObject Tractor5;
    public GameObject FireTruck;

    //[HideInInspector]
    public Vector2 garbageTruckCoord;
    [HideInInspector]
    public Vector2 medicineCoord;
    [HideInInspector]
    public Vector2 schoolBussCoord;
    [HideInInspector]
    public Vector2 b2Coord;
    [HideInInspector]
    public Vector2 CementTruckCoord;
    [HideInInspector]
    public Vector2 e46Coord;
    [HideInInspector]
    public Vector2 e61Coord;
    [HideInInspector]
    public Vector2 ExcavatorCoord;
    [HideInInspector]
    public Vector2 PoliceCoord;
    [HideInInspector]
    public Vector2 Tractor1Coord;
    [HideInInspector]
    public Vector2 Tractor5Coord;
    [HideInInspector]
    public Vector2 FireTruckCoord;

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;
    //public GameObject[] spawnPoints = new GameObject[24];
    //public Vector2[] spawnPointsCoord = new Vector2[24];
    public GameObject[] cars;
    public Transform[] spawnPoints;

    [HideInInspector]
    public bool inRightPlace = false;
    public static GameObject lastDragged = null;
    public static bool isDragging = false;

    void RandomizeSpawn()
    {
        for(int i = 0; i < spawnPoints.Length; i++)
         {
            //garbageTruck.transform.localPosition = spawnPointsCoord[UnityEngine.Random.Range(1, 12)];
            int randIndex = UnityEngine.Random.Range(0, spawnPoints.Length);

            Transform t = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randIndex];
            spawnPoints[randIndex] = t;
         }
        for(int i = 0; i < cars.Length; i++)
        {
            cars[i].GetComponent<RectTransform>().localPosition = spawnPoints[i].localPosition;
        }
    }
   
    void Awake() //17*2 gab ar placeholder
    {
        garbageTruckCoord = garbageTruck.GetComponent<RectTransform>().localPosition;
        medicineCoord = medicine.GetComponent<RectTransform>().localPosition;
        schoolBussCoord = schoolBuss.GetComponent<RectTransform>().localPosition;
        b2Coord = b2.GetComponent<RectTransform>().localPosition;
        CementTruckCoord = CementTruck.GetComponent<RectTransform>().localPosition;
        e46Coord = e46.GetComponent<RectTransform>().localPosition;
        e61Coord = e61.GetComponent<RectTransform>().localPosition;
        ExcavatorCoord = Excavator.GetComponent<RectTransform>().localPosition;
        PoliceCoord = Police.GetComponent<RectTransform>().localPosition;
        Tractor1Coord = Tractor1.GetComponent<RectTransform>().localPosition;
        Tractor5Coord = Tractor5.GetComponent<RectTransform>().localPosition;
        FireTruckCoord = FireTruck.GetComponent<RectTransform>().localPosition;

        /*for(int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPointsCoord[i] = spawnPoints[i].GetComponent<RectTransform>().localPosition;
        }
        RandomizeSpawn();*/

 

    }

    public void positions()
    {
        garbageTruckCoord = garbageTruck.GetComponent<RectTransform>().localPosition;
        medicineCoord = medicine.GetComponent<RectTransform>().localPosition;
        schoolBussCoord = schoolBuss.GetComponent<RectTransform>().localPosition;
        b2Coord = b2.GetComponent<RectTransform>().localPosition;
        CementTruckCoord = CementTruck.GetComponent<RectTransform>().localPosition;
        e46Coord = e46.GetComponent<RectTransform>().localPosition;
        e61Coord = e61.GetComponent<RectTransform>().localPosition;
        ExcavatorCoord = Excavator.GetComponent<RectTransform>().localPosition;
        PoliceCoord = Police.GetComponent<RectTransform>().localPosition;
        Tractor1Coord = Tractor1.GetComponent<RectTransform>().localPosition;
        Tractor5Coord = Tractor5.GetComponent<RectTransform>().localPosition;
        FireTruckCoord = FireTruck.GetComponent<RectTransform>().localPosition;
    }

}
