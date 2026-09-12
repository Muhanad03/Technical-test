using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSensitivity = 0.15f;

    float yaw;
    float pitch;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // Hold right mouse to look; leave left mouse available for placement.
        if (mouse.rightButton.isPressed)
        {
            Vector2 look = mouse.delta.ReadValue();
            yaw += look.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, -85f, 85f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        Vector3 movement = Vector3.zero;
        if (keyboard.wKey.isPressed) movement += Vector3.forward;
        if (keyboard.sKey.isPressed) movement += Vector3.back;
        if (keyboard.aKey.isPressed) movement += Vector3.left;
        if (keyboard.dKey.isPressed) movement += Vector3.right;
        transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.Self);
    }
}
