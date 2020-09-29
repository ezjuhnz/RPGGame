using UnityEngine;
using System.Collections;

public class TeamUIController : MonoBehaviour {
    private static TeamUIController _instance;
    
    private GameObject exitPanelGo;
    private UIButton exitTeamBtn;
    private TweenScale exitPanelTween;
    private float exitTeamPanelWidth;
    public Transform uiRootTransform;

    public static TeamUIController Instance
    {
        get { return _instance; }
    }
    // Use this for initialization
    void Start () {
        exitPanelGo = GameObject.Find("UI Root/TeamExitPanel").gameObject;
        exitPanelTween = exitPanelGo.GetComponent<TweenScale>();
        exitTeamBtn = exitPanelGo.transform.Find("team-exit").GetComponent<UIButton>();
        exitTeamPanelWidth = exitPanelGo.GetComponent<UISprite>().width; //120
        _instance = this;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ExitTeamClickHandle(Transform teamTransform,float teamPanelHeigh)
    {
        float teamPanelWidth = teamTransform.GetComponent<UISprite>().width;//150
        float widthOffset = (teamPanelWidth - exitTeamPanelWidth) / 2;
        Vector3 teamPanelPosition = teamTransform.position;
        Vector3 localSpace = uiRootTransform.InverseTransformPoint(teamPanelPosition);
        //Debug.Log("localSpace=" + localSpace);
        //加上本地偏移量得到新的本地坐标
        localSpace = new Vector3(localSpace.x + widthOffset, localSpace.y - teamPanelHeigh, 0);
        
        exitPanelGo.transform.localPosition = localSpace;
        //显示退出队伍选项
        ShowExitTeamPanel();
        //给退出按钮绑定事件
        EventDelegate.Set(exitTeamBtn.onClick, delegate () { OnExitTeam(TeamInviteController.Instance.globalMasterID); });
    }

    void OnExitTeam(int masterid)
    {
        Debug.Log("OnExitTeam masterid=" + masterid);
        //向服务器发起退出队伍请求
        TeamInviteController.Instance.OnExitTeam(masterid);
        //隐藏退出队伍弹出框
        HideExitTeamPanel();
    }

    public void ShowExitTeamPanel()
    {
        exitPanelTween.PlayForward();
    }

    public void HideExitTeamPanel()
    {
        exitPanelTween.PlayReverse();
    }
}
