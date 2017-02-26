using SocketIO;
using System.Collections.Generic;
using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    SocketIOComponent socket;

    public LayerMask m_CarMask;
    public ParticleSystem m_ExplosionParticles;       
    public AudioSource m_ExplosionAudio;              
    public float m_MaxDamage = 100f;                  
    public float m_ExplosionForce = 1000f;            
    public float m_MaxLifeTime = 2f;                  
    public float m_ExplosionRadius = 5f;

    private void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>(); 
        // Find all the cars in an area around the shell and damage them.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_CarMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (this.gameObject.name != "Shell " + colliders[i].gameObject.name)
            {
                Rigidbody targetRigidBody = colliders[i].GetComponent<Rigidbody>();
                if (!targetRigidBody)
                    continue;

                targetRigidBody.AddExplosionForce(m_ExplosionForce, transform.position, 5);

                if (this.gameObject.name == "Shell " + GameObject.Find("Controller").GetComponent<Controller>().username)
                {
                    Dictionary<string, string> data1 = new Dictionary<string, string>();
                    Vector3 pos = targetRigidBody.transform.position;
                    data1["position"] = pos.x + "," + pos.y + "," + pos.z;
                    data1["player_name"] = targetRigidBody.gameObject.name;
                    socket.Emit("UPDATE_POSITION", new JSONObject(data1));
                }

                CarHealth targetHealth = targetRigidBody.GetComponent<CarHealth>();
                if (!targetHealth)
                    continue;

                float damage = CalculateDamage(targetRigidBody.position);
                Debug.Log("damaged: " + damage);
                //targetHealth.TakeDamage(damage);

                if (this.gameObject.name == "Shell " + GameObject.Find("Controller").GetComponent<Controller>().username)
                {
                    Dictionary<string, string> data2 = new Dictionary<string, string>();
                    data2["player_name"] = colliders[i].gameObject.name;
                    data2["health_decrease"] = damage + "";

                    socket.Emit("HEALTH_DECREASE", new JSONObject(data2));
                }
            }
        }

        m_ExplosionParticles.transform.parent = null;
        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);
        Destroy(gameObject);
    }


    private float CalculateDamage(Vector3 targetPosition)
    {
        // Calculate the amount of damage a target should take based on it's position.
        Vector3 explosionToTarget = targetPosition - transform.position;
        float explosionDistance = explosionToTarget.magnitude;
        float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;
        float damage = relativeDistance * m_MaxDamage;

        damage = Mathf.Max(0f, damage);

        return damage;
    }
}