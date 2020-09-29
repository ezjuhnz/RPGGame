using UnityEngine;
using System.Collections;

public class NotificationController : MonoBehaviour {

    private static NotificationController _instance;
    public UILabel labelOne;
    public UILabel labelTwo;
    public UILabel labelThree;
    public static NotificationController Instance
    {
        get { return _instance; }
    }
    void Start () {
        _instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ShowInfomation(int index,string message)
    {
        if(index == 0)
        {
            labelOne.text = message;
        }
        else if(index == 1)
        {
            labelTwo.text = message;
        }
        else
        {
            if(index %2 == 0)
            {
                labelThree.color = Color.red;
            }
            else
            {
                labelThree.color = Color.green;
            }
            labelThree.text = message;
        }
    }
}
