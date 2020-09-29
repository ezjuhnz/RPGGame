using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//该类主要是用来展示人物的信息(角色属性,物品等)
public class MyUIEventListener : MonoBehaviour
{
    private TweenScale knapsackTween;
    private TweenScale inventoryTween;
    private TweenScale inventoryPopupTween;
    private string envBoundary = "EnvBoundary";
    public Camera nguiCamera;
    void Start()
    {
        knapsackTween = GameObject.Find("UI Root/Knapsack").GetComponent<TweenScale>();
        inventoryTween = GameObject.Find("UI Root/PlayerProperty").GetComponent<TweenScale>();
    }
    void Update()
    {
        //1.按下"I"键,显示装备信息
        if (Input.GetKey("i"))
        {
            knapsackTween.PlayForward();
        }
        //2.按下"M"键,显示人物信息
        else if (Input.GetKey("m"))
        {
            inventoryTween.PlayForward();
        }
        //3.按下ESC键
        else if (Input.GetKey(KeyCode.Escape))
        {
            inventoryPopupTween = InventoryItemController.Instance.inventoryItemPopupTween;
            knapsackTween.PlayReverse();
            inventoryTween.PlayReverse();
            inventoryPopupTween.PlayReverse();
            //隐藏所有标签为PopUpUI的弹出框
            HidePopUpUI();
            HideUIOnEnvClick();
        }
        //4.点击鼠标左键
        else if(Input.GetMouseButtonDown(0))
        {
            //如果点击鼠标左键时,点击的地方处于弹出框UI的外面,则隐藏UI
            if(IsMouseOverUI)
            {
                Debug.Log("hit on UI");
            }
            else
            {
                HideUIOnEnvClick();
            }
        }
    }

    void HidePopUpUI()
    {
        GameObject[] goArr = GameObject.FindGameObjectsWithTag("PopUpUI");
        foreach (GameObject go in goArr)
        {
            go.SetActive(false);
        }
    }

    //在点击到场景时,隐藏这种类型的UI
    void HideUIOnEnvClick()
    {
        GameObject[] goArr = GameObject.FindGameObjectsWithTag("SelfHide");
        foreach (GameObject go in goArr)
        {
            TweenScale ts = go.GetComponent<TweenScale>();
            if(ts != null)
            {
                ts.PlayReverse();
            }
        }
    }

    public static bool IsMouseOverUI
    {
        get
        {
            Vector3 mousePostion = Input.mousePosition;
            GameObject hoverobject = UICamera.Raycast(mousePostion) ? UICamera.lastHit.collider.gameObject : null;
            if (hoverobject != null)
            {
                return true;
            }
            return false;
        }
    }
}
