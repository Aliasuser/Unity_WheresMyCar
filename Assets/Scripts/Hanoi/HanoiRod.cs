using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One rod of the Tower of Hanoi. Keeps the ordered stack of blocks that sit on it
/// (index 0 = bottom) and knows where each block of the stack must rest.
/// The rod graphic itself is baked into the background image, so this is just an
/// invisible marker placed on top of the drawn rod.
/// </summary>
public class HanoiRod : MonoBehaviour
{
    [Tooltip("World Y of the floor the bottom block rests on (the top of the bamboo ground).")]
    public float floorY = -2.55f;

    private readonly List<HanoiBlock> stack = new List<HanoiBlock>();

    public int Count => stack.Count;
    public HanoiBlock Top => stack.Count > 0 ? stack[stack.Count - 1] : null;

    public void Push(HanoiBlock block) { stack.Add(block); block.rod = this; }
    public void Remove(HanoiBlock block) { stack.Remove(block); }
    public void Clear() { stack.Clear(); }

    /// <summary>True if <paramref name="block"/> may be placed on top of this rod (only smaller on bigger).</summary>
    public bool CanAccept(HanoiBlock block)
    {
        return Top == null || Top.size > block.size;
    }

    /// <summary>World position a block must have to rest at stack level <paramref name="level"/>.</summary>
    public Vector3 GetSlotPosition(int level, HanoiBlock forBlock)
    {
        float y = floorY;
        for (int i = 0; i < level && i < stack.Count; i++)
        {
            if (stack[i] == forBlock) continue;
            y += stack[i].Height;
        }
        return new Vector3(transform.position.x, y + forBlock.Height * 0.5f, 0f);
    }

    /// <summary>Slot a block would take if it were put on top of the stack right now.</summary>
    public Vector3 GetNextSlotPosition(HanoiBlock forBlock)
    {
        return GetSlotPosition(stack.Contains(forBlock) ? stack.Count - 1 : stack.Count, forBlock);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(transform.position.x, floorY, 0), new Vector3(transform.position.x, floorY + 4.5f, 0));
    }
}
