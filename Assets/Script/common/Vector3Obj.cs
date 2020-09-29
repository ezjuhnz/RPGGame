using UnityEngine;
using System.Collections;

public class Vector3Obj {

    public double x;
    public double y;
    public double z;

    public Vector3Obj()
    {
        this.x = 0;
        this.y = 0;
        this.z = 0;
    }
    public Vector3Obj(Vector3 position)
    {
        this.x = position.x;
        this.y = position.y;
        this.z = position.z;
    }
    public Vector3 ToVector3()
    {
        Vector3 tmp = Vector3.zero;
        tmp.x = (float)x;
        tmp.y = (float)y;
        tmp.z = (float)z;
        return tmp;
    }


}
