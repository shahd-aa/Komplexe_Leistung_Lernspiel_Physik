using UnityEngine;

public class ForceRenderUpdate : MonoBehaviour
{
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam != null && cam.targetTexture != null)
        {
            cam.Render();
        }
    }
}
