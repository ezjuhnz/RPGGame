using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour {
    private static FollowTarget _instance;
    public Vector3 offset;
    public Transform player;
    
    public static FollowTarget Instance
    {
        get { return _instance; }
    }
	void Start () {
        _instance = this;
    }
	
	// Update is called once per frame
	void Update () {
        if (player == null)
        {
            GameObject[] goArr = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject go in goArr)
            {
                if(PhotonEngine.Instance.role.ID == go.GetComponent<Player>().roleId)
                {
                    player = go.transform;
                    break;
                }
            }
            return;
        }
        transform.position = player.position + offset;
	}
}
