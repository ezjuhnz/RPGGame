using UnityEngine;
using System.Collections;

public class SceneManagerTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine("SwitchToScene2");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator SwitchToScene2()
    {
        yield return new WaitForSeconds(3);
        Application.LoadLevelAsync(4);
    }
}
