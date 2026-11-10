using UnityEngine;
using Valve.VR.InteractionSystem; 

public class TableSnapZone : MonoBehaviour
{
    [Header("Куда крепить деталь (обычно сам стол)")]
    public Transform tableTransform;

    void Start()
    {
        
        if (tableTransform == null)
        {
            tableTransform = transform.parent;
        }
    }

    
    private void OnTriggerStay(Collider other)
    {
       
        Throwable throwable = other.GetComponent<Throwable>();

        if (throwable != null)
        {
            
            Interactable interactable = other.GetComponent<Interactable>();
            if (interactable != null && interactable.attachedToHand != null)
            {
                
                return;
            }

            
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                
                other.transform.SetParent(tableTransform);

                
                rb.isKinematic = true;

                Debug.LogWarning($"[СТОЛ] Деталь {other.name} установлена на станок и зафиксирована!");
            }
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        Throwable throwable = other.GetComponent<Throwable>();
        if (throwable != null)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                
                rb.isKinematic = false;
            }

           
            other.transform.SetParent(null);
            Debug.LogWarning($"[СТОЛ] Деталь {other.name} снята со станка.");
        }
    }
}