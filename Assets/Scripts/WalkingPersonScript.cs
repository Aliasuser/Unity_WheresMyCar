using UnityEngine;

public class WalkingPersonScript : MonoBehaviour
{
    [Header("Sprite Frames")]
    public Sprite[] walkFrames; // Drag Walking_0 .. Walking_7 here, in order
    public float frameRate = 12f; // frames per second

    [Header("Movement")]
    public float moveSpeed = 2f;
    public bool flipSpriteWithDirection = true;

    [Header("Path Bounds (walks back and forth between these)")]
    public float startX = -8f;
    public float endX = 8f;

    private int direction = 1; // 1 = moving toward endX, -1 = moving toward startX

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float frameTimer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Vector3 pos = transform.position;
        pos.x = startX;
        transform.position = pos;

        if (walkFrames != null && walkFrames.Length > 0)
        {
            spriteRenderer.sprite = walkFrames[0];
        }
    }

    void Update()
    {
        Move();
        AnimateWalk();
    }

    void Move()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);

        if (flipSpriteWithDirection)
        {
            spriteRenderer.flipX = direction < 0;
        }

        // Reached the right end -> turn around and walk back
        if (direction == 1 && transform.position.x >= endX)
        {
            direction = -1;
        }
        // Reached the left end -> turn around and walk forward again
        else if (direction == -1 && transform.position.x <= startX)
        {
            direction = 1;
        }
    }

    void AnimateWalk()
    {
        if (walkFrames == null || walkFrames.Length == 0) return;

        frameTimer += Time.deltaTime;
        float secondsPerFrame = 1f / frameRate;

        if (frameTimer >= secondsPerFrame)
        {
            frameTimer -= secondsPerFrame;
            currentFrame = (currentFrame + 1) % walkFrames.Length;
            spriteRenderer.sprite = walkFrames[currentFrame];
        }
    }
}
