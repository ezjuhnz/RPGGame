using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using System.Collections.Generic;
using LitJson;
using XueCommon.Model;

public class PlayerController : ControllerBase {
    private static PlayerController _instance;
    private int masterid = -1; //组队时的队长id
    private int playerid = -1; //单人时的玩家id
    public List<GameObject> playerGoList;
    public Transform player_pos1;
    public Transform player_pos2;
    public Transform player_pos3;
    public Transform player_pos4;

    public int roleid_1 = 0;
    public int roleid_2 = 0;
    public int roleid_3 = 0;
    public int roleid_4 = 0;
    private int[] roleidArr ={ 0,0,0,0};

    public GameObject playerGo1;
    public GameObject playerGo2;
    public GameObject playerGo3;
    public GameObject playerGo4;

    public Transform playerHpBar;
    public Transform uiRoot;
    public Vector3 hpBarPos;

    
    public Transform currentPlayer;//当前客户端玩家
    public Dictionary<int, GameObject> transPlayerDict = new Dictionary<int, GameObject>();

    private string girlPrefabName = "Player-Girl";
    private string boyPrefabName = "Player-Boy";
    public static PlayerController Instance
    {
        get { return _instance; }
    }

    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.GetRolesInTeam;
        }
    }
    void Awake()
    {
        _instance = this;
        Vector3 scale = uiRoot.localScale;
        hpBarPos = playerHpBar.position;
    }

    public override void Start () {
        base.Start();
        
        if (!TranscriptTeamController.Instance.isTeam) //单人
        {
            playerid = PhotonEngine.Instance.role.ID;
            GetRolesInTeamRequest(playerid);
        }
        else  //组队
        {
            Debug.Log("多人副本");
            //根据队伍中的角色id一一查询数据库,实例化角色
            masterid = TranscriptTeamController.Instance.globalMasterID;
            ClientTeam team = null;
            TranscriptTeamController.Instance.teamDict.TryGetValue(masterid, out team);
            if(team != null)
            {
                roleid_1 = team.memberids[0];
                roleid_2 = team.memberids[1];
                roleid_3 = team.memberids[2];
                roleid_4 = team.memberids[3];
                GetRolesInTeamRequest(roleid_1, roleid_2, roleid_3, roleid_4);
            }
        }

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        object o = null;
        List<Role> roleList = null;
        response.Parameters.TryGetValue((byte)ParameterCode.RoleList, out o);
        roleList = JsonMapper.ToObject<List<Role>>(o.ToString());
        int index = 0;
        string prefabName = girlPrefabName;
        foreach (Role role in roleList)
        {
            index++;
            if(index == 1)
            {
                if(role.CharacterId == 1)//暂时写死
                {
                    prefabName = boyPrefabName;
                }
                playerGo1 = GameObject.Instantiate(Resources.Load("Player/" + prefabName), player_pos1.position, Quaternion.identity) as GameObject;
                playerGoList.Add(playerGo1);
                roleid_1 = role.ID;
                playerGo1.GetComponent<PlayerTransMove>().roleid = roleid_1;
                playerGo1.GetComponent<Player>().Name = role.Name;
                playerGo1.GetComponent<Player>().Level = role.Level;
                if (roleid_1 == PhotonEngine.Instance.role.ID)
                {
                    currentPlayer = playerGo1.transform;
                }
                transPlayerDict.Remove(roleid_1);
                transPlayerDict.Add(roleid_1, playerGo1);//key already exist
                roleidArr[0] = roleid_1;
            }
            else if(index == 2)
            {
                if (role.CharacterId == 1)
                {
                    prefabName = boyPrefabName;
                }
                roleid_2 = role.ID;
                playerGo2 = GameObject.Instantiate(Resources.Load("Player/" + prefabName), player_pos2.position, Quaternion.identity) as GameObject;
                playerGoList.Add(playerGo2);
                playerGo2.GetComponent<PlayerTransMove>().roleid = roleid_2;
                playerGo2.GetComponent<Player>().Name = role.Name;
                playerGo2.GetComponent<Player>().Level = role.Level;
                if (roleid_2 == PhotonEngine.Instance.role.ID)
                {
                    currentPlayer = playerGo2.transform;
                }
                transPlayerDict.Remove(roleid_2);
                transPlayerDict.Add(roleid_2, playerGo2);
                roleidArr[1] = roleid_2;
            }
            else if (index == 3)
            {
                if (role.CharacterId == 1)
                {
                    prefabName = boyPrefabName;
                }
                roleid_3 = role.ID;
                playerGo3 = GameObject.Instantiate(Resources.Load("Player/" + prefabName), player_pos3.position, Quaternion.identity) as GameObject;
                playerGoList.Add(playerGo3);
                playerGo3.GetComponent<PlayerTransMove>().roleid = roleid_3;
                playerGo3.GetComponent<Player>().Name = role.Name;
                playerGo3.GetComponent<Player>().Level = role.Level;
                if (roleid_3 == PhotonEngine.Instance.role.ID)
                {
                    currentPlayer = playerGo3.transform;
                }
                transPlayerDict.Remove(roleid_3);
                transPlayerDict.Add(roleid_3, playerGo3);
                roleidArr[3] = roleid_3;
            }
            else if (index == 4)
            {
                if (role.CharacterId == 1)
                {
                    prefabName = boyPrefabName;
                }
                roleid_4 = role.ID;
                playerGo4 = GameObject.Instantiate(Resources.Load("Player/" + prefabName), player_pos4.position, Quaternion.identity) as GameObject;
                playerGoList.Add(playerGo4);
                playerGo4.GetComponent<PlayerTransMove>().roleid = roleid_4;
                playerGo4.GetComponent<Player>().Name = role.Name;
                playerGo4.GetComponent<Player>().Level = role.Level;
                if (roleid_4 == PhotonEngine.Instance.role.ID)
                {
                    currentPlayer = playerGo4.transform;
                }
                transPlayerDict.Remove(roleid_4);
                transPlayerDict.Add(roleid_4, playerGo4);
                roleidArr[3] = roleid_4;
            }
        }
        GameObject[] goArr = GameObject.FindGameObjectsWithTag("SkillButton");
        foreach(GameObject go in goArr)
        {
            SkillController skillController = go.GetComponent<SkillController>();
            skillController.OnPlayerInitialized();
        }
    }

    void GetRolesInTeamRequest(int rid1,int rid2 = -1,int rid3=-1,int rid4=-1)
    {
        Debug.Log("roleids=" + rid1 + "," + rid2 + "," + rid3 + "," + rid4);
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        int[] roleidArr = { rid1, rid2, rid3, rid4 };
        string json = JsonMapper.ToJson(roleidArr);
        parameters.Add((byte)ParameterCode.TeamRoleIds, json);
        PhotonEngine.Instance.SendRequest(OperationCode.GetRolesInTeam, parameters);
    }
}
