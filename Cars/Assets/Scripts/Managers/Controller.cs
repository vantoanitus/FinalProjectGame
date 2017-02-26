using UnityEngine;
using System.Collections;
using SocketIO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

public class Controller : MonoBehaviour
{

    public Rigidbody m_Shell;                   // Prefab of the shell.

    public SocketIOComponent socket;
    public Player playGameObject;
    public OtherPlayer otherPlayerGameObject;
    public LoginPanel loginPanel;
    public CameraControl camera;

    public string username;

    // Use this for initialization
    void Start()
    {

        StartCoroutine(ConnectToServer());

        socket.On("USER_CONNECTED", OnUserConnected);
        socket.On("PLAY", OnUserPlay);
        socket.On("MOVE", OnUserMove);
        socket.On("TURN", OnUserTurn);
        socket.On("FIRE", OnUserFire);
        socket.On("HEALTH_DECREASE", OnHealthDecrease);
        socket.On("USER_DISCONNECTED", OnUserDisconnected);
        socket.On("UPDATE_HEALTH", OnUpdateHealth);
        socket.On("UPDATE_POSITION", OnUpdatePosition);

        loginPanel.playBtn.onClick.AddListener(() => OnClickPlayBtn());
    }

    IEnumerator ConnectToServer()
    {
        yield return new WaitForSeconds(0.5f);
        socket.Emit("USER_CONNECT");
    }

