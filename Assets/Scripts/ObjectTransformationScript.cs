using UnityEngine;

public class ObjectTransformationScript : MonoBehaviour
{
    public GameObjectsScript gameObjectsScript;
    void Awake()
    {
        gameObjectsScript = FindFirstObjectByType<GameObjectsScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObjectsScript.isGameEnded) return; // Spēle jau beigusies - vairs nevar rotēt/mainīt izmēru

        if(GameObjectsScript.lastDragged != null)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().Rotate(0, 0, Time.deltaTime * 13);
            }
            if (Input.GetKey(KeyCode.X))
            {
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().Rotate(0, 0, -Time.deltaTime * 13);

            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y < 0.9f)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale = 
                        new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x,
                        GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y + 0.001f, 1f);
                }

            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y > 0.3f)
                {
                    GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                        new Vector3(GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x,
                        GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y -  0.001f, 1f);
                }

            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                RectTransform rt = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
                float sign = Mathf.Sign(rt.localScale.x == 0 ? 1f : rt.localScale.x); // saglabā spoguļošanas virzienu (+1 vai -1)
                float magnitude = Mathf.Abs(rt.localScale.x);

                if (magnitude > 0.3f)
                {
                    magnitude -= 0.001f;
                    rt.localScale = new Vector3(sign * magnitude, rt.localScale.y, 1f);
                }
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                RectTransform rt = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
                float sign = Mathf.Sign(rt.localScale.x == 0 ? 1f : rt.localScale.x); // saglabā spoguļošanas virzienu (+1 vai -1)
                float magnitude = Mathf.Abs(rt.localScale.x);

                if (magnitude < 0.9f)
                {
                    magnitude += 0.001f;
                    rt.localScale = new Vector3(sign * magnitude, rt.localScale.y, 1f);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                float mirrorX = GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.x;
                GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale =
                        new Vector3(-mirrorX,
                        GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.y, GameObjectsScript.lastDragged.GetComponent<RectTransform>().localScale.z);
            }
        }
    }
}
