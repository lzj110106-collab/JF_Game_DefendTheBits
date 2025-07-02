using System.Collections;
using UnityEngine;


//[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
[AddComponentMenu ("Image Effects/Old Screen")]
public class PS_OldScreen : MonoBehaviour
{
	public bool enable = true;
	public float channelShift = 0.0015f;
	public float fisheyeIntensity = 0.2f;
	public float fisheyePinch = 0.1f;

	public Shader shader;

	private RenderTexture _destinationRenderTexture;
	Material mat;
	Camera cam;

	void Awake()
	{
		cam = GetComponent<Camera>();
		mat = new Material(shader);
		mat.SetFloat("_ChannelShift", channelShift);
		mat.SetFloat("_Intensity", fisheyeIntensity);
		mat.SetFloat("_Pinch", fisheyePinch);
	}

//	void OnRenderImage (RenderTexture src, RenderTexture dest)
//	{		
//		
//		Graphics.Blit(src, dest, mat);
//		//cam.targetTexture = 
//	}


	public void OnPreRender()
	{
		if (!enable)
			return;
		_destinationRenderTexture = RenderTexture.GetTemporary(cam.pixelWidth, cam.pixelHeight, 8);
		cam.targetTexture = _destinationRenderTexture;
	}

	public void OnPostRender()
	{
		if (!enable)
			return;
		mat.SetFloat("_ChannelShift", channelShift);
		mat.SetFloat("_Intensity", fisheyeIntensity);
		mat.SetFloat("_Pinch", fisheyePinch);

		cam.targetTexture = null;
		Graphics.Blit(_destinationRenderTexture, null, mat, 0);
		RenderTexture.ReleaseTemporary(_destinationRenderTexture);
	}

}
