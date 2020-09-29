using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnermyTrigger : MonoBehaviour {
    private IEnumerator coroutin;

    public GameObject[] posArray; //敌人生成的位置
    public GameObject[] monsterPrefebArr;
    private bool isSpawned = false; //是否已经生成过敌人了
    
    private string playerTag = "Fighter";
    private string firstLevelTrigger = "FirstLevelTrigger";
    private string secondLevelTrigger = "SecondLevelTrigger";
    private string thirdLevelTrigger = "ThirdLevelTrigger";
    private string fourthLevelTrigger = "FourthLevelTrigger";
    private string finalLevelTrigger = "FinalLevelTrigger";
    private int originLayerMask = 4;


    private EnermyController enermyController;
    // Use this for initialization
    void Start () {
        enermyController = TranscriptManager.Instance.GetComponent<EnermyController>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider col)
    {
        int level = 0;
        if(this.transform.gameObject.name == secondLevelTrigger)
        {
            level = 2;
        }
        else if(this.transform.gameObject.name == thirdLevelTrigger)
        {
            level = 3;
        }
        else if (this.transform.gameObject.name == fourthLevelTrigger)
        {
            level = 4;
        }
        else if (this.transform.gameObject.name == finalLevelTrigger)
        {
            level = 5;
        }
        
        coroutin = SpawnEnermy(level);
       
        //判断是否是组队,如果是,只有队长客户端才能触发怪物生成
        //怪物生成需要满足3个条件:1.队长客户端,2.col.tag=player,3.isSpawn=false;
        if (TeamInviteController.Instance.isTeam)
        {
            if (TeamInviteController.Instance.globalMasterID == PhotonEngine.Instance.role.ID
                && col.tag == playerTag 
                && isSpawned == false)
            {
                StartCoroutine(coroutin);
            }
        }
        else //不是组队
        {
            if (col.tag == playerTag && isSpawned == false)
            {
                StartCoroutine(coroutin);
            }
        }
        
    }
    //level表示生成哪个关卡的怪物,不同关卡的怪物活动的范围不一样
    private IEnumerator SpawnEnermy(int level)
    {
        Debug.Log("level = " + level);
        yield return new WaitForSeconds(0.1f);
        //使用预制体生成怪物.
        List<EnermyProperty> enermyList = new List<EnermyProperty>();
        int index = -1;
        foreach(GameObject go in monsterPrefebArr)
        {
            index++;
            string GUID = Guid.NewGuid().ToString();

            EnermyProperty enermyProperty = new EnermyProperty()
            {
                guid = GUID,
                prefabName = go.name,
                position = new Vector3Obj(posArray[index].transform.position)
            };
            enermyList.Add(enermyProperty);
            Vector3 localPosition = posArray[index].transform.position;
            Vector3 worldPosition = transform.TransformDirection(localPosition);
            GameObject enermyGo = GameObject.Instantiate(go, posArray[index].transform.position, Quaternion.identity) as GameObject;
            if(enermyGo.GetComponent<Enermy>())
            {
                enermyGo.GetComponent<Enermy>().GUID = GUID;
                enermyGo.GetComponent<Enermy>().level = level;
            }
            else if(enermyGo.GetComponent<Boss>())
            {
                enermyGo.GetComponent<Boss>().GUID = GUID;
            }
            //master客户端在此处将敌人加入Dict
            EnermyController.Instance.EnermyGoDict.Add(GUID, enermyGo);
        }
        //如果是组队,还要同步到其它客户端
        if(TeamInviteController.Instance.isTeam)
        {
            EnermyModel model = new EnermyModel()
            {
                list = enermyList
            };
            enermyController.SyncEnermySpawnRequest(model);
        }
        
        isSpawned = true;
    }
}
