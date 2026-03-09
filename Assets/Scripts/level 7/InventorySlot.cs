using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            // Tell any DropZone to clear this arrow's reference
            DropZone[] dropZones = FindObjectsByType<DropZone>(FindObjectsSortMode.None);
            foreach (DropZone zone in dropZones)
            {
                zone.ClearArrow(eventData.pointerDrag);
            }

            if (draggable.originalParent == transform)
            {
                draggable.parentAfterDrag = transform;
                Debug.Log("Arrow returned to its original slot");
            }
        }
        else
        {
            draggable.ReturnToOriginal();
        }
    }
}
