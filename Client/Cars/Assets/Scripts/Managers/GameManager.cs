using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    // Player name
    public InputField name;
    public GameObject pausePanel;

    private SocketIOComponent socket;

    private void Awake()
    {
        instance = this;
        
        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            if (null != name)
                name.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void Start()
    {
        socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
    }


    public void StartGame()
    {
        Debug.Log("Start Game");
        
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["name"] = name.text;
        data["car"] = "1";
        
        socket.Emit("StartGame", new JSONObject(data));
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
    }

    public void EndGame()
    {
        Application.LoadLevel(0);
    }

    public void Save()
    {
        if (null != name)
            PlayerPrefs.SetString("PlayerName", name.text);
    }
}
