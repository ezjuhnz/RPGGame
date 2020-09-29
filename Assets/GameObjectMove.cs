using UnityEngine;
using System.Collections;

public class GameObjectMove : MonoBehaviour {

    private float speed = 10;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
        {
            Vector3 offsetVec = new Vector3(-h * speed, 0, -v * speed) * Time.deltaTime;
            transform.position = transform.position + offsetVec;
            //Debug.Log("new position=" + transform.position);
        }

    }
}
