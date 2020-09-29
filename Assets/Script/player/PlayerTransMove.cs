using UnityEngine;
using System.Collections;
using System;

public class PlayerTransMove : MonoBehaviour {
    public float velocity = 5;
    public bool isCanControl = false;
    public int roleid = -1;
    private Animator anim;
    private bool isMove = false;
    //控制副本中角色的移动
    private Vector3 lastPosition;
    private Vector3 lastEulerAngles;
    private DateTime lastAnimTime = DateTime.Now;
    private PlayerAttack playerAttack;
    private Vector3 ForwardHit = Vector3.zero;
    private Vector3 BackwardHit = Vector3.zero;
    private Vector3 LeftHit = Vector3.zero;
    private Vector3 RightHit = Vector3.zero;
    private Vector3 ForwardHit_e = Vector3.zero;
    private Vector3 BackwardHit_e = Vector3.zero;
    private Vector3 LeftHit_e = Vector3.zero;
    private Vector3 RightHit_e = Vector3.zero;

    float p_fb = 1;
    float p_lr = 1;
    private float p_forward = 1;
    private float p_backward = 1;
    private float p_left = 1;
    private float p_right = 1;
    private byte directions = 0;
    private CharacterController cc;
    private Transform overHeadPos;

    /********************  CONSTANT DEFINITION ***************************/
    const byte forwardMask = 0x08;
    const byte backwardMask = 0x04;
    const byte leftMask = 0x02;
    const byte rightMask = 0x01;
    void Start () {
        //初始化level和名字
        overHeadPos = this.transform.Find("OverHeadPos");
        OverHeadManager.Instance.GetOverHead(overHeadPos.gameObject);

        cc = GetComponent<CharacterController>();
        isCanControl = false; //因为属性是public,所以预制体可能默认设置isCanControl为true
        playerAttack = this.GetComponent<PlayerAttack>();
        anim = this.GetComponent<Animator>();
        if(PhotonEngine.Instance.role.ID == this.roleid)
        {
            isCanControl = true;
        }
        lastPosition = transform.position;
        lastEulerAngles = transform.eulerAngles;
        
        if(TeamInviteController.Instance.isTeam)
        {
            //InvokeRepeating("SyncTransPlayerPosAndRotation", 0, 1f / 50f);
            //InvokeRepeating("SyncTransPlayerMoveAnimation", 0, 1f / 50f);
        }
    }
	
	// Update is called once per frame
    void Update()
    {
        
    }

	void FixedUpdate () {
        
        if (isCanControl == false)
        {
            return;
        }
        bool isDead = playerAttack.isDead;
        if (isDead) return;
        //玩家被击飞不会自动落回地面,所以要手动处理
        BackToGround();
        //使用射线检测碰撞
        
        float x = 0;
        float z = 0;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        //byte wallDirection = ColliWithBoundary(out ForwardHit, out BackwardHit, out LeftHit, out RightHit);
        //byte enermyDirection = ColliWithEnermy(out ForwardHit_e, out BackwardHit_e, out LeftHit_e, out RightHit_e);
        //byte directions = (byte)(wallDirection | enermyDirection);
        //检测是使用虚拟手柄还是键盘控制玩家移动
        if (JoyStickDragObject.Instance != null)
        {
            Vector3 offsetFromOrigin = JoyStickDragObject.Instance.OffsetFromOrigin;
            //使用虚拟手柄控制玩家移动
            JoyStickMove(offsetFromOrigin.normalized);
        }
        else if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
        {
            if (!anim.GetCurrentAnimatorStateInfo(1).IsName("Empty State")) return;
            anim.SetBool("Move", true);//角色移动动画
            directions = ColliWithBoundary(out ForwardHit, out BackwardHit, out LeftHit, out RightHit);
            BeforeKeyBoardMove(h, v);
            Vector3 playerOffset = Vector3.zero;
            x = h * velocity * p_right;
            z = v * velocity * p_forward;
            if (h < 0)
            {
                x = h * velocity * p_left;
            }
            if (v < 0)
            {
                z = v * velocity * p_backward;
            }
            playerOffset = new Vector3(x, 0, z);
            transform.Translate(playerOffset * Time.fixedDeltaTime, Space.World);
            //控制角色的旋转
            transform.rotation = Quaternion.LookRotation(new Vector3(h, 0, v));
        }
        else
        {
            anim.SetBool("Move", false);
            //rigidbody.velocity = new Vector3(0, nowVel.y, 0);
        }
        if (TranscriptTeamController.Instance.isTeam)
        {
            SyncTransPlayerPosAndRotation();
            SyncTransPlayerMoveAnimation();
        }     
    }

