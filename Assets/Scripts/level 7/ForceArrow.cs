using UnityEngine;

public class ForceArrow : MonoBehaviour
{
    public enum Direction { Left, Right }
    
    public int magnitude;
    public Direction direction;
    public DropZone currentDropZone;
}