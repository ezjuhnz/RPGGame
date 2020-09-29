using UnityEngine;
using System.Collections;

public class CharacterProperty : MonoBehaviour {

    private int id = 0;
    private string _name = "";
    private string description="";
    private int powerStar = 0;
    private int guardStar = 0;
    private int controlStar = 0;
    private int scopeStar = 0;
    private int speedStar = 0;

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

    public string Description
    {
        get
        {
            return description;
        }

        set
        {
            description = value;
        }
    }

    public int PowerStar
    {
        get
        {
            return powerStar;
        }

        set
        {
            powerStar = value;
        }
    }

    public int GuardStar
    {
        get
        {
            return guardStar;
        }

        set
        {
            guardStar = value;
        }
    }

    public int ControlStar
    {
        get
        {
            return controlStar;
        }

        set
        {
            controlStar = value;
        }
    }

    public int ScopeStar
    {
        get
        {
            return scopeStar;
        }

        set
        {
            scopeStar = value;
        }
    }

    public int SpeedStar
    {
        get
        {
            return speedStar;
        }

        set
        {
            speedStar = value;
        }
    }
}
