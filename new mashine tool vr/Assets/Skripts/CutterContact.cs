using UnityEngine;

public class CutterContact : MonoBehaviour
{
    [Header("Эффект стружки")]
    public ParticleSystem chipsParticles;

    private MillControl millScript;
    private bool isTouchingPiece = false;

    void Start()
    {
        
        millScript = FindFirstObjectByType<MillControl>();

        if (chipsParticles == null)
        {
            chipsParticles = GetComponentInChildren<ParticleSystem>();
        }

        
        if (chipsParticles != null) chipsParticles.Stop();
    }

    void Update()
    {
        if (millScript == null || chipsParticles == null) return;

        
        if (millScript.isWorking && isTouchingPiece)
        {
            if (!chipsParticles.isPlaying)
            {
                chipsParticles.Play();
            }
        }
        else
        {
            if (chipsParticles.isPlaying)
            {
                chipsParticles.Stop();
            }
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.GetComponent<Valve.VR.InteractionSystem.Throwable>() != null)
        {
            isTouchingPiece = true;
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Valve.VR.InteractionSystem.Throwable>() != null)
        {
            isTouchingPiece = false;
        }
    }
}