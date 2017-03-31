using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public static MiniMap Instance;

    //For placing the image of the mini map.
    public GUIStyle miniMap;
    public GUIStyle playerIcon;
    public GUIStyle enemyIcon;

    //Offset variables (X and Y) - where you want to place your map on screen.
    private float mapOffSetX = 10f;
    private float mapOffSetY = 10f;

    //The width and height of your map as it'll appear on screen,
    private float mapWidth = 120;
    private float mapHeight = 120;

    //Width and Height of your scene, or the resolution of your terrain.
    public float sceneWidth = 110;
    public float sceneHeight = 110;

    //The size of your player's and enemy's icon on the map. 
    public float iconSize = 8;
    private float iconHalfSize;
    private float mapHalfSize;
 
    private Transform player;
    private List<Transform> enemies;

    private float alphaFadeValue = 0;
    private float sign = -1;
    private float speedHide= 400.0f;

    private void Awake()
    {
        Instance = this;
        enemies = new List<Transform>();
    }

    private void Update()
    {
        //So that the pivot point of the icon is at the middle of the image.
        iconHalfSize = iconSize / 2;
        mapWidth = mapHeight = Screen.height / 5.0f;
        mapHalfSize = mapWidth / 2;
    }

    private float GetMapPos(float pos, float mapSize, float sceneSize)
    {
        return pos * mapSize / sceneSize;
    }

    private void OnGUI()
    {
        alphaFadeValue = Mathf.Clamp01(alphaFadeValue + sign * Time.deltaTime / 4);
        GUI.color = new Color(alphaFadeValue, alphaFadeValue, alphaFadeValue, alphaFadeValue);
        mapOffSetX = Mathf.Clamp(mapOffSetX + sign * (Time.deltaTime * speedHide), -(mapOffSetY + mapWidth), mapOffSetY);

        GUI.BeginGroup(new Rect(mapOffSetX, mapOffSetY, mapWidth, mapHeight), miniMap);
        foreach (Transform enemy in enemies)
        {
            if (null != enemy)
            {
                float sX = GetMapPos(enemy.position.x, mapWidth, sceneWidth);
                float sZ = GetMapPos(enemy.position.z, mapHeight, sceneHeight);
                float enemyMapX = sX - iconHalfSize + mapHalfSize;
                float enemyMapZ = -sZ - iconHalfSize + mapHalfSize;
                GUI.Box(new Rect(enemyMapX, enemyMapZ, iconSize, iconSize), "", enemyIcon);
            }
        }

        if (null != player)
        {
            float pX = GetMapPos(player.position.x, mapWidth, sceneWidth);
            float pZ = GetMapPos(player.position.z, mapHeight, sceneHeight);
            float playerMapX = pX - iconHalfSize + mapHalfSize;
            float playerMapZ = -pZ - iconHalfSize + mapHalfSize;
            GUI.Box(new Rect(playerMapX, playerMapZ, iconSize, iconSize), "", playerIcon);
        }

        GUI.EndGroup();
    }

    public void FadeIn()
    {
        sign = 1;
    }

    public void FadeOut()
    {
        sign = -1;
    }

    public void Fade()
    {
        sign *= -1;
    }

    public void SetPlayer(Transform player)
    {
        this.player = player;
    }

    public void AddEnemy(Transform enemy)
    {
        enemies.Add(enemy);
    }

    public void RemoveEnemy(Transform enemy)
    {
        enemies.Remove(enemy);
    }

}