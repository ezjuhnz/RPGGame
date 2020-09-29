using UnityEngine;
using System.Collections;

public enum EquipEnum
{
    Helm,
    Cloth,
    Weapon,
    Shoes,
    Necklace,
    Bracelet,
    Ring,
    Wing
}
public class Equip : MonoBehaviour {
    //ID 名称 图标 类型（Equip，Drug） 装备类型 售价 星级 品质 伤害 生命 战斗力 作用类型 作用值 描述
    private int id;
    private string ename;
    private string icon;
    private int type;//装备还是药
    private int etype; //装备的类型,上衣,肩甲,下装,腰带...
    private int price;
    private int starLevel;//星级
    private int quatity;//品质
    private int damage;
    private int hp;
    private int power;
    private int ustype; //作用类型:加血?加蓝?
    private int usvalue;//作用值
    private string des;//描述

    #region setter and getter
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

    public string Ename
    {
        get
        {
            return ename;
        }

        set
        {
            ename = value;
        }
    }

    public string Icon
    {
        get
        {
            return icon;
        }

        set
        {
            icon = value;
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

    public int Etype
    {
        get
        {
            return etype;
        }

        set
        {
            etype = value;
        }
    }

    public int Price
    {
        get
        {
            return price;
        }

        set
        {
            price = value;
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

    public int Quatity
    {
        get
        {
            return quatity;
        }

        set
        {
            quatity = value;
        }
    }

    public int Damage
    {
        get
        {
            return damage;
        }

        set
        {
            damage = value;
        }
    }

    public int Hp
    {
        get
        {
            return hp;
        }

        set
        {
            hp = value;
        }
    }

    public int Power
    {
        get
        {
            return power;
        }

        set
        {
            power = value;
        }
    }

    public int Ustype
    {
        get
        {
            return ustype;
        }

        set
        {
            ustype = value;
        }
    }

    public int Usvalue
    {
        get
        {
            return usvalue;
        }

        set
        {
            usvalue = value;
        }
    }

    public string Des
    {
        get
        {
            return des;
        }

        set
        {
            des = value;
        }
    }
    #endregion setter and getter
}
