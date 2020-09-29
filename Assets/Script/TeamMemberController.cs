using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using System;
using XueCommon;

public class TeamMemberController : MonoBehaviour {
    public int masterid;
    public int roleid;
    public string roleName;
    
    public UILabel roleNameLabel;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    void OnDestroy()
    {
        
    }
    //点击队伍panel调用该方法
    void OnClick()
    {
        //判断点击的是否自己角色的队伍panel
        if (PhotonEngine.Instance.role.ID != roleid)
        {
            //暂不处理
            return;
        }
        float heigh = this.gameObject.GetComponent<UISprite>().height;
        TeamUIController.Instance.ExitTeamClickHandle(this.transform, heigh);
    }
}
