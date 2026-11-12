using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class PhysicalSpawnerButton : MonoBehaviour
{
    [Header("Настройки спавна")]
    [Tooltip("Перетащи сюда синий префаб заготовки из окна Project")]
    public GameObject workpiecePrefab;

    [Tooltip("Пустой объект-точка, где будет появляться деталь")]
    public Transform spawnPoint;

    [Header("Защита от переполнения")]
    [Tooltip("Максимум деталей в сцене одновременно (0 = без ограничений)")]
    public int maxWorkpieces = 5;

    [Header("Защита от двойного нажатия")]
    public float cooldown = 0.5f;

    private Queue<GameObject> spawnedList = new Queue<GameObject>();
    private float lastPressTime = -1f;

    // 1. ТЕСТ НА ПК: Клик левой кнопкой мыши по 3D-кнопке
    private void OnMouseDown()
    {
        TrySpawn();
    }

    // 2. ДЛЯ VR-ШЛЕМА: Физическое касание виртуальной рукой
    private void OnTriggerEnter(Collider other)
    {
        Hand hand = other.GetComponentInParent<Hand>();
        if (hand != null)
        {
            TrySpawn();
        }
    }

    // Общий метод с защитой от случайного дребезга рук
    public void TrySpawn()
    {
        // Проверяем, прошла ли задержка с прошлого нажатия
        if (Time.time - lastPressTime < cooldown) return;
        lastPressTime = Time.time;

        if (workpiecePrefab == null || spawnPoint == null)
        {
            Debug.LogError("[СПАВНЕР] Перетащите префаб заготовки и точку SpawnPoint в Инспектор кнопки!");
            return;
        }

        // Удаляем самую старую деталь, если достигнут лимит
        if (maxWorkpieces > 0 && spawnedList.Count >= maxWorkpieces)
        {
            GameObject oldest = spawnedList.Dequeue();
            if (oldest != null)
            {
                Destroy(oldest);
            }
        }

        // Спавним новую заготовку
        GameObject newPiece = Instantiate(workpiecePrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedList.Enqueue(newPiece);

        Debug.LogWarning("[СПАВНЕР] Физическая кнопка нажата — заготовка создана!");
    }
}