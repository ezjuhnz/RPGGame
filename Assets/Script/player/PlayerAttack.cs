using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/*该脚本绑定在Player中,其下包含attack1,attack2,attack3特效;
每个特效中都绑定了PlayerEffect脚本,用于控制特效的播放
*/
public class PlayerAttack : MonoBehaviour {
    private static PlayerAttack _instance;
    Dictionary<string, PlayerEffect> peDict = new Dictionary<string, PlayerEffect>();
    private PlayerEffect[] peArray;
    public PlayerEffect[] effectArray;

    private Animator anim;
    public int hp = 100;
    private float originHp;
    public bool isDead = false;

    private GameObject damagePointGo;
    private GameObject hudTextGo;
    private HUDText hudText;

    private GameObject playerHpBarGo;
    private UISlider hpSlider;
    private UILabel hpLabel;

    public UISprite sorryTipSprite;
    private int killerId = 0;

    public static PlayerAttack Instance
    {
        get { return _instance; }
    }
	// Use this for initialization
	void Start () {
        _instance = this;
        killerId = this.GetComponent<PlayerTransMove>().roleid;
        originHp = hp;
        Vector3 hpBarPos = PlayerController.Instance.hpBarPos;
        GameObject uiRootGo = GameObject.FindGameObjectWithTag("UIRoot");
        
        //通过NGUI 实例化血条
        if(this.GetComponent<PlayerTransMove>().roleid == PhotonEngine.Instance.role.ID)
        {
            playerHpBarGo = NGUITools.AddChild(PlayerController.Instance.uiRoot.gameObject,
                Resources.Load("PlayerHpBg") as GameObject);
            //更改血条位置
            playerHpBarGo.transform.position = hpBarPos;
            hpSlider = playerHpBarGo.GetComponentInChildren<UISlider>();
            hpLabel = playerHpBarGo.GetComponentInChildren<UILabel>();
            hpLabel.text = hp + "/" + originHp; //null reference
        }
       
        anim = GetComponent<Animator>();
        peArray = this.GetComponentsInChildren<PlayerEffect>();
        foreach(PlayerEffect pe in peArray)
        {
            peDict.Add(pe.gameObject.name, pe);
        }
        //另一种类型的特效资源
        foreach(PlayerEffect effect in effectArray)
        {
            peDict.Add(effect.gameObject.name, effect);
        }
        damagePointGo = transform.Find("DamagePoint").gameObject;
        hudTextGo = HpNumberManager.Instance.GetHudText(damagePointGo);
        hudText = hudTextGo.GetComponent<HUDText>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    //skill3,BloodStrike1,skill3_1,forward,1,1,0,2
    //1.技能类型,2.特效名,3.声音名,4.攻击类型(前方或四周)
    //5.前进距离,6.击退距离,7.击飞高度,8.攻击距离
    public void Attack(string args)
    {
        GameObject killerGo = null; //攻击怪物的玩家
        string[] strArr = args.Split(',');
        string skillName = strArr[0];
        string effectName = strArr[1];//特效名
        string soundName = strArr[2]; //音效名
        string attackType = strArr[3];
        float moveForward = float.Parse(strArr[4]);
        float backDistance = float.Parse(strArr[5]);
        float heigh = float.Parse(strArr[6]);
        float attackDistance = float.Parse(strArr[7]);
       
        ShowPlayerEffect(effectName);//播放特效
        SoundManager._instance.Play(soundName);//播放声音
        List<GameObject> enermyList = null;
        //获取伤害,击退距离,击飞高度
        int damage = 30; //暂时写死
        if(skillName == "normal")
        {
            damage = UnityEngine.Random.Range(30,40);
        }
        else if(skillName == "skill1")
        {
            damage = UnityEngine.Random.Range(60, 70);
        }
        else if(skillName == "skill2")
        {
            damage = UnityEngine.Random.Range(80, 100);
        }
        else if (skillName == "skill3")
        {
            damage = UnityEngine.Random.Range(110, 150);
        }
        PlayerController.Instance.transPlayerDict.TryGetValue(killerId, out killerGo);
        
        if (killerGo != null)
        {
            //控制前冲效果
            if (moveForward > 0.1f)
            {
                iTween.MoveBy(killerGo, Vector3.forward * moveForward, 0.3f);
            }
            
        }
        //1.获取攻击范围内的敌人
        if (attackType == "forward")
        {
            enermyList = GetEnermyInRange(AttackRange.Forward, attackDistance);
        }
        else
        {
            enermyList = GetEnermyInRange(AttackRange.Around, attackDistance);
        }
      
        foreach (GameObject go in enermyList)
        {
            if(killerId == PhotonEngine.Instance.role.ID)
            {
                go.SendMessage("GetHurt", damage + "," + backDistance + "," + heigh + "," + killerId);
            }
            
        }
        
    }

    public void ShowPlayerEffect(string effectName)
    {
        PlayerEffect pe = null;
        peDict.TryGetValue(effectName, out pe);
        if(pe != null)
        {
            pe.Show();
        }
    }
    //args:1.特效名 2.攻击范围,3.攻击距离,4.击退距离,5.击飞高度
    //普通攻击,最后一击在敌人脚下生成"恶魔之手"
    public void ShowDevilHandMobile(string args)
    {
        List<GameObject> enermyInRangeList = null;
        string effectName = "DevilHandMobile";//直接写了,懒得从args中获取
        PlayerEffect pe = null;
        peDict.TryGetValue(effectName, out pe);
        string[] strArr = args.Split(',');
        float attackDistance = 0;
        float.TryParse(strArr[2], out attackDistance);

        if(strArr[1].Equals("forward"))
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Forward, attackDistance);
        }
        else
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Around, attackDistance);
        }
        float damage = 150;//暂时写死
        float backDistance = float.Parse(strArr[3]);
        float heigh = float.Parse(strArr[4]);
       
        foreach (GameObject go in enermyInRangeList)
        {
            //为了保证恶魔之手能从地里升出来,使用射线检测
            RaycastHit hit;
            bool isHit = Physics.Raycast(go.transform.position + Vector3.up, Vector3.down, out hit, 10f, LayerMask.GetMask("Groud"));
            if(isHit)
            {
                GameObject.Instantiate(pe, hit.point, Quaternion.identity);
            }
            if(TranscriptTeamController.Instance.isTeam && 
                this.GetComponent<PlayerTransMove>().roleid != PhotonEngine.Instance.role.ID)
            {
                continue; //只有负责同步的客户端才能打出伤害
            }
            go.SendMessage("GetHurt", damage + "," + backDistance + "," + heigh + "," + killerId);
        }
        
    }
    /*FirePhoenixMobile,bird,around,5,0,0
    1.特效名,2.声音名,3,攻击类型,4.攻击距离,5.击退距离,6.击飞高度
    */
    //技能1(旋转),最后触发凤凰跟踪怪物攻击
    public void ShowPhoenixMobile(string args)
    {
        List<GameObject> enermyInRangeList = null;
        string effectName = "FirePhoenixMobile";//直接写了,懒得从args中获取
        
        PlayerEffect pe = null;
        peDict.TryGetValue(effectName, out pe);
        string[] strArr = args.Split(',');
        string audioName = strArr[1];
        //播放音效
        SoundManager._instance.Play(audioName);
        float attackDistance = 0;
        float.TryParse(strArr[3], out attackDistance);

        if (strArr[1].Equals("forward"))
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Forward, attackDistance);
        }
        else
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Around, attackDistance);
        }
        float damage = 260; //暂时写死
        float backDistance = float.Parse(strArr[4]);
        float heigh = float.Parse(strArr[5]);
        
        foreach (GameObject go in enermyInRangeList)
        {
            GameObject phoenix = (GameObject.Instantiate(pe) as PlayerEffect).gameObject;
            phoenix.transform.position = transform.position + Vector3.up;//特效的位置
            if(phoenix != null)
            {
                phoenix.GetComponent<EffectSettings>().Target = go;
            }
            go.SendMessage("GetHurt", damage + "," + backDistance + "," + heigh + "," + killerId);
        }
    }

    //HolyFireStrike,skill3_3,forward,3,0,0
    //1.特效名, 2.声音名, 3.攻击类型, 4.攻击距离, 5.击退距离, 6.击飞高度
    //技能2(寒冰)最后一击,触发火柱特效
    public void ShowHolyFireStrike(string args)
    {
        List<GameObject> enermyInRangeList = null;
        string effectName = "HolyFireStrike";//直接写了,懒得从args中获取
        PlayerEffect pe = null;
        peDict.TryGetValue(effectName, out pe);
        string[] strArr = args.Split(',');
        string audioName = strArr[1];
        SoundManager._instance.Play(audioName);//播放声音
        float attackDistance = 0;
        float.TryParse(strArr[3], out attackDistance);

        if (strArr[2].Equals("forward"))
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Forward, attackDistance);
        }
        else
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Around, attackDistance);
        }
        float damage = 270; //暂时写死
        float backDistance = float.Parse(strArr[4]);
        float heigh = float.Parse(strArr[5]);
        
        foreach (GameObject go in enermyInRangeList)
        {
            RaycastHit hit;
            bool isHit = Physics.Raycast(go.transform.position + Vector3.up, Vector3.down, out hit, 10f, LayerMask.GetMask("Groud"));
            if (isHit)
            {
                GameObject.Instantiate(pe, hit.point, Quaternion.identity);
            }
            go.SendMessage("GetHurt", damage + "," + backDistance + "," + heigh + "," + killerId);
        }
    }

    //技能3(血溅)最后一击触发该特效
    //DustStorm,skill3_4,forward,3,0,0
    public void ShowDustStorm(string args)
    {
        List<GameObject> enermyInRangeList = null;
        string effectName = "DustStorm";//直接写了,懒得从args中获取
        PlayerEffect pe = null;
        peDict.TryGetValue(effectName, out pe);
        string[] strArr = args.Split(',');
        string audioName = strArr[1];
        SoundManager._instance.Play(audioName);
        float attackDistance = 0;
        float.TryParse(strArr[3], out attackDistance);

        if (strArr[2].Equals("forward"))
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Forward, attackDistance);
        }
        else
        {
            enermyInRangeList = GetEnermyInRange(AttackRange.Around, attackDistance);
        }
        float damage = 340; //暂时写死
        float backDistance = float.Parse(strArr[4]);
        float heigh = float.Parse(strArr[5]);
        
        foreach (GameObject go in enermyInRangeList)
        {
            RaycastHit hit;
            bool isHit = Physics.Raycast(go.transform.position + Vector3.up, Vector3.down, out hit, 10f, LayerMask.GetMask("Groud"));
            if (isHit)
            {
                GameObject.Instantiate(pe, hit.point + Vector3.up, Quaternion.Euler(0, 0, 180));
            }
            go.SendMessage("GetHurt", damage + "," + backDistance + "," + heigh + "," + killerId);
            //GameObject.Instantiate(pe, go.transform.position+Vector3.up, Quaternion.Euler(0, 0, 180));
        }
    }

    public List<GameObject> GetEnermyInRange(AttackRange range,float attackDistance)
    {
        //1.获取在攻击范围内的敌人
        List<GameObject> enermyInRangeList = new List<GameObject>();
        List<GameObject> enermyList = TranscriptManager.Instance.GetEnermyList();
        foreach (GameObject go in enermyList)
        {
            //计算每个敌人与玩家的距离,判断是否在攻击范围
            Vector3 pos = transform.InverseTransformPoint(go.transform.position);
            if (pos.z > 0) //敌人在玩家前方
            {
                float distance = Vector3.Distance(Vector3.zero, pos);
                if (distance <= attackDistance)
                {
                    enermyInRangeList.Add(go);
                }
            }
        }
        return enermyInRangeList;
    }

    //怪物对玩家造成伤害
    public void GetHurt(string args)
    {
        string[] strArr = args.Split(',');
        int hurtNum = int.Parse(strArr[0]);

        bool hurtFlag = false;
        //血量减少
        hp -= hurtNum;
        if (hp <= 0)
        {
            Dead();
            hp = 0;
        }
        ShowHurtNum(hurtNum);//1.显示伤害数字
        if (this.GetComponent<PlayerTransMove>().roleid == PhotonEngine.Instance.role.ID)
        {
            //2.被攻击对象所属客户端才会显示受伤红屏效果
            BloodScreen.Instance.ShowBloodScreen();
            //3.修改血条的值
            UpdatePlayerHpBar(hp);
        }

        int rank = UnityEngine.Random.Range(0, 100);
        if (rank < hurtNum)
        {
            //播放受伤动画
            hurtFlag = true;
            anim.SetTrigger("GetHurt");
        }
        if (TeamInviteController.Instance.isTeam)
        {
            int targetId = this.GetComponent<PlayerTransMove>().roleid;
            //如果是组队,发送请求到服务器,同步玩家受伤动画
            PlayerAnimationModel model = new PlayerAnimationModel()
            {
                takeDamage = hurtFlag,
                hurtNum = hurtNum, //怪物对玩家造成的伤害
            };
            //同步玩家受伤动画
            BattleController.Instance.SyncPlayerAttackAnimationRequest(model, targetId);
            //同步玩家血条
            BattleController.Instance.SyncPlayerHpBarRequest(hp,hurtNum, targetId);
        }
    }
    public void ShowHurtNum(int hurtNum)
    {
        hudText.Add("-" + hurtNum, Color.red, 0.1f);//显示伤害数字
    }

    public void UpdatePlayerHpBar(int hp)
    {
        //修改血条的值
        hpSlider.value = hp / (float)originHp; //NullReferenceException
        hpLabel.text = hp + "/" + originHp;
    }

    public void Dead()
    {
        bool isGameOver = true;
        anim.SetTrigger("Die"); //播放死亡动画
        isDead = true;
        this.GetComponent<Rigidbody>().useGravity = false;
        this.GetComponent<Rigidbody>().collider.enabled = false;
        if(!TranscriptTeamController.Instance.isTeam)
        {
            //通关失败
            TranscriptManager.Instance.GameFailedHandle();
        }
        else
        {
            //获取队伍中所有队员的信息,如果所有队员死亡则通关失败
            ClientTeam ct = new ClientTeam();
            TranscriptTeamController.Instance.teamDict.TryGetValue(TranscriptTeamController.Instance.globalMasterID, out ct);
            if(ct != null)
            {
                GameObject go = null;
                for(int i = 0; i < ct.Size; i++)
                {
                    PlayerController.Instance.transPlayerDict.TryGetValue(ct.memberids[i], out go);
                    if (go != null && go.GetComponent<PlayerAttack>().hp > 0)
                    {
                        isGameOver = false;
                    }
                }

                if(isGameOver)
                {
                    //Debug.Log("全军覆灭==");
                    TranscriptManager.Instance.GameFailedHandle();
                }
                
            }
        } 
    }
}
