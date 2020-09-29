using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using XueCommon;
using XueCommon.Model;

public class StartMenuController : MonoBehaviour {
    public TweenScale startPanelTween;
    public TweenScale loginPanenTween;

    public UILabel userNameLabel;//用户名输入框的值
    public UILabel passwordLabel;//密码输入框的值

    public TweenScale loginPanel;
    public TweenScale registerPanel;

    public UIInput userNameInput;
    public UIInput passwordInput;
    public UIInput repassInput;

    public static string userName;
    public static string password;
    public static string selectedRoleName;

    //服务器列表
    public UIGrid serverGrid;
    public GameObject serverRed;
    public GameObject serverGreen;
    public TweenScale tweenServer;
    public GameObject curServerGo;

    //角色选择列表
    public TweenPosition characterTween;
    public UIGrid characterGrid;
    public GameObject characterBoyGo;  //角色-男
    public GameObject characterGrilGo; //角色-女
    public static string originSprite = "半透明背景";
    public static string characterSelectedSprite = "bg_道具";

    //场景加载进度条
    public TweenPosition LoadSceneTween;

    //各种类型的controller
    //private LoginController loginController;
    private RoleController roleController;

    private static StartMenuController _instance;
    public static StartMenuController Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
        //loginController = this.GetComponent<LoginController>();
        roleController = this.GetComponent<RoleController>();
       
        //注册事件
        roleController.OnGetRole += DisplayRoles;
    }
    void OnDestroy()
    {
        if(roleController != null)
        {
            roleController.OnGetRole -= DisplayRoles;
        }
    }
    //点击"Log in"调用该函数
    public void OnLoginClick()
    {
        //1.获取用户名和密码
        userName = userNameLabel.text;
        password = passwordLabel.text;
        Debug.Log("userName=" + userName);
        //2.发送请求到PhotonServer
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        //2.1构造User对象
        User user = new User() { UserName = userName, Password = password };
        //2.2.将User对象转换成string,并添加到parameters中
        string json = JsonMapper.ToJson(user);
        parameters.Add((byte)ParameterCode.User, json);
        PhotonEngine.Instance.SendRequest(OperationCode.Login, parameters);
    }
	public void OnUserNameClick()
    {
        startPanelTween.PlayForward();
        //为了更好的性能,在隐藏面板后需要将该面板设置为非激活状态
        StartCoroutine(HidePanel(startPanelTween.gameObject));
        loginPanenTween.gameObject.SetActive(true);
        loginPanenTween.PlayForward();
    }

    //隐藏面板
    IEnumerator HidePanel(GameObject go)
    {
        yield return new WaitForSeconds(0.4f);
        go.SetActive(false);
    }

    //1.点击注册按钮,调用该方法:弹出注册窗口
    public void OnClickRegister()
    {
        //1.隐藏登录窗口
        loginPanel.PlayForward();
        //2.显示注册窗口
        registerPanel.gameObject.SetActive(true);
        registerPanel.PlayForward();
    }

    //2.点击注册窗口中的注册并登陆,调用该方法
    public void OnClickRegAndLogin()
    {
        //获取用户名,密码,确认密码
        string userName = userNameInput.value;
        string password = passwordInput.value;
        RegisterController.Instance.Register(userName, password);
    }

    //3.选择服务器:OnServerSelect 是通过SendMessage()调用的
    public void OnServerSelect(GameObject serverGo)
    {
        ServerProperty sp = serverGo.GetComponent<ServerProperty>();
        PhotonEngine.Instance.ChannelId = sp.Id;
        curServerGo.GetComponent<UISprite>().spriteName = sp.GetComponent<UISprite>().spriteName;
        curServerGo.transform.Find("Label").GetComponent<UILabel>().text = sp.Name;
        //删除当前选择服务器的OnPress监听事件
        curServerGo.GetComponent<UIButton>().isEnabled = false;
        //设置颜色为白色,灰色不好看,但是没效果
        curServerGo.GetComponent<UISprite>().color = new Color(240f, 240f, 240f,250f);
    }

    //4.点击服务器界面的"进入游戏"调用此方法
    public void OnEnterGameClick()
    {
        //1.隐藏服务器选择界面,
        tweenServer.gameObject.SetActive(false);
        tweenServer.PlayReverse();
        //2.发送请求到服务器,获取当前账号的所有角色
        roleController.GetRoles();
        //显示角色选择界面
        characterTween.gameObject.SetActive(true);
        characterTween.PlayForward();
    }

    //5.点击角色选择界面的"开始游戏"调用该方法
    public void OnGameStartClick()
    {
        //1.显示场景加载进度条
        LoadSceneTween.PlayForward();
        //2.切换到城镇场景
        AsyncOperation operation = Application.LoadLevelAsync(1);
        LoadSceneProgressBar._instance.Show(operation);
    }

    //在Unity客户端展示角色信息
    public void DisplayRoles(List<Role> roleList)
    {
        GameObject go = null;
        foreach (Role role in roleList)
        {
            if(role.CharacterId == 1)//暂时写死
            {
                go = NGUITools.AddChild(this.characterGrid.gameObject,
                this.characterBoyGo);
            }
            else if(role.CharacterId == 2)
            {
                go = NGUITools.AddChild(this.characterGrid.gameObject,
               this.characterGrilGo);
            }
            //给go中的RoleProperty赋值
            RoleProperty rp = go.GetComponent<RoleProperty>();
            rp.Id = role.ID;
            rp.Name = role.Name;
            rp.IsMan = role.IsMan;
            rp.Level = role.Level;
            rp.Exp = role.Exp;
            rp.Diamond = role.Diamond;
            rp.Coin = role.Coin;
            rp.Toughen = role.Toughen;
            rp.CharacterId = role.CharacterId;

            UILabel nameLabel = go.transform.Find("name-label").GetComponent<UILabel>();
            UILabel LVLabel = go.transform.Find("LV-label").GetComponent<UILabel>();
            nameLabel.text = role.Name;
            LVLabel.text = "LV:"+ role.Level.ToString();
            //如果不加这个代码,Grid中的元素会叠在一起不排序.
            StartMenuController.Instance.characterGrid.AddChild(go.transform);
        }
    } 
}
