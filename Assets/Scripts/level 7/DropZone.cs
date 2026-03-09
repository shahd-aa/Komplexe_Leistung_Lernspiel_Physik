using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public enum CharacterSide { Left, Right }

    [Header("Character")]
    public CharacterSide characterSide;
    public Animator characterAnimator;

    [Header("Crate & Physics")]
    public Rigidbody crate;
    public float forceMultiplier = 1f;
    public float maxSpeed = 10f;

    [Header("Other Drop Zones")]
    public DropZone leftDropZone;
    public DropZone rightDropZone;

    // Current arrow in this drop zone
    private ForceArrow currentArrow;
    public GameObject currentArrowObject;

    public GameManager_Lvl_07 gameManager;

    public Coroutine runningCheckCoroutine;

    void FixedUpdate()
    {
        float netForce = ComputeNetForce();

        // Apply physics
        crate.AddForce(Vector3.back * netForce * forceMultiplier, ForceMode.Force);

        // Limit speed
        Vector3 v = crate.linearVelocity;

        if (v.magnitude > maxSpeed)
        {
            crate.linearVelocity = v.normalized * maxSpeed;
        }

        // Update animator based on arrows direction relative to characters direction

        if (currentArrow != null)
        {
            bool pushing = IsPushing(currentArrow, characterSide);
            bool pulling = IsPulling(currentArrow, characterSide);


            characterAnimator.SetBool("Push", pushing);
            characterAnimator.SetBool("Pull", pulling);
        }
        else
        {
            ResetAnimator();
        }
    }

    // Handle drop from inventory
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        ForceArrow arrow = eventData.pointerDrag.GetComponent<ForceArrow>();

        if (draggable != null && arrow != null)
        {
            if (arrow.currentDropZone != null && arrow.currentDropZone != this)
            {
                arrow.currentDropZone.ClearArrow(arrow.gameObject);
            }

            // Return old arrow if present
            if (currentArrowObject != null && currentArrowObject != eventData.pointerDrag)
            {
                DraggableItem oldDraggable = currentArrowObject.GetComponent<DraggableItem>();
                oldDraggable?.ReturnToOriginal();
                ClearArrow(currentArrowObject);
            }

            // Parent the new arrow
            draggable.parentAfterDrag = transform;

            currentArrow = arrow;
            currentArrowObject = eventData.pointerDrag;
            arrow.currentDropZone = this;

            StopCheckCoroutine();
            runningCheckCoroutine = StartCoroutine(gameManager.CheckBothArrows());
        }
    }

    public string CheckArrow()
    {
        if (currentArrowObject != null)
        {
            string arrowName = currentArrowObject.name;
            if (arrowName != null)
            {
                Debug.Log("arrow exists in the drop zone");
                return arrowName;
            }
            else
            {
                Debug.Log("arrow doesnt exist in the drop zone");
                return null;
            }
        }
        else
        {
            Debug.Log("no currentarrowobject");
            return null;
        }
    }

    public void ClearArrow(GameObject arrowObject)
    {
        if (currentArrowObject == arrowObject)
        {
            ResetAnimator();
            currentArrowObject = null;
            currentArrow = null;
        }
    }

    // used in game manager
    public void ClearCurrentArrow()
    {
        if (currentArrowObject != null)
        {
            ClearArrow(currentArrowObject);
            ResetAnimator();
            currentArrowObject = null;
            currentArrow = null;
        }
    }

    // used in game manager
    public void StopCheckCoroutine()
    {
        if (runningCheckCoroutine != null)
        {
            StopCoroutine(runningCheckCoroutine);
            runningCheckCoroutine = null;
        }
    }

    // --- FORCE LOGIC ---

    float ComputeNetForce()
    {
        float net = 0f;

        if (leftDropZone?.currentArrow != null)
            net += GetSignedForce(leftDropZone.currentArrow, CharacterSide.Left);

        if (rightDropZone?.currentArrow != null)
            net += GetSignedForce(rightDropZone.currentArrow, CharacterSide.Right);

        Debug.Log($"net force is {net}");
        return net;
    }

    float GetSignedForce(ForceArrow arrow, CharacterSide side)
    {
        float force = arrow.magnitude;


        force *= (arrow.direction == ForceArrow.Direction.Right) ? 1f : -1f;


        Debug.Log($"{side} character arrow: {arrow.direction}, signedForce={force}");

        return force;
    }


    float GetForceForSide(CharacterSide side)
    {
        if (side == CharacterSide.Left && leftDropZone?.currentArrow != null)
            return GetSignedForce(leftDropZone.currentArrow, side);
        if (side == CharacterSide.Right && rightDropZone?.currentArrow != null)
            return GetSignedForce(rightDropZone.currentArrow, side);
        return 0f;
    }


    // --- ANIMATION LOGIC ---
    bool IsPushing(ForceArrow arrow, CharacterSide side)
    {
        // Left character pushes when arrow points right
        // Right character pushes when arrow points left
        if (side == CharacterSide.Left)
            return arrow.direction == ForceArrow.Direction.Right;
        else
            return arrow.direction == ForceArrow.Direction.Left;
    }

    bool IsPulling(ForceArrow arrow, CharacterSide side)
    {
        return !IsPushing(arrow, side);
    }

    void ResetAnimator()
    {
        characterAnimator.Play("idle anim", 0);
        characterAnimator.SetBool("Push", false);
        characterAnimator.SetBool("Pull", false);
    }
}
