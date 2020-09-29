using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Enermy : MonoBehaviour {

    public  int hp = 1000;
    private int originHp = 0;
    public bool isDead = false;
    public float downSpeed = 1;//死亡后下落速度
    private float gravity = 2;
    private float downDistance; //下落的距离
    private float strikeHeigh = -1; //击飞高度
    private float currentHeigh = 0.3f; //当前高度
    private int times = 0;
    

    public GameObject targetGo;//攻击目标
    private float targetGoHp = 0; //攻击目标的血量
    private float distance = 0;     //怪物与玩家之间的距离
    

    public GameObject bloodEffect; //出血特效预制体
    public GameObject bloodPoint; //出血的位置

    //怪物预制体的属性
    public string monsterName = "";
    public int damage = 20; //怪物对玩家造成的伤害
    public float attackDistance = 1.8f;//怪物攻击距离
    public float moveSpeed = 5; //怪物移动速度
    public float attackRate = 1.5f;//攻击频率,1.5s/次
    public string GUID = "";       //怪物唯一标识
    private List<GameObject> playerGoList = new List<GameObject>();
    //处于攻击范围内的玩家
    private List<GameObject> targetGoList = new List<GameObject>();
    //不在攻击范围,但是离怪物最近的玩家的距离
    private float minDistanceOutOfAttack; //public is for test
    //在攻击范围内,且与怪物最近的距离
    private float minDistanceInAttack;
    //切换跟踪目标的时间
    private float traceTargetTime = 2;
    private float switchTraceTimer = 0;
    //切换攻击目标时间
    private float attackTargetTime = 3;
    private float switchAttackTimer = 0;
    private float attackTimer = 0; //攻击计时器
    public GameObject targetToLockGo;
    public bool isAgentControlByOther = true;

    //敌人的位置和旋转
    public Vector3 lastPosition;
    public Vector3 lastEulerAngles;
    //敌人是否正在移动
    private bool isMove = false;
   
    private float miniDistance = 0.1f; //最小的误差距离
    public int level = 1;
    public bool isSyncEnermy = false; //是否由该客户端同步敌人位置和动画
    private CharacterController characterController;
   
    public float OriginHp
    {
        get { return originHp; }
    }

    public HUDText hudText;
    public GameObject hudTextGo;
    void Start () {
        if(TranscriptTeamController.Instance.isTeam && 
            TranscriptTeamController.Instance.globalMasterID == PhotonEngine.Instance.role.ID)
        {
            isSyncEnermy = true;
        }
        if(!TranscriptTeamController.Instance.isTeam) //单人
        {
            isSyncEnermy = true;
        }
        lastPosition = transform.position;
        lastEulerAngles = transform.eulerAngles;
        minDistanceOutOfAttack = attackDistance;
        minDistanceInAttack = attackDistance;
        attackTimer = attackRate;
        switchAttackTimer = attackTargetTime;//满足第一次计时切换目标
        switchTraceTimer = traceTargetTime;
        characterController = this.GetComponent<CharacterController>();


        playerGoList = PlayerController.Instance.playerGoList;
        if (transform.Find("DamagePoint"))
        {
            hudTextGo = HpNumberManager.Instance.GetHudText(transform.Find("DamagePoint").gameObject);
            hudText = hudTextGo.GetComponent<HUDText>();
        }
        originHp = hp;
        bloodPoint = transform.Find("BloodPoint").gameObject;
        TranscriptManager.Instance.AddEnermy(this.gameObject);
	}
    void LateUpdate()
    {
        if (isSyncEnermy)
        {
            SyncEnermyPosAndRotation();
            SyncEnermyMoveAnimation();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        targetGoList.Clear();
        playerGoList = PlayerController.Instance.playerGoList;
        if (isDead) //如果怪物死了,就不计算和玩家的距离了
        {
            downDistance += downSpeed * Time.deltaTime;
            transform.Translate(-Vector3.up * downSpeed * Time.deltaTime);
            if (downDistance > 4)
            {
                Destroy(this.gameObject);
            }
            return;
        }

        attackTimer += Time.deltaTime;
        switchTraceTimer += Time.deltaTime;
        switchAttackTimer += Time.deltaTime;

        targetGoHp = (targetGo == null) ? 0 : targetGo.GetComponent<PlayerAttack>().hp;

        //*********************** 1.组队 ********************************
        if (TranscriptTeamController.Instance.isTeam) 
        {
            if (!isSyncEnermy)
            {
                return;
            }
            BackToGround();
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
                    if (targetGo == go)
                    {
                        targetGo = null;
                        //如果攻击目标死亡,那么可以立即切换攻击目标.不用等待
                        switchAttackTimer = attackTargetTime;
                    }
                    break; //避免遍历已经改动的list
                }
                else //玩家未死亡
                {
                    distance = CalDistance(go);

                    if (distance < attackDistance)//在攻击范围内
                    {
                        targetGoList.Add(go);
                        //加上切换攻击目标时间限制,避免频繁切换攻击目标
                        if (targetGo == null
                            && switchAttackTimer >= attackTargetTime)
                        {
                            targetGo = go;
                        }
                    }
                    else //不在攻击范围
                    {
                        if (targetToLockGo == null || 
                            (distance < minDistanceOutOfAttack && switchTraceTimer >= traceTargetTime))
                        {
                            targetToLockGo = go;
                            minDistanceOutOfAttack = distance;
                        }
                        if (targetGo == go)//如果是攻击目标不在攻击范围内
                        {
                            minDistanceOutOfAttack = distance;
                            targetGo = null;
                        }
                    }
                }
            } /*************End 遍历玩家***************/
            
            //如果切换时间已到,重新设置为0
            switchAttackTimer = (switchAttackTimer >= attackTargetTime) ? 0 : switchAttackTimer;
            switchTraceTimer = (switchTraceTimer >= traceTargetTime) ? 0 : switchTraceTimer;

            //是否有玩家在攻击范围内,没有,则移动到最近的玩家身边
            if (targetGo == null)
            {
                if(!animation.isPlaying)
                {
                    animation.Play("idle");
                }
                MoveToPlayer(targetToLockGo);//不需要判断 isSyncEnermy
            }
            else
            {
                //有目标在攻击范围内,怪物停止移动
                isMove = false;
                targetToLockGo = null;//如果有攻击目标就不需要追踪目标
                //有,怪物攻击时间是否冷却
                if (attackTimer >= attackRate)
                {
                    //怪物是否正在被攻击
                    if (!animation.IsPlaying("takedamage"))
                    {
                        Attack();
                    }
                    if (!animation.isPlaying)
                    {
                        animation.Play("idle");
                    }
                }
               
            }
        }
        else //************************2.单人**************************
        {
            BackToGround();
            if (playerGoList[0].GetComponent<PlayerAttack>().hp <= 0)//玩家死亡,return
            {
                animation.Play("idle");
                return;
            }
            else
            {
                distance = CalDistance(playerGoList[0]);
                //判断怪物与玩家的距离,如果在攻击范围,则攻击玩家
                if (distance <= attackDistance)
                {
                    targetGo = playerGoList[0];
                    targetToLockGo = null;

                    if (attackTimer >= attackRate)
                    {
                        //怪物正在被攻击时不能攻击玩家
                        if (!animation.IsPlaying("takedamage"))
                        {
                            Attack();
                        }
                        if (!animation.isPlaying)
                        {
                            animation.Play("idle");
                        }

                    }
                }
                else
                {
                    //跟踪玩家
                    targetToLockGo = playerGoList[0];
                    targetGo = null;
                    MoveToPlayer(targetToLockGo);
                }
            }  
        }
        attackTimer = attackTimer >= attackTargetTime ? 0 : attackTimer;
        switchTraceTimer = switchTraceTimer >=  traceTargetTime ? 0: switchTraceTimer;
        switchAttackTimer = switchAttackTimer >= attackTargetTime ? 0: switchAttackTimer;
    }
    //计算玩家和怪物之间的距离
    float CalDistance(GameObject playerGo)
    {
        Vector3 playerPos = playerGo.transform.position;
        playerPos.y = transform.position.y;
        distance = Vector3.Distance(playerPos, transform.position);
        //Debug.Log("CalDistance = " + distance);
        return distance;
    }

    //1.伤害,2.击退距离,3.击飞高度,Killer Id
    //note:每个客户端都会发送消息调用自己的GetHurt()方法
    public void GetHurt(string args)
    {
        string[] strArr = args.Split(',');
        int hurtNum = int.Parse(strArr[0]);
        float backDistance = float.Parse(strArr[1]);
        strikeHeigh = float.Parse(strArr[2]);
        RaycastHit hit;
        int groudLayerMask = 1 << 9; //只检测layerMask为9的collider
        Physics.Raycast(transform.position, -Vector3.up, out hit, groudLayerMask);

        currentHeigh = hit.distance;
        int killerid = int.Parse(strArr[3]);
        

        //播放受伤声音
        SoundManager._instance.Play("Hurt");
        //播放受伤动画
        animation.Play("takedamage");
        //播放出血特效
        GameObject.Instantiate(bloodEffect, bloodPoint.transform.position, Quaternion.identity);

        hp -= hurtNum; //血量减少
        //显示伤害数字
        hudText.Add("-" + hurtNum, Color.red, 0.05f);

        /*如果是组队,还需要
        **1.发送信息到队友客户端同步敌人的血量
        **2.告知队友客户端不要同步该怪物,由我来同步,设置当前客户端的isSyncEnermy标记为true
        参数.guid, 2.hp, 3.damage,4.killerid
        */
        if (TranscriptTeamController.Instance.isTeam)
        {
            isSyncEnermy = true;//谁攻击了怪物就由谁来更新怪物的位置,动画.其它客户端该标识设为false
            //怪物浮空处理
            if (strikeHeigh - currentHeigh > miniDistance || backDistance > 0)
            {
                EnermyLevitate(backDistance, strikeHeigh, killerid);
            }
            if (hp <= 0)
            {
                Dead(hurtNum);
            }
            Combo.Instance.ComboPlus();//显示连击数
            HpBarController.Instance.ShowHpBar(this.gameObject);//显示敌人血条
            EnermyController.Instance.SyncEnermyHpRequest(GUID, hp,hurtNum,killerid);

        }
        else //单人模式
        {
            if (strikeHeigh - currentHeigh > miniDistance || backDistance > 0)
            {
                EnermyLevitate(backDistance, strikeHeigh, killerid);
            }
            if (hp <= 0)
            {
                Dead(hurtNum);
            }
            Combo.Instance.ComboPlus();//显示连击数
            HpBarController.Instance.ShowHpBar(this.gameObject);//显示敌人血条
        }
    }

    //攻击玩家
    public void Attack()
    {
        if (targetGoHp <= 0) return;
        attackTimer = 0;
        //转向玩家
        if(isSyncEnermy)//可以不加判断
        {
            transform.LookAt(targetGo.transform.position);
            //暂时把所有怪物的攻击当做是非群体攻击--即每个怪物每次只攻击一个玩家
            animation.Play("attack01");//播放攻击动画
            EnermyAnimationModel model = new EnermyAnimationModel()
            {
                attack = true
            };
            //false代表不播放受伤动画,因为不是每次被攻击玩家都会播放受伤动画
            //master客户端根据随机数决定是否播放受伤动画,然后同步到其它客户端
            targetGo.SendMessage("GetHurt", damage + "," + false, SendMessageOptions.DontRequireReceiver);
            if (TranscriptTeamController.Instance.isTeam)
            {
                EnermyController.Instance.SyncEnermyAnimationRequest(this.GUID, model);
            }
            
        }
    }

    //追踪玩家
    public void MoveToPlayer(GameObject targetToLock)
    {
        if (currentHeigh > miniDistance) return; //浮空时不能移动
        if (targetToLock == null)
        {
            Debug.Log("there is no target to be trace");
            return;
        }
        if(CalDistance(targetToLock) <= attackDistance)
        {
            Attack();
            return;
        }
        animation.Play("walk");//播放行走动画
        Vector3 playerPos = targetToLock.transform.position;
        playerPos.y = transform.position.y;
        transform.LookAt(playerPos);//朝向玩家
        //设置怪物的目标
        if(characterController != null)
        {
            characterController.SimpleMove(transform.forward * moveSpeed);
        }
        else
        {
            Debug.LogError("CharacterController was not attached to " + this.gameObject.name);
        }
        
    }

    public void Dead(float damage)
    {
        Debug.Log("Enermy was die==");
        isDead = true;
        if (damage > originHp/5)
        {
            //单次伤害超过hp的1/5;则显示破碎效果死亡
            //播放破碎特效:死亡方式2
            transform.GetComponentInChildren<MeshExploder>().Explode();
            this.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
        else
        {
            //播放死亡动画:死亡方式1
            animation.Play("die");
        }
        //从TranscriptManager中移除该敌人
        TranscriptManager.Instance.RemoveEnermy(this.gameObject);
        //移除collider
        this.gameObject.transform.collider.enabled = false;
        Destroy(hudTextGo);
        //同步死亡动画到其它客户端
        if (isSyncEnermy)
        {
            EnermyAnimationModel model = new EnermyAnimationModel()
            {
                die = true
            };
            EnermyController.Instance.SyncEnermyAnimationRequest(this.GUID, model);
        }
    }

    void SyncEnermyPosAndRotation()
    {
        Vector3 position = transform.position;
        Vector3 eulerAngles = transform.eulerAngles;
        if(lastPosition.x != position.x || lastPosition.y != position.y || lastPosition.z != position.z ||
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
    void SyncEnermyMoveAnimation()
    {
        if(isMove != animation.IsPlaying("walk"))
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

    //使敌人浮空:1.后退距离,2.击飞高度,3.攻击者id
    private void EnermyLevitate(float backDistance,float hitHeigh,int killerid)
    {
        GameObject killerGo = null;
        Vector3 targetPos = Vector3.zero;
        PlayerController.Instance.transPlayerDict.TryGetValue(killerid, out killerGo);
        if(killerGo == null)
        {
            Debug.LogError("no such player in the scene!");
            return;
        }
       
        //发射射线,检测与地面的距离
        RaycastHit hit;
        int groudLayerMask = 1 << 9; //只检测layerMask为9的collider
        Physics.Raycast(transform.position, -Vector3.up, out hit, groudLayerMask);
        Debug.DrawRay(transform.position, -Vector3.up, Color.red);
        currentHeigh = hit.distance;
        
        if (currentHeigh >= miniDistance+ hitHeigh && backDistance == 0)
        {
            strikeHeigh = 0; //本次浮空已达最高高度,击飞高度置零
        }
        else
        {
            Vector3 offsetVector = killerGo.transform.forward * backDistance + Vector3.up * hitHeigh;
            targetPos = transform.position + offsetVector;
            transform.position = targetPos;
            strikeHeigh = 0; //击飞高度置零
        }
    }
    //使敌人落回地面
    private void BackToGround()
    {
        //发射射线,检测与地面的距离
        RaycastHit hit;
        int groudLayerMask = 1 << 9; //只检测layerMask为9的collider
        Physics.Raycast(transform.position, -Vector3.up, out hit, groudLayerMask);
        Debug.DrawRay(transform.position, -Vector3.up, Color.red);
        currentHeigh = hit.distance;
        //Debug.Log("BackToGround transform.position.y=" + transform.position.y + ",currentHeigh=" + currentHeigh);
        
        if (currentHeigh > miniDistance && strikeHeigh <= 0) 
        {
            float baseY = hit.point.y; //地面坐标y
            float y = transform.position.y; //当前物体坐标y
            y -= gravity * Time.deltaTime;
            y = y <= baseY ? baseY : y; //当前物体坐标y的值不能小于地面坐标y

            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            Debug.DrawRay(transform.position, -Vector3.up, Color.red);
        }
    }

    /*
    该方法的初衷是解决level4的怪物在追踪玩家时出现错误:
    "SetDestination" can only be called on an active agent that has been placed on a NavMesh.
    一般来说出现该错误出于以下原因:
    1.agent在初始化的时候离NavMesh太远.
    2.bake时把renderer取消了,导致没生成NavMesh.这个可以通过观察场景的navmesh对象得知
    3.通过其他方式改变了agent的位置,但实际上agent在获取该gameObject时仍使用以前的值,
    该错误可以通过agent.updateposition = false;和agent.nextposition = newposition;解决

    但我们所有level的处理代码都是相同的,唯一不同的是NavMesh的位置不同,每个level都有各自的活动范围
    介于unity4.6并没提供agent.isOnNavMesh的API(直到Unity5.X才提供),我们只好判断是否与地面接触来重新
    刷新agent--因为我们发现在游戏运行时,取消勾选navMeshAgent再勾选,错误就解决了.
    */

}
