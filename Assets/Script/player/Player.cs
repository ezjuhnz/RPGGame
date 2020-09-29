using UnityEngine;
using System.Collections;

public class Player :MonoBehaviour{
    private static Player _instance;
    private string _name; //public is for test
    public int roleId = -1;
    public bool isTeam = false;
    private int _level;

    public static Player Instance
    {
        get { return _instance; }
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

    public int Level
    {
        get
        {
            return _level;
        }

        set
        {
            _level = value;
        }
    }

    
    void Awake()
    {
        _instance = this;
    }
}
