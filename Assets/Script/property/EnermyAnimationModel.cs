using UnityEngine;
using System.Collections;

public class EnermyAnimationModel {

    public bool attack = false;
    public bool takeDamage = false;
    public bool die = false;
    public int hp = 0;
    public int damage = 0; //怪物对玩家造成的伤害
    public int hurtNum = 0; //玩家对怪物造成的伤害
    public int killerId = 0; //攻击怪物的玩家id
    //暂时用不到下面的属性
    public bool skill1 = false;
    public bool skill2 = false;
    public bool skill3 = false;
}
