using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Knapsack : MonoBehaviour {

    public TextAsset inventoryText;
    List<Equip> equipList = new List<Equip>();

	void Start () {
        //ID 名称 图标 类型（Equip，Drug） 装备类型 售价 星级 品质 伤害 生命 战斗力 作用类型 作用值 描述
        string[] inventoryInfoArray = inventoryText.ToString().Split('\n');
        for(int i = 0; i < inventoryInfoArray.Length; i++)
        {
            string[] inventoryInfo = inventoryInfoArray[i].Split('|');
            Equip equip = new Equip();
           
            equip.Id = int.Parse(inventoryInfo[0]);
            equip.Ename = inventoryInfo[1];
            equip.Icon = inventoryInfo[2];
            equip.Type = int.Parse(inventoryInfo[3]);
            equip.Etype = int.Parse(inventoryInfo[4]);
            equip.Price = int.Parse(inventoryInfo[5]);
            equip.StarLevel = int.Parse(inventoryInfo[6]);
            equip.Quatity = int.Parse(inventoryInfo[7]);
            equip.Damage = int.Parse(inventoryInfo[8]);
            equip.Hp = int.Parse(inventoryInfo[9]);
            equip.Power = int.Parse(inventoryInfo[10]);
            equip.Ustype = int.Parse(inventoryInfo[11]);
            equip.Usvalue = int.Parse(inventoryInfo[12]);
            equip.Des = inventoryInfo[13];

            equipList.Add(equip);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
