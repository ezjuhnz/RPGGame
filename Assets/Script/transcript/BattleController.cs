using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using LitJson;
//控制副本中的玩家的位置和旋转
public class BattleController : ControllerBase {
    private static BattleController _instance;

    public static BattleController Instance
    {
        get { return _instance; }
    }
    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.Battle;
        }
    }
    void Awake()
    {
        _instance = this;
    }
    // Use this for initialization
    public override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
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
                SyncTransPosAndRotationResponse(roleId, pos.ToVector3(), rotation.ToVector3());
                break;
            case SubCode.SyncPlayerMoveAnimation:
                //同步角色移动动画
                PlayerMoveAnimationModel model = new PlayerMoveAnimationModel();
                eventData.Parameters.TryGetValue((byte)ParameterCode.PlayerMoveAnimationModel, out o);
                model = JsonMapper.ToObject<PlayerMoveAnimationModel>(o.ToString());
                SyncPlayerMoveAnimationResponse(roleId,model);
                break;
            case SubCode.SyncPlayerAttackAnimation:
                //同步角色攻击动画
                PlayerAnimationModel playerAnimationModel = new PlayerAnimationModel();
                eventData.Parameters.TryGetValue((byte)ParameterCode.PlayerAnimationModel, out o);
                playerAnimationModel = JsonMapper.ToObject<PlayerAnimationModel>(o.ToString());
               
                SyncPlayerAttackAnimationResponse(roleId, playerAnimationModel);
                break;
            case SubCode.SyncEnermySpawn:
                SyncEnermySpawnResponse();
                break;
            case SubCode.BackToHome:
                BackToHomeResponse();
                break;
            case SubCode.FightAgain:
                FightAgainResponse();
                break;
            case SubCode.SyncPlayerHpBar:
                int hp = 0;
                int hurtNum = 0;
                int targetId = 0;
                eventData.Parameters.TryGetValue((byte)ParameterCode.PlayerHp, out o);
                hp = (int)o;
                eventData.Parameters.TryGetValue((byte)ParameterCode.EnermyHurtNum, out o);
                hurtNum = (int)o;
                eventData.Parameters.TryGetValue((byte)ParameterCode.TargetId, out o);
                targetId = (int)o;
                SyncPlayerHpBarResponse(hp,hurtNum,targetId);
                break;
            default: break;
        }
    }

    public void SyncPlayerHpBarRequest(int hp,int hurtNum,int targetId)
    {
        //向服务器发起请求

        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.TargetId,targetId);
        parameters.Add((byte)ParameterCode.PlayerHp, hp);
        parameters.Add((byte)ParameterCode.EnermyHurtNum, hurtNum);
        parameters.Add((byte)ParameterCode.MasterRoleID, TeamInviteController.Instance.globalMasterID);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncPlayerHpBar);

        PhotonEngine.Instance.SendRequest(OperationCode.Battle, parameters);
    }
    public void SyncTransPosAndRotationRequest(Vector3 position, Vector3 eulerAngles)
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
        parameters.Add((byte)ParameterCode.MasterRoleID, TeamInviteController.Instance.globalMasterID);
        parameters.Add((byte)ParameterCode.MemberIndex, PhotonEngine.Instance.memeberindex);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncPosAndRotation);
        
        PhotonEngine.Instance.SendRequest(OperationCode.Battle, parameters);
    }
    //同步玩家移动动画
    public void SyncPlayerMoveAnimationRequest(PlayerMoveAnimationModel model)
    {
        //向服务器发起请求
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, TeamInviteController.Instance.globalMasterID);
        //memberIndex将弃用
        parameters.Add((byte)ParameterCode.MemberIndex, PhotonEngine.Instance.memeberindex);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncPlayerMoveAnimation);
        string json = JsonMapper.ToJson(model);
        parameters.Add((byte)ParameterCode.PlayerMoveAnimationModel, json);

        PhotonEngine.Instance.SendRequest(OperationCode.Battle, parameters);
    }

    //同步玩家攻击动画请求
    public void SyncPlayerAttackAnimationRequest(PlayerAnimationModel model,int roleId,int damage = 0)
    {
        //向服务器发起请求
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.RoleId, roleId);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        parameters.Add((byte)ParameterCode.MemberIndex, PhotonEngine.Instance.memeberindex);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncPlayerAttackAnimation);
        string json = JsonMapper.ToJson(model);
        parameters.Add((byte)ParameterCode.PlayerAnimationModel, json);
        parameters.Add((byte)ParameterCode.Damage, damage);

        PhotonEngine.Instance.SendRequest(OperationCode.Battle, parameters);
    }

    //同步怪物生成
    public void SyncEnermySpawnRequest()
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();

        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        parameters.Add((byte)ParameterCode.MemberIndex, PhotonEngine.Instance.memeberindex);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncPlayerAttackAnimation);
        
        PhotonEngine.Instance.SendRequest(OperationCode.Battle, parameters);
    }

    //返回城镇
    public void BackToHomeRequest()
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();

        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.BackToHome);

        PhotonEngine.Instance.SendRequest(OperationCode.Battle, parameters);
    }

    //再次挑战
    public void FightAgainRequest()
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();

        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        Debug.Log("MasterRoleId=" + PhotonEngine.Instance.masterid);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.FightAgain);

        PhotonEngine.Instance.SendRequest(OperationCode.Battle, parameters);
    }

    public void SyncTransPosAndRotationResponse(int roleid, Vector3 pos, Vector3 rotation)
    {
        Dictionary<int, GameObject> playerDict = PlayerController.Instance.transPlayerDict;
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

    public void SyncPlayerMoveAnimationResponse(int roleid,PlayerMoveAnimationModel model)
    {
        Dictionary<int, GameObject> playerDict = PlayerController.Instance.transPlayerDict;
        //根据roleid找到对应prefab,更新其位置
        GameObject playerGo = null;
        playerDict.TryGetValue(roleid, out playerGo);
        if (playerGo != null)
        {
            playerGo.GetComponent<PlayerTransMove>().setAnim(model);
        }
        else
        {
            Debug.LogWarning("未找到要同步动画的玩家..");
        }
    }

    //同步玩家攻击,受伤动画处理
    public void SyncPlayerAttackAnimationResponse(int roleid,PlayerAnimationModel model)
    {
        Debug.Log("SyncPlayerAttackAnimationResponse roleid=" + roleid);
        Dictionary<int, GameObject> playerDict = PlayerController.Instance.transPlayerDict;
        //根据roleid找到对应prefab,更新其位置
        GameObject playerGo = null;
        playerDict.TryGetValue(roleid, out playerGo);
  
        if (playerGo != null)
        {
            playerGo.GetComponent<PlayerTransMove>().SetAttackAnim(roleid,model);
        }
        else
        {
            Debug.LogWarning("未找到要同步动画的玩家..");
        }
    }
    //同步怪物的生成
    public void SyncEnermySpawnResponse()
    {

    }

    //同步回城
    void BackToHomeResponse()
    {
        Debug.Log("BackToHomeResponse==");
        Application.LoadLevelAsync(1);//加载城镇场景
    }

    void FightAgainResponse()
    {
        Debug.Log("FightAgainResponse");
        Application.LoadLevelAsync(2);//加载本场景
    }

    void SyncPlayerHpBarResponse(int hp,int hurtNum,int targetid)
    {
        GameObject playerGo = null;
        PlayerController.Instance.transPlayerDict.TryGetValue(targetid, out playerGo);
        if(playerGo != null)
        {
            PlayerAttack playerAttack = playerGo.GetComponent<PlayerAttack>();
            //playerAttack.ShowHurtNum(hurtNum);//1.显示伤害数字
            //更新Player的HP值
            playerAttack.hp = hp;
            //如果HP为0,则调用死亡
            if(hp <= 0)
            {
                playerAttack.Dead();
            }
            if(targetid == PhotonEngine.Instance.role.ID)
            {
                //2.被攻击对象所属客户端才会显示受伤红屏效果
                BloodScreen.Instance.ShowBloodScreen();
                playerAttack.UpdatePlayerHpBar(hp);
            }
        }
    }
    public override void OnOperationResponse(OperationResponse response)
    {
        //
    }
}
