using UnityEngine;
using System.Collections;
using XueCommon;
using System.Collections.Generic;
using LitJson;
using XueCommon.Model;
using ExitGames.Client.Photon;
using System;

//该类用于人物装备查询
public class InventoryItemController : ControllerBase {
    private static InventoryItemController _instance;

    public UISprite helmSprite;
    public UISprite clothSprite;
    public UISprite wingSprite;
    public UISprite shoesSprite;
    public UISprite weaponSprite;
    public UISprite braceletSprite;
    public UISprite necklaceSprite;
    public UISprite ringSprite;

    public GameObject equipInventoryItems;
    public TweenScale inventoryItemPopupTween;
    //
    private TweenScale equipScale;    //背包中的装备栏
    private TweenScale medicineScale; //背包中的药品栏
    private TweenScale materialScale; //背包中的材料栏
    private TweenScale otherScale;    //背包中的其它栏
    private int equipCount = 1;      //当前是装备栏的第几个格子
    private int medicineCount = 1;   //药品栏的第几个格子
    private int materialCount = 1;   //材料栏的第几个格子

    private string defaultItemSprite = "bg_道具";

    private InventoryItemProperty inventoryItemProperty;

    public static InventoryItemController Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
        equipScale = GameObject.Find("UI Root/Knapsack/EquipContainer").GetComponent<TweenScale>();
        medicineScale = GameObject.Find("UI Root/Knapsack/MedicineContainer").GetComponent<TweenScale>();
        materialScale = GameObject.Find("UI Root/Knapsack/MaterialContainer").GetComponent<TweenScale>();
        otherScale = GameObject.Find("UI Root/Knapsack/OtherContainer").GetComponent<TweenScale>();
    }

    public override void Start()
    {
        base.Start();
        //发起服务器请求,查询装备信息
        Dictionary<byte, object> parameters = new Dictionary<byte, object>();
        Role role = new Role() { ID = PhotonEngine.Instance.role.ID };
        string json = JsonMapper.ToJson(role);

        parameters.Add((byte)ParameterCode.SubCode, SubCode.GetInventoryItems);
        parameters.Add((byte)ParameterCode.Role, json);
        PhotonEngine.Instance.SendRequest(OperationCode.InventoryItem, parameters);
        //DontDestroyOnLoad(this.gameObject);//防止ControllerBase被销毁
    }

    public override OperationCode OpCode
    {
        get
        {
            return OperationCode.InventoryItem;
        }
    }

    public override void OnOperationResponse(OperationResponse response)
    {
        Dictionary<byte, object> parameters = response.Parameters;
        object o = null;
        SubCode subCode;

        parameters.TryGetValue((byte)ParameterCode.SubCode, out o);
        subCode = (SubCode) o;
        switch (subCode)
        {
            case SubCode.GetInventoryItems:
                //获取角色的装备,在面板中显示
                parameters.TryGetValue((byte)ParameterCode.InventoryItemList, out o);
                List<InventoryItemDB> itList = JsonMapper.ToObject<List<InventoryItemDB>>(o.ToString());
                DisplayInventoryItems(itList);
                break;
            case SubCode.UpdateInventoryItems:
                Debug.Log("SubCode.UpdateInventoryItems...");
                break;
        }
    }
   

    //在Unity客户端展示角色的装备和角色属性
    public void DisplayInventoryItems(List<InventoryItemDB> itdbList)
    {
        InventoryItemProperty property = null;
        foreach (InventoryItemDB itdb in itdbList)
        {
            if(itdb.Type >= (int)EquipType.HELMET)//1.如果是装备
            {
                if (itdb.IsDressed > 0)//已穿戴
                {
                    switch(itdb.Type)
                    {
                        case (byte)EquipType.HELMET:
                            //穿戴头盔
                            helmSprite.spriteName = itdb.SpriteName;
                            property = helmSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        case (byte)EquipType.CLOTH:
                            //穿戴盔甲
                            clothSprite.spriteName = itdb.SpriteName;
                            property = clothSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        case (byte)EquipType.WING:
                            //穿戴翅膀
                            wingSprite.spriteName = itdb.SpriteName;
                            property = wingSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        case (byte)EquipType.SHOES:
                            //穿戴鞋子
                            shoesSprite.spriteName = itdb.SpriteName;
                            property = shoesSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        case (byte)EquipType.WEAPON:
                            //穿戴武器
                            weaponSprite.spriteName = itdb.SpriteName;
                            property = weaponSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        
                        case (byte)EquipType.BRACELET:
                            //穿戴手镯
                            braceletSprite.spriteName = itdb.SpriteName;
                            property = braceletSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        case (byte)EquipType.NECKLACE:
                            //穿戴项链
                            necklaceSprite.spriteName = itdb.SpriteName;
                            property = necklaceSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        case (byte)EquipType.RING:
                            //穿戴戒指
                            ringSprite.spriteName = itdb.SpriteName;
                            property = ringSprite.GetComponent<InventoryItemProperty>();
                            property.SetProperties(itdb);
                            break;
                        default: break;
                    }

                }
                else//如果没穿戴,则在背包的装备栏中展示
                {
                    if(itdb.Type >= (int)EquipType.HELMET)
                    {
                        //先判断该格子是否为空,空则显示当前物品,不为空则遍历下一个格子
                        UISprite sprite = null;
                        sprite = equipInventoryItems.transform.Find("item" + equipCount).GetComponent<UISprite>();
                        inventoryItemProperty = sprite.GetComponent<InventoryItemProperty>();
                        //给inventoryItemProperty设置物品属性
                        inventoryItemProperty.SetProperties(itdb);
                        
                        if (sprite != null && sprite.spriteName == defaultItemSprite)
                        {
                            //当前格子为空
                            sprite.spriteName = itdb.SpriteName;
                        }
                        equipCount++;
                    }

                    else if (itdb.Type == (int)EquipType.MEDICINE) //2.如果是药品
                    {
                        UISprite sprite = null;
                        sprite = equipInventoryItems.transform.Find("item" + medicineCount).GetComponent<UISprite>();
                        inventoryItemProperty = sprite.GetComponent<InventoryItemProperty>();
                        //给inventoryItemProperty设置物品属性
                        inventoryItemProperty.SetProperties(itdb);
                        
                        if (sprite != null && sprite.spriteName == defaultItemSprite)
                        {
                            //当前格子为空
                            sprite.spriteName = itdb.SpriteName;
                        }
                        medicineCount++;
                    }
                    else if (itdb.Type == (int)EquipType.MATERIAL) //3.如果是材料
                    {
                        UISprite sprite = null;
                        sprite = equipInventoryItems.transform.Find("item" + materialCount).GetComponent<UISprite>();
                        inventoryItemProperty = sprite.GetComponent<InventoryItemProperty>();
                        //给inventoryItemProperty设置物品属性
                        inventoryItemProperty.SetProperties(itdb);
                        if (sprite != null && sprite.spriteName == defaultItemSprite)
                        {
                            //当前格子为空
                            sprite.spriteName = itdb.SpriteName;
                        }
                        materialCount++;
                    }
                    else //4.其他类型的物品
                    {

                    }
                }
            } 
        }
    }

    //点击装备栏Tab中的"装备"按钮
    public void OnEquipClick()
    {
        equipScale.PlayReverse();    //显示装备栏
        medicineScale.PlayReverse(); //隐藏药品栏
        materialScale.PlayReverse(); //隐藏材料栏
        otherScale.PlayReverse();    //隐藏其它栏
    }
    //点击装备栏Tab中的"药品"按钮
    public void OnMedicineClick()
    {
        equipScale.PlayForward();    //隐藏装备栏
        medicineScale.PlayForward(); //显示药品栏
        materialScale.PlayReverse(); //隐藏材料栏
        otherScale.PlayReverse();    //隐藏其它栏
    }
    //点击装备栏Tab中的"材料"按钮
    public void OnMaterialClick()
    {
        equipScale.PlayForward();    //隐藏装备栏
        medicineScale.PlayReverse(); //隐藏药品栏
        materialScale.PlayForward(); //显示材料栏
        otherScale.PlayReverse();    //隐藏其它栏
    }
    //点击装备栏Tab中的"其它"按钮
    public void OnOtherClick()
    {
        equipScale.PlayForward();    //隐藏装备栏
        medicineScale.PlayReverse(); //隐藏药品栏
        materialScale.PlayReverse(); //隐藏材料栏
        otherScale.PlayForward();    //显示其它栏
    }

    public void SendRequest(OperationCode code,Dictionary<byte,object> parameters)
    {
        PhotonEngine.Instance.SendRequest(code, parameters);
    }
    //事件定义
    public event OnGetInventoryItemEvent OnGetInventoryItem;
}
