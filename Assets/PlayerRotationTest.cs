using UnityEngine;
using System.Collections;

public class PlayerRotationTest : MonoBehaviour {
    public Transform target;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //transform.LookAt(target);
        transform.LookAt(target, Vector3.left);//Shorthand for writing Vector3(-1, 0, 0).
    }
}
