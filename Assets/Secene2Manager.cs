using UnityEngine;
using System.Collections;

public class Secene2Manager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine("SwitchToTest");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator SwitchToTest()
    {
        yield return new WaitForSeconds(3);
        Application.LoadLevelAsync(3);
    }
}
