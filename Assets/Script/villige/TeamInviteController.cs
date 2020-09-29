using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XueCommon;
using ExitGames.Client.Photon;
using System;
using LitJson;
using XueCommon.Model;

public class TeamInviteController : ControllerBase {
    private static TeamInviteController _instance;
    public bool isTeam = false;
    public Dictionary<int, ClientTeam> teamDict = new Dictionary<int, ClientTeam>();
    private GameObject inviteRequestGo;
    private GameObject joinRequestGo;
    private UILabel inviteLabel;
    private UILabel joinLabel;
    public UIButton inviteButton;
    //组队图标
    public GameObject teamPanelGo1;
    public GameObject teamPanelGo2;
    public GameObject teamPanelGo3;
    public GameObject teamPanelGo4;
    //

    public int globalMasterID = -1;   //队长id
    public int inviteid = -1;   //邀请者id
    private string inviteName = "";
    public int[] teamMemberids = { -1, -1, -1, -1 };

 

    public static TeamInviteController Instance
    {
        get { return _instance; }
    }

    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.SendTeam;
        }
    }

    public string InviteName
    {
        get
        {
            return inviteName;
        }

        set
        {
            inviteName = value;
        }
    }

    void Awake()
    {
        
    }
	// Use this for initialization
	public override void Start ()
    {
        inviteRequestGo = transform.root.Find("InviteRequest").gameObject;
        joinRequestGo = transform.root.Find("JoinRequest").gameObject;
        
        if (inviteRequestGo != null)
        {
            inviteLabel = inviteRequestGo.transform.Find("invite-label").GetComponent<UILabel>();
        }
        if (inviteRequestGo != null)
        {
            joinLabel = joinRequestGo.transform.Find("join-label").GetComponent<UILabel>();
        }
        //如果是从副本返回城镇时,要为该脚本赋值
        if (TranscriptTeamController.Instance != null)
        {
            this.globalMasterID = TranscriptTeamController.Instance.globalMasterID;
            this.isTeam = TranscriptTeamController.Instance.isTeam;
            this.teamDict = TranscriptTeamController.Instance.teamDict;
            if(isTeam)
            {
                ClientTeam team;
                teamDict.TryGetValue(globalMasterID, out team);

                //返回城镇后,如果还是组队状态,则显示队伍标志
                if (null != team)
                {
                    for (int i = 0; i < team.Size; i++)
                    {
                        if (team.memberids[i] > 0)
                        {
                            ShowTeamPanelGo(i, globalMasterID, team.memberids[i],team.roleArr[i].Name);
                        }
                    }
                }
            }   
        }
        //从城镇场景到副本场景的加载不销毁该gameObject
        DontDestroyOnLoad(this.gameObject);
        base.Start();
        _instance = this;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}


    //点击邀请组队,调用该方法
    public void OnInvite(int joinerid,string joinerName)
    {
        int inviteid = PhotonEngine.Instance.role.ID;
        string inviteName = PhotonEngine.Instance.role.Name;

        Debug.Log("joinerName=" + joinerName + "joinerid=" + joinerid);
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        
        //Channel id
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        //isTeam
        GameObject go;
        bool isTeam = false; //当前客户端角色是否处于组队状态
        GameController.Instance.playerDict.TryGetValue(inviteid, out go);
        if(null != go)
        {
            isTeam = go.GetComponent<Player>().isTeam;
        }
        //Debug.Log("current player是否组队" + isTeam);
        parameters.Add((byte)ParameterCode.IsTeam, isTeam);
        parameters.Add((byte)ParameterCode.InviteId, inviteid);
        parameters.Add((byte)ParameterCode.InviteName, inviteName);
        parameters.Add((byte)ParameterCode.RoleId, joinerid);
        parameters.Add((byte)ParameterCode.JoinerName, joinerName);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SendTeam);
        //发送请求到服务器
        PhotonEngine.Instance.SendRequest(OperationCode.SendTeam, parameters);
        HideTeamBg();
    }
    //暂时用不上
    public void OnJoinTeam(int inviteid,int playerid)
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.InviteId, inviteid);
        parameters.Add((byte)ParameterCode.RoleId, playerid);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.JoinTeam);
        //还需要Channel id
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        //发送请求到服务器
        PhotonEngine.Instance.SendRequest(OperationCode.SendTeam, parameters);
        HideTeamBg();
    }

    //点击退出队伍调用该方法
    public void OnExitTeam(int masterid)
    {
        Debug.Log("OnExitTeam masterid=" + masterid);
        int roleid = PhotonEngine.Instance.role.ID;
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.MasterRoleID, masterid);
        parameters.Add((byte)ParameterCode.RoleId, roleid);
        
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.ExitTeam);
        
        PhotonEngine.Instance.SendRequest(OperationCode.SendTeam, parameters);
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        //TODO
    }
    public override void OnEvent(EventData eventData)
    {
        Debug.Log("OnEvent()==");
        //根据subcode,判断组队状态,从而进行不同操作
        object o = null;
        SubCode subCode;
        Team team = null;
        int roleid = 0;
        string inviteName = "";

        eventData.Parameters.TryGetValue((byte)ParameterCode.SubCode, out o);
        subCode = (SubCode)o;
        eventData.Parameters.TryGetValue((byte)ParameterCode.InviteId, out o);
        if(null != o)
        {
            inviteid = (int)o;
        }
        eventData.Parameters.TryGetValue((byte)ParameterCode.Team, out o);
        if(null != o)
        {
            team = JsonMapper.ToObject<Team>(o.ToString());
        }
       
        eventData.Parameters.TryGetValue((byte)ParameterCode.InviteName, out o);
        if (o != null)
        {
            inviteName = o.ToString();
        }

        switch (subCode)
        {
            case SubCode.SendTeam:
                //组队邀请,被邀请的玩家的客户端会收到该请求
                SendTeamResponse(inviteid,inviteName,team);
                break;
            case SubCode.JoinTeam:
                JoinTeamResponse(inviteid,inviteName,team);
                break;
            case SubCode.TeamFailed:
                TeamFailedResponse(inviteid);
                break;
            case SubCode.AlreadyInTeam:
                AlreadyInTeamResponse(inviteid);
                break;
            case SubCode.AgreeInvite:
                //同意邀请
                eventData.Parameters.TryGetValue((byte)ParameterCode.Team, out o);
               
                team = JsonMapper.ToObject<Team>(o.ToString());
                eventData.Parameters.TryGetValue((byte)ParameterCode.RoleId, out o);
                if(o != null)
                {
                    roleid = (int)o;
                }
                AgreeInviteResponse(roleid,inviteName,team) ;
                break;
            case SubCode.RejectInvite:
                //拒绝邀请
                //TODO
                break;
            case SubCode.AgreeJoin:
                //同意申请者加入
                eventData.Parameters.TryGetValue((byte)ParameterCode.Team, out o);
                team = JsonMapper.ToObject<Team>(o.ToString());
                eventData.Parameters.TryGetValue((byte)ParameterCode.RoleId, out o);
                if (o != null)
                {
                    roleid = (int)o;
                }
                AgreeJoinResponse(roleid, inviteName,team);
                break;
            case SubCode.RejectJoin:
                eventData.Parameters.TryGetValue((byte)ParameterCode.Team, out o);
                team = JsonMapper.ToObject<Team>(o.ToString());
                eventData.Parameters.TryGetValue((byte)ParameterCode.RoleId, out o);
                if (o != null)
                {
                    roleid = (int)o;
                }
                RejectJoinResponse(roleid, team);
                break;
            case SubCode.ExitTeam:
                //退出队伍
                int exitRoleid = 0;
                int masterid = 0;
                Team teamTmp = new Team();
                eventData.Parameters.TryGetValue((byte)ParameterCode.RoleId, out o);
                if(o != null)
                {
                    exitRoleid = (int)o;
                }
                eventData.Parameters.TryGetValue((byte)ParameterCode.MasterRoleID, out o);
                if (o != null)
                {
                    masterid = (int)o;
                }
                eventData.Parameters.TryGetValue((byte)ParameterCode.Team, out o);
                teamTmp = JsonMapper.ToObject<Team>(o.ToString());
                ExitTeamResponse(masterid, exitRoleid, teamTmp);
                break;
            case SubCode.UpdateTeam:
                //UpdateTeamResponse(masterid, inviteid);
                break;
            default: break;

        }
    }

   

    //隐藏"邀请组队|查看装备|发送消息" 面板
    public void HideTeamBg()
    {
        GameController.Instance.HideTeamBg();
    }
   
   
    //是否同意邀请
    void SendTeamResponse(int inviteId,string inviteName,Team team)
    {
        Debug.Log("SendTeamResponse inviteName= " + inviteName);
        this.inviteid = inviteId;
        this.InviteName = inviteName;
        if(team != null)
        {
            this.globalMasterID = team.Masterid;
        }
        
        ShowInviteRequestGo();
        inviteLabel.text = "玩家 " + inviteName + "邀请您加入队伍";
    }

    //是否同意对方加入队伍
    void JoinTeamResponse(int inviteId,string inviteName,Team team)
    {
        this.inviteid = inviteId;
        if (team != null)
        {
            this.globalMasterID = team.Masterid;
        }
        ShowJoinRequestGo();
        joinLabel.text = "玩家 " + inviteName + "申请加入队伍";
    }

    void ShowInviteRequestGo()
    {
        inviteRequestGo.SetActive(true);
    }
    
    void ShowJoinRequestGo()
    {
        joinRequestGo.SetActive(true);
    }
    //邀请组队,当双方都有队伍时,组队失败
    void TeamFailedResponse(int inviteId)
    {
        //TODO:提示组队失败,在公告系统完成后才能完成此功能
        Debug.Log("对方已有队伍==");
    }
    void AlreadyInTeamResponse(int inviteid)
    {
        Debug.Log("对方在队伍中==");
    }

    void AgreeInviteResponse(int roleid,string inviteName,Team team)
    {
        int masterid = team.Masterid;
        globalMasterID = masterid;
        Debug.Log("AgreeInviteResponse masterid= " + masterid);
        Debug.Log("memberids=" + team.memberids[0] + "," + team.memberids[1] + "," + team.memberids[2] + "," + team.memberids[3]);
        PhotonEngine.Instance.masterid = masterid;
        //修改当前玩家 isTeam 的值
        int playerid = PhotonEngine.Instance.role.ID;
        GameObject playerGo = null;
        GameController.Instance.playerDict.TryGetValue(inviteid, out playerGo);
        if(null != playerGo)
        {
            isTeam = true;
            playerGo.GetComponent<Player>().isTeam = true;
        }
        ClientTeam ct = null;
        teamDict.TryGetValue(masterid, out ct);//第一次组队client team为null
        if(null == ct)
        {
            ct = new ClientTeam();
            ct.masterId = team.Masterid;
        }
        //先移除旧的再添加新的
        teamDict.Remove(masterid);
        
        //1.将服务器的Team信息 更新到客户端Team中
        for (int i = 0; i < team.TeamSize;i++)
        {
            if(ct.memberids[i] < 0 && team.memberids[i] > 0)
            {
                ct.memberids[i] = team.memberids[i];
                //组队优化,将代替上面的memberids
                Role tmp = new Role();
                ct.roleArr[i] = tmp;
                ct.roleArr[i].ID = team.RoleArr[i].ID;
                ct.roleArr[i].Name = team.RoleArr[i].Name;
                
                if(null != playerGo)
                {
                    ct.teamMemberGoDict.Remove(ct.roleArr[i].ID);//先移除,避免key已存在
                    ct.teamMemberGoDict.Add(ct.roleArr[i].ID, playerGo);
                    
                    playerGo.GetComponent<Player>().Name = ct.roleArr[i].Name;
                }
                else
                {
                    Debug.Log("error player- " + ct.roleArr[i].ID + "not found!");
                }
            }
        }
        
        
        teamDict.Add(masterid, ct);
        teamMemberids = ct.memberids;
        //2.展示队伍所有队员的组队panel
        for (int i = 0; i < team.TeamSize; i++)
        {
            if(team.memberids[i] > 0)
            {
                //将服务器的 Team 数据保存到客户端 ClientTeam
                Debug.Log("team.RoleArr[i].Name =" + i +":" + team.RoleArr[i].Name);
                ShowTeamPanelGo(i,team.Masterid,team.memberids[i],team.RoleArr[i].Name);
            }
        }
    }

    //
    void AgreeJoinResponse(int roleid,string inviteName,Team team)
    {
        int masterid = team.Masterid;
        globalMasterID = masterid;
        PhotonEngine.Instance.masterid = masterid;
        //修改邀请者和被邀请者的isTeam的值
        int inviteid = PhotonEngine.Instance.role.ID;
        GameObject playerGo = null;
        GameController.Instance.playerDict.TryGetValue(inviteid, out playerGo);
        if (null != playerGo)
        {
            isTeam = true;
            playerGo.GetComponent<Player>().isTeam = true;
        }
        GameController.Instance.playerDict.TryGetValue(roleid, out playerGo);
        if (null != playerGo)
        {
            playerGo.GetComponent<Player>().isTeam = true;
        }

        ClientTeam ct = null;
        teamDict.TryGetValue(masterid, out ct);//第一次组队team为null
        if (null == ct)
        {
            ct = new ClientTeam();
            ct.masterId = masterid; //第一次组成队伍要给masterid赋值
            ct.memberids[0] = masterid; //第一次组成队伍,队长默认是1P
        }
        //先移除旧的再添加新的
        teamDict.Remove(masterid);
        Debug.Log("Masterid=" + masterid);
        Debug.Log("之前: ct.memberids[i]=" + ct.memberids[0] + "," + ct.memberids[1] + "," + "," + ct.memberids[2] + "," + "," + ct.memberids[3]);
        for (int i = 0; i < ct.Size; i++)
        {
            if (ct.memberids[i] < 0)
            {
                ct.memberids[i] = team.memberids[i];
                //break;
            }
        }
        //1.把队长信息更新到ClientTeam
        if (!ct.teamMemberGoDict.TryGetValue(masterid, out playerGo))
        {
            GameObject[] goArr = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject go in goArr)
            {
                if (go.GetComponent<Player>().roleId == masterid)
                {
                    ct.teamMemberGoDict.Add(masterid, go);
                    break;
                }
            }
        }

        //2.把新加入的队员更新到ClientTeam中

        if (!ct.teamMemberGoDict.TryGetValue(roleid, out playerGo))
        {
            GameObject[] goArr = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject go in goArr)
            {
                if (go.GetComponent<Player>().roleId == roleid)
                {
                    ct.teamMemberGoDict.Add(roleid, go);
                    break;
                }
            }
        }

        teamDict.Add(masterid, ct);
        teamMemberids = ct.memberids;
        //3.展示队伍所有队员的组队panel
        for (int i = 0; i < team.TeamSize; i++)
        {
            if (team.memberids[i] > 0)
            {
                ShowTeamPanelGo(i, team.Masterid, team.memberids[i], team.RoleArr[i].Name);
            }
        }
    }

    //拒绝申请者加入队伍
    void RejectJoinResponse(int roleid,Team team)
    {
        //TODO
    }

    /*需要注意:masterid与serverTeam中的masterId可能并不相同,
    *因为服务器发送的是更新过后的队伍信息(如果是队长退出,则重新选择一名队员作为队长)
    *而客户端保存的还是原来的队长id,因此不能根据服务器最新的队伍信息来查找客户端的队伍信息
    */
    void ExitTeamResponse(int masterid, int exitRoleid,Team serverTeam)
    {
        GameObject go;
        Debug.Log("ExitTeamResponse masterid=" + masterid + ",exitRoleId=" + exitRoleid);
        if(PhotonEngine.Instance.role.ID == exitRoleid)
        {
            isTeam = false;
            GameController.Instance.playerDict.TryGetValue(exitRoleid, out go);
            go.GetComponent<Player>().isTeam = false;
        }
        ClientTeam clientTeam = new ClientTeam();
        teamDict.TryGetValue(masterid, out clientTeam);
        
        if(null != clientTeam)
        {
            //更新客户端队伍信息
            clientTeam.teamMemberGoDict.Remove(exitRoleid);
            for(int i = 0; i< clientTeam.Size; i++)
            {
                if(exitRoleid == clientTeam.memberids[i])
                {
                    clientTeam.memberids[i] = -1;
                    break;
                }
            }
            teamMemberids = clientTeam.memberids;
            //如果退出的人是队长,将服务器最新的队伍信息设置到客户端
            if (masterid == exitRoleid)
            {
                clientTeam.masterId = serverTeam.Masterid;
                TeamInviteController.Instance.globalMasterID = serverTeam.Masterid;
            }
            //移除旧的ClientTeam,添加新的ClientTeam
            teamDict.Remove(clientTeam.masterId); 
            teamDict.Add(clientTeam.masterId, clientTeam);
            //如果是退出队伍的是自己,则在客户端隐藏队伍信息
            if (PhotonEngine.Instance.role.ID == exitRoleid)
            {
                Debug.Log("自己退出队伍==");
                HideAllTeamPanel();
            }
            else
            {
                UpdateTeamPanel(clientTeam);
            }
        }
        else
        {
            Debug.Log("client Team is null");
        }
    }

    void UpdateTeamResponse(int masterid,int roleid,int memberIndex)
    {
        //TODO
        
    }
    public void ShowTeamPanelGo(int memberIndex,int masterid,int memberid,string memberName)
    {
        Debug.Log("memberIndex and memberName=" + memberIndex + "," + memberName);
        if(memberIndex == 0)
        {
            teamPanelGo1.SetActive(true);
            teamPanelGo1.GetComponent<TeamMemberController>().masterid = masterid;
            teamPanelGo1.GetComponent<TeamMemberController>().roleid = memberid;
            teamPanelGo1.GetComponent<TeamMemberController>().roleName = memberName;
            teamPanelGo1.transform.Find("name-label").GetComponent<UILabel>().text = memberName;
        }
        else if(memberIndex == 1)
        {
            teamPanelGo2.SetActive(true);
            teamPanelGo2.GetComponent<TeamMemberController>().masterid = masterid;
            teamPanelGo2.GetComponent<TeamMemberController>().roleid = memberid;
            teamPanelGo2.GetComponent<TeamMemberController>().roleName = memberName;
            teamPanelGo2.transform.Find("name-label").GetComponent<UILabel>().text = memberName;
        }
        else if (memberIndex == 2)
        {
            teamPanelGo3.SetActive(true);
            teamPanelGo3.GetComponent<TeamMemberController>().masterid = masterid;
            teamPanelGo3.GetComponent<TeamMemberController>().roleid = memberid;
            teamPanelGo3.GetComponent<TeamMemberController>().roleName = memberName;
            teamPanelGo3.transform.Find("name-label").GetComponent<UILabel>().text = memberName;
        }
        else if (memberIndex == 3)
        {
            teamPanelGo4.SetActive(true);
            teamPanelGo4.GetComponent<TeamMemberController>().masterid = masterid;
            teamPanelGo4.GetComponent<TeamMemberController>().roleid = memberid;
            teamPanelGo4.GetComponent<TeamMemberController>().roleName = memberName;
            teamPanelGo4.transform.Find("name-label").GetComponent<UILabel>().text = memberName;
        }

    }
   
    void HideAllTeamPanel()
    {
        teamPanelGo1.SetActive(false);
        teamPanelGo2.SetActive(false);
        teamPanelGo3.SetActive(false);
        teamPanelGo4.SetActive(false);
    }

    //更新队伍显示
    void UpdateTeamPanel(ClientTeam team)
    {
        Debug.Log("team: " + team.memberids[0] + "," + team.memberids[1] + "," + team.memberids[2]);
        for(int i = 0; i < team.Size; i++)
        {
            if(team.memberids[i] < 0)
            {
                HideTeamPanel(i);
            }
        }
    }

    void HideTeamPanel(int memberIndex)
    {
        switch(memberIndex)
        {
            case 0:
                teamPanelGo1.SetActive(false);
                break;
            case 1:
                teamPanelGo2.SetActive(false);
                break;
            case 2:
                teamPanelGo3.SetActive(false);
                break;
            case 3:
                teamPanelGo4.SetActive(false);
                break;
        }
    }
    //判断是否处于组队状态
   
    //获取队长role id

    //****************协程********************
}
