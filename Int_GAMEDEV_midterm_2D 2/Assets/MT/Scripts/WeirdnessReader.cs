using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeirdnessReader : MonoBehaviour
{
    [SerializeField]
    RenderTexture render;

    int x, y;

    public static float Red;
    public static float Green;
    public static float Blue;

    [SerializeField]
    public float r, g, b;

    void Start()
    {
        int w = render.width;
        int h = render.height;
        x = w / 2;
        y = h / 2;
    }

    void Update()
    {
        Red = GetRed();
        Green = GetGreen();
        Blue = GetBlue();
        r = Red;
        g = Green;
        b = Blue;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Red Weirdness", r);
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Green Weirdness", g);
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Blue Weirdness", b);
    }

    float GetRed()
    {
        return (Mathf.Round(GetColor().r * 10f) / 10f);
    }
    float GetGreen()
    {
        return (Mathf.Round(GetColor().g * 10f) / 10f);
    }
    float GetBlue()
    {
        return (Mathf.Round(GetColor().b * 10f) / 10f);
    }

    Color GetColor()
    {
        Texture2D t = new Texture2D(256, 256, TextureFormat.RGB24, false);
        RenderTexture.active = render;
        t.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        t.Apply();
        return t.GetPixel(x, y);
    }
}
