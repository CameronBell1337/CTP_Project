﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingSurface : MonoBehaviour
{
    [Header("Wing Surface Settings")]
    [Tooltip("Deflection with positive input."), Range(0, 90)]
    public float maxDeflectionAtPos = 16f;
    [Tooltip("Deflection with negative input."), Range(0, 90)]
    public float minDeflectionAtNeg = 16f;

    [Tooltip("Deflection with max positive input.")]
    public float rotSpeed = 90f;

    [Tooltip("Attempted deflection direction normalised at -1, 1."), Range(-1, 1)]
    public float targetDeflec = 0f;

    [SerializeField]
    [Tooltip("Access to the wing control surface may be attached too.")]
    private WingController wing = null;

    [Tooltip("Max force surface can stand before controls 'stiffen up'")]
    public float maxForce = 5000f;

    public GameObject controlSurfaceBody = null;
    public bool IsRudder { get; set; }

    public bool IsInverse { get; set; }


    private Rigidbody rig = null;
    private Quaternion sLocRot = Quaternion.identity;

    private float aoa = 0f;

    private void Awake()
    {
        if(wing != null)
        {
            rig = GetComponentInParent<Rigidbody>();
        }

        IsRudder = false;
        IsInverse = false;

    }
    private void Start()
    {
        sLocRot = transform.localRotation;
    }

    private void FixedUpdate()
    {

        // Different angles depending on positive or negative deflection.
        float targetAngle = targetDeflec > 0f ? targetDeflec * maxDeflectionAtPos : targetDeflec * minDeflectionAtNeg;
        //move models controls surfaces
        AnimateControlSurfaces(targetAngle);
    
        // How much you can deflect, depends on how much force it would take
        if (rig != null && wing != null && rig.velocity.sqrMagnitude > 1f)
        {
            float torqueAtMaxDeflection = rig.velocity.sqrMagnitude * wing.GetWingArea;
            float maxAvailableDeflection = Mathf.Asin(maxForce / torqueAtMaxDeflection) * Mathf.Rad2Deg;

            // Asin(x) checks if x > 1 or x < -1 is not a number.
            if (float.IsNaN(maxAvailableDeflection) == false)
                targetAngle *= Mathf.Clamp01(maxAvailableDeflection);
        }

        //calculates the angle of attack
        aoa = Mathf.MoveTowards(aoa, targetAngle, rotSpeed * Time.fixedDeltaTime);


        transform.localRotation = sLocRot;
        transform.Rotate(Vector3.right, aoa, Space.Self);
    }


    void AnimateControlSurfaces(float targetAngle)
    {
        /* Little Dirty
         * TODO - Make cleaner solution
         */
        if (controlSurfaceBody != null)
        {
            if (!IsRudder)
            {
                controlSurfaceBody.transform.localRotation = Quaternion.Euler(-90 + targetAngle, 0, 0);
            }
            else if(IsRudder && !IsInverse)
            {
                controlSurfaceBody.transform.localRotation = Quaternion.Euler(0, -90 + targetAngle, 0);
            }

            if(IsInverse && !IsRudder)
            {
                controlSurfaceBody.transform.localRotation = Quaternion.Euler(-90 + targetAngle, 0 , 180);
            }
       
        }
    }
}