using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XueCommon.Model;

public class ClientTeam{

    public int masterId = -1; //队长id
    private int size = 4;
    public XueCommon.Model.Role[] roleArr = { null,null,null,null};
    public int[] memberids = { -1, -1, -1, -1 };
    public int memberId_1 = -1;
    public int memberId_2 = -1;
    public int memberId_3 = -1;
    public int memberId_4 = -1;

    public Dictionary<int, GameObject> teamMemberGoDict = new Dictionary<int, GameObject>();

    public int Size
    {
        get
        {
            return size;
        }
    }

}