    //同步角色位置和旋转
    void SyncTransPlayerPosAndRotation()
    {
        if (isCanControl == false) return;
        if (this.GetComponent<PlayerAttack>().hp <= 0) return;
        Vector3 position = transform.position;
        Vector3 eulerAngles = transform.eulerAngles;
        if (lastPosition.x != position.x || lastPosition.y != position.y || lastPosition.z != position.z ||
            lastEulerAngles.x != eulerAngles.x || lastEulerAngles.y != eulerAngles.y || lastEulerAngles.z != eulerAngles.z)
        {
            //发起同步请求
            BattleController.Instance.SyncTransPosAndRotationRequest(position, eulerAngles);
            lastPosition = position;
            lastEulerAngles = eulerAngles;
        }
    }

    //同步角色移动动画
    void SyncTransPlayerMoveAnimation()
    {
        if (isCanControl == false) return;
        if (this.GetComponent<PlayerAttack>().hp <= 0) return;
        if (isMove != anim.GetBool("Move"))//只有动画状态发生改变才需要同步,以免频繁请求服务器
        {
            PlayerMoveAnimationModel model = new PlayerMoveAnimationModel()
            {
                IsMove = anim.GetBool("Move")
            };
            model.SetTime(DateTime.Now);
            BattleController.Instance.SyncPlayerMoveAnimationRequest(model);
            isMove = anim.GetBool("Move");
        }
    }

    public void setAnim(PlayerMoveAnimationModel model)
    {
        DateTime dt = model.GetTime();
        if (dt > lastAnimTime)
        {
            anim.SetBool("Move",model.IsMove);
            lastAnimTime = dt;
        }
    }

    public void SetAttackAnim(int roleid,PlayerAnimationModel model)
    {
        if (anim == null) return;
        if(model.attack)
        {
            anim.SetTrigger("Attack");
            return;
        }
        //为了解决服务器过快发送停止播放动画请求,导致动画还没开始播放就停止
        if(!model.skill1)//停止skill1动画
        {
            //通过Coroutin延迟执行"停止播放动画"
            StartCoroutine("StopAnimation");
        }
        else
        {
            anim.SetBool("Skill1", model.skill1);
        }
        if(!model.skill2)
        {
            StartCoroutine("StopAnimation");
        }
        else
        {
            anim.SetBool("Skill2", model.skill2);
        }
        if (!model.skill3)
        {
            StartCoroutine("StopAnimation");
        }
        else
        {
            anim.SetBool("Skill3", model.skill3);
        }
        //玩家被攻击动画播放
        bool isHurtAnim = model.takeDamage;
        int hurtNum = model.hurtNum;
        
        if(hurtNum > 0)
        {
            if(isHurtAnim)
            {
                anim.SetTrigger("GetHurt");
            }
            PlayerAttack playerAttack = this.GetComponent<PlayerAttack>();
            playerAttack.ShowHurtNum(hurtNum);//1.显示伤害数字
            //2.被攻击对象所属客户端才会显示受伤红屏效果
            if(roleid == PhotonEngine.Instance.role.ID)
            {
                BloodScreen.Instance.ShowBloodScreen();
            } 
        }
        
    }

