using UnityEngine;
using System.Collections;
using XueCommon.Model;
using System.Collections.Generic;
using XueCommon;
using LitJson;
using ExitGames.Client.Photon;
using System;

public class SkillController : ControllerBase {
    private static SkillController _instance;
    public static SkillController Instance
    {
        get { return _instance; }
    }
    public PosType skillType;
    private GameObject[] playerGoArr;
    public PlayerAnimation playerAnimation;//pubilc for test
    public Animator curPlayerAnim = null;//public for test
    private UISprite maskSprite;

    //技能冷却时间
    public float skillCoolTime = 4;//4秒冷却
    private float skillTimer = 0;

    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.SkillItem;
        }
    }
    void Awake()
    {
        _instance = this;
    }
    public override void Start () {
        base.Start();
        skillTimer = skillCoolTime;
        if(transform.FindChild("Mask"))
        {
            maskSprite = transform.FindChild("Mask").GetComponent<UISprite>();
        }
         //发送请求到服务器,获取Skill信息
         Role role = PhotonEngine.Instance.role;
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        string json = JsonMapper.ToJson(role);
        parameters.Add((byte)ParameterCode.SubCode, SubCode.GetSkillInfo);
        parameters.Add((byte)ParameterCode.Role, json);
        PhotonEngine.Instance.SendRequest(OperationCode.SkillItem, parameters);
	}
	
	// Update is called once per frame
	void Update () {
        if(curPlayerAnim == null)
        {
            if(PlayerController.Instance.currentPlayer)
            {
                curPlayerAnim = PlayerController.Instance.currentPlayer.GetComponent<Animator>();
            }
        }
        if (skillTimer >= skillCoolTime)
        {
            skillTimer = skillCoolTime;
            EnableBtn();
        }
        skillTimer += Time.deltaTime;
        if(maskSprite != null)//普通攻击没有Mask
        {
            maskSprite.fillAmount = (skillCoolTime-skillTimer) / skillCoolTime;
        }
    }

    void OnPress(bool isPress)
    {
        if (curPlayerAnim == null)
        {
            return;
        }
        if (playerAnimation != null)
        {
            playerAnimation.OnAttackButtonClick(isPress, skillType);
        }
        //判断是否处于idle状态,只有在idle状态下才能释放技能

        if (curPlayerAnim.GetCurrentAnimatorStateInfo(1).IsName("Empty State"))
        {
            //Debug.Log("动画为Empty State");
            if (skillType != PosType.Basic)//普通攻击没有冷却时间
            {
                if (skillTimer < skillCoolTime)
                {
                    //技能未冷却
                    DiableBtn();
                }
                else
                {
                    if (isPress)
                    {
                        skillTimer = 0;//技能已经冷却了,重新置零
                    }
                }
            }
        }
    }
    void DiableBtn()
    {
        this.collider.enabled = false;
        this.GetComponent<UIButton>().SetState(UIButtonColor.State.Normal, true);
    }
    void EnableBtn()
    {
        this.collider.enabled = true;
    }
    public override void OnOperationResponse(OperationResponse response)
    {
        object o = null;
        SubCode subCode;
        response.Parameters.TryGetValue((byte)ParameterCode.SubCode, out o);
        subCode = (SubCode)o;
        switch(subCode)
        {
            case SubCode.GetSkillInfo:
                //获取技能信息
                response.Parameters.TryGetValue((byte)ParameterCode.RoleSkillList, out o);
                List<RoleSkill> skillList = null;
                skillList = JsonMapper.ToObject<List<RoleSkill>>(o.ToString());
                ShowSkillList(skillList);
                break;
            default: break;
        } 
    }

    public void OnPlayerInitialized()
    {
        playerGoArr = GameObject.FindGameObjectsWithTag("Fighter");
        foreach (GameObject playerGo in playerGoArr)
        {
            if (playerGo.GetComponent<PlayerTransMove>().roleid == PhotonEngine.Instance.role.ID)
            {
                playerAnimation = playerGo.GetComponent<PlayerAnimation>();
            }
        }
    }
    public void ShowSkillList(List<RoleSkill> skillList)
    {
        //Debug.Log("ShowSkillList");
    }
}
