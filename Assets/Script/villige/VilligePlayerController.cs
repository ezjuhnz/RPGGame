using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using System.Collections.Generic;
using LitJson;

public class VilligePlayerController : ControllerBase
{
    private static VilligePlayerController _instance;
    public static VilligePlayerController Instance
    {
        get { return _instance; }
    }
    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.SyncVilligePlayer;
        }
    }
    void Awake()
    {
        _instance = this;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        //DO Nothing
    }

    public override void OnEvent(EventData eventData)
    {
        int roleId = -1;
        //获取SubCode,判断是增加还是删除角色
        SubCode subCode;
        object o = null;
        eventData.Parameters.TryGetValue((byte)ParameterCode.SubCode, out o);
        subCode = (SubCode)o;
        eventData.Parameters.TryGetValue((byte)ParameterCode.RoleId, out o);
        roleId = (int)o;
        switch (subCode)
        {
            case SubCode.SyncPosAndRotation:
                //同步角色位置和旋转
                object posObj = null;
                object rotObj = null;
                eventData.Parameters.TryGetValue((byte)ParameterCode.Position, out posObj);
                eventData.Parameters.TryGetValue((byte)ParameterCode.EulerAngles, out rotObj);
                Vector3Obj pos = JsonMapper.ToObject<Vector3Obj>(posObj.ToString());
                Vector3Obj rotation = JsonMapper.ToObject<Vector3Obj>(rotObj.ToString());
                SyncPositonAndRotationResponse(roleId, pos.ToVector3(), rotation.ToVector3());
                break;
            case SubCode.SyncPlayerMoveAnimation:
                PlayerMoveAnimationModel model = new PlayerMoveAnimationModel();
                eventData.Parameters.TryGetValue((byte)ParameterCode.PlayerMoveAnimationModel, out o);
                model = JsonMapper.ToObject<PlayerMoveAnimationModel>(o.ToString());
                SyncPlayerMoveAnimationResponse(roleId, model);
                break;
            default: break;
        }
    }

    public void SyncPositonAndRotationResponse(int roleid, Vector3 pos, Vector3 rotation)
    {
        Dictionary<int, GameObject> playerDict = GameController.Instance.playerDict;
        //根据roleid找到对应prefab,更新其位置
        GameObject playerGo = null;
        //Debug.Log("服务器对角色进行更新 " + roleid);
        playerDict.TryGetValue(roleid, out playerGo);
        if (playerGo != null)
        {
            playerGo.transform.position = pos;
            playerGo.transform.eulerAngles = rotation;
        }
        else
        {
            Debug.LogWarning("未找到要同步位置的玩家..");
        }
    }

    //处理城镇玩家移动动画
    void SyncPlayerMoveAnimationResponse(int roleid, PlayerMoveAnimationModel model)
    {
        Dictionary<int, GameObject> playerDict = GameController.Instance.playerDict;
        //根据roleid找到对应prefab,更新其位置
        GameObject playerGo = null;
        playerDict.TryGetValue(roleid, out playerGo);
        if (playerGo != null)
        {
            playerGo.GetComponent<PlayerMove>().setAnim(model);
        }
        else
        {
            Debug.LogWarning("未找到要同步动画的玩家..");
        }
    }

    //发起同步位置和旋转请求
    public void SyncPositonAndRotationRequest(Vector3 position, Vector3 eulerAngles)
    {
        //向服务器发起请求
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        Vector3Obj posObj = new Vector3Obj(position);
        Vector3Obj eulerObj = new Vector3Obj(eulerAngles);
        string jsonPos = JsonMapper.ToJson(posObj);
        string jsonEuler = JsonMapper.ToJson(eulerObj);

        parameters.Add((byte)ParameterCode.Position, jsonPos);
        parameters.Add((byte)ParameterCode.EulerAngles, jsonEuler);
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncPosAndRotation);
        
        PhotonEngine.Instance.SendRequest(OperationCode.SyncVilligePlayer, parameters);
    }

    //
    public void SyncPlayerMoveAnimationRequest(PlayerMoveAnimationModel model)
    {
        //向服务器发起请求
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();

        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, TeamInviteController.Instance.globalMasterID);
        
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncPlayerMoveAnimation);
        string json = JsonMapper.ToJson(model);
        parameters.Add((byte)ParameterCode.PlayerMoveAnimationModel, json);

        PhotonEngine.Instance.SendRequest(OperationCode.SyncVilligePlayer, parameters);
    }
}
