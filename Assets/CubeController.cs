using UnityEngine;
using System.Collections;

public class CubeController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnClick(Collider col)
    {
        Debug.Log("click..");
    }

    void OnMouseDown()
    {
        Debug.Log("clickgggggg.");
    }
}
