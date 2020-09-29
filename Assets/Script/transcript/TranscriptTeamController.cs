using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//该脚本的作用是存储在城镇的组队信息,在场景切换时为城镇的队伍信息提供数据
public class TranscriptTeamController : MonoBehaviour {
    private static TranscriptTeamController _instance;
    public bool isTeam = false; //是否组队
    public int globalMasterID = -1;
    public Dictionary<int, ClientTeam> teamDict = new Dictionary<int, ClientTeam>();

    public static TranscriptTeamController Instance
    {
        get { return _instance; }
    }
        

    void Awake()
    {
        if (TeamInviteController.Instance.isTeam) //组队
        {
            isTeam = true;
        }
    }

	void Start () {
        DontDestroyOnLoad(this);
        globalMasterID = TeamInviteController.Instance.globalMasterID;
        teamDict = TeamInviteController.Instance.teamDict;
        _instance = this;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
