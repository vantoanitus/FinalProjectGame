using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour {

    public RenderTexture MiniMapTexture;
    public Material MiniMapMaterial;

    private float mapWidth;
    private float mapHeight;
    private float offset;

    private float alphaFadeValue = 0;
    private float sign = 1;

    private void Awake()
    {
        offset = 10f;
        mapHeight = mapWidth = Screen.height / 4.5f;
        //miniMapTexture1 = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32);
        //miniMapTexture1.Create();
    }

    private void OnGUI()
    {
        //alphaFadeValue += sign * Mathf.Clamp01(Time.deltaTime / 5);
        //GUI.color = new Color(0, 0, 0, alphaFadeValue);
        if (Event.current.type == EventType.Repaint)
            Graphics.DrawTexture(new Rect(offset, offset, mapWidth, mapHeight), MiniMapTexture, MiniMapMaterial);

    }

    public void FadeIn()
    {
        sign = 1;
    }
    public void FadeOut()
    {
        sign = -1;
    }
}
