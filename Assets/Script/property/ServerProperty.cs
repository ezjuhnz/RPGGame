using UnityEngine;
using System.Collections;

public class ServerProperty : MonoBehaviour {

    private int id;
    public string ip = "127.0.0.1:9080";
    public string _name = "test";
    public int count = 100;
    public string IP
    {
        set; get;
    }
    public int Id
    {
        get; set;
    }
    public string Name
    {
        set
        {
            _name = value;
            transform.Find("Label").GetComponent<UILabel>().text = value;
        }
        get { return _name; }
    }
    public int Count
    {
        set;get;
    }
	public void OnPress(bool isPress)
    {
        if(isPress == false)
        {
            //给UI-Root的"OnServerClick"方法发送消息,传递参数this.gameObject
            //这是因为当前对象对的根节点是UI-Root,UI-Root绑定了StartMenuController
            //StartMenuController中定义了OnServerClick函数.
            transform.root.SendMessage("OnServerSelect", this.gameObject);
        }
    }
}
