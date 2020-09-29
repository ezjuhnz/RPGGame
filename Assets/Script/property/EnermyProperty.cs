using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnermyModel
{
    public List<EnermyProperty> list = new List<EnermyProperty>();
}
public class EnermyProperty  {

    public string guid;//敌人的唯一标识
    public string prefabName;//根据哪个prefab生成敌人
    public Vector3Obj position;//敌人的位置
}
