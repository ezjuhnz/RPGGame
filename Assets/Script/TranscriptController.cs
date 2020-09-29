using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using LitJson;
using System.Collections.Generic;

public class TranscriptController : ControllerBase {
    private static TranscriptController _instance;

    public static TranscriptController Instance
    {
        get { return _instance; }
    }
    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.Transcript;
        }
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        
    }

    public override void OnEvent(EventData eventData)
    {
        AsyncOperation operation = Application.LoadLevelAsync(2);
        LoadSceneProgressBar._instance.Show(operation);
    }

    public override void Start () {
        base.Start();
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    //通知队伍中的角色的客户端进入副本
    public void TeamMateEnterTranscript()
    {
        Debug.Log("TeamMateEnterTranscript==");
        //发送请求到服务器,队友的客户端也要进入副本
        //1.根据队伍中角色,获取角色id.发送给server
        //2.server根据role id找到peer,找到发送的客户端,从而进行场景加载
        //3.需要发给队长所属的客户端吗? 暂时不需要
        int masterid = PhotonEngine.Instance.role.ID;
        ClientTeam team = new ClientTeam();
        TeamInviteController.Instance.teamDict.TryGetValue(masterid, out team);
        
        if(team != null)
        {
            int[] roleids = { team.memberids[0], team.memberids[1],
                 team.memberids[2],  team.memberids[3] };
            string json = JsonMapper.ToJson(roleids);
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            parameters.Add((byte)ParameterCode.MasterRoleID, masterid);
            parameters.Add((byte)ParameterCode.TeamRoleIds, json);
            //频道id
            parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
            parameters.Add((byte)ParameterCode.SubCode, SubCode.EnterTranscript);
            //发送请求
            PhotonEngine.Instance.SendRequest(OperationCode.Transcript, parameters);
        }
        
    }
}
