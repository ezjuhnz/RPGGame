using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using XueCommon;
using XueCommon.Model;
using XueCommon.Tools;

//this application is suit for photonserver 4.0 or above
public class PhotonEngine : MonoBehaviour ,IPhotonPeerListener
{
    public ConnectionProtocol protocol = ConnectionProtocol.Tcp;//通信协议
   
    public string serverAddress = "https://192.168.137.1:4530"; //IP地址
    public string applicationName = "DNFServer"; //服务器名

    public static PhotonEngine _instance;
    private PhotonPeer peer;

    private Dictionary<byte, ControllerBase> controllers = new Dictionary<byte, ControllerBase>();

    private int timeSpanMs = 100;//100毫秒
    int nextSendTickCount = Environment.TickCount;

    //存储当前用户的角色信息
    public Role role;
    //Channel id
    private int channelId;
    public int masterid = -1;
    public int memeberindex = -1;
    public int joinerId = -1; //邀请组队时,被点击的那个人的id

    public bool isFirstLoad = true;
   
    public int ChannelId
    {
        get; set;
    }
    public void DebugReturn(DebugLevel level, string message)
    {
        
    }

    public void OnEvent(EventData eventData)
    {
        ControllerBase controller;
        OperationCode opCode;
        object o = null;
        eventData.Parameters.TryGetValue((byte)ParameterCode.OperationCode, out o);
        opCode = (OperationCode)o;
        controllers.TryGetValue((byte)opCode, out controller);
        if (controller != null)
        {
            controller.OnEvent(eventData);
        }
        else
        {
            Debug.LogWarning("unknown event . OperationCode: " + opCode);
        }
    }

    //服务器响应后,调用该函数
    public void OnOperationResponse(OperationResponse operationResponse)
    {
        //1.从controllers中获取controller,不同类型的controller有不同的功能
        ControllerBase controller;
        controllers.TryGetValue((byte)operationResponse.OperationCode, out controller);
        if(controller != null)
        {
            controller.OnOperationResponse(operationResponse);
        }
        else
        {
            Debug.Log("Receive a unknown response . OperationCode :" + operationResponse.OperationCode);
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        
    }
    //1.向PhotonServer发送请求
    public void SendRequest(OperationCode opCode, Dictionary<byte, object> parameters)
    {
        //Debug.Log("晓雪,SendRequest opCode= "+opCode);
        peer.OpCustom((byte)opCode, parameters, true);
    }

    void Awake()
    {
        _instance = this;//该脚本是绑定在Unity游戏组件中,相当于unity实例化了该对象
        peer = new PhotonPeer(this,protocol);
        peer.Connect(serverAddress, applicationName);
        role = new Role();
        DontDestroyOnLoad(this);
    }
    void Update()
    {
        //保持与服务器通信,为了避免过于频繁,我们加入了时间限制
        if (peer !=null && Environment.TickCount > this.nextSendTickCount)
        {
            peer.Service();//必要代码,否则无法与服务端通信
            this.nextSendTickCount = Environment.TickCount + timeSpanMs;
        }
    }
    public static PhotonEngine Instance
    {
        get
        { return _instance; }
    }

    public void RegisterController(OperationCode opCode,ControllerBase controller)
    {
        if(controllers.ContainsKey((byte)opCode))
        {
            return;
        }
        controllers.Add((byte)opCode, controller);
    }

    public void UnRegisterController(OperationCode opCode)
    {
        controllers.Remove((byte)opCode);
    }
}
