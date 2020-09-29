using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//这是绑定在子弹上的脚本
public class BossBullet : MonoBehaviour {

    private List<GameObject> playerList = new List<GameObject>();
    public float moveSpeed = 3;
    public float repeatRate = 1;//子弹攻击频率
    public int force; //子弹的推力
    public float Damage
    {
        get; set;
    }
	// Use this for initialization
	void Start () {
        InvokeRepeating("Attack", 0, repeatRate);
        Destroy(this.gameObject, 5);//5秒后销毁子弹
	}
	
	// Update is called once per frame
	void Update () {
        //子弹向前移动
        transform.position += transform.forward * Time.deltaTime * moveSpeed;
	}

    //检测子弹是否与Player发生碰撞
    void OnTriggerEnter(Collider col)
    {
        if (col.tag != "Fighter") return;
        if (TeamInviteController.Instance.isTeam)
        {
            if(TeamInviteController.Instance.globalMasterID != PhotonEngine.Instance.role.ID)
            {
                return;
            }
        }
        if (playerList.IndexOf(col.gameObject) < 0)
        {
            playerList.Add(col.gameObject);
        }
    }

    //检测Player是否离开子弹
    void OnTriggerExit(Collider col)
    {
        if (col.tag != "Fighter") return;
        if (TeamInviteController.Instance.isTeam)
        {
            if (TeamInviteController.Instance.globalMasterID != PhotonEngine.Instance.role.ID)
            {
                return;
            }
        }
        if (playerList.IndexOf(col.gameObject) > -1)
        {
            playerList.Remove(col.gameObject);
        }
    }

    void Attack()
    {
        foreach(GameObject go in playerList)
        {
            go.SendMessage("GetHurt", Damage * repeatRate + "," + false);
            go.rigidbody.AddForce(transform.forward * force);
        }
    }
}
