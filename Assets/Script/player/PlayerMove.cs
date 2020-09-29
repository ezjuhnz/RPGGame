using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    const byte forwardMask = 0x08;
    const byte backwardMask = 0x04;
    const byte leftMask = 0x02;
    const byte rightMask = 0x01;

    private static PlayerMove _instance;
	public float velocity = 35;
    private GameObject transcriptPos;
    public bool isCanControl = false;
    //public int roleId = 0;

    private NavMeshAgent agent;//自动导航
    //位置和旋转同步
    private Vector3 lastPosition = Vector3.zero;
    private Vector3 lastRotation = Vector3.zero;
    private bool isMove = false;
    private Animator anim;

    private DateTime lastAnimTime = DateTime.Now;
    private UIButton inviteButton;
    private float p_forward = 1;
    private float p_backward = 1;
    private float p_left = 1;
    private float p_right = 1;
    private byte directions = 0;
    private Vector3 forwardHit = Vector3.zero;
    private Vector3 backwardHit = Vector3.zero;
    private Vector3 leftHit = Vector3.zero;
    private Vector3 rightHit = Vector3.zero;
    private CharacterController cc;
    private Transform overHeadPos;

    private enum CollisionDirection : byte
    {
        None = 0,
        Forward,
        Backward,
        Left,
        Right,
    }

    public static PlayerMove Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
        overHeadPos = this.transform.Find("OverHeadPos");
    }
    void Start()
    {
        //初始化level和名字
        OverHeadManager.Instance.GetOverHead(overHeadPos.gameObject);
        cc = GetComponent<CharacterController>();
        inviteButton = TeamInviteController.Instance.inviteButton;
        transcriptPos = GameObject.FindGameObjectWithTag("Entry");
        agent = GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
        //把本客户端的角色的位置和旋转同步到其它客户端
        lastPosition = transform.position;
        lastRotation = transform.eulerAngles;
        //一定要加判断,否则其它客户端也会发送同步位置请求.导致服务器收到一个角色的多个位置.
        //InvokeRepeating("SyncPositonAndRotation", 0, 1f / 50);
        //InvokeRepeating("SyncPlayerMoveAnimation", 0, 1f / 50);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void FixedUpdate ()
    {
        if (isCanControl == false)
        {
            return;
        }
        //使用射线检测碰撞
        float distanceX = 0;
        float h = Input.GetAxis("Horizontal");//水平
        float v = Input.GetAxis("Vertical");  //垂直

        //检测是否存在虚拟手柄,存在则优先使用虚拟手柄,否则使用键盘控制移动
        if (JoyStickDragObject.Instance != null)
        {
            Vector3 offsetFromOrigin = JoyStickDragObject.Instance.OffsetFromOrigin;
            //使用虚拟手柄控制玩家移动
            JoyStickMove(offsetFromOrigin.normalized); 
        }
        else if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
        {
            anim.SetBool("Move", true);//角色移动动画
            directions = ColliWithBoundary(out forwardHit, out backwardHit, out leftHit, out rightHit);
            BeforeKeyBoardMove(h, v);
            Vector3 playerOffset = Vector3.zero;
            float x = 0;
            float z = 0;
            x = h * velocity * p_left;
            z = v * velocity * p_backward;
            if (h < 0)
            {
                x = h * velocity * p_right;
            }
            if(v < 0)
            {
                z = v * velocity * p_forward;
            }
            playerOffset = new Vector3(x, 0, z);
            transform.Translate(-playerOffset * Time.fixedDeltaTime, Space.World);
            //控制角色的旋转
            transform.rotation = Quaternion.LookRotation(new Vector3(-h, 0, -v));
        }
        else //未按下方向键
        {
            anim.SetBool("Move", false);  
        }
        SyncPositonAndRotation();
        SyncPlayerMoveAnimation();
        if (agent.enabled)
        {
            transform.rotation = Quaternion.LookRotation(agent.velocity); 
        }
        if(transcriptPos == null)
        {
            transcriptPos = GameObject.FindGameObjectWithTag("Entry");
        }
        distanceX = transform.position.x - transcriptPos.transform.position.x;//error
        if (distanceX < 1.0f) //magnitude 得到的是标量--长度
        {
            //到达目的地
            //是否是组队,是的话只有队长才能进入副本
            if(TeamInviteController.Instance.isTeam) //组队
            {
                //是否是队长
                if(PhotonEngine.Instance.role.ID == TeamInviteController.Instance.globalMasterID)
                {
                    //将OverHeadBg设为inactive
                    OverHeadManager.Instance.SetOverHeadBgActive(false);

                    GameController.Instance.EnterTranscript();
                    //通知队友客户端加载副本场景
                    TranscriptController.Instance.TeamMateEnterTranscript();
                    //Destroy(this.gameObject);//进入副本后,删除城镇玩家
                }
                else
                {
                    Debug.Log("只有队长才能进入副本..");
                } 
            }
            else  //单人
            {
                Debug.Log("单人状态==");
                //将OverHeadBg设为inactive
                OverHeadManager.Instance.SetOverHeadBgActive(false);
                GameController.Instance.EnterTranscript();
                //Destroy(this.gameObject);//进入副本后,删除城镇玩家
            }
        }
	}

    //同步位置和旋转
    void SyncPositonAndRotation()
    {
        if (isCanControl == false) return;
        Vector3 position = transform.position;
        Vector3 eulerAngles = transform.eulerAngles;
        if (lastPosition.x != position.x || lastPosition.y != position.y || lastPosition.z != position.z ||
            lastRotation.x != eulerAngles.x || lastRotation.y != eulerAngles.y || lastRotation.z != eulerAngles.z)
        {
            //发起同步请求
            VilligePlayerController.Instance.SyncPositonAndRotationRequest(position, eulerAngles);
            lastPosition = position;
            lastRotation = eulerAngles;
        }
    }

    //同步玩家移动动画
    void SyncPlayerMoveAnimation()
    {
        if (isCanControl == false) return;
        
        if (isMove != anim.GetBool("Move"))//只有动画状态发生改变才需要同步,以免频繁请求服务器
        {
            PlayerMoveAnimationModel model = new PlayerMoveAnimationModel()
            {
                IsMove = anim.GetBool("Move")
            };
            model.SetTime(DateTime.Now);
            VilligePlayerController.Instance.SyncPlayerMoveAnimationRequest(model);
            isMove = anim.GetBool("Move");
        }
    }

    //点击玩家的身体调用该方法
    void OnMouseDown()
    {
        //因为Unity的OnMouseDown有一些BUG,所以使用FixedOnMouseDown代替
    }

    public void FixedOnMouseDown()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 playerPosition = transform.position;
        int joinerid = -1;
        string joinerName = "";
        int inviteid = PhotonEngine.Instance.role.ID;
        joinerid = transform.GetComponent<Player>().roleId;
        joinerName = this.GetComponent<Player>().Name;
        //判断点击的是否是自己,如果是,则不响应
        if (joinerid == PhotonEngine.Instance.role.ID)
        {
            return;
        }
        else
        {
            //给"邀请组队"按钮绑定事件
            EventDelegate ed = new EventDelegate(this, "OnSendTeam");
            EventDelegate.Set(inviteButton.onClick, delegate () { OnSendTeam(joinerid, joinerName); });
            PhotonEngine.Instance.joinerId = joinerid;
            GameController.Instance.ShowTeamBg(playerPosition,mousePosition);
        }
    }
    //点击邀请组队调用该方法: 1.邀请者id, 2.被邀请者id
    void OnSendTeam(int joinerid,string joinerName)
    {
        TeamInviteController.Instance.OnInvite(joinerid,joinerName);
    }
    
    //
    public void setAnim(PlayerMoveAnimationModel model)
    {
        DateTime dt = model.GetTime();
        if (dt > lastAnimTime)
        {
            anim.SetBool("Move", model.IsMove);
            lastAnimTime = dt;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }
    byte ColliWithBoundary(out Vector3 forwardHit,out Vector3 backwardHit,out Vector3 leftHit,out Vector3 rightHit)
    {
        forwardHit = this.forwardHit;
        backwardHit = this.backwardHit;
        leftHit = this.leftHit;
        rightHit = this.rightHit;
        
        Vector3 centerPoint = new Vector3(transform.position.x, transform.position.y + cc.center.y, transform.position.z);
        
        RaycastHit hit;
        int wallLayer = 1 << 11;
        byte directions = 0x00;
        //(0,0,1)
        if(Physics.Raycast(centerPoint, Vector3.forward, out hit, 10, wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.forward * hit.distance, Color.green);
            
            if (hit.distance < 1)
            {
                forwardHit = hit.point;
                directions |= forwardMask;
            }
        }
        //(0,0,-1)
        if(Physics.Raycast(centerPoint, Vector3.back, out hit, 10,wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.back * hit.distance, Color.red);
            if (hit.distance < 1)
            {
                backwardHit = hit.point;
                directions |= backwardMask;
            }
        }
        //(-1,0,0)
        if (Physics.Raycast(centerPoint, Vector3.left, out hit, 10,wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.left * hit.distance, Color.blue);
            if (hit.distance < 1)
            {
                leftHit = hit.point;
                directions |= leftMask;
            }
        }
        //(1,0,0)
        else if (Physics.Raycast(centerPoint, Vector3.right, out hit, 10,wallLayer))
        {
            Debug.DrawRay(centerPoint, Vector3.right * hit.distance, Color.yellow);
            if (hit.distance < 1)
            {
                rightHit = hit.point;
                directions |= rightMask;
            }
        }
        return directions;
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
        if(Physics.Raycast(rayOrigin, fwd, out hit, rayDistance, wallLayer))
        {
            Debug.DrawRay(rayOrigin, fwd, Color.red);
            if(hit.distance < 10)
            {
                forwardHit = new Vector3(hit.point.x,transform.position.y,hit.point.z); //记录射线击中的位置
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
        //Down-负:对应世界坐标的z轴正方向
        if (v < -0.05f)
        {
            if ((directions & forwardMask) <= 0) //前方无障碍(障碍距离大于1)
            {
                p_forward = 1;
            }
            else
            {
                //在1米内的障碍都会被检测到,防止玩家速度过高时,每帧移动的距离超过障碍
                if (forwardHit != Vector3.zero)
                {
                    z = forwardHit.z;
                    distance_z = Mathf.Abs(transform.position.z - z);
                    stepDistance = Mathf.Abs(v * velocity * Time.fixedDeltaTime);
                    p_forward = (stepDistance / distance_z) >= 1 ? 0 : 1;
                }
            }
        }
        //Up-正:对应世界坐标的z轴负方向
        else if (v > 0.05f)
        {
            if ((directions & backwardMask) <= 0)//后方无障碍
            {
                p_backward = 1;
            }
            else
            {
                if (backwardHit != Vector3.zero)
                {
                    z = backwardHit.z;
                    distance_z = Mathf.Abs(transform.position.z - z);
                    stepDistance = Mathf.Abs(v * velocity * Time.fixedDeltaTime);
                    p_backward = (stepDistance / distance_z) >= 1 ? 0 : 1;
                }
            }
        }
        //Left-负:对应世界坐标的x轴负方向
        if (h < -0.05f)
        {
            if ((directions & rightMask) <= 0) //右方无障碍
            {
                p_right = 1;
            }
            else
            {
                if (rightHit != Vector3.zero)
                {
                    x = rightHit.x;
                    distance_x = Mathf.Abs(transform.position.x - x);
                    stepDistance = Mathf.Abs(h * velocity * Time.fixedDeltaTime);
                    p_right = (stepDistance / distance_x) >= 1 ? 0 : 1;
                }
            }
        }
        //左(负)
        else if (h > 0.05f)
        {
            if ((directions & leftMask) <= 0) //左方无障碍
            {
                p_left = 1;
            }
            else
            {
                if (leftHit != Vector3.zero)
                {
                    x = leftHit.x;
                    distance_x = Mathf.Abs(transform.position.x - x);
                    stepDistance = Mathf.Abs(h * velocity * Time.fixedDeltaTime);
                    p_left = (stepDistance / distance_x) >= 1 ? 0 : 1;
                }
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
            Vector3 worldOffset = new Vector3(-offsetFromOrigin.x, 0, -offsetFromOrigin.y);
            transform.rotation = Quaternion.LookRotation(worldOffset);

            //检测玩家前方是否有障碍物
            CheckForwardCollision(out forwardHit);
            //float fecility = 100f;
            p_forward = 1;
            if (forwardHit != Vector3.zero) //前方障碍距离小于2m
            {
                forwardHit.y = transform.position.y;
                distace_forward = Vector3.Distance(transform.position, forwardHit);
                p_forward = (distace_forward > 1) ? 1 : 0;
            }
            playerOffset = velocity * p_forward * worldOffset.normalized * Time.fixedDeltaTime;
            currentPos = playerOffset + transform.position;
            
            transform.position = currentPos;
        }
        else
        {
            anim.SetBool("Move", false);//角色移动动画
        }
       
    }

}
