using UnityEngine;
using System.Collections;

public class HpNumberManager : MonoBehaviour {
    private static HpNumberManager _instance;
    public GameObject hudTextPrefab;

    public static HpNumberManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        _instance = this;
    }
	void Start () {
	
	}

    public GameObject GetHudText(GameObject target)
    {
        GameObject go = NGUITools.AddChild(this.gameObject, hudTextPrefab);
        go.GetComponent<UIFollowTarget>().target = target.transform;
        return go;
    }

    
}
