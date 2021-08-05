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

    //Getters
    #region Getters
    public float GetWingArea
    {
        get { return wingDimention.x * wingDimention.y; }
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
        Vector3 localVelocity = isFlap ? transform.TransformDirection(bod.GetPointVelocity(transform.position)) : transform.InverseTransformDirection(bod.GetPointVelocity(transform.position));

        //takes predicted and current vel and averages
        Vector3 predictVel = bod.velocity + Time.deltaTime * 0.5f * ForceApply / bod.mass;
        Vector3 averageVel = (localVelocity + predictVel) * 0.5f;

        localVelocity.x = 0f;

        AoA = Vector3.Angle(Vector3.forward, localVelocity);
        //CalculateAoA(localVelocity);
        lCoeff = wing.GetLiftCurAoA(AoA);
        dCoeff = wing.GetDragCurAoA(AoA);

        lift = localVelocity.sqrMagnitude * lCoeff * GetWingArea * lm;
        drag = localVelocity.sqrMagnitude * dCoeff * GetWingArea * dm;

        lift *= -Mathf.Sign(localVelocity.y);

        //Cross Section of airfoil needed as lift is always perpendicular to the direction of airflow
        liftDir = Vector3.Cross(bod.velocity, transform.right).normalized;
        bod.AddForceAtPosition(liftDir * lift, 
            ForceApply, 
            ForceMode.Force);

        //Drag always opposite of the current velocity
        bod.AddForceAtPosition(-bod.velocity.normalized * drag, 
            ForceApply, 
            ForceMode.Force);
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

    private float CalculateLift(Vector3 localVel, float lCoeff)
    {
        lift = localVel.sqrMagnitude * lCoeff * GetWingArea * lm;
        return lift;
    }

    private float CalculateDrag(Vector3 localVel, float dCoeff)
    {
        drag = localVel.sqrMagnitude * dCoeff * GetWingArea * dm;
        return drag;
    }

    private float CalculateAoA(Vector3 localVel)
    {
        AoA = Vector3.Angle(Vector3.forward, localVel);

        return AoA;
    }

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
