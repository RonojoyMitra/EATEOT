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
    }

    float GetRed()
    {
        return GetColor().r;
    }
    float GetGreen()
    {
        return GetColor().g;
    }
    float GetBlue()
    {
        return GetColor().b;
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
