using UnityEngine;
using System.Collections;

public class AnimationEventRelay : MonoBehaviour {

    public GameObject reciever;

    public void AnimationEvent(string function) { reciever.SendMessage(function, SendMessageOptions.RequireReceiver); }
}
