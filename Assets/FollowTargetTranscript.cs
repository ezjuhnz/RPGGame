using UnityEngine;
using System.Collections;

public class FollowTargetTranscript : MonoBehaviour {

    private static FollowTargetTranscript _instance;
    public Vector3 offset;
    public Transform player;

    public static FollowTargetTranscript Instance
    {
        get { return _instance; }
    }
    void Start()
    {
        _instance = this;
        if(player == null)
        {
            player = PlayerController.Instance.currentPlayer;
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = PlayerController.Instance.currentPlayer;
        }
        else if(player != null)
        {
            transform.position = player.position + offset;
        }
        
    }
}
