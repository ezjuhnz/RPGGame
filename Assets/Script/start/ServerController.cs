using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using System.Collections.Generic;
using LitJson;

public class ServerController : ControllerBase
{
    private static ServerController _instance;
    public static ServerController Instance
    {
        get { return _instance; }
    }
    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.GetServer;
        }
    }

    public override void Start()
    {
        base.Start();
        _instance = this;
    }

    public void GetServerList()
    {
        PhotonEngine.Instance.SendRequest(OperationCode.GetServer, new Dictionary<byte, object>());
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        int flag = 0;
       // StartMenuController.Instance.tweenServer.PlayForward();
        //功能:获取服务器列表并在客户端显示
        if(response.ReturnCode == 0)
        {
            Dictionary<byte, object> parameters = response.Parameters;
            object jsonObj = null;
            parameters.TryGetValue((byte)ParameterCode.ServerList, out jsonObj);
            //将object转换成List<ServerProperty>
            List<XueCommon.Model.ServerProperty> serverList = JsonMapper.ToObject<List<XueCommon.Model.ServerProperty>>(jsonObj.ToString());
            //遍历serverList,逐一实例化
            GameObject go = null;
            foreach(XueCommon.Model.ServerProperty spTmp in serverList)
            {
                if(spTmp.Count > 50)
                {
                    //火爆的服务器
                    go = NGUITools.AddChild(StartMenuController.Instance.serverGrid.gameObject,
                        StartMenuController.Instance.serverRed);
                }
                else
                {
                    //畅通的服务器
                   go = NGUITools.AddChild(StartMenuController.Instance.serverGrid.gameObject,
                        StartMenuController.Instance.serverGreen);
                }
                //修改预制体的属性
                ServerProperty sp = go.GetComponent<ServerProperty>();
                sp.Count = spTmp.Count;
                sp.IP = spTmp.IP;
                sp.Name = spTmp.Name;
                sp.Id = spTmp.ID;

                //设置当前选中服务器默认为第一个服务器  
                if(flag == 0)
                {
                    StartMenuController.Instance.curServerGo.GetComponent<UISprite>().spriteName =
                        go.GetComponent<UISprite>().spriteName;
                    StartMenuController.Instance.curServerGo.transform.Find("Label").GetComponent<UILabel>().text =
                        sp.Name;
                }
                flag = 1;

                //隐藏"注册"界面
                StartMenuController.Instance.registerPanel.PlayReverse();
                StartMenuController.Instance.registerPanel.gameObject.SetActive(false);
                //隐藏"登录"界面
                StartMenuController.Instance.loginPanel.PlayReverse();
                StartMenuController.Instance.loginPanel.gameObject.SetActive(false);
                //显示server-list游戏组件
                StartMenuController.Instance.tweenServer.gameObject.SetActive(true);
            }
        }
    }
}
