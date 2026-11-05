using UnityEngine;

public class qwerty : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;

    [Header("Настройки камеры")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2.0f;
    public float upDownLookLimit = 80.0f;

    private CharacterController characterController;
    private float verticalRotation = 0f;
    private Vector3 currentVelocity;

    void Start()
    {
    }

    void HandleMovement()
    {
        // Получаем ввод с клавиатуры (WASD / Стрелки)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Вычисляем направление движения относительно того, куда смотрит игрок
        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

        // Нормализуем вектор, чтобы движение по диагонали не было быстрее, и умножаем на скорость
        Vector3 move = moveDirection.normalized * moveSpeed;

        // Применяем простую гравитацию, чтобы объект прижимался к земле
        if (characterController.isGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f; // Небольшое прижатие к земле
        }
        else
        {
            currentVelocity.y += gravity * Time.deltaTime;
        }

        // Объединяем движение по осям XZ и падение по оси Y
        move.y = currentVelocity.y;

        // Двигаем персонажа через Character Controller
        characterController.Move(move * Time.deltaTime);
    }
}
