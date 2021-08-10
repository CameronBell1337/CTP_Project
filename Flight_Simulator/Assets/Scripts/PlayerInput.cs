using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [HideInInspector] public bool inputJump;
    [HideInInspector] public bool canInput;
    [HideInInspector] public bool canAim;

    [HideInInspector] public float horizontal;
    [HideInInspector] public float vertical;
    [HideInInspector] public float mouseX;
    [HideInInspector] public float mouseY;
    [HideInInspector] public Vector3 moveCharacter;

    public float mouseSensitivity = 100.0f;
    void Start()
    {
        canInput = true;
        canAim = true;
    }

    // Update is called once per frame
    void Update()
    {
        while (canInput)
        {
           // inputJump = Input.GetButtonDown("Jump");
            //horizontal = Input.GetAxisRaw("Horizontal");
            //vertical = Input.GetAxisRaw("Vertical");

            if (canAim)
            {
                mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
                mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
            }
            break;
        }
        moveCharacter = transform.right * horizontal + transform.forward * vertical;
    }

    public void KillMoveInput()
    {

    }

    public void KillInputs()
    {
        inputJump = false;

    }
}
