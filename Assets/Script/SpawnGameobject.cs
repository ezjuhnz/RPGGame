using UnityEngine;
using System.Collections;

public class SpawnGameobject : MonoBehaviour {

    public GameObject prefabGo;
    
    private GameObject enermyGo;
    void Start () {
        enermyGo = GameObject.Instantiate(prefabGo, prefabGo.transform.position, Quaternion.identity) as GameObject;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
