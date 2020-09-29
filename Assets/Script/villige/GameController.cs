using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XueCommon.Model;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using LitJson;

//城镇Controller
public class GameController : ControllerBase {
    //public Camera nguiCamera;

    private static GameController _instance;
    private string girlPrefabName = "Player-Villige-Girl";
    private string boyPrefabName = "Player-Villige-Boy";
    private Transform playerPos; //玩家在城镇的默认初始化位置
    private Vector3 entryPos;
    private Transform uiRootTransform; //UIRoot
    private int uiRootWidth = 1038;
    private int uiRootHeigh = 630;
    private Vector3 uiCenterPoint;

    public GameObject teamBg;
    private TweenScale teamBgTween;
    public Dictionary<int,GameObject> playerDict = new Dictionary<int, GameObject>();
    public static GameController Instance
    {
        get { return _instance; }
    }

    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.SynRoleInChannel;
        }
    }

    //该方法有两种调用场景:
    //1.用户登录成功,进入城镇
    //2.玩家退出副本,返回城镇
    void Awake()
    {
        _instance = this;
        //向服务器发送EnterChannel信号,初始化城镇角色
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        Role role = PhotonEngine.Instance.role;
        string json = JsonMapper.ToJson(role);
        parameters.Add((byte)ParameterCode.Role, json);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.EnterChannel);
        //对于玩家退出副本返回城镇的情况:
        //因为进入副本会销毁当前客户端所在频道所有城镇玩家(包括没进入副本的玩家)
        //所以在返回时,需要重新初始化所有当前频道的角色
        PhotonEngine.Instance.SendRequest(OperationCode.SynRoleInChannel, parameters);
    }

    public override void Start()
    {
        uiCenterPoint = new Vector3(uiRootWidth / 2, uiRootHeigh / 2, 0);
        playerPos = GameObject.Find("Player-pos").transform;
        entryPos = GameObject.Find("TranscriptEntry").transform.position;
        uiRootTransform = GameObject.Find("UI Root").transform;
        teamBgTween = teamBg.GetComponent<TweenScale>();
        base.Start();
        this.OnArriveEntry += EnterTranscript;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        this.OnArriveEntry -= EnterTranscript;
    }

    //角色走到副本入口,执行该方法
    public void EnterTranscript()
    {
        //销毁城镇角色(全部)
        GameObject[] goArr = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject go in goArr)
        {
            Destroy(go);
        }
        AsyncOperation operation = Application.LoadLevelAsync(2);
        LoadSceneProgressBar._instance.Show(operation);
    }

    public override void OnOperationResponse(OperationResponse response)
    {

    }

    public override void OnEvent(EventData eventData)
    {
        SubCode subCode;
        object o = null;
        eventData.Parameters.TryGetValue((byte)ParameterCode.SubCode, out o);
        subCode = (SubCode)o;
        switch(subCode)
        {
            case SubCode.EnterChannel:
                //角色进入频道
                EnterChannelResponse(eventData);
                break;
            case SubCode.ExitChannel:
                //角色退出频道
                ExitChannelResponse(eventData);
                break;
            default: break;
        }


    }
    //该方法有两种调用场景:
    //1.用户登录成功,进入城镇
    //2.玩家退出副本,返回城镇
    void EnterChannelResponse(EventData eventData)
    {
        object o = null;
        Vector3Obj posObj = null;
        Vector3Obj eulerObj = null;
        GameObject playerGo = null;
        string prefabName = girlPrefabName;
        
        eventData.Parameters.TryGetValue((byte)ParameterCode.RoleList, out o);
        if (null != o)
        {
            List<Role> roleList = JsonMapper.ToObject<List<Role>>(o.ToString());
            if(roleList != null)
            {
                //1.组队:只有组队进入副本,在返回城镇时,isTeam才为true
                if (TeamInviteController.Instance && TeamInviteController.Instance.isTeam) 
                {
                    foreach (Role role in roleList)
                    {
                        if (role.CharacterId == 1) //hard code at present
                        {
                            prefabName = boyPrefabName;
                        }

                        if (role.position != null)
                        {
                            posObj = JsonMapper.ToObject<Vector3Obj>(role.position.ToString());
                            eulerObj = JsonMapper.ToObject<Vector3Obj>(role.eulerAngles.ToString());
                            
                            if (Math.Abs(entryPos.x - posObj.ToVector3().x) < 3)
                            {
                                //如果是队长,且与副本入口距离小于3,则稍远离入口,避免刚回城就重新进入副本
                                if (role.ID == TeamInviteController.Instance.globalMasterID)
                                {
                                    posObj.x += 3;
                                }
                            }
                            playerGo = GameObject.Instantiate(Resources.Load("Player/" + prefabName), posObj.ToVector3(), Quaternion.identity) as GameObject;
                            playerGo.transform.eulerAngles = eulerObj.ToVector3();
                        }
                        else
                        {
                            playerGo = GameObject.Instantiate(Resources.Load("Player/" + prefabName), playerPos.position, Quaternion.identity) as GameObject;
                        }
                        playerGo.GetComponent<Player>().roleId = role.ID;
                        playerGo.GetComponent<Player>().Level = role.Level;
                        playerGo.GetComponent<Player>().Name = role.Name;
                        if (PhotonEngine.Instance.role.ID == role.ID)
                        {
                            playerGo.GetComponent<Player>().isTeam = true;
                            //重新给摄像机赋值,以跟踪当前角色
                            FollowTarget.Instance.player = playerGo.transform;
                            playerGo.GetComponent<PlayerMove>().isCanControl = true;
                        }
                        playerDict.Remove(role.ID);
                        playerDict.Add(role.ID, playerGo);//把角色添加到集合
                    }
                }
                else //单人:由登录进入城镇,或单人进入副本后返回城镇都判定为单人
                {
                    Debug.Log("单人==");
                    foreach (Role role in roleList)
                    {
                        Debug.Log("role.characterid=" + role.CharacterId);
                        if(role.CharacterId == 1)
                        {
                            prefabName = boyPrefabName;
                        }
                        if (role.position != null)
                        {
                            posObj = JsonMapper.ToObject<Vector3Obj>(role.position.ToString());
                            eulerObj = JsonMapper.ToObject<Vector3Obj>(role.eulerAngles.ToString());

                            if (Math.Abs(entryPos.x - posObj.ToVector3().x) < 3)
                            {
                                //如果是当前客户端角色,且与副本入口距离小于3,则稍远离入口,避免刚回城就重新进入副本
                                if (role.ID == PhotonEngine.Instance.role.ID)
                                {
                                    posObj.x += 5;
                                }

                            }
                            playerGo = GameObject.Instantiate(Resources.Load("Player/" + prefabName), posObj.ToVector3(), Quaternion.identity) as GameObject;
                            playerGo.transform.eulerAngles = eulerObj.ToVector3();
                        }
                        else
                        {
                            playerGo = GameObject.Instantiate(Resources.Load("Player/" + prefabName), playerPos.position, Quaternion.identity) as GameObject;
                        }
                        playerGo.GetComponent<Player>().roleId = role.ID;
                        playerGo.GetComponent<Player>().Level = role.Level;
                        playerGo.GetComponent<Player>().Name = role.Name;
                        playerGo.GetComponent<Player>().isTeam = false;
                        if (playerDict.ContainsKey(role.ID))
                        {
                            playerDict.Remove(role.ID); //从集合中删除角色
                        }
                        playerDict.Add(role.ID, playerGo);//把角色添加到集合
                        if (PhotonEngine.Instance.role.ID == role.ID)
                        {
                            //重新给摄像机赋值,以跟踪当前角色
                            FollowTarget.Instance.player = playerGo.transform;
                            playerGo.GetComponent<PlayerMove>().isCanControl = true;
                        }
                    }
                }

            }

        }
    }

    //退出频道
    void ExitChannelResponse(EventData eventData)
    {
        object o = null;
        int roleid = 0;
        Role role = new Role();
        eventData.Parameters.TryGetValue((byte)ParameterCode.RoleId, out o);
        roleid = (int)o;
        
        //从playerDict中根据role id查找player
        GameObject playerGo = null;
        playerDict.TryGetValue(roleid, out playerGo);
        if(playerGo != null)
        {
            playerDict.Remove(roleid);//从集合中移除player
            //删除对应的UIFollowTarget
            OverHeadManager.Instance.RemoveOverHead(roleid);
            Destroy(playerGo);
        }
        else
        {
            Debug.LogWarning("未找到要销毁的角色");
        }
        
    }

    public void DelRoleInChannel()
    {
        Role role = PhotonEngine.Instance.role;
        string json = JsonMapper.ToJson(role);
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.Role, json);
        parameters.Add((byte)ParameterCode.ChannelID, PhotonEngine.Instance.ChannelId);
        //退出游戏时,发送请求到photon server,销毁角色
        PhotonEngine.Instance.SendRequest(OperationCode.DelRoleInChannel, parameters);
    }

    public void ShowTeamBg(Vector3 playerPosition,Vector3 mousePosition)
    {
        Debug.Log("ShowTeamBg");
        Vector3 screenPos;
        Vector3 nguiWorldPos;

        screenPos = new Vector3(mousePosition.x - uiCenterPoint.x, mousePosition.y - uiCenterPoint.y, 0);
        screenPos = new Vector3(screenPos.x / uiRootTransform.localScale.x, screenPos.y / uiRootTransform.localScale.y, 0);
        nguiWorldPos = UICamera.mainCamera.ScreenToWorldPoint(screenPos);

        nguiWorldPos.x *= uiRootTransform.localScale.x;
        nguiWorldPos.y *= uiRootTransform.localScale.y;
        nguiWorldPos.z *= uiRootTransform.localScale.z;

        teamBg.transform.position = nguiWorldPos;
        ShowTeamBg();
    }

    public void ShowTeamBg()
    {
        teamBgTween.PlayForward();
    }

    public void HideTeamBg()
    {
        teamBgTween.PlayReverse();
    }


    //事件定义
    public event OnArriveEntryEvent OnArriveEntry;
}
