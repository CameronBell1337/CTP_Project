using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private CharacterController controller;

    public float gravity = -9.81f;
    public float speed = 10.0f;
    public float velY;

    Vector3 velocity;

    void Start()
    {
        controller = FindObjectOfType<CharacterController>();
        input = FindObjectOfType<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }

    void PlayerMovement()
    {
        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(input.moveCharacter * speed * Time.deltaTime);
        controller.Move(velocity * Time.deltaTime);

    }
}
