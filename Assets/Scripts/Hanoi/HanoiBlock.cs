using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A draggable Tower of Hanoi block. Only the top block of a rod can be picked up.
/// On release it slides onto the nearest rod if the move is legal, otherwise it
/// slides back to where it came from.
/// size: 0 = smallest block (0block.png) ... 5 = biggest block (5block.png).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class HanoiBlock : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int size;
    [HideInInspector] public HanoiRod rod;

    private SpriteRenderer sr;
    private BoxCollider2D box;
    private Camera cam;
    private HanoiRod originRod;
    private Vector3 grabOffset;
    private bool dragging;
    private bool busy;      // true while sliding, so it can't be grabbed mid-animation
    private int baseSortingOrder;
    private Coroutine slideRoutine;

    public float Height => sr != null ? sr.bounds.size.y : 0.7f;
    public float Width => sr != null ? sr.bounds.size.x : 1f;

    public void Init(int blockSize, Sprite sprite, int sortingOrder)
    {
        size = blockSize;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = baseSortingOrder = sortingOrder;

        box = GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        box.size = sprite.bounds.size;
        box.offset = sprite.bounds.center;
        cam = Camera.main;
    }

    // ---------------------------------------------------------------- input

    public void OnPointerDown(PointerEventData e)
    {
        var mgr = HanoiGameManager.Instance;
        if (CanGrab() && !mgr.HasSelection) mgr.PlayPickUp();
    }

    /// <summary>A tap/click (press and release without dragging): select, deselect or place - decided by the manager.</summary>
    public void OnPointerClick(PointerEventData e)
    {
        if (e.dragging || busy) return;
        var mgr = HanoiGameManager.Instance;
        if (mgr != null) mgr.OnBlockClicked(this);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        dragging = false;
        if (!CanGrab()) return;

        HanoiGameManager.Instance.OnDragStarted(this);   // drops a click-selected block back first
        dragging = true;
        originRod = rod;
        if (cam == null) cam = Camera.main;
        grabOffset = transform.position - PointerToWorld(e.position);
        grabOffset.z = 0f;
        sr.sortingOrder = baseSortingOrder + 100;   // draw above everything while dragging
        HanoiGameManager.Instance.NotifyFirstGrab();
    }

    public void OnDrag(PointerEventData e)
    {
        if (!dragging) return;
        Vector3 p = PointerToWorld(e.position) + grabOffset;
        p.z = 0f;
        transform.position = ClampToScreen(p);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!dragging) return;
        dragging = false;

        var mgr = HanoiGameManager.Instance;
        HanoiRod target = mgr.NearestRod(transform.position.x);

        if (target == null || target == originRod)
        {
            // Dropped in the air or back on its own rod: go back, no step counted.
            SlideToRod(originRod, false);
        }
        else if (target.CanAccept(this))
        {
            mgr.MoveBlock(this, originRod, target);
            SlideToRod(target, true);
        }
        else
        {
            // Illegal: a bigger block can't go on a smaller one.
            mgr.PlayInvalid();
            Flash();
            SlideToRod(originRod, false);
        }
    }

    // -------------------------------------------------------------- helpers

    private bool CanGrab()
    {
        var mgr = HanoiGameManager.Instance;
        return mgr != null && mgr.CanInteract && !busy && rod != null && rod.Top == this;
    }

    private Vector3 PointerToWorld(Vector2 screenPos)
    {
        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, dist));
    }

    private Vector3 ClampToScreen(Vector3 p)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;
        float minX = c.x - halfW + Width * 0.5f, maxX = c.x + halfW - Width * 0.5f;
        float minY = rod != null ? rod.floorY + Height * 0.5f : c.y - halfH + Height * 0.5f;
        float maxY = c.y + halfH - Height * 0.5f;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        p.y = Mathf.Clamp(p.y, minY, maxY);
        return p;
    }

    /// <summary>Slides the block to the resting place on top of <paramref name="r"/>.</summary>
    public void SlideToRod(HanoiRod r, bool playDropSound)
    {
        StartSlide(r.GetNextSlotPosition(this), playDropSound, false);
    }

    /// <summary>Lifts the block straight up above its rod (click-to-select).</summary>
    public void LiftTo(float y)
    {
        StartSlide(new Vector3(transform.position.x, y, 0f), false, true);
    }

    public bool IsBusy => busy;

    private void StartSlide(Vector3 end, bool playDropSound, bool keepOnTop)
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideRoutine(end, playDropSound, keepOnTop));
    }

    /// <summary>Path: rise (if needed) -> sideways -> down, so a block never cuts through other stacks.</summary>
    private IEnumerator SlideRoutine(Vector3 end, bool playDropSound, bool keepOnTop)
    {
        busy = true;
        sr.sortingOrder = baseSortingOrder + 100;

        Vector3 start = transform.position;
        float travelY = Mathf.Max(start.y, end.y);
        float up = travelY - start.y;
        float side = Mathf.Abs(end.x - start.x);
        float down = travelY - end.y;
        float total = up + side + down;
        float duration = Mathf.Clamp(total / 16f, 0.12f, 0.45f);

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float s = Mathf.SmoothStep(0f, 1f, t / duration) * total;
            Vector3 p;
            if (s < up) p = new Vector3(start.x, start.y + s, 0f);
            else if (s < up + side)
                p = new Vector3(Mathf.MoveTowards(start.x, end.x, s - up), travelY, 0f);
            else p = new Vector3(end.x, travelY - (s - up - side), 0f);
            transform.position = p;
            yield return null;
        }
        transform.position = end;
        sr.sortingOrder = keepOnTop ? baseSortingOrder + 100 : baseSortingOrder;
        busy = false;
        slideRoutine = null;

        if (playDropSound) HanoiGameManager.Instance.PlayDrop();
        HanoiGameManager.Instance.OnBlockSettled();
    }

    /// <summary>Quick red flash = "that move isn't allowed".</summary>
    public void Flash() { StartCoroutine(FlashRed()); }

    private IEnumerator FlashRed()
    {
        Color original = Color.white;
        sr.color = new Color(1f, 0.45f, 0.45f);
        yield return new WaitForSeconds(0.25f);
        sr.color = original;
    }

    /// <summary>Instantly puts the block on a rod (used when the level is built / restarted).</summary>
    public void PlaceInstantly(HanoiRod r)
    {
        r.Push(this);
        transform.position = r.GetSlotPosition(r.Count - 1, this);
    }
}
