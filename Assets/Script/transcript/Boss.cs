using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Boss : MonoBehaviour {

    private Transform player;
    public float viewAngle = 50;//可视角度
    public float attackDistance = 3;
    public float moveSpeed = 2;
    public float rotationSpeed = 1;

    private int attackIndex = 1;//攻击方式
    public float attackInterval = 1;//攻击间隔
    private float timer = 0; //不攻击时,开始计时
    public bool attacking; //是否正在攻击

    private GameObject attack01EffectGo;//attack01攻击特效
    private GameObject attack02EffectGo;//attack02攻击特效
    private Transform attack03Pos;      //attack03特效的位置
    public GameObject attack03Effect;   //attack03特效
    public int[] damageArray;
    public int damage = 0;
    public bool isSyncBoss = false;
     

    public string GUID = "";

    //Boss属性
    public bool isDead = false;
    public int hp = 2000;
    private int originHp;
    public string monsterName = "";

    //伤害数字
    public GameObject hudTextGo;
    public HUDText hudText;
    private GameObject damagePointGo;
    private GameObject bloodPointGo;

    public GameObject bloodEffect;

    public float downSpeed = 1;
    private float downDistance = 0;

    private float passTimer = 0;
    //后面添加的属性
    //处于攻击范围内的玩家
    private List<GameObject> targetGoList = new List<GameObject>();
    public GameObject targetToLockGo; //不在攻击范围的玩家
    public GameObject targetOutOfViewGo; //在攻击范围但不在可视角度的玩家
    public GameObject targetGo;//在攻击范围且在可视角度的玩家

    private float distance = 0;     //怪物与玩家之间的距离
    public float attackRate = 1.5f;//攻击频率,1.5s/次
    private List<GameObject> playerGoList = new List<GameObject>();
    //不在攻击范围,但是离怪物最近的玩家的距离
    private float minDistanceOutOfAttack; //public is for test
    //在攻击范围内,且与怪物最近的距离
    private float minDistanceInAttack;
    //切换跟踪目标的时间
    private float traceTargetTime = 3;
    private float switchTraceTimer = 0;
    //切换攻击目标时间
    //private float attackTargetTime = 3;
    private float switchAttackTimer = 0;
    private float attackTimer = 0; //攻击计时器
   
    //private float targetGoHp = 0; //攻击目标的血量
    //敌人的位置和旋转
    public Vector3 lastPosition;
    public Vector3 lastEulerAngles;
    //敌人是否正在移动
    private bool isMove = false;
   
    private float searchPlayerTimer;
    private float originPosY = 0;
   


    public float OriginHp
    {
        get { return originHp; }
    }

    void Start () {
        if (TranscriptTeamController.Instance.isTeam &&
            TranscriptTeamController.Instance.globalMasterID == PhotonEngine.Instance.role.ID)
        {
            isSyncBoss = true;
        }
        lastPosition = transform.position;
        lastEulerAngles = transform.eulerAngles;
        minDistanceOutOfAttack = 100;
        minDistanceInAttack = attackDistance;
        attackTimer = attackRate;
        //switchAttackTimer = attackTargetTime;//满足第一次计时切换目标
        switchTraceTimer = traceTargetTime;
        playerGoList = PlayerController.Instance.playerGoList;

        TranscriptManager.Instance.AddEnermy(this.gameObject);
        
        bloodPointGo = transform.Find("BloodPoint").gameObject;
        originHp = hp;
        attack01EffectGo = transform.Find("attack01").gameObject;
        attack02EffectGo = transform.Find("attack02").gameObject;
        attack03Pos = transform.Find("attack03Pos").transform;
        //player = TranscriptManager.Instance.playerGo.transform;

        damagePointGo = transform.Find("DamagePoint").gameObject;
        hudTextGo = HpNumberManager.Instance.GetHudText(damagePointGo);
        hudText = hudTextGo.GetComponent<HUDText>();
    }
    void LateUpdate()
    {
        if (isSyncBoss)
        {
            SyncBossPosAndRotation();
            SyncBossMoveAnimation();
        }
    }

    // Update is called once per frame
    void Update ()
    {
        playerGoList = PlayerController.Instance.playerGoList;
        if (isDead)//Boss死亡
        {
            return;
        }
        
        if (animation.IsPlaying("takedamage"))//怪物被攻击,攻击被打断
        {
            attacking = false;
        }
        attackTimer += Time.deltaTime;
        switchTraceTimer += Time.deltaTime;
        switchAttackTimer += Time.deltaTime;

        //判断是组队还是单人
        if (TeamInviteController.Instance.isTeam) //1.组队
        {
            if (!animation.isPlaying)
            {
                animation.CrossFade("idle");
            }
            if (!isSyncBoss) return;
            
            //1.遍历玩家列表,判断是否有在攻击范围内的
            foreach (GameObject go in playerGoList)
            {
                //玩家是否死亡?
                if (go.GetComponent<PlayerAttack>().hp <= 0)
                {
                    PlayerController.Instance.playerGoList.Remove(go);
                    if (targetToLockGo == go)
                    {
                        targetToLockGo = null;
                        //如果追踪目标死亡,那么可以立即切换追踪目标,不用等待
                        switchTraceTimer = traceTargetTime;
                    }
                    else if (targetGo == go)
                    {
                        targetGo = null;
                        //如果攻击目标死亡,那么可以立即切换攻击目标.不用等待
                        //switchAttackTimer = attackTargetTime;
                    }
                    else if(targetOutOfViewGo == go)
                    {
                        targetOutOfViewGo = null;
                        //switchTraceTimer = traceTargetTime;//与targetToLockGo共用计时器
                    }
                    break; //避免遍历已经改动的list
                }
                else //玩家未死亡
                {
                    distance = CalDistance(go);
                    if (distance < attackDistance)//1.在攻击范围内
                    {
                        //是否在可视角度
                        float angle = Vector3.Angle(go.transform.position - transform.position, transform.forward);
                        if (angle <= viewAngle / 2) //在可视角度
                        {
                            targetGoList.Add(go);
                            //加上切换攻击目标时间限制,避免频繁切换攻击目标
                            if (targetGo == null
                                /*&& switchAttackTimer >= attackTargetTime*/)
                            {
                                targetGo = go;
                            }
                            if(go == targetOutOfViewGo)
                            {
                                targetOutOfViewGo = null;
                            }
                            else if(go == targetToLockGo)
                            {
                                targetToLockGo = null;
                            }
                        }
                        else //2.不在可视角度
                        {
                            if (targetOutOfViewGo == null
                                && switchTraceTimer >= traceTargetTime)
                            {
                                targetOutOfViewGo = go;
                            }
                            if(go == targetGo)
                            {
                                targetGo = null;
                            }
                            else if(go == targetToLockGo)
                            {
                                targetToLockGo = null;
                            }
                        }
                    }
                    else //2.不在攻击范围
                    {
                        if ((targetToLockGo == null || distance < minDistanceOutOfAttack)
                            && switchTraceTimer >= traceTargetTime)
                        {
                            minDistanceOutOfAttack = distance;
                            targetToLockGo = go;
                        }
                        if (go == targetGo)
                        {
                            targetGo = null;
                        }
                        else if(go == targetOutOfViewGo)
                        {
                            targetOutOfViewGo = null;
                        }
                    }
                }
            } /*************End 遍历玩家***************/

            //如果切换时间已到,重新设置为0
            //switchAttackTimer = (switchAttackTimer >= attackTargetTime) ? 0 : switchAttackTimer;
            switchTraceTimer = (switchTraceTimer >= traceTargetTime) ? 0 : switchTraceTimer;

            //是否有玩家在攻击范围内
            if (targetGo == null)
            {
                if (!animation.isPlaying)
                {
                    animation.CrossFade("idle");
                }
                //是否有在攻击范围但不在可视角度的玩家
                if(targetOutOfViewGo != null)
                {
                    TurnToPlayer(targetOutOfViewGo);//转向玩家
                }
                else
                {
                    if(targetToLockGo != null)
                    {
                        MoveToPlayer(targetToLockGo);
                    }
                }             
            }
            else  //有目标在攻击范围内,怪物停止移动
            {
                isMove = false;
                targetToLockGo = null;//如果有攻击目标就不需要追踪目标
                //Boss是否正在攻击玩家
                if (attacking == false)
                {
                    animation.CrossFade("idle");
                    timer += Time.deltaTime;
                    if (timer > attackInterval)
                    {
                        timer = 0;
                        //攻击玩家
                        Attack();
                    }
                }
            }
        }
        else //************************ 2.单人 *****************************
        {
            distance = CalDistance(playerGoList[0]);
            if(distance <= attackDistance) //1.在攻击范围
            {
                float angle = Vector3.Angle(playerGoList[0].transform.position - transform.position, transform.forward);
                if (angle <= viewAngle / 2) //1.1.目标在可视角度,攻击
                {
                    targetGo = playerGoList[0];
                    if (attacking == false)
                    {
                        animation.CrossFade("idle");
                        timer += Time.deltaTime;
                        if (timer > attackInterval)
                        {
                            timer = 0;
                            //攻击玩家
                            Attack();
                        }
                    }
                }
                else //1.2不在可视角度,转向目标
                {
                    TurnToPlayer(playerGoList[0]);
                }
            }
            else //2.不在攻击范围,跟踪目标
            {
                MoveToPlayer(playerGoList[0]);
            }
        }
	}

    public void Attack()
    {
        int hp = targetGo.GetComponent<PlayerAttack>().hp;
        if (hp <= 0) return;
        attacking = true;
        if (attackIndex == 4)
        {
            attackIndex = 1;
        }
        animation.CrossFade("attack0" + attackIndex);
       
        BossAnimationModel model = new BossAnimationModel()
        {
            attack01 = attackIndex == 1 ? true : false,
            attack02 = attackIndex == 2 ? true : false,
            attack03 = attackIndex == 3 ? true : false,
            damage = damageArray[attackIndex - 1] //对玩家造成的伤害
        };
        if(TeamInviteController.Instance.isTeam)
        {
            EnermyController.Instance.SyncBossAnimationRequest(this.GUID, model);
        }
        //targetGo.SendMessage("GetHurt", damageArray[attackIndex-1] + "," + false, SendMessageOptions.DontRequireReceiver);
        attackIndex++;
    }

    //回到非攻击状态
    public void BackToIdle()
    {
        attacking = false;
    }

    public void ShowAttack01Effect()
    {
        attack01EffectGo.SetActive(true);//播放特效
        if (targetGo == null) return;
        if (targetGo.GetComponent<PlayerAttack>().isDead)
        {
            return;
        }
        //计算Boss与Player的距离
        float distance = Vector3.Distance(transform.position, targetGo.transform.position);
        if(distance < attackDistance)
        {
            //false代表不播放受伤动画,此处只是为了参数匹配,并不作参考
            //master客户端根据随机数决定是否播放受伤动画,并不使用该参数
            targetGo.SendMessage("GetHurt", damageArray[0] + "," + false);
        }
    }

    public void ShowAttack02Effect()
    {
        attack02EffectGo.SetActive(true);//播放特效
        if (targetGo == null) return;
        if(targetGo.GetComponent<PlayerAttack>().isDead)
        {
            return;
        }
        //计算Boss与Player的距离
        float distance = Vector3.Distance(transform.position, targetGo.transform.position);
        if (distance < attackDistance)
        {
            targetGo.SendMessage("GetHurt", damageArray[1] + "," + false);
        }
    }

    public void ShowAttack03Effect()
    {
        GameObject go = 
            GameObject.Instantiate(attack03Effect, attack03Pos.position, transform.rotation) as GameObject;
        BossBullet bb = go.GetComponent<BossBullet>();
        bb.Damage = damageArray[2];
    }

    //1.伤害,2.击退距离,3.击飞高度
    public void GetHurt(string args)
    {
        //Boss与普通enermy不同,它是有Rigibody组件的,会受重力影响
        GameObject killerGo = null;
        //Combo.Instance.ComboPlus();
        string[] strArr = args.Split(',');
        int hurtNum = int.Parse(strArr[0]);
        float backDistance = float.Parse(strArr[1]);
        float heigh = float.Parse(strArr[2]);
        int killerid = int.Parse(strArr[3]);
        
        hp -= hurtNum; //血量减少
        //显示伤害数字
        hudText.Add("-" + hurtNum, Color.red, 0.05f);
        PlayerController.Instance.transPlayerDict.TryGetValue(killerid, out killerGo);
        //击退距离和击飞高度,InverseTransformDirection(Vector3 direc)将direc从世界坐标转换成本地坐标
        iTween.MoveBy(this.gameObject,
            transform.InverseTransformDirection(killerGo.transform.forward * backDistance + heigh * Vector3.up),
            0.3f);
        //播放受伤声音
        SoundManager._instance.Play("Hurt");
        //播放受伤动画
        animation.Play("takedamage");
        //播放出血特效
        GameObject.Instantiate(bloodEffect, bloodPointGo.transform.position, Quaternion.identity);
        
        if (TeamInviteController.Instance.isTeam) //1.组队
        {
            isSyncBoss = true;
            if (hp <= 0)
            {
                Dead(hurtNum);
            }
            Combo.Instance.ComboPlus();//显示连击数
            HpBarController.Instance.ShowBossHpBar(this.gameObject);//显示敌人血条

            EnermyController.Instance.SyncEnermyHpRequest(GUID, hp, hurtNum,killerid);
        }
        else //单人
        {
            if (hp <= 0)
            {
                Dead(hurtNum);
            }
            Combo.Instance.ComboPlus();//显示连击数
            HpBarController.Instance.ShowBossHpBar(this.gameObject);//显示敌人血条
        }
    }

    public void Dead(int damage)
    {
        isDead = true;
        animation.Play("die");
        //同步死亡动画到其它客户端
        if (isSyncBoss)
        {
            BossAnimationModel model = new BossAnimationModel()
            {
                die = true
            };
            EnermyController.Instance.SyncBossAnimationRequest(this.GUID, model);
        }
        //从TranscriptManager中移除该敌人
        TranscriptManager.Instance.RemoveEnermy(this.gameObject);
       
        //Boss死后保留尸体
        this.gameObject.rigidbody.isKinematic = true;
        Destroy(hudTextGo,1.0f);
        //通关成功
        TranscriptManager.Instance.GameCompleteHandle();
    }

    //计算玩家和怪物之间的距离
    float CalDistance(GameObject playerGo)
    {
        Vector3 playerPos = playerGo.transform.position;
        playerPos.y = transform.position.y;
        distance = Vector3.Distance(playerPos, transform.position);

        return distance;
    }

    //追踪玩家
    public void MoveToPlayer(GameObject targetToLock)
    {
        attacking = false;
        if (targetToLock == null)
        {
            isMove = false;
            return;
        }
        if (CalDistance(targetToLock) < attackDistance)
        {
            return;
        }
        animation.CrossFade("walk",0.3f);//播放行走动画
        Vector3 playerPos = targetToLock.transform.position;
        playerPos.y = transform.position.y;
        transform.LookAt(playerPos);//朝向玩家
        rigidbody.MovePosition(transform.position + transform.forward * moveSpeed * Time.deltaTime);
    }

    public void TurnToPlayer(GameObject go)
    {
        Vector3 relativePos = go.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(relativePos);
        //播放行走动画
        animation.CrossFade("walk", 0.3f);
        //攻击动画没有播完就播放walk,导致BackToIdle未被调用,手动设置为false
        attacking = false;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 1 * Time.deltaTime);
    }

    //
    void SyncBossPosAndRotation()
    {
        if (isDead) return;
        Vector3 position = transform.position;
        Vector3 eulerAngles = transform.eulerAngles;
        if (lastPosition.x != position.x || lastPosition.y != position.y || lastPosition.z != position.z ||
            lastEulerAngles.x != eulerAngles.x ||
            lastEulerAngles.y != eulerAngles.y ||
            lastEulerAngles.z != eulerAngles.z)
        {
            EnermyController.Instance.SyncEnermyPosAndRotationRequest(this.GUID, position, eulerAngles);
            lastPosition = position;
            lastEulerAngles = eulerAngles;
        }
    }

    //
    void SyncBossMoveAnimation()
    {
        if (isDead) return;
        if (isMove != animation.IsPlaying("walk"))
        {
            EnermyMoveAnimationModel model = new EnermyMoveAnimationModel()
            {
                IsMove = !isMove,
            };
            model.SetTime(DateTime.Now);
            //发送请求到服务器
            EnermyController.Instance.SyncEnermyMoveAnimationRequest(this.GUID, model);
        }
        isMove = animation.IsPlaying("walk");
    }
   
}
