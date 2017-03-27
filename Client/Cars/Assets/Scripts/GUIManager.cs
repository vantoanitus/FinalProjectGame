using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour {

    public RenderTexture MiniMapTexture;
    public Material MiniMapMaterial;

    private float offset;
    private RenderTexture miniMapTexture1;

    private void Awake()
    {
        offset = 6f;
        miniMapTexture1 = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32);
        miniMapTexture1.Create();
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
            Graphics.DrawTexture(new Rect(offset, offset, Screen.height / 4f, Screen.height / 4f), MiniMapTexture, MiniMapMaterial);
    }

}
