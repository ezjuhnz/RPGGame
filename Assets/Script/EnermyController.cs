using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using System.Collections.Generic;
using LitJson;

public class EnermyController : ControllerBase {
    public static EnermyController _instance;
    //public is for test, private is better
    private Dictionary<string, GameObject> enermyGoDict = new Dictionary<string, GameObject>();
    public static EnermyController Instance
    {
        get { return _instance; }
    }
    public Dictionary<string, GameObject> EnermyGoDict
    {
        get { return enermyGoDict; }
    }
    
    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.SyncEnermy;
        }
    }

    // Use this for initialization
    public override void Start () {
        base.Start();
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public override void OnOperationResponse(OperationResponse response)
    {
        //
    }

    public override void OnEvent(EventData eventData)
    {
        //Debug.Log("EnermyController OnEvent...");
        //获取SubCode,判断是增加还是删除角色
        SubCode subCode;
        object o = null;
        eventData.Parameters.TryGetValue((byte)ParameterCode.SubCode, out o);
        subCode = (SubCode)o;

        switch (subCode)
        {
            case SubCode.SyncEnermySpawn:
                //同步敌人生成
                SyncEnermySpawnResponse(eventData.Parameters);
                break;
            case SubCode.SyncEnermyPosAndRotation:
                //同步敌人位置和旋转
                SyncEnermyPosAndRotationResponse(eventData.Parameters);
                break;
            case SubCode.SyncEnermyMoveAnimation:
                //同步敌人移动动画
                SyncEnermyMoveAnimationResponse(eventData.Parameters);
                break;
            case SubCode.SyncEnermyAnimation:
                //同步敌人除移动动画外的其它动画
                SyncEnermyAnimationResposne(eventData.Parameters);
                break;
            case SubCode.SyncBossAnimation:
                SyncBossAniamtionResponse(eventData.Parameters);
                break;
            case SubCode.SyncEnermyHp:
                SyncEnermyHpResponse(eventData.Parameters);
                break;
            default: break;
        }
    }

    //同步敌人的生成
    public void SyncEnermySpawnRequest(EnermyModel model)
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();

        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        //memberIndex将弃用
        parameters.Add((byte)ParameterCode.MemberIndex, PhotonEngine.Instance.memeberindex);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        string json = JsonMapper.ToJson(model);
        parameters.Add((byte)ParameterCode.EnermyModel, json);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncEnermySpawn);

        PhotonEngine.Instance.SendRequest(OperationCode.SyncEnermy, parameters);
    }

    //同步敌人的位置和旋转请求
    public void SyncEnermyPosAndRotationRequest(string guid, Vector3 position, Vector3 eulerAngles)
    {
        Vector3Obj posObj = new Vector3Obj(position);
        Vector3Obj eulerObj = new Vector3Obj(eulerAngles);
        string posJson = JsonMapper.ToJson(posObj);
        string eulerJson = JsonMapper.ToJson(eulerObj);
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.EnermyGuid, guid);
        parameters.Add((byte)ParameterCode.Position, posJson);
        parameters.Add((byte)ParameterCode.EulerAngles, eulerJson);
        //其它必要参数:1.channelid , 2.role id, 3.master id,
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncEnermyPosAndRotation);
        //发送请求
        PhotonEngine.Instance.SendRequest(OperationCode.SyncEnermy, parameters);
    }

    //同步敌人移动动画
    public void SyncEnermyMoveAnimationRequest(string guid,EnermyMoveAnimationModel model)
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncEnermyMoveAnimation);
        string json = JsonMapper.ToJson(model);
        parameters.Add((byte)ParameterCode.EnermyMoveAnimationModel, json);
        parameters.Add((byte)ParameterCode.EnermyGuid, guid);
        //其它必要参数:
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        //发送请求
        PhotonEngine.Instance.SendRequest(OperationCode.SyncEnermy, parameters);
    }

    //同步普通怪物除移动动画外的其它动画
    public void SyncEnermyAnimationRequest(string guid,EnermyAnimationModel model)
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncEnermyAnimation);
        string json = JsonMapper.ToJson(model);
        parameters.Add((byte)ParameterCode.EnermyAnimationModel, json);
        parameters.Add((byte)ParameterCode.EnermyGuid, guid);
        //其它必要参数:
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        //发送请求
        PhotonEngine.Instance.SendRequest(OperationCode.SyncEnermy, parameters);
    }

    public void SyncEnermyHpRequest(string guid,int hp,int hurtNum,int killerid)
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncEnermyHp);
        
        parameters.Add((byte)ParameterCode.EnermyGuid, guid);
        parameters.Add((byte)ParameterCode.EnermyHp, hp);
        parameters.Add((byte)ParameterCode.EnermyHurtNum, hurtNum);
        //其它必要参数:
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);//killerid
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);

        //发送请求
        PhotonEngine.Instance.SendRequest(OperationCode.SyncEnermy, parameters);
    }

    //同步Boss 动画(移动动画除外)
    public void SyncBossAnimationRequest(string guid, BossAnimationModel model)
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.SyncBossAnimation);
        string json = JsonMapper.ToJson(model);
        parameters.Add((byte)ParameterCode.BossAnimationModel, json);
        parameters.Add((byte)ParameterCode.EnermyGuid, guid);
        //其它必要参数:
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.RoleId, PhotonEngine.Instance.role.ID);
        parameters.Add((byte)ParameterCode.MasterRoleID, PhotonEngine.Instance.masterid);
        //发送请求
        PhotonEngine.Instance.SendRequest(OperationCode.SyncEnermy, parameters);
    }
    //同步敌人生成: 实例化Master客户端发来的Enermy List
    void SyncEnermySpawnResponse(Dictionary<byte,object> parameters)
    {
        Debug.Log("SyncEnermySpawnResponse===");
        object o = null;
        parameters.TryGetValue((byte)ParameterCode.EnermyModel, out o);
        EnermyModel model = JsonMapper.ToObject<EnermyModel>(o.ToString());

        foreach(EnermyProperty enermy in model.list)
        {
            GameObject go = GameObject.Instantiate(Resources.Load("Enermy/" + enermy.prefabName), 
                enermy.position.ToVector3(),
                Quaternion.identity) as GameObject;
            if(go.GetComponent<Enermy>())
            {
                go.GetComponent<Enermy>().GUID = enermy.guid;
            }
            if(go.GetComponent<Boss>())
            {
                go.GetComponent<Boss>().GUID = enermy.guid;
            }
            Debug.Log("敌人 " + enermy.guid + "将要进入集合");
            //非master客户端在此处将敌人加入Dict
            enermyGoDict.Add(enermy.guid, go);
        }
    }

    //用服务器发送的数据同步敌人的位置和旋转
    void SyncEnermyPosAndRotationResponse(Dictionary<byte, object> parameters)
    {
        string guid = "";
        object o = null;
        Vector3Obj posObj = null;
        Vector3Obj eulerObj = null;
        parameters.TryGetValue((byte)ParameterCode.Position, out o);
        posObj = JsonMapper.ToObject<Vector3Obj>(o.ToString());
        parameters.TryGetValue((byte)ParameterCode.EulerAngles, out o);
        eulerObj = JsonMapper.ToObject<Vector3Obj>(o.ToString());
        parameters.TryGetValue((byte)ParameterCode.EnermyGuid, out o);
        guid = o.ToString();
        //找到对应的 enermy 进行更新
        GameObject enermyGo = null;
        enermyGoDict.TryGetValue(guid, out enermyGo);
        if(enermyGo != null)
        {
            //Debug.Log("posObj ==" + posObj.ToVector3());
            enermyGo.transform.position = posObj.ToVector3();
            enermyGo.transform.eulerAngles = eulerObj.ToVector3();
        }
    }

    //
    void SyncEnermyMoveAnimationResponse(Dictionary<byte,object> parameters)
    {
        //根据guid获取 enermyGo,更新之
        string guid = "";
        object o = null;
        parameters.TryGetValue((byte)ParameterCode.EnermyGuid, out o);
        guid = o.ToString();
        Animation anim = null;
        GameObject enermyGo = null;
        EnermyController.Instance.enermyGoDict.TryGetValue(guid, out enermyGo);
        parameters.TryGetValue((byte)ParameterCode.EnermyMoveAnimationModel, out o);
        EnermyMoveAnimationModel model = new EnermyMoveAnimationModel();
        model = JsonMapper.ToObject<EnermyMoveAnimationModel>(o.ToString());
        if(enermyGo != null)
        {
            anim = enermyGo.GetComponent<Animation>();
        }
        if (anim == null) return;
        DateTime dt = DateTime.Now;
        if(dt > model.GetTime())
        {
            if(model.IsMove)
            {
                anim.Play("walk");
            }
            else
            {
                anim.Play("idle");
            }
        }
    }

    void SyncEnermyAnimationResposne(Dictionary<byte,object> parameters)
    {
        //根据guid获取 enermyGo,更新之
        string guid = "";
        object o = null;
        parameters.TryGetValue((byte)ParameterCode.EnermyGuid, out o);
        guid = o.ToString();
        GameObject enermyGo = null;
        
        EnermyController.Instance.enermyGoDict.TryGetValue(guid, out enermyGo);
        if (enermyGo == null) return;
        parameters.TryGetValue((byte)ParameterCode.EnermyAnimationModel, out o);
        EnermyAnimationModel model = new EnermyAnimationModel();
        model = JsonMapper.ToObject<EnermyAnimationModel>(o.ToString());
        Animation anim = enermyGo.GetComponent<Animation>();//null reference
       
        if(model.attack)
        {
            anim.Play("attack01");
        }

        if(model.die)
        {
            enermyGo.GetComponent<Enermy>().Dead(model.damage);
            anim.Play("die");
        }
        if(model.takeDamage)
        {
            anim.Play("takedamage");
            TakeDamageResponse(guid,model.hp,model.hurtNum);
        }
    }
    //SyncEnermyHpResponse
    void SyncEnermyHpResponse(Dictionary<byte,object> parameters)
    {
        object o = null;
        string guid = "";
        parameters.TryGetValue((byte)ParameterCode.EnermyHp, out o);
        int hp = (int)o;
        parameters.TryGetValue((byte)ParameterCode.EnermyGuid, out o);
        guid = o.ToString();
        parameters.TryGetValue((byte)ParameterCode.EnermyHurtNum, out o);
        int hurtNum = (int)o;
        Debug.Log("SyncEnermyHpResponse hurtNum/hp=" + hurtNum +"/" + hp);
       
        GameObject enermyGo = null;
        enermyGoDict.TryGetValue(guid, out enermyGo);
        if(enermyGo != null)
        {
            if (enermyGo.GetComponent<Enermy>()) //普通怪物
            {
                Enermy enermy = enermyGo.GetComponent<Enermy>();
                if (enermy == null)
                {
                    Debug.LogError("the enermy is not exist!");
                    return;
                }
                enermy.hp = hp;
                enermy.isSyncEnermy = false;
                HpBarController.Instance.ShowHpBar(enermyGo);//显示敌人血条
            }
            else if (enermyGo.GetComponent<Boss>())
            {
                Boss boss = enermyGo.GetComponent<Boss>();
                if (boss == null)
                {
                    Debug.LogError("the boss is not exist!");
                    return;
                }
                boss.hp = hp;
                boss.isSyncBoss = false;
                HpBarController.Instance.ShowBossHpBar(enermyGo);//显示敌人血条
            }
        }
        
        
    }
    //
    void SyncBossAniamtionResponse(Dictionary<byte,object> parameters)
    {
        //根据guid获取 enermyGo,更新之
        string guid = "";
        object o = null;
        parameters.TryGetValue((byte)ParameterCode.EnermyGuid, out o);
        guid = o.ToString();
        GameObject enermyGo = null;

        EnermyController.Instance.enermyGoDict.TryGetValue(guid, out enermyGo);
        parameters.TryGetValue((byte)ParameterCode.BossAnimationModel, out o);
        BossAnimationModel model = new BossAnimationModel();
        model = JsonMapper.ToObject<BossAnimationModel>(o.ToString());
        Animation anim = enermyGo.GetComponent<Animation>();//null reference

        if (model.attack01)
        {
            anim.Play("attack01");
        }
        if (model.attack02)
        {
            anim.Play("attack02");
        }
        if (model.attack03)
        {
            anim.Play("attack03");
        }
        
        if (model.die)
        {
            enermyGo.GetComponent<Boss>().Dead(model.damage);
            anim.Play("die");
        }
        if (model.takeDamage)
        {
            //怪物血条血量减少
            TakeBossDamageResponse(guid, model);
        }
    }

    void TakeDamageResponse(string guid,int hp,int hurtNum)
    {
        GameObject enermyGo = null;
        enermyGoDict.TryGetValue(guid, out enermyGo);
        HUDText hudText = null;
        if(null != enermyGo)
        {
            if (enermyGo.GetComponent<Enermy>())
            {
                Enermy enermy = enermyGo.GetComponent<Enermy>();
                if(enermy == null)
                {
                    Debug.LogError("the enermy is not exist!");
                    return;
                }
                enermy.isSyncEnermy = false;
                hudText = enermy.hudText;
            }
            else if(enermyGo.GetComponent<Boss>())
            {
                Boss boss = enermyGo.GetComponent<Boss>();
                if (boss == null)
                {
                    Debug.LogError("the boss is not exist!");
                    return;
                }
                boss.isSyncBoss = false;
                hudText = boss.hudText;
            }
           
        }
    }

    //
    void TakeBossDamageResponse(string guid, BossAnimationModel model)
    {
        //int damage = model.damage;
        int hp = model.hp;
        GameObject enermyGo = null;
        enermyGoDict.TryGetValue(guid, out enermyGo);
        HUDText hudText = null;
        if (null != enermyGo)
        {
            Boss boss = enermyGo.GetComponent<Boss>();
            hudText = boss.hudText;
            boss.hp = hp;
            //boss.damage = damage;
            //显示血条
           
        }
    }
}
