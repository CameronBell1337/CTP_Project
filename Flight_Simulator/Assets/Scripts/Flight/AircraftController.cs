using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftController : MonoBehaviour
{
    public Rigidbody bod { get; internal set; }

    [Header("Tail")]
    public WingSurface rudder;
    public WingSurface leftElevator;
    public WingSurface rightElevator;

    [Header("Wings")]
    public WingSurface aileronLeft;
    public WingSurface aileronRight;
    public WingSurface flapLeft;
    public WingSurface flapRight;

    [Header("")]
    [Space()]
    public float thrust = 6000f;
    public float power = 0f;
    public bool isDown = false;
    void Awake()
    {
        bod = GetComponent<Rigidbody>();   
    }

    private void Start()
    {
        if(rudder == null)
        {
            Debug.LogWarning("No Rudder found!");
        }
        if (rightElevator == null && leftElevator == null)
        {
            Debug.LogWarning("No Elevators found!");
        }
        if (aileronLeft == null)
        {
            Debug.LogWarning("No Left aileron found!");
        }
        if (aileronRight == null)
        {
            Debug.LogWarning("No Right aileron found!");
        }
        if (flapLeft == null)
        {
            Debug.LogWarning("No Left flap found!");
        }
        if (flapRight == null)
        {
            Debug.LogWarning("No Right flap found!");
        }
    }

    private void Update()
    {

        if(rudder != null)
        {
            rudder.targetDeflec = Input.GetAxis("Yaw");
        }

        if(leftElevator != null && rightElevator != null)
        {
            leftElevator.targetDeflec = -Input.GetAxis("Pitch");
            rightElevator.targetDeflec = -Input.GetAxis("Pitch");
        }

        if (flapLeft != null && flapRight != null)
        {
            
            if (Input.GetButtonDown("Flap") && !isDown)
            {
                isDown = true;
            }
            else if(Input.GetButtonDown("Flap") && isDown)
            {
                isDown = false;
            }
            flapLeft.targetDeflec = isDown ? -1 : 0;
            flapRight.targetDeflec = isDown ? -1 : 0;
            
        }

        if(aileronLeft != null)
        {
            aileronLeft.targetDeflec = -Input.GetAxis("Roll");
        }
        if (aileronRight != null)
        {
            aileronRight.targetDeflec = Input.GetAxis("Roll");
        }

        power += Input.GetAxis("Mouse1") * Time.deltaTime;
        power -= Input.GetAxis("Mouse2") * Time.deltaTime;
        power = Mathf.Clamp01(power);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (power > 0)
        {
            bod.AddRelativeForce(Vector3.forward * thrust * power, ForceMode.Force);
        }
    }
}
