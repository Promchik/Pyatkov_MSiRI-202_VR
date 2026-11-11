using UnityEngine;

public class firstcode : MonoBehaviour
{
    public string owner;
    public float balance;

    
    void Start()
    {
        
        SecondScript second = GetComponent<SecondScript>();

      
        if (second != null)
        {
           
            owner = second.defaultOwner;
            balance = second.defaultBalance;

         
            PrintInfo();
        }
        else
        {
            Debug.LogError("Ошибка: Скрипт SecondScript не найден на этом объекте!");
        }
    }
    public void PrintInfo()
    {
        Debug.Log($"[firstcode] ПРИВЕТ ! Владелец: {owner}, Баланс: {balance} тенге.");
    }

    void Update()
    {
        
    }
}
