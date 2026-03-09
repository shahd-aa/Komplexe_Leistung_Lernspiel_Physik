using UnityEngine;

public class ParticleNoFog : MonoBehaviour
{
    private ParticleSystemRenderer psr;
    private bool fogEnabled;

    void Start()
    {
        psr = GetComponent<ParticleSystemRenderer>();
    }

    void OnWillRenderObject()
    {
        // fog ausschalten bevor particles rendern
        fogEnabled = RenderSettings.fog;
        RenderSettings.fog = false;
    }

    void OnRenderObject()
    {
        // fog wieder anschalten nach particles
        RenderSettings.fog = fogEnabled;
    }
}