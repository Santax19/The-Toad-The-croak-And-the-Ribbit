using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesMan: MonoBehaviour
{
    [Header("Referencias a partículas")]
    [SerializeField] private ParticleSystem[] explosionParticles;

    public void PlayShotParticles()
    {
        foreach (var ps in explosionParticles)
        {
            if (ps == null) continue;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }
    }
}
