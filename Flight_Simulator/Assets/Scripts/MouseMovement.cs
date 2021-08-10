using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public Transform characterBod;

    private PlayerInput input;

    float xRotation = 0.0f;

    void Start()
    {
        input = FindObjectOfType<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void MouseInput()
    {
        xRotation -= input.mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        characterBod.Rotate(Vector3.up * input.mouseX);
    }
}
