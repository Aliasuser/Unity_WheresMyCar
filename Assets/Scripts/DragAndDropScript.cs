using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObjectsScript gameObjectsScript;
    private CanvasGroup canvasGroup;
    public ScreenBoundaryScript screenBoundaryScript;

    private Vector3 dragOffsetWorld;
    private Camera mainCamera;
    private Canvas canvas;


    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        if (gameObjectsScript == null)
            gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();

        if (screenBoundaryScript == null)
            screenBoundaryScript = Object.FindFirstObjectByType<ScreenBoundaryScript>();

        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            mainCamera = canvas.worldCamera;
        else
            Debug.LogWarning("Canvas not found in parent hierarchy of " + gameObject.name);

    }


    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicked on " + gameObject.name);


        if (GameObjectsScript.isGameEnded) return; // Spēle jau beigusies

        if (gameObjectsScript.carSoundSource != null && gameObjectsScript.sounds != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        Debug.Log("Start drag!");

        if (GameObjectsScript.isGameEnded) return; // Spēle jau beigusies - vairs nevar vilkt mašīnas

        GameObjectsScript.isDragging = true;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        int lastIndex = transform.parent.childCount - 1;
        int positionIndex = Mathf.Max(0, lastIndex - 1);
        transform.SetSiblingIndex(positionIndex);

        Vector3 cursorWorldPos;
        if (ScreenToWorldPoint(eventData.position, out cursorWorldPos))
        {
            dragOffsetWorld = transform.position - cursorWorldPos;
        }
        else
        {
            dragOffsetWorld = Vector3.zero;
        }

        /*
            Uzliek vilkto mašīnu pilnīgi priekšplānā, kamēr to velk - nevis uz fiksētu indeksu,
            kas var novecoties, ja pēc tam tiek spawnoti jauni mākoņi/lidmašīnas.
           transform.SetAsLastSibling();

           Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(
               Input.mousePosition.x, Input.mousePosition.y, screenBoundaryScript.screenPoint.z));
            rectTransform.position = cursorWorldPos;
            screenBoundaryScript.screenPoint = Camera.main.WorldToScreenPoint(rectTransform.localPosition);
            screenBoundaryScript.offset = rectTransform.localPosition - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBoundaryScript.screenPoint.z));*/
        GameObjectsScript.lastDragged = eventData.pointerDrag;
    }

    private bool ScreenToWorldPoint(Vector2 screenPosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera is not assigned.");
            return false;
        }

        float z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, z);
        worldPosition = mainCamera.ScreenToWorldPoint(screenPoint);
        return true;
    }

    public void OnDrag(PointerEventData eventData)
    {


        if (GameObjectsScript.isGameEnded) return; // Spēle jau beigusies - pārtrauc vilkšanu, ja tā jau notiek

        Vector3 pointerWorld;
        if (!ScreenToWorldPoint(eventData.position, out pointerWorld))
        {
            return;
        }


        Vector3 desired = pointerWorld + dragOffsetWorld;
        desired.z = transform.position.z;

        screenBoundaryScript.RecalculateBounds();
        Vector2 clamped = screenBoundaryScript.GetClampedPosition(desired);

        transform.position = new Vector3(clamped.x, clamped.y, desired.z);

        /* if(Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
         {
             Vector3 cursScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBoundaryScript.screenPoint.z);
             Vector3 curPosition = Camera.main.ScreenToWorldPoint(cursScreenPoint) + screenBoundaryScript.offset;
             rectTransform.position = screenBoundaryScript.GetClampedPosition(curPosition);
         }*/
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (GameObjectsScript.isGameEnded)
        {
            //Spēle beigusies - tikai atjauno izskatu, bez skaņas un bez novietošanas loģikas
            GameObjectsScript.isDragging = false;
            canvasGroup.alpha = 1f;
            return;
        }
        if (gameObjectsScript.carSoundSource != null && gameObjectsScript.sounds != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
            GameObjectsScript.isDragging = false;
            Debug.Log("OnEndDrag called for " + gameObject.name);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            if (gameObjectsScript.inRightPlace)
            {
                canvasGroup.blocksRaycasts = false;
                GameObjectsScript.lastDragged = null;
            }
            gameObjectsScript.inRightPlace = false;
        }

    }

}