    private void OnClickPlayBtn()
    {
        if (loginPanel.inputField.text != "")
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["name"] = loginPanel.inputField.text;
            Vector3 position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-20f, 20f));
            data["position"] = position.x + "," + position.y + "," + position.z;
            data["rotation"] = 0 + "," + 0 + "," + 0 + "," + 0;
            data["health"] = 100f + "";
            socket.Emit("PLAY", new JSONObject(data));
        }
        else
        {
            loginPanel.inputField.text = "Please enter your name again";
        }
    }

    private void OnUserPlay(SocketIOEvent e)
    {
        loginPanel.gameObject.SetActive(false);

        Vector3 position = JsonToVector3(JsonToString(e.data.GetField("position").ToString(), "\""));
        Quaternion rotation = JsonToQuaternion(JsonToString(e.data.GetField("rotation").ToString(), "\""));

        GameObject player = Instantiate(playGameObject.gameObject, position, rotation) as GameObject;

        Player playerCom = player.GetComponent<Player>();
        
        camera.Target = player;

        playerCom.playerName = JsonToString(e.data.GetField("name").ToString(), "\"");
        username = playerCom.playerName;
    }

    private void OnUserConnected(SocketIOEvent e)
    {
        //Debug.Log("Get the message from server is: " + e.data + "OnUserConnected");

        Vector3 position = JsonToVector3(JsonToString(e.data.GetField("position").ToString(), "\""));
        Quaternion rotation = JsonToQuaternion(JsonToString(e.data.GetField("rotation").ToString(), "\""));

        GameObject otherPlayer = GameObject.Instantiate(otherPlayerGameObject.gameObject, position, rotation) as GameObject;

        OtherPlayer otherPlayerCom = otherPlayer.GetComponent<OtherPlayer>();

        otherPlayerCom.playerName = JsonToString(e.data.GetField("name").ToString(), "\"");
        otherPlayerCom.id = JsonToString(e.data.GetField("id").ToString(), "\"");

        //Debug.Log(otherPlayer.name + " connected");
    }

    private void OnUserMove(SocketIOEvent e)
    {
        GameObject player1 = GameObject.Find(JsonToString(e.data.GetField("name").ToString(), "\"")) as GameObject;
        Rigidbody rigidbody = player1.GetComponent<Rigidbody>();
        Vector3 position = JsonToVector3(JsonToString(e.data.GetField("position").ToString(), "\""));
        rigidbody.MovePosition(position);
        //Debug.Log("OnUserMove" + position);
    }


    private void OnUserTurn(SocketIOEvent e)
    {
        GameObject player1 = GameObject.Find(JsonToString(e.data.GetField("name").ToString(), "\"")) as GameObject;
        Quaternion rotation = JsonToQuaternion(JsonToString(e.data.GetField("rotation").ToString(), "\""));
        Rigidbody rigidbody = player1.GetComponent<Rigidbody>();
        rigidbody.MoveRotation(rotation);
        //Debug.Log("OnUserTurn" + rotation);

    }

    private void OnUserFire(SocketIOEvent e)
    {
        GameObject player = GameObject.Find(JsonToString(e.data.GetField("name").ToString(), "\"")) as GameObject;
        GameObject fireTransform = player.transform.FindChild("FireTransform").gameObject;
  
        // Create an instance of the shell and store a reference to it's rigidbody.
        Rigidbody shellInstance = Instantiate(m_Shell, fireTransform.transform.position, fireTransform.transform.rotation) as Rigidbody;
        shellInstance.gameObject.name = "Shell " + player.name;
        Vector3 velocity = JsonToVector3(JsonToString(e.data.GetField("velocity").ToString(), "\""));

        // Set the shell's velocity to the launch force in the fire position's forward direction.
        shellInstance.velocity = velocity; //JsonToVector3(JsonToString(e.data.GetField("velocity").ToString(), "\""));

    }

    private void OnHealthDecrease(SocketIOEvent e)
    {
        //Debug.Log("OnHealthDecrease");
        GameObject player = GameObject.Find(JsonToString(e.data.GetField("player_name").ToString(), "\"")) as GameObject;

        Debug.Log(player.name + " va " + username);
        if (player.name == username)
        {
            float healthDecrease = JsonToFloat(JsonToString(e.data.GetField("health_decrease").ToString(), "\""));
            Debug.Log("damage on healthdecrease: " + healthDecrease);
            Dictionary<string, string> data = new Dictionary<string, string>();
            float health = player.GetComponent<CarHealth>().GetCurrentHealth() - healthDecrease;
            //Debug.Log("Mau: " + health);
            data["health"] = health + "";

            socket.Emit("UPDATE_HEALTH", new JSONObject(data));
        }
    }

    private void OnUpdateHealth(SocketIOEvent e)
    {
        //Debug.Log("OnUpdateHealth");
        GameObject player = GameObject.Find(JsonToString(e.data.GetField("name").ToString(), "\"")) as GameObject;
        float health = JsonToFloat(JsonToString(e.data.GetField("health").ToString(), "\""));
        //Debug.Log("Health: " + health);
        player.GetComponent<CarHealth>().UpdateHealth(health);
    }

    private void OnUpdatePosition(SocketIOEvent e)
    {
        //Debug.Log("OnUpdatePosition");
        GameObject player = GameObject.Find(JsonToString(e.data.GetField("player_name").ToString(), "\"")) as GameObject;

        if (player.name == username)
        {
            Vector3 position = JsonToVector3(JsonToString(e.data.GetField("position").ToString(), "\""));

            Dictionary<string, string> data = new Dictionary<string, string>();
            data["position"] = position.x + "," + position.y + "," + position.z;

            socket.Emit("MOVE", new JSONObject(data));
            //Debug.Log("OnUpdatePosition " + data["position"]);
        }
    }

    private void OnUserDisconnected(SocketIOEvent e)
    {
        Destroy(GameObject.Find(JsonToString(e.data.GetField("name").ToString(), "\"")));
    }

    private string JsonToString(string target, string s)
    {
        string[] newString = Regex.Split(target, s);
        return newString[1];
    }

    private Vector3 JsonToVector3(string target)
    {
        Vector3 newVector;
        string[] newString = Regex.Split(target, ",");
        newVector = new Vector3(float.Parse(newString[0]), float.Parse(newString[1]), float.Parse(newString[2]));
        return newVector;
    }


    private Quaternion JsonToQuaternion(string target)
    {
        Quaternion newQuat;
        string[] newString = Regex.Split(target, ",");
        newQuat = new Quaternion(float.Parse(newString[0]), float.Parse(newString[1]), float.Parse(newString[2]), float.Parse(newString[3]));
        return newQuat;
    }

    private float JsonToFloat(string target)
    {
        return float.Parse(target, CultureInfo.InvariantCulture.NumberFormat);
    }
}
