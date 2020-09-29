using UnityEngine;
using System.Collections;

public class FollowCude : MonoBehaviour {

    public Vector3 offset;
    private GameObject player2Go;
	void Start () {
        player2Go = GameObject.Find("Player2");
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = player2Go.transform.position + offset;
	}
}
