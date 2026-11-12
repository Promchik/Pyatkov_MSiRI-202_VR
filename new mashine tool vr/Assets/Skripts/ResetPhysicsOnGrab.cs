using UnityEngine;

public class ResetPhysicsOnGrab : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Вызывается в момент взятия в руку (или клика)
    public void DisableKinematic()
    {
        // Отвязываем от стола, если деталь была приклеена
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}