    IEnumerator StopAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        anim.SetBool("Skill1", false);
        anim.SetBool("Skill2", false);
        anim.SetBool("Skill3", false);

    }
    //与边界碰撞
    byte ColliWithBoundary(out Vector3 ForwardHit, out Vector3 BackwardHit, out Vector3 LeftHit, out Vector3 RightHit)
    {
        ForwardHit = this.ForwardHit;
        BackwardHit = this.BackwardHit;
        LeftHit = this.LeftHit;
        RightHit = this.RightHit;
       
        Vector3 centerPoint = new Vector3(transform.position.x, transform.position.y + cc.center.y, transform.position.z);
        int wallLayer = 1 << 11;
        RaycastHit hit;
        byte directions = 0x00;
        if (Physics.Raycast(centerPoint, Vector3.forward, out hit, 10,wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.forward * hit.distance, Color.green);
            if (hit.distance < 1)
            {
                ForwardHit = hit.point;
                directions |= forwardMask;
            }
        }
        if (Physics.Raycast(centerPoint, Vector3.back, out hit, 10,wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.back * hit.distance, Color.red);
            if (hit.distance < 1)
            {
                BackwardHit = hit.point;
                directions |= backwardMask;
            }
        }
        if (Physics.Raycast(centerPoint, Vector3.left, out hit, 10,wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.left * hit.distance, Color.blue);
            if (hit.distance < 1)
            {
                LeftHit = hit.point;
                directions |= leftMask;
            }
        }
        else if (Physics.Raycast(centerPoint, Vector3.right, out hit, 10,wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.right * hit.distance, Color.yellow);
            if (hit.distance < 1)
            {
                RightHit = hit.point;
                directions |= rightMask;
            }
        }
        return directions;
    }

    byte ColliWithEnermy(out Vector3 forwardHit, out Vector3 backwardHit, out Vector3 leftHit, out Vector3 rightHit)
    {
        forwardHit = ForwardHit_e;
        backwardHit = BackwardHit_e;
        leftHit = LeftHit_e;
        rightHit = RightHit_e;
        RaycastHit hit;
        byte directions = 0x00;
        int enermyLayer = (1 << 13);//检测与enermy的碰撞
        Vector3 originPoint = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        if (Physics.Raycast(originPoint, Vector3.forward, out hit, 10, enermyLayer))
        {
            Debug.DrawRay(originPoint, Vector3.forward * hit.distance, Color.green);

            if (hit.distance < 1)
            {
                forwardHit = hit.point;
                directions |= forwardMask;
            }
        }
        if (Physics.Raycast(originPoint, -Vector3.forward, out hit, 10, enermyLayer))
        {
            Debug.DrawRay(originPoint, -Vector3.forward * hit.distance, Color.red);
            if (hit.distance < 1)
            {
                backwardHit = hit.point;
                directions |= backwardMask;
            }
        }
        if (Physics.Raycast(originPoint, Vector3.left, out hit, 10, enermyLayer))
        {
            Debug.DrawRay(originPoint, Vector3.left * hit.distance, Color.blue);
            if (hit.distance < 1)
            {
                leftHit = hit.point;
                directions |= leftMask;
            }
        }
        else if (Physics.Raycast(originPoint, -Vector3.left, out hit, 10, enermyLayer))
        {
            Debug.DrawRay(originPoint, -Vector3.left * hit.distance, Color.yellow);
            if (hit.distance < 1)
            {
                rightHit = hit.point;
                directions |= rightMask;
            }
        }
        return directions;
    }



    private void BackToGround()
    {
        //向下发射射线,获取地面坐标
        Vector3 groundPos = Vector3.zero;
        float y = transform.position.y;
        float defer_y = 0;
        float rayLength = 10;
        RaycastHit hit;
        int groundLayer = 1 << 9;
        Vector3 originPoint = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        if (Physics.Raycast(originPoint, -Vector3.up, out hit,rayLength, groundLayer))
        {
            Debug.DrawRay(originPoint, -Vector3.up * hit.distance, Color.red);
            groundPos = hit.point;
            defer_y = y - groundPos.y;
            if (Mathf.Abs(defer_y) > 0.05f) //上楼梯时要增加y的值
            {
                transform.position = new Vector3(transform.position.x,hit.point.y, transform.position.z);
            }
        }
    }

    void JoyStickMove(Vector3 offsetFromOrigin)
    {
        Vector3 playerOffset = Vector3.zero;
        Vector3 currentPos = Vector3.zero; ;
        if (offsetFromOrigin != Vector3.zero)
        {
            anim.SetBool("Move", true);//角色移动动画
            float distace_forward = -1;
            //旋转玩家面向指定方向
            Vector3 worldOffset = new Vector3(offsetFromOrigin.x, 0, offsetFromOrigin.y);
            transform.rotation = Quaternion.LookRotation(worldOffset);

            //检测玩家前方是否有障碍物
            CheckForwardCollision(out ForwardHit);
            //float fecility = 100f;
            p_fb = 1;
            if (ForwardHit != Vector3.zero) //前方障碍距离小于2m
            {
                ForwardHit.y = transform.position.y;
                distace_forward = Vector3.Distance(transform.position, ForwardHit);
                p_fb = (distace_forward > 1) ? 1 : 0;
            }
            playerOffset = velocity * p_fb * worldOffset.normalized * Time.fixedDeltaTime;
            currentPos = playerOffset + transform.position;

            transform.position = currentPos;
        }
        else
        {
            anim.SetBool("Move", false);//角色移动动画
        }
    }

    void CheckForwardCollision(out Vector3 forwardHit)
    {
        forwardHit = Vector3.zero;
        //TODO
        RaycastHit hit;
        int wallLayer = 1 << 11;
        int rayDistance = 30;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        Vector3 rayOrigin = transform.position;
        if (Physics.Raycast(rayOrigin, fwd, out hit, rayDistance, wallLayer))
        {
            Debug.DrawRay(rayOrigin, fwd, Color.red);
            if (hit.distance < 10)
            {
                forwardHit = new Vector3(hit.point.x, transform.position.y, hit.point.z); //记录射线击中的位置
            }
        }
    }

    void BeforeKeyBoardMove(float h, float v)
    {
        float x = 0;
        float z = 0;
        float distance_x = 0;
        float distance_z = 0;
        float stepDistance = 0;
        //UP():对应世界坐标z轴正方向
        if (v > 0.05f)
        {
            if ((directions & forwardMask) <= 0) //前方无障碍(障碍距离大于1)
            {
                p_forward = 1;
            }
            else
            {
                //在1米内的障碍都会被检测到,防止玩家速度过高时,每帧移动的距离超过障碍
                if (ForwardHit != Vector3.zero)
                {
                    z = ForwardHit.z;
                    distance_z = Mathf.Abs(transform.position.z - z);
                    p_forward = (distance_z < 1) ? 0 : 1;
                }
            }
        }
        //Down(正):对应世界坐标z轴负方向
        else if (v < -0.05f)
        {
            if ((directions & backwardMask) <= 0)//后方无障碍
            {
                p_backward = 1;
            }
            else
            {
                if (BackwardHit != Vector3.zero)
                {
                    z = BackwardHit.z;
                    distance_z = Mathf.Abs(transform.position.z - z);
                    p_backward = (distance_z < 1) ? 0 : 1;
                }
            }
        }
        //Left(负):对应世界坐标x轴正方向
        if (h > 0.05f)
        {
            if ((directions & rightMask) <= 0) //右方无障碍
            {
                p_right = 1;
            }
            else
            {
                if (RightHit != Vector3.zero)
                {
                    x = RightHit.x;
                    distance_x = Mathf.Abs(transform.position.x - x);
                    p_right = (distance_z < 1) ? 0 : 1;
                }
            }
        }
        //左(负)
        else if (h < -0.05f)
        {
            if ((directions & leftMask) <= 0) //左方无障碍
            {
                p_left = 1;
            }
            else
            {
                if (LeftHit != Vector3.zero)
                {
                    x = LeftHit.x;
                    distance_x = Mathf.Abs(transform.position.x - x);
                    p_left = (distance_z < 1) ? 0 : 1;
                }
            }
        }
    }
}
