using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//analytic parabolic motion helper class. adapted from TennisBits/LegoBatan etc
//useful for throwing an object from point A to exactly point B with a given
//launch angle. works for uneven surfaces and generates an impact time.
//can pass in scaled timeSteps to make the motion feel more game like.

public class ParabolaHelper
{
	public Vector3 launchPosition { get; private set; }
	public Vector3 targetPosition { get; private set; }
	public Vector3 currentPosition { get; private set; }

	public float launchSpeed { get; private set; }
	public float launchAngle { get; private set; }
	public float gravity { get; private set; }

	public float impactTime { get; private set; }
	public float currentTime; //leave this open to manipulation.

	Vector3 launchDirectionXZ;

	public void Init(Vector3 from, Vector3 to, float launchAngleDeg, float g)
	{ 
		launchPosition = from;
		targetPosition = to;

		launchAngle = launchAngleDeg * Mathf.Deg2Rad;
		gravity = g;

		var distance = Vector3.Magnitude(to - from);
		launchDirectionXZ = Vector3.Normalize(to - from);
		launchDirectionXZ = Vector3.Normalize(new Vector3(launchDirectionXZ.x, 0.0f, launchDirectionXZ.z));

		launchSpeed = distance * Mathf.Tan(launchAngle) + launchPosition.y;
		launchSpeed = Mathf.Sqrt(0.5f*gravity*distance*distance / launchSpeed);
		launchSpeed = 1.0f/Mathf.Cos(launchAngle) * launchSpeed;

		//figuring out the impact time
		{
			var A = -0.5f * gravity; //gravity multiplier (gravity is a +ve number in the inspector)
			var B = launchSpeed * Mathf.Sin(launchAngle); //vertical component of launch speed
			var C = launchPosition.y - targetPosition.y; //vertical displacement

			//eq. at this point is gravityMult*t*t + verticalSpeed*t + verticalDisplacement = 0
			//feed it into the quadratic formula to find the roots
			var sqrt = Mathf.Sqrt(Mathf.Max(0.0f, B*B - 4.0f*A*C));
			var denom = 1.0f / (2.0f * A);

			float root0 = (-B + sqrt) * denom;
			float root1 = (-B - sqrt) * denom;

			//take the root that is furthest away in time.
			impactTime = Mathf.Max(root0, root1);

//			Debug.Log(A + " " + B + " " + C + " " + gravity);
//			Debug.Log(root0 + " " + root1);
		}

		currentPosition = from;
		currentTime = 0.0f;
	}
		
	//returns true when the target has been reached
	public bool Update(float timeStep)
	{
		currentTime += timeStep;

		if (currentTime >= impactTime)
		{
			currentPosition = targetPosition;
			return true;
		}
		else
		{
			float x = launchSpeed * Mathf.Cos(launchAngle)*currentTime;
			float y = launchPosition.y - 0.5f*gravity*currentTime*currentTime + launchSpeed*Mathf.Sin(launchAngle)*currentTime;

			var offset = launchPosition + launchDirectionXZ * x;
			currentPosition = new Vector3(offset.x, y, offset.z);

			if (float.IsNaN(currentPosition.x) ||
				float.IsNaN(currentPosition.y) ||
				float.IsNaN(currentPosition.z))
			{
				//something went wrong. kill the update otherwise transform will
				//blow up when you try to assign the invalid number to it.
				return true;
			}

			return false;
		}
	}
}
