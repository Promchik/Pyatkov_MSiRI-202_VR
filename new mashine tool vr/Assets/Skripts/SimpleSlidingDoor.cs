using UnityEngine;
using Valve.VR.InteractionSystem;

public class SimpleSlidingDoor : MonoBehaviour
{
    [Header("Настройки открытия")]
    [Tooltip("Направление и дистанция открытия в метрах (например, X = 0.5 сдвинет вправо)")]
    public Vector3 openOffset = new Vector3(0.5f, 0f, 0f);

    [Tooltip("Чувствительность перетаскивания мышкой")]
    public float mouseSensitivity = 0.02f;

    private Vector3 closedLocalPos;
    private float openProgress = 0f; // 0 = закрыто, 1 = открыто
    private Hand attachedHand;

    void Start()
    {
        // Запоминаем точное закрытое положение двери прямо на старте
        closedLocalPos = transform.localPosition;
    }

    // 1. УПРАВЛЕНИЕ МЫШКОЙ ДЛЯ ТЕСТА НА ПК
    void OnMouseDrag()
    {
        // Считываем движение мышки влево-вправо
        float delta = Input.GetAxis("Mouse X") * mouseSensitivity;
        UpdateDoorPosition(openProgress + delta);
    }

    // 2. УПРАВЛЕНИЕ РУКОЙ ДЛЯ VR-ШЛЕМА (SteamVR)
    private void HandAttachedUpdate(Hand hand)
    {
        // Вычисляем, насколько рука сдвинулась вдоль направления открытия
        Vector3 worldClosedPos = transform.parent != null ? transform.parent.TransformPoint(closedLocalPos) : closedLocalPos;
        Vector3 worldOpenDir = transform.parent != null ? transform.parent.TransformDirection(openOffset) : openOffset;

        float progress = Vector3.Dot(hand.transform.position - worldClosedPos, worldOpenDir) / worldOpenDir.sqrMagnitude;
        UpdateDoorPosition(progress);
    }

    // Общая функция перемещения двери с жесткими ограничителями от 0 до 1
    private void UpdateDoorPosition(float newProgress)
    {
        openProgress = Mathf.Clamp01(newProgress);
        transform.localPosition = Vector3.Lerp(closedLocalPos, closedLocalPos + openOffset, openProgress);
    }
}
