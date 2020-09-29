using UnityEngine;
using System.Collections;

public class RoleProperty : MonoBehaviour {
    private int id;
    private string _name;
    private int level;
    private bool isMan;
    private int userid;
    private int exp;
    private int diamond;
    private int coin;
    private int energy;
    private int toughen;
    private int characterId;

    #region setter & getter
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

    public bool IsMan
    {
        get
        {
            return isMan;
        }

        set
        {
            isMan = value;
        }
    }

    public int Userid
    {
        get
        {
            return userid;
        }

        set
        {
            userid = value;
        }
    }

    public int Exp
    {
        get
        {
            return exp;
        }

        set
        {
            exp = value;
        }
    }

    public int Diamond
    {
        get
        {
            return diamond;
        }

        set
        {
            diamond = value;
        }
    }

    public int Coin
    {
        get
        {
            return coin;
        }

        set
        {
            coin = value;
        }
    }

    public int Energy
    {
        get
        {
            return energy;
        }

        set
        {
            energy = value;
        }
    }

    public int Toughen
    {
        get
        {
            return toughen;
        }

        set
        {
            toughen = value;
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

    public int CharacterId
    {
        get
        {
            return characterId;
        }

        set
        {
            characterId = value;
        }
    }
    #endregion setter & getter
}
