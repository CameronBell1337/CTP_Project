using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    [Header("Wheels")]
    public List<WheelCollider> wheels;
    public float breakTorque = 100f;

    [Header("Engine")]
    public GameObject[] propeller;
    public float EngineRPM;


    [Header("Aircraft Parameters")]
    [Space()]
    public float thrust = 6000f;
    public float power = 0f;
    public float weight = 1500.0f;

    public flapState currentFlapState = flapState.normal;
    public enum flapState
    {
        normal = 0,
        lowered = 1,
        raised = 2
    }



    private float scrollSens = 2.0f;
    private float zAngle;
    private float previousZEuler = 0;
    private float currentZEuler = 0;

    private int flapToggleInt = 0;

    public AudioSource mixer;

    void InitPlane()
    {
        if (rudder != null)
        {
            rudder.IsRudder = true;
        }

        if (aileronRight != null)
        {
            aileronRight.IsInverse = true;
        }

        if (rudder == null)
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

    private void Start()
    {
        InitPlane();
        bod = GetComponent<Rigidbody>();
        power = 0;
        bod.mass = weight;
        mixer.pitch = 0;
        StartCoroutine(ToggleFLap());


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

        if (wheels != null)
        {
            if(Input.GetKeyDown(KeyCode.B))
            {
                breakTorque = breakTorque > 0 ? 0 : 100.0f;
            }
        }

        if (flapLeft != null && flapRight != null)
        {
            if (Input.GetButtonDown("Flap"))
            {

                StartCoroutine(ToggleFLap());
                
            } 
        }

        if(aileronLeft != null)
        {
            aileronLeft.targetDeflec = -Input.GetAxis("Roll");
        }
        if (aileronRight != null)
        {
            aileronRight.targetDeflec = Input.GetAxis("Roll");
        }

        power += 1f * scrollSens * Input.GetAxis("MWheelD") * Time.deltaTime;
        power = Mathf.Clamp01(power);


        
        

        if (mixer.pitch >= 1.6)
        {
            mixer.pitch = 1.6f;
        }
        else
        {
            mixer.pitch = zAngle /10;
        }
        



    }

    // Update is called once per frame
    void FixedUpdate()
    {

        CalculateRPM();

        if (EngineRPM > 130)
        {
            bod.AddRelativeForce(Vector3.forward * (thrust * power), ForceMode.Force);
        }

        foreach (var wheel in wheels)
        {
            //stops wheels from getting stuck and not moving when at 0 vel
            wheel.brakeTorque = breakTorque;
            wheel.motorTorque = 0.01f;
        }
    }


    void CalculateRPM()
    {
        
        currentZEuler = propeller[0].transform.localRotation.eulerAngles.z;
        
        float degreesPerSec = Mathf.Abs(currentZEuler - previousZEuler) / Time.fixedDeltaTime;
        //0.166666666667f is from a webstie for calculating rpm from Deg/Sec
        EngineRPM = 0.166666666667f * degreesPerSec;
        Mathf.Floor(EngineRPM);
        previousZEuler = currentZEuler;

        if (power == 0 && EngineRPM <=45)
        {
            zAngle = 0;
        }
        else
        {
            zAngle = Mathf.LerpAngle(zAngle, (thrust*2) * power * Time.deltaTime, Time.deltaTime / propeller[0].transform.localRotation.eulerAngles.z);

            foreach (var i in propeller)
            {
                i.transform.Rotate(0.0f, 0.0f, zAngle, Space.Self);
            }
        }

      
        
    }


    IEnumerator ToggleFLap()
    {
        if (flapToggleInt > 2)
        {
            flapToggleInt = 0;
        }

        switch (flapToggleInt)
        {
            case 0:
                {
                    currentFlapState = flapState.normal;
                    flapLeft.targetDeflec = 0;
                    flapRight.targetDeflec = 0;
                    break;
                }
            case 1:
                {
                    currentFlapState = flapState.lowered;
                    flapLeft.targetDeflec = -1;
                    flapRight.targetDeflec = -1;
                    break;
                }
            case 2:
                {
                    currentFlapState = flapState.raised;
                    flapLeft.targetDeflec = 1;
                    flapRight.targetDeflec = 1;
                    break;
                }
        }
        yield return new WaitForSeconds(.1f);
        flapToggleInt++;
    }
}
