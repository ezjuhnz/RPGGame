using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using System.Collections.Generic;
using LitJson;
using XueCommon.Model;

public class LoginController : ControllerBase
{
    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.Login;
        }
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        //如果成功,就加载服务器列表
        if (response.ReturnCode == (short)ReturnCode.Success)
        {
            ServerController.Instance.GetServerList();
        }
        else
        {
            //登录失败
            Debug.LogError(response.DebugMessage);
        }
    }

    public void Login(string username,string password)
    {
        
    }

}
