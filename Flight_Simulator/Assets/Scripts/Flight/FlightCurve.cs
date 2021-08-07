
using UnityEngine;

[CreateAssetMenu(fileName = "New Flight Curve", menuName = "Flight Curve", order = 1)]
public class FlightCurve : ScriptableObject
{
	[SerializeField]
	private AnimationCurve lift = new AnimationCurve(new Keyframe(0.0f, 0.0f),
		new Keyframe(16f, 1.1f),
		new Keyframe(20f, 0.6f),
		new Keyframe(135f, -1.0f),
		new Keyframe(160f, -0.6f),
		new Keyframe(164f, -1.1f),
		new Keyframe(180f, 0.0f));

	[SerializeField]
	private AnimationCurve drag = new AnimationCurve(new Keyframe(0.0f, 0.025f),
		new Keyframe(90f, 1f),
		new Keyframe(180f, 0.025f));

	
	public float GetLiftCurAoA(float aoa)
	{
		//gets lift
		return lift.Evaluate(aoa);
	}

	
	public float GetDragCurAoA(float aoa)
	{
		//gets drag
		return drag.Evaluate(aoa);
	}

	
	public void SetCurve(Keyframe[] newCurve)
	{
		//overrides curve
		lift.keys = newCurve;
	}


	[HideInInspector]
	public float defaultAoALift = 0.0f;
	[HideInInspector]
	public float maxLiftPositive = 1.1f;
	[HideInInspector]
	public float minLiftPositive = 0.6f;
	[HideInInspector]
	public float criticalAngle = 16.0f;
	[HideInInspector]
	public float stallingAngle = 20.0f;
}
