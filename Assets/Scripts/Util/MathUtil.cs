using UnityEngine;
using System.Collections;

public class MathUtil
{
	public static float GetAngleInRadiansToPositionXZ(Vector3 from, Vector3 to)
	{
		//ignoring y completely
		Vector3 direction = Vector3.Normalize (new Vector3(to.x - from.x, 0, to.z - from.z));
		
		//gun facing down the z axis
		float angle = Mathf.Acos(direction.z); 
		
		if (direction.x < 0)
			angle = -angle;
		
		return angle;
	}

	public static float GetAngleInDegreesToPositionXZ(Vector3 from, Vector3 to)
	{
		return GetAngleInRadiansToPositionXZ(from, to) * Mathf.Rad2Deg;
	}
	
	public static float BindAngleToCircle(float angle)
	{
		float twoPi = 2.0f * Mathf.PI;
		while (angle < 0)		angle += twoPi;
		while (angle > twoPi)	angle -= twoPi;
		
		return angle;
	}
		
	public static float UpdateRotationAngle(float current, float desired, float rotationAmount)
	{
		//deal with wrapping around circle coordinates
		float twoPi = 2.0f * Mathf.PI;
		current = BindAngleToCircle(current);
		desired = BindAngleToCircle(desired);
		
		if (Mathf.Abs(current - desired) <= rotationAmount)
			return desired;
		
		if (current < desired)
		{
			//calc the distance between the two angles in both directions
			float diff0 = desired - current;
			float diff1 = (twoPi - desired) + current; 
			
			//if its less than the rotation speed, return the desired angle
			if (diff0 <= rotationAmount || diff1 <= rotationAmount)
				return desired;
			
			//otherwise increment the current angle
			if (diff0 <= diff1)
				return current + rotationAmount;
			
			return current - rotationAmount;
		}
		else
		{
			//as above
			float diff0 = current - desired;
			float diff1 = (twoPi - current) + desired;
			
			if (diff0 <= rotationAmount || diff1 <= rotationAmount)
				return desired;
			
			if (diff0 <= diff1)
				return current - rotationAmount;
			
			return current + rotationAmount;
		}
	}
	
	public static float AngleBetweenAngles(float a0, float a1)
	{
		float twoPi = 2.0f * Mathf.PI;
		a0 = BindAngleToCircle(a0);
		a1 = BindAngleToCircle(a1);
		
		if (a0 < a1)
		{
			float result0 = a1 - a0;
			float result1 = (twoPi - a1) + a0;
			
			return Mathf.Min(result0, result1);
		}
		else
		{
			float result0 = a0 - a1;
			float result1 = (twoPi - a0) + a1;
			
			return Mathf.Min(result0, result1);
		}
	}

	public static bool LineLineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 result)
	{
		float denom = (a.x - b.x)*(c.y - d.y) - (a.y - b.y)*(c.x - d.x);

		if (Mathf.Abs (denom) > 0.01f)
		{
			result.x = ((a.x*b.y - a.y*b.x)*(c.x - d.x) - (a.x - b.x)*(c.x*d.y - c.y*d.x))/denom;
			result.y = ((a.x*b.y - a.y*b.x)*(c.y - d.y) - (a.y - b.y)*(c.x*d.y - c.y*d.x))/denom;
			return true;
		}

		result = Vector2.zero;
		return false;
	}

	//this returns two points at which lines [a, b] and [c, d], have their closest approach. 
	//parameters a,b,c,d are actual points on lines, rather than a point and a direction.
	public static bool LineLineIntersection(Vector3 a, Vector3 b, Vector3 c, Vector3 d, 
		out Vector3 resultAB, 
		out Vector3 resultCD)
	{
		var dir0 = Vector3.Normalize(b - a);
		var dir1 = Vector3.Normalize(d - c);

		Vector3 w0 = a - c;
		float temp0 = Vector3.Dot(dir0, dir1);
		float temp1 = Vector3.Dot(dir0, w0);
		float temp2 = Vector3.Dot(dir1, w0);

		float denom = 1.0f - temp0*temp0;
		if (Mathf.Abs(denom) < 0.01f)
		{
			resultAB = a;
			resultCD = c + dir1*temp2;

			return false; //parallel
		}

		resultAB = a + dir0*((temp0*temp2 - temp1)/denom);
		resultCD = c + dir1*((temp2 - temp0*temp1)/denom);

		return true;
	}

	//returns closest point on a line defined by two points, rather than a point and a direction.
	public static bool ClosestPointOnLine(Vector3 worldPosition, Vector3 p0, Vector3 p1,  
		out Vector3 position, 
		out float param)
	{
		position = p0;
		param = 0.0f;

		float denom = Vector3.SqrMagnitude(p1 - p0);
		if (denom < 0.001f)
			return false; //line points are coincident.

		param = Vector3.Dot(p1 - p0, worldPosition - p0)/denom;
		position = Lerp(p0, p1, param);

		return true;
	}

	public static bool ClosestPointOnSegment(Vector3 worldPosition, Vector3 p0, Vector3 p1,  
		out Vector3 position, 
		out float param)
	{
		if (ClosestPointOnLine(worldPosition, p0, p1, out position, out param))
			return param >= 0.0f && param <= 1.0f;

		return false;
	}

	//Vector3.Lerp will clamp parameter t to the range [0, 1]
	public static Vector3 Lerp(Vector3 p0, Vector3 p1, float t)
	{
		return p0 + (p1 - p0)*t;
	}

#region MATRIX HELPERS

	public static Vector3 ExtractTranslation(ref Matrix4x4 matrix) 
	{
		Vector3 translate;
		translate.x = matrix.m03;
		translate.y = matrix.m13;
		translate.z = matrix.m23;
		return translate;
	}

	public static Quaternion ExtractRotation(ref Matrix4x4 matrix) 
	{
		Vector3 forward;
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;

		Vector3 upwards;
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;

		return Quaternion.LookRotation(forward, upwards);
	}

	public static Vector3 ExtractScale(ref Matrix4x4 matrix) 
	{
		Vector3 scale;
		scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
		scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
		scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
		return scale;
	}

#endregion

#region PARABOLA

	//pulled from Tennis Bits
	public static void InitialiseParabola(Vector3 launchPosition, 
										  Vector3 targetPosition,
										  ref Vector3 launchDirectionXZ,
										  ref float launchSpeed,
										  float launchAngleDegrees,
										  float gravity)
	{
		var distance = Vector3.Magnitude(targetPosition - launchPosition);
		launchDirectionXZ = Vector3.Normalize(targetPosition - launchPosition);
		launchDirectionXZ = Vector3.Normalize(new Vector3(launchDirectionXZ.x, 0.0f, launchDirectionXZ.z));

		var launchAngleRad = launchAngleDegrees * Mathf.Deg2Rad;
		launchSpeed = distance * Mathf.Tan(launchAngleRad) + launchPosition.y;
		launchSpeed = Mathf.Sqrt(0.5f*gravity*distance*distance / launchSpeed);
		launchSpeed = 1.0f/Mathf.Cos(launchAngleRad) * launchSpeed;
	}

	public static Vector3 SolveParabola(Vector3 launchPosition, 
										Vector3 launchDirectionXZ, 
										float launchSpeed, 
										float launchAngleDegrees, 
										float g, 
										float t)
	{
		float x = launchSpeed * Mathf.Cos(launchAngleDegrees)*t;
		float y = launchPosition.y - 0.5f*g*t*t + launchSpeed*Mathf.Sin(launchAngleDegrees)*t;

		var result = launchPosition + launchDirectionXZ * x;
		result.y = y;

		return result;
	}

#endregion
}
