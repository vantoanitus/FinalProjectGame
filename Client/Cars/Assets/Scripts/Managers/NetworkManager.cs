using SocketIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

    private const string TOKEN = "vuong@abc";

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private GameObject otherPlayerPrefab;

    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private CameraControl cameraCtrl;

    [SerializeField]
    private GameObject miniMap;

    private SocketIOComponent socket;
    private ectScript myScript;
    private GameObject player;

    private void Awake()
    {
        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();
        myScript = go.GetComponent<ectScript>();
    }

    private void Start()
    {
        // attach events
        socket.On("connect", OnConnect);
        socket.On("Load", OnLoad);
        socket.On("Start", OnStartGame);
        socket.On("Move", OnMove);
        socket.On("Turn", OnTurn);
        socket.On("Fire", OnFire);
        socket.On("UserConnect", OnUserConnect);
        socket.On("TakeDamage", OnTakeDamage);
        socket.On("DestroyShell", OnDestroyShell);
        socket.On("Death", OnDeath);
        socket.On("UserDisconnect", OnUserDisconnect);
    }

    private string md5(string source)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            string hash = GetMd5Hash(md5Hash, source);
            return hash;
        }
    }

    private string GetMd5Hash(MD5 md5Hash, string input)
    {
        // Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    private void OnConnect(SocketIOEvent e)
    {
        // Send Token
        Dictionary<string, string> dic = new Dictionary<string, string>();
        dic["token"] = md5(TOKEN);

        try
        {
            JSONObject jsonObject = new JSONObject(dic);
            socket.Emit("authenticate", jsonObject);
        }
        catch (Exception ex)
        {
            Debug.Log("JsonObjectException: " + "Authenticate");
        }
    }

    // Load game
    private void OnLoad(SocketIOEvent e)
    {
        Debug.Log("OnLoad ");

        miniMap.SetActive(true);

        JSONObject users = e.data.GetField("users");
        foreach (string id in users.keys)
        {
            // Get Args
            string name = users[id].GetField("name").ToString();
            int carType = int.Parse(myScript.JsonToString(users[id].GetField("car").ToString(), "\""));
            float health = float.Parse(myScript.JsonToString(users[id].GetField("health").ToString(), "\""));

            // Clone otherplay gameobject
            GameObject otherPlayer = CloneGameObject(otherPlayerPrefab, users[id]);
            otherPlayer.GetComponent<OtherPlayer>().name = id;
            otherPlayer.GetComponent<Health>().UpdateHealth(health);

            // Add to miniMap
            GameObject.Find("_GUIManager").GetComponent<MiniMap>().AddEnemy(otherPlayer.transform);
            Debug.Log("Add to miniMap");
        }
    }

    private void OnStartGame(SocketIOEvent e)
    {
        Debug.Log("On Start");

        Application.runInBackground = true;

        // Get args
        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");
        // Clone play gameobject
        GameObject player = CloneGameObject(playerPrefab, e.data);
        player.GetComponent<Player>().name = id;

        // Set player
        this.player = player;

        miniMap.GetComponent<MiniMap>().SetPlayer(player.transform);

        // Invisible login panel
        pausePanel.SetActive(false);

        // Set player for Camera Control
        cameraCtrl.SetPlayer(player);

    } // OnStartGame

    private void OnMove(SocketIOEvent e)
    {
        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");
        Vector3 pos = myScript.JsonToVector3(e.data.GetField("pos"));
        GameObject.Find(id).GetComponent<Movement>().Move(pos);
    } // OnMove

    private void OnTurn(SocketIOEvent e)
    {
        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");
        Quaternion rot = myScript.JsonToQuaternion(e.data.GetField("rot"));
        
        GameObject.Find(id).GetComponent<Movement>().Turn(rot);
    } // OnTurn

    private void OnFire(SocketIOEvent e)
    {
        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");
        Vector3 velocity = myScript.JsonToVector3(e.data.GetField("velocity"));
        Vector3 position = myScript.JsonToVector3(e.data.GetField("pos"));
        Quaternion rotation = myScript.JsonToQuaternion(e.data.GetField("rot"));
        
        GameObject.Find(id).GetComponent<Shooting>().Fire(velocity, position, rotation);
    } // OnFire

    private void OnUserConnect(SocketIOEvent e)
    {
        Debug.Log("OnUserConnect");

        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");
        string name = e.data.GetField("name").ToString();
        int carType = int.Parse(myScript.JsonToString(e.data.GetField("car").ToString(), "\""));

        // Clone otherplay gameobject
        GameObject otherPlayer = CloneGameObject(otherPlayerPrefab, e.data);
        otherPlayer.GetComponent<OtherPlayer>().name = id;
        
        // Add to miniMap
        GameObject.Find("_GUIManager").GetComponent<MiniMap>().AddEnemy(otherPlayer.transform);
    } // OnUserConnect


    // Data: id, health
    private void OnTakeDamage(SocketIOEvent e)
    {
        Debug.Log("OnTakeDamage");

        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");
        float health = float.Parse(myScript.JsonToString(e.data.GetField("health").ToString(), "\""));

        GameObject gameobject = GameObject.Find(id);
        GameObject shell = GameObject.Find("Shell" + id);

        if (null != gameobject)
        {
            if (gameobject.tag.Equals("Player"))
            {
                Debug.Log(health);
                gameobject.GetComponent<CarHealth>().UpdateHealth(health);
            }
            else
            {
                if (null != shell)
                {
                    Debug.Log("shell.GetComponent<OtherPlayerShell>().Play()");
                    shell.GetComponent<OtherPlayerShell>().Play();
                }


                gameobject.GetComponent<Health>().UpdateHealth(health);
            }
        }
    }

    private void OnDestroyShell(SocketIOEvent e) {
        Debug.Log("OnDestroyShell");

        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");

        //GameObject shell = GameObject.Find("Shell" + id);
        //if (null != shell)
        //{
        //    Debug.Log("shell.GetComponent<OtherPlayerShell>().Play()");
        //    shell.GetComponent<OtherPlayerShell>().Play();
        //}
    }

    private void OnDeath(SocketIOEvent e)
    {
        Debug.Log("OnDeath");

        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");

        GameObject gameobject = GameObject.Find(id);
        if (gameobject.tag == "Player")
        {
            Debug.Log("CarHealth");
            CarHealth carHealth = gameobject.GetComponent<CarHealth>();
            if (null != carHealth)
                carHealth.Death();

            pausePanel.SetActive(true);
        }
        else
        {
            Health carHealth = gameobject.GetComponent<Health>();
            if (null != carHealth)
                carHealth.Death();
        }
    } 

    private void OnUserDisconnect(SocketIOEvent e)
    {
        Debug.Log("OnUserDisconnect");

        string id = myScript.JsonToString(e.data.GetField("id").ToString(), "\"");
        
        GameObject gameObject = GameObject.Find(id);
        
        if (null != gameObject)
        {
            GameObject.Find("_GUIManager").GetComponent<MiniMap>().RemoveEnemy(gameObject.transform);
            Destroy(gameObject);
        }
    } // OnUserDisconnect


    private GameObject CloneGameObject(GameObject prefab, JSONObject data)
    {
        Vector3 pos = myScript.JsonToVector3(data.GetField("pos"));
        Quaternion rot = myScript.JsonToQuaternion(data.GetField("rot"));
        
        return Instantiate(prefab, pos, rot);
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
    }
}