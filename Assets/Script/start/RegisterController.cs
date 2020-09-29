using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XueCommon.Model;
using LitJson;
using XueCommon;
using ExitGames.Client.Photon;
using System;

public class RegisterController : ControllerBase {
    private static RegisterController _instance;
    public static RegisterController Instance
    {
        get { return _instance; }
    }
    public override OperationCode OpCode
    {
        get
        {
           return OperationCode.Register;
        }
    }

    public override void Start()
    {
        base.Start();
        _instance = this;
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        //如果注册成功,就加载服务器列表
        Debug.Log("ReturnCode = " + response.ReturnCode);
        if(response.ReturnCode == 0)
        {
            ServerController.Instance.GetServerList();
        }
    }

    public void Register(string username,string password)
    {
        User user = new User() { UserName = username, Password = password };
        //将User对象转成 string格式
        string json = JsonMapper.ToJson(user);
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.User, json);
        PhotonEngine.Instance.SendRequest(OperationCode.Register, parameters);
    }
	
}
