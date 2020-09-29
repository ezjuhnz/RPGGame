using UnityEngine;
using System.Collections;

public class Player1Move : MonoBehaviour {

    public float speed = 5;
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        float x = 0;
        float z = 0;
        float h = Input.GetAxis("Horizontal");//水平
        float v = Input.GetAxis("Vertical");  //垂直
        if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
        {
            Vector3 playerOffset = Vector3.zero;
            x = speed * h * Time.fixedDeltaTime;
            z = speed * v * Time.fixedDeltaTime;
            playerOffset = new Vector3(x, 0, z);
            transform.Translate(-playerOffset, Space.World);
            //控制角色的旋转
            transform.rotation = Quaternion.LookRotation(new Vector3(-h, 0, -v));
        }
    }
}
