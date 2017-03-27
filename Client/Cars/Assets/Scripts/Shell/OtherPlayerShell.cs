using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerShell : MonoBehaviour {

    public string name;

    public ParticleSystem m_ExplosionParticles;
    public AudioSource m_ExplosionAudio;

    public LayerMask m_CarMask;

    private void Start()
    {
        gameObject.name = name;
    }

    private void OnTriggerEnter(Collider other)
    {
        Play();
    }

    public void Play()
    {
        m_ExplosionParticles.transform.parent = null;
        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);
        Destroy(gameObject);
    }
}
