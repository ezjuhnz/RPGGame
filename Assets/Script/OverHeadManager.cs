using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OverHeadManager : MonoBehaviour {

    private static OverHeadManager _instance;
    private Dictionary<int, GameObject> playerOverHeadDict = new Dictionary<int, GameObject>();
    public GameObject overHeadPrefab;
    public static OverHeadManager Instance
    {
        get { return _instance; }
    }
	void Awake()
    {
        _instance = this;
    }
	
	public GameObject GetOverHead(GameObject target)
    {
        GameObject go = NGUITools.AddChild(this.gameObject, overHeadPrefab);
        go.GetComponent<UIFollowTarget>().target = target.transform;
        
        //修改go属性
        int level = target.transform.parent.GetComponent<Player>().Level;
        string name = target.transform.parent.GetComponent<Player>().Name;
        int playerId = target.transform.parent.GetComponent<Player>().roleId;
        if(!playerOverHeadDict.ContainsKey(playerId))
        {
            playerOverHeadDict.Add(playerId, go);
        }
        go.transform.Find("Label").GetComponent<UILabel>().text = "lv." + level+ " " + name;
       
        return go;
    }

    public void RemoveOverHead(int playerId)
    {
        GameObject go = null;
        playerOverHeadDict.TryGetValue(playerId, out go);
        if(go != null)
        {
            Destroy(go);
        }
        playerOverHeadDict.Remove(playerId);
    }

    public void SetOverHeadBgActive(bool flag)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(flag);
        }
    }

}
