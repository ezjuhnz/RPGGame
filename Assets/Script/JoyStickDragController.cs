using UnityEngine;
using System.Collections;

public class JoyStickDragController : MonoBehaviour {
    /*
    虚拟手柄的实现思路:
    1.监听按钮是否被按下,如果按下则可以被拖拽,如果没按下则不能拖拽按钮,并马上停止移动玩家
    2.获取拖拽点坐标
    3.计算拖拽点与手柄原点的方向向量 directionVec
    4.根据directionVec移动玩家
    */
    public Transform target;
    bool _mPressed = false;
    private Vector3 buttonOriginPos = Vector3.zero; //按钮原坐标--世界坐标
    private Vector3 buttonCurrentPos = Vector3.zero; //按钮当前坐标--世界坐标
    private Vector3 buttonLastPos = Vector3.zero;
    private float BgRadius = 0;
    private float BgWidth = 0;
    private UISprite bgSprite;
    private Transform uiRootTransform;
    private Vector3 uiRootScale = Vector3.one;


    void Start () {
        
        
        bgSprite = target.transform.parent.GetComponent<UISprite>();
        if(bgSprite != null)
        {
            BgWidth = bgSprite.width;
        }
        BgRadius = BgWidth / 2;
        buttonOriginPos = target.transform.position;
        
        buttonOriginPos.z = 0; //z轴不参与计算
        buttonLastPos = buttonOriginPos;

        Debug.Log("buttonOriginPos = " + buttonOriginPos);
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    void OnPress(bool pressed)
    {
        Debug.Log("pressed = " + pressed);
        _mPressed = pressed;
    }

    void OnDrag(Vector2 delta)
    {
        Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);
        float dist = 0f;
        Vector3 currentPos = ray.GetPoint(dist);//世界坐标
        buttonCurrentPos = currentPos;
        buttonCurrentPos.z = 0;
        //Debug.Log("buttonCurrentPos = " + buttonCurrentPos);
        //将按钮位置更新到新点击的坐标
        
        if(buttonLastPos != buttonCurrentPos)
        {
            UpdateButtonPosition(buttonCurrentPos);
        }
        buttonLastPos = buttonCurrentPos;
    }

    void UpdateButtonPosition(Vector3 pos)
    {
        
        //计算原点坐标和当前坐标之间的距离distance,如果距离大于按钮背景的半径
        //使用半径与distance之比,重新计算当前坐标
        Vector3 worldButtonOriginPos;
        Vector3 worldButtonCurrentPos;
        Vector3 originAndCurrentOffset = Vector3.zero; //原点和当前点击坐标的向量
        Vector3 calCurrentPos = pos;  

        worldButtonCurrentPos = target.transform.InverseTransformPoint(pos);
        worldButtonOriginPos = target.transform.InverseTransformPoint(buttonOriginPos);

        originAndCurrentOffset = worldButtonCurrentPos - worldButtonOriginPos;
        
        float distance = Vector3.Distance(worldButtonOriginPos, worldButtonCurrentPos);
        
        if(distance > BgRadius)//如果距离大于半径,需要重新计算当前坐标
        {
            originAndCurrentOffset = originAndCurrentOffset.normalized * BgRadius;
        }
        calCurrentPos = worldButtonOriginPos + originAndCurrentOffset;
        //Debug.Log("calCurrentPos=" + calCurrentPos);
        target.transform.localPosition = calCurrentPos;
    }
}
