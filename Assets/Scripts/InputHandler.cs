using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class input : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(pos: (Vector3)Mouse.current.position.ReadValue()));
        if (rayHit.collider) return;

        Debug.Log(rayHit.collider.gameObject.name);
    }
}
