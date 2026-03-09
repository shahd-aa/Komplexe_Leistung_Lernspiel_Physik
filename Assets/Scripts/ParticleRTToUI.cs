using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class ParticleRTToUI : MonoBehaviour
{
    [Header("references")]
    public Camera mainCamera;            // optional, just for alignment reference
    public RawImage targetRawImage;      // the RawImage on your screen-space overlay canvas
    public Transform particleRoot;       // root of your particle systems (set to layer VFX)
    
    [Header("render texture settings")]
    public int textureWidth = 1024;      // reduce for perf (e.g. 512)
    public int textureHeight = 1024;
    public RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;
    public int depthBufferBits = 16;

    [Header("camera settings")]
    public bool useOrthographic = false; // if your particles are in world or screen-space
    public float orthoSize = 5f;
    public float fieldOfView = 60f;
    public string particleLayerName = "VFX";

    // internal
    Camera vfxCamera;
    RenderTexture rt;

    void Start()
    {
        if (targetRawImage == null)
        {
            Debug.LogError("ParticleRTToUI: targetRawImage is not assigned.");
            return;
        }

        CreateRenderTexture();
        CreateParticleCamera();
        AssignTextureToRawImage();
    }

    void CreateRenderTexture()
    {
        // create or recreate RT to current size
        if (rt != null)
        {
            if (rt.width != textureWidth || rt.height != textureHeight)
            {
                rt.Release();
                Destroy(rt);
                rt = null;
            }
        }

        if (rt == null)
        {
            rt = new RenderTexture(textureWidth, textureHeight, depthBufferBits, rtFormat);
            rt.useMipMap = false;
            rt.autoGenerateMips = false;
            rt.Create();
        }
    }

    void CreateParticleCamera()
    {
        // if camera exists, destroy it first
        if (vfxCamera != null)
        {
            Destroy(vfxCamera.gameObject);
            vfxCamera = null;
        }

        GameObject camGO = new GameObject("VFX_ParticleCamera");
        camGO.transform.SetParent(transform, false);

        vfxCamera = camGO.AddComponent<Camera>();
        vfxCamera.clearFlags = CameraClearFlags.SolidColor;
        vfxCamera.backgroundColor = new Color(0, 0, 0, 0); // transparent background
        vfxCamera.cullingMask = LayerMask.GetMask(particleLayerName);
        vfxCamera.allowHDR = false;
        vfxCamera.allowMSAA = false;
        vfxCamera.targetTexture = rt;
        vfxCamera.depth = -100; // irrelevant for render texture, but keep default

        if (useOrthographic)
        {
            vfxCamera.orthographic = true;
            vfxCamera.orthographicSize = orthoSize;
        }
        else
        {
            vfxCamera.orthographic = false;
            vfxCamera.fieldOfView = fieldOfView;
        }

        // position camera relative to particleRoot if provided, otherwise at origin
        if (particleRoot != null)
        {
            // basic: position camera to look at particle root
            camGO.transform.position = particleRoot.position + new Vector3(0, 0, -10f);
            camGO.transform.LookAt(particleRoot.position);
        }
    }

    void AssignTextureToRawImage()
    {
        targetRawImage.texture = rt;
        // ensure alpha sort is correct; don't block UI clicks
        targetRawImage.raycastTarget = false;
    }

    // if your screen size changes, recreate RT if needed
    void OnDestroy()
    {
        if (vfxCamera != null)
        {
            if (vfxCamera.targetTexture != null) vfxCamera.targetTexture = null;
            Destroy(vfxCamera.gameObject);
        }

        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
            rt = null;
        }
    }

#if UNITY_EDITOR
    // helpful editor convenience: update RT when values change in inspector while playing
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            CreateRenderTexture();
            if (vfxCamera != null) vfxCamera.targetTexture = rt;
            if (targetRawImage != null) targetRawImage.texture = rt;
        }
    }
#endif
}
