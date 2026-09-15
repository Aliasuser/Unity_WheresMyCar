using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, carZRot, diffZRot;
    private Vector3 placeSize, carSize;
    private float xSizeDiff, ySizeDiff;
    public GameObjectsScript gameObjectsScript;

    void Awake()
    {
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        if((eventData.pointerDrag != null) && Input.GetMouseButtonUp(0) &&
            (!Input.GetMouseButton(2)))
        {
            if (eventData.pointerDrag.tag.Equals(tag))
            {
                placeZRot = eventData.pointerDrag.GetComponent<RectTransform>().transform.eulerAngles.z;
                carZRot = GetComponent<RectTransform>().transform.eulerAngles.z;
                diffZRot = Mathf.Abs(placeZRot - carZRot);
                Debug.Log("Diff Z Rot: " + diffZRot);

                placeSize = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
                carSize = GetComponent<RectTransform>().localScale;
                xSizeDiff = Mathf.Abs(placeSize.x - carSize.x);
                ySizeDiff = Mathf.Abs(placeSize.y - carSize.y);
                Debug.Log("Diff X Size: " + xSizeDiff);
                Debug.Log("Diff Y Size: " + ySizeDiff);

                if((diffZRot <= 7 || (diffZRot >= 353 && diffZRot <= 360)) &&
                    (xSizeDiff <= 0.08f && ySizeDiff <= 0.08f))
                {
                    Debug.Log("Car placed correctly!");
                    gameObjectsScript.inRightPlace = true;
                    eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;

                    eventData.pointerDrag.GetComponent<RectTransform>().localScale =
                        GetComponent<RectTransform>().localScale;

                    eventData.pointerDrag.GetComponent<RectTransform>().localRotation =
                        GetComponent<RectTransform>().localRotation;

                    switch (eventData.pointerDrag.tag)
                    {
                        case "Garbage":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[1]);
                            break;
                        case "Ambulance":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[2]);
                            break;
                        case "School":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[3]);
                            break;
                        case "b2":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[5]);
                            break;
                        case "Cement":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[6]);
                            break;
                        case "e46":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[7]);
                            break;
                        case "e61":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[8]);
                            break;
                        case "Excavator":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[9]);
                            break;
                        case "Police":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[10]);
                            break;
                        case "Tractor1":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[11]);
                            break;
                        case "Tractor5":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[12]);
                            break;
                        case "FireTruck":
                            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[13]);
                            break;
                        default:
                            Debug.Log("No matching tag found.");
                            break;
                    }

                }
            }
            else
            {
                gameObjectsScript.inRightPlace = false;
                gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[4]);

                switch(eventData.pointerDrag.tag)
                {
                    case "Garbage":
                        gameObjectsScript.garbageTruck.GetComponent<RectTransform>().localPosition = 
                            gameObjectsScript.garbageTruckCoord;
                        break;
                    case "Ambulance":
                        gameObjectsScript.medicine.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.medicineCoord;
                        break;
                    case "School":
                        gameObjectsScript.schoolBuss.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.schoolBussCoord;
                        break;
                    case "b2":
                        gameObjectsScript.b2.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.b2Coord;
                        break;
                    case "Cement":
                        gameObjectsScript.CementTruck.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.CementTruckCoord;
                        break;
                    case "e46":
                        gameObjectsScript.e46.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.e46Coord;
                        break;
                    case "e61":
                        gameObjectsScript.e61.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.e61Coord;
                        break;
                    case "Excavator":
                        gameObjectsScript.Excavator.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.ExcavatorCoord;
                        break;
                    case "Police":
                        gameObjectsScript.Police.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.PoliceCoord;
                        break;
                    case "Tractor1":
                        gameObjectsScript.Tractor1.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.Tractor1Coord;
                        break;
                    case "Tractor5":
                        gameObjectsScript.Tractor5.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.Tractor5Coord;
                        break;
                    case "FireTruck":
                        gameObjectsScript.FireTruck.GetComponent<RectTransform>().localPosition =
                            gameObjectsScript.FireTruckCoord;
                        break;

                    default:
                        Debug.Log("No matching tag found.");
                        break;
                }
            }
        }
    }

}
