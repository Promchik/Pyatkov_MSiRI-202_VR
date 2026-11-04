using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {

    }

    void OnCollisionEnter(UnityEngine.Collision collision)
    {
        print(gameObject.name + " врезался в " + collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        print(gameObject.name + " пересёк триггер: " + other.gameObject.name);
    }
}
