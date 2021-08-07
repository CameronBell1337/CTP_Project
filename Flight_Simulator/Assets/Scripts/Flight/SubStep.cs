using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MandAM
{
    //Recreation of another Vector3 for fixing jittering issues
    //Momentum and angular momentum sub step
    public Vector3 m; public Vector3 aM;
    public MandAM(Vector3 _m, Vector3 _aM) 
    { this.m = _m; this.aM = _aM; }
    public static MandAM operator+(MandAM i, MandAM j) 
    { return new MandAM(i.m + j.m, i.aM + j.aM); }
    public static MandAM operator *(float t, MandAM i) 
    { return new MandAM(t * i.m, t * i.aM); }
    public static MandAM operator*(MandAM i, float t) 
    { return t * i; }

}

