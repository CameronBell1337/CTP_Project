using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AircraftController : MonoBehaviour
{
    public Rigidbody bod { get; internal set; }

    [Header("Test out experimental system")]
    [Tooltip("Uses of simplified or more complex flight system (STILL BUGGY AND EXPERIMENTAL!)")]
    public bool setSimpleFlightSystem = false;

    [Header("Tail Control Surfaces")]
    [Space()]



    [Header("Flight System")]
    public WingSurface rudder;
    public WingSurface leftElevator;
    public WingSurface rightElevator;

    [Header("Wings Control Surfaces")]
    public WingSurface aileronLeft;
    public WingSurface aileronRight;
    public WingSurface flapLeft;
    public WingSurface flapRight;

    [Header("Wheels")]
    public List<WheelCollider> wheels;
    public float breakTorque = 1000.0f;
    public float breakTouqueMax = 1000.0f;

    [Header("Engine")]
    public GameObject[] propeller;
    public float EngineRPM;
    public int minRequiredEngineRPM = 130;
    public AudioSource[] mixer;
    public float thrust = 6000f;

    [Header("Aircraft Parameters")]
    [Space()]
    public float power = 0f;
    public float weight = 1500.0f;
    
    [Header("Current Flap State")]
    public flapState currentFlapState = flapState.normal;

    [Header("Controls")]
    [Range(-1, 1)]
    public float pitch;
    [Range(-1, 1)]
    public float yaw;
    [Range(-1, 1)]
    public float roll;


    private Text HUD = null;


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


    private AerodynamicController[] surface;

    

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

        surface = GetComponentsInChildren<AerodynamicController>();
    }

    private void Start()
    {
        HUD = GameObject.FindGameObjectWithTag("HUD").GetComponent<Text>();
        InitPlane();
        bod = GetComponent<Rigidbody>();
        power = 0;
        bod.mass = weight;
        foreach (var sound in mixer)
        {
            sound.pitch = 0;
        }
        StartCoroutine(ToggleFLap());

       foreach (var system in surface)
       {
         system.SetSystemSimple = setSimpleFlightSystem;
       }
    }

    private void Update()
    {
        Inputs();

        UIElements();
    }

    void Inputs()
    {
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");
        roll = Input.GetAxis("Roll");

        if (rudder != null)
        {
            rudder.targetDeflec = yaw;
        }

        if (leftElevator != null && rightElevator != null)
        {
            leftElevator.targetDeflec = -pitch;
            rightElevator.targetDeflec = -pitch;
        }

        if (aileronLeft != null)
        {
            aileronLeft.targetDeflec = -roll;
        }
        if (aileronRight != null)
        {
            aileronRight.targetDeflec = roll;
        }

        if (wheels != null)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                breakTorque = breakTorque > 0 ? 0 : breakTouqueMax;
            }
        }

        if (flapLeft != null && flapRight != null)
        {
            if (Input.GetButtonDown("Flap"))
            {

                StartCoroutine(ToggleFLap());

            }
        }

       

        power += 1f * scrollSens * Input.GetAxis("MWheelD") * Time.deltaTime;
        power = Mathf.Clamp01(power);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        CalculateRPM();

        SetControllerParams();

        if (EngineRPM > minRequiredEngineRPM)
        {
            bod.AddRelativeForce(Vector3.forward * (thrust * power), ForceMode.Force);
        }

        foreach (var wheel in wheels)
        {
            //stops wheels from getting stuck and not moving when at 0 vel
            wheel.brakeTorque = breakTorque;
            wheel.motorTorque = 0.01f;
        }

        //Engine sounds
        float clamp = Mathf.Clamp((zAngle / 10f), 0.0f, 1.8f);

        foreach (var sound in mixer)
        {
            sound.pitch = clamp;
        }
    }

    void SetControllerParams()
    {
        foreach (var parms in surface)
        {
            parms.Thrust = thrust;
            parms.power = power;
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
            zAngle = Mathf.LerpAngle(zAngle, thrust* power * Time.deltaTime, Time.deltaTime);

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

    void UIElements()
    {
        float mPs2Knots = bod.velocity.magnitude * 1.943844f;
        float meters2Feet = transform.position.y * 3.28084f;
        float power2String = power * 100;
        HUD.text = "Speed: " + mPs2Knots.ToString("000.00") + " kn\n";
        HUD.text += "Altitude: " + meters2Feet.ToString("0000.00") + " ft\n";
        HUD.text += "Engine Power: " + power2String.ToString("000") + "%\n";
        HUD.text += breakTorque > 0 ? "Brakes: ON" + "\n" : "Brakes: OFF" + "\n";
        HUD.text += "Flaps: " + currentFlapState.ToString() + "\n";
       
    }
}
