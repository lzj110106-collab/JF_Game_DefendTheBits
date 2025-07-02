using UnityEngine;
using System.Collections;

public class TrailConstantForce : MonoBehaviour {

    public float speed;
    private PigeonCoopToolkit.Effects.Trails.SmokePlume[] plumes;

    void Awake()
    {
    	plumes = GetComponents<PigeonCoopToolkit.Effects.Trails.SmokePlume>();
    }


    void Update ()
    {
        for(int i=0; i<plumes.Length; i++)
        {
            plumes[i].ConstantForce = transform.forward * speed;
        }
    }
}