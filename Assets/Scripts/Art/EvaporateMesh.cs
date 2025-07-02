using UnityEngine;
using System.Collections;

public class EvaporateMesh : MonoBehaviour {

	public Transform voxelContainer;
	public Transform target;
	public Transform targetToFollowTransform;
	public float minRadius;
	public float maxRadius;
	public float maxOffset;
	private Transform[] voxels;
	private Vector3[] originalPositions;
	private float[] jitterOffsets;
	private Vector3 targetPos;
	private Vector3 newPosition;
	private float dist;

	private bool initialized;


	void Initialize()
	{
		if(initialized)
			return;
		initialized = true;

		voxels = new Transform[voxelContainer.childCount];
		originalPositions = new Vector3[voxelContainer.childCount];
		jitterOffsets = new float[voxelContainer.childCount];

		for (int i=0; i<voxelContainer.childCount; i++)
		{
			voxels[i] = voxelContainer.GetChild(i);
			originalPositions[i] = voxels[i].localPosition;
			jitterOffsets[i] = Random.Range(0, 0.25f);
		}
	}


	void Update ()
	{
		Initialize();

		target.position = targetToFollowTransform.position;
		targetPos = target.position;

		for (int i=0; i<voxels.Length; i++)
		{
			dist = DistanceToTarget(originalPositions[i]);

			if(dist <= maxRadius && dist >= minRadius)
			{
				Vector3 upOffset = Vector3.up * Mathf.Clamp((dist + jitterOffsets[i]), minRadius, maxRadius);
				voxels[i].localPosition = Vector3.Lerp(originalPositions[i], originalPositions[i] + upOffset, dist/maxOffset);
			}
			else
			{
				voxels[i].localPosition = originalPositions[i];
			}
		}
	}

	float DistanceToTarget(Vector3 pos)
	{
		return (targetPos - pos).magnitude;
	}


/*	void OnGUI() { GUI.Label(new Rect(10, 10, 700, 20), "Orig: " + originalPositions[5] + " - New: " + voxels[5].position + " - Dist " + dist); }
	void OnDrawGizmos()
	{
	 	Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(targetPos, minRadius);
	 	Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos, maxRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetPos, maxOffset);
	}*/
}
