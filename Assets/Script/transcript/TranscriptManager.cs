using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TranscriptManager : MonoBehaviour {
    //为了避免在移动角色时,无法实时得到角色的位置,我们采用public
    public GameObject playerGo;
   
    private static TranscriptManager _instance;
    //public is for test,private is better
    public List<GameObject> enermyList = new List<GameObject>();

    public TweenPosition gameOverPanelTween;
    public TweenScale successTipTween;
    public TweenScale failedTipTween;
    public static TranscriptManager Instance
    {
        get { return _instance; }
    }
    void Awake()
    {
        _instance = this;
    }
	// Use this for initialization
	void Start () {
        GameObject[] goArr = GameObject.FindGameObjectsWithTag("TranscriptTeam");
        int len = goArr.Length;
        for(int i = 0; i < len-2; i++)
        {
            Destroy(goArr[i]);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void AddEnermy(GameObject enermyGo)
    {
        enermyList.Add(enermyGo);
    }

    public void RemoveEnermy(GameObject enermyGo)
    {
        enermyList.Remove(enermyGo);
    }

    public List<GameObject> GetEnermyList()
    {
        return enermyList;
    }

    //点击弹出窗口中的"返回城镇"调用该方法
    public void BackToHome()
    {
        //如果是组队,只有队长才有权限返回城镇
        if (TranscriptTeamController.Instance.isTeam) //组队
        {
            if (TranscriptTeamController.Instance.globalMasterID == PhotonEngine.Instance.role.ID)
            {
                Application.LoadLevelAsync(1);//加载场景2-城镇场景
                //通知队友客户端加载场景2
                BattleController.Instance.BackToHomeRequest();
            }
        }
        else //单人
        {
            Debug.Log("单人返回城镇==");
            Application.LoadLevelAsync(1);//加载场景2-城镇场景
        }
    }

    public void FightAgain()
    {
        //如果是组队,只有队长才有权限返回城镇
        if (TranscriptTeamController.Instance.isTeam) //组队
        {
            if (TranscriptTeamController.Instance.globalMasterID == PhotonEngine.Instance.role.ID)
            {
                Application.LoadLevelAsync(2);//加载场景
                //通知队友客户端加载场景3
                BattleController.Instance.FightAgainRequest();
            }
        }
        else //单人
        {
            Debug.Log("单人再次挑战==");
            Application.LoadLevelAsync(2);//加载场景2-城镇场景
        }
    }

    public void ShowGameOverPanel()
    {
        gameOverPanelTween.PlayForward();
    }

    public void HideGameOverPanel()
    {
        gameOverPanelTween.PlayReverse();
    }

    //显示通关成功提示
    public void ShowSuccessTip()
    {
        successTipTween.PlayForward();
        Invoke("HideSuccessTip", 7);
    }

    //隐藏通关成功提示
    public void HideSuccessTip()
    {
        successTipTween.PlayReverse();
    }

    //显示通关失败提示
    public void ShowFailedTip()
    {
        failedTipTween.PlayForward();
        Invoke("HideFailedTip",7);
    }
    //隐藏通关成功提示
    public void HideFailedTip()
    {
        failedTipTween.PlayReverse();
    }

    //游戏通关或失败
    public void GameCompleteHandle()
    {
        ShowSuccessTip();
        ShowGameOverPanel();
    }
    public void GameFailedHandle()
    {
        ShowFailedTip();
        ShowGameOverPanel();
    }
}
