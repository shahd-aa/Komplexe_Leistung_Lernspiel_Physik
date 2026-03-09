using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    [HideInInspector] public Transform parentAfterDrag;

    [HideInInspector] public Transform originalParent; // Store original
    private Vector3 originalLocalPosition; // Store original position
    private bool initialized = false;

    private void Start()
    {
        // Automatically assign the Image component if not assigned in the Inspector
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        // Store where it came from
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        initialized = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root); // unlink the item from parent
        transform.SetAsLastSibling(); // always keep the item at top
        image.raycastTarget = false; // hide item from the mouse to detect slot
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // drags the item with mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true; // make the item interactable after drop
    }

    public void ReturnToOriginal()
    {
        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPosition;

        DropZone[] dropZones = FindObjectsByType<DropZone>(FindObjectsSortMode.None);
        foreach (DropZone zone in dropZones)
        {
            zone.ClearArrow(gameObject);
        }
    }
}
