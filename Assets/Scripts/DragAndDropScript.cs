using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObjectsScript gameObjectsScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    public ScreenBoundaryScript screenBoundaryScript;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        rectTransform = GetComponent<RectTransform>();
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
        screenBoundaryScript = Object.FindFirstObjectByType<ScreenBoundaryScript>();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if(Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            Debug.Log("Left mouse button clicked on " + gameObject.name);
            gameObjectsScript.carSoundSource.PlayOneShot(gameObjectsScript.sounds[0]);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            GameObjectsScript.isDragging = true;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            int lastIndex = transform.parent.childCount - 1;
            int positionIndex = Mathf.Max(0, lastIndex - 1);
            transform.SetSiblingIndex(positionIndex);

            Vector3 cursorWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x, Input.mousePosition.y, screenBoundaryScript.screenPoint.z));
            rectTransform.position = cursorWorldPos;
            screenBoundaryScript.screenPoint = Camera.main.WorldToScreenPoint(rectTransform.localPosition);
            screenBoundaryScript.offset = rectTransform.localPosition - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBoundaryScript.screenPoint.z));
            GameObjectsScript.lastDragged = eventData.pointerDrag;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            Vector3 cursScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenBoundaryScript.screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(cursScreenPoint) + screenBoundaryScript.offset;
            rectTransform.position = screenBoundaryScript.GetClampedPosition(curPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Input.GetMouseButton(0))
        {
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

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
