using UnityEngine;
using System.Collections;
using XueCommon.Model;
using UnityEngine.EventSystems;
using System;
using XueCommon;
using System.Collections.Generic;
using LitJson;

public class InventoryItemProperty : MonoBehaviour
{
    private int id;
    private int inventoryId;
    private int count;
    private int level;
    private int isDressed;
    private int roleId;
    private int userId;
    private int type; //物品类型
    private int toughenLevel;//强化等级
    private string _name; //物品名
    private string spriteName; //图片名
    private int starLevel;
    private int intel;
    private int strength;
    private int matk;
    private int patk;
    private string desc;

    #region setter and getter;
    public int InventoryId
    {
        get
        {
            return inventoryId;
        }

        set
        {
            inventoryId = value;
        }
    }

    public int Count
    {
        get
        {
            return count;
        }

        set
        {
            count = value;
        }
    }

    public int Level
    {
        get
        {
            return level;
        }

        set
        {
            level = value;
        }
    }

    public int IsDressed
    {
        get
        {
            return isDressed;
        }

        set
        {
            isDressed = value;
        }
    }

    public int RoleId
    {
        get
        {
            return roleId;
        }

        set
        {
            roleId = value;
        }
    }

    public int UserId
    {
        get
        {
            return userId;
        }

        set
        {
            userId = value;
        }
    }

