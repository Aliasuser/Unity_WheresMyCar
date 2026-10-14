using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectTransformationScript : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float scaledSpeed = 0.5f;
    public float minScale = 0.3f;
    public float maxScale = 0.9f;
    public bool rotateCW, rotateCCW, scaleUpY, scaleDownY, scaleUpX, scaleDownX;
    public static bool isTransforming = false;
    //public GameObjectsScript gameObjectsScript;
    void Update()
    {
        if (GameObjectsScript.lastDragged == null)
            return;

        RectTransform rc = GameObjectsScript.lastDragged.GetComponent<RectTransform>();

        if (rotateCW)
        {
            rc.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }

        if (rotateCCW)
        {
            rc.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        // Mērogu maina caur izmēra vērtību (Abs), lai spoguļošana (negatīvs X) neapgrieztu mērogošanu
        Vector3 s = rc.localScale;
        float signX = s.x < 0f ? -1f : 1f;
        float magX = Mathf.Abs(s.x);
        float magY = s.y;

        if (scaleUpY) magY += scaledSpeed * Time.deltaTime;
        if (scaleDownY) magY -= scaledSpeed * Time.deltaTime;
        if (scaleUpX) magX += scaledSpeed * Time.deltaTime;
        if (scaleDownX) magX -= scaledSpeed * Time.deltaTime;

        magX = Mathf.Clamp(magX, minScale, maxScale);
        magY = Mathf.Clamp(magY, minScale, maxScale);

        rc.localScale = new Vector3(signX * magX, magY, s.z);

        isTransforming = rotateCW || rotateCCW || scaleUpY || scaleDownY || scaleUpX || scaleDownX;
    }

    public void MirrorCar()
    {
        if (GameObjectsScript.lastDragged == null)
            return;

        RectTransform rc = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
        Vector3 s = rc.localScale;
        rc.localScale = new Vector3(-s.x, s.y, s.z); // mainās tikai X zīme, izmērs un rotācija paliek
    }

    public void StartRotateCW(BaseEventData eventData)
    {
        rotateCW = true;
    }

    public void StopRotateCW(BaseEventData eventData)
    {
        rotateCW = false;
    }

    public void StartRotateCCW(BaseEventData eventData)
    {
        rotateCCW = true;
    }

    public void StopRotateCCW(BaseEventData eventData)
    {
        rotateCCW = false;
    }
    public void StartScaleUpY(BaseEventData eventData)
    {
        scaleUpY = true;
    }

    public void StopScaleUpY(BaseEventData eventData)
    {
        scaleUpY = false;
    }
    public void StartScaleDownY(BaseEventData eventData)
    {
        scaleDownY = true;
    }
    public void StopScaleDownY(BaseEventData eventData)
    {
        scaleDownY = false;
    }

    public void StartScaleUpX(BaseEventData eventData)
    {
        scaleUpX = true;
    }

    public void StopScaleUpX(BaseEventData eventData)
    {
        scaleUpX = false;
    }

    public void StartScaleDownX(BaseEventData eventData)
    {
        scaleDownX = true;
    }

    public void StopScaleDownX(BaseEventData eventData)
    {
        scaleDownX = false;
    }
}