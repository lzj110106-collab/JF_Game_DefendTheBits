using UnityEngine;
using System.Collections;

public class RandomizeTransform : MonoBehaviour {

	public Vector3 positionMin;
	public Vector3 positionMax;
	public Vector3 rotationMin;
	public Vector3 rotationMax;
	public Vector3 scaleMin = Vector3.one;
	public Vector3 scaleMax = Vector3.one;

	void Awake ()
	{
		transform.position = new Vector3(
											Random.Range(positionMin.x, positionMax.x),
											Random.Range(positionMin.y, positionMax.y),
											Random.Range(positionMin.z, positionMax.z)
										);
		transform.eulerAngles = new Vector3(
											Random.Range(rotationMin.x, rotationMax.x),
											Random.Range(rotationMin.y, rotationMax.y),
											Random.Range(rotationMin.z, rotationMax.z)
										);
		transform.localScale = new Vector3(
											Random.Range(scaleMin.x, scaleMax.x),
											Random.Range(scaleMin.y, scaleMax.y),
											Random.Range(scaleMin.z, scaleMax.z)
										);
	}
	
}
