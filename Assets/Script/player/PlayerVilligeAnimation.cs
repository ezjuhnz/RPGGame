using UnityEngine;
using System.Collections;

public class PlayerVilligeAnimation : MonoBehaviour {

    private Animator anim;
	void Start () {
        anim = this.GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
	  if(rigidbody.velocity.magnitude > 0.5f)
      {
          anim.SetBool("Move", true);
      }
      else
      {
          anim.SetBool("Move", false);
      }
	}
}