    public int Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
        }
    }

    public int ToughenLevel
    {
        get
        {
            return toughenLevel;
        }

        set
        {
            toughenLevel = value;
        }
    }

    public string Name
    {
        get
        {
            return _name;
        }

        set
        {
            _name = value;
        }
    }

    public string SpriteName
    {
        get
        {
            return spriteName;
        }

        set
        {
            spriteName = value;
        }
    }

    public int StarLevel
    {
        get
        {
            return starLevel;
        }

        set
        {
            starLevel = value;
        }
    }

    public int Intel
    {
        get
        {
            return intel;
        }

        set
        {
            intel = value;
        }
    }



    public int Matk
    {
        get
        {
            return matk;
        }

        set
        {
            matk = value;
        }
    }

    public int Patk
    {
        get
        {
            return patk;
        }

        set
        {
            patk = value;
        }
    }

    public int Strength
    {
        get
        {
            return strength;
        }

        set
        {
            strength = value;
        }
    }

    public string Desc
    {
        get
        {
            return desc;
        }

        set
        {
            desc = value;
        }
    }

    public int Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
        }
    }

    public void SetProperties(InventoryItemDB itdb)
    {
        this.Id = itdb.ID;
        this.inventoryId = itdb.InventoryID;
        this.roleId = itdb.RoleId;
        this.Count = itdb.Count;
        this.Level = itdb.Level;
        this.IsDressed = itdb.IsDressed;
        this.UserId = itdb.UserId;
        this.Type = itdb.Type;
        this.ToughenLevel = itdb.ToughenLevel;
        this.Name = itdb.Name;
        this.SpriteName = itdb.SpriteName;
        this.ToughenLevel = itdb.ToughenLevel;
        this.StarLevel = itdb.StarLevel;
        this.Intel = itdb.Intelligence;
        this.Strength = itdb.Strength;
        this.Matk = itdb.Matk;
        this.Patk = itdb.Patk;
        this.Desc = itdb.Desc;
    }



    #endregion setter and getter;

    private UILabel nameLabel;
    private UILabel toughenLabel;
    private UILabel levelLabel;
    private UILabel starLevelLabel;
    private UILabel intelLabel;
    private UILabel strengthLabel;
    private UILabel matkLabel;
    private UILabel patkLabel;
    private UILabel descLabel;
    private TweenScale tween;

    private InventoryItemDB itemdbTmp;
    private InventoryItemProperty playerProperty;

    private UISprite playerSprite;
    private UISprite knapsackSprite;

    void Start()
    {
        tween = InventoryItemController.Instance.inventoryItemPopupTween;
        nameLabel = tween.transform.Find("name-label").GetComponent<UILabel>();
        toughenLabel = tween.transform.Find("toughen-label").GetComponent<UILabel>();
        levelLabel = tween.transform.Find("level-label").GetComponent<UILabel>();
        starLevelLabel = tween.transform.Find("starlevel-label").GetComponent<UILabel>();
        intelLabel = tween.transform.Find("intel-label").GetComponent<UILabel>();
        strengthLabel = tween.transform.Find("strength-label").GetComponent<UILabel>();
        matkLabel = tween.transform.Find("matk-label").GetComponent<UILabel>();
        patkLabel = tween.transform.Find("patk-label").GetComponent<UILabel>();
        descLabel = tween.transform.Find("desc-label").GetComponent<UILabel>();
    }
    int mouse = 0;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))//zuo
        {
            mouse = 0;
        }
        if (Input.GetMouseButtonDown(1))//右
        {
            mouse = 1;
        }
        if (Input.GetMouseButtonDown(2))//中
        {
            mouse = 2;
        }
    }

    void OnClick()
    {
        if(mouse == 0) //鼠标左键
        {
            nameLabel.text = this.Name;
            toughenLabel.text = "强化: " + this.ToughenLevel;
            levelLabel.text = "等级: " + this.Level;
            starLevelLabel.text = "品质: " + this.StarLevel;
            intelLabel.text = "智力: " + this.Intel;
            strengthLabel.text = "力量: " + this.Strength;
            matkLabel.text = "魔法攻击力: " + this.Matk;
            patkLabel.text = "物理攻击力: " + this.Patk;
            descLabel.text = this.Desc;
            tween.PlayForward();
        }
        if(mouse == 1)//鼠标右键
        {
            itemdbTmp = new InventoryItemDB();
            //1.根据当前物品的信息找到PlayerProperty组件中的对应Sprite
           switch (this.Type)
            {
                case (int)EquipType.HELMET: //2.头盔
                    
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/HelmSprite").GetComponent<InventoryItemProperty>();

                    SetProperties(this,itemdbTmp);//将背包中的物品信息存储到临时变量
                    this.SetProperties(playerProperty); //
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                   
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;

                    break;
                case (int)EquipType.CLOTH: //3.衣服
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/ClothSprite").GetComponent<InventoryItemProperty>();
                    SetProperties(this, itemdbTmp);//将背包中的物品信息存储到临时变量
                    
                    this.SetProperties(playerProperty); //
                   
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                   
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;
                    break;
                case (int)EquipType.WING: //4.翅膀
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/WingSprite").GetComponent<InventoryItemProperty>();
                    SetProperties(this, itemdbTmp);//将背包中的物品信息存储到临时变量
                   
                    this.SetProperties(playerProperty); //
                   
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                    
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;
                    break;
                case (int)EquipType.SHOES: //5.鞋子
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/ShoesSprite").GetComponent<InventoryItemProperty>();
                    SetProperties(this, itemdbTmp);//将背包中的物品信息存储到临时变量
                   
                    this.SetProperties(playerProperty); //
                   
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                    
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;
                    break;
                case (int)EquipType.WEAPON: //6.武器
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/WeaponSprite").GetComponent<InventoryItemProperty>();
                    SetProperties(this, itemdbTmp);//将背包中的物品信息存储到临时变量
                    
                    this.SetProperties(playerProperty); //
                    
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                   
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;
                    break;
                case (int)EquipType.BRACELET: //7.手镯
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/BraceletSprite").GetComponent<InventoryItemProperty>();
                    SetProperties(this, itemdbTmp);//将背包中的物品信息存储到临时变量
                   
                    this.SetProperties(playerProperty); //
                    
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                    
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;
                    break;
                case (int)EquipType.NECKLACE: //8.项链
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/NecklaceSprite").GetComponent<InventoryItemProperty>();
                    SetProperties(this, itemdbTmp);//将背包中的物品信息存储到临时变量
                    
                    this.SetProperties(playerProperty); //
                    
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                    
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;
                    break;
                case (int)EquipType.RING: //9.戒指
                    playerProperty = GameObject.Find("UI Root/PlayerProperty/RingSprite").GetComponent<InventoryItemProperty>();
                    SetProperties(this, itemdbTmp);//将背包中的物品信息存储到临时变量
                    
                    this.SetProperties(playerProperty); //
                    
                    this.IsDressed = 0; //装备脱下,IsDressed设置为0
                    playerProperty.SetProperties(itemdbTmp);//
                    
                    //交换--图片的显示
                    playerSprite = playerProperty.GetComponent<UISprite>();
                    knapsackSprite = transform.GetComponent<UISprite>();
                    playerSprite.spriteName = playerProperty.SpriteName;
                    knapsackSprite.spriteName = this.SpriteName;
                    break;
                default: break;
            }
            //2.与数据库同步
            InventoryItemDB itdb_player = CreateInventoryItem(playerProperty);
            InventoryItemDB itdb_knapsack = CreateInventoryItem(this);
            List<InventoryItemDB> list = new List<InventoryItemDB>();
            list.Add(itdb_player);
            list.Add(itdb_knapsack);
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            string json = JsonMapper.ToJson(list);
            parameters.Add((byte)ParameterCode.SubCode, SubCode.UpdateInventoryItems);
            parameters.Add((byte)ParameterCode.InventoryItemList,json);
            InventoryItemController.Instance.SendRequest(OperationCode.InventoryItem, parameters);
        }
    }
 

    public InventoryItemDB CreateInventoryItem(InventoryItemProperty property)
    {
        InventoryItemDB itdb = new InventoryItemDB()
        {
            ID = property.Id,
            InventoryID = property.InventoryId,
            Name = property.Name,
            Type = property.Type,
            UserId = property.UserId,

            Level = property.Level,
            IsDressed = property.IsDressed,
            RoleId = property.RoleId,
            ToughenLevel = property.ToughenLevel,
            SpriteName = property.SpriteName,
            StarLevel = property.StarLevel,
            Intelligence = property.Intel,
            Strength = property.Strength,
            Matk = property.Matk,
            Patk = property.Patk,
            Desc = property.Desc,
        };
        return itdb;
    }

    public void SetProperties(InventoryItemProperty property,InventoryItemDB itdb)
    {
        itdb.ID = property.Id;
        itdb.Name = property.Name;
        itdb.Type = property.Type;
        itdb.UserId = property.UserId;
        itdb.InventoryID = property.InventoryId;
        itdb.Level = property.Level;
        itdb.IsDressed = 1;
        itdb.RoleId = property.RoleId;
        itdb.ToughenLevel = property.ToughenLevel;
        itdb.SpriteName = property.SpriteName;
        itdb.StarLevel = property.StarLevel;
        itdb.Intelligence = property.Intel;
        itdb.Strength = property.Strength;
        itdb.Matk = property.Matk;
        itdb.Patk = property.Patk;
        itdb.Desc = property.Desc;
    }

    public void SetProperties(InventoryItemProperty property)
    {
        this.Id = property.Id;
        this.Name = property.Name;
        this.Type = property.Type;
        this.UserId = property.UserId;
        this.InventoryId = property.InventoryId;
        this.Level = property.Level;
        this.IsDressed = 1;
        this.RoleId = property.RoleId;
        this.ToughenLevel = property.ToughenLevel;
        this.SpriteName = property.SpriteName;
        this.StarLevel = property.StarLevel;
        this.Intel = property.Intel;
        this.Strength = property.Strength;
        this.Matk = property.Matk;
        this.Patk = property.Patk;
        this.Desc = property.Desc;
    }
}
