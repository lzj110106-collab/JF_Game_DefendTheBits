using UnityEngine;
using System.Collections;

public class CanvasMesh : MonoBehaviour
{
        public CanvasRenderer canvasRenderer;
        public Material material;
        public Mesh CustomMesh;

        void Update()
        {
        	RefreshMesh();
            
        }


        public void RefreshMesh()
        {
        	if(CustomMesh)
            {
        		canvasRenderer.SetMaterial(material,null);
           		canvasRenderer.SetMesh(CustomMesh);
           }
        }

	void OnDisable()
	{
		canvasRenderer.Clear ();
	}
}
