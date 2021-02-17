using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    private CharacterController controller;

    public float gravity = -9.81f;
    public float speed = 10.0f;


    public float velY;
    public float mass = 100.0f;

    public bool test = false;
    [SerializeField] private Vector3 velocity;


    private bool inputJump;
    private bool canInput;
    private bool canAim;

    private float horizontal;
    private float vertical;
    private float mouseX;
    private float mouseY;
    private float xRotation = 0.0f;

    private Vector3 moveCharacter;

    public Material mat;

    public float mouseSensitivity = 100.0f;

    void Start()
    {
        canInput = true;
        canAim = true;

        controller = GetComponent<CharacterController>();
       
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {

        while (canInput)
        {
            inputJump = Input.GetButtonDown("Jump");
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            if (canAim)
            {
                mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
                mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
            }
            break;
        }
        moveCharacter = transform.right * horizontal + transform.forward * vertical;

        //controller.Move(Time.deltaTime * (moveCharacter * speed));
        controller.Move(velocity * Time.deltaTime);
        

        
    }

    private void FixedUpdate()
    {
        while (!controller.isGrounded)
        {

            Velocity();
            test = false;

            //mat.color = Color.green;

            gameObject.GetComponent<Renderer>().material.color = Color.green;
            break;
        }

        if(controller.isGrounded)
        {
            velY = 0f;
            test = true;
            gameObject.GetComponent<Renderer>().material.color = Color.red;
            //mat.color = Color.red;
        }
    }

    void Velocity()
    {
        velY += Time.fixedDeltaTime * gravity;
        velocity = Vector3.up * velY;
        
    }

    public void MouseInput()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}

