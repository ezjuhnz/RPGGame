using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using System.Collections.Generic;

public class InviteRequestController : MonoBehaviour {
   
    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //邀请组队点击'同意'调用该方法
    public void OnAgreeClick()
    {
        string inviteName = "";
        string joinerName = "";
        int roleid = PhotonEngine.Instance.role.ID; //当前角色id
        int channelid = PhotonEngine.Instance.ChannelId;
        int masterid = TeamInviteController.Instance.globalMasterID;
        
        int inviteid = TeamInviteController.Instance.inviteid;
        inviteName = TeamInviteController.Instance.InviteName;
        joinerName = PhotonEngine.Instance.role.Name;
        Debug.Log("globalMasterID= " + masterid);

        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.AgreeInvite);
        parameters.Add((byte)ParameterCode.ChannelID, channelid);
        parameters.Add((byte)ParameterCode.MasterRoleID, masterid);
        parameters.Add((byte)ParameterCode.InviteId, inviteid);
        parameters.Add((byte)ParameterCode.InviteName, inviteName);
       
        parameters.Add((byte)ParameterCode.RoleId, roleid);
        parameters.Add((byte)ParameterCode.JoinerName, joinerName);
       
        //通知服务器同意组队
        PhotonEngine.Instance.SendRequest(OperationCode.SendTeam, parameters);

        //隐藏询问组队请求窗口
        this.gameObject.SetActive(false);
    }

    //邀请组队点击拒绝调用该方法
    public void OnRejectClick()
    {
        int roleid = PhotonEngine.Instance.role.ID; //当前角色id
        int masterid = TeamInviteController.Instance.globalMasterID;
        int channelid = PhotonEngine.Instance.ChannelId;
        
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.RejectInvite);
        parameters.Add((byte)ParameterCode.MasterRoleID, masterid);
        parameters.Add((byte)ParameterCode.RoleId, roleid);
        parameters.Add((byte)ParameterCode.ChannelID, channelid);
        //通知服务器拒绝组队
        PhotonEngine.Instance.SendRequest(OperationCode.SendTeam, parameters);
        //隐藏询问组队请求窗口
        this.gameObject.SetActive(false);
    }

    //同意申请者加入队伍
    public void OnJoinAgreeClick()
    {
        int roleid = PhotonEngine.Instance.role.ID; //当前角色id
        int channelid = PhotonEngine.Instance.ChannelId;
        int masterid = TeamInviteController.Instance.globalMasterID;
        int inviteid = TeamInviteController.Instance.inviteid;

        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.AgreeJoin);
        parameters.Add((byte)ParameterCode.MasterRoleID, masterid);
        parameters.Add((byte)ParameterCode.InviteId, inviteid);
        parameters.Add((byte)ParameterCode.RoleId, roleid);
        parameters.Add((byte)ParameterCode.ChannelID, channelid);
        //通知服务器同意组队
        PhotonEngine.Instance.SendRequest(OperationCode.SendTeam, parameters);

        //隐藏询问组队请求窗口
        this.gameObject.SetActive(false);
    }

    //拒绝申请者加入队伍
    public void OnJoinRejectClick()
    {
        int roleid = PhotonEngine.Instance.role.ID; //当前角色id
        int masterid = TeamInviteController.Instance.globalMasterID;
        int channelid = PhotonEngine.Instance.ChannelId;

        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.RejectJoin);
        parameters.Add((byte)ParameterCode.MasterRoleID, masterid);
        parameters.Add((byte)ParameterCode.RoleId, roleid);
        parameters.Add((byte)ParameterCode.ChannelID, channelid);
        //通知服务器拒绝组队
        PhotonEngine.Instance.SendRequest(OperationCode.SendTeam, parameters);
        //隐藏询问组队请求窗口
        this.gameObject.SetActive(false);
    }
}
