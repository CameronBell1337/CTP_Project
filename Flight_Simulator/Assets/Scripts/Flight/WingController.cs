using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingController : MonoBehaviour
{

    //A = span^2 * cord
    //A = .5 * (chortTip + chordRoot) * Semi-Span
    //A = .5 *
    //AR = legnth^2 / WingArea


    [Header("Wing Specs")]
    public float AR;
    [Tooltip("Dimention of each wing")]
    public Vector2 wingDimention = new Vector2(0f, 0f);
    public bool autoAR = false;

    [Header("Control Surfaces")]
    [Tooltip("Apply to Global or Local COM")]
    public bool applyToCentre = false;
    [Tooltip("Apply to flaps ONLY!")]
    public bool isFlap = false;
    [Tooltip("Apply to Rudder ONLY!")]
    public bool isRudder = false;

    [Tooltip("Flight Curve")]
    public FlightCurve wing;
    [Tooltip("Max Forces Variable for bigger planes need clamping due to calculation errors occuring")]
    public float maxForce = 100000f;



    //Getters
    #region Getters
    public float GetWingArea
    {
        get { return wingDimention.y * wingDimention.x; }
    }

    //Coefficients
    public float GetLiftCoefficient
    {
        get { return lCoeff; }
    }
    public float GetDragCoefficient
    {
        get { return dCoeff; }
    }

    public bool SetSystemSimple
    { get; set; }



    public float Thrust { get; set; }
    public float power { get; set; }

    public float AngleOfAttack
    {
        get
        {
            if (bod != null)
            {
                Vector3 localVelocity = transform.InverseTransformDirection(bod.velocity);
                return AoA * -Mathf.Sign(localVelocity.y);
            }
            else
            {
                return 0.0f;
            }
        }
    }

    //Forces
    public float GetLift
    {
        get { return lift; }
    }
    public float GetDrag
    {
        get { return drag; }
    }

    public Rigidbody Bod
    {
        set { bod = value; }
    }
    #endregion

    //********** PRIVATE VARIABLES **********//
    private float lm = 1f;
    private float dm = 1f;

    private Rigidbody bod;

    private float lCoeff = 0f;
    private float dCoeff = 0f;
    private float lift = 0f;
    private float drag = 0f;
    private float AoA = 0f;

    private Vector3 liftDir = Vector3.up;
    private MandAM currentFT;

    private void Awake()
    {
        bod = GetComponentInParent<Rigidbody>();
        if (autoAR) { AR = wingDimention.x / wingDimention.y; }

        //wing.IsRudder
    }

    private void FixedUpdate()
    {
        Vector3 ForceApply = (applyToCentre) ? bod.transform.TransformPoint(bod.centerOfMass) : transform.position;

        //Check f it is a flap as will want to apply to same direction flap is rotating towards rather than opposite.

        if (SetSystemSimple)
        {
            CalculateForcesSimple();
        }
        else
        {
            //use in combinations with simple to get a average between two system but costly on pc
            CalculateForcesSimple();
            CalculateMoreComplexData();
        }
        

    }

    private void Update()
    {
        //Error checks
        if(wingDimention.x <= 0f)
        {
            wingDimention.x = .01f;
        }

        if(wingDimention.y <= 0f)
        {
            wingDimention.y = .0f;
        }


        if (bod != null)
        {
            Debug.DrawRay(transform.position, liftDir * lift * 0.0001f, Color.blue);
            Debug.DrawRay(transform.position, -bod.velocity.normalized * drag * 0.002f, Color.red);
        }

    }

    void CalculateForcesSimple()
    {
        Vector3 ForceApply = (applyToCentre) ? bod.transform.TransformPoint(bod.centerOfMass) : transform.position;


        //Check f it is a flap as will want to apply to same direction flap is rotating towards rather than opposite.
        Vector3 localVelocity = isFlap ? transform.InverseTransformDirection(bod.GetPointVelocity(transform.position)) : transform.InverseTransformDirection(bod.GetPointVelocity(transform.position));

        localVelocity.x = 0f;

        AoA = Vector3.Angle(Vector3.forward, localVelocity);

        

        //CalculateAoA(localVelocity);
        lCoeff = wing.GetLiftCurAoA(AoA);
        dCoeff = wing.GetDragCurAoA(AoA);

        //deltaP = 1/2 * cL * airDens * airFlow * Area - Airflow ignored 
        lift = 0.5f* (lCoeff * localVelocity.sqrMagnitude * GetWingArea * lm);

        //deltaP = 1/2 * cD * airDens * airFlow * Area - Airflow ignored
        drag = 0.5f* (dCoeff * localVelocity.sqrMagnitude  * GetWingArea * dm);

        lift *= -Mathf.Sign(localVelocity.y);

        //Cross Section of airfoil needed as lift is always perpendicular to the direction of airflow
        liftDir = Vector3.Cross(bod.velocity, transform.right).normalized;

        //HAD TO CLAMP AS BIGGER AIRCRAFT WOULD GO OUT OF CONTROL BECAUSE OF INACCURATE OR OUT OF SCOPE FLOATS
        bod.AddForceAtPosition(Vector3.ClampMagnitude(liftDir * lift, maxForce),
            ForceApply,
            ForceMode.Force);

        //HAD TO CLAMP AS BIGGER AIRCRAFT WOULD GO OUT OF CONTROL BECAUSE OF INACCURATE OR OUT OF SCOPE FLOATS
        //Drag always opposite of the current velocity
        bod.AddForceAtPosition(Vector3.ClampMagnitude(-bod.velocity.normalized * drag, maxForce),
            ForceApply,
            ForceMode.Force);
    }


    void CalculateMoreComplexData()
    {
        MandAM ftThisStep = CalculatePhysicsForces(bod.velocity, bod.angularVelocity, Vector3.zero, 1.2f, bod.worldCenterOfMass);
        Vector3 velPred = GetVelPred(ftThisStep.m + transform.position + Physics.gravity * bod.mass);
        Vector3 aVelPred = GetAVelPred(ftThisStep.aM);

        MandAM ftPred = CalculatePhysicsForces(velPred, aVelPred, Vector3.zero, 1.2f, bod.worldCenterOfMass);

        currentFT = (ftThisStep + ftPred) * 0.5f;

        bod.AddForce(currentFT.m);
        bod.AddForce(currentFT.aM);

    }


    private MandAM CalcuateForces(Vector3 worldAirVel, float aD, Vector3 relPos)
    {
        MandAM momentumAndAngularMC = new MandAM();


        /*Calculates air velocity relative to wing coords
        *no point using Z value as would put too much strain on system and only a 
        *little bit of surface friction is created along the chord of wing surface
        */
        Vector3 airVel = transform.InverseTransformDirection(worldAirVel);
        airVel = new Vector3(airVel.x, airVel.y);

        Vector3 dragDir = transform.TransformDirection(airVel.normalized);
        Vector3 liftDir = Vector3.Cross(dragDir, transform.forward);

        float dP = 0.5f * aD * airVel.sqrMagnitude;

        Vector3 localVelocity = transform.InverseTransformDirection(bod.GetPointVelocity(transform.position));

       float _aoa = Mathf.Atan2(airVel.y, -airVel.x);

        Debug.Log(AngleOfAttack);

        float liftCo = wing.GetLiftCurAoA(_aoa);
        float dragCo = wing.GetDragCurAoA(_aoa);

        Vector3 lift = liftDir *   liftCo * dP * GetWingArea;
        
        Vector3 drag = dragDir *  dragCo * dP * GetWingArea;
        //A really small negative number so doesnt steer off to the side
        Vector3 angularMomentum = -transform.forward * -0.02045139f * dP * GetWingArea * wingDimention.y;

        momentumAndAngularMC.m += (lift + drag);
        momentumAndAngularMC.aM += Vector3.Cross(relPos, momentumAndAngularMC.m);
        momentumAndAngularMC.aM += angularMomentum;

        return momentumAndAngularMC;

    }


    private MandAM CalculatePhysicsForces(Vector3 v, Vector3 aV, Vector3 aF, float aD, Vector3 com)
    {
        //substep
        MandAM mAndAM = new MandAM();

        Vector3 relPos = transform.position - com;
        mAndAM += CalcuateForces(-v + aF -Vector3.Cross(aV, relPos), aD, relPos);

        return mAndAM;
    }

    // *************** PREDICTION EQUATIONS *************** //
    private Vector3 GetVelPred(Vector3 f)
    {
        return bod.velocity + Time.deltaTime * 0.5f * f / bod.mass;
    }

    private Vector3 GetAVelPred(Vector3 aM)
    {
        Quaternion iTWorldRotation = bod.rotation * bod.inertiaTensorRotation;
        Vector3 angMomentumWSpace = Quaternion.Inverse(iTWorldRotation) * aM;
        Vector3 angVelChange;
        angVelChange.x = angMomentumWSpace.x / bod.inertiaTensor.x;
        angVelChange.y = angMomentumWSpace.y / bod.inertiaTensor.y;
        angVelChange.z = angMomentumWSpace.z / bod.inertiaTensor.z;

        return bod.angularVelocity + Time.deltaTime * 0.5f * (iTWorldRotation * angVelChange);
    }


    // *************** PREDICTION EQUATIONS *************** //

    // *************** LIFT EQUATIONS *************** //

    //deltaP = 1/2 * cL * airDens * airFlow * Area
    private float LiftEquation(float _cl, float _airDens, float _airFlowSpeed, float _wingArea )
    {
        //UNSED CALCULATED A DIFFERENT WAY
        return 0.5f * _cl * _airDens * _airFlowSpeed * _wingArea;
    }

    private float GetLiftCoEff(float _aoa)
    {
        //UNSED CALCULATED A DIFFERENT WAY
        return lCoeff = wing.GetLiftCurAoA(_aoa);
    }

    // *************** LIFT EQUATIONS *************** //


    // *************** DRAG EQUATIONS *************** //
    private float CalculateDrag(Vector3 localVel, float dCoeff)
    {
        //UNSED CALCULATED A DIFFERENT WAY
        drag = localVel.sqrMagnitude * dCoeff * GetWingArea * dm;
        return drag;
    }

    //deltaP = 1/2 * cD * airDens * airFlow * Area
    private float DragEquation(float _dL, float _airDens, float _airFlowSpeed, float _wingArea)
    {
        //UNSED CALCULATED A DIFFERENT WAY
        return 0.5f * _dL * _airDens * _airFlowSpeed * _wingArea;
    }



    // *************** DRAG EQUATIONS *************** //



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Matrix4x4 oldMatrix = Gizmos.matrix;
        if (bod != null)
        {
            if (applyToCentre)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(bod.transform.TransformPoint(bod.centerOfMass), .2f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, .2f);
            }
        }
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, new Vector3(wingDimention.x, 0f, wingDimention.y));
        Gizmos.matrix = oldMatrix;
    }
#endif

}
