using UnityEngine;
using System.Collections;


public class JoyStickDragObject : MonoBehaviour
{
    private static JoyStickDragObject _instance;
    private float bgRadius = 0;
    private UISprite bgSprite;
    private Transform uiRootTransform;
    private Vector3 uiRootScale = Vector3.zero;
    Vector3 mTargetPos;                                         // 目标当前位置
    Vector3 mLastPos;
    
    int mTouchID = 0;

    bool mStarted = false;
    bool mPressed = false;

    private Vector3 offsetFromOrigin = Vector3.zero;
    protected Vector3 originPos = Vector3.zero;
    
    Vector3 calOriginPos = Vector3.zero;


    public Vector3 OffsetFromOrigin
    {
        get { return offsetFromOrigin; }
        set { offsetFromOrigin = value; }
    }

    public static JoyStickDragObject Instance
    {
        get
        {
            return _instance;
        }
    }

    // Use this for initialization
    void Start()
    {
        _instance = this;
        bgSprite = this.transform.parent.GetComponent<UISprite>();
        if(bgSprite != null)
        {
            bgRadius = bgSprite.width/2;//圆心可移动半径
        }
        uiRootTransform = this.transform.root;
        if(uiRootTransform != null)
        {
            uiRootScale = uiRootTransform.localScale;
        }
        originPos = transform.TransformPoint(originPos);
        calOriginPos = new Vector3(originPos.x / uiRootScale.x, originPos.y / uiRootScale.y, 0);
    }

    // Update is called once per frame
    float testTimer = 0;
    int index = 0;
    void Update()
    {
        
        testTimer += Time.deltaTime;
        if(testTimer > 1)
        {
            string message = "origin=" + originPos;
            if (NotificationController.Instance != null)
            {
                NotificationController.Instance.ShowInfomation(index, message);
            }
            testTimer = 0;
            index++;
        }
        
    }

    void OnPress(bool pressed)
    {
        if (enabled && NGUITools.GetActive(gameObject))
        {
            if (pressed)
            {
                if (!mPressed)
                {
                    mTouchID = UICamera.currentTouchID;
                    mPressed = true;
                    mStarted = false;
                    CancelMovement();
                }
            }
            else if (mPressed && mTouchID == UICamera.currentTouchID)
            {
                mPressed = false;
                //target.position = Vector3.zero;
                transform.position = originPos;
                UpdateVector3FromOrigin(originPos);
            }
        }

    }
    void OnDrag(Vector2 delta)
    {
        Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
        float dist = 0f;

        Vector3 currentPos = ray.GetPoint(dist);
        Vector3 offset = currentPos - mLastPos; //当前点与上一个点的向量
        UpdateVector3FromOrigin(currentPos);
        //更新当前坐标到上一时刻坐标的向量
        mLastPos = currentPos;
        if (!mStarted)
        {
            mStarted = true;
            offset = Vector3.zero;
        }
        Move(offset);
    }
    void Move(Vector3 moveDelta)
    {
        mTargetPos += moveDelta;
        Vector3 calTargetPos;
        calTargetPos = new Vector3(mTargetPos.x / uiRootScale.x, mTargetPos.y / uiRootScale.y, 0);
        //第一次OnDrag并不移动target,只是将mStarted标记设为true,第二次及以后才移动target
        float distance = Vector3.Distance(calTargetPos, calOriginPos);

        if(distance > bgRadius) //如果按钮移动的位置超出了bgRaduis半径
        {
            Vector3 tmp = calTargetPos - calOriginPos;
            tmp = tmp.normalized * bgRadius;
            tmp = new Vector3(tmp.x * uiRootScale.x, tmp.y * uiRootScale.y, 0);
            mTargetPos = tmp + originPos;
        }
        transform.position = mTargetPos;
    }
    void CancelMovement()
    {
        mTargetPos = transform.position;
    }

    void UpdateVector3FromOrigin(Vector3 pos)
    {
        pos.z = 0;
        OffsetFromOrigin = pos - originPos;
        //Debug.Log("JoyStick OffsetFromOrigin=" + OffsetFromOrigin);
    }

}
