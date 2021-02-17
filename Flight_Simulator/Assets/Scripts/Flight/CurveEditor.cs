using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(FlightCurve))]
public class CurveEditor : Editor
{
	private float defaultAoALift = 0.0f;
	private float maxLiftPositive = 1.1f;
	private float minLiftPositive = 0.6f;
	private float criticalAngle = 16.0f;
	private float stallingAngle = 20.0f;

	const float fPMax = 0.85f;
	const float stallPadding = 0.1f;
	const float liftPadding = 0.01f;

	//multipliers
	private float nAoAM = 1.0f;
	private float fPM = 1.0f;

	const string defaultAoALiftDesc = "Lift produced at 0 degrees.";
	const string MaxLiftPosDesc = "Lift coeffient at a positive, most efficient angle of attack";
	const string MinLiftPosDesc = "Lift coeffient when the wing is fully stalled at a positive angle of attack.";
	const string CriticalDesc = "Critical angle of attack is both the angle at which the wing starts to stall, and the angle at which it produces the most lift.";
	const string stallingAngleDesc = "Angle of attack at which the wing is fully stalled and producing the minimum lift.";


	public override void OnInspectorGUI()
    {
		base.OnInspectorGUI();

		FlightCurve curve = (FlightCurve)target;

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Lift curve parameters", EditorStyles.boldLabel);

		defaultAoALift = EditorGUILayout.FloatField(new GUIContent("Lift at Zero AOA: ", defaultAoALiftDesc), defaultAoALift);
		maxLiftPositive = EditorGUILayout.FloatField(new GUIContent("Lift at Critical: ", MaxLiftPosDesc), maxLiftPositive);
		minLiftPositive = EditorGUILayout.FloatField(new GUIContent("Lift when in stall: ", MinLiftPosDesc), minLiftPositive);

		EditorGUILayout.Space();
		criticalAngle = EditorGUILayout.FloatField(new GUIContent("Critical AoA: ", CriticalDesc), criticalAngle);
		stallingAngle = EditorGUILayout.FloatField(new GUIContent("Fully stalled AoA: ", stallingAngleDesc), stallingAngle);

		bool createCurve = GUILayout.Button("Create new Curve.");


		if (stallingAngle < criticalAngle)
        {
			stallingAngle = criticalAngle + stallPadding;
        }

		if(defaultAoALift > maxLiftPositive)
        {
			defaultAoALift = maxLiftPositive - liftPadding;        
		}

		List<Keyframe> points = new List<Keyframe>(9)
		{
			// Wing at positive AOA.
			new Keyframe(0.0f, defaultAoALift),
			new Keyframe(criticalAngle, maxLiftPositive),
			new Keyframe(stallingAngle, minLiftPositive),

			// Flat plate, generic across all wings.
			new Keyframe(45.0f, fPMax * fPM),
			new Keyframe(90.0f, 0.0f),
			new Keyframe(135.0f, -fPMax * fPM),

			// Wing at negative AOA.
			new Keyframe(180.0f - stallingAngle, -minLiftPositive * nAoAM),
			new Keyframe(180.0f - criticalAngle, -maxLiftPositive * nAoAM),
			new Keyframe(180.0f, defaultAoALift)
		};

		if(createCurve)
        {
			curve.SetCurve(points.ToArray());
			Repaint();
        }
	}

	

}
