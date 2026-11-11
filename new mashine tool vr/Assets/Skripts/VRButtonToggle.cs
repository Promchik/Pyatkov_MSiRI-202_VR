using UnityEngine;

using Valve.VR.InteractionSystem;

public class VRButtonToggle : MonoBehaviour
{
    [Header("Ссылка на станок")]
    public MillControl millScript;

    [Header("Настройки теста на ПК")]
    public bool allowSpacebarTest = true;
    public bool allowMouseClickTest = true;

    void Start()
    {
        if (millScript == null)
        {
            millScript = FindFirstObjectByType<MillControl>();
        }
    }

    void Update()
    {
        
        if (allowSpacebarTest && Input.GetKeyDown(KeyCode.Space))
        {
            ToggleMill();
        }
    }

    
    private void OnMouseDown()
    {
        if (allowMouseClickTest)
        {
            ToggleMill();
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
        
        Hand hand = other.GetComponentInParent<Hand>();

        if (hand != null)
        {
            ToggleMill();
        }
    }

    public void ToggleMill()
    {
        if (millScript == null) return;

        
        millScript.isWorking = !millScript.isWorking;

        Debug.LogWarning($"[ПЕРЕКЛЮЧАТЕЛЬ] Станок изменен! IsWorking = {millScript.isWorking}");
    }
}