using UnityEngine;

namespace PigeonCoopToolkit.Effects.Trails
{
    [AddComponentMenu("Pigeon Coop Toolkit/Effects/Smoke Plume Gravity")]
    public class SmokePlumeGravity : SmokePlume
    {
    	 public float gravity = 0.1f;
    	 public float gravityBias = 0.5f;


	    protected override void UpdateTrail(PCTrail trail, float deltaTime)
	    {
	        if (_noDecay)
	            return;

	        foreach (PCTrailPoint point in trail.Points)
	        {
	        	Vector3 gravityForce = Vector3.up * Mathf.Lerp(0.0f, -gravity, point.TimeActive()/TrailData.Lifetime);
	        	point.Position += (ConstantForce + gravityForce) * deltaTime;
	        }
	    }
	}
}
