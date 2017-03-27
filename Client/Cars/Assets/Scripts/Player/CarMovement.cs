using UnityEngine;
using System.Collections;
using SocketIO;
using System.Collections.Generic;
using System;

public class CarMovement : MonoBehaviour {

	public float m_Speed = 12f;            
	public float m_TurnSpeed = 180f;       
	public AudioSource m_MovementAudio;    
	public AudioClip m_EngineIdling;       
	public AudioClip m_EngineDriving;      
	public float m_PitchRange = 0.2f;

	private string m_MovementAxisName;     
	private string m_TurnAxisName;         
	private Rigidbody m_Rigidbody;         
	private float m_MovementInputValue;    
	private float m_TurnInputValue;        
	private float m_OriginalPitch;


    private SocketIOComponent socket;

	private void Awake()
	{
        m_Rigidbody = GetComponent<Rigidbody>();
        socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
	}


	private void OnEnable ()
	{
		m_Rigidbody.isKinematic = false;
		m_MovementInputValue = 0f;
		m_TurnInputValue = 0f;
	}


	private void OnDisable ()
	{
		m_Rigidbody.isKinematic = true;
	}


	private void Start()
	{
		m_MovementAxisName = "Vertical";
		m_TurnAxisName = "Horizontal";

		m_OriginalPitch = m_MovementAudio.pitch;
	}


	private void Update()
    {
        // Store the player's input and make sure the audio for the engine is playing.
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

        EngineAudio();
	}


	private void EngineAudio()
	{
		// Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.

		if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f) 
		{
			if (m_MovementAudio.clip == m_EngineDriving) 
			{
				m_MovementAudio.clip = m_EngineIdling;
				m_MovementAudio.pitch = UnityEngine.Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
				m_MovementAudio.Play ();
			}
		} 
		else 
		{

			if (m_MovementAudio.clip == m_EngineIdling) 
			{
				m_MovementAudio.clip = m_EngineDriving;
				m_MovementAudio.pitch = UnityEngine.Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
				m_MovementAudio.Play ();
			}
		}
	}


	private void FixedUpdate()
	{
		// Move and turn the tank.
		Move ();
		Turn ();
	}


	public void Move()
	{
		// Adjust the position of the tank based on the player's input.
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
        Vector3 pos = m_Rigidbody.position + movement;
        
        m_Rigidbody.MovePosition(pos);

        if (movement != Vector3.zero)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            
            data["x"] = Math.Round((Decimal)pos.x, 4, MidpointRounding.AwayFromZero).ToString();
            data["y"] = Math.Round((Decimal)pos.y, 4, MidpointRounding.AwayFromZero).ToString();
            data["z"] = Math.Round((Decimal)pos.z, 4, MidpointRounding.AwayFromZero).ToString();
            
            socket.Emit("Move", new JSONObject(data));
        }

        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
       
	}


	public void Turn()
	{
        // Adjust the rotation of the tank based on the player's input.
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

        if (turn != 0)
        {
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);

            Dictionary<string, string> data = new Dictionary<string, string>();
            
            data["x"] = Math.Round((Decimal)m_Rigidbody.rotation.x, 4, MidpointRounding.AwayFromZero).ToString();
            data["y"] = Math.Round((Decimal)m_Rigidbody.rotation.y, 4, MidpointRounding.AwayFromZero).ToString();
            data["z"] = Math.Round((Decimal)m_Rigidbody.rotation.z, 4, MidpointRounding.AwayFromZero).ToString();
            data["w"] = Math.Round((Decimal)m_Rigidbody.rotation.w, 4, MidpointRounding.AwayFromZero).ToString();
            
            socket.Emit("Turn", new JSONObject(data));
        }
	}

}
