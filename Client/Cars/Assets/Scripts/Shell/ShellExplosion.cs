using SocketIO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public string name = "";

    public LayerMask m_CarMask;
    public ParticleSystem m_ExplosionParticles;
    public AudioSource m_ExplosionAudio;
    public float m_MaxDamage = 100f;
    public float m_ExplosionForce = 1000f;
    public float m_MaxLifeTime = 2f;
    public float m_ExplosionRadius = 5f;

    private SocketIOComponent socket;

    private void Start()
    {
        gameObject.name = name;
        Destroy(gameObject, m_MaxLifeTime);
        socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
    }


    private void OnTriggerEnter(Collider other)
    {
        // Find all the cars in an area around the shell and damage them.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_CarMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidBody = colliders[i].GetComponent<Rigidbody>();
            if (!targetRigidBody)
                continue;

            //targetRigidBody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            //CarHealth targetHealth = targetRigidBody.GetComponent<CarHealth>();
            //if (!targetHealth)
            //    continue;


            float damage = CalculateDamage(targetRigidBody.position);
            //targetHealth.TakeDamage(damage);
            // Đang suy nghĩ nó có đồng bộ giữa các client ko
            //

            Dictionary<string, string> data = new Dictionary<string, string>();
            data["id"] = targetRigidBody.name;
            data["damage"] = Math.Round((Decimal)damage, 4, MidpointRounding.AwayFromZero).ToString();

            socket.Emit("TakeDamage", new JSONObject(data));
        }

        Play();
    }

    public void Play()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();
        data["id"] = GameObject.FindGameObjectWithTag("Player").name;
        socket.Emit("DestroyShell", new JSONObject());

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