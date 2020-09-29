using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;
using XueCommon.Model;
using LitJson;
using System.Collections.Generic;
using XueCommon.Tools;

public class RoleController : ControllerBase
{
    public GameObject[] starPrefabArr;
    public TweenScale createConfirmTS;

    private static string[] strArr = {"fighter","assi"};
    private Transform uiRootTransform;
    private Transform characterCreateTransform;
    private TweenScale characterCreateTS;
    private TweenScale warningTS;
    private GameObject characterScrollGo;
    private UIScrollView characterScrollView;
    private GameObject gridGo;
    private GameObject characterDetailLeftGo;
    private GameObject characterDetailRightGo;
    private UILabel detailLabel;
    private UILabel powerLabel;
    private UILabel scopeLabel;
    private UILabel speedLabel;
    private UILabel controlLabel;
    private UILabel guardLabel;
    private UILabel inputRoleNameLabel;

    private GameObject currentClickGo;
    private GameObject lastClickGo;
    private GameObject characterListGo;
    private TweenPosition characterListTP;
    private string normalSprite = "半透明背景";
    private string selectedSprite = "技能系统背景";
    
   
    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.GetRole;
        }
    }

    public override void Start()
    {
        uiRootTransform = GameObject.Find("UI Root").transform;
        if(uiRootTransform != null)
        {
            characterCreateTransform = uiRootTransform.Find("bg/character-create");
            characterListGo = uiRootTransform.Find("bg/character-list").gameObject;
            characterScrollGo = characterListGo.transform.Find("character-scroll").gameObject;
            characterScrollView = characterCreateTransform.Find("scroll-list").GetComponent<UIScrollView>();
            characterDetailLeftGo = characterCreateTransform.Find("character-detail/left").gameObject;
            characterDetailRightGo = characterCreateTransform.Find("character-detail/right").gameObject;
            gridGo = characterScrollView.transform.Find("Grid").gameObject;
            characterCreateTS = characterCreateTransform.GetComponent<TweenScale>();
            warningTS = characterCreateTransform.Find("warning").GetComponent<TweenScale>();
            characterListTP = characterListGo.transform.GetComponent<TweenPosition>();
            inputRoleNameLabel = createConfirmTS.transform.Find("input-rolename/Label").GetComponent<UILabel>();
            detailLabel = characterDetailLeftGo.transform.Find("detail").GetComponent<UILabel>();
            powerLabel = characterDetailRightGo.transform.Find("power-label").GetComponent<UILabel>();
            scopeLabel = characterDetailRightGo.transform.Find("scope-label").GetComponent<UILabel>();
            speedLabel = characterDetailRightGo.transform.Find("speed-label").GetComponent<UILabel>();
            controlLabel = characterDetailRightGo.transform.Find("control-label").GetComponent<UILabel>();
            guardLabel = characterDetailRightGo.transform.Find("guard-label").GetComponent<UILabel>();
        }
        base.Start();
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        //根据SubCode,进行不同的操作
        object o = null;
        response.Parameters.TryGetValue((byte)ParameterCode.SubCode, out o);
        SubCode subCode = (SubCode)o;
        Debug.Log("SubCode subCode. =" + subCode);
        switch (subCode)
        {
            case SubCode.GetRoles:
                //遍历所有角色,展示在unity客户端中
                response.Parameters.TryGetValue((byte)ParameterCode.RoleList, out o);
                List<Role> roleList = JsonMapper.ToObject<List<Role>>(o.ToString());
                //展示角色并给某些游戏对象赋值
                OnGetRole(roleList);
                break;
            case SubCode.GetCharacters:
                response.Parameters.TryGetValue((byte)ParameterCode.CharacterList, out o);
                if(o == null)
                {
                    Debug.LogError("no character is get from database!!");
                    return;
                }
                List<Character> characterList = JsonMapper.ToObject<List<Character>>(o.ToString());
                OnGetCharacters(characterList);
                break;
            case SubCode.GetCharacterByID:
                response.Parameters.TryGetValue((byte)ParameterCode.CharacterList, out o);
                if (o == null)
                {
                    Debug.LogError("no character is get from database!!");
                    return;
                }
                Character character = JsonMapper.ToObject<Character>(o.ToString());
                OnGetCharacterByID(character);
                break;
            case SubCode.AddRole:
                //新建角色成功
                OnCreateRoleSuccess();
                break;
 
        }
         
    }

    private void OnGetCharacterByID(Character character)
    {
        //detail默认显示第一个角色的属性
        ShowCharacterDetail(character);
    }

    public void GetRoles()
    {
        //因为在登录的时候,服务端已经记录了当前登录的用户,所以不用再传递用户信息
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.GetRoles);
        //发送请求到Photon Server
        PhotonEngine.Instance.SendRequest(OperationCode.GetRole, parameters);
    }

    //关闭按钮点击事件
    public void OnCloseBtnClick()
    {
        //关闭创建角色界面
        HideCreateCharacterUI();
        //展示已存在的角色列表
        ShowCharacterListUI();
        //关闭输入框
        HideCreateConfirm();
    }

    //新建角色按钮点击
    public void OnCreateRoleClick()
    {
        currentClickGo = null;
        lastClickGo = null;
        //隐藏角色列表UI
        HideCharacterListUI();
        //清除之前的数据
        ClearStar();
        ClearGridCharacters();
        //查询数据库获取可以创建的角色有哪些
        GetCharactersInfo();
        //显示创建角色界面
        ShowCreateCharacterUI();
    }

    //选择哪一个角色
    public void OnCharacterSelectClick(GameObject go)
    {
        currentClickGo = go;
        OutStandSprite(go);
        if(lastClickGo != null && lastClickGo != currentClickGo)
        {
            RestoreSprite(lastClickGo);
        }
        lastClickGo = currentClickGo;
        //查询数据库获取选中的角色的信息
        int id = go.GetComponent<CharacterProperty>().Id;
        GetCharacterByID(id);
    }

    private void GetCharacterByID(int id)
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.GetCharacterByID);
        parameters.Add((byte)ParameterCode.CharacterId, id);
        PhotonEngine.Instance.SendRequest(OperationCode.GetRole, parameters);
    }



    //确认建立角色按钮点击:弹出输入框
    public void CreateRole()
    {
        StopCoroutine("HideWarningTip");
        if(currentClickGo == null)
        {
            //弹出请选择角色
            warningTS.PlayForward();
            StartCoroutine("HideWarningTip");
            return;
        }
        ShowCreateConfirm();
    }

    IEnumerator  HideWarningTip()
    {
        yield return new WaitForSeconds(1);
        warningTS.PlayReverse();
    }

    void ShowCreateConfirm()
    {
        createConfirmTS.PlayForward();
    }

    public void HideCreateConfirm()
    {
        createConfirmTS.PlayReverse();
    }

   

    void GetCharactersInfo()
    {
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.SubCode, SubCode.GetCharacters);

        PhotonEngine.Instance.SendRequest(OperationCode.GetRole, parameters);
    }

    void OnGetCharacters(List<Character> list)
    {
        CharacterProperty cp;
        string prefabName = "";
        int min = 1;
        foreach(Character character in list)
        {
            prefabName = strArr[character.ID-1];
            GameObject prefabGo = Resources.Load("Player/" + prefabName) as GameObject;
            GameObject go = NGUITools.AddChild(gridGo, prefabGo);
            //给初始化的go绑定OnClick事件
            UIButton btn = go.GetComponent<UIButton>();
            EventDelegate ed = new EventDelegate(this, "OnCharacterSelectClick");
            ed.parameters[0].obj = go;
            btn.onClick.Add(ed);
            
            //修改go中的CharacterProperty属性
            cp = go.GetComponent<CharacterProperty>();
            cp.Id = character.ID;
            cp.Name = character.Name;
            cp.PowerStar = character.PowerStar;
            cp.SpeedStar = character.SpeedStar;
            cp.ControlStar = character.ControlStar;
            cp.GuardStar = character.GuardStar;
            cp.ScopeStar = character.ScopeStar;
            gridGo.GetComponent<UIGrid>().AddChild(go.transform);
            //detail默认显示第一个角色的属性
            if(min == 1)
            {
                ShowCharacterDetail(character);
            }
            min++;
        }
    }

    private void ShowCharacterDetail(Character character)
    {
        GameObject powerGo;
        GameObject speedGo;
        GameObject guardGo;
        GameObject scopeGo;
        GameObject controlGo;
        detailLabel.text = character.Description; //角色描述
        //清除右边的星星
        ClearStar();
        
        powerGo = NGUITools.AddChild(powerLabel.gameObject, starPrefabArr[character.PowerStar - 1]);
        speedGo = NGUITools.AddChild(speedLabel.gameObject, starPrefabArr[character.SpeedStar - 1]);
        guardGo = NGUITools.AddChild(guardLabel.gameObject, starPrefabArr[character.GuardStar - 1]);
        scopeGo = NGUITools.AddChild(scopeLabel.gameObject, starPrefabArr[character.ScopeStar - 1]);
        controlGo = NGUITools.AddChild(controlLabel.gameObject, starPrefabArr[character.ControlStar - 1]);

        powerGo.transform.localPosition = new Vector3(150, 0, 0); //向右偏移150
        speedGo.transform.localPosition = new Vector3(150, 0, 0);
        guardGo.transform.localPosition = new Vector3(150, 0, 0);
        scopeGo.transform.localPosition = new Vector3(150, 0, 0);
        controlGo.transform.localPosition = new Vector3(150, 0, 0);
    }

    private void ClearStar()
    {
        int childCount = powerLabel.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(powerLabel.transform.GetChild(i).gameObject);
        }
        childCount = speedLabel.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(speedLabel.transform.GetChild(i).gameObject);
        }
        childCount = guardLabel.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(guardLabel.transform.GetChild(i).gameObject);
        }
        childCount = controlLabel.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(controlLabel.transform.GetChild(i).gameObject);
        }
        childCount = scopeLabel.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(scopeLabel.transform.GetChild(i).gameObject);
        }
    }

    void RestoreSprite(GameObject go)
    {
        go.GetComponent<UISprite>().spriteName = normalSprite;
        UIButton btn = go.GetComponent<UIButton>();
        btn.normalSprite = normalSprite;
        btn.pressedSprite = normalSprite;
        btn.disabledSprite = normalSprite;
        btn.hoverSprite = normalSprite;
    }

    void OutStandSprite(GameObject go)
    {
        UIButton btn = go.GetComponent<UIButton>();
        go.GetComponent<UISprite>().spriteName = selectedSprite;
        btn.normalSprite = selectedSprite;
        btn.pressedSprite = selectedSprite;
        btn.disabledSprite = selectedSprite;
        btn.hoverSprite = selectedSprite;
    }

    public void CreateNewRoleRequest()
    {
        //获取新建角色的信息:角色唯一标识ID
        int characterId = currentClickGo.GetComponent<CharacterProperty>().Id;
        //int userId = PhotonEngine.Instance.role.User.ID;
        string roleName = inputRoleNameLabel.text;
        if(roleName.Length > 10)
        {
            Debug.LogError("角色名不能超过10个字符");
        }
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        parameters.Add((byte)ParameterCode.CharacterId, characterId);
        //parameters.Add((byte)ParameterCode.UserId, userId);
        parameters.Add((byte)ParameterCode.RoleName, roleName);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.AddRole);
        //发送请求
        PhotonEngine.Instance.SendRequest(OperationCode.GetRole, parameters);
    }

    void OnCreateRoleSuccess()
    {
        //清空之前的角色列表
        ClearCharacterList();
        //刷新角色列表
        GetRoles();
        //关闭创建角色界面
        HideCreateCharacterUI();
        //显示角色列表
        characterListTP.PlayForward();
        //关闭输入框
        HideCreateConfirm();
    }

    void HideCharacterListUI()
    {
        characterListTP.PlayReverse();
    }

    void ShowCharacterListUI()
    {
        characterListTP.PlayForward();
    }
    void HideCreateCharacterUI()
    {
        characterCreateTS.PlayReverse();
    }

    void ShowCreateCharacterUI()
    {
        characterCreateTS.PlayForward();
    }

    void ClearCharacterList()
    {
        Transform gridTransform = characterScrollGo.transform.FindChild("Grid");
        int childCount = gridTransform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            Destroy(gridTransform.GetChild(i).gameObject);
        }
    }

    void ClearGridCharacters()
    {
        int childCount = gridGo.transform.childCount;
        for(int i =0; i < childCount; i++)
        {
            Destroy(gridGo.transform.GetChild(i).gameObject);
        }
    }

    //定义事件
    /*Events are a special kind of multicast delegate that can only be invoked from within 
    the class or struct where they are declared(the publisher class).
    */
    public event OnGetRoleEvent OnGetRole;
}
